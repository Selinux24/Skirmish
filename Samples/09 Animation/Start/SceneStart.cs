using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Animation.Start
{
    class SceneStart : Scene
    {
        private const int layerHUD = 50;
        private const int layerCursor = 100;

        private Model backGround = null;
        private UITextArea title = null;
        private UIPanel mainPanel = null;

        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";

        private IAudioEffect currentMusic = null;

        public SceneStart(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            var assetTasks = new[] {
                InitializeCursor(),
                InitializeBackground(),
                InitializeTitle(),
                InitializeMainPanel(),
                InitializeMusic(),
            };

            await LoadResourcesAsync(assetTasks, PrepareAssets);
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = new UICursorDescription()
            {
                ContentPath = "Common",
                Textures = new[] { "start/resources/pointer.png" },
                Height = 48,
                Width = 48,
                Centered = false,
                Delta = new Vector2(-14f, -7f),
                BaseColor = Color.White,
            };
            await this.AddComponentUICursor("Cursor", cursorDesc, layerCursor);
        }
        private async Task InitializeBackground()
        {
            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("start/resources", "SkyPlane.xml"),
            };
            backGround = await this.AddComponentModel("Background", backGroundDesc, SceneObjectUsages.UI);
        }
        private async Task InitializeTitle()
        {
            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72);

            title = await this.AddComponentUITextArea("Title", UITextAreaDescription.Default(titleFont), layerHUD);
            title.GrowControlWithText = false;
            title.Text = "Animation";
            title.TextForeColor = Color.Gold;
            title.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            title.TextShadowDelta = new Vector2(4, 4);
            title.TextHorizontalAlign = HorizontalTextAlign.Center;
            title.TextVerticalAlign = VerticalTextAlign.Middle;
        }
        private async Task InitializeMainPanel()
        {
            mainPanel = await this.AddComponentUIPanel("MainPanel", UIPanelDescription.Default(Color.Transparent), layerHUD);
            mainPanel.Spacing = 10;
            mainPanel.Padding = 15;
            mainPanel.SetGridLayout(GridLayout.FixedRows(2));

            var buttonFont = TextDrawerDescription.FromFamily(buttonFonts, 36);
            Color4 highlightColor = new Color4(0.3333f, 0.3333f, 0.3333f, 0f);
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton(Color.Red, Color.Red.ToColor4() + highlightColor);
            buttonDesc.Font = buttonFont;
            buttonDesc.TextForeColor = Color.Gold;
            buttonDesc.TextHorizontalAlign = HorizontalTextAlign.Center;
            buttonDesc.TextVerticalAlign = VerticalTextAlign.Middle;
            var exitDesc = UIButtonDescription.DefaultTwoStateButton(Color.Orange, Color.Orange.ToColor4() + highlightColor);
            exitDesc.Font = buttonFont;
            exitDesc.TextForeColor = Color.Gold;
            exitDesc.TextHorizontalAlign = HorizontalTextAlign.Center;
            exitDesc.TextVerticalAlign = VerticalTextAlign.Middle;

            var emptyDesc = SpriteDescription.Default("start/resources/empty.png");

            var panSimpleAnimation = AddButtonPanel(buttonDesc, "Simple Animation", (sender, args) => { Game.SetScene<SimpleAnimation.SceneSimpleAnimation>(); });
            var panAnimationParts = AddButtonPanel(buttonDesc, "Animation Parts", (sender, args) => { Game.SetScene<AnimationParts.SceneAnimationParts>(); });
            var panExit = AddButtonPanel(exitDesc, "Exit", (sender, args) => { Game.Exit(); });

            mainPanel.AddChild(panSimpleAnimation, false);
            mainPanel.AddChild(new Sprite("Empty", this, emptyDesc), false);
            mainPanel.AddChild(new Sprite("Empty", this, emptyDesc), false);
            mainPanel.AddChild(new Sprite("Empty", this, emptyDesc), false);
            mainPanel.AddChild(new Sprite("Empty", this, emptyDesc), false);
            mainPanel.AddChild(panAnimationParts, false);
            mainPanel.AddChild(new Sprite("Empty", this, emptyDesc), false);
            mainPanel.AddChild(panExit, false);
        }
        private UIPanel AddButtonPanel(UIButtonDescription desc, string text, EventHandler buttonJustReleased)
        {
            var panel = new UIPanel($"MainPanel.Panel.{text}", this, UIPanelDescription.Default(new Color4(1, 1, 1, 0.25f)));

            var button = new UIButton($"MainPanel.Button.{text}", this, desc);
            button.Caption.Text = text;
            button.JustReleased += buttonJustReleased;
            panel.AddChild(button);

            return panel;
        }
        private async Task InitializeMusic()
        {
            AudioManager.LoadSound("Music", "start/resources", "anttisinstrumentals+keepshiningoninstrumental.mp3");
            AudioManager.AddEffectParams(
                "Music",
                new GameAudioEffectParameters
                {
                    DestroyWhenFinished = false,
                    SoundName = "Music",
                    IsLooped = true,
                    UseAudio3D = true,
                });

            currentMusic = AudioManager.CreateEffectInstance("Music");

            await Task.CompletedTask;
        }
        private void PrepareAssets(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            AudioManager.MasterVolume = 1f;
            AudioManager.Start();

            currentMusic?.Play();
            currentMusic?.TweenVolumeUp((long)(currentMusic?.Duration.TotalMilliseconds * 0.2f), ScaleFuncs.Linear);

            backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            UpdateLayout();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateCamera();
        }
        private void UpdateCamera()
        {
            float xmouse = ((Game.Input.MouseX / (float)Game.Form.RenderWidth) - 0.5f) * 2f;
            float ymouse = ((Game.Input.MouseY / (float)Game.Form.RenderHeight) - 0.5f) * 2f;

            float d = 0.25f;
            float vx = 0.5f;
            float vy = 0.25f;

            Vector3 position = Vector3.Zero;
            position.X = +((xmouse * d) + (0.2f * (float)Math.Cos(vx * Game.GameTime.TotalSeconds)));
            position.Y = -((ymouse * d) + (0.1f * (float)Math.Sin(vy * Game.GameTime.TotalSeconds)));

            Camera.Position = new Vector3(0, 0, -5f);
            Camera.LookTo(position);
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            mainPanel.Width = Game.Form.RenderWidth * 0.8f;
            mainPanel.Height = Game.Form.RenderHeight * 0.7f;
            mainPanel.Anchor = Anchors.HorizontalCenter;
            mainPanel.Top = Game.Form.RenderHeight * 0.25f;

            var rect = Game.Form.RenderRectangle;
            rect.Height = Game.Form.RenderHeight * 0.3f;
            title.SetRectangle(rect);
        }
    }
}
