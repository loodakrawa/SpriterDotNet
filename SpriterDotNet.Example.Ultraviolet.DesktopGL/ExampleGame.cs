// Copyright (C) The original author or authors
//
// This software may be modified and distributed under the terms
// of the zlib license.  See the LICENSE file for details.

using SpriterDotNet.Providers;
using SpriterDotNet.Ultraviolet;
using SpriterDotNet.Ultraviolet.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TwistedLogik.Nucleus;
using TwistedLogik.Nucleus.Text;
using TwistedLogik.Ultraviolet;
using TwistedLogik.Ultraviolet.Audio;
using TwistedLogik.Ultraviolet.Content;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D.Text;
using TwistedLogik.Ultraviolet.Input;
using TwistedLogik.Ultraviolet.OpenGL;

namespace SpriterDotNet.Example.Ultraviolet.DesktopGL
{
    public class ExampleGame : UltravioletApplication
    {
        private IList<UltravioletAnimator> animators = new List<UltravioletAnimator>();
        private UltravioletAnimator animator;

        private string status = string.Empty;
        private string metadata = string.Empty;

        private ContentManager content;

        private SpriteFont spriteFont;
        private SpriteBatch spriteBatch;
        private TextRenderer textRenderer;
        private StringFormatter textFormatter;
        private StringBuilder textBuffer;

        private float elapsedTime;

        private static readonly UltravioletSingleton<GameInputActions> actions = InputActionCollection.CreateSingleton<GameInputActions>();
        private static GameInputActions Actions => actions;

        public ExampleGame() : base("loodakrawa", "SpriterDotNet.Example")
        {
            IsFixedTimeStep = false;
        }

        protected override UltravioletContext OnCreatingUltravioletContext()
        {
            var configuration = new OpenGLUltravioletConfiguration();
            PopulateConfiguration(configuration);
           
            return new OpenGLUltravioletContext(this, configuration);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            SynchronizeWithVerticalRetrace = true;
        }

        protected override void OnLoadingContent()
        {
            base.OnLoadingContent();
            
            content = ContentManager.Create("Content");

            spriteBatch = SpriteBatch.Create();
            spriteFont = content.Load<SpriteFont>("Fonts/SegoeUI");

            textRenderer = new TextRenderer();
            textFormatter = new StringFormatter();
            textBuffer = new StringBuilder();


            DefaultProviderFactory<ISprite, SoundEffect> factory = new DefaultProviderFactory<ISprite, SoundEffect>(Constants.Config, true);

            foreach (string scmlPath in Constants.ScmlFiles)
            {
                SpriterContentLoader loader = new SpriterContentLoader(content, scmlPath);
                loader.Fill(factory);

                Stack<SpriteDrawInfo> drawInfoPool = new Stack<SpriteDrawInfo>();

                foreach (SpriterEntity entity in loader.Spriter.Entities)
                {
                    var animator = new UltravioletAnimator(entity, factory, drawInfoPool);
                    animators.Add(animator);
                    animator.EventTriggered += x => Debug.WriteLine("Event Happened: " + x);
                }
            }

            animator = animators.First();

            GC.Collect(2);
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            Vector2 scale = animator.Scale;

            if (Actions.SwitchEntity.IsPressed()) SwitchEntity();
            if (Actions.NextAnimation.IsPressed()) animator.Play(animator.GetNextAnimation());
            if (Actions.DecreaseSpeed.IsPressed()) animator.ChangeAnimationSpeed(-0.2f, 5.0f);
            if (Actions.IncreaseSpeed.IsPressed()) animator.ChangeAnimationSpeed(0.2f, 5.0f);
            if (Actions.ReverseAnimation.IsPressed()) animator.Speed = -animator.Speed;
            if (Actions.Transition.IsPressed()) animator.Transition(animator.GetNextAnimation(), 1000.0f);
            if (Actions.PushCharMap.IsPressed()) animator.PushNextCharacterMap();
            if (Actions.PopCharMap.IsPressed()) animator.SpriteProvider.PopCharMap();
            if (Actions.ToggleColour.IsPressed()) animator.Color = animator.Color == Color.White ? Color.Red : Color.White;
            if (Actions.MoveUp.IsPressed()) animator.Position += new Vector2(0, -10);
            if (Actions.MoveDown.IsPressed()) animator.Position += new Vector2(0, 10);
            if (Actions.MoveLeft.IsPressed()) animator.Position += new Vector2(-10, 0);
            if (Actions.MoveRight.IsPressed()) animator.Position += new Vector2(10, 0);
            if (Actions.RotateLeft.IsPressed()) animator.Rotation -= 15 * (float)Math.PI / 180;
            if (Actions.RotateRight.IsPressed()) animator.Rotation += 15 * (float)Math.PI / 180;
            if (Actions.ScaleUp.IsPressed()) animator.Scale += new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f);
            if (Actions.ScaleDown.IsPressed()) animator.Scale -= new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f); ;
            if (Actions.FlipX.IsPressed()) animator.Scale *= new Vector2(-1, 1);
            if (Actions.FlipY.IsPressed()) animator.Scale *= new Vector2(1, -1);
            //if (actions.DrawOutlines.IsPressed()) (currentAnimator as MonoGameDebugAnimator).DrawSpriteOutlines = !(currentAnimator as MonoGameDebugAnimator).DrawSpriteOutlines;
            if (Actions.ToggleVSync.IsPressed()) SynchronizeWithVerticalRetrace = !SynchronizeWithVerticalRetrace;

            float deltaTime = time.ElapsedTime.Ticks / (float)TimeSpan.TicksPerMillisecond;

            animator.Update(deltaTime);

            elapsedTime += deltaTime;
            if (elapsedTime >= 100)
            {
                elapsedTime -= 100;
                string entity = animator.Entity.Name;
                status = string.Format("{0} : {1}", entity, animator.Name);
                metadata = string.Format("Variables:\n{0}\nTags:\n", animator.GetVarValues(), animator.GetTagValues());

                textFormatter.Reset();
                textFormatter.AddArgument(Ultraviolet.GetGraphics().FrameRate);
                textFormatter.AddArgument(GC.GetTotalMemory(false) / 1024);
                textFormatter.AddArgument(Environment.Is64BitProcess ? "64-bit" : "32-bit");
                textFormatter.Format("{0:decimals:2} FPS\nAllocated: {1:decimals:2} kb\n{2}", textBuffer);
            }

            base.OnUpdating(time);
        }

        protected override void OnDrawing(UltravioletTime time)
        {
            Size2 size = Ultraviolet.GetPlatform().Windows.GetCurrent().ClientSize;

            animator.Position = new Vector2(size.Width, size.Height) / 2;           

            spriteBatch.Begin();
            animator.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, textBuffer, new Vector2(size.Width - 150, 10), Color.White);
            spriteBatch.DrawString(spriteFont, Constants.KeyBindings, Vector2.One * 8, Color.White);
            spriteBatch.DrawString(spriteFont, status, new Vector2(10, size.Height - 50), Color.Black);
            spriteBatch.DrawString(spriteFont, metadata, new Vector2(size.Width - 300, size.Height * 0.5f), Color.Black);
            spriteBatch.End();

            base.OnDrawing(time);
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) SafeDispose.DisposeRef(ref content);
            base.Dispose(disposing);
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
