/**
The MIT License (MIT)

Copyright (c) 2015 Luka "loodakrawa" Sverko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
**/

using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SpriterDotNet
{
    [XmlRoot("spriter_data")]
    [Serializable]
    public class Spriter
    {
        public static Spriter Parse(string data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Spriter));
            using (TextReader reader = new StringReader(data))
            {
                Spriter spriter = serializer.Deserialize(reader) as Spriter;
                SetDefaultPivots(spriter);
                return spriter;
            }
        }

        private static void SetDefaultPivots(Spriter spriter)
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

        [XmlElement("folder")]
        public SpriterFolder[] Folders { get; set; }

        [XmlElement("entity")]
        public SpriterEntity[] Entities { get; set; }
    }

    [Serializable]
    public class SpriterFolder : SpriterElement
    {
        [XmlElement("file")]
        public SpriterFile[] Files { get; set; }
    }

    [Serializable]
    public class SpriterFile : SpriterElement
    {
        [XmlAttribute("pivot_x")]
        public float PivotX { get; set; }

        [XmlAttribute("pivot_y")]
        public float PivotY { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        public SpriterFile()
        {
            PivotX = 0f;
            PivotY = 1f;
        }
    }

    [Serializable]
    public class SpriterEntity : SpriterElement
    {
        [XmlElement("character_map")]
        public SpriterCharacterMap[] CharacterMaps { get; set; }

        [XmlElement("animation")]
        public SpriterAnimation[] Animations { get; set; }
    }

    [Serializable]
    public class SpriterCharacterMap : SpriterElement
    {
        [XmlElement("map")]
        public SpriterMapInstruction[] Maps { get; set; }
    }

    [Serializable]
    public class SpriterMapInstruction
    {
        [XmlAttribute("folder")]
        public int FolderId { get; set; }

        [XmlAttribute("file")]
        public int FileId { get; set; }

        [XmlAttribute("target_folder")]
        public int TargetFolderId { get; set; }

        [XmlAttribute("target_file")]
        public int TargetFileId { get; set; }

        public SpriterMapInstruction()
        {
            TargetFolderId = -1;
            TargetFileId = -1;
        }
    }

    [Serializable]
    public class SpriterAnimation : SpriterElement
    {
        [XmlAttribute("length")]
        public float Length { get; set; }

        [XmlAttribute("looping")]
        public bool Looping { get; set; }

        [XmlArray("mainline"), XmlArrayItem("key")]
        public SpriterMainLineKey[] MainlineKeys { get; set; }

        [XmlElement("timeline")]
        public SpriterTimeLine[] Timelines { get; set; }

        public SpriterAnimation()
        {
            Looping = true;
        }
    }

    [Serializable]
    public class SpriterMainLineKey : SpriterKey
    {
        [XmlElement("bone_ref")]
        public SpriterRef[] BoneRefs { get; set; }

        [XmlElement("object_ref")]
        public SpriterObjectRef[] ObjectRefs { get; set; }
    }

    [Serializable]
    public class SpriterRef : SpriterElement
    {
        [XmlAttribute("parent")]
        public int ParentId { get; set; }

        [XmlAttribute("timeline")]
        public int TimelineId { get; set; }

        [XmlAttribute("key")]
        public int KeyId { get; set; }

        public SpriterRef()
        {
            ParentId = -1;
        }
    }

    [Serializable]
    public class SpriterObjectRef : SpriterRef
    {
        [XmlAttribute("z_index")]
        public int ZIndex { get; set; }
    }

    [Serializable]
    public class SpriterTimeLine : SpriterElement
    {
        [XmlAttribute("type")]
        public SpriterObjectType ObjectType { get; set; }

        [XmlElement("key")]
        public SpriterTimeLineKey[] Keys { get; set; }
    }

    [Serializable]
    public class SpriterTimeLineKey : SpriterKey
    {
        [XmlAttribute("spin")]
        public int Spin { get; set; }

        [XmlElement("bone", typeof(SpriterSpatialInfo))]
        public SpriterSpatialInfo BoneInfo { get; set; }

        [XmlElement("object", typeof(SpriterObjectInfo))]
        public SpriterObjectInfo ObjectInfo { get; set; }

        public SpriterTimeLineKey()
        {
            Spin = 1;
        }
    }

    [Serializable]
    public class SpriterSpatialInfo
    {
        [XmlAttribute("x")]
        public float X { get; set; }

        [XmlAttribute("y")]
        public float Y { get; set; }

        [XmlAttribute("angle")]
        public float Angle { get; set; }

        [XmlAttribute("scale_x")]
        public float ScaleX { get; set; }

        [XmlAttribute("scale_y")]
        public float ScaleY { get; set; }

        [XmlAttribute("a")]
        public float Alpha { get; set; }

        public SpriterSpatialInfo()
        {
            ScaleX = 1;
            ScaleY = 1;
            Alpha = 1;
        }
    }

    [Serializable]
    public class SpriterObjectInfo : SpriterSpatialInfo
    {
        [XmlAttribute("folder")]
        public int FolderId { get; set; }

        [XmlAttribute("file")]
        public int FileId { get; set; }

        [XmlAttribute("pivot_x")]
        public float PivotX { get; set; }

        [XmlAttribute("pivot_y")]
        public float PivotY { get; set; }

        public SpriterObjectInfo()
        {
            PivotX = float.NaN;
            PivotY = float.NaN;
        }
    }

    [Serializable]
    public abstract class SpriterElement
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }

    public abstract class SpriterKey : SpriterElement
    {
        [XmlAttribute("time")]
        public float Time { get; set; }

        [XmlAttribute("curve_type")]
        public SpriterCurveType CurveType { get; set; }

        [XmlAttribute("c1")]
        public float C1 { get; set; }

        [XmlAttribute("c2")]
        public float C2 { get; set; }

        [XmlAttribute("c3")]
        public float C3 { get; set; }

        [XmlAttribute("c4")]
        public float C4 { get; set; }

        public SpriterKey()
        {
            Time = 0;
            CurveType = SpriterCurveType.Linear;
        }
    }

    [Serializable]
    public enum SpriterObjectType
    {
        [XmlEnum("sprite")]
        Sprite,

        [XmlEnum("bone")]
        Bone,

        [XmlEnum("box")]
        Box,

        [XmlEnum("point")]
        Point,

        [XmlEnum("sound")]
        Sound,

        [XmlEnum("entity")]
        Entity,

        [XmlEnum("variable")]
        Variable
    }

    [Serializable]
    public enum SpriterCurveType
    {
        [XmlEnum("instant")]
        Instant,

        [XmlEnum("linear")]
        Linear,

        [XmlEnum("quadratic")]
        Quadratic,

        [XmlEnum("cubic")]
        Cubic,

        [XmlEnum("quartic")]
        Quartic,

        [XmlEnum("quintic")]
        Quintic
    }
}