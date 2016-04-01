// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;

namespace SpriterDotNet.Helpers
{
    internal static class SpriterHelper
    {
        /// <summary>
        /// Checks if the given Mainline keys are compatible for animation blending. 
        /// Even if this method returns true there is no guarantee that the animations are really compatible.
        /// </summary>
        public static bool WillItBlend(SpriterMainlineKey firstKey, SpriterMainlineKey secondKey)
        {
            if (firstKey.BoneRefs != null)
            {
                if (secondKey.BoneRefs == null) return false;
                if (firstKey.BoneRefs.Length != secondKey.BoneRefs.Length) return false;
            }
            else if (secondKey.BoneRefs != null) return false;

            if (firstKey.ObjectRefs != null)
            {
                if (secondKey.ObjectRefs == null) return false;
                if (firstKey.ObjectRefs.Length != secondKey.ObjectRefs.Length) return false;
            }
            else if (secondKey.ObjectRefs != null) return false;

            return true;
        }

        /// <summary>
        /// Gets the next animation key coming after the given one.
        /// </summary>
        public static T GetNextKey<T>(this T[] keys, T firstKey, bool looping) where T : SpriterKey
        {
            if (keys.Length == 1) return null;

            int keyBId = firstKey.Id + 1;
            if (keyBId >= keys.Length)
            {
                if (!looping) return null;
                keyBId = 0;
            }

            return keys[keyBId];
        }

        /// <summary>
        /// Finds the last key before the given time.
        /// </summary>
        public static T GetLastKey<T>(this T[] keys, float targetTime) where T : SpriterKey
        {
            T current = null;
            for (int i = 0; i < keys.Length; ++i)
            {
                T key = keys[i];
                if (key.Time > targetTime) break;
                current = key;
            }

            return current;
        }

        /// <summary>
        /// Applies parent transforms to the given child spatial.
        /// </summary>
        public static void ApplyParentTransform(this SpriterSpatial child, SpriterSpatial parent)
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

        /// <summary>
        /// Fills all the values of target object from the given source object.
        /// </summary>
        public static void FillFrom(this SpriterObject target, SpriterObject source)
        {
            target.AnimationId = source.AnimationId;
            target.EntityId = source.EntityId;
            target.FileId = source.FileId;
            target.FolderId = source.FolderId;
            target.PivotX = source.PivotX;
            target.PivotY = source.PivotY;
            target.T = source.T;

            target.FillFrom((SpriterSpatial)source);
        }

        /// <summary>
        /// Fills all the values of target spatial from the given source spatial.
        /// </summary>
        public static void FillFrom(this SpriterSpatial target, SpriterSpatial source)
        {
            target.Alpha = source.Alpha;
            target.Angle = source.Angle;
            target.ScaleX = source.ScaleX;
            target.ScaleY = source.ScaleY;
            target.X = source.X;
            target.Y = source.Y;
        }

        /// <summary>
        /// Fills all the values of target varvalue by interpolating values from the two given varvalues with the given factor.
        /// </summary>
        public static void Interpolate(this SpriterVarValue target, SpriterVarValue valA, SpriterVarValue valB, float factor)
        {
            target.Type = valA.Type;
            target.StringValue = valA.StringValue;
            target.FloatValue = MathHelper.Linear(valA.FloatValue, valB.FloatValue, factor);
            target.IntValue = (int)MathHelper.Linear(valA.IntValue, valB.IntValue, factor);
        }

        /// <summary>
        /// Fills all the values of target spatial by interpolating values from the two given spatials with the given factor and spin.
        /// </summary>
        public static void Interpolate(this SpriterSpatial target, SpriterSpatial a, SpriterSpatial b, float factor, int spin)
        {
            target.Angle = MathHelper.AngleLinear(a.Angle, b.Angle, spin, factor);
            target.X = MathHelper.Linear(a.X, b.X, factor);
            target.Y = MathHelper.Linear(a.Y, b.Y, factor);
            target.ScaleX = MathHelper.Linear(a.ScaleX, b.ScaleX, factor);
            target.ScaleY = MathHelper.Linear(a.ScaleY, b.ScaleY, factor);
        }

        /// <summary>
        /// Fills all the values of target object by interpolating values from the two given object with the given factor and spin.
        /// </summary>
        public static void Interpolate(this SpriterObject target, SpriterObject a, SpriterObject b, float factor, int spin)
        {
            target.Angle = MathHelper.AngleLinear(a.Angle, b.Angle, spin, factor);
            target.Alpha = MathHelper.Linear(a.Alpha, b.Alpha, factor);
            target.X = MathHelper.Linear(a.X, b.X, factor);
            target.Y = MathHelper.Linear(a.Y, b.Y, factor);
            target.ScaleX = MathHelper.Linear(a.ScaleX, b.ScaleX, factor);
            target.ScaleY = MathHelper.Linear(a.ScaleY, b.ScaleY, factor);
            target.PivotX = a.PivotX;
            target.PivotY = a.PivotY;
            target.FileId = a.FileId;
            target.FolderId = a.FolderId;
            target.EntityId = a.EntityId;
            target.AnimationId = a.AnimationId;
            target.T = MathHelper.Linear(a.T, b.T, factor);
        }

        /// <summary>
        /// Adjusts the factor based on the curve type from the given key.
        /// </summary>
        public static float AdjustFactor(float factor, SpriterKey key)
        {
            switch (key.CurveType)
            {
                case SpriterCurveType.Instant:
                    factor = 0.0f;
                    break;
                case SpriterCurveType.Linear:
                    break;
                case SpriterCurveType.Quadratic:
                    factor = MathHelper.Bezier(0.0f, key.C1, 1.0f, factor);
                    break;
                case SpriterCurveType.Cubic:
                    factor = MathHelper.Bezier(0.0f, key.C1, key.C2, 1.0f, factor);
                    break;
                case SpriterCurveType.Quartic:
                    factor = MathHelper.Bezier(0.0f, key.C1, key.C2, key.C3, 1.0f, factor);
                    break;
                case SpriterCurveType.Quintic:
                    factor = MathHelper.Bezier(0.0f, key.C1, key.C2, key.C3, key.C4, 1.0f, factor);
                    break;
                case SpriterCurveType.Bezier:
                    factor = MathHelper.Bezier2D(key.C1, key.C2, key.C3, key.C4, factor);
                    break;
            }

            return factor;
        }

        /// <summary>
        /// Adjusts the animation time taking into account key curve types.
        /// </summary>
        public static float AdjustTime(float targetTime, SpriterKey keyA, SpriterKey keyB, float animationLength)
        {
            float nextTime = keyB.Time > keyA.Time ? keyB.Time : animationLength;
            float factor = GetFactor(keyA, keyB, animationLength, targetTime);
            return MathHelper.Linear(keyA.Time, nextTime, factor);
        }

        /// <summary>
        /// Gets the interpolation factor for the two keys. Takes into account key curve types.
        /// </summary>
        public static float GetFactor(SpriterKey keyA, SpriterKey keyB, float animationLength, float targetTime)
        {
            float timeA = keyA.Time;
            float timeB = keyB.Time;

            if (timeA > timeB)
            {
                timeB += animationLength;
                if (targetTime < timeA) targetTime += animationLength;
            }

            float factor = MathHelper.GetFactor(timeA, timeB, targetTime);
            factor = AdjustFactor(factor, keyA);
            return factor;
        }
    }
}
