using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace SceneTest.SceneCascadedShadows
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class SceneCascadedShadows : Scene
    {
        private const int layerEffects = 2;
        private const float spaceSize = 80;

        private ModelInstanced buildingObelisks = null;

        private UITextureRenderer bufferDrawer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneCascadedShadows(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000;
            Camera.Goto(-10, 8, 20f);
            Camera.LookTo(0, 0, 0);

            await LoadResourcesAsync(
                new Task[]
                {
                    InitializeFloorAsphalt(),
                    InitializeBuildingObelisk(),
                    InitializeTree(),
                    InitializeSkyEffects(),
                    InitializeLights(),
                    InitializeDebug()
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    buildingObelisks[0].Manipulator.SetPosition(+5, 0, +5);
                    buildingObelisks[1].Manipulator.SetPosition(+5, 0, -5);
                    buildingObelisks[2].Manipulator.SetPosition(-5, 0, +5);
                    buildingObelisks[3].Manipulator.SetPosition(-5, 0, -5);
                });
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

            MaterialContent mat = MaterialContent.Default;
            mat.DiffuseTexture = "SceneLights/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneLights/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneLights/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Name = "Floor",
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content
                }
            };

            await this.AddComponentModel(desc);
        }
        private async Task InitializeBuildingObelisk()
        {
            var desc = new ModelInstancedDescription()
            {
                Name = "Obelisk",
                Instances = 4,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "SceneLights/buildings/obelisk",
                    ModelContentFilename = "Obelisk.xml",
                }
            };

            buildingObelisks = await this.AddComponentModelInstanced(desc);
        }
        private async Task InitializeTree()
        {
            var desc = new ModelDescription()
            {
                Name = "Tree",
                CastShadow = true,
                UseAnisotropicFiltering = true,
                BlendMode = BlendModes.DefaultTransparent,
                Content = new ContentDescription()
                {
                    ContentFolder = "SceneLights/trees",
                    ModelContentFilename = "Tree.xml",
                }
            };

            await this.AddComponentModel(desc);
        }
        private async Task InitializeSkyEffects()
        {
            await this.AddComponentLensFlare(new LensFlareDescription()
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
        private async Task InitializeDebug()
        {
            int width = (int)(Game.Form.RenderWidth * 0.33f);
            int height = (int)(Game.Form.RenderHeight * 0.33f);
            int smLeft = Game.Form.RenderWidth - width;
            int smTop = Game.Form.RenderHeight - height;

            var desc = new UITextureRendererDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = UITextureRendererChannels.NoAlpha,
            };
            bufferDrawer = await this.AddComponentUITextureRenderer(desc, layerEffects);
            bufferDrawer.Visible = false;
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

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
#endif
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }

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
            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                var shadowMap = Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);
                if (shadowMap != null)
                {
                    bufferDrawer.Texture = shadowMap;
                    bufferDrawer.TextureIndex = 0;
                    bufferDrawer.Channels = UITextureRendererChannels.Red;
                    bufferDrawer.Visible = true;
                }
            }

            if (Game.Input.KeyJustReleased(Keys.Add))
            {
                int tIndex = bufferDrawer.TextureIndex + 1;
                tIndex %= 3;
                bufferDrawer.TextureIndex = tIndex;
            }
            else if (Game.Input.KeyJustReleased(Keys.Subtract))
            {
                int tIndex = bufferDrawer.TextureIndex - 1;
                if (tIndex < 0)
                {
                    tIndex = 2;
                }
                bufferDrawer.TextureIndex = tIndex;
            }
        }
    }
}
