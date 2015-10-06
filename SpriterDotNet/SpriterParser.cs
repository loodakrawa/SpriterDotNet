// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SpriterDotNet.Parser;

namespace SpriterDotNet
{
    public static class SpriterParser
    {
        public static readonly ICollection<ISpriterParser> Parsers = new List<ISpriterParser>();

        static SpriterParser()
        {
            Parsers.Add(new XmlSpriterParser());
        }

        public static Spriter Parse(string data)
        {
            if (data != null) data = data.Trim();
            if (String.IsNullOrEmpty(data)) return null;

            Spriter spriter = null;
            foreach (ISpriterParser parser in Parsers)
            {
                if (!parser.CanParse(data)) continue;
                spriter = parser.Parse(data);
                break;
            }

            if (spriter != null) Init(spriter);

            return spriter;
        }

        public static void Init(Spriter spriter)
        {
            var infos = from e in spriter.Entities
                        from a in e.Animations
                        from t in a.Timelines
                        from k in t.Keys
                        let o = k.ObjectInfo
                        where o != null && (float.IsNaN(o.PivotX) || float.IsNaN(o.PivotY))
                        select o;

            foreach (SpriterObjectInfo info in infos)
            {
                SpriterFile file = spriter.Folders[info.FolderId].Files[info.FileId];
                info.PivotX = file.PivotX;
                info.PivotY = file.PivotY;
            }
        }
    }
}
