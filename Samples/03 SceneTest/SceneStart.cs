using Engine;
using Engine.Tween;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest
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
        private UIButton exitButton = null;

        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.Orange, 1.5f);

        public SceneStart(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            await this.LoadResourcesAsync(InitializeAssets(), () =>
            {
                this.backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

                this.title.Text = "Scene Manager Test";
                this.sceneMaterialsButton.Text = "Materials";
                this.sceneWaterButton.Text = "Water";
                this.sceneStencilPassButton.Text = "Stencil Pass";
                this.sceneLightsButton.Text = "Lights";
                this.sceneCascadedShadowsButton.Text = "Cascaded";
                this.sceneTestButton.Text = "Test";
                this.exitButton.Text = "Exit";

                var sceneButtons = new[]
                {
                    this.sceneMaterialsButton,
                    this.sceneWaterButton,
                    this.sceneStencilPassButton,
                    this.sceneLightsButton,
                    this.sceneCascadedShadowsButton,
                    this.sceneTestButton,
                };

                int numButtons = sceneButtons.Length + 1;
                int div = numButtons + 1;
                int h = 4;
                int hv = h - 1;

                var rect = this.Game.Form.RenderRectangle;
                rect.Height /= 2;
                this.title.SetRectangle(rect);
                this.title.CenterHorizontally = CenterTargets.Parent;
                this.title.CenterVertically = CenterTargets.Parent;

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
            });
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
                Color = Color.White,
            };
            await this.AddComponentUICursor(cursorDesc, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = ModelDescription.FromXml("Background", "SceneStart", "SkyPlane.xml");
            this.backGround = await this.AddComponentModel(backGroundDesc, SceneObjectUsages.UI);

            #endregion

            #region Title text

            var titleDesc = new UITextAreaDescription
            {
                Font = new TextDrawerDescription()
                {
                    Name = "Title",
                    Font = "Showcard Gothic",
                    FontSize = 72,
                    Style = FontMapStyles.Bold,
                    TextColor = Color.Gold,
                    ShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f),
                    ShadowDelta = new Vector2(4, 4),
                },
            };
            this.title = await this.AddComponentUITextArea(titleDesc, layerHUD);

            #endregion

            #region Scene buttons

            var startButtonDesc = new UIButtonDescription()
            {
                Name = "Scene buttons",

                Width = 185,
                Height = 40,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f),

                Font = new TextDrawerDescription()
                {
                    Font = "Verdana",
                    Style = FontMapStyles.Bold,
                    FontSize = 20,
                    TextColor = Color.Gold,
                },
            };
            this.sceneMaterialsButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneWaterButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneStencilPassButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneLightsButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneCascadedShadowsButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);
            this.sceneTestButton = await this.AddComponentUIButton(startButtonDesc, layerHUD);

            #endregion

            #region Exit button

            var exitButtonDesc = new UIButtonDescription()
            {
                Name = "Exit button",

                Width = 185,
                Height = 40,

                TwoStateButton = true,

                TextureReleased = "common/buttons.png",
                TextureReleasedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f),

                TexturePressed = "common/buttons.png",
                TexturePressedUVMap = new Vector4(44, 30, 556, 136) / 600f,
                ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f),

                Font = new TextDrawerDescription()
                {
                    Font = "Verdana",
                    Style = FontMapStyles.Bold,
                    FontSize = 20,
                    TextColor = Color.Gold,
                },
            };
            this.exitButton = await this.AddComponentUIButton(exitButtonDesc, layerHUD);

            #endregion
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
            if (sender == this.sceneMaterialsButton)
            {
                this.Game.SetScene<SceneMaterials>();
            }
            else if (sender == this.sceneWaterButton)
            {
                this.Game.SetScene<SceneWater>();
            }
            else if (sender == this.sceneStencilPassButton)
            {
                this.Game.SetScene<SceneStencilPass>();
            }
            else if (sender == this.sceneLightsButton)
            {
                this.Game.SetScene<SceneLights>();
            }
            else if (sender == this.sceneCascadedShadowsButton)
            {
                this.Game.SetScene<SceneCascadedShadows>();
            }
            else if (sender == this.sceneTestButton)
            {
                this.Game.SetScene<SceneTest>();
            }
        }
        private void SceneButtonMouseEnter(object sender, EventArgs e)
        {
            if (sender is UIControl ctrl)
            {
                ctrl.ScaleInScaleOut(1.0f, 1.10f, 0.25f);
            }
        }
        private void SceneButtonMouseLeave(object sender, EventArgs e)
        {
            if (sender is UIControl ctrl)
            {
                ctrl.ClearTween();
                ctrl.TweenScale(ctrl.Scale, 1.0f, 0.5f, ScaleFuncs.Linear);
            }
        }

        private void ExitButtonClick(object sender, EventArgs e)
        {
            this.Game.Exit();
        }
    }

    static class UIControlExtensions
    {
        public static void Show(this UIControl ctrl, float time)
        {
            ctrl.TweenShow(time, ScaleFuncs.Linear);
        }

        public static void Hide(this UIControl ctrl, float time)
        {
            ctrl.TweenHide(time, ScaleFuncs.Linear);
        }

        public static void Roll(this UIControl ctrl, float time)
        {
            ctrl.TweenRotate(MathUtil.TwoPi, time, ScaleFuncs.Linear);
            ctrl.TweenScale(1, 0.5f, time, ScaleFuncs.QuinticEaseOut);
        }

        public static void ShowRoll(this UIControl ctrl, float time)
        {
            ctrl.TweenScaleUp(time, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenShow(time * 0.25f, ScaleFuncs.Linear);
            ctrl.TweenRotate(MathUtil.TwoPi, time * 0.25f, ScaleFuncs.Linear);
        }

        public static void HideRoll(this UIControl ctrl, float time)
        {
            ctrl.TweenScaleDown(time, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenHide(time * 0.25f, ScaleFuncs.Linear);
            ctrl.TweenRotate(-MathUtil.TwoPi, time * 0.25f, ScaleFuncs.Linear);
        }

        public static void ScaleInScaleOut(this UIControl ctrl, float from, float to, float time)
        {
            ctrl.TweenScaleBounce(from, to, time, ScaleFuncs.Linear);
        }
    }
}
