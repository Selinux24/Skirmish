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

        private UIControlTweener uiTweener;

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
            LoadResourcesAsync(
                new[]
                {
                    InitializeTweener(),
                    InitializeAssets()
                },
                InitializeComponentsCompleted);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeAssets()
        {
            #region Cursor

            var cursorDesc = UICursorDescription.Default("Start/pointer.png", 48, 48, false, new Vector2(-14f, -7f));
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("Start", "SkyPlane.json"),
            };
            backGround = await AddComponent<Model, ModelDescription>("Background", "Background", backGroundDesc);

            #endregion

            #region Title text

            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Bold, true);
            titleFont.CustomKeycodes = new[] { '✌' };

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.TextForeColor = Color.Gold;
            titleDesc.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);
            titleDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            titleDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc, layerHUD);
            title.GrowControlWithText = false;
            title.Text = "Terrain Tests ✌";

            #endregion

            #region Scene buttons

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, true);
            buttonsFont.CustomKeycodes = new[] { '➀', '➁' };

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "Start/buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            startButtonDesc.Width = 275;
            startButtonDesc.Height = 65;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextForeColor = Color.Gold;
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            scenePerlinNoiseButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonPerlinNoise", "ButtonPerlinNoise", startButtonDesc, layerHUD);
            scenePerlinNoiseButton.MouseClick += SceneButtonClick;
            scenePerlinNoiseButton.MouseEnter += SceneButtonMouseEnter;
            scenePerlinNoiseButton.MouseLeave += SceneButtonMouseLeave;
            scenePerlinNoiseButton.Caption.Text = "➀ Perlin Noise";

            sceneRtsButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonRts", "ButtonRts", startButtonDesc, layerHUD);
            sceneRtsButton.MouseClick += SceneButtonClick;
            sceneRtsButton.MouseEnter += SceneButtonMouseEnter;
            sceneRtsButton.MouseLeave += SceneButtonMouseLeave;
            sceneRtsButton.Caption.Text = "➁ Real Time Strategy Game";

            #endregion

            #region Exit button

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "Start/buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            exitButtonDesc.Width = 275;
            exitButtonDesc.Height = 65;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            exitButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonExit", "ButtonExit", exitButtonDesc, layerHUD);
            exitButton.MouseClick += ExitButtonClick;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;
            exitButton.Caption.Text = "Exit";

            #endregion
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Renderer.PostProcessingObjectsEffects.AddToneMapping(Engine.BuiltIn.PostProcess.BuiltInToneMappingTones.Uncharted2);

            UpdateLayout();

            backGround.Manipulator.SetScale(1.5f, 1.25f, 1.5f);

            sceneReady = true;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();
            UpdateLayout();
        }
        private void UpdateLayout()
        {
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
            }

            exitButton.Left = (Game.Form.RenderWidth / div) * numButtons - (exitButton.Width / 2);
            exitButton.Top = (Game.Form.RenderHeight / h) * hv - (exitButton.Height / 2);
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

            if (sender == scenePerlinNoiseButton)
            {
                Game.SetScene<PerlinNoiseScene>();
            }
            else if (sender == sceneRtsButton)
            {
                Game.SetScene<RtsScene>();
            }
        }
        private void SceneButtonMouseEnter(IUIControl sender, MouseEventArgs e)
        {
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

    static class UIControlExtensions
    {
        public static void Show(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenShow(ctrl, milliseconds, ScaleFuncs.Linear);
        }

        public static void Hide(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenHide(ctrl, milliseconds, ScaleFuncs.Linear);
        }

        public static void Roll(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenRotate(ctrl, MathUtil.TwoPi, milliseconds, ScaleFuncs.Linear);
            tweener.TweenScale(ctrl, 1, 0.5f, milliseconds, ScaleFuncs.QuinticEaseOut);
        }

        public static void ShowRoll(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenScaleUp(ctrl, milliseconds, ScaleFuncs.QuinticEaseOut);
            tweener.TweenShow(ctrl, milliseconds / 4, ScaleFuncs.Linear);
            tweener.TweenRotate(ctrl, MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void HideRoll(this UIControlTweener tweener, IUIControl ctrl, long milliseconds)
        {
            tweener.TweenScaleDown(ctrl, milliseconds, ScaleFuncs.QuinticEaseOut);
            tweener.TweenHide(ctrl, milliseconds / 4, ScaleFuncs.Linear);
            tweener.TweenRotate(ctrl, -MathUtil.TwoPi, milliseconds / 4, ScaleFuncs.Linear);
        }

        public static void ScaleInScaleOut(this UIControlTweener tweener, IUIControl ctrl, float from, float to, long milliseconds)
        {
            tweener.TweenScaleBounce(ctrl, from, to, milliseconds, ScaleFuncs.Linear);
        }
    }
}
