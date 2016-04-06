// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;
using SpriterDotNet.Parser;
using SpriterDotNet.Preprocessors;
using System;

namespace SpriterDotNet
{
    public class SpriterReader
    {
        public static SpriterReader Default { get; set; }

        static SpriterReader()
        {
            Default = new SpriterReader();
            Default.Parsers.Add(new XmlSpriterParser());
            Default.Preprocessors.Add(new SpriterInitPreprocessor());
        }

        public ICollection<ISpriterParser> Parsers { get; set; }
        public ICollection<ISpriterPreprocessor> Preprocessors { get; set; }

        public SpriterReader()
        {
            Parsers = new List<ISpriterParser>();
            Preprocessors = new List<ISpriterPreprocessor>();
        }

        public virtual Spriter Read(string data)
        {
            if (data == null) throw new ArgumentNullException("data");

            data = data.Trim();
            if (string.IsNullOrWhiteSpace(data)) return null;

            Spriter spriter = null;
            foreach (ISpriterParser parser in Parsers)
            {
                if (!parser.CanParse(data)) continue;
                spriter = parser.Parse(data);
                break;
            }

            foreach (ISpriterPreprocessor preprocessor in Preprocessors) preprocessor.Preprocess(spriter);

            return spriter;
        }
    }
}
