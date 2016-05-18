// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet
{
    /// <summary>
    /// A factory for asset and frame data providers.
    /// </summary>
    public interface IProviderFactory<TSprite, TSound>
    {
        IAssetProvider<TSprite> GetSpriteProvider(SpriterEntity entity);
        IAssetProvider<TSound> GetSoundProvider(SpriterEntity entity);
        IFrameDataProvider GetDataProvider(SpriterEntity entity);
    }
}
