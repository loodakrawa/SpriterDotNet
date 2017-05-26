using SpriterDotNet.MonoGame.Example;
using System;

namespace DesktopGL
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (SpriterGame game = new SpriterGame())
            {
                game.Run();
            }
        }
    }
}
