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
        public static Dictionary<string, FrameData[]> CalculateData(SpriterEntity entity, int interval)
        {
            return Calculate<FrameData>(entity, interval, (d, a, t, i) => SpriterProcessor.UpdateFrameData(d, a, t));
        }

        public static Dictionary<string, FrameMetadata[]> CalculateMetadata(SpriterEntity entity, int interval)
        {
            return Calculate<FrameMetadata>(entity, interval, (d, a, t, i) => SpriterProcessor.UpdateFrameMetadata(d, a, t, i));
        }

        private static Dictionary<string, T[]> Calculate<T>(SpriterEntity entity, int interval, Action<T, SpriterAnimation, float, float> filler) where T : new()
        {
            Dictionary<string, T[]> results = new Dictionary<string, T[]>();

            foreach (SpriterAnimation anim in entity.Animations)
            {
                int length = (int)Math.Ceiling(anim.Length / interval);
                T[] animData = new T[length];

                for (int i = 0; i < animData.Length; ++i)
                {
                    float time = i * interval;
                    if (time > anim.Length) time = anim.Length;

                    T data = new T();
                    filler(data, anim, time, interval);
                    animData[i] = data;
                }

                results[anim.Name] = animData;
            }
            return results;
        }

        private readonly Dictionary<string, FrameData[]> data;
        private readonly Dictionary<string, FrameMetadata[]> metaData;

        public SnapshotAnimationDataProvider(Dictionary<string, FrameData[]> data, Dictionary<string, FrameMetadata[]> metaData)
        {
            this.data = data;
            this.metaData = metaData;
        }

        public SnapshotAnimationDataProvider(SpriterEntity entity, int interval)
        {
            data = CalculateData(entity, interval);
            metaData = CalculateMetadata(entity, interval);
        }

        public override FrameData GetFrameData(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            if (data == null || second != null) return base.GetFrameData(time, deltaTime, factor, first, second);

            FrameData[] animData = data[first.Name];
            int index = (int)(time / first.Length * animData.Length);
            if (index == animData.Length) index = animData.Length - 1;
            return animData[index];
        }

        public override FrameMetadata GetFrameMetadata(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            if(metaData == null) base.GetFrameMetadata(time, deltaTime, factor, first, second);

            FrameMetadata[] animMetadata = metaData[first.Name];
            int index = (int)(time / first.Length * animMetadata.Length);
            if (index == animMetadata.Length) index = animMetadata.Length - 1;
            return animMetadata[index];
        }
    }
}
