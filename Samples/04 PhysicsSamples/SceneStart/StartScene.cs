using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Drawers.PostProcess;
using Engine.BuiltIn.UI;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace PhysicsSamples.SceneStart
{
    class StartScene : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private UIControlTweener uiTweener;

        private Model backGround = null;
        private UITextArea title = null;

        private UIButton[] sceneButtons;
        private UIButton scenePhysicsButton = null;
        private UIButton exitButton = null;

        private readonly string resourcesFolder = "SceneStart";
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

        public override void Initialize()
        {
            base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTweener,
                    InitializeCursor,
                    InitializeBackground,
                    InitializeAssets,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default("pointer.png", 48, 48, false, new Vector2(-14f, -7f));
            cursorDesc.ContentPath = resourcesFolder;
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc, layerCursor);
        }
        private async Task InitializeBackground()
        {
            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile(resourcesFolder, "SkyPlane.json"),
            };
            backGround = await AddComponent<Model, ModelDescription>("Background", "Background", backGroundDesc);
        }
        private async Task InitializeAssets()
        {
            #region Title text

            var titleFont = TextDrawerDescription.FromFamily(titleFonts, 72, FontMapStyles.Bold, true);
            titleFont.ContentPath = resourcesFolder;

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.ContentPath = resourcesFolder;
            titleDesc.TextForeColor = Color.Gold;
            titleDesc.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);
            titleDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            titleDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc, layerHUD);
            title.GrowControlWithText = false;
            title.Text = "Samples";

            #endregion

            #region Scene buttons

            var buttonsFont = TextDrawerDescription.FromFamily(buttonFonts, 20, FontMapStyles.Bold, true);
            buttonsFont.ContentPath = resourcesFolder;

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            startButtonDesc.ContentPath = resourcesFolder;
            startButtonDesc.Width = 275;
            startButtonDesc.Height = 65;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextForeColor = Color.Gold;
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            scenePhysicsButton = await InitializeButton(nameof(scenePhysicsButton), "Physics", startButtonDesc);

            #endregion

            #region Exit button

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            exitButtonDesc.ContentPath = resourcesFolder;
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

            sceneButtons =
            [
                scenePhysicsButton,
                exitButton,
            ];
        }
        private async Task<UIButton> InitializeButton(string name, string caption, UIButtonDescription desc)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, layerHUD);
            button.MouseClick += SceneButtonClick;
            button.MouseEnter += SceneButtonMouseEnter;
            button.MouseLeave += SceneButtonMouseLeave;
            button.Caption.Text = caption;

            return button;
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Renderer.PostProcessingObjectsEffects.AddToneMapping(BuiltInToneMappingTones.Uncharted2);

            UpdateLayout();

            backGround.Manipulator.SetScaling(1.5f, 1.25f, 1.5f);

            sceneReady = true;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            var rect = Game.Form.RenderRectangle;
            rect.Height /= 2;
            rect.Top = 0;
            title.SetRectangle(rect);
            title.Anchor = Anchors.HorizontalCenter;

            int numButtons = sceneButtons.Length;
            int cols = 4;
            int rowCount = (int)MathF.Ceiling(numButtons / (float)cols);
            int div = cols + 1;

            int h = 3;
            int hv = h - 1;

            float butWidth = exitButton.Width;
            float butHeight = exitButton.Height;

            int formWidth = Game.Form.RenderWidth;
            int formHeight = Game.Form.RenderHeight;

            int i = 0;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (i >= sceneButtons.Length)
                    {
                        break;
                    }

                    sceneButtons[i].Left = (formWidth / div * (col + 1)) - (butWidth / 2);
                    sceneButtons[i].Top = formHeight / h * hv - (butHeight / 2) + (row * (butHeight + 10));
                    i++;
                }
            }
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            float xmouse = ((Game.Input.MouseX / (float)Game.Form.RenderWidth) - 0.5f) * 2f;
            float ymouse = ((Game.Input.MouseY / (float)Game.Form.RenderHeight) - 0.5f) * 2f;

            float d = 0.25f;
            float vx = 0.5f;
            float vy = 0.25f;

            Vector3 position = Vector3.Zero;
            position.X = +((xmouse * d) + (0.2f * MathF.Cos(vx * Game.GameTime.TotalSeconds)));
            position.Y = -((ymouse * d) + (0.1f * MathF.Sin(vy * Game.GameTime.TotalSeconds)));

            Camera.SetPosition(0, 0, -5f);
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

            if (sender == scenePhysicsButton) Game.SetScene<ScenePhysics.PhysicsScene>();
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
}
