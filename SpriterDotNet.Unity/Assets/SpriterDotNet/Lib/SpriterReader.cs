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
    /// <summary>
    /// Class responsible for getting a Spriter instance from a string input. It is also responsible for any processing logic such as initialisation.
    /// This class basically contains no parsing / processing logic by itself but has collections of parsers / preprocessors to delegate the work to.
    /// 
    /// For parsing, it iterates over all registered parses until a parser can parse the input string or until it reaches the end.
    /// 
    /// For preprocessing, it invokes all the preprocessors in order.
    /// </summary>
    public class SpriterReader
    {
        /// <summary>
        /// An instance of the default Spriter reader.
        /// </summary>
        public static SpriterReader Default { get; private set; }

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
            if (string.IsNullOrEmpty(data)) return null;

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
