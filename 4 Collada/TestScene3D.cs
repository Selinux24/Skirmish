using Engine;
using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Collada
{
    public class TestScene3D : Scene
    {
        private const float fogStartRel = 0.15f;
        private const float fogRangeRel = 0.45f;

        private readonly Vector3 minScaleSize = new Vector3(0.5f);
        private readonly Vector3 maxScaleSize = new Vector3(2f);

        private Random rnd = new Random();

        private Cursor cursor = null;
        private TextDrawer title = null;
        private TextDrawer fps = null;
        private TextDrawer picks = null;
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

            this.picks = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.picks.Text = null;
            this.picks.Position = new Vector2(0, 48);

            TerrainDescription terrainDescription = new TerrainDescription()
            {
                ContentPath = "Resources",
                Model = new TerrainDescription.ModelDescription()
                {
                    ModelFileName = "Ground.dae",
                },
                Vegetation = new TerrainDescription.VegetationDescription()
                {
                    VegetarionTextures = new[] { "tree0.dds", "tree1.dds", "tree2.dds", "tree3.dds", "tree4.png", "tree5.png" },
                    Saturation = 0.05f,
                    StartRadius = 0f,
                    EndRadius = 300f,
                    MinSize = Vector2.One * 10f,
                    MaxSize = Vector2.One * 15f,
                    Seed = 1024,
                    Opaque = true,
                },
                PathFinder = new TerrainDescription.PathFinderDescription()
                {
                    NodeSize = 20f,
                },
                Opaque = true,
            };

            this.ground = this.AddTerrain(terrainDescription, Matrix.Scaling(20, 10, 20));
            this.helicoptersModel = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "Helicopter.dae",
                Instances = 15,
            });
            this.lampsModel = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "Poly.dae",
                Instances = 2
            });

            var rainEmitter = new ParticleEmitter()
            {
                Color = Color.LightBlue,
                Size = 0.5f,
                Position = Vector3.Zero,
            };
            this.rain = this.AddParticleSystem(ParticleSystemDescription.Rain(rainEmitter, "raindrop.dds"));

            BoundingBox[] bboxes = this.ground.GetBoundingBoxes(5);
            Line3[] listBoxes = GeometryUtil.CreateWiredBox(bboxes);

            this.bboxesDrawer = this.AddLineListDrawer(listBoxes, Color.Red);
            this.bboxesDrawer.Visible = false;
            this.bboxesDrawer.Opaque = false;
            this.bboxesDrawer.EnableAlphaBlending = true;

            List<Line3> squares = new List<Line3>();

            var nodes = this.ground.GetNodes();

            for (int i = 0; i < nodes.Length; i++)
            {
                GridNode node = (GridNode)nodes[i];

                squares.AddRange(GeometryUtil.CreateWiredSquare(node.GetCorners()));
            }

            this.terrainGridDrawer = this.AddLineListDrawer(squares.ToArray(), new Color4(Color.Gainsboro.ToColor3(), 0.5f));
            this.terrainGridDrawer.Visible = false;
            this.terrainGridDrawer.Opaque = false;
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
            GameEnvironment.Background = Color.WhiteSmoke;

            this.Lights.FogStart = this.Camera.FarPlaneDistance * fogStartRel;
            this.Lights.FogRange = this.Camera.FarPlaneDistance * fogRangeRel;
            this.Lights.FogColor = Color.WhiteSmoke;

            this.Lights.Add(new SceneLightPoint()
            {
                Name = "Point light",
                LightColor = Color.White,
                AmbientIntensity = 0.3f,
                DiffuseIntensity = 0.7f,
                Position = Vector3.Zero,
                Radius = 5f,
                Enabled = true,
                CastShadow = false,
            });

            this.Lights.Add(new SceneLightSpot()
            {
                Name = "Helilight",
                LightColor = Color.White,
                AmbientIntensity = 1.0f,
                DiffuseIntensity = 1.0f,
                Direction = Vector3.Down,
                Position = Vector3.Zero,
                Radius = 10f,
                Angle = 15f,
                Enabled = true,
                CastShadow = false,
            });

            this.SceneVolume = this.ground.GetBoundingSphere();
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

                Vector3[] points = this.GetRandomPoints(Vector3.UnitY * 15f, 11);

                points[0] = manipulator.Position;

                //this.helicopters[i].SetPath(this.ground, points, 10f, 15f);
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
                this.Lights.DirectionalLights[0].Enabled = true;
                this.Lights.DirectionalLights[1].Enabled = true;
                this.Lights.DirectionalLights[2].Enabled = true;
                this.Lights.PointLights[0].Enabled = true;
                this.Lights.SpotLights[0].Enabled = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad1))
            {
                this.Lights.DirectionalLights[0].Enabled = !this.Lights.DirectionalLights[0].Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad2))
            {
                this.Lights.DirectionalLights[1].Enabled = !this.Lights.DirectionalLights[1].Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad3))
            {
                this.Lights.DirectionalLights[2].Enabled = !this.Lights.DirectionalLights[2].Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad4))
            {
                this.Lights.PointLights[0].Enabled = !this.Lights.PointLights[0].Enabled;
            }

            if (this.Game.Input.KeyJustReleased(Keys.NumPad5))
            {
                this.Lights.SpotLights[0].Enabled = !this.Lights.SpotLights[0].Enabled;
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

            #region DEBUG

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.terrainGridDrawer.Visible = !this.terrainGridDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
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

            this.Lights.PointLights[0].Position = this.lampsModel.Instances[0].Manipulator.Position;

            #endregion

            if (!this.chaseCamera)
            {
                this.UpdateCamera(gameTime);
            }
            else
            {
                this.UpdateHelicopters(gameTime);
            }

            for (int i = 0; i < this.helicopters.Length; i++)
            {
                this.helicopters[i].Update(gameTime);
            }

            #region Second lamp

            Vector3 lampPosition = (this.helicopters[this.selectedHelicopter].Manipulator.Forward * 3f);
            Quaternion lampRotation = Quaternion.RotationAxis(this.helicopters[this.selectedHelicopter].Manipulator.Right, MathUtil.DegreesToRadians(45f));

            this.lampsModel.Instances[1].Manipulator.SetPosition(lampPosition + this.helicopters[this.selectedHelicopter].Manipulator.Position);
            this.lampsModel.Instances[1].Manipulator.SetRotation(lampRotation * this.helicopters[this.selectedHelicopter].Manipulator.Rotation);

            this.Lights.SpotLights[0].Position = this.lampsModel.Instances[1].Manipulator.Position;
            this.Lights.SpotLights[0].Direction = this.lampsModel.Instances[1].Manipulator.Down;

            #endregion

            this.fps.Text = this.Game.RuntimeText;
            this.picks.Text = string.Format("PicksPerFrame: {0}; PickingAverageTime: {1:0.00000000};", Counters.PicksPerFrame, Counters.PickingAverageTime);
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
            Vector3 offset = this.helicopters[this.selectedHelicopter].Position;
            Vector3 view = this.helicopters[this.selectedHelicopter].View;
            Manipulator3D manipulator = this.helicopters[this.selectedHelicopter].Manipulator;

            this.Camera.Following = new FollowingManipulator(manipulator, offset, view);
        }
        private void StandUp(GameTime gameTime)
        {
            Vector3 position = this.Camera.Position;
            Vector3 interest = this.Camera.Interest;

            this.Camera.Following = null;
            this.Camera.Position = position;
            this.Camera.Interest = interest;
        }

        private Vector3[] GetRandomPoints(Vector3 offset, int count)
        {
            Vector3[] points = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                points[i] = this.GetRandomPoint(offset);
            }

            return points;
        }
        private Vector3 GetRandomPoint(Vector3 offset)
        {
            BoundingBox bbox = this.ground.GetBoundingBox();

            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                Vector3 p;
                if (this.ground.FindTopGroundPosition(v.X, v.Z, out p))
                {
                    return p + offset;
                }
            }
        }
    }
}
