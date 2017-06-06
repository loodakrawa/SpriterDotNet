// Copyright (C) The original author or authors
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
    /// <summary>
    /// MonoGame Animator implementation. It has separate Draw and Update steps. 
    /// During the Update step all spatial infos are calculated (translated from Spriter values) and the Draw step only draws the calculated values.
    /// </summary>
	public class MonoGameAnimator : Animator<ISprite, SoundEffect>
    {
        /// <summary>
        /// Scale factor of the animator. Negative values flip the image.
        /// </summary>
        public virtual Vector2 Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                scaleAbs = new Vector2(Math.Abs(value.X), Math.Abs(value.Y));
            }
        }

        /// <summary>
        /// Rotation in radians.
        /// </summary>
        public virtual float Rotation
        {
            get { return rotation; }
            set
            {
                rotation = value;
                rotationSin = (float)Math.Sin(Rotation);
                rotationCos = (float)Math.Cos(Rotation);
            }
        }

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

        protected Stack<SpriteDrawInfo> DrawInfoPool { get; set; }
        protected List<SpriteDrawInfo> DrawInfos { get; set; } = new List<SpriteDrawInfo>();

        private static readonly float DefaultDepth = 0.5f;
        private static readonly float DefaultDeltaDepth = -0.000001f;

        private float rotation;
        private float rotationSin;
        private float rotationCos;

        private Vector2 scale;
        private Vector2 scaleAbs;

        public MonoGameAnimator
        (
            SpriterEntity entity,
            IProviderFactory<ISprite, SoundEffect> providerFactory = null,
            Stack<SpriteDrawInfo> drawInfoPool = null
        ) : base(entity, providerFactory)
        {
            Scale = Vector2.One;
            Rotation = 0;
            DrawInfoPool = drawInfoPool ?? new Stack<SpriteDrawInfo>();
        }

        /// <summary>
        /// Draws the animation with the given SpriteBatch.
        /// </summary>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < DrawInfos.Count; ++i)
            {
                SpriteDrawInfo di = DrawInfos[i];
                ISprite sprite = di.Drawable;
                sprite.Draw(spriteBatch, di.Pivot, di.Position, di.Scale, di.Rotation, di.Color, di.Depth);
            }
        }

        public override void Update(float deltaTime)
        {
            for (int i = 0; i < DrawInfos.Count; ++i) DrawInfoPool.Push(DrawInfos[i]);
            DrawInfos.Clear();

            base.Update(deltaTime);
        }

        protected override void ApplySpriteTransform(ISprite drawable, SpriterObject info)
        {
            float posX, posY, rotation;
            GetPositionAndRotation(info, out posX, out posY, out rotation);

            SpriteDrawInfo di = DrawInfoPool.Count > 0 ? DrawInfoPool.Pop() : new SpriteDrawInfo();

            di.Drawable = drawable;
            di.Pivot = new Vector2(info.PivotX, (1 - info.PivotY));
            di.Position = new Vector2(posX, posY);
            di.Scale = new Vector2(info.ScaleX, info.ScaleY) * Scale;
            di.Rotation = rotation;
            di.Color = Color * info.Alpha;
            di.Depth = Depth + DeltaDepth * DrawInfos.Count;

            DrawInfos.Add(di);
        }

        protected override void PlaySound(SoundEffect sound, SpriterSound info)
        {
            sound.Play(info.Volume, 0.0f, info.Panning);
        }

        public Box GetBoundingBox(SpriterObject info, float width, float height)
        {
            float posX, posY, rotation;
            GetPositionAndRotation(info, out posX, out posY, out rotation);

            float w = width * info.ScaleX * Scale.X;
            float h = height * info.ScaleY * Scale.Y;

            float rs = (float)Math.Sin(rotation);
            float rc = (float)Math.Cos(rotation);

            Vector2 originDelta = Rotate(new Vector2(-info.PivotX * w, -(1 - info.PivotY) * h), rs, rc);

            Box cb = new Box();
            Vector2 horizontal = Rotate(new Vector2(w, 0), rs, rc);
            cb.Point1 = new Vector2(posX, posY) + originDelta;
            cb.Point2 = cb.Point1 + horizontal;
            cb.Point4 = cb.Point1 + Rotate(new Vector2(0, h), rs, rc);
            cb.Point3 = cb.Point4 + horizontal;

            return cb;
        }

        public Vector2 GetPosition(SpriterObject info)
        {
            float posX, posY, rotation;
            GetPositionAndRotation(info, out posX, out posY, out rotation);
            return new Vector2(posX, posY);
        }

        private void GetPositionAndRotation(SpriterObject info, out float posX, out float posY, out float rotation)
        {
            float px = info.X;
            float py = -info.Y;
            rotation = MathHelper.ToRadians(-info.Angle);

            if (Scale.X < 0)
            {
                px = -px;
                rotation = -rotation;
            }

            if (Scale.Y < 0)
            {
                py = -py;
                rotation = -rotation;
            }

            px *= scaleAbs.X;
            py *= scaleAbs.Y;

            rotation += Rotation;

            posX = px * rotationCos - py * rotationSin + Position.X;
            posY = px * rotationSin + py * rotationCos + Position.Y;
        }

        private static Vector2 Rotate(Vector2 v, float s, float c)
        {
            return new Vector2(v.X * c - v.Y * s, v.X * s + v.Y * c);
        }
    }
}
