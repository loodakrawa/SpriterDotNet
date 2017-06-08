using TwistedLogik.Ultraviolet;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;

namespace SpriterDotNet.Ultraviolet
{
    public interface ISprite
    {
        float Width { get; }
        float Height { get; }
        void Draw(SpriteBatch spriteBatch, Vector2 pivot, Vector2 position, Vector2 scale, float rotation, Color color, float depth);
    }
}
