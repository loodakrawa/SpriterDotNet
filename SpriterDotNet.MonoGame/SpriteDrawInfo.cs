// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame
{
    /// <summary>
    /// Class for holding the draw info for a sprite.
    /// </summary>
    public class SpriteDrawInfo
    {
        public ISprite Drawable;
        public Vector2 Pivot;
        public Vector2 Position;
        public Vector2 Origin;
        public Vector2 Scale;
        public float Rotation;
        public Color Color;
        public float Depth;
    }
}
