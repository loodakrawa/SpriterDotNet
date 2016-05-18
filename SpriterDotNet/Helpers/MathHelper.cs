// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using System;

namespace SpriterDotNet.Helpers
{
    internal static class MathHelper
    {
        /// <summary>
        /// Does a linear angle interpolation taking into account the spin
        /// </summary>
        public static float AngleLinear(float a, float b, int spin, float f)
        {
            if (spin == 0) return a;
            if (spin > 0 && (b - a) < 0) b += 360.0f;
            if (spin < 0 && (b - a) > 0) b -= 360.0f;
            return Linear(a, b, f);
        }

        /// <summary>
        /// Does a linear angle interpolation towards the closest direction
        /// </summary>
        public static float CloserAngleLinear(float a, float b, float f)
        {
            if (Math.Abs(b - a) < 180.0f) return Linear(a, b, f);
            if (a < b) a += 360.0f;
            else b += 360.0f;
            return Linear(a, b, f);
        }

        /// <summary>
        /// Calculates the interpolation factor of the given value.
        /// </summary>
        public static float GetFactor(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        /// <summary>
        /// Does a linear interpolation of the two values for the given factor.
        /// </summary>
        public static float Linear(float a, float b, float f)
        {
            return a + (b - a) * f;
        }

        /// <summary>
        /// Calculates the value of the 1-Dimensional Bezier curve defined with control points c for the given parameter f [0...1] using De Casteljau's algorithm.
        /// </summary>
        public static float Bezier(float c0, float c1, float c2, float f)
        {
            return Linear(Linear(c0, c1, f), Linear(c1, c2, f), f);
        }

        /// <summary>
        /// Calculates the value of the 1-Dimensional Bezier curve defined with control points c for the given parameter f [0...1] using De Casteljau's algorithm.
        /// </summary>
        public static float Bezier(float c0, float c1, float c2, float c3, float f)
        {
            return Linear(Bezier(c0, c1, c2, f), Bezier(c1, c2, c3, f), f);
        }

        /// <summary>
        /// Calculates the value of the 1-Dimensional Bezier curve defined with control points c for the given parameter f [0...1] using De Casteljau's algorithm.
        /// </summary>
        public static float Bezier(float c0, float c1, float c2, float c3, float c4, float f)
        {
            return Linear(Bezier(c0, c1, c2, c3, f), Bezier(c1, c2, c3, c4, f), f);
        }

        /// <summary>
        /// Calculates the value of the 1-Dimensional Bezier curve defined with control points c for the given parameter f [0...1] using De Casteljau's algorithm.
        /// </summary>
        public static float Bezier(float c0, float c1, float c2, float c3, float c4, float c5, float f)
        {
            return Linear(Bezier(c0, c1, c2, c3, c4, f), Bezier(c1, c2, c3, c4, c5, f), f);
        }

        #region BezierCodeFromSomewhere
        public static float Bezier2D(float x1, float y1, float x2, float y2, float t)
        {
            float duration = 1;
            float cx = 3.0f * x1;
            float bx = 3.0f * (x2 - x1) - cx;
            float ax = 1.0f - cx - bx;
            float cy = 3.0f * y1;
            float by = 3.0f * (y2 - y1) - cy;
            float ay = 1.0f - cy - by;

            return Solve(ax, bx, cx, ay, by, cy, t, SolveEpsilon(duration));
        }

        private static float SampleCurve(float a, float b, float c, float t)
        {
            return ((a * t + b) * t + c) * t;
        }

        private static float SampleCurveDerivativeX(float ax, float bx, float cx, float t)
        {
            return (3.0f * ax * t + 2.0f * bx) * t + cx;
        }

        private static float SolveEpsilon(float duration)
        {
            return 1.0f / (200.0f * duration);
        }

        private static float Solve(float ax, float bx, float cx, float ay, float by, float cy, float x, float epsilon)
        {
            return SampleCurve(ay, by, cy, SolveCurveX(ax, bx, cx, x, epsilon));
        }

        private static float SolveCurveX(float ax, float bx, float cx, float x, float epsilon)
        {
            float t0;
            float t1;
            float t2;
            float x2;
            float d2;
            int i;

            for (t2 = x, i = 0; i < 8; i++)
            {
                x2 = SampleCurve(ax, bx, cx, t2) - x;
                if (Math.Abs(x2) < epsilon) return t2;

                d2 = SampleCurveDerivativeX(ax, bx, cx, t2);
                if (Math.Abs(d2) < 1e-6) break;

                t2 = t2 - x2 / d2;
            }

            t0 = 0.0f;
            t1 = 1.0f;
            t2 = x;

            if (t2 < t0) return t0;
            if (t2 > t1) return t1;

            while (t0 < t1)
            {
                x2 = SampleCurve(ax, bx, cx, t2);
                if (Math.Abs(x2 - x) < epsilon) return t2;
                if (x > x2) t0 = t2;
                else t1 = t2;
                t2 = (t1 - t0) * 0.5f + t0;
            }

            return t2;
        }

        #endregion
    }
}
