// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the MIT license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SpriterDotNet.MonoGame
{
    public class MonogameSpriterAnimator : SpriterAnimator<Texture2D>
    {
        private Vector2 charLocation;
        private SpriteBatch spriteBatch;

        public MonogameSpriterAnimator(SpriterEntity entity, Vector2 charLocation, SpriteBatch spriteBatch) : base(entity)
        {
            this.charLocation = charLocation;
            this.spriteBatch = spriteBatch;
        }

        protected override void ApplyTransform(Texture2D texture, SpriterObjectInfo info)
        {
            Vector2 origin = new Vector2(info.PivotX * texture.Width, (1 - info.PivotY) * texture.Height);
            Vector2 location = charLocation + new Vector2(info.X, -info.Y);
            Vector2 scale = new Vector2(Math.Abs(info.ScaleX), Math.Abs(info.ScaleY));
            float angle = -(float)(Math.PI / 180.0f) * info.Angle;
            Color color = Color.White * info.Alpha;
            SpriteEffects effects = SpriteEffects.None;

            if (info.ScaleX < 0)
            {
                effects |= SpriteEffects.FlipHorizontally;
                origin = new Vector2(texture.Width - origin.X, origin.Y);
            }

            if (info.ScaleY < 0)
            {
                effects |= SpriteEffects.FlipVertically;
                origin = new Vector2(origin.X, texture.Height - origin.Y);
            }

            spriteBatch.Draw(texture, location, null, color, angle, origin, scale, effects, 1);
        }
    }
}
