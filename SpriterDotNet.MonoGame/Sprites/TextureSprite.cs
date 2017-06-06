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
    /// A drawable wrapper for a Texture2D
    /// </summary>
    public class TextureSprite : ISprite
    {
        public float Width => texture.Width;
        public float Height => texture.Height;

        private readonly Texture2D texture;

        public TextureSprite(Texture2D texture)
        {
            this.texture = texture;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pivot, Vector2 position, Vector2 scale, float rotation, Color color, float depth)
        {
            SpriteEffects effects = SpriteEffects.None;

            float originX = pivot.X * texture.Width;
            float originY = pivot.Y * texture.Height;

            if (scale.X < 0)
            {
                effects |= SpriteEffects.FlipHorizontally;
                originX = texture.Width - originX;
            }

            if (scale.Y < 0)
            {
                effects |= SpriteEffects.FlipVertically;
                originY = texture.Height - originY;
            }

            scale = new Vector2(Math.Abs(scale.X), Math.Abs(scale.Y));
            Vector2 origin = new Vector2(originX, originY);

            spriteBatch.Draw(texture, position, null, color, rotation, origin, scale, effects, depth);
        }
    }
}
