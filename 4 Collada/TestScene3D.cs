﻿using System;
using System.Collections.Generic;
using Engine;
using Engine.Common;
using SharpDX;

namespace Collada
{
    public class TestScene3D : Scene
    {
        private const float fogStartRel = 0.25f;
        private const float fogRangeRel = 0.75f;

        private readonly Vector3 minScaleSize = new Vector3(0.5f);
        private readonly Vector3 maxScaleSize = new Vector3(2f);

        private Cursor cursor = null;
        private TextDrawer title = null;
        private TextDrawer fps = null;
        private Terrain ground = null;
        private ModelInstanced lampsModel = null;
        private ModelInstanced helicoptersModel = null;
        private ParticleSystem rain = null;

        private int selectedHelicopter = 0;
        private Helicopter[] helicopters = null;
        private bool chaseCamera = false;

        private LineListDrawer bboxesDrawer = null;
        private LineListDrawer terrainGridDrawer = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            SpriteDescription cursorDesc = new SpriteDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };
            this.cursor = this.AddCursor(cursorDesc);

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.title.Text = "Collada Scene with billboards and animation";
            this.title.Position = Vector2.Zero;

            this.fps = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            TerrainDescription terrainDescription = new TerrainDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "Ground.dae",
                AddVegetation = true,
                VegetarionTextures = new[] { "tree0.dds", "tree1.dds", "tree2.dds", "tree3.dds", "tree4.png", "tree5.png" },
                Saturation = 0.05f,
                MinSize = Vector2.One * 2f,
                MaxSize = Vector2.One * 4f,
                Seed = 1024,
                UsePathFinding = true,
                PathNodeSize = 20f,
            };

            this.ground = this.AddTerrain(Matrix.Scaling(20, 20, 20), terrainDescription);
            this.helicoptersModel = this.AddInstancingModel("Resources", "Helicopter.dae", 15);
            this.lampsModel = this.AddInstancingModel("Resources", "Poly.dae", 2);
            this.rain = this.AddParticleSystem(ParticleSystemDescription.Rain(0.5f, "raindrop.dds"));

            BoundingBox[] bboxes = this.ground.pickingQuadtree.GetBoundingBoxes(5);
            Line[] listBoxes = GeometryUtil.CreateWiredBox(bboxes);

            this.bboxesDrawer = this.AddLineListDrawer(listBoxes, Color.Red);

            List<Line> squares = new List<Line>();

            for (int i = 0; i < this.ground.grid.Nodes.Length; i++)
            {
                squares.AddRange(GeometryUtil.CreateWiredSquare(this.ground.grid.Nodes[i].GetCorners()));
            }

            this.terrainGridDrawer = this.AddLineListDrawer(squares.ToArray(), new Color4(Color.Gainsboro.ToColor3(), 0.5f));
            this.terrainGridDrawer.UseZBuffer = false;
            this.terrainGridDrawer.EnableAlphaBlending = true;

            this.InitializeCamera();
            this.InitializeEnvironment();
            this.InitializeHelicopters();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Mode = CameraModes.Free;
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
            this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
            this.Lights.FogColor = Color.CornflowerBlue;

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attenuation = new Vector3(0.1f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 80.0f;

            this.Lights.SpotLightEnabled = true;
            this.Lights.SpotLight.Direction = Vector3.Down;
            this.Lights.SpotLight.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Diffuse = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            this.Lights.SpotLight.Specular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            this.Lights.SpotLight.Attenuation = new Vector3(0.15f, 0.0f, 0.0f);
            this.Lights.SpotLight.Spot = 16f;
            this.Lights.SpotLight.Range = 100.0f;
        }
        private void InitializeHelicopters()
        {
            this.helicopters = new Helicopter[this.helicoptersModel.Count];

            int rows = 3;
            float left = 15f;
            float back = 25f;
            int x = 0;
            int z = 0;

            this.lampsModel.Instances[0].Manipulator.SetScale(0.1f);
            this.lampsModel.Instances[1].Manipulator.SetScale(0.1f);

            Random rnd = new Random();

            for (int i = 0; i < this.helicoptersModel.Count; i++)
            {
                this.helicoptersModel.Instances[i].TextureIndex = rnd.Next(0, 2);

                Manipulator3D manipulator = this.helicoptersModel.Instances[i].Manipulator;

                this.helicopters[i] = new Helicopter(manipulator);

                manipulator.LinearVelocity = 10f;
                manipulator.AngularVelocity = 45f;

                if (x >= rows) x = 0;
                z = i / rows;

                float posX = (x++ * left);
                float posZ = (z * -back);

                Vector3 p;
                if (this.ground.FindTopGroundPosition(posX, posZ, out p))
                {
                    manipulator.SetScale(1);
                    manipulator.SetRotation(Quaternion.Identity);
                    manipulator.SetPosition(p + (Vector3.UnitY * 15f));
                }
            }

            this.Camera.Goto(this.helicopters[this.selectedHelicopter].Manipulator.Position + (Vector3.One * 10f));
            this.Camera.LookTo(this.helicopters[this.selectedHelicopter].Manipulator.Position + (Vector3.UnitY * 2.5f));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            #region Lights and Fog activation

            if (this.Game.Input.KeyJustReleased(Keys.NumPad0))
            {
                this.Lights.DirectionalLight1Enabled = true;
                this.Lights.DirectionalLight2Enabled = true;
                this.Lights.DirectionalLight3Enabled = true;
                this.Lights.PointLightEnabled = true;
                this.Lights.SpotLightEnabled = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad1))
            {
                this.Lights.DirectionalLight1Enabled = !this.Lights.DirectionalLight1Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad2))
            {
                this.Lights.DirectionalLight2Enabled = !this.Lights.DirectionalLight2Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad3))
            {
                this.Lights.DirectionalLight3Enabled = !this.Lights.DirectionalLight3Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad4))
            {
                this.Lights.PointLightEnabled = !this.Lights.PointLightEnabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad5))
            {
                this.Lights.SpotLightEnabled = !this.Lights.SpotLightEnabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad6))
            {
                if (this.Lights.FogRange == 0)
                {
                    this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
                    this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
                }
                else
                {
                    this.Lights.FogStart = 0;
                    this.Lights.FogRange = 0;
                }
            }

            #endregion

            #region Helicopters

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.InitializeHelicopters();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Tab))
            {
                this.NextHelicopter(gameTime);
            }

            if (this.Game.Input.KeyJustReleased(Keys.C))
            {
                this.chaseCamera = !this.chaseCamera;

                if (this.chaseCamera)
                {
                    this.SitDown(gameTime);
                }
                else
                {
                    this.StandUp(gameTime);
                }
            }

            #endregion

            #region First lamp

            float r = 500.0f;

            float lampPosX = r * (float)Math.Cos(1f / r * this.Game.GameTime.TotalSeconds);
            float lampPosZ = r * (float)Math.Sin(1f / r * this.Game.GameTime.TotalSeconds);

            Vector3 lampPos;
            if (this.ground.FindTopGroundPosition(lampPosX, lampPosZ, out lampPos))
            {
                this.lampsModel.Instances[0].Manipulator.SetPosition(lampPos + (Vector3.UnitY * 30f));
            }

            this.Lights.PointLight.Position = this.lampsModel.Instances[0].Manipulator.Position;

            #endregion

            if (!this.chaseCamera)
            {
                this.UpdateCamera(gameTime);
            }
            else
            {
                this.UpdateHelicopters(gameTime);
            }

            this.fps.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera(GameTime gameTime)
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, slow);
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
        }
        private void UpdateHelicopters(GameTime gameTime)
        {
            Helicopter helicopter = this.helicopters[this.selectedHelicopter];
            Manipulator3D manipulator = helicopter.Manipulator;

            if (this.Game.Input.KeyPressed(Keys.F5))
            {
                helicopter.SetPilot();

                this.SitDown(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.F6))
            {
                helicopter.SetCopilot();

                this.SitDown(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.F7))
            {
                helicopter.SetLeftMachineGun();

                this.SitDown(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.F8))
            {
                helicopter.SetRightMachineGun();

                this.SitDown(gameTime);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                manipulator.MoveLeft(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.D))
            {
                manipulator.MoveRight(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.W))
            {
                manipulator.MoveForward(gameTime);
            }
            if (this.Game.Input.KeyPressed(Keys.S))
            {
                manipulator.MoveBackward(gameTime);
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                if (this.Game.Input.MouseXDelta < 0)
                {
                    manipulator.YawLeft(gameTime);
                }
                if (this.Game.Input.MouseXDelta > 0)
                {
                    manipulator.YawRight(gameTime);
                }
                if (this.Game.Input.MouseYDelta < 0)
                {
                    manipulator.MoveUp(gameTime);
                }
                if (this.Game.Input.MouseYDelta > 0)
                {
                    manipulator.MoveDown(gameTime);
                }
            }

            #region Second lamp

            Vector3 lampPosition = (manipulator.Forward * 3f);
            Quaternion lampRotation = Quaternion.RotationAxis(manipulator.Right, MathUtil.DegreesToRadians(45f));

            this.lampsModel.Instances[1].Manipulator.SetPosition(lampPosition + manipulator.Position);
            this.lampsModel.Instances[1].Manipulator.SetRotation(lampRotation * manipulator.Rotation);

            this.Lights.SpotLight.Position = this.lampsModel.Instances[1].Manipulator.Position;
            this.Lights.SpotLight.Direction = this.lampsModel.Instances[1].Manipulator.Down;

            #endregion
        }
        private void NextHelicopter(GameTime gameTime)
        {
            this.selectedHelicopter++;
            if (this.selectedHelicopter >= this.helicoptersModel.Count)
            {
                this.selectedHelicopter = 0;
            }

            if (this.chaseCamera)
            {
                this.SitDown(gameTime);
            }
        }
        private void PrevHelicopter(GameTime gameTime)
        {
            this.selectedHelicopter--;
            if (this.selectedHelicopter < 0)
            {
                this.selectedHelicopter = this.helicoptersModel.Count - 1;
            }

            if (this.chaseCamera)
            {
                this.SitDown(gameTime);
            }
        }
        private void SitDown(GameTime gameTime)
        {
            Vector3 offset = this.helicopters[this.selectedHelicopter].Offset;
            Manipulator3D manipulator = this.helicopters[this.selectedHelicopter].Manipulator;

            this.Camera.Following = new FollowingManipulator(manipulator, offset);
        }
        private void StandUp(GameTime gameTime)
        {
            Vector3 position = this.Camera.Position;
            Vector3 interest = this.Camera.Interest;

            this.Camera.Following = null;
            this.Camera.Position = position;
            this.Camera.Interest = interest;
        }
    }
}
