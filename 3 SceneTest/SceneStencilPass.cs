using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;
using System;

namespace SceneTest
{
    public class SceneStencilPass : Scene
    {
        private float spaceSize = 40;

        private Model floorAsphalt = null;

        private Model buildingObelisk = null;

        private Model lightEmitter1 = null;
        private Model lightEmitter2 = null;

        private LineListDrawer lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        public SceneStencilPass(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-20, 10, 40f);
            this.Camera.LookTo(0, 0, 0);

            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeEmitter();
            this.InitializeLights();

            this.lightsVolumeDrawer = this.AddLineListDrawer(new LineListDrawerDescription() { AlwaysVisible = false, EnableDepthStencil = true }, 5000);

            this.SceneVolume = new BoundingSphere(Vector3.Zero, 50f);
        }

        private void InitializeFloorAsphalt()
        {
            float l = spaceSize;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 0.0f) },
                new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            MaterialContent mat = MaterialContent.Default;
            mat.DiffuseTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            //mat.SpecularTexture = "SceneStencilPass/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Name = "Floor",
                Static = true,
                CastShadow = true,
                AlwaysVisible = false,
                DeferredEnabled = true,
                EnableDepthStencil = true,
                EnableAlphaBlending = false,
            };

            this.floorAsphalt = this.AddModel(content, desc);
        }
        private void InitializeBuildingObelisk()
        {
            this.buildingObelisk = this.AddModel(
                "SceneStencilPass/buildings/obelisk",
                "Obelisk.xml",
                new ModelDescription()
                {
                    Name = "Obelisk",
                    CastShadow = true,
                    Static = true,
                });

            this.buildingObelisk.Manipulator.SetPosition(0, 0, 0);
        }
        private void InitializeEmitter()
        {
            MaterialContent mat = MaterialContent.Default;
            mat.EmissionColor = Color.White;

            Vector3[] v = null;
            Vector3[] n = null;
            Vector2[] uv = null;
            uint[] ix = null;
            GeometryUtil.CreateSphere(0.25f, (uint)16, (uint)5, out v, out n, out uv, out ix);

            VertexData[] vertices = new VertexData[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vertices[i] = VertexData.CreateVertexPositionNormalTexture(v[i], n[i], uv[i]);
            }

            var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalColor, vertices, ix, mat);

            var desc = new ModelDescription()
            {
                Name = "Emitter",
                Static = false,
                CastShadow = false,
                AlwaysVisible = false,
                DeferredEnabled = true,
                EnableDepthStencil = true,
                EnableAlphaBlending = false,
            };

            this.lightEmitter1 = this.AddModel(content, desc);
            this.lightEmitter2 = this.AddModel(content, desc);
        }
        private void InitializeLights()
        {
            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;

            this.Lights.Add(new SceneLightPoint("Point1", false, Color.White, Color.White, true, Vector3.Zero, 5, 5));

            this.Lights.Add(new SceneLightSpot("Spot1", false, Color.White, Color.White, true, Vector3.Zero, Vector3.Down, 20, 5, 5));
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            #region Light

            this.UpdateLight(gameTime);

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.drawDrawVolumes = !this.drawDrawVolumes;
                this.drawCullVolumes = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.drawCullVolumes = !this.drawCullVolumes;
                this.drawDrawVolumes = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = false;
            }

            if (this.drawDrawVolumes)
            {
                this.UpdateLightDrawingVolumes();
            }

            if (this.drawCullVolumes)
            {
                this.UpdateLightCullingVolumes();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            #endregion

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

            position.X = 3.0f * (float)Math.Cos(0.4f * this.Game.GameTime.TotalSeconds);
            position.Y = 5f;
            position.Z = 3.0f * (float)Math.Sin(0.4f * this.Game.GameTime.TotalSeconds);

            this.Lights.PointLights[0].Position = position;
            this.lightEmitter1.Manipulator.SetPosition(position);

            position.X *= -1;
            position.Z *= -1;

            this.Lights.SpotLights[0].Position = position;
            this.Lights.SpotLights[0].Direction = -Vector3.Normalize(new Vector3(position.X, 0, position.Z));
            this.lightEmitter2.Manipulator.SetPosition(position);
        }
        private void UpdateLightDrawingVolumes()
        {
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                this.lightsVolumeDrawer.AddLines(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                this.lightsVolumeDrawer.AddLines(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void UpdateLightCullingVolumes()
        {
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
    }
}
