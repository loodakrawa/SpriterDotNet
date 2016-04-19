// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SpriterDotNet.Parser
{
    public class XmlSpriterParser : ISpriterParser
    {
        private static readonly string XmlStart = "<";

        public virtual Spriter Parse(string data)
        {
            data = FixBadNanValue(data);
            XmlSerializer serializer = new XmlSerializer(typeof(Spriter));
            using (TextReader reader = new StringReader(data))
            {
                Spriter spriter = serializer.Deserialize(reader) as Spriter;
                return spriter;
            }
        }

        public virtual bool CanParse(string data)
        {
            return data.StartsWith(XmlStart);
        }

        private static string FixBadNanValue(string data)
        {
            var nanRegex = new Regex(@"(a)=""nan""");
            data = nanRegex.Replace(data, @"$1=""0""");
            return data;
        }
    }
}
