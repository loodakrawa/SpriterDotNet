// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Collections.Generic;

namespace SpriterDotNet.Providers
{
    public class SnapshotFrameDataProvider : DefaultFrameDataProvider
    {
        public static Dictionary<string, FrameData[]> Calculate(SpriterEntity entity, int interval, Config config)
        {
            Dictionary<string, FrameData[]> results = new Dictionary<string, FrameData[]>();

            foreach (SpriterAnimation anim in entity.Animations)
            {
                int length = (int)Math.Ceiling(anim.Length / interval);
                FrameData[] animData = new FrameData[length];

                for (int i = 0; i < animData.Length; ++i)
                {
                    float time = i * interval;
                    if (time > anim.Length) time = anim.Length;

                    ObjectPool pool = new ObjectPool(config);
                    FrameData data = new FrameDataCalculator(config, pool).GetFrameData(anim, time, interval);
                    animData[i] = data;
                }

                results[anim.Name] = animData;
            }
            return results;
        }

        protected Dictionary<string, FrameData[]> Data { get; set; }

        public SnapshotFrameDataProvider(Config config, ObjectPool pool, Dictionary<string, FrameData[]> data) : base(config, pool)
        {
            Data = data;
        }

        public override FrameData GetFrameData(float time, float deltaTime, float factor, SpriterAnimation first, SpriterAnimation second = null)
        {
            if (Data == null || second != null) return base.GetFrameData(time, deltaTime, factor, first, second);

            FrameData[] animData = Data[first.Name];
            int index = (int)(time / first.Length * animData.Length);
            if (index == animData.Length) index = animData.Length - 1;
            return animData[index];
        }
    }
}
