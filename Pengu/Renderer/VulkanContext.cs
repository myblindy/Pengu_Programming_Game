using SharpVk;
using SharpVk.Glfw;
using SharpVk.Khronos;
using SharpVk.Multivendor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static MoreLinq.Extensions.ForEachExtension;

namespace Pengu.Renderer
{
    public partial class VulkanContext : IDisposable
    {
        WindowHandle window;
        Extent2D extent;
        Instance instance;
        DebugReportCallback debugReportCallback;
        Surface surface;
        PhysicalDevice physicalDevice;
        PhysicalDeviceMemoryProperties physicalDevideMemoryProperties;
        PhysicalDeviceFeatures physicalDeviceFeatures;
        PhysicalDeviceProperties physicalDeviceProperties;
        Device device;
        Queue graphicsQueue, presentQueue, transferQueue;
        CommandPool graphicsCommandPool, presentCommandPool, transientTransferCommandPool;
        Swapchain swapChain;
        Image[] swapChainImages;
        ImageView[] swapChainImageViews;
        Framebuffer[] swapChainFramebuffers;
        RenderPass renderPass;
        PipelineLayout pipelineLayout;
        CommandBuffer[] swapChainImageCommandBuffers;

        Font monospaceFont;

        Semaphore imageAvailableSemaphore, renderingFinishedSemaphore;

        private struct QueueFamilyIndices
        {
            public uint? GraphicsFamily;
            public uint? TransferFamily;
            public uint? PresentFamily;

            public IEnumerable<uint> Indices
            {
                get
                {
                    if (GraphicsFamily.HasValue)
                        yield return GraphicsFamily.Value;

                    if (PresentFamily.HasValue && PresentFamily != GraphicsFamily)
                        yield return PresentFamily.Value;

                    if (TransferFamily.HasValue && TransferFamily != GraphicsFamily && TransferFamily != PresentFamily)
                        yield return TransferFamily.Value;
                }
            }

            public bool IsComplete => GraphicsFamily.HasValue && PresentFamily.HasValue && TransferFamily.HasValue;

            public static QueueFamilyIndices Find(PhysicalDevice device, Surface surface)
            {
                QueueFamilyIndices indices = default;

                var queueFamilies = device.GetQueueFamilyProperties();

                for (uint index = 0; index < queueFamilies.Length && !indices.IsComplete; index++)
                {
                    if (queueFamilies[index].QueueFlags.HasFlag(QueueFlags.Graphics))
                        indices.GraphicsFamily = index;

                    if (queueFamilies[index].QueueFlags.HasFlag(QueueFlags.Transfer))
                        indices.TransferFamily = index;

                    if (device.GetSurfaceSupport(index, surface))
                        indices.PresentFamily = index;
                }

                indices.TransferFamily ??= indices.GraphicsFamily;

                return indices;
            }
        }

        public VulkanContext(bool debug)
        {
            Glfw3.Init();

            const int Width = 1280;
            const int Height = 720;

            Glfw3.WindowHint((WindowAttribute)0x00022001, 0);
            window = Glfw3.CreateWindow(Width, Height, "Pengu", MonitorHandle.Zero, WindowHandle.Zero);

            const string StandardValidationLayerName = "VK_LAYER_LUNARG_standard_validation";

            // create instance
            string[] enabledLayers = null;
            string[] enabledExtensions = null;
            if (debug)
            {
                var availableLayers = Instance.EnumerateLayerProperties();
                if (availableLayers.Any(w => w.LayerName == StandardValidationLayerName))
                    enabledLayers = new[] { StandardValidationLayerName };

                enabledExtensions = Glfw3.GetRequiredInstanceExtensions().Append(ExtExtensions.DebugReport).ToArray();
            }
            else
            {
                enabledLayers = Array.Empty<string>();
                enabledExtensions = Glfw3.GetRequiredInstanceExtensions();
            }

            instance = Instance.Create(enabledLayers, enabledExtensions,
                applicationInfo: new ApplicationInfo
                {
                    ApplicationName = "Pengu",
                    ApplicationVersion = new SharpVk.Version(1, 0, 0),
                    EngineName = "SharpVk",
                    EngineVersion = new SharpVk.Version(1, 0, 0),
                    ApiVersion = new SharpVk.Version(1, 1, 0)
                });

            // debug layer
            if (debug)
                debugReportCallback = instance.CreateDebugReportCallback(
                    (flags, objectType, @object, location, messageCode, layerPrefix, message, userData) =>
                    {
                        Debug.WriteLine($"[{flags}][{layerPrefix}] {message}");
                        return flags.HasFlag(DebugReportFlags.Error);
                    }, DebugReportFlags.Error | DebugReportFlags.Warning | DebugReportFlags.PerformanceWarning);

            // create the surface surface
            surface = instance.CreateGlfw3Surface(window);

            var (pd, qs) = instance.EnumeratePhysicalDevices()
                .Select(device => (device, q: QueueFamilyIndices.Find(device, surface)))
                .Where(w => w.device.EnumerateDeviceExtensionProperties(null)
                    .Any(extension => extension.ExtensionName == KhrExtensions.Swapchain) && w.q.IsComplete)
                .First();
            physicalDevice = pd;
            var indices = qs.Indices.ToArray();

            physicalDevideMemoryProperties = physicalDevice.GetMemoryProperties();
            physicalDeviceFeatures = physicalDevice.GetFeatures();
            physicalDeviceProperties = physicalDevice.GetProperties();

            // create the logical device
            device = physicalDevice.CreateDevice(
                indices.Select(idx => new DeviceQueueCreateInfo { QueueFamilyIndex = idx, QueuePriorities = new[] { 1f } }).ToArray(),
                null, KhrExtensions.Swapchain);

            // get the queue and create the pool
            graphicsQueue = device.GetQueue(qs.GraphicsFamily.Value, 0);
            graphicsCommandPool = device.CreateCommandPool(qs.GraphicsFamily.Value);
            presentQueue = device.GetQueue(qs.GraphicsFamily.Value, 0);
            presentCommandPool = device.CreateCommandPool(qs.PresentFamily.Value);
            transferQueue = device.GetQueue(qs.TransferFamily.Value, 0);
            transientTransferCommandPool = device.CreateCommandPool(qs.TransferFamily.Value, CommandPoolCreateFlags.Transient);

            imageAvailableSemaphore = device.CreateSemaphore();
            renderingFinishedSemaphore = device.CreateSemaphore();

            var surfaceCapabilities = physicalDevice.GetSurfaceCapabilities(surface);
            var surfaceFormats = physicalDevice.GetSurfaceFormats(surface);
            var surfacePresentModes = physicalDevice.GetSurfacePresentModes(surface);

            // try to get an R8G8B8_A8_SRGB surface format, otherwise pick the first one in the list of available formats
            var surfaceFormat = surfaceFormats.FirstOrDefault(f => f.ColorSpace == ColorSpace.SrgbNonlinear && f.Format == Format.B8G8R8A8Srgb);
            if (surfaceFormat.Format == Format.Undefined) surfaceFormat = surfaceFormats[0];

            // try to get a mailbox present mode if available, otherwise revert to FIFO which is always available
            var surfacePresentMode = surfacePresentModes.Contains(PresentMode.Mailbox) ? PresentMode.Mailbox : PresentMode.Fifo;

            // construct the swap chain extent based on window size
            if (surfaceCapabilities.CurrentExtent.Width != int.MaxValue)
                extent = surfaceCapabilities.CurrentExtent;
            else
                extent = new Extent2D(
                    Math.Max(surfaceCapabilities.MinImageExtent.Width, Math.Min(surfaceCapabilities.MaxImageExtent.Width, Width)),
                    Math.Max(surfaceCapabilities.MinImageExtent.Height, Math.Min(surfaceCapabilities.MaxImageExtent.Height, Height)));

            // swap chain count, has to be between min+1 and max
            var swapChainImageCount = surfaceCapabilities.MinImageCount + 1;
            if (surfaceCapabilities.MaxImageCount > 0 && surfaceCapabilities.MaxImageCount < swapChainImageCount)
                swapChainImageCount = surfaceCapabilities.MaxImageCount;

            // build the swap chain
            swapChain = device.CreateSwapchain(
                surface, swapChainImageCount, surfaceFormat.Format, surfaceFormat.ColorSpace, extent, 1, ImageUsageFlags.ColorAttachment,
                indices.Length == 1 ? SharingMode.Exclusive : SharingMode.Concurrent, indices,
                surfaceCapabilities.CurrentTransform, CompositeAlphaFlags.Opaque, surfacePresentMode, true, swapChain);

            // get the swap chain images, and build image views for them
            swapChainImages = swapChain.GetImages();
            swapChainImageViews = swapChainImages
                .Select(i => device.CreateImageView(i, ImageViewType.ImageView2d, surfaceFormat.Format, ComponentMapping.Identity,
                    new ImageSubresourceRange(ImageAspectFlags.Color, 0, 1, 0, 1))).ToArray();

            // the render pass
            renderPass = device.CreateRenderPass(
                new AttachmentDescription
                {
                    Format = surfaceFormat.Format,
                    Samples = SampleCountFlags.SampleCount1,
                    LoadOp = AttachmentLoadOp.Clear,
                    StoreOp = AttachmentStoreOp.Store,
                    StencilLoadOp = AttachmentLoadOp.DontCare,
                    StencilStoreOp = AttachmentStoreOp.DontCare,
                    InitialLayout = ImageLayout.Undefined,
                    FinalLayout = ImageLayout.PresentSource,
                },
                new SubpassDescription
                {
                    PipelineBindPoint = PipelineBindPoint.Graphics,
                    ColorAttachments = new[] { new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal } },
                },
                new[]
                {
                    new SubpassDependency
                    {
                        SourceSubpass = Constants.SubpassExternal,
                        DestinationSubpass = 0,
                        SourceStageMask = PipelineStageFlags.BottomOfPipe,
                        SourceAccessMask = AccessFlags.MemoryRead,
                        DestinationStageMask = PipelineStageFlags.ColorAttachmentOutput,
                        DestinationAccessMask = AccessFlags.ColorAttachmentRead | AccessFlags.ColorAttachmentWrite
                    },
                    new SubpassDependency
                    {
                        SourceSubpass = 0,
                        DestinationSubpass = Constants.SubpassExternal,
                        SourceStageMask = PipelineStageFlags.ColorAttachmentOutput,
                        SourceAccessMask = AccessFlags.ColorAttachmentRead | AccessFlags.ColorAttachmentWrite,
                        DestinationStageMask = PipelineStageFlags.BottomOfPipe,
                        DestinationAccessMask = AccessFlags.MemoryRead
                    }
                });

            // and the frame buffers for the render pass
            swapChainFramebuffers = swapChainImageViews
                .Select(iv => device.CreateFramebuffer(renderPass, iv, extent.Width, extent.Height, 1))
                .ToArray();

            pipelineLayout = device.CreatePipelineLayout(null, null);

            swapChainImageCommandBuffers = device.AllocateCommandBuffers(graphicsCommandPool, CommandBufferLevel.Primary, (uint)swapChainFramebuffers.Length);

            monospaceFont = new Font(this);

            for (int idx = 0; idx < swapChainFramebuffers.Length; ++idx)
            {
                var commandBuffer = swapChainImageCommandBuffers[idx];

                commandBuffer.Begin(CommandBufferUsageFlags.SimultaneousUse);
                commandBuffer.BeginRenderPass(renderPass, swapChainFramebuffers[idx], new Rect2D(extent), new ClearValue(), SubpassContents.Inline);
                monospaceFont.Draw(commandBuffer);
                commandBuffer.EndRenderPass();
                commandBuffer.End();
            }
        }

        private ShaderModule CreateShaderModule(string filePath)
        {
            var fileBytes = File.ReadAllBytes(Path.Combine("Shaders", filePath));
            var shaderData = new uint[(int)Math.Ceiling(fileBytes.Length / 4f)];

            System.Buffer.BlockCopy(fileBytes, 0, shaderData, 0, fileBytes.Length);

            return device.CreateShaderModule(fileBytes.Length, shaderData);
        }

        uint FindMemoryType(uint typeFilter, MemoryPropertyFlags memoryPropertyFlags)
        {
            var memoryProperties = physicalDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypes.Length; i++)
                if ((typeFilter & (1u << i)) > 0 && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(memoryPropertyFlags))
                    return (uint)i;

            throw new Exception("No compatible memory type.");
        }

        void CreateBuffer(ulong size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags,
            out SharpVk.Buffer buffer, out DeviceMemory deviceMemory)
        {
            buffer = device.CreateBuffer(size, usageFlags, SharingMode.Exclusive, null);
            var memRequirements = buffer.GetMemoryRequirements();
            deviceMemory = device.AllocateMemory(memRequirements.Size, FindMemoryType(memRequirements.MemoryTypeBits, memoryPropertyFlags));
            buffer.BindMemory(deviceMemory, 0);
        }

        void CopyBuffer(SharpVk.Buffer sourceBuffer, SharpVk.Buffer destinationBuffer, ulong size)
        {
            var transferBuffers = device.AllocateCommandBuffers(transientTransferCommandPool, CommandBufferLevel.Primary, 1);

            transferBuffers[0].Begin(CommandBufferUsageFlags.OneTimeSubmit);
            transferBuffers[0].CopyBuffer(sourceBuffer, destinationBuffer, new BufferCopy { Size = size });
            transferBuffers[0].End();

            transferQueue.Submit(new SubmitInfo { CommandBuffers = transferBuffers }, null);
            transferQueue.WaitIdle();

            transientTransferCommandPool.FreeCommandBuffers(transferBuffers);
        }

        private void DrawFrame()
        {
            uint nextImage = swapChain.AcquireNextImage(uint.MaxValue, imageAvailableSemaphore, null);

            graphicsQueue.Submit(
                new SubmitInfo
                {
                    CommandBuffers = new[] { swapChainImageCommandBuffers[nextImage] },
                    SignalSemaphores = new[] { renderingFinishedSemaphore },
                    WaitDestinationStageMask = new[] { PipelineStageFlags.ColorAttachmentOutput },
                    WaitSemaphores = new[] { imageAvailableSemaphore }
                },
                null);

            presentQueue.Present(renderingFinishedSemaphore, swapChain, nextImage);
        }

        static readonly TimeSpan fpsMeasurementInterval = TimeSpan.FromSeconds(2);
        DateTime nextFpsMeasurement = DateTime.Now + fpsMeasurementInterval;
        int framesRendered;

        internal void Run()
        {
            while (!Glfw3.WindowShouldClose(window))
            {
                ++framesRendered;
                var now = DateTime.Now;
                if (now >= nextFpsMeasurement)
                {
                    Debug.WriteLine($"FPS: {framesRendered / (now - nextFpsMeasurement + fpsMeasurementInterval).TotalSeconds}");
                    framesRendered = 0;
                    nextFpsMeasurement = now + fpsMeasurementInterval;
                }

                DrawFrame();

                Glfw3.PollEvents();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                monospaceFont.Dispose();
                pipelineLayout.Dispose();
                renderPass.Dispose();
                swapChainImageViews.ForEach(i => i.Dispose());
                swapChain.Dispose();
                imageAvailableSemaphore.Dispose();
                renderingFinishedSemaphore.Dispose();
                transientTransferCommandPool.Dispose();
                graphicsCommandPool.Dispose();
                presentCommandPool.Dispose();
                device.Dispose();
                surface.Dispose();
                debugReportCallback.Dispose();
                instance.Dispose();

                disposedValue = true;
            }
        }

        ~VulkanContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
