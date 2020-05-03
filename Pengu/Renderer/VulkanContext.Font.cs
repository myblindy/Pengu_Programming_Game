﻿using System;
using System.Collections.Generic;
using System.Text;
using SharpVk;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Net;
using Pengu.Support;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class Font : IDisposable
        {
            const int InitialCharacterSize = 64;

            readonly VulkanContext context;
            private readonly Dictionary<char, (float u0, float v0, float u1, float v1)> Characters = new Dictionary<char, (float u0, float v0, float u1, float v1)>();

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

            FontVertex[] vertices = new FontVertex[InitialCharacterSize * 6];
            uint usedVertices;

            readonly List<FontString> fontStrings = new List<FontString>();

            public bool IsBufferDataDirty { get; set; }
            public bool IsCommandBufferDirty { get; set; }

            public Font(VulkanContext context, string fontName)
            {
                this.context = context;

                using (var binfile = new BinaryReader(File.Open(Path.Combine("Media", fontName + ".bin"), FileMode.Open)))
                {
                    var length = binfile.BaseStream.Length;
                    do
                    {
                        const float offset1 = 0.020f, offset2u = 0.019f, offset2v = 0.00f;
                        Characters.Add(binfile.ReadChar(), (binfile.ReadSingle() + offset1, binfile.ReadSingle() + offset1, binfile.ReadSingle() + offset2u, binfile.ReadSingle() + offset2v));
                    } while (binfile.BaseStream.Position < length);
                }

                var size = (ulong)(FontVertex.Size * vertices.Length);

                stagingVertexBuffer = context.CreateBuffer(size, BufferUsageFlags.TransferSource,
                    MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingVertexBufferMemory);

                vertexBuffer = context.CreateBuffer(size, BufferUsageFlags.TransferDestination | BufferUsageFlags.VertexBuffer, MemoryPropertyFlags.DeviceLocal, out vertexBufferMemory);

                // build the font texture objects
                fontTextureImage = context.CreateTextureImage("pt_mono.png", context.queueIndices.TransferFamily.Value, out var format, out fontTextureImageMemory);
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

                descriptorSets = context.device.AllocateDescriptorSets(descriptorPool, Enumerable.Repeat(descriptorSetLayout, context.swapChainImages.Length).ToArray());

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
                    new PipelineInputAssemblyStateCreateInfo { Topology = PrimitiveTopology.TriangleList },
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

            public void UpdateBuffer()
            {
                if (IsBufferDataDirty)
                {
                    var charCount = fontStrings.Sum(fs => fs.Value?.Length ?? 0);
                    usedVertices = (uint)(charCount * 6);
                    if (vertices.Length < usedVertices)
                        throw new NotImplementedException();
                    else
                    {
                        // build the string vertices
                        var idx = 0;
                        foreach (var fs in fontStrings)
                        {
                            var x = fs.Position.X;
                            if (!string.IsNullOrWhiteSpace(fs.Value))
                                foreach (var ch in fs.Value)
                                {
                                    var (u0, v0, u1, v1) = Characters[ch];

                                    vertices[idx++] = new FontVertex(new Vector4(x, fs.Position.Y, u0, v0));
                                    vertices[idx++] = new FontVertex(new Vector4(x, fs.Position.Y + fs.Size, u0, v1));
                                    vertices[idx++] = new FontVertex(new Vector4(x + fs.Size, fs.Position.Y, u1, v0));

                                    vertices[idx++] = new FontVertex(new Vector4(x + fs.Size, fs.Position.Y, u1, v0));
                                    vertices[idx++] = new FontVertex(new Vector4(x, fs.Position.Y + fs.Size, u0, v1));
                                    vertices[idx++] = new FontVertex(new Vector4(x + fs.Size, fs.Position.Y + fs.Size, u1, v1));

                                    x += fs.Size;
                                }
                        }

                        var memoryBuffer = stagingVertexBufferMemory.Map(0, usedVertices * FontVertex.Size);
                        vertices.AsSpan(0, (int)usedVertices).CopyTo(memoryBuffer, (int)(usedVertices * FontVertex.Size));
                        stagingVertexBufferMemory.Unmap();

                        context.CopyBuffer(stagingVertexBuffer, vertexBuffer, usedVertices * FontVertex.Size);
                    }

                    IsBufferDataDirty = false;
                }
            }

            public void Draw(CommandBuffer commandBuffer, int idx)
            {
                commandBuffer.BindPipeline(PipelineBindPoint.Graphics, pipeline);
                commandBuffer.BindVertexBuffers(0, vertexBuffer, (DeviceSize)0);
                commandBuffer.BindDescriptorSets(PipelineBindPoint.Graphics, pipelineLayout, 0, descriptorSets[idx], null);
                commandBuffer.Draw(usedVertices, 1, 0, 0);
            }

            public FontString AllocateString(Vector2 pos, float size)
            {
                var fs = new FontString(this, pos, size);
                fontStrings.Add(fs);
                return fs;
            }

            public void FreeString(FontString fs) => fontStrings.Remove(fs);

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

        class FontString
        {
            readonly Font font;

            public FontString(Font font, Vector2 pos, float size)
            {
                this.font = font;
                position = pos;
                this.size = size;
            }

            string value;
            public string Value
            {
                get => value; set
                {
                    font.IsCommandBufferDirty = (string.IsNullOrWhiteSpace(this.value) ? 0 : this.value.Length) != (string.IsNullOrWhiteSpace(value) ? 0 : value.Length);
                    this.value = value;
                    font.IsBufferDataDirty = true;
                }
            }

            Vector2 position;
            public Vector2 Position { get => position; set { position = value; font.IsBufferDataDirty = true; font.IsCommandBufferDirty = true; } }

            float size;
            public float Size { get => size; set { size = value; font.IsBufferDataDirty = true; font.IsCommandBufferDirty = true; } }
        }

        struct FontVertex
        {
            public Vector4 posUv;

            public FontVertex(Vector4 posUv) => this.posUv = posUv;

            public static readonly uint Size = (uint)Marshal.SizeOf<FontVertex>();

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
