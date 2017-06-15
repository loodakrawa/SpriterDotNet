// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using SpriterDotNet.MonoGame;
using System;
using System.Collections.Generic;

namespace SpriterDotNet.Example.MonoGame
{
    public class MonoGameDebugAnimator : MonoGameAnimator
    {
        public bool DrawSpriteOutlines { get; set; }
        public Color DebugColor { get; set; } = Color.Red;

        private readonly List<KeyValuePair<Vector2, Vector2>> lines = new List<KeyValuePair<Vector2, Vector2>>();
        private readonly Texture2D whiteDot;

        private static readonly float PointBoxSize = 2.5f;

        public MonoGameDebugAnimator
        (
            SpriterEntity entity, 
            GraphicsDevice graphicsDevice, 
            IProviderFactory<ISprite, SoundEffect> providerFactory = null,
            Stack<SpriteDrawInfo> drawInfoPool = null
        ) : base(entity, providerFactory, drawInfoPool)
        {
            whiteDot = CreateTexture(graphicsDevice, 1, 1, Color.White);
        }

        protected override void ApplySpriteTransform(ISprite drawable, SpriterObject info)
        {
            base.ApplySpriteTransform(drawable, info);
            if (DrawSpriteOutlines) AddForDrawing(GetBoundingBox(info, drawable.Width, drawable.Height));
        }

        protected override void ApplyPointTransform(string name, SpriterObject info)
        {
            Vector2 position = GetPosition(info);
            Box box = new Box
            {
                Point1 = position + new Vector2(-PointBoxSize, -PointBoxSize),
                Point2 = position + new Vector2(PointBoxSize, -PointBoxSize),
                Point3 = position + new Vector2(PointBoxSize, PointBoxSize),
                Point4 = position + new Vector2(-PointBoxSize, PointBoxSize)
            };
            AddForDrawing(box);
        }

        protected override void ApplyBoxTransform(SpriterObjectInfo objInfo, SpriterObject info)
        {
            Box bounds = GetBoundingBox(info, objInfo.Width, objInfo.Height);
            AddForDrawing(bounds);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (var pair in lines) DrawLine(spriteBatch, pair.Key, pair.Value);
            lines.Clear();
        }

        public void AddForDrawing(Box cb)
        {
            AddForDrawing(cb.Point1, cb.Point2);
            AddForDrawing(cb.Point2, cb.Point3);
            AddForDrawing(cb.Point3, cb.Point4);
            AddForDrawing(cb.Point4, cb.Point1);
        }

        public void AddForDrawing(Vector2 v1, Vector2 v2)
        {
            lines.Add(new KeyValuePair<Vector2, Vector2>(v1, v2));
        }

        private void DrawLine(SpriteBatch batch, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);

            Rectangle rec = new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1);

            batch.Draw(whiteDot, rec, null, DebugColor, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        private Texture2D CreateTexture(GraphicsDevice graphics, int width, int height, Color color)
        {
            Texture2D rect = new Texture2D(graphics, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            rect.SetData(data);

            return rect;
        }
    }
}
