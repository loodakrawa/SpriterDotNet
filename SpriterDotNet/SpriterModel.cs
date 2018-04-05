// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Xml.Serialization;

namespace SpriterDotNet
{
    [XmlRoot("spriter_data")]
    public class Spriter
    {
        [XmlElement("folder")]
        public SpriterFolder[] Folders;

        [XmlElement("entity")]
        public SpriterEntity[] Entities;

        [XmlArray("tag_list"), XmlArrayItem("i")]
        public SpriterElement[] Tags;

        [XmlArray("atlas"), XmlArrayItem("i")]
        public SpriterElement[] Atlases;
    }

    public class SpriterFolder : SpriterElement
    {
        [XmlElement("file")]
        public SpriterFile[] Files;

        [XmlAttribute("atlas")]
        public int AtlasId;

        public SpriterFolder()
        {
            AtlasId = -1;
        }
    }

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

        [XmlElement("eventline")]
        public SpriterEventline[] Eventlines;

        [XmlElement("soundline")]
        public SpriterSoundline[] Soundlines;

        [XmlElement("meta")]
        public SpriterMeta Meta;

        public SpriterAnimation()
        {
            Looping = true;
        }
    }

    public class SpriterMainlineKey : SpriterKey
    {
        [XmlElement("bone_ref")]
        public SpriterRef[] BoneRefs;

        [XmlElement("object_ref")]
        public SpriterObjectRef[] ObjectRefs;
    }

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

    public class SpriterObjectRef : SpriterRef
    {
        [XmlAttribute("z_index")]
        public int ZIndex;
    }

    public class SpriterTimeline : SpriterElement
    {
        [XmlAttribute("object_type")]
        public SpriterObjectType ObjectType;

        [XmlAttribute("obj")]
        public int ObjectId;

        [XmlElement("key")]
        public SpriterTimelineKey[] Keys;

        [XmlElement("meta")]
        public SpriterMeta Meta;
    }

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

    public class SpriterCharacterMap : SpriterElement
    {
        [XmlElement("map")]
        public SpriterMapInstruction[] Maps;
    }

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

    public class SpriterMeta
    {
        [XmlElement("varline")]
        public SpriterVarline[] Varlines;

        [XmlElement("tagline")]
        public SpriterTagline Tagline;
    }

    public class SpriterVarDef : SpriterElement
    {
        [XmlAttribute("type")]
        public SpriterVarType Type;

        [XmlAttribute("default")]
        public string DefaultValue;

        [XmlIgnore]
        public SpriterVarValue VariableValue;
    }

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

        [XmlIgnore]
        public SpriterVarValue VariableValue;
    }

    public class SpriterVarValue
    {
        public SpriterVarType Type;
        public string StringValue;
        public float FloatValue;
        public int IntValue;
    }

    public class SpriterEventline : SpriterElement
    {
        [XmlElement("key")]
        public SpriterKey[] Keys;
    }

    public class SpriterTagline
    {
        [XmlElement("key")]
        public SpriterTaglineKey[] Keys;
    }

    public class SpriterTaglineKey : SpriterKey
    {
        [XmlElement("tag")]
        public SpriterTag[] Tags;
    }

    public class SpriterTag : SpriterElement
    {
        [XmlAttribute("t")]
        public int TagId;
    }

    public class SpriterSoundline : SpriterElement
    {
        [XmlElement("key")]
        public SpriterSoundlineKey[] Keys;
    }

    public class SpriterSoundlineKey : SpriterKey
    {
        [XmlElement("object")]
        public SpriterSound SoundObject;
    }

    public class SpriterSound : SpriterElement
    {
        [XmlAttribute("folder")]
        public int FolderId;

        [XmlAttribute("file")]
        public int FileId;

        [XmlAttribute("trigger")]
        public bool Trigger;

        [XmlAttribute("panning")]
        public float Panning;

        [XmlAttribute("volume")]
        public float Volume;

        public SpriterSound()
        {
            Trigger = true;
            Volume = 1.0f;
        }
    }

    public class SpriterElement
    {
        [XmlAttribute("id")]
        public int Id;

        [XmlAttribute("name")]
        public string Name;
    }

    public class SpriterKey : SpriterElement
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

    public enum SpriterFileType
    {
        Image,

        [XmlEnum("sound")]
        Sound
    }

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