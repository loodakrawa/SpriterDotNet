// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SpriterDotNet.AnimationDataProvider
{
    public class SnapshotAnimationDataProvider : DefaultAnimationDataProvider
    {
        public static IDictionary<string, FrameData[]> GetData(SpriterEntity entity, int interval)
        {
            IDictionary<string, FrameData[]> data = new Dictionary<string, FrameData[]>();

            foreach (SpriterAnimation anim in entity.Animations)
            {
                int length = (int)Math.Ceiling(anim.Length / interval);
                FrameData[] animData = new FrameData[length];

                for (int i = 0; i < animData.Length; ++i)
                {
                    float time = i * interval;
                    if (time > anim.Length) time = anim.Length;

                    FrameData fd = new FrameData();
                    SpriterProcessor.UpdateFrameData(fd, anim, time);
                    animData[i] = fd;
                }

                data[anim.Name] = animData;
            }

            return data;
        }

        private readonly IDictionary<string, FrameData[]> data;

        public SnapshotAnimationDataProvider(IDictionary<string, FrameData[]> data)
        {
            this.data = data;
        }

        public override FrameData GetFrameData(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            if (second != null) return base.GetFrameData(time, deltaTime, factor, first, second);

            FrameData[] animData = data[first.Name];
            int index = (int)(time / first.Length * animData.Length);
            if (index == animData.Length) index = animData.Length - 1;
            return animData[index];
        }
    }
}
