using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoteCopy
{
    partial class Session
    {
        public required string Host { get; set; }
        public required string Id { get; set; }
        public required long TotalSize { get; set; }
        public required int FileCount { get; set; }
        public required IList<File> FileList { get; set; }
        public required string FromDir { get; set; }

        public class File
        {
            public required string Path { get; init; }
            public required long Size { get; init; }
            public required long Date { get; init; }
        }

        [JsonSerializable(typeof(File))]
        [JsonSerializable(typeof(Session))]
        [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
        partial class JsonContext : JsonSerializerContext
        {
        }

        public string ToJson()
            => JsonSerializer.Serialize(this, JsonContext.Default.Session);

        public static Session? FromJson(string json)
            => JsonSerializer.Deserialize(json, JsonContext.Default.Session);
    }
}