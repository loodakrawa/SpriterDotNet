// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet.Providers
{
    /// <summary>
    /// Default IFrameDataProvider implementation. It simply calculates the frame data for every frame.
    /// </summary>
    public class DefaultFrameDataProvider : IFrameDataProvider
    {
        protected FrameDataCalculator Calculator { get; set; }

        public DefaultFrameDataProvider()
        {
            Config config = new Config();
            ObjectPool pool = new ObjectPool(config);
            Calculator = new FrameDataCalculator(config, pool);
        }

        public DefaultFrameDataProvider(Config config, ObjectPool pool)
        {
            Calculator = new FrameDataCalculator(config, pool);
        }

        public virtual FrameData GetFrameData(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            return second == null ? Calculator.GetFrameData(first, time, deltaTime) : Calculator.GetFrameData(first, second, time, deltaTime, factor);
        }
    }
}
