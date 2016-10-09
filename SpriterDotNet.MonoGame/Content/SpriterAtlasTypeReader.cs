// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using System.IO;

namespace SpriterDotNet.MonoGame.Content
{
	public class SpriterAtlasTypeReader : ContentTypeReader<SpriterAtlas>
    {
		protected override SpriterAtlas Read(ContentReader input, SpriterAtlas existingInstance)
        {
            string data = input.ReadString();

			XmlSerializer serializer = new XmlSerializer(typeof(SpriterAtlas));
			using (TextReader reader = new StringReader(data))
			{
				SpriterAtlas spriterAtlas = serializer.Deserialize(reader) as SpriterAtlas;
				return spriterAtlas;
			}
		}
    }
}
