// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using System;

namespace SpriterDotNet.MonoGame
{
    internal static class MathHelper
    {
        public static readonly float DegToRad = (float)(Math.PI / 180.0);

        public static Matrix GetMatrix(Vector2 scale, float rotation, Vector2 position)
        {
            return Matrix.CreateScale(Math.Abs(scale.X), Math.Abs(scale.Y), 1.0f) * Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(position.X, position.Y, 0.0f);
        }

        public static void DecomposeMatrix(this Matrix matrix, out Vector2 scale, out float rotation, out Vector2 position)
        {
            Vector3 position3, scale3;
            Quaternion rotationQ;
            matrix.Decompose(out scale3, out rotationQ, out position3);
            Vector2 direction = Vector2.Transform(Vector2.UnitX, rotationQ);
            rotation = (float)Math.Atan2(direction.Y, direction.X);
            position = new Vector2(position3.X, position3.Y);
            scale = new Vector2(scale3.X, scale3.Y);
        }
    }
}
