// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SpriterDotNet.MonoGame
{
    /// <summary>
    /// MonoGame Animator implementation. It has separate Draw and Update steps. 
    /// During the Update step all spatial infos are calculated (translated from Spriter values) and the Draw step only draws the calculated values.
    /// </summary>
	public class MonoGameAnimator : Animator<Sprite, SoundEffect>
    {
        /// <summary>
        /// Scale factor of the animator. Negative values flip the image.
        /// </summary>
        public virtual Vector2 Scale { get; set; } = Vector2.One;

        /// <summary>
        /// Rotation in radians.
        /// </summary>
        public virtual float Rotation { get; set; }

        /// <summary>
        /// Position in pixels.
        /// </summary>
        public virtual Vector2 Position { get; set; }

        /// <summary>
        /// The drawing depth. Should be in the [0,1] interval.
        /// </summary>
        public virtual float Depth { get; set; } = DefaultDepth;

        /// <summary>
        /// The depth distance between different sprites of the same animation.
        /// </summary>
        public virtual float DeltaDepth { get; set; } = DefaultDeltaDepth;

        /// <summary>
        /// The color used to render all the sprites.
        /// </summary>
        public virtual Color Color { get; set; } = Color.White;

        protected Stack<DrawInfo> DrawInfoPool { get; set; } = new Stack<DrawInfo>();
        protected List<DrawInfo> DrawInfos { get; set; } = new List<DrawInfo>();
        protected Matrix Transform { get; set; }

        private static readonly float SpriteAtlasRotation = -90 * MathHelper.DegToRad;
        private static readonly float DefaultDepth = 0.5f;
        private static readonly float DefaultDeltaDepth = -0.000001f;

        public MonoGameAnimator(SpriterEntity entity, IProviderFactory<Sprite, SoundEffect> providerFactory = null) : base(entity, providerFactory)
        {
        }

        /// <summary>
        /// Draws the animation with the given SpriteBatch.
        /// </summary>
        public virtual void Draw(SpriteBatch batch)
        {
            for (int i = 0; i < DrawInfos.Count; ++i)
            {
                DrawInfo di = DrawInfos[i];
                Sprite sprite = di.Sprite;
                batch.Draw(sprite.Texture, di.Position, sprite.SourceRectangle, di.Color, di.Rotation, di.Origin, di.Scale, di.Effects, di.Depth);
                DrawInfoPool.Push(di);
            }
        }

        public override void Update(float deltaTime)
        {
            DrawInfos.Clear();

            Transform = MathHelper.GetMatrix(Scale, Rotation, Position);

            base.Update(deltaTime);
        }

        protected override void ApplySpriteTransform(Sprite sprite, SpriterObject info)
        {
            bool rotated = sprite.Rotated;
            Vector2 position = new Vector2(info.X, -info.Y);
            Vector2 scale = new Vector2(info.ScaleX, info.ScaleY);
            float rotation = -info.Angle * MathHelper.DegToRad;

            bool flipX = (scale.X * Scale.X) < 0;
            bool flipY = (scale.Y * Scale.Y) < 0;

            SpriteEffects effects = SpriteEffects.None;
            if (flipX) effects |= !rotated ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically;
            if (flipY) effects |= !rotated ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;

            float originX;
            float originY;

            if (Scale.X < 0)
            {
                position = new Vector2(-position.X, position.Y);
                rotation = -rotation;
            }

            if (Scale.Y < 0)
            {
                position = new Vector2(position.X, -position.Y);
                rotation = -rotation;
            }

            if (!rotated)
            {
                if (!flipX) originX = info.PivotX * sprite.Width - sprite.TrimLeft;
                else originX = (1 - info.PivotX) * sprite.Width - sprite.TrimRight;

                if (!flipY) originY = (1 - info.PivotY) * sprite.Height - sprite.TrimTop;
                else originY = info.PivotY * sprite.Height - sprite.TrimBottom;
            }
            else
            {
                if (!flipX)
                {
                    originX = info.PivotY * sprite.Height - sprite.TrimBottom;
                    originY = info.PivotX * sprite.Width - sprite.TrimLeft;
                }
                else
                {
                    originX = (1 - info.PivotY) * sprite.Height - sprite.TrimTop;
                    originY = (1 - info.PivotX) * sprite.Width - sprite.TrimRight;
                }
            }

            Matrix globalTransform = MathHelper.GetMatrix(scale, rotation, position) * Transform;
            globalTransform.DecomposeMatrix(out scale, out rotation, out position);

            float depth = Depth + DeltaDepth * DrawInfos.Count;
            depth = (depth < 0) ? 0 : (depth > 1) ? 1 : depth;

            DrawInfo di = DrawInfoPool.Count > 0 ? DrawInfoPool.Pop() : new DrawInfo();

            di.Sprite = sprite;
            di.Position = position;
            di.Origin = new Vector2(originX, originY);
            di.Scale = scale;
            di.Rotation = rotation + (sprite.Rotated ? SpriteAtlasRotation : 0);
            di.Color = Color * info.Alpha;
            di.Effects = effects;
            di.Depth = depth;

            DrawInfos.Add(di);
        }

        protected override void PlaySound(SoundEffect sound, SpriterSound info)
        {
            sound.Play(info.Volume, 0.0f, info.Panning);
        }

        /// <summary>
        /// Class for holding the draw info for a sprite.
        /// </summary>
        protected class DrawInfo
        {
            public Sprite Sprite { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Origin { get; set; }
            public Vector2 Scale { get; set; }
            public float Rotation { get; set; }
            public Color Color { get; set; }
            public SpriteEffects Effects { get; set; }
            public float Depth { get; set; }
        }
    }
}
