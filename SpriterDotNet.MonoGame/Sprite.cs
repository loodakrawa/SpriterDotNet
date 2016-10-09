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
		public float TrimLeft { get; set; }
		public float TrimRight { get; set; }
		public float TrimTop { get; set; }
		public float TrimBottom { get; set; }
		public bool Rotated { get; set; }
	}
}

