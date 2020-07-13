using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class SceneLights : Scene
    {
        private const int layerEffects = 2;
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

            await this.LoadResourcesAsync(
                new[]
                {
                    this.InitializeFloorAsphalt(),
                    this.InitializeBuildingObelisk(),
                    this.InitializeTree(),
                    this.InitializeEmitter(),
                    this.InitializeLanterns(),
                    this.InitializeLights(),
                    this.InitializeVolumeDrawer(),
                    this.InitializeBufferDrawer()
                },
                () =>
                {
                    this.buildingObelisks[0].Manipulator.SetPosition(+5, 0, +5);
                    this.buildingObelisks[1].Manipulator.SetPosition(+5, 0, -5);
                    this.buildingObelisks[2].Manipulator.SetPosition(-5, 0, +5);
                    this.buildingObelisks[3].Manipulator.SetPosition(-5, 0, -5);
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

            this.buildingObelisks = await this.AddComponentModelInstanced(desc);
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
        private async Task InitializeEmitter()
        {
            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.White;

            var s = GeometryUtil.CreateSphere(0.1f, 16, 5);
            var vertices = VertexData.FromDescriptor(s);
            var indices = s.Indices;
            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelInstancedDescription()
            {
                Name = "Emitter",
                Instances = 4,
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.lightEmitters = await this.AddComponentModelInstanced(desc);
        }
        private async Task InitializeLanterns()
        {
            MaterialContent mat = MaterialContent.Default;

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

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelInstancedDescription()
            {
                Name = "Lanterns",
                Instances = 3,
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.lanterns = await this.AddComponentModelInstanced(desc);
        }
        private async Task InitializeLights()
        {
            this.Lights.KeyLight.Enabled = false;
            this.Lights.KeyLight.CastShadow = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
            this.Lights.FillLight.CastShadow = false;

            var pointDesc = SceneLightPointDescription.Create(Vector3.Zero, 25, 25);
            this.Lights.Add(new SceneLightPoint("Point1", true, Color.White, Color.White, true, pointDesc));

            var spotDesc = SceneLightSpotDescription.Create(Vector3.Zero, Vector3.Down, 50, 25, 25);
            this.Lights.Add(new SceneLightSpot("Spot1", true, Color.White, Color.White, true, spotDesc));
            this.Lights.Add(new SceneLightSpot("Spot2", true, Color.White, Color.White, true, spotDesc));
            this.Lights.Add(new SceneLightSpot("Spot3", true, Color.White, Color.White, true, spotDesc));

            await Task.CompletedTask;
        }
        private async Task InitializeVolumeDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                DepthEnabled = true,
                Count = 5000
            };
            this.lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(desc);
        }
        private async Task InitializeBufferDrawer()
        {
            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            var desc = new UITextureRendererDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = UITextureRendererChannels.NoAlpha,
            };
            this.bufferDrawer = await this.AddComponentUITextureRenderer(desc, layerEffects);

            this.bufferDrawer.Visible = false;
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

            // Light
            this.UpdateLight(shift);

            // Debug
            this.UpdateDebug();

            // Buffers
            this.UpdateBufferDrawer(shift);

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
        private void UpdateLight(bool shift)
        {
            Vector3 position = Vector3.Zero;
            float h = 8.0f;
            float r = 10.0f;
            float hv = 1.0f;
            float av = 0.5f;
            float s = MathUtil.Pi;

            position.X = r * (float)Math.Cos(av * this.Game.GameTime.TotalSeconds);
            position.Y = hv * (float)Math.Sin(av * this.Game.GameTime.TotalSeconds);
            position.Z = r * (float)Math.Sin(av * this.Game.GameTime.TotalSeconds);

            var pPos = (position * +1) + new Vector3(0, h, 0);
            this.lightEmitters[0].Manipulator.SetPosition(pPos);
            this.Lights.PointLights[0].Position = pPos;

            var sPos1 = (position * -1) + new Vector3(0, h, 0);
            var sDir1 = -Vector3.Normalize(new Vector3(sPos1.X, sPos1.Y, sPos1.Z));
            this.lightEmitters[1].Manipulator.SetPosition(sPos1);
            this.lanterns[0].Manipulator.SetPosition(sPos1);
            this.lanterns[0].Manipulator.LookAt(sPos1 + sDir1, false);
            this.Lights.SpotLights[0].Position = sPos1;
            this.Lights.SpotLights[0].Direction = sDir1;

            position.X = r * (float)Math.Cos(av * (this.Game.GameTime.TotalSeconds + s));
            position.Y = hv * (float)Math.Sin(av * (this.Game.GameTime.TotalSeconds + s));
            position.Z = r * (float)Math.Sin(av * (this.Game.GameTime.TotalSeconds + s));

            var sPos2 = (position * +1) + new Vector3(0, h, 0);
            var sDir2 = -Vector3.Normalize(new Vector3(sPos2.X, sPos2.Y, sPos2.Z));
            this.lightEmitters[2].Manipulator.SetPosition(sPos2);
            this.lanterns[1].Manipulator.SetPosition(sPos2);
            this.lanterns[1].Manipulator.LookAt(sPos2 + sDir2, false);
            this.Lights.SpotLights[1].Position = sPos2;
            this.Lights.SpotLights[1].Direction = sDir2;

            var sPos3 = (position * -1) + new Vector3(0, h, 0);
            var sDir3 = -Vector3.Normalize(new Vector3(sPos3.X, sPos3.Y, sPos3.Z));
            this.lightEmitters[3].Manipulator.SetPosition(sPos3);
            this.lanterns[2].Manipulator.SetPosition(sPos3);
            this.lanterns[2].Manipulator.LookAt(sPos3 + sDir3, false);
            this.Lights.SpotLights[2].Position = sPos3;
            this.Lights.SpotLights[2].Direction = sDir3;

            if (this.Game.Input.KeyJustReleased(Keys.D1))
            {
                UpdateSingleLight(this.Lights.PointLights[0], shift);
            }
            if (this.Game.Input.KeyJustReleased(Keys.D2))
            {
                UpdateSingleLight(this.Lights.SpotLights[0], shift);
            }
            if (this.Game.Input.KeyJustReleased(Keys.D3))
            {
                UpdateSingleLight(this.Lights.SpotLights[1], shift);
            }
            if (this.Game.Input.KeyJustReleased(Keys.D4))
            {
                UpdateSingleLight(this.Lights.SpotLights[2], shift);
            }
        }
        private void UpdateSingleLight(ISceneLight light, bool shift)
        {
            if (shift)
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
            this.lightsVolumeDrawer.Clear();

            if (this.drawDrawVolumes)
            {
                foreach (var spot in this.Lights.SpotLights)
                {
                    var color = new Color4(spot.DiffuseColor.RGB(), 0.25f);

                    var lines = spot.GetVolume(30);

                    this.lightsVolumeDrawer.AddPrimitives(color, lines);
                }

                foreach (var point in this.Lights.PointLights)
                {
                    var color = new Color4(point.DiffuseColor.RGB(), 0.25f);

                    var lines = point.GetVolume(30, 30);

                    this.lightsVolumeDrawer.AddPrimitives(color, lines);
                }
            }

            if (this.drawCullVolumes)
            {
                var spotColor = new Color4(Color.Red.RGB(), 0.50f);
                var pointColor = new Color4(Color.Green.RGB(), 0.50f);

                foreach (var spot in this.Lights.SpotLights)
                {
                    var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 24, 24);

                    this.lightsVolumeDrawer.AddPrimitives(spotColor, lines);
                }

                foreach (var point in this.Lights.PointLights)
                {
                    var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 24, 24);

                    this.lightsVolumeDrawer.AddPrimitives(pointColor, lines);
                }
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = (drawDrawVolumes || drawCullVolumes);
        }
        private void UpdateBufferDrawer(bool shift)
        {
            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                SetBuffer(SceneRendererResults.ShadowMapDirectional);
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                SetBuffer(SceneRendererResults.ShadowMapPoint);
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                SetBuffer(SceneRendererResults.ShadowMapSpot);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.bufferDrawer.TextureIndex++;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                if (this.bufferDrawer.TextureIndex > 0)
                {
                    this.bufferDrawer.TextureIndex--;
                }

                if (shift)
                {
                    this.bufferDrawer.TextureIndex = 0;
                }
            }
        }
        private void SetBuffer(SceneRendererResults resource)
        {
            var buffer = this.Renderer.GetResource(resource);

            this.bufferDrawer.Texture = buffer;
            this.bufferDrawer.TextureIndex = 0;
            this.bufferDrawer.Channels = UITextureRendererChannels.Red;
            this.bufferDrawer.Visible = true;
        }
    }
}
