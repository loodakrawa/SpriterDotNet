namespace SpriterDotNet
{
    public interface IProviderFactory<TSprite, TSound>
    {
        IAssetProvider<TSprite> GetSpriteProvider(SpriterEntity spriter);
        IAssetProvider<TSound> GetSoundProvider(SpriterEntity spriter);
        IAnimationDataProvider GetDataProvider(SpriterEntity spriter);
    }
}
