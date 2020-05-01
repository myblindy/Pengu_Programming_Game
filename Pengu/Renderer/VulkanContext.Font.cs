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
            Pipeline pipeline;
            Image fontTextureImage;
            DeviceMemory fontTextureImageMemory;

            readonly FontVertex[] vertices =
            {
                new FontVertex(new Vector2(-0.5f, -0.5f)),
                new FontVertex(new Vector2(-0.5f, 0.5f)),
                new FontVertex(new Vector2(0.5f, -0.5f)),
                new FontVertex(new Vector2(0.5f, 0.5f)),
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
                context.CreateTextureImage("pt_mono.png", context.queueIndices.TransferFamily.Value, out fontTextureImage, out fontTextureImageMemory);

                using var vShader = context.CreateShaderModule("font.vert.spv");
                using var fShader = context.CreateShaderModule("font.frag.spv");

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
                    context.pipelineLayout, context.renderPass, 0, null, -1,
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

            public void Draw(CommandBuffer commandBuffer)
            {
                commandBuffer.BindPipeline(PipelineBindPoint.Graphics, pipeline);
                commandBuffer.BindVertexBuffers(0, vertexBuffer, (DeviceSize)0);
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
            Vector2 position;

            public FontVertex(Vector2 position) => this.position = position;

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
                        Format = Format.R32G32SFloat,
                        Offset = (uint)Marshal.OffsetOf<FontVertex>(nameof(position)),
                    }
                };
        }
    }
}
