using Engine;
using Engine.BuiltIn.Components.Flares;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.UI;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Linq;
using System.Threading.Tasks;

namespace BasicSamples.SceneCascadedShadows
{
    /// <summary>
    /// Cascade shadows scene test
    /// </summary>
    public class CascadedShadowsScene : Scene
    {
        private const float spaceSize = 80;
        private const string fontFamilyName = "Arial";
        private const string resourceFlare = "Common/lensFlare/";
        private const string resourceGlowString = "lfGlow.png";
        private const string resourceFlare1String = "lfFlare1.png";
        private const string resourceFlare2String = "lfFlare2.png";
        private const string resourceFlare3String = "lfFlare3.png";
        private const string resourceFlare4String = "lfFlare4.png";
        private const string resourceFloorDiffuse = "Common/floors/dirt/dirt002.dds";
        private const string resourceFloorNormal = "Common/floors/dirt/normal001.dds";
        private const string resourceObelisk = "Common/buildings/obelisk/";
        private const string resourceTrees = "Common/trees/";

        private bool uiReady = false;
        private bool gameReady = false;

        private UITextArea title = null;
        private UITextArea help = null;
        private UITextArea selector = null;
        private Sprite backPanel = null;
        private UIConsole console = null;
        private bool showHelp = false;
        private readonly string helpText1 = $"{Color.Yellow}Press {Color.Red}F1 {Color.Yellow}for help.";
        private readonly string helpText2 = "F1 close help. F2 show shadow buffers.";

        private Sprite spLevel1 = null;
        private Sprite spLevel2 = null;
        private Sprite spLevel3 = null;
        private Sprite spSelect1 = null;
        private Sprite spSelect2 = null;
        IUIControl currentSelector = null;
        float min = 0;
        float max = 0;

        private UITextArea caption1 = null;
        private UITextArea caption2 = null;
        private UITextArea caption3 = null;

        private UITextureRenderer bufferDrawer1 = null;
        private UITextureRenderer bufferDrawer2 = null;
        private UITextureRenderer bufferDrawer3 = null;
        private bool showBuffers = false;

        private ModelInstanced buildingObelisks = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public CascadedShadowsScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 1000;
            Camera.Goto(-10, 8, 20f);
            Camera.LookTo(0, 0, 0);
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeUI();
        }

        private void InitializeUI()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeUIText,
                    InitializeUILevelsControl,
                    InitializeUIDrawers,
                ],
                InitializeUICompleted);

            LoadResources(group);
        }
        private async Task InitializeUIText()
        {
            var defaultFont20 = Engine.UI.FontDescription.FromFamily(fontFamilyName, 20);
            var defaultFont14 = Engine.UI.FontDescription.FromFamily(fontFamilyName, 14);
            var defaultFont12 = Engine.UI.FontDescription.FromFamily(fontFamilyName, 12);

            var titleDesc = new UITextAreaDescription
            {
                Font = defaultFont20,
                Text = "Cascaded Shadows",
                TextForeColor = Color.Yellow,
                TextShadowColor = Color.OrangeRed
            };
            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc);

            var helpDesc = new UITextAreaDescription
            {
                Font = defaultFont14,
                Text = helpText1,
                TextForeColor = Color.LightBlue,
                TextShadowColor = Color.DarkBlue
            };
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", helpDesc);

            var selectorDesc = new UITextAreaDescription
            {
                Font = defaultFont12,
                TextForeColor = Color.LightBlue,
            };
            selector = await AddComponentUI<UITextArea, UITextAreaDescription>("SelectorText", "SelectorText", selectorDesc);

            backPanel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), LayerUI - 1);

            var consoleDesc = UIConsoleDescription.Default(Color.DarkSlateBlue);
            consoleDesc.MaxTextLength = 5000;
            console = await AddComponentUI<UIConsole, UIConsoleDescription>("Console", "Console", consoleDesc, LayerUI + 1);
            console.Visible = false;
        }
        private async Task InitializeUILevelsControl()
        {
            spLevel1 = await AddComponentUI<Sprite, SpriteDescription>("High Level", "High Level", SpriteDescription.Default(new Color(0x17, 0x3F, 0x5F, 0xFF)), LayerUI);
            spLevel2 = await AddComponentUI<Sprite, SpriteDescription>("Medium Level", "Medium Level", SpriteDescription.Default(new Color(0x20, 0x63, 0x9B, 0xFF)), LayerUI);
            spLevel3 = await AddComponentUI<Sprite, SpriteDescription>("Low Level", "Low Level", SpriteDescription.Default(new Color(0x3C, 0xAE, 0xA3, 0xFF)), LayerUI);

            spSelect1 = await AddComponentUI<Sprite, SpriteDescription>("First selector", "First selector", SpriteDescription.Default(new Color(0xF6, 0xD5, 0x5C, 0xFF)), LayerUI + 1);
            spSelect1.EventsEnabled = true;
            spSelect1.MouseJustPressed += PbJustPressed;
            spSelect1.MouseJustReleased += PbJustReleased;

            spSelect2 = await AddComponentUI<Sprite, SpriteDescription>("Second selector", "Second selector", SpriteDescription.Default(new Color(0xF6, 0xD5, 0x5C, 0xFF)), LayerUI + 1);
            spSelect2.EventsEnabled = true;
            spSelect2.MouseJustPressed += PbJustPressed;
            spSelect2.MouseJustReleased += PbJustReleased;
        }
        private async Task InitializeUIDrawers()
        {
            var shadowMap = Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);

            var bufferDesc = UITextureRendererDescription.Default();
            bufferDesc.Channel = ColorChannels.Red;
            bufferDesc.StartsVisible = false;

            bufferDrawer1 = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("Sh1", "Sh1", bufferDesc);
            bufferDrawer2 = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("Sh2", "Sh2", bufferDesc);
            bufferDrawer3 = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("Sh3", "Sh3", bufferDesc);

            bufferDrawer1.Texture = shadowMap;
            bufferDrawer2.Texture = shadowMap;
            bufferDrawer3.Texture = shadowMap;

            bufferDrawer1.TextureIndex = 0;
            bufferDrawer2.TextureIndex = 1;
            bufferDrawer3.TextureIndex = 2;

            var defaultFont14 = Engine.UI.FontDescription.FromFamily(fontFamilyName, 14);

            var captionDesc = new UITextAreaDescription
            {
                Font = defaultFont14,
                TextForeColor = Color.LightBlue,
                TextShadowColor = Color.DarkBlue,
                GrowControlWithText = false,
                StartsVisible = false,
            };

            caption1 = await AddComponentUI<UITextArea, UITextAreaDescription>("Caption1", "Caption1", captionDesc, LayerUI + 1);
            caption2 = await AddComponentUI<UITextArea, UITextAreaDescription>("Caption2", "Caption2", captionDesc, LayerUI + 1);
            caption3 = await AddComponentUI<UITextArea, UITextAreaDescription>("Caption3", "Caption3", captionDesc, LayerUI + 1);

            caption1.Text = $"Hight Level Map";
            caption2.Text = $"Medium Level Map";
            caption3.Text = $"Low Level Map";
        }
        private void InitializeUICompleted(LoadResourcesResult res)
        {
            res.ThrowExceptions();

            uiReady = true;

            selector.Visible = true;

            UpdateLayout();

            InitializeObjects();
        }

        private void InitializeObjects()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeFloor,
                    InitializeBuildingObelisk,
                    InitializeTree,
                    InitializeSkyEffects,
                ],
                InitializeObjectsCompleted);

            LoadResources(group);
        }
        private async Task InitializeFloor()
        {
            float l = spaceSize;
            float h = 0f;

            var geo = GeometryUtil.CreatePlane(l, h, Vector3.Up);
            geo.Uvs = geo.Uvs.Select(uv => uv * 5f);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourceFloorDiffuse;
            mat.NormalMapTexture = resourceFloorNormal;

            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.Directional,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            await AddComponentGround<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeBuildingObelisk()
        {
            var desc = new ModelInstancedDescription()
            {
                Instances = 4,
                CastShadow = ShadowCastingAlgorihtms.Directional,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(resourceObelisk, "Obelisk.json"),
            };

            buildingObelisks = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Obelisk", "Obelisk", desc);
        }
        private async Task InitializeTree()
        {
            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.Directional,
                UseAnisotropicFiltering = true,
                BlendMode = BlendModes.OpaqueTransparent,
                Content = ContentDescription.FromFile(resourceTrees, "Tree.json"),
            };

            await AddComponent<Model, ModelDescription>("Tree", "Tree", desc);
        }
        private async Task InitializeSkyEffects()
        {
            await AddComponentEffect<LensFlare, LensFlareDescription>("Flare", "Flare", new()
            {
                ContentPath = resourceFlare,
                GlowTexture = resourceGlowString,
                Flares =
                [
                    new (-0.7f, 0.7f, new Color( 50, 100,  25), resourceFlare3String),
                    new (-0.5f, 0.7f, new Color( 50,  25,  50), resourceFlare1String),
                    new (-0.3f, 0.7f, new Color(200,  50,  50), resourceFlare2String),
                    new ( 0.0f, 5.6f, new Color( 25,  25,  25), resourceFlare4String),
                    new ( 0.3f, 0.4f, new Color(100, 255, 200), resourceFlare1String),
                    new ( 0.6f, 0.9f, new Color( 50, 100,  50), resourceFlare2String),
                    new ( 0.7f, 0.4f, new Color( 50, 200, 200), resourceFlare2String),
                    new ( 1.2f, 1.0f, new Color(100,  50,  50), resourceFlare1String),
                    new ( 1.5f, 1.5f, new Color( 50, 100,  50), resourceFlare1String),
                    new ( 2.0f, 1.4f, new Color( 25,  50, 100), resourceFlare3String),
                ]
            });
        }
        private void InitializeObjectsCompleted(LoadResourcesResult res)
        {
            res.ThrowExceptions();

            StartLights();

            buildingObelisks[0].Manipulator.SetPosition(+5, 0, +5);
            buildingObelisks[1].Manipulator.SetPosition(+5, 0, -5);
            buildingObelisks[2].Manipulator.SetPosition(-5, 0, +5);
            buildingObelisks[3].Manipulator.SetPosition(-5, 0, -5);

            gameReady = true;
        }
        private void StartLights()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            Lights.KeyLight.Enabled = true;
            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-1, -1, -3));

            Lights.BackLight.Enabled = true;
            Lights.FillLight.Enabled = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!uiReady)
            {
                return;
            }

            if (!gameReady)
            {
                return;
            }

            // Camera
            UpdateCamera(gameTime);

            // Input
            UpdateGameInput();

            // Selector state
            UpdateSelector();
        }

        private void UpdateCamera(IGameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateGameInput()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                showHelp = !showHelp;

                help.Text = showHelp ? helpText2 : helpText1;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                showBuffers = !showBuffers;

                bufferDrawer1.Visible = showBuffers;
                caption1.Visible = showBuffers;
                bufferDrawer2.Visible = showBuffers;
                caption2.Visible = showBuffers;
                bufferDrawer3.Visible = showBuffers;
                caption3.Visible = showBuffers;

                UpdateLayout();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (Game.Input.KeyJustReleased(Keys.Oem5))
            {
                console.Toggle();
            }
        }
        private void UpdateSelector()
        {
            selector.Text = $"High: {GameEnvironment.ShadowDistanceHigh:0.00} - Medium: {GameEnvironment.ShadowDistanceMedium:0.00} - Low: {GameEnvironment.ShadowDistanceLow:0.00}";

            if (currentSelector != null)
            {
                currentSelector.Left = MathUtil.Clamp(Game.Input.MouseX, min, max) - 5f;
            }

            float select1Pos = spSelect1.Left + 5;
            float select2Pos = spSelect2.Left + 5;

            float totalWidth = Game.Form.RenderWidth * 0.9f;
            float left = (Game.Form.RenderWidth - totalWidth) * 0.5f;

            spLevel1.Left = left;
            spLevel2.Left = select1Pos;
            spLevel3.Left = select2Pos;

            spLevel1.Width = select1Pos - left;
            spLevel2.Width = select2Pos - left - spLevel1.Width;
            spLevel3.Width = totalWidth - spLevel2.Width - spLevel1.Width;

            float tLevel = GameEnvironment.ShadowDistanceHigh + GameEnvironment.ShadowDistanceMedium + GameEnvironment.ShadowDistanceLow;
            GameEnvironment.ShadowDistanceHigh = spLevel1.Width / totalWidth * tLevel;
            GameEnvironment.ShadowDistanceMedium = spLevel2.Width / totalWidth * tLevel;
            GameEnvironment.ShadowDistanceLow = spLevel3.Width / totalWidth * tLevel;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            UpdateLayoutUI();

            float selectorTop;
            if (showBuffers)
            {
                selectorTop = Game.Form.RenderHeight - (Game.Form.RenderHeight * 0.33f) - 50f;

                UpdateLayoutUIDrawers();
            }
            else
            {
                selectorTop = Game.Form.RenderHeight - 50f;
            }

            UpdateLayoutUISelectors(selectorTop);
        }
        private void UpdateLayoutUI()
        {
            title.Anchor = Anchors.TopLeft;

            help.Left = 0;
            help.Top = title.AbsoluteRectangle.Bottom + 5;

            backPanel.SetPosition(Vector2.Zero);
            backPanel.Height = help.Top + help.Height + 5;
            backPanel.Width = Game.Form.RenderWidth;
        }
        private void UpdateLayoutUISelectors(float top)
        {
            float totalWidth = Game.Form.RenderWidth * 0.9f;
            float left = (Game.Form.RenderWidth - totalWidth) * 0.5f;

            float tLevel = GameEnvironment.ShadowDistanceHigh + GameEnvironment.ShadowDistanceMedium + GameEnvironment.ShadowDistanceLow;
            float hLevel = GameEnvironment.ShadowDistanceHigh / tLevel;
            float mLevel = GameEnvironment.ShadowDistanceMedium / tLevel;
            float lLevel = GameEnvironment.ShadowDistanceLow / tLevel;

            spLevel1.Width = totalWidth * hLevel;
            spLevel2.Width = totalWidth * mLevel;
            spLevel3.Width = totalWidth * lLevel;

            spLevel1.Height = 20f;
            spLevel2.Height = 20f;
            spLevel3.Height = 20f;

            selector.Left = left;
            spLevel1.Left = left;
            spLevel2.Left = left + spLevel1.Width;
            spLevel3.Left = left + spLevel1.Width + spLevel2.Width;

            selector.Top = top;
            top += (int)selector.Height + 5;
            spLevel1.Top = top;
            spLevel2.Top = top;
            spLevel3.Top = top;

            selector.Width = totalWidth;
            spSelect1.Width = 10;
            spSelect1.Height = 25;
            spSelect1.SetPosition(spLevel2.Left - 5f, spLevel2.Top - 2.5f);

            spSelect2.Width = 10;
            spSelect2.Height = 25;
            spSelect2.SetPosition(spLevel3.Left - 5f, spLevel3.Top - 2.5f);
        }
        private void UpdateLayoutUIDrawers()
        {
            int height = (int)(Game.Form.RenderHeight * 0.33f);
            int top = Game.Form.RenderHeight - height;
            int width = (int)(Game.Form.RenderWidth / 3f);

            bufferDrawer1.Height = height;
            bufferDrawer2.Height = height;
            bufferDrawer3.Height = height;

            bufferDrawer1.Width = width - 1f;
            bufferDrawer2.Width = width - 1f;
            bufferDrawer3.Width = width - 1f;

            bufferDrawer1.Anchor = Anchors.BottomLeft;
            bufferDrawer2.Anchor = Anchors.BottomCenter;
            bufferDrawer3.Anchor = Anchors.BottomRight;

            caption1.Width = width;
            caption2.Width = width;
            caption3.Width = width;

            caption1.Left = 0;
            caption2.Left = width;
            caption3.Left = width + width;

            caption1.Top = top;
            caption2.Top = top;
            caption3.Top = top;

            caption1.TextHorizontalAlign = TextHorizontalAlign.Center;
            caption2.TextHorizontalAlign = TextHorizontalAlign.Center;
            caption3.TextHorizontalAlign = TextHorizontalAlign.Center;
        }

        private void PbJustPressed(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            currentSelector = sender;

            if (sender == spSelect1)
            {
                min = spLevel1.Left;
                max = spLevel2.Left + spLevel2.Width;
            }

            if (sender == spSelect2)
            {
                min = spLevel2.Left;
                max = spLevel3.Left + spLevel3.Width;
            }
        }
        private void PbJustReleased(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            currentSelector = null;
            min = 0;
            max = 0;
        }
    }
}
