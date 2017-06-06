﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Content.Pipeline;
using System.IO;

namespace SpriterDotNet.MonoGame.Importer
{
    [ContentImporter(".scml", DisplayName = "Scml Importer", DefaultProcessor = "PassThroughProcessor")]
    public class SpriterImporter : ContentImporter<SpriterWrapper>
    {
        public override SpriterWrapper Import(string filename, ContentImporterContext context)
        {
            context.Logger.LogMessage("Importing SCML file: {0}", filename);
            string data = File.ReadAllText(filename);
            return new SpriterWrapper { SpriterData = data };
        }
    }
}
