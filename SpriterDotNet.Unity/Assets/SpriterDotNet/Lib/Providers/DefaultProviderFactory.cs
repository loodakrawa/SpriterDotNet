// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.Helpers;
using System.Collections.Generic;

namespace SpriterDotNet.Providers
{
    /// <summary>
    /// The default IProviderFactory. It implements the Flyweight pattern sharing as much state as possible between Animators.
    /// </summary>
    public class DefaultProviderFactory<TSprite, TSound> : IProviderFactory<TSprite, TSound>
    {
        private const int DefaultInterval = 20;

        protected Dictionary<SpriterEntity, SnapshotFrameDataProvider> AnimProviders { get; set; }
        protected Dictionary<Spriter, DefaultAssetProvider<TSprite>> SpriteProviders { get; set; }
        protected Dictionary<Spriter, DefaultAssetProvider<TSound>> SoundProviders { get; set; }

        protected bool CacheAnimations { get; set; }
        protected int Interval { get; set; }

        protected Config Config { get; set; }
        protected ObjectPool Pool { get; set; }

        public DefaultProviderFactory(Config config) : this(config, false, DefaultInterval)
        {
        }

        public DefaultProviderFactory(Config config, bool cacheAnimations) : this(config, cacheAnimations, DefaultInterval)
        {
        }

        public DefaultProviderFactory(Config config, bool cacheAnimations, int interval)
        {
            Config = config;
            CacheAnimations = cacheAnimations;
            Interval = interval;

            Pool = new ObjectPool(config);
            AnimProviders = new Dictionary<SpriterEntity, SnapshotFrameDataProvider>();
            SpriteProviders = new Dictionary<Spriter, DefaultAssetProvider<TSprite>>();
            SoundProviders = new Dictionary<Spriter, DefaultAssetProvider<TSound>>();
        }

        public virtual IFrameDataProvider GetDataProvider(SpriterEntity entity)
        {
            if (!CacheAnimations) return new DefaultFrameDataProvider(Config, Pool);

            SnapshotFrameDataProvider provider;
            AnimProviders.TryGetValue(entity, out provider);
            if (provider == null)
            {
                var data = SnapshotFrameDataProvider.Calculate(entity, Interval, Config);
                provider = new SnapshotFrameDataProvider(Config, Pool, data);
                AnimProviders[entity] = provider;
            }
            return provider;
        }

        public virtual IAssetProvider<TSprite> GetSpriteProvider(SpriterEntity entity)
        {
            DefaultAssetProvider<TSprite> provider = SpriteProviders.GetOrCreate(entity.Spriter);
            return new DefaultAssetProvider<TSprite>(provider.AssetMappings);
        }

        public virtual IAssetProvider<TSound> GetSoundProvider(SpriterEntity entity)
        {
            DefaultAssetProvider<TSound> provider = SoundProviders.GetOrCreate(entity.Spriter);
            return new DefaultAssetProvider<TSound>(provider.AssetMappings);
        }

        public virtual void SetSprite(Spriter spriter, SpriterFolder folder, SpriterFile file, TSprite sprite)
        {
            IAssetProvider<TSprite> provider = SpriteProviders.GetOrCreate(spriter);
            provider.Set(folder.Id, file.Id, sprite);
        }

        public virtual void SetSound(Spriter spriter, SpriterFolder folder, SpriterFile file, TSound sound)
        {
            IAssetProvider<TSound> provider = SoundProviders.GetOrCreate(spriter);
            provider.Set(folder.Id, file.Id, sound);
        }
    }
}
