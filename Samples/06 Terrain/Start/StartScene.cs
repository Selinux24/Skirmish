using Engine;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Terrain.Start
{
    using Terrain.PerlinNoise;
    using Terrain.Rts;

    class StartScene : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private Model backGround = null;
        private UITextArea title = null;
        private UIButton scenePerlinNoiseButton = null;
        private UIButton sceneRtsButton = null;
        private UIButton exitButton = null;

        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";
        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.DarkSeaGreen, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.OrangeRed, 1.5f);

        private bool sceneReady = false;

        public StartScene(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;

            await LoadResourcesAsync(InitializeAssets(), PrepareAssets);
        }
        private async Task InitializeAssets()
        {
            #region Cursor

            var cursorDesc = new UICursorDescription()
            {
                ContentPath = "Start",
                Textures = new[] { "pointer.png" },
                Height = 48,
                Width = 48,
                Centered = false,
                Delta = new Vector2(-14f, -7f),
                BaseColor = Color.White,
            };
            await this.AddComponentUICursor("Cursor", cursorDesc, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("Start", "SkyPlane.xml"),
            };
            backGround = await this.AddComponentModel("Background", backGroundDesc, SceneObjectUsages.UI);

            #endregion

            #region Title text

            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Bold);

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.TextForeColor = Color.Gold;
            titleDesc.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);
            titleDesc.TextHorizontalAlign = HorizontalTextAlign.Center;
            titleDesc.TextVerticalAlign = VerticalTextAlign.Middle;

            title = await this.AddComponentUITextArea("Title", titleDesc, layerHUD);
            title.GrowControlWithText = false;

            #endregion

            #region Scene buttons

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold);

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton("Start/buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            startButtonDesc.Width = 275;
            startButtonDesc.Height = 65;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.Font = buttonsFont;
            startButtonDesc.TextForeColor = Color.Gold;
            startButtonDesc.TextHorizontalAlign = HorizontalTextAlign.Center;
            startButtonDesc.TextVerticalAlign = VerticalTextAlign.Middle;

            scenePerlinNoiseButton = await this.AddComponentUIButton("ButtonPerlinNoise", startButtonDesc, layerHUD);
            sceneRtsButton = await this.AddComponentUIButton("ButtonRts", startButtonDesc, layerHUD);

            #endregion

            #region Exit button

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton("Start/buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            exitButtonDesc.Width = 275;
            exitButtonDesc.Height = 65;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.Font = buttonsFont;
            exitButtonDesc.TextHorizontalAlign = HorizontalTextAlign.Center;
            exitButtonDesc.TextVerticalAlign = VerticalTextAlign.Middle;

            exitButton = await this.AddComponentUIButton("ButtonExit", exitButtonDesc, layerHUD);

            #endregion
        }
        private void PrepareAssets(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            title.Text = "Terrain Tests";
            scenePerlinNoiseButton.Caption.Text = "Perlin Noise";
            sceneRtsButton.Caption.Text = "Real Time Strategy Game";
            exitButton.Caption.Text = "Exit";

            var sceneButtons = new[]
            {
                scenePerlinNoiseButton,
                sceneRtsButton,
            };

            int numButtons = sceneButtons.Length + 1;
            int div = numButtons + 1;
            int h = 4;
            int hv = h - 1;

            var rect = Game.Form.RenderRectangle;
            rect.Height /= 2;
            title.SetRectangle(rect);
            title.Anchor = Anchors.Center;

            for (int i = 0; i < sceneButtons.Length; i++)
            {
                sceneButtons[i].Left = ((Game.Form.RenderWidth / div) * (i + 1)) - (scenePerlinNoiseButton.Width / 2);
                sceneButtons[i].Top = (Game.Form.RenderHeight / h) * hv - (scenePerlinNoiseButton.Height / 2);
                sceneButtons[i].JustReleased += SceneButtonClick;
                sceneButtons[i].MouseEnter += SceneButtonMouseEnter;
                sceneButtons[i].MouseLeave += SceneButtonMouseLeave;
            }

            exitButton.Left = (Game.Form.RenderWidth / div) * numButtons - (exitButton.Width / 2);
            exitButton.Top = (Game.Form.RenderHeight / h) * hv - (exitButton.Height / 2);
            exitButton.JustReleased += ExitButtonClick;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;

            sceneReady = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float xmouse = (((float)Game.Input.MouseX / (float)Game.Form.RenderWidth) - 0.5f) * 2f;
            float ymouse = (((float)Game.Input.MouseY / (float)Game.Form.RenderHeight) - 0.5f) * 2f;

            float d = 0.25f;
            float vx = 0.5f;
            float vy = 0.25f;

            Vector3 position = Vector3.Zero;
            position.X = +((xmouse * d) + (0.2f * (float)Math.Cos(vx * Game.GameTime.TotalSeconds)));
            position.Y = -((ymouse * d) + (0.1f * (float)Math.Sin(vy * Game.GameTime.TotalSeconds)));

            Camera.Position = new Vector3(0, 0, -5f);
            Camera.LookTo(position);
        }

        private void SceneButtonClick(object sender, EventArgs e)
        {
            if (!sceneReady)
            {
                return;
            }

            if (sender == scenePerlinNoiseButton)
            {
                Game.SetScene<PerlinNoiseScene>();
            }
            else if (sender == sceneRtsButton)
            {
                Game.SetScene<RtsScene>();
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

            Game.Exit();
        }
    }

    static class UIControlExtensions
    {
        public static void Show(this UIControl ctrl, long milliseconds)
        {
            ctrl.TweenShow(milliseconds, ScaleFuncs.Linear);
        }

        public static void Hide(this UIControl ctrl, long milliseconds)
        {
            ctrl.TweenHide(milliseconds, ScaleFuncs.Linear);
        }

        public static void Roll(this UIControl ctrl, long milliseconds)
        {
            ctrl.TweenRotate(MathUtil.TwoPi, milliseconds, ScaleFuncs.Linear);
            ctrl.TweenScale(1, 0.5f, milliseconds, ScaleFuncs.QuinticEaseOut);
        }

        public static void ShowRoll(this UIControl ctrl, long milliseconds)
        {
            ctrl.TweenScaleUp(milliseconds, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenShow(milliseconds / 4, ScaleFuncs.Linear);
            ctrl.TweenRotate(MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void HideRoll(this UIControl ctrl, long milliseconds)
        {
            ctrl.TweenScaleDown(milliseconds, ScaleFuncs.QuinticEaseOut);
            ctrl.TweenHide(milliseconds / 4, ScaleFuncs.Linear);
            ctrl.TweenRotate(-MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void ScaleInScaleOut(this UIControl ctrl, float from, float to, long milliseconds)
        {
            ctrl.TweenScaleBounce(from, to, milliseconds, ScaleFuncs.Linear);
        }
    }
}
