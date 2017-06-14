// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;

namespace SpriterDotNet.Example.MonoGame.DesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (ExampleGame game = new ExampleGame())
            {
                game.Run();
            }
        }
    }
}
