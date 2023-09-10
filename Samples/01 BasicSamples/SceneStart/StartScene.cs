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
        private AudioEffectTweener audioTweener;
        private UIControlTweener uiTweener;

        private Model backGround = null;
        private UITextArea title = null;

        private UIButton exitButton = null;
        private readonly List<UIButton> sceneButtons = new();
        private readonly List<(char Key, Type SceneType)> sceneButtonsChars = new();
        private UIPanel buttonPanel = null;

        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";
        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.Orange, 1.5f);

        private IAudioEffect currentMusic = null;

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
            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, true);

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            startButtonDesc.Width = 150;
            startButtonDesc.Height = 55;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextForeColor = Color.Gold;
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            startButtonDesc.StartsVisible = false;

            await CreateButton<SceneCascadedShadows.CascadedShadowsScene>("ButtonCascadedShadows", startButtonDesc, "Cascaded", 'C');
            await CreateButton<SceneLights.LightsScene>("ButtonLights", startButtonDesc, "Lights", 'L');
            await CreateButton<SceneMaterials.MaterialsScene>("ButtonMaterials", startButtonDesc, "Materials", 'M');
            await CreateButton<SceneNormalMap.NormalMapScene>("ButtonNormalMap", startButtonDesc, "Normal Maps", 'N');
            await CreateButton<SceneParticles.ParticlesScene>("ButtonParticles", startButtonDesc, "Particles", 'P');
            await CreateButton<SceneStencilPass.StencilPassScene>("ButtonStencilPass", startButtonDesc, "Stencil Pass", 'S');
            await CreateButton<SceneTest.TestScene>("ButtonTest", startButtonDesc, "Test Scene", 'T');
            await CreateButton<SceneUI.UIScene>("ButtonUI", startButtonDesc, "User Interface", 'U');
            await CreateButton<SceneWater.WaterScene>("ButtonWater", startButtonDesc, "Water", 'W');

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f);
            exitButtonDesc.Width = 150;
            exitButtonDesc.Height = 55;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            exitButtonDesc.StartsVisible = false;

            exitButton = await CreateComponent<UIButton, UIButtonDescription>("ButtonExit", "ButtonExit", exitButtonDesc);
            exitButton.Caption.Text = $"{Color.Red}E{Color.Gold}xit";
            exitButton.MouseClick += ExitButtonClick;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;

            sceneButtons.Add(exitButton);

            buttonPanel = await AddComponentUI<UIPanel, UIPanelDescription>("ButtonPanel", "ButtonPanel", UIPanelDescription.Default(Color.Transparent));
            buttonPanel.SetGridLayout(GridLayout.FixedColumns(4));
            buttonPanel.Spacing = 20;
            buttonPanel.EventsEnabled = true;
            buttonPanel.AddChildren(sceneButtons, false);
        }
        private async Task<UIButton> CreateButton<T>(string name, UIButtonDescription desc, string title, char keyChar) where T : Scene
        {
            var button = await CreateComponent<UIButton, UIButtonDescription>(name, name, desc);

            string text;
            int keyIndex = title.IndexOf(keyChar);
            if (keyIndex < 0)
            {
                text = $"{Color.Gold}{title}";
            }
            else
            {
                string pre = title[..keyIndex];
                string key = title[keyIndex].ToString();
                string suf = title[(keyIndex + 1)..];
                pre = pre.Length > 0 ? $"{Color.Gold}{pre}" : pre;
                key = key.Length > 0 ? $"{Color.Red}{key}" : key;
                suf = suf.Length > 0 ? $"{Color.Gold}{suf}" : suf;
                text = pre + key + suf;

                sceneButtonsChars.Add(new(keyChar, typeof(T)));
            }

            button.Caption.Text = text;

            button.MouseClick += (s, a) =>
            {
                if (!sceneReady)
                {
                    return;
                }

                if (!a.Buttons.HasFlag(MouseButtons.Left))
                {
                    return;
                }

                Game.SetScene<T>();
            };
            button.MouseEnter += SceneButtonMouseEnter;
            button.MouseLeave += SceneButtonMouseLeave;

            sceneButtons.Add(button);

            return button;
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
            title.Text = $"{Color.Red}|{shadow}B{Color.Cyan}a{Color.Red}s{Color.Cyan}i{Color.Red}c {Color.Green}S{Color.Orange}a{Color.Green}m{Color.Orange}p{Color.Green}l{Color.Orange}e{Color.Green}s";

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
            buttonPanel.Height = 65 * rows;
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
