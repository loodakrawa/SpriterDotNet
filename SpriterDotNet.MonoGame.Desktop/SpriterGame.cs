// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriterDotNet.Monogame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpriterDotNet.MonoGame.Desktop
{
    public class SpriteGame : Game
    {
        private static readonly string RootDirectory = "Content";
        private static readonly string FontName = "Status";

        private static readonly IDictionary<string, string> Scmls = new Dictionary<string, string>
        {
            { "Content/GreyGuy/player.scml", "GreyGuy"},
            { "Content/TestSquares/squares.scml", "TestSquares"},
            { "Content/GreyGuyPlus/player_006.scml", "GreyGuyPlus"}
        };

        private static readonly int Width = 1280;
        private static readonly int Height = 960;
        private static readonly float MaxSpeed = 5.0f;
        private static readonly float DeltaSpeed = 0.2f;
        private static readonly string Instructions = "Enter = Next Scml\nSpace = Next Animation\nP = Anim Speed +\nO = Anim Speed -\n" + 
            "R = Reverse Direction\nX = Reset Animation\nT = Transition to Next Animation\nC = Push Next CharMap\nV = Pop CharMap";

        private IList<MonogameSpriterAnimator> animators = new List<MonogameSpriterAnimator>();
        private MonogameSpriterAnimator currentAnimator;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private KeyboardState oldState;
        private string status;
        private string metadata;
        private Fps fps = new Fps();

        private Vector2 centre = new Vector2(Width / 2.0f, Height / 2.0f);

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
            base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>(FontName);
            Texture2D debugTexture = new Texture2D(GraphicsDevice, 1, 1);

            foreach (var pair in Scmls)
            {
                string scmlPath = pair.Key;
                string spriterName = pair.Value;
                string data = File.ReadAllText(scmlPath);
                Spriter spriter = SpriterParser.Parse(data);

                foreach (SpriterEntity entity in spriter.Entities)
                {
                    var animator = new MonogameDebugSpriterAnimator(entity, GraphicsDevice);
                    RegisterTextures(animator, spriter, spriterName);
                    animators.Add(animator);
                    animator.Position = centre;
                    animator.Scale = new Vector2(-1, 1);
                }
            }

            currentAnimator = animators.First();
            currentAnimator.EventTriggered += CurrentAnimator_EventTriggered;
        }

        private void CurrentAnimator_EventTriggered(string obj)
        {
            System.Diagnostics.Debug.WriteLine(obj);
        }

        protected override void Draw(GameTime gameTime)
        {
            fps.OnDraw(gameTime);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            currentAnimator.Draw(spriteBatch);

            DrawText(String.Format("FPS (Update) = {0}\nFPS (Draw) =    {1}", fps.UpdateFps, fps.DrawFps), new Vector2(Width - 200, 10), 0.6f);
            DrawText(Instructions, new Vector2(10, 10), 0.6f);
            DrawText(status, new Vector2(10, Height - 50));
            DrawText(metadata, new Vector2(Width - 300, Height * 0.5f), 0.6f);
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

            currentAnimator.Rotation -= 1;

            if (IsPressed(Keys.Enter)) SwitchEntity();
            if (IsPressed(Keys.Space)) currentAnimator.Play(GetNextAnimation());
            if (IsPressed(Keys.P)) ChangeAnimationSpeed(DeltaSpeed);
            if (IsPressed(Keys.O)) ChangeAnimationSpeed(-DeltaSpeed);
            if (IsPressed(Keys.R)) currentAnimator.Speed = -currentAnimator.Speed;
            if (IsPressed(Keys.X)) currentAnimator.Play(currentAnimator.Name);
            if (IsPressed(Keys.T)) currentAnimator.Transition(GetNextAnimation(), 1000.0f);
            if (IsPressed(Keys.C)) PushCharacterMap();
            if (IsPressed(Keys.V)) currentAnimator.PopCharMap();

            oldState = Keyboard.GetState();

            foreach(var animator in animators) animator.Step(gameTime.ElapsedGameTime.Milliseconds);
            
            string entity = currentAnimator.Entity.Name;
            status = String.Format("{0} : {1}", entity, currentAnimator.Name);
            metadata = "Variables:\n" + GetVarValues() + "\nTags:\n" + GetTagValues();
            base.Update(gameTime);
        }

        private void PushCharacterMap()
        {
            SpriterCharacterMap[] maps = currentAnimator.Entity.CharacterMaps;
            if (maps == null || maps.Length == 0) return;
            SpriterCharacterMap charMap = currentAnimator.CharacterMap;
            if (charMap == null) charMap = maps[0];
            else
            {
                int index = charMap.Id + 1;
                if (index >= maps.Length) charMap = null;
                else charMap = maps[index];
            }

            if(charMap != null) currentAnimator.PushCharMap(charMap);
        }

        private string GetVarValues()
        {
            FrameMetadata metadata = currentAnimator.Metadata;

            StringBuilder sb = new StringBuilder();
            foreach (var entry in metadata.AnimationVars)
            {
                object value = GetValue(entry.Value);
                sb.Append(entry.Key).Append(" = ").AppendLine(value.ToString());
            }
            foreach (var objectEntry in metadata.ObjectVars)
            {
                foreach (var varEntry in objectEntry.Value)
                {
                    object value = GetValue(varEntry.Value);
                    sb.Append(objectEntry.Key).Append(".").Append(varEntry.Key).Append(" = ").AppendLine((value ?? string.Empty).ToString());
                }
            }

            return sb.ToString();
        }

        private object GetValue(SpriterVarValue varValue)
        {
            object value;
            switch (varValue.Type)
            {
                case SpriterVarType.Float:
                    value = varValue.FloatValue;
                    break;
                case SpriterVarType.Int:
                    value = varValue.IntValue;
                    break;
                default:
                    value = varValue.StringValue;
                    break;
            }
            return value;
        }

        private string GetTagValues()
        {
            FrameMetadata metadata = currentAnimator.Metadata;

            StringBuilder sb = new StringBuilder();
            foreach (string tag in metadata.AnimationTags) sb.AppendLine(tag);
            foreach (var objectEntry in metadata.ObjectTags)
            {
                foreach (string tag in objectEntry.Value) sb.Append(objectEntry.Key).Append(".").AppendLine(tag);
            }

            return sb.ToString();
        }

        private bool IsPressed(Keys key)
        {
            KeyboardState state = Keyboard.GetState();
            return oldState.IsKeyUp(key) && state.IsKeyDown(key);
        }

        private void SwitchEntity()
        {
            int index = animators.IndexOf(currentAnimator);
            ++index;
            if (index >= animators.Count) index = 0;
            currentAnimator = animators[index];
        }

        private string GetNextAnimation()
        {
            List<string> animations = currentAnimator.GetAnimations().ToList();
            int index = animations.IndexOf(currentAnimator.CurrentAnimation.Name);
            ++index;
            if (index >= animations.Count) index = 0;
            return animations[index];
        }

        private void ChangeAnimationSpeed(float delta)
        {
            var speed = currentAnimator.Speed + delta;
            speed = Math.Abs(speed) < MaxSpeed ? speed : MaxSpeed * Math.Sign(speed);
            currentAnimator.Speed = speed;
        }

        private void RegisterTextures(MonogameSpriterAnimator animator, Spriter spriter, string spriterName)
        {
            foreach (SpriterFolder folder in spriter.Folders)
            {
                foreach (SpriterFile file in folder.Files)
                {
                    string path = FormatPath(folder, file, spriterName);

                    if (file.Type == SpriterFileType.Sound)
                    {
                        SoundEffect sound = LoadContent<SoundEffect>(path);
                        animator.Register(folder.Id, file.Id, sound);
                    }
                    else
                    {
                        Texture2D texture = LoadContent<Texture2D>(path);
                        if (texture != null) animator.Register(folder.Id, file.Id, texture);
                    }

                }
            }
        }

        private T LoadContent<T>(string path)
        {
            T asset = default(T);
            try
            {
                asset = Content.Load<T>(path);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Missing Asset: " + path);
            }

            return asset;
        }

        private string FormatPath(SpriterFolder folder, SpriterFile file, string spriterName)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (String.IsNullOrEmpty(folder.Name)) return String.Format("{0}/{1}", spriterName, fileName);
            return String.Format("{0}/{1}/{2}", spriterName, folder.Name, fileName);
        }
    }
}