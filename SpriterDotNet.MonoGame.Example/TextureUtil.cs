// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SpriterDotNet.MonoGame.Example
{
    public static class TextureUtil
    {
        public static Texture2D CreateCircle(GraphicsDevice graphics, int radius, Color color)
        {
            int diameter = radius * 2;
            Vector2 center = new Vector2(radius, radius);

            Texture2D circle = new Texture2D(graphics, diameter, diameter);
            Color[] colors = new Color[diameter * diameter];

            for (int i = 0; i < colors.Length; i++)
            {
                int x = (i + 1) % diameter;
                int y = (i + 1) / diameter;
                Vector2 distance = new Vector2(Math.Abs(center.X - x), Math.Abs(center.Y - y));
                colors[i] = distance.Length() > radius ? Color.Transparent : color;
            }

            circle.SetData<Color>(colors);
            return circle;
        }

        public static Texture2D CreateRectangle(GraphicsDevice graphics, int width, int height, Color color)
        {
            Texture2D rect = new Texture2D(graphics, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            rect.SetData(data);

            return rect;
        }
    }
}
