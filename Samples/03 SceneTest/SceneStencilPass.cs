using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest
{
    public class SceneStencilPass : Scene
    {
        private readonly float spaceSize = 10;

        private Model buildingObelisk = null;
        private Model lightEmitter1 = null;
        private Model lightEmitter2 = null;

        private SceneObject<PrimitiveListDrawer<Line3D>> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private bool animateLightColors = false;

        public SceneStencilPass(Game game)
            : base(game)
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
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-10, 8, 20f);
            this.Camera.LookTo(0, 0, 0);

            await this.InitializeFloorAsphalt();
            await this.InitializeBuildingObelisk();
            await this.InitializeEmitter();
            await this.InitializeLights();

            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                DepthEnabled = true,
                Count = 5000
            };
            this.lightsVolumeDrawer = await this.AddComponent<PrimitiveListDrawer<Line3D>>(desc);
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
            mat.DiffuseTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Name = "Floor",
                Static = true,
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

            await this.AddComponent<Model>(desc);
        }
        private async Task InitializeBuildingObelisk()
        {
            var obj = await this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Obelisk",
                    CastShadow = true,
                    Static = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneStencilPass/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });

            this.buildingObelisk = obj.Instance;

            this.buildingObelisk.Manipulator.SetPosition(0, 0, 0);
        }
        private async Task InitializeEmitter()
        {
            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.White;

            var sphere = GeometryUtil.CreateSphere(0.1f, 16, 5);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;
            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Name = "Emitter",
                Static = false,
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                AlphaEnabled = false,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.lightEmitter1 = (await this.AddComponent<Model>(desc)).Instance;
            this.lightEmitter2 = (await this.AddComponent<Model>(desc)).Instance;
        }
        private async Task InitializeLights()
        {
            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;

            this.Lights.Add(new SceneLightPoint("Point1", false, Color.White, Color.White, true, SceneLightPointDescription.Create(Vector3.Zero, 5, 5)));

            this.Lights.Add(new SceneLightSpot("Spot1", false, Color.White, Color.White, true, SceneLightSpotDescription.Create(Vector3.Zero, Vector3.Down, 20, 5, 5)));

            await Task.CompletedTask;
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

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.animateLightColors = !this.animateLightColors;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            // Camera
            this.UpdateCamera(gameTime, shift, rightBtn);

            // Light
            this.UpdateLight();

            // Debug
            this.UpdateDebug();

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                gameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

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
        private void UpdateLight()
        {
            Vector3 position = Vector3.Zero;
            float h = 3.0f;
            float r = 3.0f;
            float hv = 1.0f;
            float av = 1f;

            position.X = r * (float)Math.Cos(av * this.Game.GameTime.TotalSeconds);
            position.Y = hv * (float)Math.Sin(av * this.Game.GameTime.TotalSeconds);
            position.Z = r * (float)Math.Sin(av * this.Game.GameTime.TotalSeconds);

            var pos1 = position + new Vector3(0, h, 0);
            var col1 = animateLightColors ? new Color4(pos1.X, pos1.Y, pos1.Z, 1.0f) : Color.White;

            this.lightEmitter1.Manipulator.SetPosition(pos1);
            this.Lights.PointLights[0].Position = pos1;
            this.Lights.PointLights[0].DiffuseColor = col1;
            this.Lights.PointLights[0].SpecularColor = col1;

            var pos2 = (position * -1) + new Vector3(0, h, 0);
            var col2 = animateLightColors ? new Color4(pos2.X, pos2.Y, pos2.Z, 1.0f) : Color.White;

            this.lightEmitter2.Manipulator.SetPosition(pos2);
            this.Lights.SpotLights[0].Position = pos2;
            this.Lights.SpotLights[0].Direction = -Vector3.Normalize(new Vector3(pos2.X, 0, pos2.Z));
            this.Lights.SpotLights[0].DiffuseColor = col2;
            this.Lights.SpotLights[0].SpecularColor = col2;
        }
        private void UpdateDebug()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.drawDrawVolumes = !this.drawDrawVolumes;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.drawCullVolumes = !this.drawCullVolumes;
            }

            this.UpdateLightVolumes();
        }
        private void UpdateLightVolumes()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            if (this.drawDrawVolumes)
            {
                foreach (var spot in this.Lights.SpotLights)
                {
                    var color = new Color4(spot.DiffuseColor.RGB(), 0.25f);

                    var lines = spot.GetVolume(30);

                    this.lightsVolumeDrawer.Instance.AddPrimitives(color, lines);
                }

                foreach (var point in this.Lights.PointLights)
                {
                    var color = new Color4(point.DiffuseColor.RGB(), 0.25f);

                    var lines = point.GetVolume(30, 30);

                    this.lightsVolumeDrawer.Instance.AddPrimitives(color, lines);
                }
            }

            if (this.drawCullVolumes)
            {
                var color = new Color4(Color.Red.RGB(), 0.50f);

                foreach (var spot in this.Lights.SpotLights)
                {
                    var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 24, 24);

                    this.lightsVolumeDrawer.Instance.AddPrimitives(color, lines);
                }

                foreach (var point in this.Lights.PointLights)
                {
                    var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 24, 24);

                    this.lightsVolumeDrawer.Instance.AddPrimitives(color, lines);
                }
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = (drawDrawVolumes || drawCullVolumes);
        }
    }
}
