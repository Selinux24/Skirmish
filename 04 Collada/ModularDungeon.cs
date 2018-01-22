using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Collada
{
    public class ModularDungeon : Scene
    {
        private const int layerHUD = 99;
        private const int layerEffects = 98;

        private const float maxDistance = 35;

        private Random rnd = new Random();

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> fps = null;
        private SceneObject<TextDrawer> info = null;
        private SceneObject<Sprite> backPannel = null;

        private Player agent = null;
        //private Player2 agent = null;

        private SceneObject<ModularScenery> scenery = null;
        private BoundingBox sceneryBBOX = new BoundingBox();

        private SceneObject<Model> rat = null;
        private BasicManipulatorController ratController = null;
        private Player ratAgentType = null;
        private Dictionary<string, AnimationPlan> ratPaths = new Dictionary<string, AnimationPlan>();
        private bool ratActive = false;
        private float ratTime = 5;
        private float nextTime = 3;
        private Vector3[] ratHoles = null;

        private SceneObject<LineListDrawer> bboxesDrawer = null;
        private SceneObject<LineListDrawer> ratDrawer = null;
        private SceneObject<TriangleListDrawer> graphDrawer = null;
        private int currentGraph = 0;

        public ModularDungeon(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.InitializeUI();
            this.InitializeModularScenery();
            this.InitializeRat();
            this.InitializeEnvironment();
            this.InitializeDebug();
            this.InitializeCamera();
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
            this.Lights.FillLight.Direction = Vector3.Down;
            this.Lights.FillLight.Brightness *= 0.25f;

            this.Lights.BaseFogColor = Color.Black;
            this.Lights.FogRange = 10f;
            this.Lights.FogStart = maxDistance - 20f;

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = new NavigationMeshGenerationSettings()
                {
                    Agents = new[] { agent, ratAgentType },
                    CellSize = 0.1f,
                    CellHeight = 0.1f,
                    ContourFlags = ContourBuildFlags.TessellateAreaEdges,
                }
            };

            /*
            this.agent = new Player2()
            {
                Name = "Player",
                Height = 1.5f,
                MaxClimb = 0.8f,
                MaxSlope = 45f,
                Radius = 0.5f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };

            var nmsettings = Engine.PathFinding.NavMesh2.BuildSettings.Default;
            nmsettings.Agents = new[] { this.agent };

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = nmsettings
            };
            */
        }
        private void InitializeUI()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Collada Modular Dungeon Scene";
            this.title.Instance.Position = Vector2.Zero;

            this.fps = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.fps.Instance.Text = null;
            this.fps.Instance.Position = new Vector2(0, 24);

            this.info = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.info.Instance.Text = null;
            this.info.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.info.Instance.Top + this.info.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
        }
        private void InitializeModularScenery()
        {
            var desc = new ModularSceneryDescription()
            {
                Name = "Dungeon",
                UseAnisotropic = true,
                CastShadow = true,
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources/ModularDungeon",
                    ModelContentFilename = "assets.xml",
                },
                AssetsConfigurationFile = "assetsmap.xml",
            };

            this.scenery = this.AddComponent<ModularScenery>(desc, SceneObjectUsageEnum.Ground);

            this.SetGround(this.scenery, true);

            this.agent = new Player()
            {
                Name = "Player",
                Height = 1.5f,
                MaxClimb = 0.8f,
                MaxSlope = 45f,
                Radius = 0.5f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };
        }
        private void InitializeRat()
        {
            this.rat = this.AddComponent<Model>(
                new ModelDescription()
                {
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/ModularDungeon/Characters/Rat",
                        ModelContentFilename = "rat.xml",
                    }
                });

            this.ratAgentType = new Player()
            {
                Name = "Rat",
                Height = 0.2f,
                MaxClimb = 0.25f,
                MaxSlope = 50f,
                Radius = 0.1f,
                Velocity = 1f,
                VelocitySlow = 0.5f,
            };

            this.rat.Transform.SetScale(0.5f, true);
            this.rat.Transform.SetPosition(0, 0, 0, true);
            this.rat.Visible = false;

            this.ratController = new BasicManipulatorController();

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");
            this.ratPaths.Add("walk", new AnimationPlan(p0));

            this.rat.Instance.AnimationController.AddPath(this.ratPaths["walk"]);
        }
        private void InitializeDebug()
        {
            var graphDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 10000,
            };
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(graphDrawerDesc);
            this.graphDrawer.Visible = false;

            var bboxesDrawerDesc = new LineListDrawerDescription()
            {
                Name = "DEBUG++ Bounding volumes",
                AlphaEnabled = true,
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.55f),
                Count = 10000,
            };
            this.bboxesDrawer = this.AddComponent<LineListDrawer>(bboxesDrawerDesc);
            this.bboxesDrawer.Visible = false;

            var ratDrawerDesc = new LineListDrawerDescription()
            {
                Name = "DEBUG++ Rat",
                AlphaEnabled = true,
                Color = new Color4(0.0f, 1.0f, 1.0f, 0.55f),
                Count = 10000,
            };
            this.ratDrawer = this.AddComponent<LineListDrawer>(ratDrawerDesc);
            this.ratDrawer.Visible = false;
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = maxDistance;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(-8, 5.5f, -26);
            this.Camera.Interest = new Vector3(-6, 5.5f, -26);


            //this.Camera.Position = new Vector3(53, 1.5f, -12);
            //this.Camera.Interest = new Vector3(53, 1.5f, -30);
        }

        public override void Initialized()
        {
            base.Initialized();

            this.sceneryBBOX = this.scenery.Instance.GetBoundingBox();

            this.ratHoles = this.scenery.Instance.GetAssetPositionsByName("Dn_Rat_Hole_1");

            //Graph
            this.UpdateGraphNodes(this.agent);
            this.currentGraph++;

            //Boxes
            {
                Random rndBoxes = new Random(1);

                var dict = this.scenery.Instance.GetObjectVolumes();

                foreach (var item in dict.Values)
                {
                    this.bboxesDrawer.Instance.SetLines(rndBoxes.NextColor(), Line3D.CreateWiredBox(item.ToArray()));
                }
            }
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                Random clrRnd = new Random(24);
                Color[] regions = new Color[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
                }

                this.graphDrawer.Instance.Clear();

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = (NavigationMeshNode)nodes[i];
                    var color = regions[node.RegionId];
                    var poly = node.Poly;
                    var tris = poly.Triangulate();

                    this.graphDrawer.Instance.AddTriangles(color, tris);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.graphDrawer.Visible = !this.graphDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.ratDrawer.Visible = !this.ratDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.UpdateGraphNodes(this.currentGraph == 0 ? this.agent : this.ratAgentType);
                this.currentGraph++;
                this.currentGraph %= 2;
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.rat.Visible = false;
                this.ratActive = false;
                this.ratController.Clear();
            }

            this.UpdateRat(gameTime);

            this.UpdateCamera(gameTime);

            this.fps.Instance.Text = this.Game.RuntimeText;
            this.info.Instance.Text = string.Format("{0}", this.GetRenderMode());
        }
        private void UpdateCamera(GameTime gameTime)
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            var prevPos = this.Camera.Position;

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

            Vector3 walkerPos;
            if (this.Walk(this.agent, prevPos, this.Camera.Position, out walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }
        }
        private void UpdateRat(GameTime gameTime)
        {
            this.ratTime -= gameTime.ElapsedSeconds;

            if (this.ratActive)
            {
                this.ratController.UpdateManipulator(gameTime, this.rat.Transform);
                if (!this.ratController.HasPath)
                {
                    this.ratActive = false;
                    this.ratTime = this.nextTime;
                    this.rat.Visible = false;
                    this.ratController.Clear();
                }
            }

            if (!this.ratActive && this.ratTime <= 0)
            {
                var iFrom = rnd.Next(0, this.ratHoles.Length);
                var iTo = rnd.Next(0, this.ratHoles.Length);
                if (iFrom == iTo) return;

                var from = this.ratHoles[iFrom] + rnd.NextVector3(new Vector3(-1, 0, -1), new Vector3(1, 0, 1));
                var to = this.ratHoles[iTo] + rnd.NextVector3(new Vector3(-1, 0, -1), new Vector3(1, 0, 1));

                Triangle fromT;
                float fromD;
                if (this.FindNearestGroundPosition(from, out from, out fromT, out fromD))
                {
                    Triangle toT;
                    float toD;
                    if (this.FindNearestGroundPosition(to, out to, out toT, out toD))
                    {
                        var path = this.FindPath(this.ratAgentType, from, to);
                        if (path != null && path.ReturnPath.Count > 0)
                        {
                            path.ReturnPath.Insert(0, this.ratHoles[iFrom]);
                            path.Normals.Insert(0, Vector3.Up);

                            path.ReturnPath.Add(this.ratHoles[iTo]);
                            path.Normals.Add(Vector3.Up);

                            this.ratDrawer.Instance.SetLines(Color.Red, Line3D.CreateLineList(path.ReturnPath.ToArray()));

                            this.ratController.Follow(new NormalPath(path.ReturnPath.ToArray(), path.Normals.ToArray()));
                            this.ratController.MaximumSpeed = this.ratAgentType.Velocity;
                            this.rat.Visible = true;
                            this.rat.Instance.AnimationController.Start(0);

                            this.ratActive = true;
                            this.ratTime = this.nextTime;
                        }
                    }
                }
            }

            if (this.rat.Visible)
            {
                var bbox = this.rat.Instance.GetBoundingBox();

                this.ratDrawer.Instance.SetLines(Color.White, Line3D.CreateWiredBox(bbox));
            }
        }
    }
}
