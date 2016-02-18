// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame.Desktop
{
    public class Fps
    {
        public int UpdateFps { get; private set; }
        public int DrawFps { get; private set; }

        private int elapsedSeconds;
        private bool updateFps;

        public void OnUpdate(GameTime gameTime)
        {
            int seconds = gameTime.TotalGameTime.Seconds;
            if (seconds > elapsedSeconds)
            {
                int elapsed = gameTime.ElapsedGameTime.Milliseconds;
                if (elapsed == 0) ++elapsed;
                UpdateFps = 1000 / elapsed;
                elapsedSeconds = seconds;
                updateFps = true;
            }
        }

        public void OnDraw(GameTime gameTime)
        {
            if (updateFps)
            {
                updateFps = true;
                int elapsed = gameTime.ElapsedGameTime.Milliseconds;
                if (elapsed == 0) ++elapsed;
                DrawFps = 1000 / elapsed;
                updateFps = false;
            }
        }
    }
}
