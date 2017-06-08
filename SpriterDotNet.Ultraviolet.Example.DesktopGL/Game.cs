using System;
using System.IO;
using System.Text;
using UltravioletGame1.Input;
using TwistedLogik.Nucleus;
using TwistedLogik.Nucleus.Text;
using TwistedLogik.Ultraviolet;
using TwistedLogik.Ultraviolet.Content;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D.Text;
using TwistedLogik.Ultraviolet.OpenGL;
using TwistedLogik.Ultraviolet.Platform;
using System.Collections.Generic;
using SpriterDotNet;
using SpriterDotNet.Ultraviolet;
using System.Linq;
using SpriterDotNet.Providers;
using TwistedLogik.Ultraviolet.Audio;
using SpriterDotNet.Ultraviolet.Content;

namespace UltravioletGame1
{
    public class Game : UltravioletApplication
    {
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
            "J = Toggle Colour",
            "~ = Toggle Sprite Outlines"
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

        private IList<UltravioletAnimator> animators = new List<UltravioletAnimator>();
        private UltravioletAnimator currentAnimator;

        private string rtfm = string.Join(Environment.NewLine, Instructions);
        private string status = string.Empty;
        private string metadata = string.Empty;

        private ContentManager content;

        private CursorCollection cursors;
        private SpriteFont spriteFont;
        private SpriteBatch spriteBatch;
        private TextRenderer textRenderer;
        private StringFormatter textFormatter;
        private StringBuilder textBuffer;

        private float elapsedTime;

        public Game() : base("loodakrawa", "SpriterDotNet.Example")
        {
            IsFixedTimeStep = false;
        }

        protected override UltravioletContext OnCreatingUltravioletContext()
        {
            var configuration = new OpenGLUltravioletConfiguration();
            PopulateConfiguration(configuration);

#if DEBUG
            configuration.Debug = true;
            configuration.DebugLevels = DebugLevels.Error | DebugLevels.Warning;
            configuration.DebugCallback = (uv, level, message) =>
            {
                System.Diagnostics.Debug.WriteLine(message);
            };
#endif
            return new OpenGLUltravioletContext(this, configuration);
        }

        protected override void OnInitialized()
        {
            SetFileSourceFromManifestIfExists("UltravioletGame.Content.uvarc");

            base.OnInitialized();
        }

        protected override void OnLoadingContent()
        {
            content = ContentManager.Create("Content");

            LoadLocalizationDatabases();
            LoadCursors();

            spriteBatch = SpriteBatch.Create();
            spriteFont = content.Load<SpriteFont>("Fonts/SegoeUI");

            textRenderer = new TextRenderer();
            textFormatter = new StringFormatter();
            textBuffer = new StringBuilder();


            DefaultProviderFactory<ISprite, SoundEffect> factory = new DefaultProviderFactory<ISprite, SoundEffect>(config, true);

            foreach (string scmlPath in Scmls)
            {
                SpriterContentLoader loader = new SpriterContentLoader(content, scmlPath);
                loader.Fill(factory);

                Stack<SpriteDrawInfo> drawInfoPool = new Stack<SpriteDrawInfo>();

                foreach (SpriterEntity entity in loader.Spriter.Entities)
                {
                    var animator = new UltravioletAnimator(entity, factory, drawInfoPool);
                    animators.Add(animator);
                }
            }

            currentAnimator = animators.First();
            currentAnimator.EventTriggered += CurrentAnimator_EventTriggered;

            GC.Collect(2);

            base.OnLoadingContent();
        }

        protected void LoadLocalizationDatabases()
        {
            var fss = FileSystemService.Create();
            var databases = content.GetAssetFilePathsInDirectory("Localization", "*.xml");
            foreach (var database in databases)
            {
                using (var stream = fss.OpenRead(database))
                {
                    Localization.Strings.LoadFromStream(stream);
                }
            }
        }

        protected void LoadCursors()
        {
            cursors = content.Load<CursorCollection>("Cursors/Cursors");
            Ultraviolet.GetPlatform().Cursor = cursors["Normal"];
        }

        protected override void OnUpdating(UltravioletTime time)
        {
            Vector2 scale = currentAnimator.Scale;

            var actions = Ultraviolet.GetInput().GetActions();

            if (actions.SwitchEntity.IsPressed()) SwitchEntity();
            if (actions.NextAnimation.IsPressed()) currentAnimator.Play(GetNextAnimation());
            if (actions.IncreaseSpeed.IsPressed()) ChangeAnimationSpeed(DeltaSpeed);
            if (actions.DecreaseSpeed.IsPressed()) ChangeAnimationSpeed(-DeltaSpeed);
            if (actions.ReverseAnimation.IsPressed()) currentAnimator.Speed = -currentAnimator.Speed;
            if (actions.Transition.IsPressed()) currentAnimator.Transition(GetNextAnimation(), 1000.0f);
            if (actions.PushCharMap.IsPressed()) PushCharacterMap();
            if (actions.PopCharMap.IsPressed()) currentAnimator.SpriteProvider.PopCharMap();
            if (actions.ToggleColour.IsPressed()) currentAnimator.Color = currentAnimator.Color == Color.White ? Color.Red : Color.White;
            if (actions.MoveUp.IsPressed()) currentAnimator.Position += new Vector2(0, -10);
            if (actions.MoveDown.IsPressed()) currentAnimator.Position += new Vector2(0, 10);
            if (actions.MoveLeft.IsPressed()) currentAnimator.Position += new Vector2(-10, 0);
            if (actions.MoveRight.IsPressed()) currentAnimator.Position += new Vector2(10, 0);
            if (actions.RotateLeft.IsPressed()) currentAnimator.Rotation -= 15 * (float)Math.PI / 180;
            if (actions.RotateRight.IsPressed()) currentAnimator.Rotation += 15 * (float)Math.PI / 180;
            if (actions.ScaleUp.IsPressed()) currentAnimator.Scale += new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f);
            if (actions.ScaleDown.IsPressed()) currentAnimator.Scale -= new Vector2(Math.Sign(scale.X) * 0.2f, Math.Sign(scale.Y) * 0.2f); ;
            if (actions.FlipX.IsPressed()) currentAnimator.Scale *= new Vector2(-1, 1);
            if (actions.FlipY.IsPressed()) currentAnimator.Scale *= new Vector2(1, -1);
            //if (actions.DrawOutlines.IsPressed()) (currentAnimator as MonoGameDebugAnimator).DrawSpriteOutlines = !(currentAnimator as MonoGameDebugAnimator).DrawSpriteOutlines;

            float deltaTime = time.ElapsedTime.Ticks / (float)TimeSpan.TicksPerMillisecond;

            currentAnimator.Update(deltaTime);

            elapsedTime += deltaTime;
            if (elapsedTime >= 100)
            {
                elapsedTime -= 100;
                string entity = currentAnimator.Entity.Name;
                status = string.Format("{0} : {1}", entity, currentAnimator.Name);
                metadata = string.Format("Variables:\n{0}\nTags:\n", GetVarValues(), GetTagValues());
            }

            base.OnUpdating(time);
        }

        protected override void OnDrawing(UltravioletTime time)
        {
            textFormatter.Reset();
            textFormatter.AddArgument(Ultraviolet.GetGraphics().FrameRate);
            textFormatter.AddArgument(GC.GetTotalMemory(false) / 1024);
            textFormatter.AddArgument(Environment.Is64BitProcess ? "64-bit" : "32-bit");
            textFormatter.Format("{0:decimals:2} FPS\nAllocated: {1:decimals:2} kb\n{2}", textBuffer);

            Size2 size = Ultraviolet.GetPlatform().Windows.GetCurrent().ClientSize;

            currentAnimator.Position = new Vector2(size.Width, size.Height) / 2;           

            spriteBatch.Begin();
            currentAnimator.Draw(spriteBatch);
            spriteBatch.DrawString(spriteFont, textBuffer, new Vector2(size.Width - 150, 10), Color.White);
            spriteBatch.DrawString(spriteFont, rtfm, Vector2.One * 8, Color.White);
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
    }
}
