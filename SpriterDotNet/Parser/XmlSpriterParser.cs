// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using System.IO;
using System.Xml.Serialization;

namespace SpriterDotNet.Parser
{
    public class XmlSpriterParser : ISpriterParser
    {
        private static readonly string XmlStart = "<";

        public Spriter Parse(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Spriter));
            using (TextReader reader = new StringReader(data))
            {
                Spriter spriter = serializer.Deserialize(reader) as Spriter;
                return spriter;
            }
        }

        public bool CanParse(string data)
        {
            return data.StartsWith(XmlStart);
        }
    }
}
