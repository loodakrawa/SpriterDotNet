// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System.Collections.Generic;

namespace SpriterDotNet.Providers
{
    public class DefaultProviderFactory<TSprite, TSound> : IProviderFactory<TSprite, TSound>
    {
        private Dictionary<SpriterEntity, SnapshotAnimationDataProvider> animProviders = new Dictionary<SpriterEntity, SnapshotAnimationDataProvider>();
        private Dictionary<Spriter, DefaultAssetProvider<TSprite>> spriteProviders = new Dictionary<Spriter, DefaultAssetProvider<TSprite>>();
        private Dictionary<Spriter, DefaultAssetProvider<TSound>> soundProviders = new Dictionary<Spriter, DefaultAssetProvider<TSound>>();

        private bool cacheAnimations;
        private int interval;

        public DefaultProviderFactory(bool cacheAnimations = true, int interval = 20)
        {
            this.cacheAnimations = cacheAnimations;
            this.interval = interval;
        }

        public virtual IAnimationDataProvider GetDataProvider(SpriterEntity entity)
        {
            if (!cacheAnimations) return new DefaultAnimationDataProvider();
            SnapshotAnimationDataProvider provider;
            animProviders.TryGetValue(entity, out provider);
            if (provider == null)
            {
                provider = new SnapshotAnimationDataProvider(entity, interval);
                animProviders[entity] = provider;
            }
            return provider;
        }

        public virtual IAssetProvider<TSprite> GetSpriteProvider(SpriterEntity entity)
        {
            DefaultAssetProvider<TSprite> provider = spriteProviders.GetOrCreate(entity.Spriter);
            return new DefaultAssetProvider<TSprite>(provider.AssetMappings);
        }

        public virtual IAssetProvider<TSound> GetSoundProvider(SpriterEntity entity)
        {
            DefaultAssetProvider<TSound> provider = soundProviders.GetOrCreate(entity.Spriter);
            return new DefaultAssetProvider<TSound>(provider.AssetMappings);
        }

        public virtual void SetSprite(Spriter spriter, SpriterFolder folder, SpriterFile file, TSprite sprite)
        {
            IAssetProvider<TSprite> provider = spriteProviders.GetOrCreate(spriter);
            provider.Set(folder.Id, file.Id, sprite);
        }

        public virtual void SetSound(Spriter spriter, SpriterFolder folder, SpriterFile file, TSound sound)
        {
            IAssetProvider<TSound> provider = soundProviders.GetOrCreate(spriter);
            provider.Set(folder.Id, file.Id, sound);
        }
    }
}
