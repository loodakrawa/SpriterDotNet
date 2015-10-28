// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SpriterDotNet.MonoGame
{
    public class MonogameSpriterAnimator : SpriterAnimator<Texture2D, SoundEffect>
    {
        private Vector2 charLocation;
        private SpriteBatch spriteBatch;
        private Texture2D pointTexture;
        private IDictionary<string, Texture2D> boxTextures = new Dictionary<string, Texture2D>();

        public MonogameSpriterAnimator(SpriterEntity entity, Vector2 charLocation, SpriteBatch spriteBatch, GraphicsDevice graphics) : base(entity)
        {
            this.charLocation = charLocation;
            this.spriteBatch = spriteBatch;

            pointTexture = TextureUtil.CreateCircle(graphics, 5, Color.Cyan);
            if(entity.ObjectInfos != null)
            {
                foreach (SpriterObjectInfo objInfo in entity.ObjectInfos)
                {
                    if (objInfo.ObjectType != SpriterObjectType.Box) continue;
                    boxTextures[objInfo.Name] = TextureUtil.CreateRectangle(graphics, (int)objInfo.Width, (int)objInfo.Height, Color.Cyan);
                }
            }
        }

        protected override void ApplySpriteTransform(Texture2D texture, SpriterObject info)
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

        protected override void PlaySound(SoundEffect sound, SpriterSound info)
        {
            sound.Play(info.Volume, 0.0f, info.Panning);
        }

        protected override void ApplyPointTransform(string name, SpriterObject info)
        {
            if (pointTexture == null) return;
            ApplySpriteTransform(pointTexture, info);
        }

        protected override void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
            ApplySpriteTransform(boxTextures[objInfo.Name], info);
        }
    }
}
