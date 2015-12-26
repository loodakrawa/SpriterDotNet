// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

namespace SpriterDotNet.AnimationDataProvider
{
    public class DefaultAnimationDataProvider : IAnimationDataProvider
    {
        private readonly FrameData data = new FrameData();
        private readonly FrameMetadata metadata = new FrameMetadata();

        public virtual FrameData GetFrameData(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            data.Clear();

            if (second == null)
            {
                SpriterProcessor.UpdateFrameData(data, first, time);
            }
            else
            {
                SpriterProcessor.UpdateFrameData(data, first, second, time, factor);
            }

            return data;
        }

        public virtual FrameMetadata GetFrameMetadata(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            metadata.Clear();

            if (second == null)
            {
                SpriterProcessor.UpdateFrameMetadata(metadata, first, time, deltaTime);
            }
            else
            {
                SpriterProcessor.GetFrameMetadata(metadata, first, second, time, deltaTime, factor);
            }

            return metadata;
        }
    }
}
