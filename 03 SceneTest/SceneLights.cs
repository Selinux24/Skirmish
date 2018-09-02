﻿using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTest
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class SceneLights : Scene
    {
        private const int layerObjects = 0;
        private const int layerTerrain = 1;
        private const int layerEffects = 2;
        private const int layerHUD = 99;
        private const float spaceSize = 20;

        private SceneObject<Model> floorAsphalt = null;

        private SceneObject<ModelInstanced> buildingObelisks = null;

        private SceneObject<ModelInstanced> lightEmitters = null;
        private SceneObject<ModelInstanced> lanterns = null;

        private SceneObject<LineListDrawer> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private SceneObject<SpriteTexture> bufferDrawer = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public SceneLights(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

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

            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeEmitter();
            this.InitializeLanterns();
            this.InitializeLights();
            this.InitializeVolumeDrawer();
            this.InitializeBufferDrawer();
        }

        private void InitializeFloorAsphalt()
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

            this.floorAsphalt = this.AddComponent<Model>(desc);
        }
        private void InitializeBuildingObelisk()
        {
            var desc = new ModelInstancedDescription()
            {
                Name = "Obelisk",
                Instances = 5,
                CastShadow = true,
                Static = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "SceneLights/buildings/obelisk",
                    ModelContentFilename = "Obelisk.xml",
                }
            };

            this.buildingObelisks = this.AddComponent<ModelInstanced>(desc);
        }
        private void InitializeEmitter()
        {
            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.White;

            GeometryUtil.CreateSphere(
                0.1f, 16, 5,
                out Vector3[] v, out Vector3[] n, out Vector2[] uv, out uint[] ix);

            VertexData[] vertices = new VertexData[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vertices[i] = new VertexData()
                {
                    Position = v[i],
                    Normal = n[i],
                    Texture = uv[i],
                };
            }

            var content = ModelContent.GenerateTriangleList(vertices, ix, mat);

            var desc = new ModelInstancedDescription()
            {
                Name = "Emitter",
                Instances = 4,
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

            this.lightEmitters = this.AddComponent<ModelInstanced>(desc);
        }
        private void InitializeLanterns()
        {
            MaterialContent mat = MaterialContent.Default;

            GeometryUtil.CreateCone(
                0.25f, 16, 0.5f,
                out Vector3[] v, out uint[] ix);

            Matrix m = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.ForwardLH * 0.5f);

            Vector3.TransformCoordinate(v, ref m, v);

            VertexData[] vertices = new VertexData[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vertices[i] = new VertexData()
                {
                    Position = v[i],
                    Color = Color.Gray,
                };
            }

            var content = ModelContent.GenerateTriangleList(vertices, ix, mat);

            var desc = new ModelInstancedDescription()
            {
                Name = "Lanterns",
                Instances = 3,
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

            this.lanterns = this.AddComponent<ModelInstanced>(desc);
        }
        private void InitializeLights()
        {
            this.Lights.KeyLight.Enabled = false;
            this.Lights.KeyLight.CastShadow = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
            this.Lights.FillLight.CastShadow = false;

            this.Lights.Add(new SceneLightPoint("Point1", true, Color.White, Color.White, true, Vector3.Zero, 25, 25));

            this.Lights.Add(new SceneLightSpot("Spot1", true, Color.White, Color.White, true, Vector3.Zero, Vector3.Down, 50, 25, 25));
            this.Lights.Add(new SceneLightSpot("Spot2", true, Color.White, Color.White, true, Vector3.Zero, Vector3.Down, 50, 25, 25));
            this.Lights.Add(new SceneLightSpot("Spot3", true, Color.White, Color.White, true, Vector3.Zero, Vector3.Down, 50, 25, 25));
        }
        private void InitializeVolumeDrawer()
        {
            var desc = new LineListDrawerDescription()
            {
                DepthEnabled = true,
                Count = 5000
            };
            this.lightsVolumeDrawer = this.AddComponent<LineListDrawer>(desc);
        }
        private void InitializeBufferDrawer()
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
                Channel = SpriteTextureChannelsEnum.NoAlpha,
            };
            this.bufferDrawer = this.AddComponent<SpriteTexture>(desc, SceneObjectUsageEnum.UI, layerEffects);

            this.bufferDrawer.Visible = false;
        }

        public override void Initialized()
        {
            base.Initialized();

            this.buildingObelisks.Instance[0].Manipulator.SetPosition(0, 0, 0);
            this.buildingObelisks.Instance[1].Manipulator.SetPosition(+5, 0, +5);
            this.buildingObelisks.Instance[2].Manipulator.SetPosition(+5, 0, -5);
            this.buildingObelisks.Instance[3].Manipulator.SetPosition(-5, 0, +5);
            this.buildingObelisks.Instance[4].Manipulator.SetPosition(-5, 0, -5);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            // Camera
            this.UpdateCamera(gameTime, shift, rightBtn);

            // Light
            this.UpdateLight(gameTime);

            // Debug
            this.UpdateDebug(gameTime);

            // Buffers
            this.UpdateBufferDrawer(gameTime);

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
        private void UpdateLight(GameTime gameTime)
        {
            Vector3 position = Vector3.Zero;
            float h = 3.0f;
            float r = 10.0f;
            float hv = 1.0f;
            float av = 0.5f;
            float s = MathUtil.Pi;

            position.X = r * (float)Math.Cos(av * this.Game.GameTime.TotalSeconds);
            position.Y = hv * (float)Math.Sin(av * this.Game.GameTime.TotalSeconds);
            position.Z = r * (float)Math.Sin(av * this.Game.GameTime.TotalSeconds);

            {
                var pos = (position * +1) + new Vector3(0, h, 0);
                this.lightEmitters.Instance[0].Manipulator.SetPosition(pos);
                this.Lights.PointLights[0].Position = pos;
            }

            {
                var pos = (position * -1) + new Vector3(0, h, 0);
                var dir = -Vector3.Normalize(new Vector3(pos.X, 0, pos.Z));
                this.lightEmitters.Instance[1].Manipulator.SetPosition(pos);
                this.lanterns.Instance[0].Manipulator.SetPosition(pos);
                this.lanterns.Instance[0].Manipulator.LookAt(pos + dir);
                this.Lights.SpotLights[0].Position = pos;
                this.Lights.SpotLights[0].Direction = dir;
            }

            position.X = r * (float)Math.Cos(av * (this.Game.GameTime.TotalSeconds + s));
            position.Y = hv * (float)Math.Sin(av * (this.Game.GameTime.TotalSeconds + s));
            position.Z = r * (float)Math.Sin(av * (this.Game.GameTime.TotalSeconds + s));

            {
                var pos = (position * +1) + new Vector3(0, h, 0);
                var dir = -Vector3.Normalize(new Vector3(pos.X, 0, pos.Z));
                this.lightEmitters.Instance[2].Manipulator.SetPosition(pos);
                this.lanterns.Instance[1].Manipulator.SetPosition(pos);
                this.lanterns.Instance[1].Manipulator.LookAt(pos + dir);
                this.Lights.SpotLights[1].Position = pos;
                this.Lights.SpotLights[1].Direction = dir;
            }

            {
                var pos = (position * -1) + new Vector3(0, h, 0);
                var dir = -Vector3.Normalize(new Vector3(pos.X, 0, pos.Z));
                this.lightEmitters.Instance[3].Manipulator.SetPosition(pos);
                this.lanterns.Instance[2].Manipulator.SetPosition(pos);
                this.lanterns.Instance[2].Manipulator.LookAt(pos + dir);
                this.Lights.SpotLights[2].Position = pos;
                this.Lights.SpotLights[2].Direction = dir;
            }
        }
        private void UpdateDebug(GameTime gameTime)
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

                    this.lightsVolumeDrawer.Instance.AddLines(color, lines);
                }

                foreach (var point in this.Lights.PointLights)
                {
                    var color = new Color4(point.DiffuseColor.RGB(), 0.25f);

                    var lines = point.GetVolume(30, 30);

                    this.lightsVolumeDrawer.Instance.AddLines(color, lines);
                }
            }

            if (this.drawCullVolumes)
            {
                var spotColor = new Color4(Color.Red.RGB(), 0.50f);
                var pointColor = new Color4(Color.Green.RGB(), 0.50f);

                foreach (var spot in this.Lights.SpotLights)
                {
                    var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 24, 24);

                    this.lightsVolumeDrawer.Instance.AddLines(spotColor, lines);
                }

                foreach (var point in this.Lights.PointLights)
                {
                    var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 24, 24);

                    this.lightsVolumeDrawer.Instance.AddLines(pointColor, lines);
                }
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = (drawDrawVolumes || drawCullVolumes);
        }
        private void UpdateBufferDrawer(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                SetBuffer(SceneRendererResultEnum.ShadowMapDirectional);
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                SetBuffer(SceneRendererResultEnum.ShadowMapOmnidirectional);
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                SetBuffer(SceneRendererResultEnum.ShadowMapSpot);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.bufferDrawer.Instance.TextureIndex++;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                if (this.bufferDrawer.Instance.TextureIndex > 0)
                {
                    this.bufferDrawer.Instance.TextureIndex--;
                }
            }
        }
        private void SetBuffer(SceneRendererResultEnum resource)
        {
            var buffer = this.Renderer.GetResource(resource);

            this.bufferDrawer.Instance.Texture = buffer;
            this.bufferDrawer.Instance.TextureIndex = 0;
            this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.Red;
            this.bufferDrawer.Visible = true;
        }
    }
}