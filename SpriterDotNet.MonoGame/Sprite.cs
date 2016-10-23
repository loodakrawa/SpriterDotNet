// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame
{
    /// <summary>
    /// A wrapper around Texture2D to cater for sprite atlas metadata.
    /// </summary>
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

