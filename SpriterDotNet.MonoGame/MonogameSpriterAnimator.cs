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
        public readonly Dictionary<string, SpriterObject> PointInfo = new Dictionary<string, SpriterObject>();

        protected readonly Stack<DrawInfo> DrawInfoPool = new Stack<DrawInfo>();
        protected readonly List<DrawInfo> DrawInfos = new List<DrawInfo>();

        private const float DefaultLayerDelta = -0.000001f;

        private static readonly float ToRad = (float)(Math.PI / 180.0);
        private Matrix transform = Matrix.Identity;

        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }
        public Vector2 Position { get; set; }
        public float LayerDepth { get; set; }
        public float LayerDelta { get; set; }

        public MonogameSpriterAnimator(SpriterEntity entity, IProviderFactory<Texture2D, SoundEffect> providerFactory = null) : base(entity, providerFactory)
        {
            Scale = Vector2.One;
            LayerDelta = DefaultLayerDelta;
            LayerDepth = 0.5f;
        }

        public override void Step(float deltaTime)
        {
            DrawInfos.Clear();
            PointInfo.Clear();
            base.Step(deltaTime);
        }

        protected override void ApplySpriteTransform(Texture2D texture, SpriterObject info)
        {
            Vector2 origin = new Vector2(info.PivotX * texture.Width, (1 - info.PivotY) * texture.Height);
            Vector2 position = new Vector2(info.X, -info.Y);
            Vector2 scale = new Vector2(info.ScaleX, info.ScaleY);
            float rotation = -(float)(Math.PI / 180.0f) * info.Angle;
            Color color = Color.White * info.Alpha;
            SpriteEffects effects = SpriteEffects.None;

            if ((scale.X * Scale.X) < 0)
            {
                effects |= SpriteEffects.FlipHorizontally;
                origin = new Vector2(texture.Width - origin.X, origin.Y);
            }

            if ((scale.Y * Scale.Y) < 0)
            {
                effects |= SpriteEffects.FlipVertically;
                origin = new Vector2(origin.X, texture.Height - origin.Y);
            }

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

            Matrix globalTransform = GetMatrix(scale, rotation, position) * transform;
            DecomposeMatrix(ref globalTransform, out scale, out rotation, out position);

            DrawInfo di = DrawInfoPool.Count > 0 ? DrawInfoPool.Pop() : new DrawInfo();

            di.Texture = texture;
            di.Position = position;
            di.Origin = origin;
            di.Scale = scale;
            di.Rotation = rotation;
            di.Color = color;
            di.Effects = effects;

            DrawInfos.Add(di);
        }

        protected override void ApplyPointTransform(string name, SpriterObject info)
        {
            PointInfo[name] = info;
        }

        protected override void PlaySound(SoundEffect sound, SpriterSound info)
        {
            sound.Play(info.Volume, 0.0f, info.Panning);
        }

        public virtual void Draw(SpriteBatch batch)
        {
            transform = GetMatrix(Scale, Rotation * ToRad, Position);

            for (int i = 0; i < DrawInfos.Count; ++i)
            {
                DrawInfo di = DrawInfos[i];
                float depth = LayerDepth + LayerDelta * i;
                depth = (depth < 0) ? 0 : (depth > 1) ? 1 : depth;
                batch.Draw(di.Texture, di.Position, null, di.Color, di.Rotation, di.Origin, di.Scale, di.Effects, depth);
                DrawInfoPool.Push(di);
            }
        }

        private static Matrix GetMatrix(Vector2 scale, float rotation, Vector2 position)
        {
            return Matrix.CreateScale(Math.Abs(scale.X), Math.Abs(scale.Y), 1f) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0f);
        }

        private static void DecomposeMatrix(ref Matrix matrix, out Vector2 scale, out float rotation, out Vector2 position)
        {
            Vector3 position3, scale3;
            Quaternion rotationQ;
            matrix.Decompose(out scale3, out rotationQ, out position3);
            Vector2 direction = Vector2.Transform(Vector2.UnitX, rotationQ);
            rotation = (float)Math.Atan2(direction.Y, direction.X);
            position = new Vector2(position3.X, position3.Y);
            scale = new Vector2(scale3.X, scale3.Y);
        }

        protected class DrawInfo
        {
            public Texture2D Texture { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 Origin { get; set; }
            public Vector2 Scale { get; set; }
            public float Rotation { get; set; }
            public Color Color { get; set; }
            public SpriteEffects Effects { get; set; }
        }
    }
}
