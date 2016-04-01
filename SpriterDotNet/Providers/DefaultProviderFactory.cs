// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.Helpers;
using System.Collections.Generic;

namespace SpriterDotNet.Providers
{
    public class DefaultProviderFactory<TSprite, TSound> : IProviderFactory<TSprite, TSound>
    {
        private readonly Dictionary<SpriterEntity, SnapshotAnimationDataProvider> animProviders = new Dictionary<SpriterEntity, SnapshotAnimationDataProvider>();
        private readonly Dictionary<Spriter, DefaultAssetProvider<TSprite>> spriteProviders = new Dictionary<Spriter, DefaultAssetProvider<TSprite>>();
        private readonly Dictionary<Spriter, DefaultAssetProvider<TSound>> soundProviders = new Dictionary<Spriter, DefaultAssetProvider<TSound>>();

        private readonly bool cacheAnimations;
        private readonly int interval;

        private readonly SpriterConfig config;
        private readonly SpriterObjectPool pool;

        public DefaultProviderFactory(SpriterConfig config, bool cacheAnimations = false, int interval = 20)
        {
            this.config = config;
            this.cacheAnimations = cacheAnimations;
            this.interval = interval;
            pool = new SpriterObjectPool(config);
        }

        public virtual IAnimationDataProvider GetDataProvider(SpriterEntity entity)
        {
            if (!cacheAnimations) return new DefaultAnimationDataProvider(config, pool);

            SnapshotAnimationDataProvider provider;
            animProviders.TryGetValue(entity, out provider);
            if (provider == null)
            {
                var data = SnapshotAnimationDataProvider.Calculate(entity, interval, config);
                provider = new SnapshotAnimationDataProvider(config, pool, data);
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
