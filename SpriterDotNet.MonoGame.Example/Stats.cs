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
        private static readonly float Kb = 1.0f / 1024;
        private static readonly float Mb = Kb / 1024;
        private static readonly TimeSpan Second = TimeSpan.FromSeconds(1);

        public int FrameRate { get; private set; }

        public long Memory { get; private set; }
        public float MemoryKb { get { return Memory * Kb; } }
        public float MemoryMb { get { return Memory * Kb; } }

        public long FrameMalloc { get; private set; }
        public float FrameMallocKb { get { return FrameMalloc * Kb; } }
        public float FrameMallocMb { get { return FrameMalloc * Kb; } }

        public bool GcHappened { get; private set; }

        private int frameCount;
        private TimeSpan elapsedTime;

        public void OnUpdate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > Second)
            {
                elapsedTime -= Second;
                FrameRate = frameCount;
                frameCount = 0;

                long currentMem = GC.GetTotalMemory(false);
                FrameMalloc = currentMem - Memory;
                Memory = currentMem;

                GcHappened = FrameMalloc < 0;
            }
        }

        public void OnDraw()
        {
            ++frameCount;
        }
    }
}
