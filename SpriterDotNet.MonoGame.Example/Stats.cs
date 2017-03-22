// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using System;

namespace SpriterDotNet.MonoGame.Example
{
    public class Stats
    {
        private static readonly float Second = 1000.0f;

        public int DrawRate { get; private set; }

        private int drawCount;
        private int updateCount;

        private float elapsedTime;

        public void OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;

            ++updateCount;

            if (elapsedTime > Second)
            {
                elapsedTime -= Second;
                DrawRate = drawCount;
                drawCount = 0;
            }
        }

        public void OnDraw()
        {
            ++drawCount;
        }
    }
}
