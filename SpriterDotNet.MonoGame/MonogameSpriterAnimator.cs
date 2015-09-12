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
