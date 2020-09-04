using Engine;
using Engine.Audio;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest.SceneStart
{
    class SceneStart : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private Model backGround = null;
        private UITextArea title = null;
        private UIButton sceneMaterialsButton = null;
        private UIButton sceneWaterButton = null;
        private UIButton sceneStencilPassButton = null;
        private UIButton sceneLightsButton = null;
        private UIButton sceneCascadedShadowsButton = null;
        private UIButton sceneTestButton = null;
        private UIButton sceneTanksGameButton = null;
        private UIButton exitButton = null;

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
        private async Task InitializeAssets()
        {
            #region Cursor

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

            #endregion

            #region Background

            var backGroundDesc = ModelDescription.FromXml("Background", "SceneStart", "SkyPlane.xml");
            this.backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);

            #endregion

            #region Title text

            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Bold, Color.Gold);
            titleFont.Name = "Title";
            titleFont.ShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleFont.ShadowDelta = new Vector2(4, 4);
            titleFont.HorizontalAlign = HorizontalTextAlign.Center;
            titleFont.VerticalAlign = VerticalTextAlign.Middle;

            var titleDesc = UITextAreaDescription.Default(titleFont);

            this.title = await this.AddComponentUITextArea(titleDesc, layerHUD);
            this.title.AdjustAreaWithText = false;

            #endregion

            #region Scene buttons

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, Color.Gold);
            buttonsFont.HorizontalAlign = HorizontalTextAlign.Center;
            buttonsFont.VerticalAlign = VerticalTextAlign.Middle;

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton("common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f, buttonsFont);
            startButtonDesc.Name = "Scene buttons";
            startButtonDesc.Width = 150;
            startButtonDesc.Height = 55;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);

            this.sceneMaterialsButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneWaterButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneStencilPassButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneLightsButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneCascadedShadowsButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneTestButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneTanksGameButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);

            #endregion

            #region Exit button

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton("common/buttons.png", new Vector4(44, 30, 556, 136) / 600f, new Vector4(44, 30, 556, 136) / 600f, buttonsFont);
            exitButtonDesc.Name = "Exit button";
            exitButtonDesc.Width = 150;
            exitButtonDesc.Height = 55;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);

            this.exitButton = await this.AddComponentUIButton(exitButtonDesc, layerHUD);

            #endregion

            #region Music

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

            #endregion
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

            this.backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            this.title.Text = "Scene Manager Test";
            this.sceneMaterialsButton.Caption.Text = "Materials";
            this.sceneWaterButton.Caption.Text = "Water";
            this.sceneStencilPassButton.Caption.Text = "Stencil Pass";
            this.sceneLightsButton.Caption.Text = "Lights";
            this.sceneCascadedShadowsButton.Caption.Text = "Cascaded";
            this.sceneTestButton.Caption.Text = "Test";
            this.sceneTanksGameButton.Caption.Text = "Tanks Game";
            this.exitButton.Caption.Text = "Exit";

            var sceneButtons = new[]
            {
                this.sceneMaterialsButton,
                this.sceneWaterButton,
                this.sceneStencilPassButton,
                this.sceneLightsButton,
                this.sceneCascadedShadowsButton,
                this.sceneTestButton,
                this.sceneTanksGameButton,
            };

            int numButtons = sceneButtons.Length + 1;
            int div = numButtons + 1;
            int h = 4;
            int hv = h - 1;

            var rect = this.Game.Form.RenderRectangle;
            rect.Height /= 2;
            this.title.SetRectangle(rect);
            this.title.CenterHorizontally = CenterTargets.Screen;
            this.title.CenterVertically = CenterTargets.Screen;

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                sceneButtons[i].Left = ((this.Game.Form.RenderWidth / div) * (i + 1)) - (this.sceneMaterialsButton.Width / 2);
                sceneButtons[i].Top = (this.Game.Form.RenderHeight / h) * hv - (this.sceneMaterialsButton.Height / 2);
                sceneButtons[i].JustReleased += SceneButtonClick;
                sceneButtons[i].MouseEnter += SceneButtonMouseEnter;
                sceneButtons[i].MouseLeave += SceneButtonMouseLeave;
            }

            this.exitButton.Left = (this.Game.Form.RenderWidth / div) * numButtons - (this.exitButton.Width / 2);
            this.exitButton.Top = (this.Game.Form.RenderHeight / h) * hv - (this.exitButton.Height / 2);
            this.exitButton.JustReleased += ExitButtonClick;
            this.exitButton.MouseEnter += SceneButtonMouseEnter;
            this.exitButton.MouseLeave += SceneButtonMouseLeave;

            this.sceneReady = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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

        private void SceneButtonClick(object sender, EventArgs e)
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

        private void ExitButtonClick(object sender, EventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            this.Game.Exit();
        }
    }
}
