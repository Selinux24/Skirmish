using Engine;
using Engine.Audio;
using Engine.Audio.Tween;
using Engine.Content;
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
        private AudioEffectTweener audioTweener;
        private UIControlTweener uiTweener;

        private Model backGround = null;
        private UITextArea title = null;
        private UIButton sceneMaterialsButton = null;
        private UIButton sceneWaterButton = null;
        private UIButton sceneStencilPassButton = null;
        private UIButton sceneLightsButton = null;
        private UIButton sceneCascadedShadowsButton = null;
        private UIButton sceneTestButton = null;
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
                InitializeOptionsButton(),
                InitializeTabPanel(),
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
            buttonPanel = await AddComponentUI<UIPanel, UIPanelDescription>("ButtonPanel", "ButtonPanel", UIPanelDescription.Default(Color.Transparent));
            buttonPanel.SetGridLayout(GridLayout.FixedRows(1));
            buttonPanel.Spacing = 20;
            buttonPanel.EventsEnabled = true;

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, true);

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            startButtonDesc.Width = 150;
            startButtonDesc.Height = 55;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextForeColor = Color.Gold;
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            sceneMaterialsButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonMaterials", "ButtonMaterials", startButtonDesc);
            sceneMaterialsButton.Visible = false;
            sceneWaterButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonWater", "ButtonWater", startButtonDesc);
            sceneWaterButton.Visible = false;
            sceneStencilPassButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonStencilPass", "ButtonStencilPass", startButtonDesc);
            sceneStencilPassButton.Visible = false;
            sceneLightsButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonLights", "ButtonLights", startButtonDesc);
            sceneLightsButton.Visible = false;
            sceneCascadedShadowsButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonCascadedShadows", "ButtonCascadedShadows", startButtonDesc);
            sceneCascadedShadowsButton.Visible = false;
            sceneTestButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonTest", "ButtonTest", startButtonDesc);
            sceneTestButton.Visible = false;

            var sceneButtons = new[]
            {
                sceneMaterialsButton,
                sceneWaterButton,
                sceneStencilPassButton,
                sceneLightsButton,
                sceneCascadedShadowsButton,
                sceneTestButton,
            };

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                sceneButtons[i].MouseClick += SceneButtonClick;
                sceneButtons[i].MouseEnter += SceneButtonMouseEnter;
                sceneButtons[i].MouseLeave += SceneButtonMouseLeave;
            }

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            exitButtonDesc.Width = 150;
            exitButtonDesc.Height = 55;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            exitButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonExit", "ButtonExit", exitButtonDesc);
            exitButton.MouseClick += ExitButtonClick;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;

            buttonPanel.AddChildren(sceneButtons, false);
            buttonPanel.AddChild(exitButton, false);
        }
        private async Task InitializeOptionsButton()
        {
            optsButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonOptions", "ButtonOptions", UIButtonDescription.Default("SceneStart/ui_options.png"));
            optsButton.Visible = false;
            optsButton.MouseClick += OptsButtonClick;

            var optsBackground = await CreateComponent<Sprite, SpriteDescription>("ButtonOptions.Background", "ButtonOptions.Background", SpriteDescription.Default(Color.White));
            optsBackground.Visible = false;
            optsBackground.EventsEnabled = false;

            optsButton.InsertChild(0, optsBackground);
        }
        private async Task InitializeTabPanel()
        {
            Color4 baseColor = Color.CornflowerBlue;
            Color4 highLightColor = new(baseColor.RGB() * 1.25f, 1f);
            var tabDesc = UITabPanelDescription.Default(3, Color.Transparent, baseColor, highLightColor);
            tabDesc.TabCaptions = new[] { "But 1", "But 2", "But 3" };
            tabDesc.TabButtonsSpacing = new Spacing() { Horizontal = 5f };

            tabsPanel = await AddComponentUI<UITabPanel, UITabPanelDescription>("TabPanel", "TabPanel", tabDesc, LayerUI + 1);
            tabsPanel.Visible = false;
            tabsPanel.TabClick += TabsPanelTabClick;

            var pan1Desc = UIPanelDescription.Default(@"SceneStart/TanksGame.png");
            await tabsPanel.SetTabPanel(1, pan1Desc);

            var p = GridLayout.FixedRows(1);
            tabsPanel.TabPanels[0].SetGridLayout(p);
            tabsPanel.TabPanels[0].Spacing = 10;
            tabsPanel.TabPanels[0].Padding = 10;
            tabsPanel.TabPanels[0].BaseColor = new Color4(1, 1, 1, 0.25f);
            for (int i = 0; i < 5; i++)
            {
                var panDesc = UIPanelDescription.Default(new Color4(Helper.RandomGenerator.NextVector3(Vector3.Zero, Vector3.One), 1f));
                var pan = await CreateComponent<UIPanel, UIPanelDescription>($"TabPanel.Level1_{i}", $"TabPanel.Level1_{i}", panDesc);
                tabsPanel.TabPanels[0].AddChild(pan, false);
            }

            var lastPan = tabsPanel.TabPanels[0].Children.OfType<UIPanel>().Last();
            var p2 = GridLayout.FixedColumns(1);
            lastPan.SetGridLayout(p2);
            lastPan.Spacing = 10;
            lastPan.BaseColor = Color.Transparent;
            for (int i = 0; i < 2; i++)
            {
                var panDesc = UIPanelDescription.Default(new Color4(Helper.RandomGenerator.NextVector3(Vector3.Zero, Vector3.One), 1f));
                var pan = await CreateComponent<UIPanel, UIPanelDescription>($"TabPanel.Level2_{i}", $"TabPanel.Level2_{i}", panDesc);
                lastPan.AddChild(pan, false);
            }

            var lastPan2 = lastPan.Children.OfType<UIPanel>().Last();
            var p3 = GridLayout.Uniform;
            lastPan2.SetGridLayout(p3);
            lastPan2.Spacing = 10;
            lastPan2.BaseColor = Color.Transparent;
            for (int i = 0; i < 4; i++)
            {
                var panDesc = UIPanelDescription.Default(new Color4(Helper.RandomGenerator.NextVector3(Vector3.Zero, Vector3.One), 1f));
                var pan = await CreateComponent<UIPanel, UIPanelDescription>($"TabPanel.Level3_{i}", $"TabPanel.Level3_{i}", panDesc);
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

            backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            var shadow = new Color4(0, 0, 0, 0.5f);
            title.Text = $"{Color.Red}|{shadow}S{Color.Cyan}c{Color.Red}e{Color.Cyan}n{Color.Red}e {Color.Green}M{Color.Orange}a{Color.Green}n{Color.Orange}a{Color.Green}g{Color.Orange}e{Color.Green}r {Color.Yellow}T{Color.Blue}e{Color.Yellow}s{Color.Blue}t";

            sceneMaterialsButton.Caption.Text = $"{Color.Red}M{Color.Gold}aterials";
            sceneWaterButton.Caption.Text = $"{Color.Red}W{Color.Gold}ater";
            sceneStencilPassButton.Caption.Text = $"{Color.Red}S{Color.Gold}tencil Pass";
            sceneLightsButton.Caption.Text = $"{Color.Red}L{Color.Gold}ights";
            sceneCascadedShadowsButton.Caption.Text = $"{Color.Red}C{Color.Gold}ascaded";
            sceneTestButton.Caption.Text = $"{Color.Red}T{Color.Gold}est";

            exitButton.Caption.Text = $"{Color.Red}E{Color.Gold}xit";

            UpdateLayout();

            sceneReady = true;
        }

        public override void Update(GameTime gameTime)
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

            Camera.Position = new Vector3(0, 0, -5f);
            Camera.LookTo(position);
        }
        private void UpdateInput()
        {
            if (Game.Input.KeyJustReleased(Keys.Escape) || Game.Input.KeyJustReleased(Keys.E))
            {
                ClosePanel();
            }
            else if (Game.Input.KeyJustReleased(Keys.M))
            {
                Game.SetScene<SceneMaterials.SceneMaterials>();
            }
            else if (Game.Input.KeyJustReleased(Keys.W))
            {
                Game.SetScene<SceneWater.SceneWater>();
            }
            else if (Game.Input.KeyJustReleased(Keys.S))
            {
                Game.SetScene<SceneStencilPass.SceneStencilPass>();
            }
            else if (Game.Input.KeyJustReleased(Keys.L))
            {
                Game.SetScene<SceneLights.SceneLights>();
            }
            else if (Game.Input.KeyJustReleased(Keys.C))
            {
                Game.SetScene<SceneCascadedShadows.SceneCascadedShadows>();
            }
            else if (Game.Input.KeyJustReleased(Keys.T))
            {
                Game.SetScene<SceneTest.SceneTest>();
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
            tabsPanel.Anchor = Anchors.HorizontalCenter;
            tabsPanel.Top = Game.Form.RenderHeight * 0.1f;

            var rect = Game.Form.RenderRectangle;
            rect.Height /= 2;
            title.SetRectangle(rect);
            title.Anchor = Anchors.Center;

            int h = 8;
            int hv = h - 1;

            buttonPanel.Width = Game.Form.RenderWidth * 0.9f;
            buttonPanel.Height = 50;
            buttonPanel.Anchor = Anchors.HorizontalCenter;
            buttonPanel.Top = Game.Form.RenderHeight / h * hv - (buttonPanel.Height / 2);

            optsButton.Width = 50;
            optsButton.Height = 50;
            optsButton.SetPosition(Game.Form.RenderWidth - 10 - optsButton.Width, 10);
            optsButton.Visible = true;

            sceneMaterialsButton.Visible = true;
            sceneWaterButton.Visible = true;
            sceneStencilPassButton.Visible = true;
            sceneLightsButton.Visible = true;
            sceneCascadedShadowsButton.Visible = true;
            sceneTestButton.Visible = true;
        }

        private void OpenPanel()
        {
            if (tabsPanel.Visible)
            {
                return;
            }

            uiTweener.Hide(optsButton, 100);
            uiTweener.Show(tabsPanel, 100);
        }
        private void ClosePanel()
        {
            if (!tabsPanel.Visible)
            {
                return;
            }

            uiTweener.Show(optsButton, 100);
            uiTweener.Hide(tabsPanel, 100);
        }

        private void SceneButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (sender == sceneMaterialsButton)
            {
                Game.SetScene<SceneMaterials.SceneMaterials>();
            }
            else if (sender == sceneWaterButton)
            {
                Game.SetScene<SceneWater.SceneWater>();
            }
            else if (sender == sceneStencilPassButton)
            {
                Game.SetScene<SceneStencilPass.SceneStencilPass>();
            }
            else if (sender == sceneLightsButton)
            {
                Game.SetScene<SceneLights.SceneLights>();
            }
            else if (sender == sceneCascadedShadowsButton)
            {
                Game.SetScene<SceneCascadedShadows.SceneCascadedShadows>();
            }
            else if (sender == sceneTestButton)
            {
                Game.SetScene<SceneTest.SceneTest>();
            }
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
        private void TabsPanelTabClick(object sender, UITabPanelEventArgs e)
        {
            Logger.WriteDebug(this, $"Clicked button {e.TabButton.Caption.Text}");
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
        private void OptsButtonClick(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            OpenPanel();
        }
    }
}
