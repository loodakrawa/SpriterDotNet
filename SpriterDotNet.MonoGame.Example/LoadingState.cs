// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace SpriterDotNet.MonoGame.Example
{
    public class LoadingState : GameState
    {
        private readonly GameState targetState;

        private Task loadTask;
        private Texture2D spinner;
        private SpriteBatch spriteBatch;

        private static readonly float RotationSpeed = (float)(90 * Math.PI / 180);
        private float rotation;

        public LoadingState(GameState targetState)
        {
            this.targetState = targetState;
        }

        protected override void Load()
        {
            loadTask = Task.Factory.StartNew(() => targetState.BaseLoad());
            spinner = Content.Load<Texture2D>("spinner");
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.Beige);

            spriteBatch.Begin();
            spriteBatch.Draw(spinner, new Vector2(Width, Height) / 2, null, null, new Vector2(spinner.Width, spinner.Height) / 2, rotation, Vector2.One, Color.White, SpriteEffects.None, 0.5f);
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            rotation += RotationSpeed * gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            if (loadTask.IsCompleted)
            {
                GameStateManager.Pop();
                GameStateManager.Push(targetState);
            }
        }

        protected override void Unload()
        {
            base.Unload();
        }
    }
}
