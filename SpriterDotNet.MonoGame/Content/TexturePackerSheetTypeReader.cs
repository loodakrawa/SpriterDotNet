// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Content;
using System.Xml.Serialization;
using System.IO;

namespace SpriterDotNet.MonoGame.Content
{
	public class TexturePackerSheetTypeReader : ContentTypeReader<TexturePackerSheet>
    {
		protected override TexturePackerSheet Read(ContentReader input, TexturePackerSheet existingInstance)
        {
            string data = input.ReadString();

			XmlSerializer serializer = new XmlSerializer(typeof(TexturePackerSheet));
			using (TextReader reader = new StringReader(data))
			{
				TexturePackerSheet spriterAtlas = serializer.Deserialize(reader) as TexturePackerSheet;
				return spriterAtlas;
			}
		}
    }
}
