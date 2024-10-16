﻿using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BasicSamples.SceneLights
{
    /// <summary>
    /// Lights scene test
    /// </summary>
    public class LightsScene : Scene
    {
        private const float spaceSize = 20;
        private const string resourceFloorDiffuse = "Common/floors/dirt/dirt002.dds";
        private const string resourceFloorNormal = "Common/floors/dirt/normal001.dds";
        private const string resourceObelisk = "Common/buildings/obelisk/";
        private const string resourceTrees = "Common/trees/";

        private bool gameReady = false;

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
        public LightsScene(Game game) : base(game)
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
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeFloor,
                    InitializeBuildingObelisk,
                    InitializeTree,
                    InitializeEmitter,
                    InitializeLanterns,
                    InitializeLights,
                    InitializeVolumeDrawer,
                    InitializeBufferDrawer
                ],
                InitializeComponentsCompleted);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            await AddComponent<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeBuildingObelisk()
        {
            var desc = new ModelInstancedDescription()
            {
                Instances = 4,
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(resourceObelisk, "Obelisk.json"),
            };

            buildingObelisks = await AddComponent<ModelInstanced, ModelInstancedDescription>("Obelisk", "Obelisk", desc);
        }
        private async Task InitializeTree()
        {
            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                BlendMode = BlendModes.OpaqueTransparent,
                Content = ContentDescription.FromFile(resourceTrees, "Tree.json"),
            };

            await AddComponent<Model, ModelDescription>("Tree", "Tree", desc);
        }
        private async Task InitializeEmitter()
        {
            var mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color.White.RGB();

            var geo = GeometryUtil.CreateSphere(Topology.TriangleList, 0.1f, 16, 5);

            var desc = new ModelInstancedDescription()
            {
                Instances = 4,
                CastShadow = ShadowCastingAlgorihtms.None,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            lightEmitters = await AddComponent<ModelInstanced, ModelInstancedDescription>("Emitter", "Emitter", desc);
        }
        private async Task InitializeLanterns()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;

            var geo = GeometryUtil.CreateConeBaseRadius(Topology.TriangleList, 0.25f, 0.5f, 16);

            //Transform base position
            geo.Transform(Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.ForwardLH * 0.5f));

            var vertices = VertexData.FromDescriptor(geo).Select(v =>
            {
                v.Color = Color.Gray;
                return v;
            });

            var indices = geo.Indices;

            var desc = new ModelInstancedDescription()
            {
                Instances = 3,
                CastShadow = ShadowCastingAlgorihtms.None,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            lanterns = await AddComponent<ModelInstanced, ModelInstancedDescription>("Lanterns", "Lanterns", desc);
        }
        private async Task InitializeLights()
        {
            Lights.KeyLight.Enabled = false;
            Lights.KeyLight.CastShadow = false;
            Lights.BackLight.Enabled = false;
            Lights.BackLight.CastShadow = false;
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
                Count = 5000,
                DepthEnabled = true,
                BlendMode = BlendModes.Alpha,
                StartsVisible = false,
            };

            lightsVolumeDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DebugLightsVolumeDrawer", "DebugLightsVolumeDrawer", desc);
        }
        private async Task InitializeBufferDrawer()
        {
            int width = (int)(Game.Form.RenderWidth * 0.33f);
            int height = (int)(Game.Form.RenderHeight * 0.33f);
            int smLeft = Game.Form.RenderWidth - width;
            int smTop = Game.Form.RenderHeight - height;

            var desc = UITextureRendererDescription.Default(smLeft, smTop, width, height);
            desc.StartsVisible = false;

            bufferDrawer = await AddComponentEffect<UITextureRenderer, UITextureRendererDescription>("DebugBufferDrawer", "DebugBufferDrawer", desc);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            buildingObelisks[0].Manipulator.SetPosition(+5, 0, +5);
            buildingObelisks[1].Manipulator.SetPosition(+5, 0, -5);
            buildingObelisks[2].Manipulator.SetPosition(-5, 0, +5);
            buildingObelisks[3].Manipulator.SetPosition(-5, 0, -5);

            gameReady = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!gameReady)
            {
                return;
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
        }
        private void UpdateLight()
        {
            Vector3 position = Vector3.Zero;
            float h = 8.0f;
            float r = 10.0f;
            float hv = 1.0f;
            float av = 0.5f;
            float s = MathUtil.Pi;

            position.X = r * MathF.Cos(av * Game.GameTime.TotalSeconds);
            position.Y = hv * MathF.Sin(av * Game.GameTime.TotalSeconds);
            position.Z = r * MathF.Sin(av * Game.GameTime.TotalSeconds);

            var pPos = (position * +1) + new Vector3(0, h, 0);
            lightEmitters[0].Manipulator.SetPosition(pPos);
            Lights.PointLights[0].Position = pPos;

            var sPos1 = (position * -1) + new Vector3(0, h, 0);
            var sDir1 = -Vector3.Normalize(new Vector3(sPos1.X, sPos1.Y, sPos1.Z));
            lightEmitters[1].Manipulator.SetPosition(sPos1);
            lanterns[0].Manipulator.SetPosition(sPos1);
            lanterns[0].Manipulator.LookAt(sPos1 + sDir1);
            Lights.SpotLights[0].Position = sPos1;
            Lights.SpotLights[0].Direction = sDir1;

            position.X = r * MathF.Cos(av * (Game.GameTime.TotalSeconds + s));
            position.Y = hv * MathF.Sin(av * (Game.GameTime.TotalSeconds + s));
            position.Z = r * MathF.Sin(av * (Game.GameTime.TotalSeconds + s));

            var sPos2 = (position * +1) + new Vector3(0, h, 0);
            var sDir2 = -Vector3.Normalize(new Vector3(sPos2.X, sPos2.Y, sPos2.Z));
            lightEmitters[2].Manipulator.SetPosition(sPos2);
            lanterns[1].Manipulator.SetPosition(sPos2);
            lanterns[1].Manipulator.LookAt(sPos2 + sDir2);
            Lights.SpotLights[1].Position = sPos2;
            Lights.SpotLights[1].Direction = sDir2;

            var sPos3 = (position * -1) + new Vector3(0, h, 0);
            var sDir3 = -Vector3.Normalize(new Vector3(sPos3.X, sPos3.Y, sPos3.Z));
            lightEmitters[3].Manipulator.SetPosition(sPos3);
            lanterns[2].Manipulator.SetPosition(sPos3);
            lanterns[2].Manipulator.LookAt(sPos3 + sDir3);
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
                    var lines = Line3D.CreateSphere(spot.BoundingSphere, 24, 24);

                    lightsVolumeDrawer.AddPrimitives(spotColor, lines);
                }

                foreach (var point in Lights.PointLights)
                {
                    var lines = Line3D.CreateSphere(point.BoundingSphere, 24, 24);

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
            bufferDrawer.Channel = ColorChannels.Red;
            bufferDrawer.Visible = true;
        }
    }
}
