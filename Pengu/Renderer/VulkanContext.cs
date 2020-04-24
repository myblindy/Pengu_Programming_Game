using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    new DebugReportCallbackCreateInfoExt(DebugReportFlagsExt.Error | DebugReportFlagsExt.PerformanceWarning,
                    args =>
                    {
                        Debug.WriteLine($"[{args.Flags}][{args.LayerPrefix}] {args.Message}");
                        return args.Flags.HasFlag(DebugReportFlagsExt.Error);
                    }));
            }

            // create form and surface
            form = new Form { Width = 1280, Height = 720 };
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
            var deviceCreateInfo = new DeviceCreateInfo(new[] { deviceQueueCreateInfo }, new[] { Constant.DeviceExtension.KhrSwapchain },
                physicalDeviceFeatures);
            device = physicalDevice.CreateDevice(deviceCreateInfo);

            // get the queue and create the pool
            graphicsPresentQueue = device.GetQueue(queueFamilyIndex);
            graphicsCommandQueue = device.CreateCommandPool(new CommandPoolCreateInfo(queueFamilyIndex));

            imageAvailableSemaphore = device.CreateSemaphore();
            renderingFinishedSemaphore = device.CreateSemaphore();
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
