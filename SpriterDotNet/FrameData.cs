// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet
{
    public class FrameData
    {
        public List<SpriterObject> SpriteData { get; private set; }
        public List<SpriterObject> PointData { get; private set; }
        public IDictionary<int, SpriterObject> BoxData { get; private set; }

        public FrameData()
        {
            SpriteData = new List<SpriterObject>();
            PointData = new List<SpriterObject>();
            BoxData = new Dictionary<int, SpriterObject>();
        }
    }
}
