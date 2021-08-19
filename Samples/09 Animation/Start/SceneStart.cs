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
            var cursorDesc = UICursorDescription.Default("start/resources/pointer.png", 48, 48, false, new Vector2(-14f, -7f));
            await this.AddComponentUICursor("Cursor", "Cursor", cursorDesc);
        }
        private async Task InitializeBackground()
        {
            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("start/resources", "SkyPlane.json"),
            };
            backGround = await this.AddComponentModel("Background", "Background", backGroundDesc, SceneObjectUsages.UI);
        }
        private async Task InitializeTitle()
        {
            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, true);

            title = await this.AddComponentUITextArea("Title", "Title", UITextAreaDescription.Default(titleFont));
            title.GrowControlWithText = false;
            title.Text = "Animation";
            title.TextForeColor = Color.Gold;
            title.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            title.TextShadowDelta = new Vector2(4, 4);
            title.TextHorizontalAlign = TextHorizontalAlign.Center;
            title.TextVerticalAlign = TextVerticalAlign.Middle;
        }
        private async Task InitializeMainPanel()
        {
            mainPanel = await this.AddComponentUIPanel("MainPanel", "MainPanel", UIPanelDescription.Default(Color.Transparent));
            mainPanel.Spacing = 10;
            mainPanel.Padding = 15;
            mainPanel.SetGridLayout(GridLayout.FixedRows(2));

            var buttonFont = TextDrawerDescription.FromFamily(buttonFonts, 36, true);

            Color4 highlightColor = new Color4(0.3333f, 0.3333f, 0.3333f, 0f);
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton(buttonFont, Color.Red, Color.Red.ToColor4() + highlightColor);
            buttonDesc.TextForeColor = Color.Gold;
            buttonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            buttonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            var exitDesc = UIButtonDescription.DefaultTwoStateButton(buttonFont, Color.Orange, Color.Orange.ToColor4() + highlightColor);
            exitDesc.TextForeColor = Color.Gold;
            exitDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            var emptyDesc = SpriteDescription.Default("start/resources/empty.png");

            var panSimpleAnimation = AddButtonPanel(buttonDesc, "Simple Animation", (sender, args) =>
            {
                if (!args.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                Game.SetScene<SimpleAnimation.SceneSimpleAnimation>();
            });
            var panAnimationParts = AddButtonPanel(buttonDesc, "Animation Parts", (sender, args) =>
            {
                if (!args.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                Game.SetScene<AnimationParts.SceneAnimationParts>();
            });
            var panExit = AddButtonPanel(exitDesc, "Exit", (sender, args) => { Game.Exit(); });

            mainPanel.AddChild(panSimpleAnimation, false);
            mainPanel.AddChild(new Sprite("Empty1", "Empty", this, emptyDesc), false);
            mainPanel.AddChild(new Sprite("Empty2", "Empty", this, emptyDesc), false);
            mainPanel.AddChild(new Sprite("Empty3", "Empty", this, emptyDesc), false);
            mainPanel.AddChild(new Sprite("Empty4", "Empty", this, emptyDesc), false);
            mainPanel.AddChild(panAnimationParts, false);
            mainPanel.AddChild(new Sprite("Empty5", "Empty", this, emptyDesc), false);
            mainPanel.AddChild(panExit, false);
        }
        private UIPanel AddButtonPanel(UIButtonDescription desc, string text, MouseEventHandler buttonJustReleased)
        {
            var panel = new UIPanel($"MainPanel.Panel.{text}", $"MainPanel.Panel.{text}", this, UIPanelDescription.Default(new Color4(1, 1, 1, 0.25f)));

            var button = new UIButton($"MainPanel.Button.{text}", $"MainPanel.Button.{text}", this, desc);
            button.Caption.Text = text;
            button.MouseClick += buttonJustReleased;
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
