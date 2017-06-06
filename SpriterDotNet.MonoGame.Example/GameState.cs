﻿// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpriterDotNet.MonoGame.Example
{
    public abstract class GameState
    {
        public GameStateManager GameStateManager { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public ContentManager Content { get; set; }
        protected SpriteBatch SpriteBatch { get; private set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public bool Loaded { get; private set; }

        public void BaseLoad()
        {
            Load();
            Loaded = true;
        }


        public void BaseUnload()
        {
            Unload();
            Loaded = false;
            Content.Unload();
        }

        protected abstract void Load();
        protected abstract void Unload();
        public abstract void Update(float deltaTime);
        public abstract void Draw(float deltaTime);
    }
}
