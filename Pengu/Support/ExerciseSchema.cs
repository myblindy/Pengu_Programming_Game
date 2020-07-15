using Pengu.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Pengu.Support
{
    class StringDataJsonConverter : JsonConverter<byte[]>
    {
        [return: MaybeNull]
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<byte>();
            ReadOnlySpan<char> span = reader.GetString();

            while (!span.IsEmpty)
            {
                while (!span.IsEmpty && char.IsWhiteSpace(span[0]))
                    span = span[1..];

                var len = 0;
                while (span.Length > len && !char.IsWhiteSpace(span[len]) && span[len] != ',')
                    ++len;

                var token = span[..len];
                var hexmod = token.StartsWith("0x");
                if (!hexmod && span.Length > 2)
                {
                    // two by two
                    var p = span;
                    for (int idx = 0; idx < span.Length / 2; ++idx, p = p[2..])
                        result.Add(byte.Parse(p[..2]));
                }
                else
                    result.Add(byte.Parse(hexmod ? token[2..] : token, NumberStyles.HexNumber));

                span = span[len..];
                while (!span.IsEmpty && char.IsWhiteSpace(span[0]))
                    span = span[1..];
                if (!span.IsEmpty && span[0] == ',')
                    span = span[1..];
            }

            return result.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) =>
            throw new NotImplementedException();
    }

    class Memory
    {
        public string Name { get; set; }
        public int Size { get; set; }
        [JsonConverter(typeof(StringDataJsonConverter))]
        public byte[] Data { get; set; }
        public bool ReadOnly { get; set; }
    }

    [DataContract]
    enum InterruptType
    {
        [EnumMember(Value = "read_memory")]
        ReadMemory,
        [EnumMember(Value = "write_memory")]
        WriteMemory,
        [EnumMember(Value = "read_write_memory")]
        ReadWriteMemory,
    }

    class Interrupt
    {
        public int Irq { get; set; }
        public InterruptType Type { get; set; }
        public string InputRegister { get; set; }
        public string OutputRegister { get; set; }
        public string MemoryName { get; set; }
    }

    class SevenDigitDisplay
    {
        public string Name { get; set; }
    }

    class CPU
    {
        public string Name { get; set; }
        public int RegisterCount { get; set; }
        public Memory Memory { get; set; }
        public List<Interrupt> Interrupts { get; set; }
    }

    [DataContract]
    enum WindowType
    {
        [EnumMember(Value = "hex_editor")]
        HexEditor,
        Assembler,
        Playground
    }

    class Window
    {
        public WindowType Type { get; set; }
        public string MemoryName { get; set; }
        public int? PositionX { get; set; }
        public int? PositionY { get; set; }
        public FontColor BackColor { get; set; }
        public FontColor ForeColor { get; set; }
        public int? LinesCount { get; set; }
    }

    class Input
    {
        public string MemoryName { get; set; }
        public int MemoryIndex { get; set; }
        [JsonConverter(typeof(StringDataJsonConverter))]
        public byte[] Data { get; set; }
    }

    class ExpectationItem
    {
        public string MemoryName { get; set; }
        public int MemoryIndex { get; set; }
        [JsonConverter(typeof(StringDataJsonConverter))]
        public byte[] Data { get; set; }
    }

    class Expectation
    {
        public List<ExpectationItem> ExpectationGroup { get; set; }
    }

    class Solution
    {
        public List<Input> Inputs { get; set; }
        public List<Expectation> Expectations { get; set; }
    }

    class Exercise
    {
        public string Name { get; set; }
        public List<CPU> CPUs { get; set; }
        public List<Memory> Memories { get; set; }
        public List<SevenDigitDisplay> SevenDigitDisplays { get; set; }
        public List<Window> Windows { get; set; }
        public List<Solution> Solutions { get; set; }
    }

    class Exercises
    {
        public static Exercise[] List;

        public static async Task ReadExercises()
        {
            var results = new List<Exercise>();

            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumMemberConverter() },
            };

            foreach (var path in Directory.EnumerateDirectories("Exercises"))
                using (var file = File.OpenRead(Path.Combine(path, "description.json")))
                    results.Add(await JsonSerializer.DeserializeAsync<Exercise>(file, options).ConfigureAwait(false));

            List = results.ToArray();
        }
    }
}