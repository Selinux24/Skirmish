using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest.SceneStencilPass
{
    public class SceneStencilPass : Scene
    {
        private readonly float spaceSize = 10;

        private Model buildingObelisk = null;
        private Model lightEmitter1 = null;
        private Model lightEmitter2 = null;

        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;
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
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(-10, 8, 20f);
            Camera.LookTo(0, 0, 0);

            await LoadResourcesAsync(
                new[]
                {
                    InitializeFloorAsphalt(),
                    InitializeBuildingObelisk(),
                    InitializeEmitter(),
                    InitializeLights(),
                    InitializeLightsDrawer()
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
            mat.DiffuseTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_specular.dds";

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
            buildingObelisk = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Obelisk",
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneStencilPass/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });
            buildingObelisk.Manipulator.SetPosition(0, 0, 0);
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
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            lightEmitter1 = await this.AddComponentModel(desc);
            lightEmitter2 = await this.AddComponentModel(desc);
        }
        private async Task InitializeLights()
        {
            Lights.KeyLight.Enabled = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = true;

            Lights.Add(new SceneLightPoint("Point1", false, Color.White, Color.White, true, SceneLightPointDescription.Create(Vector3.Zero, 5, 5)));

            Lights.Add(new SceneLightSpot("Spot1", false, Color.White, Color.White, true, SceneLightSpotDescription.Create(Vector3.Zero, Vector3.Down, 20, 5, 5)));

            await Task.CompletedTask;
        }
        private async Task InitializeLightsDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                DepthEnabled = true,
                Count = 5000
            };
            lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(desc);
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

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                animateLightColors = !animateLightColors;
            }

            // Camera
            UpdateCamera(gameTime);

            // Light
            UpdateLight();

            // Debug
            UpdateDebug();

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
        private void UpdateLight()
        {
            Vector3 position = Vector3.Zero;
            float h = 3.0f;
            float r = 3.0f;
            float hv = 1.0f;
            float av = 1f;

            position.X = r * (float)Math.Cos(av * Game.GameTime.TotalSeconds);
            position.Y = hv * (float)Math.Sin(av * Game.GameTime.TotalSeconds);
            position.Z = r * (float)Math.Sin(av * Game.GameTime.TotalSeconds);

            var pos1 = position + new Vector3(0, h, 0);
            var col1 = animateLightColors ? new Color4(pos1.X, pos1.Y, pos1.Z, 1.0f) : Color.White;

            lightEmitter1.Manipulator.SetPosition(pos1);
            Lights.PointLights[0].Position = pos1;
            Lights.PointLights[0].DiffuseColor = col1;
            Lights.PointLights[0].SpecularColor = col1;

            var pos2 = (position * -1) + new Vector3(0, h, 0);
            var col2 = animateLightColors ? new Color4(pos2.X, pos2.Y, pos2.Z, 1.0f) : Color.White;

            lightEmitter2.Manipulator.SetPosition(pos2);
            Lights.SpotLights[0].Position = pos2;
            Lights.SpotLights[0].Direction = -Vector3.Normalize(new Vector3(pos2.X, 0, pos2.Z));
            Lights.SpotLights[0].DiffuseColor = col2;
            Lights.SpotLights[0].SpecularColor = col2;
        }
        private void UpdateDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                drawDrawVolumes = !drawDrawVolumes;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                drawCullVolumes = !drawCullVolumes;
            }

            UpdateLightVolumes();
        }
        private void UpdateLightVolumes()
        {
            lightsVolumeDrawer.Clear();

            if (drawDrawVolumes)
            {
                foreach (var spot in Lights.SpotLights)
                {
                    var color = new Color4(spot.DiffuseColor.RGB(), 0.25f);

                    var lines = spot.GetVolume(30);

                    lightsVolumeDrawer.AddPrimitives(color, lines);
                }

                foreach (var point in Lights.PointLights)
                {
                    var color = new Color4(point.DiffuseColor.RGB(), 0.25f);

                    var lines = point.GetVolume(30, 30);

                    lightsVolumeDrawer.AddPrimitives(color, lines);
                }
            }

            if (drawCullVolumes)
            {
                var color = new Color4(Color.Red.RGB(), 0.50f);

                foreach (var spot in Lights.SpotLights)
                {
                    var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 24, 24);

                    lightsVolumeDrawer.AddPrimitives(color, lines);
                }

                foreach (var point in Lights.PointLights)
                {
                    var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 24, 24);

                    lightsVolumeDrawer.AddPrimitives(color, lines);
                }
            }

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = (drawDrawVolumes || drawCullVolumes);
        }
    }
}
