using Microsoft.Xna.Framework.Content;

namespace SpriterDotNet.MonoGame.Content
{
    public class SpriterTypeReader : ContentTypeReader<Spriter>
    {
        protected override Spriter Read(ContentReader input, Spriter existingInstance)
        {
            string data = input.ReadString();
            return SpriterParser.Parse(data);
        }
    }
}
