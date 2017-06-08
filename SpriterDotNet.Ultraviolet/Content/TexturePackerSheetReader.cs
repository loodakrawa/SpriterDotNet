using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SpriterDotNet.Ultraviolet.Content
{
    public static class TexturePackerSheetReader
    {
        public static TexturePackerSheet Read(string data)
        {
            SpriterAtlasJson atlasJson = JsonConvert.DeserializeObject<SpriterAtlasJson>(data, new RectangleConverter());
            TexturePackerSheet atlas = new TexturePackerSheet();
            atlasJson.Fill(atlas);
            return atlas;
        }

        public class SpriterAtlasJson
        {
            public FramesJson Frames { get; set; }
            public Meta Meta { get; set; }

            public void Fill(TexturePackerSheet atlas)
            {
                atlas.Meta = Meta;
                atlas.ImageInfos = new List<ImageInfo>();

                foreach (var entry in Frames.ImageInfos)
                {
                    ImageInfo info = entry.Value;
                    info.Name = entry.Key;
                    atlas.ImageInfos.Add(info);
                }
            }
        }

        public class FramesJson
        {
            public Dictionary<string, ImageInfo> ImageInfos { get; set; }
        }

        public class RectangleConverter : CustomConverter<FramesJson>
        {
            protected override object CreateObject(JsonReader reader, JsonSerializer serializer)
            {
                Dictionary<string, ImageInfo> values = serializer.Deserialize<Dictionary<string, ImageInfo>>(reader);
                return new FramesJson { ImageInfos = values };
            }
        }

        public abstract class CustomConverter<T> : JsonConverter
        {
            public override bool CanWrite { get { return false; } }
            public override bool CanRead { get { return true; } }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(T).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                return CreateObject(reader, serializer);
            }

            protected abstract object CreateObject(JsonReader reader, JsonSerializer serializer);
        }
    }
}
