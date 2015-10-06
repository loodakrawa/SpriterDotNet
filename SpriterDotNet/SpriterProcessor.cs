// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;
using System.Linq;

namespace SpriterDotNet
{
    public static class SpriterProcessor
    {
        public static SpriterObjectInfo[] GetDrawData(SpriterAnimation first, SpriterAnimation second, float targetTime, float factor)
        {
            float targetTimeSecond = targetTime / first.Length * second.Length;

            SpriterMainLineKey firstKeyA;
            SpriterMainLineKey firstKeyB;
            GetKeys(first, targetTime, out firstKeyA, out firstKeyB);
            
            SpriterMainLineKey secondKeyA;
            SpriterMainLineKey secondKeyB;
            GetKeys(second, targetTimeSecond, out secondKeyA, out secondKeyB);

            if (firstKeyA.BoneRefs.Length != secondKeyA.BoneRefs.Length
                || firstKeyB.BoneRefs.Length != secondKeyB.BoneRefs.Length
                || firstKeyA.ObjectRefs.Length != secondKeyA.ObjectRefs.Length
                || firstKeyB.ObjectRefs.Length != secondKeyB.ObjectRefs.Length) return GetDrawData(first, targetTime);

            float adjustedTimeFirst = AdjustTime(firstKeyA, firstKeyB, first.Length, targetTime);
            float adjustedTimeSecond = AdjustTime(secondKeyA, secondKeyB, second.Length, targetTimeSecond);

            SpriterSpatialInfo[] boneInfosA = GetBoneInfos(firstKeyA, first, adjustedTimeFirst);
            SpriterSpatialInfo[] boneInfosB = GetBoneInfos(secondKeyA, second, adjustedTimeSecond);
            SpriterSpatialInfo[] boneInfos = null;
            if (boneInfosA != null && boneInfosB != null)
            {
                boneInfos = new SpriterSpatialInfo[boneInfosA.Length];
                for (int i = 0; i < boneInfosA.Length; ++i)
                {
                    SpriterSpatialInfo boneA = boneInfosA[i];
                    SpriterSpatialInfo boneB = boneInfosB[i];
                    SpriterSpatialInfo interpolated = Interpolate(boneA, boneB, factor, 1);
                    interpolated.Angle = CloserAngleLinear(boneA.Angle, boneB.Angle, factor);
                    boneInfos[i] = interpolated;
                }
            }

            SpriterMainLineKey baseKey = factor < 0.5f ? firstKeyA : firstKeyB;

            SpriterObjectInfo[] ret = new SpriterObjectInfo[baseKey.ObjectRefs.Length];

            for (int i = 0; i < baseKey.ObjectRefs.Length; ++i)
            {
                SpriterObjectRef objectRefFirst = baseKey.ObjectRefs[i];
                SpriterObjectInfo interpolatedFirst = GetObjectInfo(objectRefFirst, first, adjustedTimeFirst);

                SpriterObjectRef objectRefSecond = secondKeyA.ObjectRefs[i];
                SpriterObjectInfo interpolatedSecond = GetObjectInfo(objectRefSecond, second, adjustedTimeSecond);

                SpriterObjectInfo info = Interpolate(interpolatedFirst, interpolatedSecond, factor, 1);
                info.Angle = CloserAngleLinear(interpolatedFirst.Angle, interpolatedSecond.Angle, factor);

                if (boneInfos != null && objectRefFirst.ParentId >= 0) ApplyParentTransform(info, boneInfos[objectRefFirst.ParentId]);

                ret[objectRefFirst.Id] = info;
            }

            return ret;
        }

        public static SpriterObjectInfo[] GetDrawData(SpriterAnimation animation, float targetTime)
        {
            SpriterMainLineKey keyA;
            SpriterMainLineKey keyB;
            GetKeys(animation, targetTime, out keyA, out keyB);

            float adjustedTime = AdjustTime(keyA, keyB, animation.Length, targetTime);

            var boneInfos = GetBoneInfos(keyA, animation, targetTime);

            SpriterObjectInfo[] ret = new SpriterObjectInfo[keyA.ObjectRefs.Length];

            foreach (SpriterObjectRef objectRef in keyA.ObjectRefs)
            {
                SpriterObjectInfo interpolated = GetObjectInfo(objectRef, animation, adjustedTime);
                if (boneInfos != null && objectRef.ParentId >= 0) ApplyParentTransform(interpolated, boneInfos[objectRef.ParentId]);
                ret[objectRef.Id] = interpolated;
            }

            return ret;
        }

        private static SpriterSpatialInfo[] GetBoneInfos(SpriterMainLineKey key, SpriterAnimation animation, float targetTime)
        {
            if (key.BoneRefs == null) return null;
            SpriterSpatialInfo[] ret = new SpriterSpatialInfo[key.BoneRefs.Length];

            for (int i = 0; i < key.BoneRefs.Length; ++i)
            {
                SpriterRef boneRef = key.BoneRefs[i];
                SpriterSpatialInfo interpolated = GetBoneInfo(boneRef, animation, targetTime);

                if (boneRef.ParentId >= 0) ApplyParentTransform(interpolated, ret[boneRef.ParentId]);
                ret[i] = interpolated;
            }

            return ret;
        }

        private static float AdjustTime(SpriterKey keyA, SpriterKey keyB, float animationLength, float targetTime)
        {
            float nextTime = keyB.Time > keyA.Time ? keyB.Time : animationLength;
            float factor = GetFactor(keyA, keyB, animationLength, targetTime);
            return Linear(keyA.Time, nextTime, factor);
        }

        private static void GetKeys(SpriterAnimation animation, float targetTime, out SpriterMainLineKey keyA, out SpriterMainLineKey keyB)
        {
            SpriterMainLineKey[] keys = animation.MainlineKeys;
            keyA = keys.Last(k => k.Time <= targetTime);
            int nextKey = keyA.Id + 1;
            if (nextKey >= keys.Length) nextKey = 0;
            keyB = keys[nextKey];
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

            if (timeA > timeB)
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
                Alpha = Linear(a.Alpha, b.Alpha, f),
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
            child.Angle = parent.Angle + Math.Sign(parent.ScaleX * parent.ScaleY) * child.Angle;
            child.Angle %= 360.0f;
        }

        private static float AngleLinear(float a, float b, int spin, float f)
        {
            if (spin == 0) return a;
            if (spin > 0 && (b - a) < 0) b += 360;
            if (spin < 0 && (b - a) > 0) b -= 360;
            return Linear(a, b, f);
        }

        private static float CloserAngleLinear(float a1, float a2, float factor)
        {
            if (Math.Abs(a2 - a1) < 180.0f) return Linear(a1, a2, factor);
            if (a1 < a2) a1 += 360.0f;
            else a2 += 360.0f;
            return Linear(a1, a2, factor);
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