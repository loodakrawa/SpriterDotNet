// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpriterDotNet.MonoGame.Helpers;
using System;
using System.Collections.Generic;

namespace SpriterDotNet.MonoGame
{
    /// <summary>
    /// MonoGame Animator implementation. It has separate Draw and Update steps. 
    /// During the Update step all spatial infos are calculated (translated from Spriter values) and the Draw step only draws the calculated values.
    /// </summary>
	public class MonoGameAnimator : Animator<ISprite, SoundEffect>
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

        private static readonly float DefaultDepth = 0.5f;
        private static readonly float DefaultDeltaDepth = -0.000001f;

        public MonoGameAnimator(SpriterEntity entity, IProviderFactory<ISprite, SoundEffect> providerFactory = null) : base(entity, providerFactory)
        {
        }

        /// <summary>
        /// Draws the animation with the given SpriteBatch.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < DrawInfos.Count; ++i)
            {
                DrawInfo di = DrawInfos[i];
                ISprite sprite = di.Drawable;
                sprite.Draw(spriteBatch, di.Pivot, di.Position, di.Scale, di.Rotation, di.Color, di.Depth);
                DrawInfoPool.Push(di);
            }
        }

        public override void Update(float deltaTime)
        {
            DrawInfos.Clear();

            Transform = MathUtil.GetMatrix(Scale, Rotation, Position);

            base.Update(deltaTime);
        }

        protected override void ApplySpriteTransform(ISprite drawable, SpriterObject info)
        {
            Vector2 position = new Vector2(info.X, -info.Y);
            Vector2 scale = new Vector2(info.ScaleX, info.ScaleY);
            float rotation = -info.Angle * MathUtil.DegToRad;
            Color color = Color.White * info.Alpha;

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

            int signX = Math.Sign(Scale.X * scale.X);
            int signY = Math.Sign(Scale.Y * scale.Y);

            Matrix globalTransform = MathUtil.GetMatrix(scale, rotation, position) * Transform;
            globalTransform.DecomposeMatrix(out scale, out rotation, out position);

            scale = new Vector2(Math.Abs(scale.X) * signX, Math.Abs(scale.Y) * signY);

            float depth = Depth + DeltaDepth * DrawInfos.Count;
            depth = (depth < 0) ? 0 : (depth > 1) ? 1 : depth;

            DrawInfo di = DrawInfoPool.Count > 0 ? DrawInfoPool.Pop() : new DrawInfo();

            di.Pivot = new Vector2(info.PivotX, (1 - info.PivotY));
            di.Drawable = drawable;
            di.Position = position;
            di.Scale = scale;
            di.Rotation = rotation;
            di.Color = Color * info.Alpha;
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
}
