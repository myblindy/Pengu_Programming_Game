using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VulkanCore;
using VulkanCore.Ext;
using VulkanCore.Khr;

namespace Pengu.Renderer
{
    public class VulkanContext : IDisposable
    {
        Instance instance;
        DebugReportCallbackExt debugReportCallback;
        Form form;
        SurfaceKhr surface;
        PhysicalDevice physicalDevice;
        PhysicalDeviceMemoryProperties physicalDevideMemoryProperties;
        PhysicalDeviceFeatures physicalDeviceFeatures;
        PhysicalDeviceProperties physicalDeviceProperties;
        Device device;
        Queue graphicsPresentQueue;
        CommandPool graphicsCommandQueue;
        SwapchainKhr swapChain;
        Image[] swapChainImages;
        ImageView[] swapChainImageViews;
        Framebuffer[] swapChainFramebuffers;
        RenderPass renderPass;
        PipelineLayout pipelineLayout;


        Semaphore imageAvailableSemaphore, renderingFinishedSemaphore;

        public VulkanContext(bool debug)
        {
            // create instance
            var instanceCreateInfo = new InstanceCreateInfo();
            if (debug)
            {
                var availableLayers = Instance.EnumerateLayerProperties();
                if (availableLayers.Contains(Constant.InstanceLayer.LunarGStandardValidation))
                    instanceCreateInfo.EnabledLayerNames = new[] { Constant.InstanceLayer.LunarGStandardValidation };
                instanceCreateInfo.EnabledExtensionNames = new[]
                {
                    Constant.InstanceExtension.KhrSurface,
                    Constant.InstanceExtension.KhrWin32Surface,                 // TODO cross-platform
                    Constant.InstanceExtension.ExtDebugReport
                };
            }
            else
            {
                instanceCreateInfo.EnabledExtensionNames = new[]
                {
                    Constant.InstanceExtension.KhrSurface,
                    Constant.InstanceExtension.KhrWin32Surface,                 // TODO cross-platform
                };
            }

            instance = new Instance(instanceCreateInfo);

            // debug layer
            if (debug)
            {
                debugReportCallback = instance.CreateDebugReportCallbackExt(
                    new DebugReportCallbackCreateInfoExt(DebugReportFlagsExt.All,
                    args =>
                    {
                        Debug.WriteLine($"[{args.Flags}][{args.LayerPrefix}] {args.Message}");
                        return args.Flags.HasFlag(DebugReportFlagsExt.Error);
                    }));
            }

            // create form and surface
            const int Width = 1280;
            const int Height = 720;
            form = new Form { Width = Width, Height = Height, FormBorderStyle = FormBorderStyle.FixedDialog };
            form.Show();
            surface = instance.CreateWin32SurfaceKhr(new Win32SurfaceCreateInfoKhr(Process.GetCurrentProcess().Handle, form.Handle));

            int queueFamilyIndex = 0;
            foreach (var physicalDevice in instance.EnumeratePhysicalDevices())
            {
                var queueFamilyProperties = physicalDevice.GetQueueFamilyProperties();
                for (int i = 0; i < queueFamilyProperties.Length; ++i)
                    if (queueFamilyProperties[i].QueueFlags.HasFlag(Queues.Graphics) &&
                        physicalDevice.GetSurfaceSupportKhr(i, surface) && physicalDevice.GetWin32PresentationSupportKhr(i))
                    {
                        queueFamilyIndex = i;
                        this.physicalDevice = physicalDevice;
                        break;
                    }

                if (this.physicalDevice != null) break;
            }

            physicalDevideMemoryProperties = physicalDevice.GetMemoryProperties();
            physicalDeviceFeatures = physicalDevice.GetFeatures();
            physicalDeviceProperties = physicalDevice.GetProperties();

            // create the logical device
            var deviceQueueCreateInfo = new DeviceQueueCreateInfo(queueFamilyIndex, 1, 1.0f);
            var deviceCreateInfo = new DeviceCreateInfo(
                new[] { deviceQueueCreateInfo },
                new[] { Constant.DeviceExtension.KhrSwapchain },
                physicalDeviceFeatures);
            device = physicalDevice.CreateDevice(deviceCreateInfo);

            // get the queue and create the pool
            graphicsPresentQueue = device.GetQueue(queueFamilyIndex);
            graphicsCommandQueue = device.CreateCommandPool(new CommandPoolCreateInfo(queueFamilyIndex));

            imageAvailableSemaphore = device.CreateSemaphore();
            renderingFinishedSemaphore = device.CreateSemaphore();

            var surfaceCapabilities = physicalDevice.GetSurfaceCapabilitiesKhr(surface);
            var surfaceFormats = physicalDevice.GetSurfaceFormatsKhr(surface);
            var surfacePresentModes = physicalDevice.GetSurfacePresentModesKhr(surface);

            // try to get an R8G8B8_A8_SRGB surface format, otherwise pick the first one in the list of available formats
            var surfaceFormat = surfaceFormats.FirstOrDefault(f => f.ColorSpace == ColorSpaceKhr.SRgbNonlinear && f.Format == Format.B8G8R8A8SRgb);
            if (surfaceFormat.Format == Format.Undefined) surfaceFormat = surfaceFormats[0];

            // try to get a mailbox present mode if available, otherwise revert to FIFO which is always available
            var surfacePresentMode = surfacePresentModes.Contains(PresentModeKhr.Mailbox) ? PresentModeKhr.Mailbox : PresentModeKhr.Fifo;

            // construct the swap chain extent based on window size
            Extent2D extent;
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
            swapChain = device.CreateSwapchainKhr(new SwapchainCreateInfoKhr(
                surface, surfaceFormat.Format, extent, swapChainImageCount, surfaceFormat.ColorSpace,
                imageUsage: ImageUsages.ColorAttachment, imageSharingMode: SharingMode.Exclusive,
                preTransform: surfaceCapabilities.CurrentTransform, presentMode: surfacePresentMode));

            // get the swap chain images, and build image views for them
            swapChainImages = swapChain.GetImages();
            swapChainImageViews = swapChainImages.Select(i => i.CreateView(new ImageViewCreateInfo(surfaceFormat.Format,
                new ImageSubresourceRange(ImageAspects.Color, 0, 1, 0, 1)))).ToArray();

            // the render pass
            renderPass = device.CreateRenderPass(new RenderPassCreateInfo(
                new[] { new SubpassDescription(new[] { new AttachmentReference(0, VulkanCore.ImageLayout.ColorAttachmentOptimal) }) },
                new[]
                {
                    new AttachmentDescription(0, surfaceFormat.Format, SampleCounts.Count1, AttachmentLoadOp.Clear, AttachmentStoreOp.Store,
                        AttachmentLoadOp.DontCare, AttachmentStoreOp.DontCare, VulkanCore.ImageLayout.Undefined, VulkanCore.ImageLayout.PresentSrcKhr)
                }));

            // and the frame buffers for the render pass
            swapChainFramebuffers = swapChainImageViews
                .Select(iv => renderPass.CreateFramebuffer(new FramebufferCreateInfo(new[] { iv }, extent.Width, extent.Height)))
                .ToArray();

            using var vShader = BuildShaderModule("tris.vert.spiv");
            using var fShader = BuildShaderModule("tris.frag.spiv");

            pipelineLayout = device.CreatePipelineLayout();

            var graphicsPipeline = device.CreateGraphicsPipeline(new GraphicsPipelineCreateInfo(
                pipelineLayout, renderPass, 0,
                new[]
                {
                    new PipelineShaderStageCreateInfo(ShaderStages.Vertex, vShader, "main"),
                    new PipelineShaderStageCreateInfo(ShaderStages.Fragment, fShader, "main"),
                },
                new PipelineInputAssemblyStateCreateInfo(PrimitiveTopology.TriangleList),
                new PipelineVertexInputStateCreateInfo(),
                new PipelineRasterizationStateCreateInfo(),
                viewportState: new PipelineViewportStateCreateInfo(
                    new Viewport(0, 0, extent.Width, extent.Height),
                    new Rect2D(0, 0, extent.Width, extent.Height))));
        }

        internal ShaderModule BuildShaderModule(string fn) =>
            device.CreateShaderModule(new ShaderModuleCreateInfo(File.ReadAllBytes(Path.Combine("ShaderSource", fn))));

        internal void Run() => Application.Run(form);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                renderPass.Dispose();
                swapChainImageViews.ForEach(i => i.Dispose());
                swapChain.Dispose();
                imageAvailableSemaphore.Dispose();
                renderingFinishedSemaphore.Dispose();
                graphicsCommandQueue.Dispose();
                device.Dispose();
                surface.Dispose();
                form.Dispose();
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
