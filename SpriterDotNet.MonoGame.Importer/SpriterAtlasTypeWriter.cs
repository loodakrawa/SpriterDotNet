// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline;
using SpriterDotNet.MonoGame.Content;

namespace SpriterDotNet.MonoGame.Importer
{
    [ContentTypeWriter]
    public class SpriterAtlasTypeWriter : ContentTypeWriter<SpriterAtlasWrapper>
    {
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(SpriterAtlasTypeReader).AssemblyQualifiedName;
        }

        protected override void Write(ContentWriter output, SpriterAtlasWrapper value)
        {
			output.Write(value.AtlasData);
        }
    }
}
