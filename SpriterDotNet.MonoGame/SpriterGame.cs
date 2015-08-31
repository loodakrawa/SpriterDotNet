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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SpriterDotNet.MonoGame
{
    public class SpriteGame : Game
    {
        private static readonly string RootDirectory = "Content";
        private static readonly string FontName = "Status";

        private static readonly IDictionary<string, string> Scmls = new Dictionary<string, string>
        {
            { "Content/GreyGuy/player.scml", "GreyGuy"},
            { "Content/TestSquares/squares.scml", "TestSquares"}
        };

        private static readonly int Width = 1024;
        private static readonly int Height = 768;
        private static readonly float MaxSpeed = 5.0f;
        private static readonly float DeltaSpeed = 0.2f;
        private static readonly string Instructions = "Enter = Next Scml\nSpace = Next Animation\nP = Anim Speed +\nO = Anim Speed -\nR = Reverse Direction\nM = Update Speed +\nN = Update Speed -";

        private IList<MonogameSpriterAnimator> animators = new List<MonogameSpriterAnimator>();
        private MonogameSpriterAnimator currentAnimator;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private KeyboardState oldState;
        private string status;
        private Fps fps = new Fps();

        public SpriteGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            Content.RootDirectory = RootDirectory;
        }

        protected override void Initialize()
        {
            base.Initialize();

            oldState = Keyboard.GetState();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>(FontName);
            Vector2 charPosition = new Vector2(Width / 2.0f, Height / 1.5f);

            foreach (var pair in Scmls)
            {
                string scmlPath = pair.Key;
                string spriterName = pair.Value;
                string data = File.ReadAllText(scmlPath);
                Spriter spriter = Spriter.Parse(data);

                var animator = new MonogameSpriterAnimator(spriter.Entities[0], charPosition, spriteBatch);
                RegisterTextures(animator, spriter, spriterName);

                animators.Add(animator);
            }

            currentAnimator = animators.First();
        }

        protected override void Draw(GameTime gameTime)
        {
            fps.OnDraw(gameTime);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred);

            DrawText(String.Format("FPS (Update) = {0}\nFPS (Draw) =    {1}", fps.UpdateFps, fps.DrawFps), new Vector2(Width - 200, 10), 0.6f);
            DrawText(Instructions, new Vector2(10, 10), 0.6f);
            DrawText(status, new Vector2(10, Height - 50));
            currentAnimator.Step((float)gameTime.ElapsedGameTime.Milliseconds);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawText(string text, Vector2 position, float size = 1.0f)
        {
            spriteBatch.DrawString(spriteFont, text, position, Color.Black, 0, Vector2.Zero, size, SpriteEffects.None, 0.0f);
        }

        protected override void Update(GameTime gameTime)
        {
            fps.OnUpdate(gameTime);

            if (IsPressed(Keys.Enter)) SwitchScml();
            if (IsPressed(Keys.Space)) NextAnimation();
            if (IsPressed(Keys.P)) ChangeAnimationSpeed(DeltaSpeed);
            if (IsPressed(Keys.O)) ChangeAnimationSpeed(-DeltaSpeed);
            if (IsPressed(Keys.R)) currentAnimator.Speed = -currentAnimator.Speed;
            if (IsPressed(Keys.M)) ChangeUpdateSpeed(true);
            if (IsPressed(Keys.N)) ChangeUpdateSpeed(false);

            oldState = Keyboard.GetState();

            string spriter = Scmls[Scmls.Keys.ElementAt(animators.IndexOf(currentAnimator))];
            status = String.Format("{0} : {1}", spriter, currentAnimator.Name);

            base.Update(gameTime);
        }

        private bool IsPressed(Keys key)
        {
            KeyboardState state = Keyboard.GetState();
            return oldState.IsKeyUp(key) && state.IsKeyDown(key);
        }

        private void SwitchScml()
        {
            int index = animators.IndexOf(currentAnimator);
            ++index;
            if (index >= animators.Count) index = 0;
            currentAnimator = animators[index];
        }

        private void NextAnimation()
        {
            List<string> animations = currentAnimator.GetAnimations().ToList();
            int index = animations.IndexOf(currentAnimator.CurrentAnimation.Name);
            ++index;
            if (index >= animations.Count) index = 0;
            currentAnimator.Play(animations[index]);
        }

        private void ChangeAnimationSpeed(float delta)
        {
            var speed = currentAnimator.Speed + delta;
            speed = Math.Abs(speed) < MaxSpeed ? speed : MaxSpeed * Math.Sign(speed);
            currentAnimator.Speed = speed;
        }

        private void ChangeUpdateSpeed(bool up)
        {
            int amount = up ? -1 : 1;
            int targetAmount = TargetElapsedTime.Milliseconds + amount;
            if (targetAmount == 0) targetAmount = 1;
            if (targetAmount > 40) targetAmount = 40;
            TargetElapsedTime = TimeSpan.FromMilliseconds(targetAmount);
        }

        private void RegisterTextures(MonogameSpriterAnimator animator, Spriter spriter, string spriterName)
        {
            foreach (SpriterFolder folder in spriter.Folders)
            {
                foreach (SpriterFile file in folder.Files)
                {
                    string path = FormatPath(folder, file, spriterName);
                    Texture2D texture = null;
                    try
                    {
                        texture = Content.Load<Texture2D>(path);
                    }
                    catch
                    {
                        Debug.WriteLine("Missing Texture: " + path);
                    }
                    if (texture == null) continue;

                    animator.Register(folder.Id, file.Id, texture);
                }
            }
        }

        private string FormatPath(SpriterFolder folder, SpriterFile file, string spriterName)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (String.IsNullOrEmpty(folder.Name)) return String.Format("{0}/{1}", spriterName, fileName);
            return String.Format("{0}/{1}/{2}", spriterName, folder.Name, fileName);
        }
    }
}