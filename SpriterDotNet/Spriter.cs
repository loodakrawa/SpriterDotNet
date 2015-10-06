// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Xml.Serialization;

namespace SpriterDotNet
{
    [Serializable, XmlRoot("spriter_data")]
    public class Spriter
    {
        [XmlElement("folder")]
        public SpriterFolder[] Folders;

        [XmlElement("entity")]
        public SpriterEntity[] Entities;
    }

    [Serializable]
    public class SpriterFolder : SpriterElement
    {
        [XmlElement("file")]
        public SpriterFile[] Files;
    }

    [Serializable]
    public class SpriterFile : SpriterElement
    {
        [XmlAttribute("pivot_x")]
        public float PivotX;

        [XmlAttribute("pivot_y")]
        public float PivotY;

        [XmlAttribute("width")]
        public int Width;

        [XmlAttribute("height")]
        public int Height;

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
        public SpriterCharacterMap[] CharacterMaps;

        [XmlElement("animation")]
        public SpriterAnimation[] Animations;
    }

    [Serializable]
    public class SpriterCharacterMap : SpriterElement
    {
        [XmlElement("map")]
        public SpriterMapInstruction[] Maps;
    }

    [Serializable]
    public class SpriterMapInstruction
    {
        [XmlAttribute("folder")]
        public int FolderId;

        [XmlAttribute("file")]
        public int FileId;

        [XmlAttribute("target_folder")]
        public int TargetFolderId;

        [XmlAttribute("target_file")]
        public int TargetFileId;

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
        public float Length;

        [XmlAttribute("looping")]
        public bool Looping;

        [XmlArray("mainline"), XmlArrayItem("key")]
        public SpriterMainLineKey[] MainlineKeys;

        [XmlElement("timeline")]
        public SpriterTimeLine[] Timelines;

        public SpriterAnimation()
        {
            Looping = true;
        }
    }

    [Serializable]
    public class SpriterMainLineKey : SpriterKey
    {
        [XmlElement("bone_ref")]
        public SpriterRef[] BoneRefs;

        [XmlElement("object_ref")]
        public SpriterObjectRef[] ObjectRefs;
    }

    [Serializable]
    public class SpriterRef : SpriterElement
    {
        [XmlAttribute("parent")]
        public int ParentId;

        [XmlAttribute("timeline")]
        public int TimelineId;

        [XmlAttribute("key")]
        public int KeyId;

        public SpriterRef()
        {
            ParentId = -1;
        }
    }

    [Serializable]
    public class SpriterObjectRef : SpriterRef
    {
        [XmlAttribute("z_index")]
        public int ZIndex;
    }

    [Serializable]
    public class SpriterTimeLine : SpriterElement
    {
        [XmlAttribute("object_type")]
        public SpriterObjectType ObjectType;

        [XmlElement("key")]
        public SpriterTimeLineKey[] Keys;
    }

    [Serializable]
    public class SpriterTimeLineKey : SpriterKey
    {
        [XmlAttribute("spin")]
        public int Spin;

        [XmlElement("bone", typeof(SpriterSpatialInfo))]
        public SpriterSpatialInfo BoneInfo;

        [XmlElement("object", typeof(SpriterObjectInfo))]
        public SpriterObjectInfo ObjectInfo;

        public SpriterTimeLineKey()
        {
            Spin = 1;
        }
    }

    [Serializable]
    public class SpriterSpatialInfo
    {
        [XmlAttribute("x")]
        public float X;

        [XmlAttribute("y")]
        public float Y;

        [XmlAttribute("angle")]
        public float Angle;

        [XmlAttribute("scale_x")]
        public float ScaleX;

        [XmlAttribute("scale_y")]
        public float ScaleY;

        [XmlAttribute("a")]
        public float Alpha;

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
        public int FolderId;

        [XmlAttribute("file")]
        public int FileId;

        [XmlAttribute("pivot_x")]
        public float PivotX;

        [XmlAttribute("pivot_y")]
        public float PivotY;

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
        public int Id;

        [XmlAttribute("name")]
        public string Name;
    }

    [Serializable]
    public abstract class SpriterKey : SpriterElement
    {
        [XmlAttribute("time")]
        public float Time;

        [XmlAttribute("curve_type")]
        public SpriterCurveType CurveType;

        [XmlAttribute("c1")]
        public float C1;

        [XmlAttribute("c2")]
        public float C2;

        [XmlAttribute("c3")]
        public float C3;

        [XmlAttribute("c4")]
        public float C4;

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