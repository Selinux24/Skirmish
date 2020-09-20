using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest.SceneStart
{
    class SceneStart : Scene
    {
        private const int layerHUD = 50;
        private const int layerCursor = 100;

        private Model backGround = null;
        private UITextArea title = null;
        private UIButton[] sceneButtons = null;
        private UIButton sceneMaterialsButton = null;
        private UIButton sceneWaterButton = null;
        private UIButton sceneStencilPassButton = null;
        private UIButton sceneLightsButton = null;
        private UIButton sceneCascadedShadowsButton = null;
        private UIButton sceneTestButton = null;
        private UIButton sceneTanksGameButton = null;
        private UIButton exitButton = null;
        private UIPanel buttonPanel = null;
        private UIButton optsButton = null;
        private UITabPanel tabsPanel = null;

        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";
        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.Orange, 1.5f);

        private IAudioEffect currentMusic = null;

        private bool sceneReady = false;

        public SceneStart(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            await this.LoadResourcesAsync(InitializeAssets(), PrepareAssets);
        }
        private Task[] InitializeAssets()
        {
            return new[] {
                InitializeCursor(),
                InitializeBackground(),
                InitializeTitle(),
                InitializeButtonPanel(),
                InitializeOptionsButton(),
                InitializeTabPanel(),
                InitializeMusic(),
            };
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = new UICursorDescription()
            {
                Name = "Cursor",
                ContentPath = "Common",
                Textures = new[] { "pointer.png" },
                Height = 48,
                Width = 48,
                Centered = false,
                Delta = new Vector2(-14f, -7f),
                TintColor = Color.White,
            };
            await this.AddComponentUICursor(cursorDesc, layerCursor);
        }
        private async Task InitializeBackground()
        {
            var backGroundDesc = ModelDescription.FromXml("Background", "SceneStart", "SkyPlane.xml");
            backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);
        }
        private async Task InitializeTitle()
        {
            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Bold, Color.Gold);
            titleFont.Name = "Title";
            titleFont.ShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleFont.ShadowDelta = new Vector2(4, 4);
            titleFont.HorizontalAlign = HorizontalTextAlign.Center;
            titleFont.VerticalAlign = VerticalTextAlign.Middle;

            var titleDesc = UITextAreaDescription.Default(titleFont);

            title = await this.AddComponentUITextArea(titleDesc, layerHUD);
            title.AdjustAreaWithText = false;
        }
        private async Task InitializeButtonPanel()
        {
            buttonPanel = await this.AddComponentUIPanel(UIPanelDescription.Default(Color.Transparent), layerHUD);
            buttonPanel.SetGridLayout(GridLayout.FixedRows(1));
            buttonPanel.Spacing = 20;

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, Color.Gold);
            buttonsFont.HorizontalAlign = HorizontalTextAlign.Center;
            buttonsFont.VerticalAlign = VerticalTextAlign.Middle;

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(
                "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f,
                UITextAreaDescription.Default(buttonsFont));
            startButtonDesc.Name = "Scene buttons";
            startButtonDesc.Width = 150;
            startButtonDesc.Height = 55;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);

            sceneMaterialsButton = new UIButton(this, startButtonDesc);
            sceneWaterButton = new UIButton(this, startButtonDesc);
            sceneStencilPassButton = new UIButton(this, startButtonDesc);
            sceneLightsButton = new UIButton(this, startButtonDesc);
            sceneCascadedShadowsButton = new UIButton(this, startButtonDesc);
            sceneTestButton = new UIButton(this, startButtonDesc);
            sceneTanksGameButton = new UIButton(this, startButtonDesc);

            sceneButtons = new[]
            {
                sceneMaterialsButton,
                sceneWaterButton,
                sceneStencilPassButton,
                sceneLightsButton,
                sceneCascadedShadowsButton,
                sceneTestButton,
                sceneTanksGameButton,
            };

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                sceneButtons[i].JustReleased += SceneButtonJustReleased;
                sceneButtons[i].MouseEnter += SceneButtonMouseEnter;
                sceneButtons[i].MouseLeave += SceneButtonMouseLeave;
            }

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(
                "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f,
                UITextAreaDescription.Default(buttonsFont));
            exitButtonDesc.Name = "Exit button";
            exitButtonDesc.Width = 150;
            exitButtonDesc.Height = 55;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);

            exitButton = new UIButton(this, exitButtonDesc);
            exitButton.JustReleased += ExitButtonJustReleased;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;

            buttonPanel.AddChildren(sceneButtons, false);
            buttonPanel.AddChild(exitButton, false);
        }
        private async Task InitializeOptionsButton()
        {
            optsButton = await this.AddComponentUIButton(UIButtonDescription.Default("SceneStart/ui_options.png"));
            optsButton.JustReleased += OptsButtonJustReleased;

            var optsBackground = new Sprite(this, SpriteDescription.Default(Color.White))
            {
                EventsEnabled = false
            };
            optsButton.InsertChild(0, optsBackground);
        }
        private async Task InitializeTabPanel()
        {
            Color4 baseColor = Color.CornflowerBlue;
            Color4 highLightColor = new Color4(baseColor.RGB() * 1.25f, 1f);
            var tabDesc = UITabPanelDescription.Default(3, Color.Transparent, baseColor, highLightColor);
            tabDesc.Captions = new[] { "But 1", "But 2", "But 3" };

            tabsPanel = await this.AddComponentUITabPanel(tabDesc, layerHUD + 1);
            tabsPanel.Visible = false;
            tabsPanel.TabJustReleased += TabsPanelTabJustReleased;

            var pan1Desc = UIPanelDescription.Default(@"SceneStart/TanksGame.png");
            tabsPanel.SetTabPanel(1, pan1Desc);

            var p = GridLayout.FixedRows(1);
            tabsPanel.TabPanels[0].SetGridLayout(p);
            tabsPanel.TabPanels[0].Spacing = 10;
            tabsPanel.TabPanels[0].Padding = 15;
            tabsPanel.TabPanels[0].TintColor = new Color4(1, 1, 1, 0.25f);
            for (int i = 0; i < 5; i++)
            {
                var panDesc = UIPanelDescription.Default(new Color4(Helper.RandomGenerator.NextVector3(Vector3.Zero, Vector3.One), 1f));
                var pan = new UIPanel(this, panDesc);
                tabsPanel.TabPanels[0].AddChild(pan, false);
            }

            var lastPan = tabsPanel.TabPanels[0].Children.OfType<UIPanel>().Last();
            var p2 = GridLayout.FixedColumns(1);
            lastPan.SetGridLayout(p2);
            lastPan.Spacing = 10;
            lastPan.TintColor = Color.Transparent;
            for (int i = 0; i < 2; i++)
            {
                var panDesc = UIPanelDescription.Default(new Color4(Helper.RandomGenerator.NextVector3(Vector3.Zero, Vector3.One), 1f));
                var pan = new UIPanel(this, panDesc);
                lastPan.AddChild(pan, false);
            }

            var lastPan2 = lastPan.Children.OfType<UIPanel>().Last();
            var p3 = GridLayout.Uniform;
            lastPan2.SetGridLayout(p3);
            lastPan2.Spacing = 10;
            lastPan2.TintColor = Color.Transparent;
            for (int i = 0; i < 4; i++)
            {
                var panDesc = UIPanelDescription.Default(new Color4(Helper.RandomGenerator.NextVector3(Vector3.Zero, Vector3.One), 1f));
                var pan = new UIPanel(this, panDesc);
                lastPan2.AddChild(pan, false);
            }

            tabsPanel.Visible = false;
        }
        private async Task InitializeMusic()
        {
            AudioManager.LoadSound("Music", "SceneStart", "anttisinstrumentals+icemanandangelinstrumental.mp3");
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

            title.Text = "Scene Manager Test";
            sceneMaterialsButton.Caption.Text = "Materials";
            sceneWaterButton.Caption.Text = "Water";
            sceneStencilPassButton.Caption.Text = "Stencil Pass";
            sceneLightsButton.Caption.Text = "Lights";
            sceneCascadedShadowsButton.Caption.Text = "Cascaded";
            sceneTestButton.Caption.Text = "Test";
            sceneTanksGameButton.Caption.Text = "Tanks Game";
            exitButton.Caption.Text = "Exit";

            UpdateLayout();

            this.sceneReady = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateCamera();
            UpdateInput();
        }
        private void UpdateCamera()
        {
            float xmouse = (((float)this.Game.Input.MouseX / (float)this.Game.Form.RenderWidth) - 0.5f) * 2f;
            float ymouse = (((float)this.Game.Input.MouseY / (float)this.Game.Form.RenderHeight) - 0.5f) * 2f;

            float d = 0.25f;
            float vx = 0.5f;
            float vy = 0.25f;

            Vector3 position = Vector3.Zero;
            position.X = +((xmouse * d) + (0.2f * (float)Math.Cos(vx * this.Game.GameTime.TotalSeconds)));
            position.Y = -((ymouse * d) + (0.1f * (float)Math.Sin(vy * this.Game.GameTime.TotalSeconds)));

            this.Camera.Position = new Vector3(0, 0, -5f);
            this.Camera.LookTo(position);
        }
        private void UpdateInput()
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                ClosePanel();
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }

        private void UpdateLayout()
        {
            tabsPanel.Width = Game.Form.RenderWidth * 0.9f;
            tabsPanel.Height = Game.Form.RenderHeight * 0.7f;
            tabsPanel.CenterHorizontally = CenterTargets.Screen;
            tabsPanel.Top = Game.Form.RenderHeight * 0.1f;

            var rect = Game.Form.RenderRectangle;
            rect.Height /= 2;
            title.SetRectangle(rect);
            title.CenterHorizontally = CenterTargets.Screen;
            title.CenterVertically = CenterTargets.Screen;

            int h = 8;
            int hv = h - 1;

            buttonPanel.Width = Game.Form.RenderWidth * 0.9f;
            buttonPanel.Height = 50;
            buttonPanel.CenterHorizontally = CenterTargets.Screen;
            buttonPanel.Top = Game.Form.RenderHeight / h * hv - (buttonPanel.Height / 2);

            optsButton.Width = 50;
            optsButton.Height = 50;
            optsButton.SetPosition(Game.Form.RenderWidth - 10 - optsButton.Width, 10);
        }

        private void OpenPanel()
        {
            if (tabsPanel.Visible)
            {
                return;
            }

            optsButton.Hide(100);
            tabsPanel.Show(5000);
        }
        private void ClosePanel()
        {
            if (!tabsPanel.Visible)
            {
                return;
            }

            optsButton.Show(100);
            tabsPanel.Hide(5000);
        }

        private void SceneButtonJustReleased(object sender, EventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            if (sender == this.sceneMaterialsButton)
            {
                this.Game.SetScene<SceneMaterials.SceneMaterials>();
            }
            else if (sender == this.sceneWaterButton)
            {
                this.Game.SetScene<SceneWater.SceneWater>();
            }
            else if (sender == this.sceneStencilPassButton)
            {
                this.Game.SetScene<SceneStencilPass.SceneStencilPass>();
            }
            else if (sender == this.sceneLightsButton)
            {
                this.Game.SetScene<SceneLights.SceneLights>();
            }
            else if (sender == this.sceneCascadedShadowsButton)
            {
                this.Game.SetScene<SceneCascadedShadows.SceneCascadedShadows>();
            }
            else if (sender == this.sceneTestButton)
            {
                this.Game.SetScene<SceneTest.SceneTest>();
            }
            else if (sender == this.sceneTanksGameButton)
            {
                this.Game.SetScene<SceneTanksGame.SceneTanksGame>();
            }
        }
        private void SceneButtonMouseEnter(object sender, EventArgs e)
        {
            if (sender is UIControl ctrl)
            {
                ctrl.ScaleInScaleOut(1.0f, 1.10f, 250);
            }
        }
        private void SceneButtonMouseLeave(object sender, EventArgs e)
        {
            if (sender is UIControl ctrl)
            {
                ctrl.ClearTween();
                ctrl.TweenScale(ctrl.Scale, 1.0f, 500, ScaleFuncs.Linear);
            }
        }
        private void TabsPanelTabJustReleased(object sender, UITabPanelEventArgs e)
        {
            Logger.WriteDebug($"Clicked button {e.TabButton.Caption.Text}");
        }
        private void ExitButtonJustReleased(object sender, EventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            this.Game.Exit();
        }
        private void OptsButtonJustReleased(object sender, EventArgs e)
        {
            OpenPanel();
        }
    }
}
