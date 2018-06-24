using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        private Color ambientDown = new Color(127, 127, 127, 255);
        private Color ambientUp = new Color(137, 116, 104, 255);

        private Player agent = null;
        private Color agentTorchLight = new Color(255, 249, 224, 255);

        private SceneLightPoint torch = null;

        private SceneObject<ModularScenery> scenery = null;

        private float doorDistance = 3f;
        private SceneObject<TextDrawer> messages = null;

        private SceneObject<Model> rat = null;
        private BasicManipulatorController ratController = null;
        private Player ratAgentType = null;
        private Dictionary<string, AnimationPlan> ratPaths = new Dictionary<string, AnimationPlan>();
        private bool ratActive = false;
        private float ratTime = 5;
        private float nextTime = 3;
        private Vector3[] ratHoles = null;

        private SceneObject<ModelInstanced> human = null;

        private SceneObject<LineListDrawer> bboxesDrawer = null;
        private SceneObject<LineListDrawer> ratDrawer = null;
        private SceneObject<TriangleListDrawer> graphDrawer = null;
        private SceneObject<TriangleListDrawer> obstacleDrawer = null;
        private int currentGraph = 0;

        private string nmFile = "nm.graph";
        private string ntFile = "nm.obj";
        private bool taskRunning = false;

        Dictionary<int, object> obstacles = new Dictionary<int, object>();
        private Color obstacleColor = new Color(Color.Pink.ToColor3(), 0.5f);

        public ModularDungeon(Game game)
            : base(game, SceneModesEnum.DeferredLightning)
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

            this.InitializeUI();
            this.InitializeModularScenery();
            this.InitializePlayer();
            this.InitializeRat();
            this.InitializeHuman();
            this.InitializeEnvironment();
            this.InitializeLights();
            this.InitializeCamera();
            this.InitializeDebug();
        }
        private void InitializeEnvironment()
        {
            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = 0.2f;
            nmsettings.CellHeight = 0.15f;

            //Agents
            nmsettings.Agents = new[] { agent, ratAgentType };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypeEnum.Watershed;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModesEnum.TempObstacles;
            nmsettings.TileSize = 48;

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = nmsettings,
            };
        }
        private void InitializeLights()
        {
            this.Lights.HemisphericLigth = new SceneLightHemispheric("hemi_light", this.ambientDown, this.ambientUp, true);
            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = false;

            this.Lights.BaseFogColor = GameEnvironment.Background = Color.Black;
            this.Lights.FogRange = 10f;
            this.Lights.FogStart = maxDistance - 15f;

            this.torch = new SceneLightPoint("player_torch", true, this.agentTorchLight, this.agentTorchLight, true, Vector3.Zero, 10f, 25f);
            this.Lights.Add(torch);
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

            this.messages = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 48, Color.Red, Color.DarkRed), SceneObjectUsageEnum.UI, layerHUD);
            this.messages.Instance.Text = null;
            this.messages.Instance.Position = new Vector2(0, 0);
            this.messages.Visible = false;
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
                LevelsFile = "levels.xml",
            };

            this.scenery = this.AddComponent<ModularScenery>(desc, SceneObjectUsageEnum.Ground);

            this.SetGround(this.scenery, true);
        }
        private void InitializePlayer()
        {
            this.agent = new Player()
            {
                Name = "Player",
                Height = 1.5f,
                Radius = 0.2f,
                MaxClimb = 1.0f,
                MaxSlope = 45f,
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
                Radius = 0.1f,
                MaxClimb = 0.5f,
                MaxSlope = 50f,
                Velocity = 3f,
                VelocitySlow = 1f,
            };

            this.rat.Transform.SetScale(0.5f, true);
            this.rat.Transform.SetPosition(0, 0, 0, true);
            this.rat.Visible = false;

            this.ratController = new BasicManipulatorController();

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");
            this.ratPaths.Add("walk", new AnimationPlan(p0));

            this.rat.Instance.AnimationController.AddPath(this.ratPaths["walk"]);
            this.rat.Instance.AnimationController.TimeDelta = 1.5f;
        }
        private void InitializeHuman()
        {
            this.human = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/ModularDungeon/Characters/Human2",
                        ModelContentFilename = "Human2.xml",
                    }
                });

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("stand");

            for (int i = 0; i < this.human.Count; i++)
            {
                this.human.Instance[i].Manipulator.SetPosition(31, 0, i == 0 ? -31 : -29, true);
                this.human.Instance[i].Manipulator.SetRotation(-MathUtil.PiOverTwo, 0, 0, true);

                this.human.Instance[i].AnimationController.AddPath(new AnimationPlan(p0));
                this.human.Instance[i].AnimationController.Start(i * 1f);
                this.human.Instance[i].AnimationController.TimeDelta = 0.5f + (i * 0.1f);
            }
        }
        private void InitializeDebug()
        {
            var graphDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 50000,
            };
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(graphDrawerDesc);
            this.graphDrawer.Visible = false;

            var bboxesDrawerDesc = new LineListDrawerDescription()
            {
                Name = "DEBUG++ Bounding volumes",
                AlphaEnabled = true,
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.25f),
                Count = 10000,
            };
            this.bboxesDrawer = this.AddComponent<LineListDrawer>(bboxesDrawerDesc);
            this.bboxesDrawer.Visible = false;

            var ratDrawerDesc = new LineListDrawerDescription()
            {
                Name = "DEBUG++ Rat",
                AlphaEnabled = true,
                Color = new Color4(0.0f, 1.0f, 1.0f, 0.25f),
                Count = 10000,
            };
            this.ratDrawer = this.AddComponent<LineListDrawer>(ratDrawerDesc);
            this.ratDrawer.Visible = false;

            var obstacleDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Obstacles",
                AlphaEnabled = true,
                DepthEnabled = true,
                Count = 1000,
            };
            this.obstacleDrawer = this.AddComponent<TriangleListDrawer>(obstacleDrawerDesc);
            this.obstacleDrawer.Visible = true;
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = maxDistance;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(-6, 5.5f, -26);
            this.Camera.Interest = new Vector3(-4, 5.5f, -26);
        }

        public override void Initialized()
        {
            base.Initialized();

            //Rat holes
            this.ratHoles = this.scenery.Instance.GetObjectsPositionsByAssetName("Dn_Rat_Hole_1");

            //Human obstacles
            for (int i = 0; i < this.human.Count; i++)
            {
                var pos = this.human.Instance[i].Manipulator.Position;
                this.AddObstacle(new BoundingCylinder(pos, 0.8f, 1.5f));
            }

            this.UpdateDebug();
        }
        private void UpdateDebug()
        {
            //Graph
            this.UpdateGraphNodes(this.agent);
            this.currentGraph++;

            this.bboxesDrawer.Instance.Clear();

            //Boxes
            {
                Random rndBoxes = new Random(1);

                var dict = this.scenery.Instance.GetMapVolumes();

                foreach (var item in dict.Values)
                {
                    var color = rndBoxes.NextColor().ToColor4();
                    color.Alpha = 0.40f;

                    this.bboxesDrawer.Instance.SetLines(color, Line3D.CreateWiredBox(item.ToArray()));
                }
            }

            //Doors
            UpdateBoundingBoxes(this.scenery.Instance.GetObjectsByType(ModularSceneryObjectTypeEnum.Door), Color.LightYellow);
            UpdateBoundingBoxes(this.scenery.Instance.GetObjectsByType(ModularSceneryObjectTypeEnum.Entrance), Color.PaleVioletRed);
            UpdateBoundingBoxes(this.scenery.Instance.GetObjectsByType(ModularSceneryObjectTypeEnum.Exit), Color.ForestGreen);
        }
        private void UpdateBoundingBoxes(ModelInstance[] items, Color color)
        {
            foreach (var item in items)
            {
                var bbox = item.GetBoundingBox();

                this.bboxesDrawer.Instance.SetLines(color, Line3D.CreateWiredBox(bbox));
            }
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                this.graphDrawer.Instance.Clear();

                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] is GraphNode node)
                    {
                        var color = node.Color;
                        var tris = node.Triangles;

                        this.graphDrawer.Instance.AddTriangles(color, tris);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.N))
            {
                this.ChangeToLevel("Lvl1");
            }

            if (this.Game.Input.KeyJustReleased(Keys.M))
            {
                this.ChangeToLevel("Lvl2");
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

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                var fileName = this.scenery.Instance.CurrentLevel.Name + nmFile;

                //Refresh the navigation mesh
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                this.UpdateNavigationGraph();
                this.UpdateGraphNodes(this.currentGraph == 0 ? this.ratAgentType : this.agent);
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                //Save the navigation triangles to a file
                var task = Task.Run(() =>
                {
                    if (!taskRunning)
                    {
                        taskRunning = true;

                        var fileName = this.scenery.Instance.CurrentLevel.Name + ntFile;

                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }

                        var loader = new LoaderOBJ();
                        var tris = this.GetTrianglesForNavigationGraph();
                        loader.Save(tris, fileName);

                        taskRunning = false;
                    }
                });
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                //Add obstacle
                this.AddTestObstacles();

                this.PaintObstacles();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                //Remove obstacle
                this.RemoveTestObstacles();

                this.PaintObstacles();
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

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.torch.Enabled = !this.torch.Enabled;
            }

            this.UpdateRat(gameTime);

            this.UpdateCamera(gameTime);

            this.UpdateEntities(gameTime);

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

            if (this.Walk(this.agent, prevPos, this.Camera.Position, out Vector3 walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }

            if (this.torch.Enabled)
            {
                this.torch.Position =
                    this.Camera.Position +
                    (this.Camera.Direction * 0.5f) +
                    (this.Camera.Left * 0.2f);
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

                var from = this.ratHoles[iFrom];
                var to = this.ratHoles[iTo];

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

            if (this.rat.Visible)
            {
                var bbox = this.rat.Instance.GetBoundingBox();

                this.ratDrawer.Instance.SetLines(Color.White, Line3D.CreateWiredBox(bbox));
            }
        }
        private void UpdateEntities(GameTime gameTime)
        {
            var sphere = new BoundingSphere(this.Camera.Position, doorDistance);

            var objTypes = ModularSceneryObjectTypeEnum.Entrance |
                ModularSceneryObjectTypeEnum.Exit |
                ModularSceneryObjectTypeEnum.Door |
                ModularSceneryObjectTypeEnum.Light;

            var items = this.scenery.Instance.GetObjectsInVolume(sphere, objTypes, true);
            if (items != null && items.Length > 0)
            {
                if (items[0].Object.Type == ModularSceneryObjectTypeEnum.Entrance)
                {
                    var msg = "The door locked when you closed it.\r\nYou must find an exit...";

                    PrepareMessage(true, msg);

                    return;
                }

                if (items[0].Object.Type == ModularSceneryObjectTypeEnum.Exit)
                {
                    var msg = "Press space to exit...";

                    PrepareMessage(true, msg);

                    UpdateExit(gameTime, items[0].Item);

                    return;
                }

                if (items[0].Object.Type == ModularSceneryObjectTypeEnum.Door)
                {
                    var msg = string.Format("Press space to {0} the door...", items[0].Item.Visible ? "open" : "close");

                    PrepareMessage(true, msg);

                    UpdateDoor(gameTime, items[0].Item);

                    return;
                }

                if (items[0].Object.Type == ModularSceneryObjectTypeEnum.Light)
                {
                    var lights = items[0].Item.Lights;

                    if (lights != null && lights.Length > 0)
                    {
                        var msg = string.Format("Press space to {0} the light...", lights[0].Enabled ? "turn off" : "turn on");

                        PrepareMessage(true, msg);

                        UpdateLight(gameTime, lights);

                        return;
                    }
                }
            }

            PrepareMessage(false, null);
        }
        private void PrepareMessage(bool show, string text)
        {
            if (show)
            {
                if (messages.Instance.Text != text)
                {
                    messages.Instance.Text = text;
                    messages.Instance.CenterHorizontally();
                    messages.Instance.CenterVertically();
                    messages.Visible = true;
                }
            }
            else
            {
                if (messages.Instance.Text != text)
                {
                    messages.Instance.Text = text;
                    messages.Visible = false;
                }
            }
        }
        private void UpdateExit(GameTime gameTime, ModelInstance item)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                // TODO: Set navigation between levels in levels file
                if (this.scenery.Instance.CurrentLevel.Name == "Lvl1")
                {
                    this.ChangeToLevel("Lvl2");
                }
                else if (this.scenery.Instance.CurrentLevel.Name == "Lvl2")
                {
                    this.Game.SetScene<SceneStart>();
                }
            }
        }
        private void UpdateDoor(GameTime gameTime, ModelInstance item)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                item.Visible = !item.Visible;

                messages.Instance.Text = string.Format("Press space to {0} the door...", item.Visible ? "open" : "close");
                messages.Instance.CenterHorizontally();
                messages.Instance.CenterVertically();

                var geom = new InputGeometry(this.GetTrianglesForNavigationGraph());
                ((Graph)this.navigationGraph).BuildTile(item.Manipulator.Position, geom);
                this.UpdateGraphNodes(this.currentGraph == 0 ? this.ratAgentType : this.agent);
            }
        }
        private void UpdateLight(GameTime gameTime, SceneLight[] lights)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                bool enabled = lights[0].Enabled;

                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].Enabled = !enabled;
                }

                messages.Instance.Text = string.Format("Press space to {0} the light...", !enabled ? "turn off" : "turn on");
                messages.Instance.CenterHorizontally();
                messages.Instance.CenterVertically();
            }
        }

        private void ChangeToLevel(string name)
        {
            this.Lights.ClearPointLights();
            this.Lights.ClearSpotLights();
            this.scenery.Instance.LoadLevel(name);
            this.Lights.Add(this.torch);

            this.UpdateNavigationGraph();
            var pos = this.scenery.Instance.CurrentLevel.StartPosition;
            var dir = this.scenery.Instance.CurrentLevel.LookingVector;
            pos.Y += agent.Height;
            this.Camera.Position = pos;
            this.Camera.Interest = pos + dir;
            this.UpdateDebug();
        }

        private void AddTestObstacles()
        {
            var bc1 = new BoundingCylinder(new Vector3(-1.21798706f, 3.50000000f, -26.1250477f), 0.8f, 2);
            var obstacle1 = Triangle.ComputeTriangleList(Topology.TriangleList, bc1, 32);
            obstacles.Add(this.AddObstacle(bc1), bc1);

            //var bc2 = new BoundingBox(
            //    new Vector3(-3.71798706f, 4.0f, -26.6250477f),
            //    new Vector3(-2.71798706f, 5.0f, -25.6250477f));
            //var obstacle2 = Triangle.ComputeTriangleList(Topology.TriangleList, bc2);
            //obstacles.Add(this.AddObstacle(bc2), bc2);

            var r3 = MathUtil.PiOverFour;
            var c3 = (new Vector3(-3.71798706f, 4.0f, -26.6250477f) + new Vector3(-2.71798706f, 5.0f, -25.6250477f)) * 0.5f;
            var bc3 = new OrientedBoundingBox(-Vector3.One * 0.5f, Vector3.One * 0.5f);
            bc3.Transform(Matrix.RotationY(r3) * Matrix.Translation(c3));

            var obstacle3 = Triangle.ComputeTriangleList(Topology.TriangleList, bc3);
            obstacles.Add(this.AddObstacle(bc3.Center, bc3.Extents, r3), bc3);
        }
        private void RemoveTestObstacles()
        {
            if (obstacles.Count > 0)
            {
                int obstacleIndex = obstacles.Keys.ToArray()[0];
                obstacles.Remove(obstacleIndex);
                this.RemoveObstacle(obstacleIndex);
            }
        }
        private void PaintObstacles()
        {
            this.obstacleDrawer.Instance.Clear(obstacleColor);

            foreach (var item in obstacles)
            {
                var obstacle = item.Value;

                Triangle[] obstacleTris = null;

                if (obstacle is BoundingCylinder)
                {
                    obstacleTris = Triangle.ComputeTriangleList(Topology.TriangleList, (BoundingCylinder)obstacle, 32);
                }
                else if (obstacle is BoundingBox)
                {
                    obstacleTris = Triangle.ComputeTriangleList(Topology.TriangleList, (BoundingBox)obstacle);
                }
                else if (obstacle is OrientedBoundingBox)
                {
                    obstacleTris = Triangle.ComputeTriangleList(Topology.TriangleList, (OrientedBoundingBox)obstacle);
                }

                if (obstacleTris != null && obstacleTris.Length > 0)
                {
                    this.obstacleDrawer.Instance.AddTriangles(obstacleColor, obstacleTris);
                }
            }
        }

        //public override void UpdateNavigationGraph()
        //{
        //    var fileName = this.scenery.Instance.CurrentLevel.Name + nmFile;

        //    if (File.Exists(fileName))
        //    {
        //        this.navigationGraph = new Graph();
        //        this.navigationGraph.Load(fileName);
        //    }
        //    else
        //    {
        //        base.UpdateNavigationGraph();
        //        this.navigationGraph.Save(fileName);
        //    }
        //}
        public override void NavigationGraphUpdated()
        {
            base.NavigationGraphUpdated();

            this.UpdateGraphNodes(this.currentGraph == 0 ? this.agent : this.ratAgentType);
        }
    }
}
