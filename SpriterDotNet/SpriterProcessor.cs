/**
The MIT License (MIT)

Copyright (c) 2015 Luka "loodakrawa" Sverko

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
**/

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpriterDotNet
{
    public static class SpriterProcessor
    {
        public static SpriterObjectInfo[] GetDrawData(SpriterAnimation animation, float targetTime)
        {
            SpriterMainLineKey[] keys = animation.MainlineKeys;
            SpriterMainLineKey keyA = keys.Last(k => k.Time <= targetTime);
            int nextKey = keyA.Id + 1;
            if (nextKey >= keys.Length) nextKey = 0;
            SpriterMainLineKey keyB = keys[nextKey];

            float nextTime = keyB.Time > keyA.Time ? keyB.Time : animation.Length;
            float factor = GetFactor(keyA, keyB, animation.Length, targetTime);
            float adjustedTime = Linear(keyA.Time, nextTime, factor);

            var boneInfos = new Dictionary<int, SpriterSpatialInfo>();

            if (keyA.BoneRefs != null)
            {
                for (int i = 0; i < keyA.BoneRefs.Length; ++i)
                {
                    SpriterRef boneRef = keyA.BoneRefs[i];
                    SpriterSpatialInfo interpolated = GetBoneInfo(boneRef, animation, adjustedTime);

                    if (boneRef.ParentId >= 0) ApplyParentTransform(interpolated, boneInfos[boneRef.ParentId]);
                    boneInfos[i] = interpolated;
                }
            }

            SpriterObjectInfo[] ret = new SpriterObjectInfo[keyA.ObjectRefs.Length];

            for (int i = 0; i < keyA.ObjectRefs.Length; ++i)
            {
                SpriterObjectRef objectRef = keyA.ObjectRefs[i];
                SpriterObjectInfo interpolated = GetObjectInfo(objectRef, animation, adjustedTime);

                if (objectRef.ParentId >= 0) ApplyParentTransform(interpolated, boneInfos[objectRef.ParentId]);
                ret[objectRef.Id] = interpolated;
            }

            return ret;
        }

        private static SpriterSpatialInfo GetBoneInfo(SpriterRef spriterRef, SpriterAnimation animation, float targetTime)
        {
            SpriterTimeLineKey keyA;
            SpriterTimeLineKey keyB;

            GetKeys(spriterRef, animation, out keyA, out keyB);
            if (keyB == null) return Copy(keyA.BoneInfo);

            float factor = GetFactor(keyA, keyB, animation.Length, targetTime);
            return Interpolate(keyA.BoneInfo, keyB.BoneInfo, factor, keyA.Spin);
        }

        private static SpriterObjectInfo GetObjectInfo(SpriterRef spriterRef, SpriterAnimation animation, float targetTime)
        {
            SpriterTimeLineKey keyA;
            SpriterTimeLineKey keyB;

            GetKeys(spriterRef, animation, out keyA, out keyB);
            if (keyB == null) return Copy(keyA.ObjectInfo);

            float factor = GetFactor(keyA, keyB, animation.Length, targetTime);
            return Interpolate(keyA.ObjectInfo, keyB.ObjectInfo, factor, keyA.Spin);
        }

        private static void GetKeys(SpriterRef spriterRef, SpriterAnimation animation, out SpriterTimeLineKey keyA, out SpriterTimeLineKey keyB)
        {
            int tId = spriterRef.TimelineId;
            int kId = spriterRef.KeyId;

            var timelineKeys = animation.Timelines[tId].Keys;

            keyA = timelineKeys[kId];
            keyB = null;

            if (timelineKeys.Length == 1) return;

            int keyBId = kId + 1;
            if (keyBId >= timelineKeys.Length)
            {
                if (animation.Looping) keyBId = 0;
                else return;
            }

            keyB = timelineKeys[keyBId];
        }

        private static SpriterSpatialInfo Copy(SpriterSpatialInfo info)
        {
            SpriterSpatialInfo copy = new SpriterSpatialInfo();
            FillFrom(copy, info);
            return copy;
        }

        private static SpriterObjectInfo Copy(SpriterObjectInfo info)
        {
            SpriterObjectInfo copy = new SpriterObjectInfo
            {
                FileId = info.FileId,
                FolderId = info.FolderId,
                PivotX = info.PivotX,
                PivotY = info.PivotY
            };

            FillFrom(copy, info);
            return copy;
        }

        private static void FillFrom(SpriterSpatialInfo target, SpriterSpatialInfo source)
        {
            target.Alpha = source.Alpha;
            target.Angle = source.Angle;
            target.ScaleX = source.ScaleX;
            target.ScaleY = source.ScaleY;
            target.X = source.X;
            target.Y = source.Y;
        }

        private static float GetFactor(SpriterKey keyA, SpriterKey keyB, float animationLength, float targetTime)
        {
            float timeA = keyA.Time;
            float timeB = keyB.Time;

            if(timeA > timeB)
            {
                timeB += animationLength;
                if (targetTime < timeA) targetTime += animationLength;
            }

            float factor = ReverseLinear(timeA, timeB, targetTime);
            factor = ApplySpeedCurve(keyA, factor);
            return factor;
        }

        private static float ApplySpeedCurve(SpriterKey key, float factor)
        {
            switch (key.CurveType)
            {
                case SpriterCurveType.Instant:
                    factor = 0.0f;
                    break;
                case SpriterCurveType.Linear:
                    break;
                case SpriterCurveType.Quadratic:
                    factor = Bezier(factor, 0.0f, key.C1, 1.0f);
                    break;
                case SpriterCurveType.Cubic:
                    factor = Bezier(factor, 0.0f, key.C1, key.C2, 1.0f);
                    break;
                case SpriterCurveType.Quartic:
                    factor = Bezier(factor, 0.0f, key.C1, key.C2, key.C3, 1.0f);
                    break;
                case SpriterCurveType.Quintic:
                    factor = Bezier(factor, 0.0f, key.C1, key.C2, key.C3, key.C4, 1.0f);
                    break;
            }

            return factor;
        }

        private static SpriterSpatialInfo Interpolate(SpriterSpatialInfo a, SpriterSpatialInfo b, float f, int spin)
        {
            return new SpriterSpatialInfo
            {
                Angle = AngleLinear(a.Angle, b.Angle, spin, f),
                X = Linear(a.X, b.X, f),
                Y = Linear(a.Y, b.Y, f),
                ScaleX = Linear(a.ScaleX, b.ScaleX, f),
                ScaleY = Linear(a.ScaleY, b.ScaleY, f)
            };
        }

        private static SpriterObjectInfo Interpolate(SpriterObjectInfo a, SpriterObjectInfo b, float f, int spin)
        {
            return new SpriterObjectInfo
            {
                Angle = AngleLinear(a.Angle, b.Angle, spin, f),
                X = Linear(a.X, b.X, f),
                Y = Linear(a.Y, b.Y, f),
                ScaleX = Linear(a.ScaleX, b.ScaleX, f),
                ScaleY = Linear(a.ScaleY, b.ScaleY, f),
                PivotX = a.PivotX,
                PivotY = a.PivotY,
                FileId = a.FileId,
                FolderId = a.FolderId
            };
        }

        private static void ApplyParentTransform(SpriterSpatialInfo child, SpriterSpatialInfo parent)
        {
            float px = parent.ScaleX * child.X;
            float py = parent.ScaleY * child.Y;
            double angleRad = parent.Angle * Math.PI / 180;
            float s = (float)Math.Sin(angleRad);
            float c = (float)Math.Cos(angleRad);

            child.X = px * c - py * s + parent.X;
            child.Y = px * s + py * c + parent.Y;
            child.ScaleX *= parent.ScaleX;
            child.ScaleY *= parent.ScaleY;
            child.Angle += parent.Angle;
            child.Angle %= 360.0f;
        }

        private static float AngleLinear(float a, float b, int spin, float f)
        {
            if (spin == 0) return a;
            if (spin > 0 && (b - a) < 0) b += 360;
            if (spin < 0 && (b - a) > 0) b -= 360;
            return Linear(a, b, f);
        }

        private static float ReverseLinear(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        private static float Linear(float c1, float c2, float f)
        {
            return c1 + (c2 - c1) * f;
        }

        private static float Bezier(float t, params float[] f)
        {
            for (int i = f.Length - 1; i > 0; --i)
            {
                for (int j = 0; j < i; ++j)
                {
                    f[j] = Linear(f[j], f[j + 1], t);
                }
            }

            return f[0];
        }
    }
}