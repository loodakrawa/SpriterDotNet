// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    public struct Transform
    {
        public float X;
        public float Y;
        public float Angle;
        public float ScaleX;
        public float ScaleY;
        public float Alpha;
    }

    public struct ObjectData
    {
        public int AnimationId;
        public int EntityId;
        public int FolderId;
        public int FileId;
        public float PivotX;
        public float PivotY;
        public float T;
    }

    public struct BoneRef
    {
        public int ParentId;
        public int TransformIndex;
    }

    public struct ObjectRef
    {
        public int ParentId;
        public int ZIndex;
        public int TransformIndex;
        public int ObjectIndex;
    }
}