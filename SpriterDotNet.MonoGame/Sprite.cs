using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame
{
	public class Sprite
	{
		public Texture2D Texture { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public Rectangle? SourceRectangle { get; set; }
		public Vector2 OriginDelta { get; set; }
		public float Rotation { get; set; }
	}
}

