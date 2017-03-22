// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace SpriterDotNet.MonoGame.Example
{
    public class SpriterGame : Game
    {
        private static readonly string RootDirectory = "Content";

        private static readonly int Width = 1000;
        private static readonly int Height = 625;
        private GraphicsDeviceManager graphics;

        private readonly GameStateManager gameStateManager = new GameStateManager();
        
        public SpriterGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            Content.RootDirectory = RootDirectory;

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var dm = graphics.GraphicsDevice.DisplayMode;

#if DIRECTX
            // center window
            Window.Position = new Point((dm.Width - Width) / 2, 0);
#endif

            SpriterGameState sgs = new SpriterGameState();
            FillGameState(sgs);
            //gameStateManager.Push(sgs);

            LoadingState ls = new LoadingState(sgs);
            FillGameState(ls);
            gameStateManager.Push(ls);
        }

        private void FillGameState(GameState gameState)
        {
            gameState.GraphicsDevice = GraphicsDevice;
            gameState.Content = new ContentManager(Services, RootDirectory);
            gameState.Width = Width;
            gameState.Height = Height;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            gameStateManager.Update(gameTime.ElapsedGameTime.Ticks / (float) TimeSpan.TicksPerMillisecond);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            gameStateManager.Draw(gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond);
        }
    }
}