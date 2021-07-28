using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest.SceneLights
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class SceneLights : Scene
    {
        private const float spaceSize = 20;

        private ModelInstanced buildingObelisks = null;
        private ModelInstanced lightEmitters = null;
        private ModelInstanced lanterns = null;

        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private UITextureRenderer bufferDrawer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneLights(Game game) : base(game)
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
                    InitializeTree(),
                    InitializeEmitter(),
                    InitializeLanterns(),
                    InitializeLights(),
                    InitializeVolumeDrawer(),
                    InitializeBufferDrawer()
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
                Content = ContentDescription.FromFile("SceneLights/buildings/obelisk", "Obelisk.json"),
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
                Content = ContentDescription.FromFile("SceneLights/trees", "Tree.json"),
            };

            await this.AddComponentModel("Tree", desc);
        }
        private async Task InitializeEmitter()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color.White.RGB();

            var s = GeometryUtil.CreateSphere(0.1f, 16, 5);
            var vertices = VertexData.FromDescriptor(s);
            var indices = s.Indices;

            var desc = new ModelInstancedDescription()
            {
                Instances = 4,
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            lightEmitters = await this.AddComponentModelInstanced("Emitter", desc);
        }
        private async Task InitializeLanterns()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;

            var cone = GeometryUtil.CreateCone(0.25f, 16, 0.5f);

            //Transform base position
            Matrix m = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.ForwardLH * 0.5f);
            cone.Transform(m);

            var vertices = VertexData.FromDescriptor(cone);
            vertices = vertices.Select(v =>
            {
                v.Color = Color.Gray;
                return v;
            });

            var indices = cone.Indices;

            var desc = new ModelInstancedDescription()
            {
                Instances = 3,
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            lanterns = await this.AddComponentModelInstanced("Lanterns", desc);
        }
        private async Task InitializeLights()
        {
            Lights.KeyLight.Enabled = false;
            Lights.KeyLight.CastShadow = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = true;
            Lights.FillLight.CastShadow = false;

            var pointDesc = SceneLightPointDescription.Create(Vector3.Zero, 25, 25);
            Lights.Add(new SceneLightPoint("Point1", true, Color3.White, Color3.White, true, pointDesc));

            var spotDesc = SceneLightSpotDescription.Create(Vector3.Zero, Vector3.Down, 50, 25, 25);
            Lights.Add(new SceneLightSpot("Spot1", true, Color3.White, Color3.White, true, spotDesc));
            Lights.Add(new SceneLightSpot("Spot2", true, Color3.White, Color3.White, true, spotDesc));
            Lights.Add(new SceneLightSpot("Spot3", true, Color3.White, Color3.White, true, spotDesc));

            await Task.CompletedTask;
        }
        private async Task InitializeVolumeDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                DepthEnabled = true,
                Count = 5000
            };
            lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer("DebugLightsVolumeDrawer", desc);
        }
        private async Task InitializeBufferDrawer()
        {
            int width = (int)(Game.Form.RenderWidth * 0.33f);
            int height = (int)(Game.Form.RenderHeight * 0.33f);
            int smLeft = Game.Form.RenderWidth - width;
            int smTop = Game.Form.RenderHeight - height;

            bufferDrawer = await this.AddComponentUITextureRenderer("DebugBufferDrawer", UITextureRendererDescription.Default(smLeft, smTop, width, height), LayerEffects);
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

            // Light
            UpdateLight();

            // Debug
            UpdateDebug();

            // Buffers
            UpdateBufferDrawer();

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime)
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
        }
        private void UpdateLight()
        {
            Vector3 position = Vector3.Zero;
            float h = 8.0f;
            float r = 10.0f;
            float hv = 1.0f;
            float av = 0.5f;
            float s = MathUtil.Pi;

            position.X = r * (float)Math.Cos(av * Game.GameTime.TotalSeconds);
            position.Y = hv * (float)Math.Sin(av * Game.GameTime.TotalSeconds);
            position.Z = r * (float)Math.Sin(av * Game.GameTime.TotalSeconds);

            var pPos = (position * +1) + new Vector3(0, h, 0);
            lightEmitters[0].Manipulator.SetPosition(pPos);
            Lights.PointLights[0].Position = pPos;

            var sPos1 = (position * -1) + new Vector3(0, h, 0);
            var sDir1 = -Vector3.Normalize(new Vector3(sPos1.X, sPos1.Y, sPos1.Z));
            lightEmitters[1].Manipulator.SetPosition(sPos1);
            lanterns[0].Manipulator.SetPosition(sPos1);
            lanterns[0].Manipulator.LookAt(sPos1 + sDir1, Axis.None);
            Lights.SpotLights[0].Position = sPos1;
            Lights.SpotLights[0].Direction = sDir1;

            position.X = r * (float)Math.Cos(av * (Game.GameTime.TotalSeconds + s));
            position.Y = hv * (float)Math.Sin(av * (Game.GameTime.TotalSeconds + s));
            position.Z = r * (float)Math.Sin(av * (Game.GameTime.TotalSeconds + s));

            var sPos2 = (position * +1) + new Vector3(0, h, 0);
            var sDir2 = -Vector3.Normalize(new Vector3(sPos2.X, sPos2.Y, sPos2.Z));
            lightEmitters[2].Manipulator.SetPosition(sPos2);
            lanterns[1].Manipulator.SetPosition(sPos2);
            lanterns[1].Manipulator.LookAt(sPos2 + sDir2, Axis.None);
            Lights.SpotLights[1].Position = sPos2;
            Lights.SpotLights[1].Direction = sDir2;

            var sPos3 = (position * -1) + new Vector3(0, h, 0);
            var sDir3 = -Vector3.Normalize(new Vector3(sPos3.X, sPos3.Y, sPos3.Z));
            lightEmitters[3].Manipulator.SetPosition(sPos3);
            lanterns[2].Manipulator.SetPosition(sPos3);
            lanterns[2].Manipulator.LookAt(sPos3 + sDir3, Axis.None);
            Lights.SpotLights[2].Position = sPos3;
            Lights.SpotLights[2].Direction = sDir3;

            if (Game.Input.KeyJustReleased(Keys.D1))
            {
                UpdateSingleLight(Lights.PointLights[0]);
            }
            if (Game.Input.KeyJustReleased(Keys.D2))
            {
                UpdateSingleLight(Lights.SpotLights[0]);
            }
            if (Game.Input.KeyJustReleased(Keys.D3))
            {
                UpdateSingleLight(Lights.SpotLights[1]);
            }
            if (Game.Input.KeyJustReleased(Keys.D4))
            {
                UpdateSingleLight(Lights.SpotLights[2]);
            }
        }
        private void UpdateSingleLight(ISceneLight light)
        {
            if (Game.Input.ShiftPressed)
            {
                light.CastShadow = !light.CastShadow;
            }
            else
            {
                light.Enabled = !light.Enabled;
            }
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
                    var color = new Color4(spot.DiffuseColor, 0.25f);

                    var lines = spot.GetVolume(30);

                    lightsVolumeDrawer.AddPrimitives(color, lines);
                }

                foreach (var point in Lights.PointLights)
                {
                    var color = new Color4(point.DiffuseColor, 0.25f);

                    var lines = point.GetVolume(30, 30);

                    lightsVolumeDrawer.AddPrimitives(color, lines);
                }
            }

            if (drawCullVolumes)
            {
                var spotColor = new Color4(Color.Red.RGB(), 0.50f);
                var pointColor = new Color4(Color.Green.RGB(), 0.50f);

                foreach (var spot in Lights.SpotLights)
                {
                    var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 24, 24);

                    lightsVolumeDrawer.AddPrimitives(spotColor, lines);
                }

                foreach (var point in Lights.PointLights)
                {
                    var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 24, 24);

                    lightsVolumeDrawer.AddPrimitives(pointColor, lines);
                }
            }

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = (drawDrawVolumes || drawCullVolumes);
        }
        private void UpdateBufferDrawer()
        {
            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                SetBuffer(SceneRendererResults.ShadowMapDirectional);
            }

            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                SetBuffer(SceneRendererResults.ShadowMapPoint);
            }

            if (Game.Input.KeyJustReleased(Keys.F7))
            {
                SetBuffer(SceneRendererResults.ShadowMapSpot);
            }

            if (Game.Input.KeyJustReleased(Keys.Add))
            {
                bufferDrawer.TextureIndex++;
            }

            if (Game.Input.KeyJustReleased(Keys.Subtract))
            {
                if (bufferDrawer.TextureIndex > 0)
                {
                    bufferDrawer.TextureIndex--;
                }

                if (Game.Input.ShiftPressed)
                {
                    bufferDrawer.TextureIndex = 0;
                }
            }
        }
        private void SetBuffer(SceneRendererResults resource)
        {
            var buffer = Renderer.GetResource(resource);

            bufferDrawer.Texture = buffer;
            bufferDrawer.TextureIndex = 0;
            bufferDrawer.Channels = UITextureRendererChannels.Red;
            bufferDrawer.Visible = true;
        }
    }
}
