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
        [XmlAttribute("type")]
        public SpriterFileType Type;

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
        public Spriter Spriter;

        [XmlElement("obj_info")]
        public SpriterObjectInfo[] ObjectInfos;

        [XmlElement("character_map")]
        public SpriterCharacterMap[] CharacterMaps;

        [XmlElement("animation")]
        public SpriterAnimation[] Animations;

        [XmlArray("var_defs"), XmlArrayItem("i")]
        public SpriterVarDef[] Variables;
    }

    [Serializable]
    public class SpriterObjectInfo : SpriterElement
    {
        [XmlAttribute("type")]
        public SpriterObjectType ObjectType;

        [XmlAttribute("w")]
        public float Width;

        [XmlAttribute("h")]
        public float Height;

        [XmlAttribute("pivot_x")]
        public float PivotX;

        [XmlAttribute("pivot_y")]
        public float PivotY;

        [XmlArray("var_defs"), XmlArrayItem("i")]
        public SpriterVarDef[] Variables;
    }

    [Serializable]
    public class SpriterAnimation : SpriterElement
    {
        public SpriterEntity Entity;

        [XmlAttribute("length")]
        public float Length;

        [XmlAttribute("looping")]
        public bool Looping;

        [XmlArray("mainline"), XmlArrayItem("key")]
        public SpriterMainlineKey[] MainlineKeys;

        [XmlElement("timeline")]
        public SpriterTimeline[] Timelines;

        [XmlArray("meta"), XmlArrayItem("varline")]
        public SpriterVarline[] Varlines;

        public SpriterAnimation()
        {
            Looping = true;
        }
    }

    [Serializable]
    public class SpriterMainlineKey : SpriterKey
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
    public class SpriterTimeline : SpriterElement
    {
        [XmlAttribute("object_type")]
        public SpriterObjectType ObjectType;

        [XmlAttribute("obj")]
        public int ObjectId;

        [XmlElement("key")]
        public SpriterTimelineKey[] Keys;

        [XmlArray("meta"), XmlArrayItem("varline")]
        public SpriterVarline[] Varlines;
    }

    [Serializable]
    public class SpriterTimelineKey : SpriterKey
    {
        [XmlAttribute("spin")]
        public int Spin;

        [XmlElement("bone", typeof(SpriterSpatial))]
        public SpriterSpatial BoneInfo;

        [XmlElement("object", typeof(SpriterObject))]
        public SpriterObject ObjectInfo;

        public SpriterTimelineKey()
        {
            Spin = 1;
        }
    }

    [Serializable]
    public class SpriterSpatial
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

        public SpriterSpatial()
        {
            ScaleX = 1;
            ScaleY = 1;
            Alpha = 1;
        }
    }

    [Serializable]
    public class SpriterObject : SpriterSpatial
    {
        [XmlAttribute("animation")]
        public int AnimationId;

        [XmlAttribute("entity")]
        public int EntityId;

        [XmlAttribute("folder")]
        public int FolderId;

        [XmlAttribute("file")]
        public int FileId;

        [XmlAttribute("pivot_x")]
        public float PivotX;

        [XmlAttribute("pivot_y")]
        public float PivotY;

        [XmlAttribute("t")]
        public float T;

        public SpriterObject()
        {
            PivotX = float.NaN;
            PivotY = float.NaN;
        }
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
    public class SpriterVarDef : SpriterElement
    {
        [XmlAttribute("type")]
        public SpriterVarType Type;

        [XmlAttribute("default")]
        public string DefaultValue;

        [NonSerialized, XmlIgnore]
        public SpriterVarValue VariableValue;
    }

    [Serializable]
    public class SpriterVarline : SpriterElement
    {
        [XmlAttribute("def")]
        public int Def;

        [XmlElement("key")]
        public SpriterVarlineKey[] Keys;
    }

    public class SpriterVarlineKey : SpriterKey
    {
        [XmlAttribute("val")]
        public string Value;

        [NonSerialized, XmlIgnore]
        public SpriterVarValue VariableValue;
    }

    public struct SpriterVarValue
    {
        public SpriterVarType Type;
        public string StringValue;
        public float FloatValue;
        public int IntValue;
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
        [XmlEnum("linear")]
        Linear,

        [XmlEnum("instant")]
        Instant,

        [XmlEnum("quadratic")]
        Quadratic,

        [XmlEnum("cubic")]
        Cubic,

        [XmlEnum("quartic")]
        Quartic,

        [XmlEnum("quintic")]
        Quintic,

        [XmlEnum("bezier")]
        Bezier
    }

    [Serializable]
    public enum SpriterFileType
    {
        Image,

        [XmlEnum("sound")]
        Sound
    }

    [Serializable]
    public enum SpriterVarType
    {
        [XmlEnum("string")]
        String,

        [XmlEnum("int")]
        Int,

        [XmlEnum("float")]
        Float
    }
}