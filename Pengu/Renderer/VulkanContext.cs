using SharpVk;
using GLFW;
using SharpVk.Khronos;
using SharpVk.Multivendor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Collections.Specialized;
using static MoreLinq.Extensions.ForEachExtension;

using Image = SharpVk.Image;
using Buffer = SharpVk.Buffer;
using Version = SharpVk.Version;
using Constants = SharpVk.Constants;
using Exception = System.Exception;
using Pengu.VirtualMachine;

namespace Pengu.Renderer
{
    public partial class VulkanContext : IDisposable
    {
        NativeWindow window;
        Extent2D extent;
        Instance instance;
        DebugReportCallback debugReportCallback;
        Surface surface;
        PhysicalDevice physicalDevice;
        Device device;
        Queue graphicsQueue, presentQueue, transferQueue;
        QueueFamilyIndices queueIndices;
        CommandPool graphicsCommandPool, presentCommandPool, transientTransferCommandPool;
        Swapchain swapChain;
        Image[] swapChainImages;
        ImageView[] swapChainImageViews;
        Framebuffer[] swapChainFramebuffers;
        RenderPass renderPass;

        CommandBuffer[] swapChainImageCommandBuffers;
        BitVector32 swapChainImageCommandBuffersDirty;

        Font monospaceFont;
        FontString fontStringFps;

        GameSurface gameSurface;

        Semaphore[] imageAvailableSemaphores, renderingFinishedSemaphores;
        Fence[] inflightFences, imagesInFlight;

        readonly Queue<(Keys key, int scanCode, InputState state, ModifierKeys modifiers)> KeyQueue =
            new Queue<(Keys key, int scanCode, InputState state, ModifierKeys modifiers)>();

        // needed for Glfw's events
        static VulkanContext ContextInstance;

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


        public VulkanContext(VM vm, bool debug)
        {
            ContextInstance = this;

            const int Width = 1280;
            const int Height = 720;

            Glfw.WindowHint(Hint.ClientApi, ClientApi.None);
            Glfw.Init();

            window = new NativeWindow(Width, Height, "Pengu");

            static void KeyActionCallback(object sender, KeyEventArgs args) => ContextInstance.KeyQueue.Enqueue((args.Key, args.ScanCode, args.State, args.Modifiers));
            window.KeyAction += KeyActionCallback;

            const string StandardValidationLayerName = "VK_LAYER_LUNARG_standard_validation";

            // create instance
            string[] enabledLayers = null;
            string[] enabledExtensions = null;
            if (debug)
            {
                var availableLayers = Instance.EnumerateLayerProperties();
                if (availableLayers.Any(w => w.LayerName == StandardValidationLayerName))
                    enabledLayers = new[] { StandardValidationLayerName };

                enabledExtensions = Vulkan.GetRequiredInstanceExtensions().Append(ExtExtensions.DebugReport).ToArray();
            }
            else
            {
                enabledLayers = Array.Empty<string>();
                enabledExtensions = Vulkan.GetRequiredInstanceExtensions();
            }

            instance = Instance.Create(enabledLayers, enabledExtensions,
                applicationInfo: new ApplicationInfo
                {
                    ApplicationName = "Pengu",
                    ApplicationVersion = new Version(1, 0, 0),
                    EngineName = "SharpVk",
                    EngineVersion = new Version(1, 0, 0),
                    ApiVersion = new Version(1, 1, 0)
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
            _ = Vulkan.CreateWindowSurface(new IntPtr((long)instance.RawHandle.ToUInt64()), window.Handle, IntPtr.Zero, out var surfacePtr);
            surface = Surface.CreateFromHandle(instance, (ulong)surfacePtr.ToInt64());

            (physicalDevice, queueIndices) = instance.EnumeratePhysicalDevices()
                .Select(device => (device, q: QueueFamilyIndices.Find(device, surface)))
                .Where(w => w.device.EnumerateDeviceExtensionProperties(null)
                    .Any(extension => extension.ExtensionName == KhrExtensions.Swapchain) && w.q.IsComplete)
                .First();
            var indices = queueIndices.Indices.ToArray();

            // create the logical device
            device = physicalDevice.CreateDevice(
                indices.Select(idx => new DeviceQueueCreateInfo { QueueFamilyIndex = idx, QueuePriorities = new[] { 1f } }).ToArray(),
                null, KhrExtensions.Swapchain);

            // get the queue and create the pool
            graphicsQueue = device.GetQueue(queueIndices.GraphicsFamily.Value, 0);
            graphicsCommandPool = device.CreateCommandPool(queueIndices.GraphicsFamily.Value);
            presentQueue = device.GetQueue(queueIndices.GraphicsFamily.Value, 0);
            presentCommandPool = device.CreateCommandPool(queueIndices.PresentFamily.Value);
            transferQueue = device.GetQueue(queueIndices.TransferFamily.Value, 0);
            transientTransferCommandPool = device.CreateCommandPool(queueIndices.TransferFamily.Value, CommandPoolCreateFlags.Transient);

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

            imageAvailableSemaphores = Enumerable.Range(0, (int)swapChainImageCount).Select(_ => device.CreateSemaphore()).ToArray();
            renderingFinishedSemaphores = Enumerable.Range(0, (int)swapChainImageCount).Select(_ => device.CreateSemaphore()).ToArray();
            inflightFences = Enumerable.Range(0, (int)swapChainImageCount).Select(_ => device.CreateFence(FenceCreateFlags.Signaled)).ToArray();
            imagesInFlight = new Fence[swapChainImageCount];

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

            swapChainImageCommandBuffers = device.AllocateCommandBuffers(graphicsCommandPool, CommandBufferLevel.Primary, (uint)swapChainFramebuffers.Length);

            monospaceFont = new Font(this, "pt_mono");
            fontStringFps = monospaceFont.AllocateString(new Vector2(-1f * extent.AspectRatio, -.995f), .033f);

            gameSurface = new GameSurface(this, vm);
        }

        ShaderModule CreateShaderModule(string filePath)
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

        Buffer CreateBuffer(ulong size, BufferUsageFlags usageFlags, MemoryPropertyFlags memoryPropertyFlags, out DeviceMemory deviceMemory)
        {
            var buffer = device.CreateBuffer(size, usageFlags, SharingMode.Exclusive, null);
            var memRequirements = buffer.GetMemoryRequirements();
            deviceMemory = device.AllocateMemory(memRequirements.Size, FindMemoryType(memRequirements.MemoryTypeBits, memoryPropertyFlags));
            buffer.BindMemory(deviceMemory, 0);

            return buffer;
        }

        void CopyBuffer(Buffer sourceBuffer, Buffer destinationBuffer, ulong size) =>
            RunTransientCommands(commandBuffer => commandBuffer.CopyBuffer(sourceBuffer, destinationBuffer, new BufferCopy { Size = size }));

        void RunTransientCommands(Action<CommandBuffer> action)
        {
            var commandbuffers = device.AllocateCommandBuffers(transientTransferCommandPool, CommandBufferLevel.Primary, 1);
            commandbuffers[0].Begin(CommandBufferUsageFlags.OneTimeSubmit);

            action(commandbuffers[0]);

            commandbuffers[0].End();

            transferQueue.Submit(new SubmitInfo { CommandBuffers = commandbuffers }, null);
            transferQueue.WaitIdle();

            transientTransferCommandPool.FreeCommandBuffers(commandbuffers);
        }

        void TransitionImageLayout(Image image, ImageLayout oldLayout, ImageLayout newLayout) =>
            RunTransientCommands(commandBuffer =>
            {
                var barrier = new ImageMemoryBarrier
                {
                    OldLayout = oldLayout,
                    NewLayout = newLayout,
                    DestinationQueueFamilyIndex = uint.MaxValue,
                    SourceQueueFamilyIndex = uint.MaxValue,
                    Image = image,
                    SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Color,
                        LayerCount = 1,
                        LevelCount = 1,
                    },
                };
                PipelineStageFlags sourceStage, destinationStage;

                if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDestinationOptimal)
                {
                    // transfer writes that don't need to wait on anything
                    barrier.SourceAccessMask = 0;
                    barrier.DestinationAccessMask = AccessFlags.TransferWrite;

                    sourceStage = PipelineStageFlags.TopOfPipe;
                    destinationStage = PipelineStageFlags.Transfer;
                }
                else if (oldLayout == ImageLayout.TransferDestinationOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
                {
                    // shader reads should wait on transfer writes, specifically the shader reads in the fragment shader
                    // because that's where we're going to use the texture
                    barrier.SourceAccessMask = AccessFlags.TransferWrite;
                    barrier.DestinationAccessMask = AccessFlags.ShaderRead;

                    sourceStage = PipelineStageFlags.Transfer;
                    destinationStage = PipelineStageFlags.FragmentShader;
                }
                else
                    throw new InvalidOperationException($"Undefined image layout transition from {oldLayout} to {newLayout}");

                commandBuffer.PipelineBarrier(sourceStage, destinationStage, null, null, barrier);
            });

        void CopyBufferToImage2D(Buffer buffer, Image image, uint width, uint height) =>
            RunTransientCommands(
                commandBuffer => commandBuffer.CopyBufferToImage(buffer, image, ImageLayout.TransferDestinationOptimal,
                    new BufferImageCopy
                    {
                        ImageSubresource = new ImageSubresourceLayers
                        {
                            AspectMask = ImageAspectFlags.Color,
                            LayerCount = 1,
                        },
                        ImageOffset = Offset3D.Zero,
                        ImageExtent = new Extent3D(width, height, 1),
                    }));

        Image CreateTextureImage(string fn, uint queueFamilyIndex, out Format format, out DeviceMemory imageMemory)
        {
            Buffer stagingBuffer = default;
            DeviceMemory stagingBufferMemory = default;

            // upload to a staging buffer in host memory
            try
            {
                using var imagedata = SixLabors.ImageSharp.Image.Load<Bgra32>(Path.Combine("Media", fn));

                var (width, height) = (imagedata.Width, imagedata.Height);
                var size = width * height * 4;

                if (!imagedata.TryGetSinglePixelSpan(out var pixelSpan))
                    throw new InvalidOperationException($"Could not get pixel span for {fn}");

                stagingBuffer = CreateBuffer((ulong)size, BufferUsageFlags.TransferSource, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingBufferMemory);
                var mappedData = stagingBufferMemory.Map(0, (ulong)size);
                unsafe { pixelSpan.CopyTo(new Span<Bgra32>(mappedData.ToPointer(), size / 4)); }
                stagingBufferMemory.Unmap();

                // create the image
                format = Format.B8G8R8A8Srgb;
                var image = device.CreateImage(ImageType.Image2d, format, new Extent3D((uint)width, (uint)height, 1), 1, 1,
                    SampleCountFlags.SampleCount1, ImageTiling.Optimal, ImageUsageFlags.Sampled | ImageUsageFlags.TransferDestination,
                    SharingMode.Exclusive, queueFamilyIndex, ImageLayout.Undefined);

                // allocate memory for the image 
                var memoryRequirements = image.GetMemoryRequirements();
                imageMemory = device.AllocateMemory(memoryRequirements.Size, FindMemoryType(memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocal));
                image.BindMemory(imageMemory, 0);

                // transition into a transfer destination, copy the buffer data, and then transition to shader readonly
                TransitionImageLayout(image, ImageLayout.Undefined, ImageLayout.TransferDestinationOptimal);
                CopyBufferToImage2D(stagingBuffer, image, (uint)width, (uint)height);
                TransitionImageLayout(image, ImageLayout.TransferDestinationOptimal, ImageLayout.ShaderReadOnlyOptimal);

                return image;
            }
            finally
            {
                stagingBuffer?.Dispose();
                stagingBufferMemory?.Free();
            }
        }

        private void UpdateLogic(TimeSpan elapsedTime)
        {
            while (KeyQueue.Count > 0)
            {
                var (key, scanCode, action, modifiers) = KeyQueue.Dequeue();
                gameSurface.ProcessKey(key, scanCode, action, modifiers);
            }

            gameSurface.UpdateLogic(elapsedTime);
            monospaceFont.UpdateLogic(elapsedTime);
        }

        int currentFrame = 0;
        private void DrawFrame()
        {
            device.WaitForFences(inflightFences[currentFrame], true, ulong.MaxValue);

            uint nextImage = swapChain.AcquireNextImage(uint.MaxValue, imageAvailableSemaphores[currentFrame], null);

            if (!(imagesInFlight[nextImage] is null))
                device.WaitForFences(imagesInFlight[nextImage], true, ulong.MaxValue);

            imagesInFlight[nextImage] = inflightFences[currentFrame];

            device.ResetFences(inflightFences[currentFrame]);

            if (monospaceFont.IsCommandBufferDirty)
            {
                Enumerable.Range(0, swapChainImageCommandBuffers.Length).ForEach(idx => swapChainImageCommandBuffersDirty[idx] = true);
                monospaceFont.IsCommandBufferDirty = false;
            }

            gameSurface.PreRender(nextImage);
            monospaceFont.PreRender(nextImage);

            if (swapChainImageCommandBuffersDirty[(int)nextImage])
            {
                var commandBuffer = swapChainImageCommandBuffers[nextImage];

                commandBuffer.Begin(CommandBufferUsageFlags.SimultaneousUse);
                commandBuffer.BeginRenderPass(renderPass, swapChainFramebuffers[nextImage], new Rect2D(extent), new ClearValue(), SubpassContents.Inline);
                monospaceFont.Draw(commandBuffer, (int)nextImage);
                commandBuffer.EndRenderPass();
                commandBuffer.End();

                swapChainImageCommandBuffersDirty[(int)nextImage] = false;
            }

            graphicsQueue.Submit(
                new SubmitInfo
                {
                    CommandBuffers = new[] { swapChainImageCommandBuffers[nextImage] },
                    SignalSemaphores = new[] { renderingFinishedSemaphores[currentFrame] },
                    WaitDestinationStageMask = new[] { PipelineStageFlags.ColorAttachmentOutput },
                    WaitSemaphores = new[] { imageAvailableSemaphores[currentFrame] }
                },
                inflightFences[currentFrame]);

            presentQueue.Present(renderingFinishedSemaphores[currentFrame], swapChain, nextImage);

            currentFrame = (currentFrame + 1) % swapChainImages.Length;
        }

        static readonly TimeSpan fpsMeasurementInterval = TimeSpan.FromSeconds(2);
        DateTime nextFpsMeasurement = DateTime.Now + fpsMeasurementInterval;
        int framesRendered;

        internal void Run()
        {
            DateTime lastUpdateAt = DateTime.Now;

            while (!window.IsClosing)
            {
                var now = DateTime.Now;

                ++framesRendered;
                if (now >= nextFpsMeasurement)
                {
                    fontStringFps.Value = $"FPS: {framesRendered / (now - nextFpsMeasurement + fpsMeasurementInterval).TotalSeconds:0.00} Font Verts: {monospaceFont.UsedVertices} used out of {monospaceFont.MaxVertices}";
                    framesRendered = 0;
                    nextFpsMeasurement = now + fpsMeasurementInterval;
                }

                UpdateLogic(now - lastUpdateAt);
                DrawFrame();

                Glfw.PollEvents();

                lastUpdateAt = now;
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
                renderPass.Dispose();
                swapChainImageViews.ForEach(i => i.Dispose());
                swapChain.Dispose();
                imageAvailableSemaphores.ForEach(s => s.Dispose());
                renderingFinishedSemaphores.ForEach(s => s.Dispose());
                inflightFences.ForEach(s => s.Dispose());
                transientTransferCommandPool.Dispose();
                graphicsCommandPool.Dispose();
                presentCommandPool.Dispose();
                device.Dispose();
                surface.Dispose();
                debugReportCallback.Dispose();
                instance.Dispose();
                window.Dispose();

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

    public interface IRenderableModule
    {
        public bool ProcessKey(Keys key, int scanCode, InputState state, ModifierKeys modifiers);
        public void UpdateLogic(TimeSpan elapsedTime);
        public void PreRender(uint nextImage);
    }
}
