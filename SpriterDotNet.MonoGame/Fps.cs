/**
The MIT License (MIT)

Copyright (c) 2015 Luka "loodakrawa" Sverko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
**/

using Microsoft.Xna.Framework;

namespace SpriterDotNet.MonoGame
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
