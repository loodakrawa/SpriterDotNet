using Microsoft.Xna.Framework.Content;

namespace SpriterDotNet.MonoGame.Content
{
    public class SpriterTypeReader : ContentTypeReader<Spriter>
    {
        public static SpriterReader Reader = SpriterReader.Default;

        protected override Spriter Read(ContentReader input, Spriter existingInstance)
        {
            string data = input.ReadString();
            return Reader.Read(data);
        }
    }
}
