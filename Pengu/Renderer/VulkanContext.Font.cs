using System;
using System.Collections.Generic;
using System.Text;
using SharpVk;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Net;
using Pengu.Support;
using System.Runtime.CompilerServices;
using GLFW;

using Image = SharpVk.Image;
using Buffer = SharpVk.Buffer;
using MoreLinq;
using SixLabors.ImageSharp;
using SharpVk.Interop.Khronos;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class Font : IRenderableModule, IDisposable
        {
            readonly VulkanContext context;
            private readonly Dictionary<char, (float u0, float v0, float u1, float v1)> Characters = new Dictionary<char, (float u0, float v0, float u1, float v1)>();

            private TimeSpan totalElapsedTime;

            Buffer vertexBuffer, stagingVertexBuffer, indexBuffer, stagingIndexBuffer;
            DeviceMemory vertexBufferMemory, stagingVertexBufferMemory, indexBufferMemory, stagingIndexBufferMemory;

            Buffer[] uniformBuffers;
            DeviceMemory[] uniformBufferMemories;

            PipelineLayout pipelineLayout;
            DescriptorSetLayout descriptorSetLayout;
            DescriptorSet[] descriptorSets;
            Pipeline pipeline;
            Image fontTextureImage;
            DeviceMemory fontTextureImageMemory;
            ImageView fontTextureImageView;
            Sampler fontTextureSampler;

            public uint MaxCharacters { get; private set; } = 2000;
            public uint MaxVertices => MaxCharacters * 4;
            public uint MaxIndices => MaxCharacters * 6;

            public uint UsedCharacters { get; private set; }
            public uint UsedVertices => UsedCharacters * 4;
            public uint UsedIndices => UsedCharacters * 6;

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
                        const float offset1 = 0.00f, offset2u = 0, offset2v = 0.00f;
                        Characters.Add(binfile.ReadChar(), (binfile.ReadSingle() + offset1, binfile.ReadSingle() + offset1, binfile.ReadSingle() + offset2u, binfile.ReadSingle() + offset2v));
                    } while (binfile.BaseStream.Position < length);
                }

                CreateVertexIndexBuffers();

                uniformBuffers = new Buffer[context.swapChainImages.Length];
                uniformBufferMemories = new DeviceMemory[context.swapChainImages.Length];
                for (int idx = 0; idx < context.swapChainImages.Length; ++idx)
                    uniformBuffers[idx] = context.CreateBuffer(FontUniformObject.Size, BufferUsageFlags.UniformBuffer,
                        MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out uniformBufferMemories[idx]);

                // build the font texture objects
                fontTextureImage = context.CreateTextureImage("pt_mono.png", context.queueIndices.TransferFamily.Value, out var format, out fontTextureImageMemory);
                fontTextureImageView = context.device.CreateImageView(fontTextureImage, ImageViewType.ImageView2d, format, ComponentMapping.Identity,
                    new ImageSubresourceRange { AspectMask = ImageAspectFlags.Color, LayerCount = 1, LevelCount = 1 });

                fontTextureSampler = context.device.CreateSampler(Filter.Linear, Filter.Linear, SamplerMipmapMode.Linear, SamplerAddressMode.ClampToBorder, SamplerAddressMode.ClampToBorder,
                    SamplerAddressMode.ClampToBorder, 0, false, 1, false, CompareOp.Always, 0, 0, BorderColor.IntOpaqueBlack, false);

                using var vShader = context.CreateShaderModule("font.vert.spv");
                using var fShader = context.CreateShaderModule("font.frag.spv");

                var descriptorPool = context.device.CreateDescriptorPool((uint)context.swapChainImages.Length,
                    new[]
                    {
                        new DescriptorPoolSize
                        {
                            Type = DescriptorType.UniformBuffer,
                            DescriptorCount = (uint)context.swapChainImages.Length,
                        },
                        new DescriptorPoolSize
                        {
                            Type = DescriptorType.CombinedImageSampler,
                            DescriptorCount = (uint)context.swapChainImages.Length
                        }
                    });

                descriptorSetLayout = context.device.CreateDescriptorSetLayout(
                    new[]
                    {
                        new DescriptorSetLayoutBinding
                        {
                            Binding = 0,
                            DescriptorCount = 1,
                            DescriptorType = DescriptorType.UniformBuffer,
                            StageFlags = ShaderStageFlags.Vertex,
                        },
                        new DescriptorSetLayoutBinding
                        {
                            Binding = 1,
                            DescriptorCount = 1,
                            DescriptorType = DescriptorType.CombinedImageSampler,
                            StageFlags = ShaderStageFlags.Fragment,
                        }
                    });

                descriptorSets = context.device.AllocateDescriptorSets(descriptorPool, Enumerable.Repeat(descriptorSetLayout, context.swapChainImages.Length).ToArray());

                for (int idx = 0; idx < context.swapChainImages.Length; ++idx)
                    context.device.UpdateDescriptorSets(
                        new[]
                        {
                            new WriteDescriptorSet
                            {
                                DestinationSet = descriptorSets[idx],
                                DestinationBinding = 0,
                                DestinationArrayElement = 0,
                                DescriptorType = DescriptorType.UniformBuffer,
                                DescriptorCount = 1,
                                BufferInfo = new[]
                                {
                                    new DescriptorBufferInfo
                                    {
                                        Buffer = uniformBuffers[idx],
                                        Offset = 0,
                                        Range = FontUniformObject.Size
                                    }
                                },
                            },
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
                            }
                        }, null);

                pipelineLayout = context.device.CreatePipelineLayout(descriptorSetLayout, null);

                pipeline = context.device.CreateGraphicsPipeline(null,
                    new[]
                    {
                        new PipelineShaderStageCreateInfo { Stage = ShaderStageFlags.Vertex, Module = vShader, Name = "main" },
                        new PipelineShaderStageCreateInfo { Stage = ShaderStageFlags.Fragment, Module = fShader, Name = "main" },
                    },
                    new PipelineRasterizationStateCreateInfo { LineWidth = 1 },
                    pipelineLayout, context.renderPass, 0, null, -1,
                    vertexInputState: new PipelineVertexInputStateCreateInfo
                    {
                        VertexAttributeDescriptions = FontVertex.AttributeDescriptions,
                        VertexBindingDescriptions = new[] { FontVertex.BindingDescription },
                    },
                    inputAssemblyState: new PipelineInputAssemblyStateCreateInfo { Topology = PrimitiveTopology.TriangleList },
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

            private void CreateVertexIndexBuffers()
            {
                var vertexSize = (ulong)(FontVertex.Size * MaxVertices);
                var indexSize = (ulong)(sizeof(ushort) * MaxIndices);

                DisposeVertexIndexBuffers();

                stagingVertexBuffer = context.CreateBuffer(vertexSize, BufferUsageFlags.TransferSource,
                    MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingVertexBufferMemory);

                vertexBuffer = context.CreateBuffer(vertexSize, BufferUsageFlags.TransferDestination | BufferUsageFlags.VertexBuffer,
                    MemoryPropertyFlags.DeviceLocal, out vertexBufferMemory);

                stagingIndexBuffer = context.CreateBuffer(indexSize, BufferUsageFlags.TransferSource,
                    MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingIndexBufferMemory);

                indexBuffer = context.CreateBuffer(indexSize, BufferUsageFlags.TransferDestination | BufferUsageFlags.VertexBuffer,
                    MemoryPropertyFlags.DeviceLocal, out indexBufferMemory);
            }

            private void DisposeVertexIndexBuffers()
            {
                vertexBuffer?.Dispose();
                vertexBufferMemory?.Free();
                indexBuffer?.Dispose();
                indexBufferMemory?.Free();
                stagingVertexBuffer?.Dispose();
                stagingVertexBufferMemory?.Free();
                stagingIndexBuffer?.Dispose();
                stagingIndexBufferMemory?.Free();
            }

            public void PreRender(uint nextImage)
            {
                if (IsBufferDataDirty)
                {
                    UsedCharacters = (uint)fontStrings.Sum(fs => fs.Value?.Count(s => s != ' ' && s != '\n') ?? 0);

                    if (MaxVertices < UsedVertices)
                    {
                        MaxCharacters = (uint)(UsedCharacters * Math.Max(1.5, (double)UsedVertices / MaxVertices * 1.5));
                        CreateVertexIndexBuffers();
                    }

                    unsafe
                    {
                        // build the string vertices
                        var vertexPtr = (FontVertex*)stagingVertexBufferMemory.Map(0, UsedVertices * FontVertex.Size);
                        ushort vertexIdx = 0;
                        var indexPtr = (ushort*)stagingIndexBufferMemory.Map(0, UsedIndices * sizeof(ushort));

                        foreach (var fs in fontStrings)
                            if (!string.IsNullOrWhiteSpace(fs.Value))
                            {
                                var x = fs.Position.X;
                                var y = fs.Position.Y;
                                int charIndex = 0;

                                foreach (var ch in fs.Value)
                                {
                                    if (ch == '\n')
                                    {
                                        x = fs.Position.X;
                                        y += fs.Size;
                                    }
                                    else if (ch == ' ')
                                    {
                                        var (u0, v0, u1, v1) = Characters[' '];
                                        x += fs.Size * (u1 - u0) / (v1 - v0);
                                    }
                                    else
                                    {
                                        var (u0, v0, u1, v1) = Characters[ch == ' ' ? ' ' : ch];
                                        var aspect = (u1 - u0) / (v1 - v0);
                                        var xSize = fs.Size * aspect;

                                        var @override = fs.TryGetOverrideForIndex(charIndex);

                                        var bg = FontColor.Black;
                                        var fg = FontColor.BrightGreen;
                                        var selected = false;

                                        if (@override.HasValue)
                                            (bg, fg, selected) = (@override.Value.bg, @override.Value.fg, @override.Value.selected);

                                        *vertexPtr++ = new FontVertex(new Vector4(x / context.extent.AspectRatio, y, u0, v0), bg, fg, selected);
                                        *vertexPtr++ = new FontVertex(new Vector4(x / context.extent.AspectRatio, y + fs.Size, u0, v1), bg, fg, selected);
                                        *vertexPtr++ = new FontVertex(new Vector4((x + xSize) / context.extent.AspectRatio, y, u1, v0), bg, fg, selected);
                                        *vertexPtr++ = new FontVertex(new Vector4((x + xSize) / context.extent.AspectRatio, y + fs.Size, u1, v1), bg, fg, selected);

                                        *indexPtr++ = (ushort)(vertexIdx + 0);
                                        *indexPtr++ = (ushort)(vertexIdx + 1);
                                        *indexPtr++ = (ushort)(vertexIdx + 2);

                                        *indexPtr++ = (ushort)(vertexIdx + 2);
                                        *indexPtr++ = (ushort)(vertexIdx + 1);
                                        *indexPtr++ = (ushort)(vertexIdx + 3);

                                        vertexIdx += 4;

                                        x += xSize;
                                    }

                                    ++charIndex;
                                }
                            }
                    }
                    stagingIndexBufferMemory.Unmap();
                    stagingVertexBufferMemory.Unmap();

                    context.CopyBuffer(stagingVertexBuffer, vertexBuffer, UsedVertices * FontVertex.Size);
                    context.CopyBuffer(stagingIndexBuffer, indexBuffer, UsedIndices * sizeof(ushort));

                    IsBufferDataDirty = false;
                }

                // update the UBO with the time
                unsafe
                {
                    var memPtr = (FontUniformObject*)uniformBufferMemories[nextImage].Map(0, FontUniformObject.Size);
                    *memPtr = new FontUniformObject { time = (float)totalElapsedTime.TotalMilliseconds };
                }
                uniformBufferMemories[nextImage].Unmap();
            }

            public void Draw(CommandBuffer commandBuffer, int idx)
            {
                commandBuffer.BindPipeline(PipelineBindPoint.Graphics, pipeline);
                commandBuffer.BindVertexBuffers(0, vertexBuffer, 0);
                commandBuffer.BindIndexBuffer(indexBuffer, 0, IndexType.Uint16);
                commandBuffer.BindDescriptorSets(PipelineBindPoint.Graphics, pipelineLayout, 0, descriptorSets[idx], null);
                commandBuffer.DrawIndexed(UsedIndices, 1, 0, 0, 0);
            }

            public FontString AllocateString(Vector2 pos, float size)
            {
                var fs = new FontString(this, pos, size);
                fontStrings.Add(fs);
                return fs;
            }

            public void FreeString(FontString fs) => fontStrings.Remove(fs);

            public void UpdateLogic(TimeSpan elapsedTime) => totalElapsedTime += elapsedTime;

            public bool ProcessKey(Keys key, int scanCode, InputState state, ModifierKeys modifiers) => throw new NotImplementedException();

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
                    uniformBuffers.ForEach(w => w.Dispose());
                    uniformBufferMemories.ForEach(w => w.Free());
                    DisposeVertexIndexBuffers();

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

            public (FontColor bg, FontColor fg, bool selected)? TryGetOverrideForIndex(int needle)
            {
                if (Overrides is null) return null;

                int min = 0, max = Overrides.Length, idx = (max - min) / 2;
                while (true)
                {
                    if (Overrides[idx].start <= needle && Overrides[idx].start + Overrides[idx].count > needle)
                        return (Overrides[idx].bg, Overrides[idx].fg, Overrides[idx].selected);

                    if (idx == max || idx == min) return null;

                    if (Overrides[idx].start + Overrides[idx].count <= needle)
                        min = idx;
                    else
                        max = idx;
                    idx = (max - min) / 2 + min;
                }
            }

            public void Set(string value, FontColor defaultBg, FontColor defaultFg, (int start, int count, FontColor bg, FontColor fg, bool selected)[] overrides)
            {
                if (value == Value && defaultBg == DefaultBackground && defaultFg == DefaultForeground && ((overrides is null && Overrides is null) ||
                    (!(overrides is null) && !(Overrides is null) && overrides.SequenceEqual(Overrides))))
                {
                    return;
                }

                font.IsCommandBufferDirty = (string.IsNullOrWhiteSpace(Value) ? 0 : Value.Length) != (string.IsNullOrWhiteSpace(value) ? 0 : value.Length);

                Value = value;
                DefaultBackground = defaultBg;
                DefaultForeground = defaultFg;
                Overrides = overrides;

                Length = value.Count(c => c != '\n' && c != ' ');

                font.IsBufferDataDirty = true;
            }

            public string Value { get; private set; }

            public FontColor DefaultBackground { get; private set; }

            public FontColor DefaultForeground { get; private set; }

            public (int start, int count, FontColor bg, FontColor fg, bool selected)[] Overrides { get; private set; }

            //string value;
            //public string Value
            //{
            //    get => value;
            //    set
            //    {
            //        font.IsCommandBufferDirty = (string.IsNullOrWhiteSpace(this.value) ? 0 : this.value.Length) != (string.IsNullOrWhiteSpace(value) ? 0 : value.Length);
            //        this.value = value;
            //        Length = value.Count(c => c != '\n' && c != ' ');
            //        font.IsBufferDataDirty = true;
            //    }
            //}

            //int[] selectedCharacters;
            //public int[] SelectedCharacters
            //{
            //    get => selectedCharacters;
            //    set
            //    {
            //        if ((!(selectedCharacters is null) && !(value is null) && !selectedCharacters.SequenceEqual(value)) || (selectedCharacters is null && !(value is null)) || (!(selectedCharacters is null) && value is null))
            //        {
            //            selectedCharacters = value;
            //            font.IsBufferDataDirty = true;
            //        }
            //    }
            //}

            public int Length { get; private set; }

            Vector2 position;
            public Vector2 Position { get => position; set { position = value; font.IsBufferDataDirty = true; font.IsCommandBufferDirty = true; } }

            float size;
            public float Size { get => size; set { size = value; font.IsBufferDataDirty = true; font.IsCommandBufferDirty = true; } }
        }

        struct FontUniformObject
        {
            public float time;

            public static readonly uint Size = (uint)Marshal.SizeOf<FontUniformObject>();
        }

        enum FontColor
        {
            Black,
            DarkBlue,
            DarkGreen,
            DarkCyan,
            DarkRed,
            DarkMagenta,
            DarkYellow,
            DarkWhite,
            BrightBlack,
            BrightBlue,
            BrightGreen,
            BrightCyan,
            BrightRed,
            BrightMagenta,
            BrightYellow,
            White
        }

        struct FontVertex
        {
            public Vector4 posUv;
            public Vector3 bgFgSelected;

            public FontVertex(Vector4 posUv, FontColor bg, FontColor fg, bool selected)
            {
                this.posUv = posUv;
                bgFgSelected = new Vector3((int)bg, (int)fg, selected ? 1 : 0);
            }

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
                    },
                    new VertexInputAttributeDescription
                    {
                        Binding = 0,
                        Location = 1,
                        Format = Format.R32G32B32SFloat,
                        Offset = (uint)Marshal.OffsetOf<FontVertex>(nameof(bgFgSelected)),
                    },
                };
        }
    }
}
