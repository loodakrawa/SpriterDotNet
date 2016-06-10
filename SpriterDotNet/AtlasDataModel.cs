// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet
{
	public class SpriterAtlas
	{
		public List<ImageInfo> ImageInfos { get; set; }
		public Meta Meta { get; set; }
	}

	public class Meta
	{
		public string App { get; set; }
		public string Format { get; set; }
		public string Image { get; set; }
		public float Scale { get; set; }
		public Size Size { get; set; }
		public string Version { get; set; }
	}

	public class ImageInfo
	{
		public string Name { get; set; }
		public bool Rotated { get; set; }
		public bool Trimmed { get; set; }
		public Size Frame { get; set; }
		public Size SourceSize { get; set; }
		public Size SpriteSourceSize { get; set; }
	}

	public class Size
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int W { get; set; }
		public int H { get; set; }
	}
}