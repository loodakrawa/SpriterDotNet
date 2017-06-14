// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriterDotNet.MonoGame;
using SpriterDotNet.MonoGame.Content;
using SpriterDotNet.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpriterDotNet.Example.MonoGame
{
    public class ExampleGame : Game
    {
        private static readonly int WindowWidth = 1000;
        private static readonly int WindowHeight = 625;

        private static readonly string ContentRootDirectory = "Content";
        private static readonly string FontName = "Status";

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private SpriteFont spriteFont;
        private readonly List<MonoGameAnimator> animators = new List<MonoGameAnimator>();
        private MonoGameAnimator animator;

        private string animatorInfo = String.Empty;
        private string metadata = String.Empty;

        private readonly Stats stats = new Stats();

        private KeyboardState oldState;
        private float elapsedTime;

        public ExampleGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            Content.RootDirectory = ContentRootDirectory;

            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            oldState = Keyboard.GetState();
        }

        protected override void LoadContent()
        {
            Vector2 screenCentre = new Vector2(WindowWidth * 0.5f, WindowHeight * 0.5f);

            spriteFont = Content.Load<SpriteFont>(FontName);
            DefaultProviderFactory<ISprite, SoundEffect> factory = new DefaultProviderFactory<ISprite, SoundEffect>(Constants.Config, true);

            foreach (string scmlPath in Constants.ScmlFiles)
            {
                SpriterContentLoader loader = new SpriterContentLoader(Content, scmlPath);
                loader.Fill(factory);

                Stack<SpriteDrawInfo> drawInfoPool = new Stack<SpriteDrawInfo>();

                foreach (SpriterEntity entity in loader.Spriter.Entities)
                {
                    var animator = new MonoGameDebugAnimator(entity, GraphicsDevice, factory, drawInfoPool);
                    animators.Add(animator);
                    animator.Position = screenCentre;
                    animator.EventTriggered += x => Debug.WriteLine("Event Happened: " + x);
                }
            }

            animator = animators.First();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float deltaTime = gameTime.ElapsedGameTime.Ticks / (float)TimeSpan.TicksPerMillisecond;

            stats.OnUpdate(deltaTime);
            Vector2 scale = animator.Scale;

            if (WasPressed(Keys.Enter)) SwitchEntity();
            if (WasPressed(Keys.Space)) animator.Play(animator.GetNextAnimation());
            if (WasPressed(Keys.O)) animator.ChangeAnimationSpeed(-0.2f, 5.0f);
            if (WasPressed(Keys.P)) animator.ChangeAnimationSpeed(0.2f, 5.0f);
            if (WasPressed(Keys.R)) animator.Speed = -animator.Speed;
            if (WasPressed(Keys.X)) animator.Play(animator.Name);
            if (WasPressed(Keys.T)) animator.Transition(animator.GetNextAnimation(), 1000.0f);
            if (WasPressed(Keys.C)) animator.PushNextCharacterMap();
            if (WasPressed(Keys.V)) animator.PopCharacterMap();
            if (WasPressed(Keys.J)) animator.Color = animator.Color == Color.White ? Color.Red : Color.White;

            if (WasPressed(Keys.W)) animator.Position += new Vector2(0, -10);
            if (WasPressed(Keys.S)) animator.Position += new Vector2(0, 10);
            if (WasPressed(Keys.A)) animator.Position += new Vector2(-10, 0);
            if (WasPressed(Keys.D)) animator.Position += new Vector2(10, 0);
            if (WasPressed(Keys.Q)) animator.Rotation -= 15 * (float)Math.PI / 180;
            if (WasPressed(Keys.E)) animator.Rotation += 15 * (float)Math.PI / 180;
            if (WasPressed(Keys.N)) animator.Scale -= new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f);
            if (WasPressed(Keys.M)) animator.Scale += new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f);
            if (WasPressed(Keys.F)) animator.Scale *= new Vector2(-1, 1);
            if (WasPressed(Keys.G)) animator.Scale *= new Vector2(1, -1);

            if (WasPressed(Keys.OemTilde)) (animator as MonoGameDebugAnimator).DrawSpriteOutlines = !(animator as MonoGameDebugAnimator).DrawSpriteOutlines;
            if (WasPressed(Keys.OemMinus))
            {
                graphics.SynchronizeWithVerticalRetrace = !graphics.SynchronizeWithVerticalRetrace;
                graphics.ApplyChanges();
            }

            oldState = Keyboard.GetState();

            animator.Update(deltaTime);

            elapsedTime += deltaTime;
            if (elapsedTime >= 100)
            {
                elapsedTime -= 100;
                string entity = animator.Entity.Name;
                animatorInfo = string.Format("{0} : {1}", entity, animator.Name);
                metadata = string.Format("Variables:\n{0}\nTags:\n", animator.GetVarValues(), animator.GetTagValues());
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            stats.OnDraw();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront);

            animator.Draw(spriteBatch);

            DrawText("FPS: " + stats.DrawRate, new Vector2(WindowWidth - 100, 10), 0.6f, Color.Yellow);

            DrawText(Constants.KeyBindings, new Vector2(10, 10), 0.6f, Color.Black);
            DrawText(animatorInfo, new Vector2(10, WindowHeight - 50), 1.0f, Color.Black);
            DrawText(metadata, new Vector2(WindowWidth - 300, WindowHeight * 0.5f), 0.6f, Color.Black);
            spriteBatch.End();
        }

        private void DrawText(string text, Vector2 position, float size, Color color)
        {
            spriteBatch.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, size, SpriteEffects.None, 0.0f);
        }

        private bool WasPressed(Keys key)
        {
            KeyboardState state = Keyboard.GetState();
            return oldState.IsKeyUp(key) && state.IsKeyDown(key);
        }

        private void SwitchEntity()
        {
            int index = animators.IndexOf(animator);
            ++index;
            if (index >= animators.Count) index = 0;
            animator = animators[index];
        }
    }
}