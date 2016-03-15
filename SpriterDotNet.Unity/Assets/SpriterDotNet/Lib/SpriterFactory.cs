using SpriterDotNet.AnimationDataProvider;
using System.Collections.Generic;

namespace SpriterDotNet
{
    public abstract class SpriterFactory<TSprite, TSound> where TSprite : class where TSound : class
    {
        private Dictionary<Spriter, Dictionary<TSprite, KeyValuePair<int, int>>> spriteMappings = new Dictionary<Spriter, Dictionary<TSprite, KeyValuePair<int, int>>>();

        public SpriterAnimator<TSprite, TSound> CreateAnimator(SpriterEntity entity)
        {
            IAnimationDataProvider animProvider = new DefaultAnimationDataProvider();
            //IAssetProvider<TSprite> spriteProvider = GetSpriteProvider(entity.Spriter);
            //IAssetProvider<TSound> soundProvider = GetSoundProvider(entity.Spriter);

            //return new SpriterAni
            return null;
        }

        //protected virtual IAssetProvider<TSprite> GetSpriteProvider(Spriter spriter)
        //{
        //    Dictionary<TSprite, KeyValuePair<int, int>> mappings;
        //}

        //protected virtual IAssetProvider<TSound> GetSoundProvider(Spriter spriter)
        //{

        //}
    }
}
