// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet.Providers
{
    public class DefaultAnimationDataProvider : IAnimationDataProvider
    {
        private readonly SpriterProcessor processor;

        public DefaultAnimationDataProvider()
        {
            SpriterConfig config = new SpriterConfig();
            SpriterObjectPool pool = new SpriterObjectPool(config);
            processor = new SpriterProcessor(config, pool);
        }

        public DefaultAnimationDataProvider(SpriterConfig config, SpriterObjectPool pool)
        {
            processor = new SpriterProcessor(config, pool);
        }

        public virtual FrameData GetFrameData(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            return second == null ? processor.GetFrameData(first, time, deltaTime) : processor.GetFrameData(first, second, time, deltaTime, factor);
        }
    }
}
