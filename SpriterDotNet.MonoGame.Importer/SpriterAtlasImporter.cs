// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;
using System.Xml;

namespace SpriterDotNet.MonoGame.Importer
{
    [ContentImporter(".json", DisplayName = "Spriter Atlas Importer", DefaultProcessor = "PassThroughProcessor")]
    public class SpriterAtlasImporter : ContentImporter<SpriterAtlasWrapper>
    {
		public override SpriterAtlasWrapper Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing Spriter Atlas file: {0}", filename);
            string jsonData = File.ReadAllText(filename);

			SpriterAtlasWrapper ret = new SpriterAtlasWrapper();

			SpriterAtlasJson atlasJson = JsonConvert.DeserializeObject<SpriterAtlasJson>(jsonData, new RectangleConverter());
			SpriterAtlas atlas = new SpriterAtlas();
			atlasJson.Fill(atlas);

			XmlSerializer serializer = new XmlSerializer(typeof(SpriterAtlas));

			using(StringWriter sww = new StringWriter())
			using(XmlWriter writer = XmlWriter.Create(sww))
			{
				serializer.Serialize(writer, atlas);
				ret.AtlasData = sww.ToString();
			}

			return ret;
        }
    }

	public class SpriterAtlasJson
	{
		public FramesJson Frames { get; set; }
		public Meta Meta { get; set; }

		public void Fill(SpriterAtlas atlas)
		{
			atlas.Meta = Meta;
			atlas.ImageInfos = new List<ImageInfo>();

			foreach(var entry in Frames.ImageInfos)
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
