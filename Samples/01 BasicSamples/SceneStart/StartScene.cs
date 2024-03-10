using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicSamples.SceneStart
{
    class StartScene : Scene
    {
        private const string MusicResourceString = "Music";

        private AudioEffectTweener audioTweener;
        private UIControlTweener uiTweener;

        private Model backGround = null;
        private UITextArea title = null;

        private readonly List<UIButton> sceneButtons = [];
        private readonly List<(char Key, Type SceneType)> sceneButtonsChars = [];
        private UIPanel buttonPanel = null;

        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";
        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.Orange, 1.5f);

        private IGameAudioEffect currentMusic = null;

        private bool sceneReady = false;

        public StartScene(Game game) : base(game)
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var assetTasks = new[]
            {
                InitializeTweener(),
                InitializeCursor(),
                InitializeBackground(),
                InitializeTitle(),
                InitializeButtonPanel(),
                InitializeMusic(),
            };

            LoadResourcesAsync(
                assetTasks,
                InitializeComponentsCompleted);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            audioTweener = this.AddAudioEffectTweener();
            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default("Common/pointer.png", 48, 48, false, new Vector2(-14f, -7f));

            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc);
        }
        private async Task InitializeBackground()
        {
            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("SceneStart", "SkyPlane.json"),
            };

            backGround = await AddComponent<Model, ModelDescription>("Background", "Background", backGroundDesc);
        }
        private async Task InitializeTitle()
        {
            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Bold, true);

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.TextForeColor = Color.Gold;
            titleDesc.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);
            titleDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            titleDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc);
            title.GrowControlWithText = false;
        }
        private async Task InitializeButtonPanel()
        {
            const int cols = 4;

            buttonPanel = await AddComponentUI<UIPanel, UIPanelDescription>("ButtonPanel", "ButtonPanel", UIPanelDescription.Default(Color.Transparent));
            buttonPanel.SetGridLayout(GridLayout.FixedColumns(cols));
            buttonPanel.Spacing = 40;
            buttonPanel.EventsEnabled = true;

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, true);

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            startButtonDesc.StartsVisible = false;

            await CreateButton("ButtonCascadedShadows", startButtonDesc, "Cascaded", 'C', SceneButtonClick<SceneCascadedShadows.CascadedShadowsScene>);
            await CreateButton("ButtonLights", startButtonDesc, "Lights", 'L', SceneButtonClick<SceneLights.LightsScene>);
            await CreateButton("ButtonMaterials", startButtonDesc, "Materials", 'M', SceneButtonClick<SceneMaterials.MaterialsScene>);
            await CreateButton("ButtonNormalMap", startButtonDesc, "Normal Maps", 'N', SceneButtonClick<SceneNormalMap.NormalMapScene>);
            await CreateButton("ButtonParticles", startButtonDesc, "Particles", 'P', SceneButtonClick<SceneParticles.ParticlesScene>);
            await CreateButton("ButtonStencilPass", startButtonDesc, "Stencil Pass", 'S', SceneButtonClick<SceneStencilPass.StencilPassScene>);
            await CreateButton("ButtonTest", startButtonDesc, "Test Scene", 'T', SceneButtonClick<SceneTest.TestScene>);
            await CreateButton("ButtonUI", startButtonDesc, "User Interface", 'U', SceneButtonClick<SceneUI.UIScene>);
            await CreateButton("ButtonWater", startButtonDesc, "Water", 'W', SceneButtonClick<SceneWater.WaterScene>);

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            exitButtonDesc.StartsVisible = false;

            await CreateButton("ButtonExit", exitButtonDesc, "Exit", 'E', ExitButtonClick);
            buttonPanel.AddChildren(sceneButtons, false);
        }
        private async Task<UIButton> CreateButton(string name, UIButtonDescription desc, string title, char keyChar, MouseEventHandler onClick)
        {
            var button = await CreateComponent<UIButton, UIButtonDescription>(name, name, desc);
            button.Caption.SetTextWithKeyChar(title, keyChar, Color.Gold, Color.Red);

            button.MouseClick += onClick;
            button.MouseEnter += SceneButtonMouseEnter;
            button.MouseLeave += SceneButtonMouseLeave;

            sceneButtons.Add(button);

            return button;
        }
        private async Task InitializeMusic()
        {
            AudioManager.LoadSound(MusicResourceString, "SceneStart", "anttisinstrumentals+icemanandangelinstrumental.mp3");
            AudioManager.AddEffectParams(
                MusicResourceString,
                new GameAudioEffectParameters
                {
                    DestroyWhenFinished = false,
                    SoundName = MusicResourceString,
                    IsLooped = true,
                    UseAudio3D = true,
                });

            currentMusic = AudioManager.CreateEffectInstance(MusicResourceString);

            await Task.CompletedTask;
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Lights.KeyLight.Direction = Vector3.ForwardLH;

            AudioManager.MasterVolume = 1f;
            AudioManager.Start();

            currentMusic?.Play();
            audioTweener.TweenVolumeUp(currentMusic, (long)(currentMusic?.Duration.TotalMilliseconds * 0.2f), ScaleFuncs.Linear);

            backGround.Manipulator.SetScaling(1.5f, 1.25f, 1.5f);

            var shadow = new Color4(0, 0, 0, 0.5f);
            title.Text = $"{Color.Red}|{shadow}B{Color.Cyan}a{Color.Red}s{Color.Cyan}i{Color.Red}c {Color.Green}S{Color.Orange}a{Color.Green}m{Color.Orange}p{Color.Green}l{Color.Orange}e{Color.Green}s";

            UpdateLayout();

            sceneReady = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            UpdateCamera();
            UpdateInput();
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

            Camera.SetPosition(new Vector3(0, 0, -5f));
            Camera.LookTo(position);
        }
        private void UpdateInput()
        {
            if (Game.Input.KeyJustReleased(Keys.E) || Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
                return;
            }

            foreach (var (Key, SceneType) in sceneButtonsChars)
            {
                if (Game.Input.KeyJustReleased(Key))
                {
                    Game.SetScene(SceneType);
                    return;
                }
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            var rect = Game.Form.RenderRectangle;
            rect.Top = 0;
            rect.Height /= 2;
            title.SetRectangle(rect);
            title.Anchor = Anchors.HorizontalCenter;

            int h = 8;
            int hv = h - 1;
            int rows = buttonPanel.Rows;

            buttonPanel.Width = Game.Form.RenderWidth * 0.9f;
            buttonPanel.Height = 85 * rows;
            buttonPanel.Anchor = Anchors.HorizontalCenter;
            buttonPanel.Top = Game.Form.RenderHeight / h * hv - buttonPanel.Height;

            sceneButtons.ForEach(button => { button.Visible = true; });
        }

        private void SceneButtonMouseEnter(IUIControl sender, MouseEventArgs e)
        {
            sender.PivotAnchor = PivotAnchors.Center;
            uiTweener.ScaleInScaleOut(sender, 1.0f, 1.10f, 250);
        }
        private void SceneButtonMouseLeave(IUIControl sender, MouseEventArgs e)
        {
            uiTweener.ClearTween(sender);
            uiTweener.TweenScale(sender, sender.Scale, 1.0f, 500, ScaleFuncs.Linear);
        }
        private void SceneButtonClick<T>(IUIControl sender, MouseEventArgs e) where T : Scene
        {
            if (!sceneReady)
            {
                return;
            }

            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            Game.SetScene<T>();
        }
        private void ExitButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            Game.Exit();
        }
    }
}
