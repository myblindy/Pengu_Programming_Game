using System;
using System.Collections.Generic;
using System.Text;
using SharpVk;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class Font : IDisposable
        {
            const int InitialCharacterSize = 64;

            readonly VulkanContext context;
            SharpVk.Buffer vertexBuffer, stagingVertexBuffer;
            DeviceMemory vertexBufferMemory, stagingVertexBufferMemory;
            PipelineLayout pipelineLayout;
            DescriptorSetLayout descriptorSetLayout;
            DescriptorSet[] descriptorSets;
            Pipeline pipeline;
            Image fontTextureImage;
            DeviceMemory fontTextureImageMemory;
            ImageView fontTextureImageView;
            Sampler fontTextureSampler;

            readonly FontVertex[] vertices =
            {
                new FontVertex(new Vector4(-0.5f, -0.5f, 0, 0)),
                new FontVertex(new Vector4(-0.5f, 0.5f, 0, 1)),
                new FontVertex(new Vector4(0.5f, -0.5f, 1, 0)),
                new FontVertex(new Vector4(0.5f, 0.5f, 1, 1)),
            };

            public Font(VulkanContext context)
            {
                this.context = context;

                var size = (ulong)(FontVertex.Size * vertices.Length);

                context.CreateBuffer(size, BufferUsageFlags.TransferSource,
                    MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingVertexBuffer, out stagingVertexBufferMemory);

                var memoryBuffer = stagingVertexBufferMemory.Map(0, size);
                for (int idx = 0; idx < vertices.Length; ++idx)
                    Marshal.StructureToPtr(vertices[idx], memoryBuffer + (int)(idx * FontVertex.Size), false);
                stagingVertexBufferMemory.Unmap();

                context.CreateBuffer(size, BufferUsageFlags.TransferDestination | BufferUsageFlags.VertexBuffer, MemoryPropertyFlags.DeviceLocal,
                    out vertexBuffer, out vertexBufferMemory);
                context.CopyBuffer(stagingVertexBuffer, vertexBuffer, size);

                // build the font texture objects
                context.CreateTextureImage("pt_mono.png", context.queueIndices.TransferFamily.Value, out fontTextureImage, out var format, out fontTextureImageMemory);
                fontTextureImageView = context.device.CreateImageView(fontTextureImage, ImageViewType.ImageView2d, format, ComponentMapping.Identity,
                    new ImageSubresourceRange { AspectMask = ImageAspectFlags.Color, LayerCount = 1, LevelCount = 1 });

                fontTextureSampler = context.device.CreateSampler(Filter.Linear, Filter.Linear, SamplerMipmapMode.Linear, SamplerAddressMode.ClampToBorder, SamplerAddressMode.ClampToBorder,
                    SamplerAddressMode.ClampToBorder, 0, false, 1, false, CompareOp.Always, 0, 0, BorderColor.IntOpaqueBlack, false);

                using var vShader = context.CreateShaderModule("font.vert.spv");
                using var fShader = context.CreateShaderModule("font.frag.spv");

                var descriptorPool = context.device.CreateDescriptorPool((uint)context.swapChainImages.Length, new DescriptorPoolSize
                {
                    Type = DescriptorType.CombinedImageSampler,
                    DescriptorCount = (uint)context.swapChainImages.Length
                });

                descriptorSetLayout = context.device.CreateDescriptorSetLayout(new DescriptorSetLayoutBinding
                {
                    Binding = 1,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    StageFlags = ShaderStageFlags.Fragment,
                });

                descriptorSets = context.device.AllocateDescriptorSets(descriptorPool, Enumerable.Range(0, context.swapChainImages.Length).Select(_ => descriptorSetLayout).ToArray());

                for (int idx = 0; idx < context.swapChainImages.Length; ++idx)
                    context.device.UpdateDescriptorSets(
                        new WriteDescriptorSet
                        {
                            DestinationSet = descriptorSets[idx],
                            DestinationBinding = 1,
                            DestinationArrayElement = 0,
                            DescriptorType = DescriptorType.CombinedImageSampler,
                            DescriptorCount = 1,
                            ImageInfo = new[]
                            {
                                new DescriptorImageInfo
                                {
                                    ImageLayout= ImageLayout.ShaderReadOnlyOptimal,
                                    ImageView = fontTextureImageView,
                                    Sampler = fontTextureSampler,
                                }
                            }
                        }, null);

                pipelineLayout = context.device.CreatePipelineLayout(descriptorSetLayout, null);

                pipeline = context.device.CreateGraphicsPipeline(null,
                    new[]
                    {
                        new PipelineShaderStageCreateInfo { Stage = ShaderStageFlags.Vertex, Module = vShader, Name = "main" },
                        new PipelineShaderStageCreateInfo { Stage = ShaderStageFlags.Fragment, Module = fShader, Name = "main" },
                    },
                    new PipelineVertexInputStateCreateInfo
                    {
                        VertexAttributeDescriptions = FontVertex.AttributeDescriptions,
                        VertexBindingDescriptions = new[] { FontVertex.BindingDescription },
                    },
                    new PipelineInputAssemblyStateCreateInfo { Topology = PrimitiveTopology.TriangleStrip },
                    new PipelineRasterizationStateCreateInfo { LineWidth = 1 },
                    pipelineLayout, context.renderPass, 0, null, -1,
                    viewportState: new PipelineViewportStateCreateInfo
                    {
                        Viewports = new[] { new Viewport(0, 0, context.extent.Width, context.extent.Height, 0, 1) },
                        Scissors = new[] { new Rect2D(context.extent) },
                    },
                    colorBlendState: new PipelineColorBlendStateCreateInfo
                    {
                        Attachments = new[]
                        {
                            new PipelineColorBlendAttachmentState
                            {
                                ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A,
                            }
                        }
                    },
                    multisampleState: new PipelineMultisampleStateCreateInfo
                    {
                        SampleShadingEnable = false,
                        RasterizationSamples = SampleCountFlags.SampleCount1,
                        MinSampleShading = 1
                    });

            }

            public void Draw(CommandBuffer commandBuffer, int idx)
            {
                commandBuffer.BindPipeline(PipelineBindPoint.Graphics, pipeline);
                commandBuffer.BindVertexBuffers(0, vertexBuffer, (DeviceSize)0);
                commandBuffer.BindDescriptorSets(PipelineBindPoint.Graphics, pipelineLayout, 0, descriptorSets[idx], null);
                commandBuffer.Draw((uint)vertices.Length, 1, 0, 0);
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // managed resources
                    }

                    // native resources
                    fontTextureSampler.Dispose();
                    fontTextureImageView.Dispose();
                    fontTextureImage.Dispose();
                    fontTextureImageMemory.Free();
                    pipeline.Dispose();
                    vertexBuffer.Dispose();
                    vertexBufferMemory.Free();
                    stagingVertexBuffer.Dispose();
                    stagingVertexBufferMemory.Free();

                    disposedValue = true;
                }
            }

            ~Font()
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

        struct FontVertex
        {
            Vector4 posUv;

            public FontVertex(Vector4 posUv) => this.posUv = posUv;

            public static uint Size = (uint)Marshal.SizeOf<FontVertex>();

            public static readonly VertexInputBindingDescription BindingDescription =
                new VertexInputBindingDescription
                {
                    Binding = 0,
                    Stride = Size,
                    InputRate = VertexInputRate.Vertex
                };

            public static readonly VertexInputAttributeDescription[] AttributeDescriptions =
                new[]
                {
                    new VertexInputAttributeDescription
                    {
                        Binding = 0,
                        Location = 0,
                        Format = Format.R32G32B32A32SFloat,
                        Offset = (uint)Marshal.OffsetOf<FontVertex>(nameof(posUv)),
                    }
                };
        }
    }
}
