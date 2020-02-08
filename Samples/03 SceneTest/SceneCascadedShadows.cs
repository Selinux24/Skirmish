using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class SceneCascadedShadows : Scene
    {
        private const int layerEffects = 2;
        private const float spaceSize = 80;

        private ModelInstanced buildingObelisks = null;

        private SpriteTexture bufferDrawer = null;

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
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000;
            this.Camera.Goto(-10, 8, 20f);
            this.Camera.LookTo(0, 0, 0);

            await this.LoadResourcesAsync(Guid.NewGuid(),
                this.InitializeFloorAsphalt(),
                this.InitializeBuildingObelisk(),
                this.InitializeTree(),
                this.InitializeSkyEffects(),
                this.InitializeLights(),
                this.InitializeDebug()
            );
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
                AlphaEnabled = false,
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

            this.buildingObelisks = await this.AddComponentModelInstanced(desc);
        }
        private async Task InitializeTree()
        {
            var desc = new ModelDescription()
            {
                Name = "Tree",
                CastShadow = true,
                UseAnisotropicFiltering = true,
                AlphaEnabled = true,
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

            this.Lights.KeyLight.Enabled = true;
            this.Lights.KeyLight.CastShadow = true;
            this.Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-1, -1, -3));

            this.Lights.BackLight.Enabled = true;
            this.Lights.FillLight.Enabled = true;

            await Task.CompletedTask;
        }
        private async Task InitializeDebug()
        {
            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            var desc = new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = SpriteTextureChannels.NoAlpha,
            };
            this.bufferDrawer = await this.AddComponentSpriteTexture(desc, SceneObjectUsages.UI, layerEffects);
            this.bufferDrawer.Visible = false;
        }

        public override void GameResourcesLoaded(Guid id)
        {
            this.buildingObelisks[0].Manipulator.SetPosition(+5, 0, +5);
            this.buildingObelisks[1].Manipulator.SetPosition(+5, 0, -5);
            this.buildingObelisks[2].Manipulator.SetPosition(-5, 0, +5);
            this.buildingObelisks[3].Manipulator.SetPosition(-5, 0, -5);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            // Camera
            this.UpdateCamera(gameTime, shift, rightBtn);

            // Debug
            this.UpdateDebug();

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }

        private void UpdateDebug()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                var shadowMap = this.Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);
                if (shadowMap != null)
                {
                    this.bufferDrawer.Texture = shadowMap;
                    this.bufferDrawer.TextureIndex = 0;
                    this.bufferDrawer.Channels = SpriteTextureChannels.Red;
                    this.bufferDrawer.Visible = true;
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                int tIndex = this.bufferDrawer.TextureIndex + 1;
                tIndex %= 3;
                this.bufferDrawer.TextureIndex = tIndex;
            }
            else if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                int tIndex = this.bufferDrawer.TextureIndex - 1;
                if (tIndex < 0)
                {
                    tIndex = 2;
                }
                this.bufferDrawer.TextureIndex = tIndex;
            }
        }
    }
}
