using Engine;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace TerrainSamples.SceneStart
{
    class StartScene : Scene
    {
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private UIControlTweener uiTweener;

        private Model backGround = null;
        private UITextArea title = null;

        private UIButton sceneCrowdsButton = null;
        private UIButton sceneGridButton = null;
        private UIButton sceneHeightmapButton = null;
        private UIButton sceneModularDungeonButton = null;
        private UIButton sceneNavMeshTestButton = null;
        private UIButton scenePerlinNoiseButton = null;
        private UIButton sceneRtsButton = null;
        private UIButton sceneSkyboxButton = null;
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

            var cursorDesc = UICursorDescription.Default("SceneStart/pointer.png", 48, 48, false, new Vector2(-14f, -7f));
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc, layerCursor);

            #endregion

            #region Background

            var backGroundDesc = new ModelDescription()
            {
                Content = ContentDescription.FromFile("SceneStart", "SkyPlane.json"),
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

            var startButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "SceneStart/buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            startButtonDesc.Width = 275;
            startButtonDesc.Height = 65;
            startButtonDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            startButtonDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            startButtonDesc.TextForeColor = Color.Gold;
            startButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            startButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;

            sceneCrowdsButton = await InitializeButton(nameof(sceneCrowdsButton), "Crowds", startButtonDesc);
            sceneGridButton = await InitializeButton(nameof(sceneGridButton), "A* Grid", startButtonDesc);
            sceneHeightmapButton = await InitializeButton(nameof(sceneHeightmapButton), "Heightmap", startButtonDesc);
            sceneModularDungeonButton = await InitializeButton(nameof(sceneModularDungeonButton), "Modular Dungeon", startButtonDesc);
            sceneNavMeshTestButton = await InitializeButton(nameof(sceneNavMeshTestButton), "Navigation Mesh", startButtonDesc);
            scenePerlinNoiseButton = await InitializeButton(nameof(scenePerlinNoiseButton), "Perlin Noise", startButtonDesc);
            sceneRtsButton = await InitializeButton(nameof(sceneRtsButton), "Real Time Strategy Game", startButtonDesc);
            sceneSkyboxButton = await InitializeButton(nameof(sceneSkyboxButton), "Skybox", startButtonDesc);

            #endregion

            #region Exit button

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "SceneStart/buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
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
            var rect = Game.Form.RenderRectangle;
            rect.Height /= 2;
            title.SetRectangle(rect);
            title.Anchor = Anchors.Center;

            var sceneButtons = new[]
            {
                sceneCrowdsButton,
                sceneGridButton,
                sceneHeightmapButton,
                sceneModularDungeonButton,
                sceneNavMeshTestButton,
                scenePerlinNoiseButton,
                sceneRtsButton,
                sceneSkyboxButton,
                exitButton,
            };

            int numButtons = sceneButtons.Length;
            int cols = 4;
            int rowCount = (int)Math.Ceiling(numButtons / (float)cols);
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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

            if (sender == sceneCrowdsButton) Game.SetScene<SceneCrowds.CrowdsScene>();
            if (sender == sceneGridButton) Game.SetScene<SceneGrid.GridScene>();
            if (sender == sceneHeightmapButton) Game.SetScene<SceneHeightmap.HeightmapScene>();
            if (sender == sceneModularDungeonButton) Game.SetScene<SceneModularDungeon.ModularDungeonScene>();
            if (sender == sceneNavMeshTestButton) Game.SetScene<SceneNavmeshTest.NavmeshTestScene>();
            if (sender == scenePerlinNoiseButton) Game.SetScene<ScenePerlinNoise.PerlinNoiseScene>();
            if (sender == sceneRtsButton) Game.SetScene<SceneRts.RtsScene>();
            if (sender == sceneSkyboxButton) Game.SetScene<SceneSkybox.SkyboxScene>();
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
