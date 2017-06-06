// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SpriterDotNet.MonoGame.Sprites
{
    /// <summary>
    /// A drawable wrapper for a texture region created with TexturePacker
    /// </summary>
	public class TexturePackerSprite : ISprite
    {
        public float Width => width;
        public float Height => height;

        private static readonly float SpriteAtlasRotation = MathHelper.ToRadians(-90);

        public Texture2D texture;
        public Rectangle sourceRectangle;
        public int width;
        public int height;
        public bool rotated;
        public float trimLeft;
        public float trimRight;
        public float trimTop;
        public float trimBottom;

        public TexturePackerSprite(Texture2D texture, Rectangle sourceRectangle, int width, int height, bool rotated, float trimLeft, float trimRight, float trimTop, float trimBottom)
        {
            this.texture = texture;
            this.sourceRectangle = sourceRectangle;
            this.width = width;
            this.height = height;
            this.rotated = rotated;
            this.trimLeft = trimLeft;
            this.trimRight = trimRight;
            this.trimTop = trimTop;
            this.trimBottom = trimBottom;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pivot, Vector2 position, Vector2 scale, float rotation, Color color, float depth)
        {
            bool flipX = scale.X < 0;
            bool flipY = scale.Y < 0;

            if (rotated) scale = new Vector2(scale.Y, scale.X);

            scale = new Vector2(Math.Abs(scale.X), Math.Abs(scale.Y));

            SpriteEffects effects = SpriteEffects.None;
            if (flipX) effects |= !rotated ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically;
            if (flipY) effects |= !rotated ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;

            float x = pivot.X * width - trimLeft;
            float fx = (1 - pivot.X) * width - trimRight;
            float y = pivot.Y * height - trimTop;
            float fy = (1 - pivot.Y) * height - trimBottom;

            float ox;
            float oy;

            if (rotated)
            {
                ox = flipY ? y : fy;
                oy = flipX ? fx : x;
            }
            else
            {
                ox = flipX ? fx : x;
                oy = flipY ? fy : y;
            }

            Vector2 origin = new Vector2(ox, oy);

            if (rotated) rotation += SpriteAtlasRotation;

            spriteBatch.Draw
            (
                texture: texture,
                sourceRectangle: sourceRectangle,
                origin: origin,
                position: position,
                scale: scale,
                rotation: rotation,
                color: color,
                layerDepth: depth,
                effects: effects
            );
        }
    }
}

