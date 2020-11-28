﻿using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest.SceneCascadedShadows
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class SceneCascadedShadows : Scene
    {
        private const float spaceSize = 80;

        private UITextArea title = null;
        private UITextArea help = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

        private Sprite spLevel1 = null;
        private Sprite spLevel2 = null;
        private Sprite spLevel3 = null;
        private Sprite spSelect1 = null;
        private Sprite spSelect2 = null;
        UIControl currentSelector = null;
        float min = 0;
        float max = 0;

        private UITextArea caption1 = null;
        private UITextArea caption2 = null;
        private UITextArea caption3 = null;

        private UITextureRenderer bufferDrawer1 = null;
        private UITextureRenderer bufferDrawer2 = null;
        private UITextureRenderer bufferDrawer3 = null;

        private ModelInstanced buildingObelisks = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneCascadedShadows(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000;
            Camera.Goto(-10, 8, 20f);
            Camera.LookTo(0, 0, 0);

            await LoadResourcesAsync(
                new[]
                {
                    InitializeUI(),
                    InitializeUILevelsControl(),
                    InitializeUIDrawers(),
                    InitializeFloorAsphalt(),
                    InitializeBuildingObelisk(),
                    InitializeTree(),
                    InitializeSkyEffects(),
                    InitializeLights(),
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayout();

                    buildingObelisks[0].Manipulator.SetPosition(+5, 0, +5);
                    buildingObelisks[1].Manipulator.SetPosition(+5, 0, -5);
                    buildingObelisks[2].Manipulator.SetPosition(-5, 0, +5);
                    buildingObelisks[3].Manipulator.SetPosition(-5, 0, -5);
                });
        }

        private async Task InitializeUI()
        {
            title = await this.AddComponentUITextArea("Title", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Arial", 20), TextForeColor = Color.Yellow, TextShadowColor = Color.OrangeRed }, LayerUI);
            title.Text = "Cascaded Shadows";
            help = await this.AddComponentUITextArea("Help", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Arial", 14), TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue }, LayerUI);
            help.Text = $"Press {Color.Red}F1";
            backPanel = await this.AddComponentSprite("Backpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), SceneObjectUsages.UI, LayerUI - 1);

            console = await this.AddComponentUIConsole("Console", UIConsoleDescription.Default(Color.DarkSlateBlue), LayerUI + 1);
            console.Visible = false;
        }
        private async Task InitializeUILevelsControl()
        {
            spLevel1 = await this.AddComponentSprite("High Level", SpriteDescription.Default(new Color(0x17, 0x3F, 0x5F, 0xFF)), SceneObjectUsages.UI, LayerUI);
            spLevel2 = await this.AddComponentSprite("Medium Level", SpriteDescription.Default(new Color(0x20, 0x63, 0x9B, 0xFF)), SceneObjectUsages.UI, LayerUI);
            spLevel3 = await this.AddComponentSprite("Low Level", SpriteDescription.Default(new Color(0x3C, 0xAE, 0xA3, 0xFF)), SceneObjectUsages.UI, LayerUI);

            spSelect1 = await this.AddComponentSprite("First selector", SpriteDescription.Default(new Color(0xF6, 0xD5, 0x5C, 0xFF)), SceneObjectUsages.UI, LayerUI + 1);
            spSelect1.EventsEnabled = true;
            spSelect1.JustPressed += PbJustPressed;
            spSelect1.JustReleased += PbJustReleased;

            spSelect2 = await this.AddComponentSprite("Second selector", SpriteDescription.Default(new Color(0xF6, 0xD5, 0x5C, 0xFF)), SceneObjectUsages.UI, LayerUI + 1);
            spSelect2.EventsEnabled = true;
            spSelect2.JustPressed += PbJustPressed;
            spSelect2.JustReleased += PbJustReleased;
        }
        private async Task InitializeUIDrawers()
        {
            caption1 = await this.AddComponentUITextArea("Caption1", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Arial", 14), TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue }, LayerUI);
            caption2 = await this.AddComponentUITextArea("Caption2", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Arial", 14), TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue }, LayerUI);
            caption3 = await this.AddComponentUITextArea("Caption3", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Arial", 14), TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue }, LayerUI);
            caption1.Text = $"Hight Level Map";
            caption2.Text = $"Medium Level Map";
            caption3.Text = $"Low Level Map";
            caption1.GrowControlWithText = false;
            caption2.GrowControlWithText = false;
            caption3.GrowControlWithText = false;

            bufferDrawer1 = await this.AddComponentUITextureRenderer("DebugTextureRenderer1", UITextureRendererDescription.Default(), LayerEffects);
            bufferDrawer2 = await this.AddComponentUITextureRenderer("DebugTextureRenderer2", UITextureRendererDescription.Default(), LayerEffects);
            bufferDrawer3 = await this.AddComponentUITextureRenderer("DebugTextureRenderer3", UITextureRendererDescription.Default(), LayerEffects);

            var shadowMap = Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);

            bufferDrawer1.Texture = shadowMap;
            bufferDrawer1.TextureIndex = 0;
            bufferDrawer1.Channels = UITextureRendererChannels.Red;

            bufferDrawer2.Texture = shadowMap;
            bufferDrawer2.TextureIndex = 1;
            bufferDrawer2.Channels = UITextureRendererChannels.Red;

            bufferDrawer3.Texture = shadowMap;
            bufferDrawer3.TextureIndex = 2;
            bufferDrawer3.Channels = UITextureRendererChannels.Red;
        }

        private async Task InitializeFloorAsphalt()
        {
            float l = spaceSize;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 0.0f) },
                new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = "SceneLights/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneLights/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneLights/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var desc = new ModelDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            await this.AddComponentModel("Floor", desc);
        }
        private async Task InitializeBuildingObelisk()
        {
            var desc = new ModelInstancedDescription()
            {
                Instances = 4,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile("SceneLights/buildings/obelisk", "Obelisk.xml"),
            };

            buildingObelisks = await this.AddComponentModelInstanced("Obelisk", desc);
        }
        private async Task InitializeTree()
        {
            var desc = new ModelDescription()
            {
                CastShadow = true,
                UseAnisotropicFiltering = true,
                BlendMode = BlendModes.DefaultTransparent,
                Content = ContentDescription.FromFile("SceneLights/trees", "Tree.xml"),
            };

            await this.AddComponentModel("Tree", desc);
        }
        private async Task InitializeSkyEffects()
        {
            await this.AddComponentLensFlare("Flare", new LensFlareDescription()
            {
                ContentPath = @"Common/lensFlare",
                GlowTexture = "lfGlow.png",
                Flares = new[]
                {
                    new LensFlareDescription.Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare(-0.3f, 0.7f, new Color(200,  50,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.0f, 5.6f, new Color( 25,  25,  25), "lfFlare4.png"),
                    new LensFlareDescription.Flare( 0.3f, 0.4f, new Color(100, 255, 200), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 1.2f, 1.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            });
        }
        private async Task InitializeLights()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            Lights.KeyLight.Enabled = true;
            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-1, -1, -3));

            Lights.BackLight.Enabled = true;
            Lights.FillLight.Enabled = true;

            await Task.CompletedTask;
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            // Camera
            UpdateCamera(gameTime);

            // Debug
            UpdateDebug();

            UpdateSelector();

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
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
        }
        private void UpdateDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                bool visible = !bufferDrawer1.Visible;

                bufferDrawer1.Visible = visible;
                caption1.Visible = visible;
                bufferDrawer2.Visible = visible;
                caption2.Visible = visible;
                bufferDrawer3.Visible = visible;
                caption3.Visible = visible;
            }
        }
        private void UpdateSelector()
        {
            help.Text = $"HL: {GameEnvironment.ShadowDistanceHigh}; ML: {GameEnvironment.ShadowDistanceMedium}; LL:{GameEnvironment.ShadowDistanceLow};";

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

            UpdateLayoutUISelectors();

            UpdateLayoutUIDrawers();
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
        private void UpdateLayoutUISelectors()
        {
            float totalWidth = Game.Form.RenderWidth * 0.9f;
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

            float left = (Game.Form.RenderWidth - totalWidth) * 0.5f;

            spLevel1.Left = left;
            spLevel2.Left = left + spLevel1.Width;
            spLevel3.Left = left + spLevel1.Width + spLevel2.Width;

            float top = Game.Form.RenderHeight - (Game.Form.RenderHeight * 0.33f) - 25f;

            spLevel1.Top = top;
            spLevel2.Top = top;
            spLevel3.Top = top;

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

            bufferDrawer1.Height = height;
            bufferDrawer2.Height = height;
            bufferDrawer3.Height = height;

            float width = Game.Form.RenderWidth / 3f;

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

            caption1.Top = Game.Form.RenderHeight - height;
            caption2.Top = Game.Form.RenderHeight - height;
            caption3.Top = Game.Form.RenderHeight - height;

            caption1.TextHorizontalAlign = HorizontalTextAlign.Center;
            caption2.TextHorizontalAlign = HorizontalTextAlign.Center;
            caption3.TextHorizontalAlign = HorizontalTextAlign.Center;
        }

        private void PbJustPressed(object sender, EventArgs e)
        {
            if (sender is UIControl control)
            {
                currentSelector = control;

                if (control == spSelect1)
                {
                    min = spLevel1.Left;
                    max = spLevel2.Left + spLevel2.Width;
                }

                if (control == spSelect2)
                {
                    min = spLevel2.Left;
                    max = spLevel3.Left + spLevel3.Width;
                }
            }
        }
        private void PbJustReleased(object sender, EventArgs e)
        {
            currentSelector = null;
            min = 0;
            max = 0;
        }
    }
}
