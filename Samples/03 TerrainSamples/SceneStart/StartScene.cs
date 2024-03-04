using Engine;
using Engine.Content;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneStart
{
    class StartScene : Scene
    {
        private const int layerHUD = 50;
        private const int layerCursor = 100;

        private UIControlTweener uiTweener;

        private Model backGround = null;
        private UITextArea title = null;

        private UIButton[] sceneButtons;
        private UIButton sceneCrowdsButton = null;
        private UIButton sceneGridButton = null;
        private UIButton sceneHeightmapButton = null;
        private UIButton sceneModularDungeonButton = null;
        private UIButton sceneNavMeshTestButton = null;
        private UIButton scenePerlinNoiseButton = null;
        private UIButton sceneRtsButton = null;
        private UIButton sceneSkyboxButton = null;
        private UIButton exitButton = null;
        private UITabPanel modularDungeonTabs = null;
        private readonly string modularDungeonTabsPath = "scenemodulardungeon/resources";

        private readonly string resourcesFolder = "SceneStart";
        private readonly string titleFonts = "Showcard Gothic, Verdana, Consolas";
        private readonly string buttonFonts = "Verdana, Consolas";
        private readonly string mediumControlsFont = "HelveticaNeueHv.ttf";
        private readonly string largeControlsFont = "HelveticaNeue Medium.ttf";
        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.DarkSeaGreen, 1.5f);
        private readonly Color exitButtonColor = Color.AdjustSaturation(Color.OrangeRed, 1.5f);
        private Color4 SceneButtonColorBase { get { return new Color4(sceneButtonColor.RGB(), 0.8f); } }
        private Color4 SceneButtonColorHighlight { get { return new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f); } }

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
                    InitializeCursor(),
                    InitializeBackground(),
                    InitializeAssets(),
                },
                InitializeComponentsCompleted);
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
            titleFont.CustomKeycodes = new[] { '✌' };

            var titleDesc = UITextAreaDescription.Default(titleFont);
            titleDesc.ContentPath = resourcesFolder;
            titleDesc.TextForeColor = Color.Gold;
            titleDesc.TextShadowColor = new Color4(Color.LightYellow.RGB(), 0.25f);
            titleDesc.TextShadowDelta = new Vector2(4, 4);
            titleDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            titleDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            titleDesc.StartsVisible = false;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc, layerHUD);
            title.GrowControlWithText = false;
            title.Text = "✌ Terrain Samples ✌";

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
            startButtonDesc.StartsVisible = false;

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

            var exitButtonDesc = UIButtonDescription.DefaultTwoStateButton(buttonsFont, "buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            exitButtonDesc.ContentPath = resourcesFolder;
            exitButtonDesc.Width = 275;
            exitButtonDesc.Height = 65;
            exitButtonDesc.ColorReleased = new Color4(exitButtonColor.RGB(), 0.8f);
            exitButtonDesc.ColorPressed = new Color4(exitButtonColor.RGB() * 1.2f, 0.9f);
            exitButtonDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            exitButtonDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            exitButtonDesc.StartsVisible = false;

            exitButton = await AddComponentUI<UIButton, UIButtonDescription>("ButtonExit", "ButtonExit", exitButtonDesc, layerHUD);
            exitButton.MouseClick += ExitButtonClick;
            exitButton.MouseEnter += SceneButtonMouseEnter;
            exitButton.MouseLeave += SceneButtonMouseLeave;
            exitButton.Caption.Text = "Exit";

            #endregion

            sceneButtons = new[]
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

            await InitializeModularDungeonTabs();
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
        private async Task InitializeModularDungeonTabs()
        {
            List<string> tabButtons = new();
            int basicIndex = -1;
            int backIndex = -1;

            //Load available maps from "one page dungeon" folder
            string[] mapFiles = Directory.GetFiles(modularDungeonTabsPath + "/onepagedungeons", "*.json");
            tabButtons.AddRange(mapFiles.Select(m =>
            {
                string name = Path.GetFileNameWithoutExtension(m).Replace("_", " ");
                return string.Concat(name.First().ToString().ToUpper(), name.AsSpan(1));
            }));
            basicIndex = tabButtons.Count;
            tabButtons.Add("Basic Dungeon");
            backIndex = tabButtons.Count;
            tabButtons.Add("Back");

            var largeFont = TextDrawerDescription.FromFile(largeControlsFont, 72);
            largeFont.ContentPath = resourcesFolder;
            var mediumFont = TextDrawerDescription.FromFile(mediumControlsFont, 12);
            mediumFont.ContentPath = resourcesFolder;
            var mediumClickFont = TextDrawerDescription.FromFile(mediumControlsFont, 12);
            mediumClickFont.ContentPath = resourcesFolder;

            var desc = UITabPanelDescription.Default(tabButtons.ToArray(), Color.Transparent, SceneButtonColorBase, SceneButtonColorHighlight);
            desc.ContentPath = resourcesFolder;
            desc.ButtonDescription.Font = mediumFont;
            desc.ButtonDescription.TextForeColor = Color.LightGoldenrodYellow;
            desc.ButtonDescription.TextHorizontalAlign = TextHorizontalAlign.Center;
            desc.ButtonDescription.TextVerticalAlign = TextVerticalAlign.Middle;
            desc.TabButtonsAreaSize *= 1.5f;
            desc.TabButtonsSpacing = new Spacing() { Horizontal = 10f };
            desc.TabButtonsPadding = new Padding() { Bottom = 0, Left = 5, Right = 5, Top = 5 };
            desc.TabButtonPadding = 5;
            desc.TabPanelsPadding = new Padding() { Bottom = 5, Left = 5, Right = 5, Top = 0 };
            desc.TabPanelPadding = 2;

            modularDungeonTabs = await AddComponentUI<UITabPanel, UITabPanelDescription>("ModularDungeonTabs", "ModularDungeonTabs", desc, layerHUD + 1);
            modularDungeonTabs.Visible = false;

            for (int i = 0; i < mapFiles.Length; i++)
            {
                string mapFile = mapFiles[i];
                string mapTexture = Path.ChangeExtension(mapFile, ".png");
                //string mapCnf = "OnePageDungeons/basicDungeon.config"
                //string mapCnf = "OnePageDungeons/UMRP.config"
                string mapCnf = "OnePageDungeons/MDP.config";

                var buttonDesc = UIButtonDescription.Default(mediumClickFont, mapTexture);
                buttonDesc.ContentPath = resourcesFolder;
                buttonDesc.Text = "Click image to load...";
                buttonDesc.TextForeColor = Color.DarkGray;
                buttonDesc.TextHorizontalAlign = TextHorizontalAlign.Right;
                buttonDesc.TextVerticalAlign = TextVerticalAlign.Bottom;
                var button = await CreateComponent<UIButton, UIButtonDescription>($"ModularDungeonTabs.Button_{i}", $"ModularDungeonTabs.Button_{i}", buttonDesc);
                button.MouseClick += (s, o) =>
                {
                    if (o.Buttons.HasFlag(MouseButtons.Left))
                    {
                        Game.SetScene(new SceneModularDungeon.ModularDungeonScene(Game, true, Path.GetFileName(mapFile), Path.GetFileName(mapTexture), mapCnf), SceneModes.DeferredLightning);
                    }
                };

                modularDungeonTabs.TabPanels[i].AddChild(button);
            }

            var buttonBasicDesc = UIButtonDescription.Default(largeFont, "basicdungeon/basicdungeon.png");
            buttonBasicDesc.ContentPath = modularDungeonTabsPath;
            buttonBasicDesc.Text = "Basic Dungeon";
            buttonBasicDesc.TextForeColor = Color.Gold;
            buttonBasicDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            buttonBasicDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            var buttonBasic = await CreateComponent<UIButton, UIButtonDescription>("ModularDungeonTabs.ButtonBasicDungeon", "ModularDungeonTabs.ButtonBasicDungeon", buttonBasicDesc);
            buttonBasic.MouseClick += (s, o) =>
            {
                if (o.Buttons.HasFlag(MouseButtons.Left))
                {
                    Game.SetScene(new SceneModularDungeon.ModularDungeonScene(Game, false, "basicdungeon", null, null), SceneModes.DeferredLightning);
                }
            };
            modularDungeonTabs.TabPanels[basicIndex].AddChild(buttonBasic);

            var backButton = modularDungeonTabs.TabButtons[backIndex];
            backButton.MouseClick += (s, o) =>
            {
                if (o.Buttons.HasFlag(MouseButtons.Left))
                {
                    ModularDungeonTabsHide();
                }
            };
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Renderer.PostProcessingObjectsEffects.AddToneMapping(Engine.BuiltIn.PostProcess.BuiltInToneMappingTones.Uncharted2);

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
            title.SetRectangle(rect);
            title.Anchor = Anchors.Center;

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

            modularDungeonTabs.Height = Game.Form.RenderHeight * 0.9f;
            modularDungeonTabs.Width = modularDungeonTabs.Height * 1.4f;
            modularDungeonTabs.Top = (Game.Form.RenderHeight - modularDungeonTabs.Height) * 0.5f;
            modularDungeonTabs.Left = (Game.Form.RenderWidth - modularDungeonTabs.Width) * 0.5f;

            title.Visible = true;
            sceneButtons.ToList().ForEach(b => b.Visible = true);
            exitButton.Visible = true;
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
            if (sender == sceneModularDungeonButton) ModularDungeonTabsShow();
            if (sender == sceneNavMeshTestButton) Game.SetScene<SceneNavMeshTest.NavmeshTestScene>();
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

        private void ModularDungeonTabsShow()
        {
            HideAllSceneButtons(100);

            uiTweener.Show(modularDungeonTabs, 250);
            modularDungeonTabs.SetSelectedTab(0);
        }
        private void ModularDungeonTabsHide()
        {
            uiTweener.Hide(modularDungeonTabs, 100);

            ShowAllSceneButtons(250);
        }

        private void ShowAllSceneButtons(long milliseconds)
        {
            foreach (var but in sceneButtons)
            {
                ShowButton(but, milliseconds);
            }
        }
        private void HideAllSceneButtons(long milliseconds)
        {
            foreach (var but in sceneButtons)
            {
                HideButton(but, milliseconds);
            }
        }
        private void ShowButton(IUIControl ctrl, long milliseconds)
        {
            uiTweener.ClearTween(ctrl);
            uiTweener.Show(ctrl, milliseconds);
        }
        private void HideButton(IUIControl ctrl, long milliseconds)
        {
            uiTweener.ClearTween(ctrl);
            uiTweener.Hide(ctrl, milliseconds);
        }
    }
}
