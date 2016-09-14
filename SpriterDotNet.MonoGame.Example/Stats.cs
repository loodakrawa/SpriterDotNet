// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using System;

namespace SpriterDotNet.MonoGame.Example
{
    public class Stats
    {
        private static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

        public int FrameRate { get; private set; }
		public long Memory { get; private set; }

        private int frameCount;
        private TimeSpan elapsedTime;

        public void OnUpdate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > Second)
            {
                elapsedTime -= Second;
                FrameRate = frameCount;
				Memory = GC.GetTotalMemory(false);
                frameCount = 0;
            }
        }

        public void OnDraw(GameTime gameTime)
        {
            ++frameCount;
        }
    }
}
