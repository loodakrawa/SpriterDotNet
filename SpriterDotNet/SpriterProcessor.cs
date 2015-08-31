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

            float adjustedTime = AdjustTime(keyA, keyB, targetTime, animation.Length);

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

            targetTime = AdjustTime(keyA, keyB, targetTime, animation.Length);
            float factor = GetInterpolationFactor(keyA, keyB, targetTime, animation.Length);
            return Interpolate(keyA.BoneInfo, keyB.BoneInfo, factor, keyA.Spin);
        }

        private static SpriterObjectInfo GetObjectInfo(SpriterRef spriterRef, SpriterAnimation animation, float targetTime)
        {
            SpriterTimeLineKey keyA;
            SpriterTimeLineKey keyB;

            GetKeys(spriterRef, animation, out keyA, out keyB);
            if (keyB == null) return Copy(keyA.ObjectInfo);

            targetTime = AdjustTime(keyA, keyB, targetTime, animation.Length);
            float factor = GetInterpolationFactor(keyA, keyB, targetTime, animation.Length);
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

            ++keyBId;
            if (keyBId < timelineKeys.Length)
            {
                SpriterTimeLineKey nextKey = timelineKeys[keyBId];
                if (nextKey.Time == keyB.Time) keyB = nextKey;
            }
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

        private static float GetInterpolationFactor(SpriterKey keyA, SpriterKey keyB, float time, float animationLength)
        {
            float timeA = keyA.Time;
            float timeB = keyB.Time > keyA.Time ? keyB.Time : animationLength;

            return ReverseLinear(timeA, timeB, time);
        }

        private static float AdjustTime(SpriterKey keyA, SpriterKey keyB, float time, float animationLength)
        {
            float timeA = keyA.Time;
            float timeB = keyB.Time > keyA.Time ? keyB.Time : animationLength;

            float factor = ReverseLinear(timeA, timeB, time);

            switch (keyA.CurveType)
            {
                case SpriterCurveType.Instant:
                    factor = 0.0f;
                    break;
                case SpriterCurveType.Linear:
                    break;
                case SpriterCurveType.Quadratic:
                    factor = Quadratic(0.0f, keyA.C1, 1.0f, factor);
                    break;
                case SpriterCurveType.Cubic:
                    factor = Cubic(0.0f, keyA.C1, keyA.C2, 1.0f, factor);
                    break;
                case SpriterCurveType.Quartic:
                    factor = Quartic(0.0f, keyA.C1, keyA.C2, keyA.C3, 1.0f, factor);
                    break;
                case SpriterCurveType.Quintic:
                    factor = Quintic(0.0f, keyA.C1, keyA.C2, keyA.C3, keyA.C4, 1.0f, factor);
                    break;
            }

            return Linear(timeA, timeB, factor);
        }

        private static SpriterSpatialInfo Interpolate(SpriterSpatialInfo a, SpriterSpatialInfo b, float f, int spin)
        {
            return new SpriterSpatialInfo
            {
                Angle = Linear(a.Angle, AdjustAngle(a.Angle, b.Angle, spin), f),
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
                Angle = Linear(a.Angle, AdjustAngle(a.Angle, b.Angle, spin), f),
                X = Linear(a.X, b.X, f),
                Y = Linear(a.Y, b.Y, f),
                ScaleX = Linear(a.ScaleX, b.ScaleX, f),
                ScaleY = Linear(a.ScaleY, b.ScaleY, f),
                PivotX = Linear(a.PivotX, b.PivotX, f),
                PivotY = Linear(a.PivotY, b.PivotY, f),
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
        }

        private static float AdjustAngle(float a, float b, int spin)
        {
            if (spin == 1 && (b - a) < 0) b += 360;
            if (spin == -1 && (b - a) > 0) b -= 360;
            return b;
        }

        private static float ReverseLinear(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        private static float Linear(float c1, float c2, float f)
        {
            return c1 + (c2 - c1) * f;
        }

        private static float Quadratic(float c1, float c2, float c3, float f)
        {
            return Linear(Linear(c1, c2, f), Linear(c2, c3, f), f);
        }

        private static float Cubic(float c1, float c2, float c3, float c4, float f)
        {
            return Linear(Quadratic(c1, c2, c3, f), Quadratic(c2, c3, c4, f), f);
        }

        private static float Quartic(float c1, float c2, float c3, float c4, float c5, float f)
        {
            return Linear(Cubic(c1, c2, c3, c4, f), Cubic(c2, c3, c4, c5, f), f);
        }

        private static float Quintic(float c1, float c2, float c3, float c4, float c5, float c6, float f)
        {
            return Linear(Quartic(c1, c2, c3, c4, c5, f), Quartic(c2, c3, c4, c5, c6, f), f);
        }
    }
}