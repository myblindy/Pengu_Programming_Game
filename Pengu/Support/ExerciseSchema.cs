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
        public string? Name { get; set; }
        public int? Size { get; set; }
        [JsonConverter(typeof(StringDataJsonConverter))]
        public byte[]? Data { get; set; }
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
        [EnumMember(Value = "write_memory_literal")]
        WriteMemoryLiteral,
    }

    class Interrupt
    {
        public int Irq { get; set; }
        public InterruptType Type { get; set; }
        public string? InputRegister { get; set; }
        [JsonIgnore]
        public int? InputRegisterNumber => string.IsNullOrWhiteSpace(InputRegister) ? new int?() : int.Parse(InputRegister.AsSpan(1));
        public string? OutputRegister { get; set; }
        [JsonIgnore]
        public int? OutputRegisterNumber => string.IsNullOrWhiteSpace(OutputRegister) ? new int?() : int.Parse(OutputRegister.AsSpan(1));
        public int? OutputLiteral { get; set; }
        public string? MemoryName { get; set; }
    }

    class SevenSegmentDigitDisplay
    {
        public string? Name { get; set; }
    }

    class CPU
    {
        public string? Name { get; set; }
        public int RegisterCount { get; set; }
        public Memory? Memory { get; set; }
        public List<Interrupt>? Interrupts { get; set; }
    }

    [DataContract]
    enum WindowType
    {
        [EnumMember(Value = "hex_editor")]
        HexEditor,
        Assembler,
        Playground
    }

    class DisplayComponent
    {
        public string? Name { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }

    class Window
    {
        public WindowType Type { get; set; }
        public string? MemoryName { get; set; }
        public int? PositionX { get; set; }
        public int? PositionY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? LinesCount { get; set; }
        public FontColor BackColor { get; set; }
        public FontColor ForeColor { get; set; }
        public string? LoadFile { get; set; }
        public List<DisplayComponent>? DisplayComponents { get; set; }
    }

    class Input
    {
        public string? MemoryName { get; set; }
        public int MemoryIndex { get; set; }
        [JsonConverter(typeof(StringDataJsonConverter))]
        public byte[]? Data { get; set; }
    }

    class ExpectationItem
    {
        public string? MemoryName { get; set; }
        public int MemoryIndex { get; set; }
        [JsonConverter(typeof(StringDataJsonConverter))]
        public byte[]? Data { get; set; }
    }

    class Expectation
    {
        public List<ExpectationItem>? ExpectationGroup { get; set; }
    }

    class Solution
    {
        public List<Input>? Inputs { get; set; }
        public List<Expectation>? Expectations { get; set; }
    }

    class Label
    {
        public string? Name { get; set; }
        public string? Text { get; set; }
    }

    class Exercise
    {
        [JsonIgnore]
        public string? Path { get; set; }
        public string? Name { get; set; }
        public List<CPU>? CPUs { get; set; }
        public List<Memory>? Memories { get; set; }
        public List<SevenSegmentDigitDisplay>? SevenSegmentDigitDisplays { get; set; }
        public List<Label> Labels { get; set; }
        public List<Window>? Windows { get; set; }
        public List<Solution>? Solutions { get; set; }

        public FileStream OpenAssociatedFile(string file) =>
            File.OpenRead(System.IO.Path.Combine(Path!, file));
        public string ReadAllAssociatedFile(string file) =>
            File.ReadAllText(System.IO.Path.Combine(Path!, file)).Replace("\r", "");
    }

    class Exercises
    {
        public static Exercise[]? List;

        public static Exercise Get(string name) =>
            List!.First(e => e.Name == name);

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
                {
                    var exercise = await JsonSerializer.DeserializeAsync<Exercise>(file, options).ConfigureAwait(false);
                    exercise.Path = path;

                    results.Add(exercise);
                }

            List = results.ToArray();
        }
    }
}