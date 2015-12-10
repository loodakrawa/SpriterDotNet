﻿// Copyright (c) 2015 The original author or authors
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
            foreach (SpriterEntity entity in spriter.Entities)
            {
                entity.Spriter = spriter;
                if (entity.ObjectInfos == null) entity.ObjectInfos = new SpriterObjectInfo[0];
                foreach (SpriterAnimation animation in entity.Animations)
                {
                    animation.Entity = entity;

                    InitInfos(animation);
                    InitVarDefs(animation);
                }
            }
        }

        private static void InitInfos(SpriterAnimation animation)
        {
            if (animation.Timelines == null) animation.Timelines = new SpriterTimeline[0];

            var infos = from t in animation.Timelines
                        from k in t.Keys
                        let o = k.ObjectInfo
                        where o != null && (float.IsNaN(o.PivotX) || float.IsNaN(o.PivotY))
                        select o;

            foreach (SpriterObject info in infos)
            {
                SpriterFile file = animation.Entity.Spriter.Folders[info.FolderId].Files[info.FileId];
                info.PivotX = file.PivotX;
                info.PivotY = file.PivotY;
            }
        }

        private static void InitVarDefs(SpriterAnimation animation)
        {
            if (animation.Meta == null || animation.Meta.Varlines == null || animation.Meta.Varlines.Length == 0) return;

            foreach (SpriterVarline varline in animation.Meta.Varlines)
            {
                SpriterVarDef varDefs = animation.Entity.Variables[varline.Def];
                Init(varDefs, varline);
            }

            foreach (SpriterTimeline timeline in animation.Timelines)
            {
                if (timeline.Meta == null || timeline.Meta.Varlines == null || timeline.Meta.Varlines.Length == 0) continue;
                SpriterObjectInfo objInfo = animation.Entity.ObjectInfos.First(o => o.Name == timeline.Name);
                foreach (SpriterVarline varline in timeline.Meta.Varlines)
                {
                    SpriterVarDef varDef = objInfo.Variables[varline.Def];
                    Init(varDef, varline);
                }
            }
        }

        private static void Init(SpriterVarDef varDef, SpriterVarline varline)
        {
            varDef.VariableValue = GetVarValue(varDef.DefaultValue, varDef.Type);
            foreach (SpriterVarlineKey key in varline.Keys) key.VariableValue = GetVarValue(key.Value, varDef.Type);
        }

        private static SpriterVarValue GetVarValue(string value, SpriterVarType type)
        {
            float floatValue = Single.MinValue;
            int intValue = Int32.MinValue;

            if (type == SpriterVarType.Float) Single.TryParse(value, out floatValue);
            else if (type == SpriterVarType.Int) Int32.TryParse(value, out intValue);

            return new SpriterVarValue
            {
                Type = type,
                StringValue = value,
                FloatValue = floatValue,
                IntValue = intValue
            };
        }

    }
}
