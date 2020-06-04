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
using System.Net.NetworkInformation;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        unsafe class Font : IRenderableModule, IDisposable
        {
            readonly VulkanContext context;
            private readonly Dictionary<char, (float u0, float v0, float u1, float v1)> Characters =
                new Dictionary<char, (float u0, float v0, float u1, float v1)>();

            private TimeSpan totalElapsedTime;

            Buffer vertexIndexBuffer, stagingVertexIndexBuffer;
            DeviceMemory vertexIndexBufferMemory, stagingIndexVertexBufferMemory;
            IntPtr stagingIndexVertexBufferMemoryStartPtr;

            struct PerImageResourcesType
            {
                public Buffer uniformBuffer;
                public DeviceMemory uniformBufferMemory;
                public IntPtr uniformBufferMemoryStartPtr;
                public DescriptorSet descriptorSet;
            }

            PerImageResourcesType[] perImageResources;

            PipelineLayout pipelineLayout;
            DescriptorSetLayout descriptorSetLayout;
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

                perImageResources = new PerImageResourcesType[context.swapChainImages.Length];

                for (int idx = 0; idx < context.swapChainImages.Length; ++idx)
                {
                    perImageResources[idx].uniformBuffer = context.CreateBuffer(FontUniformObject.Size, BufferUsageFlags.UniformBuffer,
                        MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out perImageResources[idx].uniformBufferMemory);
                    perImageResources[idx].uniformBufferMemoryStartPtr = perImageResources[idx].uniformBufferMemory.Map(0, FontUniformObject.Size);
                }

                // build the font texture objects
                fontTextureImage = context.CreateTextureImage("pt_mono.png", out var format, out fontTextureImageMemory);
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

                context.device.AllocateDescriptorSets(descriptorPool, Enumerable.Repeat(descriptorSetLayout, context.swapChainImages.Length).ToArray())
                    .ForEach((ds, idx) => perImageResources[idx].descriptorSet = ds);

                for (int idx = 0; idx < context.swapChainImages.Length; ++idx)
                    context.device.UpdateDescriptorSets(
                        new[]
                        {
                            new WriteDescriptorSet
                            {
                                DestinationSet = perImageResources[idx].descriptorSet,
                                DestinationBinding = 0,
                                DestinationArrayElement = 0,
                                DescriptorType = DescriptorType.UniformBuffer,
                                DescriptorCount = 1,
                                BufferInfo = new[]
                                {
                                    new DescriptorBufferInfo
                                    {
                                        Buffer = perImageResources[idx].uniformBuffer,
                                        Offset = 0,
                                        Range = FontUniformObject.Size
                                    }
                                },
                            },
                            new WriteDescriptorSet
                            {
                                DestinationSet = perImageResources[idx].descriptorSet,
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

                stagingVertexIndexBuffer = context.CreateBuffer(vertexSize + indexSize, BufferUsageFlags.TransferSource,
                    MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingIndexVertexBufferMemory);
                stagingIndexVertexBufferMemoryStartPtr = stagingIndexVertexBufferMemory.Map(0, vertexSize + indexSize);

                vertexIndexBuffer = context.CreateBuffer(vertexSize + indexSize, BufferUsageFlags.TransferDestination | BufferUsageFlags.VertexBuffer,
                    MemoryPropertyFlags.DeviceLocal, out vertexIndexBufferMemory);
            }

            private void DisposeVertexIndexBuffers()
            {
                vertexIndexBuffer?.Dispose();
                vertexIndexBufferMemory?.Free();
                stagingVertexIndexBuffer?.Dispose();
                stagingIndexVertexBufferMemory?.Free();
            }

            public CommandBuffer[] PreRender(uint nextImage)
            {
                CommandBuffer resultCommandBuffer = default;

                if (IsBufferDataDirty)
                {
                    UsedCharacters = (uint)fontStrings.Sum(fs => fs.Value?.Count(s => s != ' ' && s != '\n') ?? 0);

                    if (MaxVertices < UsedVertices)
                    {
                        MaxCharacters = (uint)(UsedCharacters * Math.Max(1.5, (double)UsedVertices / MaxVertices * 1.5));
                        CreateVertexIndexBuffers();
                    }

                    // build the string vertices
                    var vertexPtr = (FontVertex*)stagingIndexVertexBufferMemoryStartPtr.ToPointer();
                    ushort vertexIdx = 0;
                    var indexPtr = (ushort*)(vertexPtr + UsedVertices);

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

                                    *vertexPtr++ = new FontVertex(
                                        new Vector4(x / context.extent.AspectRatio, y, u0, v0), bg, fg, selected, fs.Offset);
                                    *vertexPtr++ = new FontVertex(
                                        new Vector4(x / context.extent.AspectRatio, y + fs.Size, u0, v1), bg, fg, selected, fs.Offset);
                                    *vertexPtr++ = new FontVertex(
                                        new Vector4((x + xSize) / context.extent.AspectRatio, y, u1, v0), bg, fg, selected, fs.Offset);
                                    *vertexPtr++ = new FontVertex(
                                        new Vector4((x + xSize) / context.extent.AspectRatio, y + fs.Size, u1, v1), bg, fg, selected, fs.Offset);

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

                    resultCommandBuffer = context.CopyBuffer(stagingVertexIndexBuffer, vertexIndexBuffer,
                        UsedVertices * FontVertex.Size + UsedIndices * sizeof(ushort));

                    IsBufferDataDirty = false;
                }

                // update the UBO with the time and X/Y offset
                *(FontUniformObject*)perImageResources[nextImage].uniformBufferMemoryStartPtr =
                    new FontUniformObject { time = (float)totalElapsedTime.TotalMilliseconds };

                return resultCommandBuffer is null ? Array.Empty<CommandBuffer>() : new[] { resultCommandBuffer };
            }

            public void Draw(CommandBuffer commandBuffer, int idx)
            {
                commandBuffer.BindPipeline(PipelineBindPoint.Graphics, pipeline);
                commandBuffer.BindVertexBuffers(0, vertexIndexBuffer, 0);
                commandBuffer.BindIndexBuffer(vertexIndexBuffer, UsedVertices * FontVertex.Size, IndexType.Uint16);
                commandBuffer.BindDescriptorSets(PipelineBindPoint.Graphics, pipelineLayout, 0, perImageResources[idx].descriptorSet, null);
                commandBuffer.DrawIndexed(UsedIndices, 1, 0, 0, 0);
            }

            public (float u0, float v0, float u1, float v1) this[char ch] => Characters[ch];

            public FontString AllocateString(Vector2 pos, float size)
            {
                var fs = new FontString(this, pos, size);
                fontStrings.Add(fs);
                return fs;
            }

            public void FreeString(FontString fs) => fontStrings.Remove(fs);

            public void UpdateLogic(TimeSpan elapsedTime) => totalElapsedTime += elapsedTime;

            public bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers) => throw new NotImplementedException();

            public bool ProcessMouseMove(double x, double y) => throw new NotImplementedException();

            public bool ProcessMouseButton(MouseButton button, InputState action, ModifierKeys modifiers) => throw new NotImplementedException();

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
                    descriptorSetLayout.Dispose();
                    pipeline.Dispose();
                    pipelineLayout.Dispose();
                    perImageResources.ForEach(w =>
                    {
                        w.uniformBuffer.Dispose();
                        w.uniformBufferMemory.Free();
                    });
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

            public void Set(string value = null, FontColor? defaultBg = null, FontColor? defaultFg = null, Vector2? offset = null,
                (int start, int count, FontColor bg, FontColor fg, bool selected)[] overrides = null)
            {
                if ((value is null || value == Value) && (!defaultBg.HasValue || defaultBg == DefaultBackground) &&
                    (!defaultFg.HasValue || defaultFg == DefaultForeground) && (!offset.HasValue || offset == Offset) &&
                    (overrides is null || overrides.SequenceEqual(Overrides)))
                {
                    return;
                }

                var nonSpaceLengthNewValue = value?.Count(c => c != '\n' && c != ' ') ?? 0;
                font.IsCommandBufferDirty = Length != nonSpaceLengthNewValue;

                if (!(value is null))
                {
                    Value = value;
                    Length = nonSpaceLengthNewValue;
                }
                if (defaultBg.HasValue) DefaultBackground = defaultBg.Value;
                if (defaultFg.HasValue) DefaultForeground = defaultFg.Value;
                if (!(overrides is null)) Overrides = overrides;
                if (offset.HasValue) Offset = offset.Value;

                font.IsBufferDataDirty = true;
            }

            public string Value { get; private set; }

            public FontColor DefaultBackground { get; private set; }

            public FontColor DefaultForeground { get; private set; }

            public (int start, int count, FontColor bg, FontColor fg, bool selected)[] Overrides { get; private set; }

            public Vector2 Offset { get; private set; }

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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FontVertex
        {
            public Vector4 posUv;
            public int bgFgSelected;
            public Vector2 offset;

            public FontVertex(Vector4 posUv, FontColor bg, FontColor fg, bool selected, Vector2 offset)
            {
                this.posUv = posUv;
                bgFgSelected = ((int)bg << 16) | ((int)fg << 8) | (selected ? 1 : 0);
                this.offset = offset;
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
                        Format = Format.R32UInt,
                        Offset = (uint)Marshal.OffsetOf<FontVertex>(nameof(bgFgSelected)),
                    },
                    new VertexInputAttributeDescription
                    {
                        Binding = 0,
                        Location = 2,
                        Format = Format.R32G32SFloat,
                        Offset = (uint)Marshal.OffsetOf<FontVertex>(nameof(offset)),
                    },
                };
        }
    }
}
