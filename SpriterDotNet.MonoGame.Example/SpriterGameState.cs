// Copyright (c) 2015 The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpriterDotNet.MonoGame.Content;
using SpriterDotNet.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpriterDotNet.MonoGame.Example
{
    public class SpriterGameState : GameState
    {
        private static readonly string FontName = "Status";

        private static readonly IList<string> Scmls = new List<string>
        {
            "AtlasExample/0",
            "GreyGuy/player",
            "TestSquares/squares",
            "GreyGuyPlus/player_006"
        };

        private static readonly IList<string> Instructions = new List<string>
        {
            "Enter = Next Scml",
            "Space = Next Animation",
            "O/P = Change Anim Speed",
            "R = Reverse Direction",
            "X = Reset Animation",
            "T = Transition to Next Animation",
            "C/V = Push/Pop CharMap",
            "W/A/S/D = Move",
            "Q/E = Rotate",
            "N/M = Scale",
            "F/G = Flip",
            "J = Toggle Colour"
        };

        private static readonly Config config = new Config
        {
            MetadataEnabled = true,
            EventsEnabled = true,
            PoolingEnabled = true,
            TagsEnabled = true,
            VarsEnabled = true,
            SoundsEnabled = false
        };

        private static readonly float MaxSpeed = 5.0f;
        private static readonly float DeltaSpeed = 0.2f;

        private IList<MonoGameAnimator> animators = new List<MonoGameAnimator>();
        private MonoGameAnimator currentAnimator;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private KeyboardState oldState;
        private string rtfm = string.Join(Environment.NewLine, Instructions);
        private string status = string.Empty;
        private string metadata = string.Empty;
        private Stats stats = new Stats();

        private Vector2 centre;

        protected override void Load()
        {
            centre = new Vector2(Width / 2.0f, Height / 2.0f);
            oldState = Keyboard.GetState();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteFont = Content.Load<SpriteFont>(FontName);
            DefaultProviderFactory<ISprite, SoundEffect> factory = new DefaultProviderFactory<ISprite, SoundEffect>(config, true);

            foreach (string scmlPath in Scmls)
            {
                SpriterContentLoader loader = new SpriterContentLoader(Content, scmlPath);
                loader.Fill(factory);

                Stack<SpriteDrawInfo> drawInfoPool = new Stack<SpriteDrawInfo>();

                foreach (SpriterEntity entity in loader.Spriter.Entities)
                {
                    var animator = new MonoGameDebugAnimator(entity, GraphicsDevice, factory, drawInfoPool);
                    animators.Add(animator);
                    animator.Position = centre;
                }
            }

            currentAnimator = animators.First();
            currentAnimator.EventTriggered += CurrentAnimator_EventTriggered;
        }

        public override void Draw(float deltaTime)
        {
            stats.OnDraw();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.BackToFront);

            currentAnimator.Draw(spriteBatch);

            DrawText("FPS: " + stats.DrawRate, new Vector2(Width - 100, 10), 0.6f, Color.Yellow);

            DrawText(rtfm, new Vector2(10, 10), 0.6f, Color.Black);
            DrawText(status, new Vector2(10, Height - 50), 1.0f, Color.Black);
            DrawText(metadata, new Vector2(Width - 300, Height * 0.5f), 0.6f, Color.Black);
            spriteBatch.End();
        }

        public override void Update(float deltaTime)
        {
            stats.OnUpdate(deltaTime);
            Vector2 scale = currentAnimator.Scale;

            if (IsPressed(Keys.Enter)) SwitchEntity();
            if (IsPressed(Keys.Space)) currentAnimator.Play(GetNextAnimation());
            if (IsPressed(Keys.P)) ChangeAnimationSpeed(DeltaSpeed);
            if (IsPressed(Keys.O)) ChangeAnimationSpeed(-DeltaSpeed);
            if (IsPressed(Keys.R)) currentAnimator.Speed = -currentAnimator.Speed;
            if (IsPressed(Keys.X)) currentAnimator.Play(currentAnimator.Name);
            if (IsPressed(Keys.T)) currentAnimator.Transition(GetNextAnimation(), 1000.0f);
            if (IsPressed(Keys.C)) PushCharacterMap();
            if (IsPressed(Keys.V)) currentAnimator.SpriteProvider.PopCharMap();
            if (IsPressed(Keys.J)) currentAnimator.Color = currentAnimator.Color == Color.White ? Color.Red : Color.White;

            if (IsPressed(Keys.W)) currentAnimator.Position += new Vector2(0, -10);
            if (IsPressed(Keys.S)) currentAnimator.Position += new Vector2(0, 10);
            if (IsPressed(Keys.A)) currentAnimator.Position += new Vector2(-10, 0);
            if (IsPressed(Keys.D)) currentAnimator.Position += new Vector2(10, 0);
            if (IsPressed(Keys.Q)) currentAnimator.Rotation -= 15 * (float)Math.PI / 180;
            if (IsPressed(Keys.E)) currentAnimator.Rotation += 15 * (float)Math.PI / 180;
            if (IsPressed(Keys.N)) currentAnimator.Scale -= new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f);
            if (IsPressed(Keys.M)) currentAnimator.Scale += new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f);
            if (IsPressed(Keys.F)) currentAnimator.Scale *= new Vector2(-1, 1);
            if (IsPressed(Keys.G)) currentAnimator.Scale *= new Vector2(1, -1);

            oldState = Keyboard.GetState();

            currentAnimator.Update(deltaTime);

            string entity = currentAnimator.Entity.Name;
            status = string.Format("{0} : {1}", entity, currentAnimator.Name);
            metadata = "Variables:\n" + GetVarValues() + "\nTags:\n" + GetTagValues();
        }

        private void DrawText(string text, Vector2 position, float size, Color color)
        {
            spriteBatch.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, size, SpriteEffects.None, 0.0f);
        }

        private void PushCharacterMap()
        {
            SpriterCharacterMap[] maps = currentAnimator.Entity.CharacterMaps;
            if (maps == null || maps.Length == 0) return;
            SpriterCharacterMap charMap = currentAnimator.SpriteProvider.CharacterMap;
            if (charMap == null) charMap = maps[0];
            else
            {
                int index = charMap.Id + 1;
                if (index >= maps.Length) charMap = null;
                else charMap = maps[index];
            }

            if (charMap != null) currentAnimator.SpriteProvider.PushCharMap(charMap);
        }

        private string GetVarValues()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in currentAnimator.FrameData.AnimationVars)
            {
                object value = GetValue(entry.Value);
                sb.Append(entry.Key).Append(" = ").AppendLine(value.ToString());
            }
            foreach (var objectEntry in currentAnimator.FrameData.ObjectVars)
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
            StringBuilder sb = new StringBuilder();
            foreach (string tag in currentAnimator.FrameData.AnimationTags) sb.AppendLine(tag);
            foreach (var objectEntry in currentAnimator.FrameData.ObjectTags)
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

        private void CurrentAnimator_EventTriggered(string obj)
        {
            System.Diagnostics.Debug.WriteLine(obj);
        }

        protected override void Unload()
        {
            Content.Unload();
        }
    }
}
