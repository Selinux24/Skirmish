using Engine;
using Engine.Animation;
using Engine.Audio;
using Engine.Common;
using Engine.Content;
using Engine.Content.FmtObj;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Collada
{
    public class SceneModularDungeon : Scene
    {
        private const int layerHUD = 99;

        private const float maxDistance = 35;

        private TextDrawer fps = null;
        private TextDrawer info = null;
        private SpriteProgressBar progressBar = null;

        private readonly Color ambientDown = new Color(127, 127, 127, 255);
        private readonly Color ambientUp = new Color(137, 116, 104, 255);

        private Player playerAgentType = null;
        private readonly Color agentTorchLight = new Color(255, 249, 224, 255);
        private readonly Vector3 cameraInitialPosition = new Vector3(1000, 1000, 1000);
        private readonly Vector3 cameraInitialInterest = new Vector3(1001, 1000, 1000);

        private SceneLightPoint torch = null;

        private ModularScenery scenery = null;

        private readonly float doorDistance = 3f;
        private TextDrawer messages = null;

        private Model rat = null;
        private BasicManipulatorController ratController = null;
        private Player ratAgentType = null;
        private bool ratActive = false;
        private float ratTime = 5f;
        private readonly float nextRatTime = 3f;
        private Vector3[] ratHoles = new Vector3[] { };

        private PrimitiveListDrawer<Triangle> selectedItemDrawer = null;
        private ModularSceneryItem selectedItem = null;

        private ModelInstanced human = null;

        private PrimitiveListDrawer<Line3D> bboxesDrawer = null;
        private PrimitiveListDrawer<Line3D> ratDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private PrimitiveListDrawer<Triangle> obstacleDrawer = null;
        private PrimitiveListDrawer<Line3D> connectionDrawer = null;
        private int currentGraph = 0;

        private readonly string nmFile = "nm.graph";
        private readonly string ntFile = "nm.obj";
        private bool taskRunning = false;

        private readonly List<ObstacleInfo> obstacles = new List<ObstacleInfo>();
        private readonly Color obstacleColor = new Color(Color.Pink.ToColor3(), 0.5f);

        private readonly Color connectionColor = new Color(Color.LightBlue.ToColor3(), 1f);

        private string soundDoor = null;
        private string soundLadder = null;
        private string soundTorch = null;

        private string[] soundWinds = null;
        private Vector3 windPosition = new Vector3(60, 0, -20);
        private bool windCreated = false;

        private string ratSoundMove = null;
        private string ratSoundTalk = null;
        private GameAudioEffect ratSoundInstance = null;

        private bool userInterfaceInitialized = false;
        private Guid userInterfaceId = Guid.NewGuid();
        private bool gameAssetsInitialized = false;
        private bool gameAssetsInitializing = false;
        private Guid gameAssetsId = Guid.NewGuid();
        private bool levelInitialized = false;
        private bool levelInitializing = false;
        private Guid levelId = Guid.NewGuid();
        private bool gameReady = false;

        private AgentType CurrentAgent
        {
            get
            {
                return this.currentGraph == 0 ? this.playerAgentType : this.ratAgentType;
            }
        }

        public SceneModularDungeon(Game game)
            : base(game, SceneModes.DeferredLightning)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = true;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif
            await this.LoadResourcesAsync(userInterfaceId, this.InitializeUI());
        }
        public override void GameResourcesLoaded(Guid id)
        {
            if (id == userInterfaceId && !userInterfaceInitialized)
            {
                userInterfaceInitialized = true;

                return;
            }

            if (id == gameAssetsId && !gameAssetsInitialized)
            {
                gameAssetsInitialized = true;

                this.InitializeEnvironment();
                this.InitializeLights();

                this.StartCamera();

                this.AudioManager.Start();
            }

            if (id == levelId && !levelInitialized)
            {
                levelInitialized = true;
            }
        }
        public override void OnReportProgress(float value)
        {
            if (progressBar != null)
            {
                progressBar.ProgressValue = value;
            }
        }
        public override async Task UpdateNavigationGraph()
        {
            if (this.scenery?.CurrentLevel == null)
            {
                return;
            }

            var fileName = this.scenery.CurrentLevel.Name + nmFile;

            if (File.Exists(fileName))
            {
                try
                {
                    var graph = await this.PathFinderDescription.Load(fileName);
                    if (graph != null)
                    {
                        this.NavigationGraphUpdating();

                        this.SetNavigationGraph(graph);

                        this.NavigationGraphUpdated();
                    }
                }
                catch (EngineException ex)
                {
                    Console.WriteLine($"Bad graph file. Generating navigation mesh. {ex.Message}");
                }
            }

            await base.UpdateNavigationGraph();

            await this.PathFinderDescription.Save(fileName, this.NavigationGraph);
        }
        public override void NavigationGraphUpdated()
        {
            if (!gameAssetsInitialized)
            {
                return;
            }

            //Update active paths with the new graph configuration
            if (this.ratController.HasPath)
            {
                Vector3 from = this.rat.Manipulator.Position;
                Vector3 to = this.ratController.Last;

                CalcPath(this.ratAgentType, from, to);
            }

            this.UpdateGraphDebug(this.CurrentAgent);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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

            if (!userInterfaceInitialized)
            {
                return;
            }

            this.fps.Text = this.Game.RuntimeText;
            this.info.Text = string.Format("{0}", this.GetRenderMode());

            if (!gameAssetsInitialized && !gameAssetsInitializing)
            {
                gameAssetsInitializing = true;

                Task.WhenAll(this.InitializeAssets());

                return;
            }

            if (!gameAssetsInitialized)
            {
                return;
            }

            if (this.Game.Input.KeyJustReleased(Keys.B))
            {
                this.LoadResources(levelId, this.ChangeToLevel("Lvl1"));
            }

            if (this.Game.Input.KeyJustReleased(Keys.N))
            {
                this.LoadResources(levelId, this.ChangeToLevel("Lvl2"));
            }

            if (this.Game.Input.KeyJustReleased(Keys.M))
            {
                this.LoadResources(levelId, this.ChangeToLevel("Lvl3"));
            }

            if (!levelInitialized && !levelInitializing)
            {
                levelInitializing = true;

                this.LoadResources(levelId, this.ChangeToLevel("Lvl1"));
            }

            if (!gameReady)
            {
                return;
            }

            this.UpdateRatController(gameTime);
            this.UpdateEntities();
            this.UpdateWind();

            this.UpdateDebugInput();
            this.UpdateGraphInput();
            this.UpdateRatInput();
            this.UpdatePlayerInput();
            this.UpdateEntitiesInput();

            this.UpdateSelection();
        }

        private async Task InitializeUI()
        {
            var title = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            title.Text = "Collada Modular Dungeon Scene";
            title.Position = Vector2.Zero;

            this.fps = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            this.info = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.info.Text = null;
            this.info.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                Name = "Back Panel",
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.info.Top + this.info.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            this.messages = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Lucida Casual", 48, Color.Red, Color.DarkRed), SceneObjectUsages.UI, layerHUD);
            this.messages.Text = null;
            this.messages.Position = new Vector2(0, 0);
            this.messages.Visible = false;

            var drawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "Seleced Items Drawer",
                AlphaEnabled = true,
                CastShadow = false,
                Count = 50000,
            };
            this.selectedItemDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(drawerDesc, SceneObjectUsages.UI, layerHUD);
            this.selectedItemDrawer.Visible = true;

            var pbDesc = new SpriteProgressBarDescription
            {
                Name = "Progress Bar",
                Top = this.Game.Form.RenderHeight - 20,
                Left = 100,
                Width = this.Game.Form.RenderWidth - 200,
                Height = 10,
                BaseColor = Color.Transparent,
                ProgressColor = Color.Green,
            };
            this.progressBar = await this.AddComponentSpriteProgressBar(pbDesc, SceneObjectUsages.UI, layerHUD);
        }
        private async Task InitializeAssets()
        {
            List<Task> tasks = new List<Task>
            {
                InitializeDebug(),
                InitializeDungeon(),
                InitializePlayer(),
                InitializeNPCs(),
                InitializeAudio(),
            };

            await this.LoadResourcesAsync(gameAssetsId, tasks.ToArray());
        }
        private void InitializeEnvironment()
        {
            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = 0.2f;
            nmsettings.CellHeight = 0.2f;

            //Agents
            nmsettings.Agents = new[] { playerAgentType, ratAgentType };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Layers;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 16;
            nmsettings.UseTileCache = true;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
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

            var desc = SceneLightPointDescription.Create(Vector3.Zero, 10f, 25f);

            this.torch = new SceneLightPoint("player_torch", true, this.agentTorchLight, this.agentTorchLight, true, desc);
            this.Lights.Add(torch);
        }
        private async Task InitializeDungeon()
        {
            var desc = new ModularSceneryDescription()
            {
                Name = "Dungeon",
                UseAnisotropic = true,
                CastShadow = true,
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources/SceneModularDungeon",
                    ModelContentFilename = "assets.xml",
                },
                AssetsConfigurationFile = "assetsmap.xml",
                LevelsFile = "levels.xml",
            };

            this.scenery = await this.AddComponentModularScenery(desc, SceneObjectUsages.Ground);
            this.scenery.TriggerEnd += TriggerEnds;

            this.SetGround(this.scenery, true);
        }
        private async Task InitializePlayer()
        {
            this.playerAgentType = new Player()
            {
                Name = "Player",
                Height = 1.5f,
                Radius = 0.2f,
                MaxClimb = 0.8f,
                MaxSlope = 50f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };

            await Task.CompletedTask;
        }
        private async Task InitializeNPCs()
        {
            await Task.WhenAll(
                this.InitializeRat(),
                this.InitializeHuman());
        }
        private async Task InitializeRat()
        {
            this.rat = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Rat",
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/SceneModularDungeon/Characters/Rat",
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

            this.rat.Manipulator.SetScale(0.5f, true);
            this.rat.Manipulator.SetPosition(0, 0, 0, true);
            this.rat.Visible = false;

            var ratPaths = new Dictionary<string, AnimationPlan>();
            this.ratController = new BasicManipulatorController();

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");
            ratPaths.Add("walk", new AnimationPlan(p0));

            this.rat.AnimationController.AddPath(ratPaths["walk"]);
            this.rat.AnimationController.TimeDelta = 1.5f;
        }
        private async Task InitializeHuman()
        {
            this.human = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Human Instanced",
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/SceneModularDungeon/Characters/Human2",
                        ModelContentFilename = "Human2.xml",
                    }
                });

            this.human.Visible = false;
        }
        private async Task InitializeDebug()
        {
            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 50000,
            };
            this.graphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(graphDrawerDesc);
            this.graphDrawer.Visible = false;

            var bboxesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Bounding volumes",
                AlphaEnabled = true,
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.25f),
                Count = 10000,
            };
            this.bboxesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(bboxesDrawerDesc);
            this.bboxesDrawer.Visible = false;

            var ratDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Rat",
                AlphaEnabled = true,
                Color = new Color4(0.0f, 1.0f, 1.0f, 0.25f),
                Count = 10000,
            };
            this.ratDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(ratDrawerDesc);
            this.ratDrawer.Visible = false;

            var obstacleDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Obstacles",
                AlphaEnabled = true,
                DepthEnabled = false,
                Count = 10000,
            };
            this.obstacleDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(obstacleDrawerDesc);
            this.obstacleDrawer.Visible = false;

            var connectionDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Connections",
                AlphaEnabled = true,
                Color = connectionColor,
                Count = 10000,
            };
            this.connectionDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(connectionDrawerDesc);
            this.connectionDrawer.Visible = false;
        }
        private async Task InitializeAudio()
        {
            this.AudioManager.MasterVolume = 1;
            this.AudioManager.UseMasteringLimiter = true;
            this.AudioManager.SetMasteringLimit(15, 1500);

            //Sounds
            soundDoor = "door";
            soundLadder = "ladder";
            this.AudioManager.LoadSound(soundDoor, "Resources/SceneModularDungeon/Audio/Effects", "door.wav");
            this.AudioManager.LoadSound(soundLadder, "Resources/SceneModularDungeon/Audio/Effects", "ladder.wav");

            string soundWind1 = "wind1";
            string soundWind2 = "wind2";
            string soundWind3 = "wind3";
            this.AudioManager.LoadSound(soundWind1, "Resources/SceneModularDungeon/Audio/Effects", "Wind1_S.wav");
            this.AudioManager.LoadSound(soundWind2, "Resources/SceneModularDungeon/Audio/Effects", "Wind2_S.wav");
            this.AudioManager.LoadSound(soundWind3, "Resources/SceneModularDungeon/Audio/Effects", "Wind3_S.wav");
            this.soundWinds = new[] { soundWind1, soundWind2, soundWind3 };

            ratSoundMove = "mouseMove";
            ratSoundTalk = "mouseTalk";
            this.AudioManager.LoadSound(ratSoundMove, "Resources/SceneModularDungeon/Audio/Effects", "mouse1.wav");
            this.AudioManager.LoadSound(ratSoundTalk, "Resources/SceneModularDungeon/Audio/Effects", "mouse2.wav");

            soundTorch = "torch";
            this.AudioManager.LoadSound(soundTorch, "Resources/SceneModularDungeon/Audio/Effects", "loop_torch.wav");

            //Effects
            this.AudioManager.AddEffectParams(
                soundDoor,
                new GameAudioEffectParameters
                {
                    SoundName = soundDoor,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    UseAudio3D = true,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            this.AudioManager.AddEffectParams(
                soundLadder,
                new GameAudioEffectParameters
                {
                    SoundName = soundLadder,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    UseAudio3D = true,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            for (int i = 0; i < soundWinds.Length; i++)
            {
                this.AudioManager.AddEffectParams(
                    soundWinds[i],
                    new GameAudioEffectParameters
                    {
                        DestroyWhenFinished = true,
                        IsLooped = false,
                        SoundName = soundWinds[i],
                        Volume = 1f,
                        UseAudio3D = true,
                        EmitterRadius = 15,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });
            }

            this.AudioManager.AddEffectParams(
                ratSoundMove,
                new GameAudioEffectParameters
                {
                    SoundName = ratSoundMove,
                    DestroyWhenFinished = false,
                    Volume = 1f,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            this.AudioManager.AddEffectParams(
                ratSoundTalk,
                new GameAudioEffectParameters
                {
                    SoundName = ratSoundTalk,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            await Task.CompletedTask;
        }

        private void StartCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = maxDistance;
            this.Camera.MovementDelta = playerAgentType.Velocity;
            this.Camera.SlowMovementDelta = playerAgentType.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = cameraInitialPosition;
            this.Camera.Interest = cameraInitialInterest;
        }
        private void UpdateDebugInfo()
        {
            //Graph
            this.bboxesDrawer.Clear();

            //Boxes
            Random rndBoxes = new Random(1);

            var dict = this.scenery.GetMapVolumes();

            foreach (var item in dict.Values)
            {
                var color = rndBoxes.NextColor().ToColor4();
                color.Alpha = 0.40f;

                this.bboxesDrawer.SetPrimitives(color, Line3D.CreateWiredBox(item.ToArray()));
            }

            //Objects
            UpdateBoundingBoxes(this.scenery.GetObjectsByType(ModularSceneryObjectTypes.Entrance).Select(o => o.Item), Color.PaleVioletRed);
            UpdateBoundingBoxes(this.scenery.GetObjectsByType(ModularSceneryObjectTypes.Exit).Select(o => o.Item), Color.ForestGreen);
            UpdateBoundingBoxes(this.scenery.GetObjectsByType(ModularSceneryObjectTypes.Trigger).Select(o => o.Item), Color.Cyan);
            UpdateBoundingBoxes(this.scenery.GetObjectsByType(ModularSceneryObjectTypes.Door).Select(o => o.Item), Color.LightYellow);
            UpdateBoundingBoxes(this.scenery.GetObjectsByType(ModularSceneryObjectTypes.Light).Select(o => o.Item), Color.MediumPurple);
        }
        private void UpdateBoundingBoxes(IEnumerable<ModelInstance> items, Color color)
        {
            List<Line3D> lines = new List<Line3D>();

            foreach (var item in items)
            {
                var bbox = item.GetBoundingBox(true);

                lines.AddRange(Line3D.CreateWiredBox(bbox));
            }

            this.bboxesDrawer.SetPrimitives(color, lines);
        }
        private void TriggerEnds(object sender, ModularSceneryTriggerEventArgs e)
        {
            if (e.Items.Any())
            {
                var obs = obstacles.Where(o => e.Items.Select(i => i.Item).Contains(o.Item)).ToList();
                if (obs.Any())
                {
                    //Refresh affected obstacles (if any)
                    obs.ForEach(o =>
                    {
                        var obb = OrientedBoundingBoxExtensions.FromPoints(
                            o.Item.GetPoints(true),
                            o.Item.Manipulator.FinalTransform);

                        this.RemoveObstacle(o.Index);
                        o.Index = this.AddObstacle(obb);
                        o.Obstacle = obb;
                    });

                    PaintObstacles();
                }
            }
        }

        private void UpdatePlayerInput()
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
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                this.Game.GameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            if (this.Walk(this.playerAgentType, prevPos, this.Camera.Position, out Vector3 walkerPos))
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

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.torch.Enabled = !this.torch.Enabled;
            }
        }
        private void UpdateDebugInput()
        {
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
                this.obstacleDrawer.Visible = !this.obstacleDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.ratDrawer.Visible = !this.ratDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                //Frustum
                var frustum = Line3D.CreateWiredFrustum(this.Camera.Frustum);

                this.bboxesDrawer.SetPrimitives(Color.White, frustum);
            }
        }
        private void UpdateGraphInput()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                //Refresh the navigation mesh
                this.RefreshNavigation();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                //Save the navigation triangles to a file
                this.SaveGraphToFile();
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.currentGraph++;
                this.currentGraph %= 2;

                this.UpdateGraphDebug(this.CurrentAgent);
            }
        }
        private void UpdateRatInput()
        {
            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.rat.Visible = false;
                this.ratActive = false;
                this.ratController.Clear();
            }
        }
        private void UpdateEntitiesInput()
        {
            if (this.selectedItem == null)
            {
                return;
            }

            if (this.selectedItem.Object.Type == ModularSceneryObjectTypes.Exit)
            {
                UpdateEntityExit(this.selectedItem);
            }

            if (this.selectedItem.Object.Type == ModularSceneryObjectTypes.Trigger)
            {
                UpdateEntityTrigger(this.selectedItem);
            }

            if (this.selectedItem.Object.Type == ModularSceneryObjectTypes.Light)
            {
                UpdateEntityLight(this.selectedItem);
            }
        }

        private void UpdateWind()
        {
            if (!windCreated)
            {
                CreateWind(0);

                windCreated = true;
            }
        }

        private void UpdateSelection()
        {
            if (this.selectedItem == null)
            {
                return;
            }

            var tris = this.selectedItem.Item.GetTriangles(true);
            if (tris.Any())
            {
                Color4 sItemColor = Color.LightYellow;
                sItemColor.Alpha = 0.3333f;

                this.selectedItemDrawer.SetPrimitives(sItemColor, tris);
            }
        }
        private void UpdateRatController(GameTime gameTime)
        {
            this.ratTime -= gameTime.ElapsedSeconds;

            if (!this.ratHoles.Any())
            {
                return;
            }

            if (this.ratActive)
            {
                this.ratController.UpdateManipulator(gameTime, this.rat.Manipulator);
                if (!this.ratController.HasPath)
                {
                    this.ratActive = false;
                    this.ratTime = this.nextRatTime;
                    this.rat.Visible = false;
                    this.ratController.Clear();

                    this.ratSoundInstance?.Pause();
                    this.RatTalkPlay();
                }
            }

            if (!this.ratActive && this.ratTime <= 0)
            {
                var iFrom = Helper.RandomGenerator.Next(0, this.ratHoles.Length);
                var iTo = Helper.RandomGenerator.Next(0, this.ratHoles.Length);
                if (iFrom == iTo) return;

                var from = this.ratHoles[iFrom];
                var to = this.ratHoles[iTo];

                this.rat.Manipulator.SetPosition(from);

                if (CalcPath(this.ratAgentType, from, to))
                {
                    this.ratController.UpdateManipulator(gameTime, this.rat.Manipulator);

                    this.ratSoundInstance?.Play();
                    this.RatTalkPlay();
                }
            }

            if (this.rat.Visible && this.ratDrawer.Visible)
            {
                var bbox = this.rat.GetBoundingBox();

                this.ratDrawer.SetPrimitives(Color.White, Line3D.CreateWiredBox(bbox));
            }
        }
        private bool CalcPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var path = this.FindPath(agent, from, to);
            if (path?.ReturnPath?.Count > 0)
            {
                path.ReturnPath.Insert(0, from);
                path.Normals.Insert(0, Vector3.Up);

                path.ReturnPath.Add(to);
                path.Normals.Add(Vector3.Up);

                this.ratDrawer.SetPrimitives(Color.Red, Line3D.CreateLineList(path.ReturnPath.ToArray()));

                this.ratController.Follow(new NormalPath(path.ReturnPath.ToArray(), path.Normals.ToArray()));
                this.ratController.MaximumSpeed = this.ratAgentType.Velocity;
                this.rat.Visible = true;
                this.rat.AnimationController.Start(0);

                this.ratActive = true;
                this.ratTime = this.nextRatTime;

                return true;
            }

            return false;
        }
        private void RatTalkPlay()
        {
            this.AudioManager.CreateEffectInstance(ratSoundTalk, this.rat, this.Camera)?.Play();
        }
        private void UpdateEntities()
        {
            var sphere = new BoundingSphere(this.Camera.Position, doorDistance);

            var objTypes =
                ModularSceneryObjectTypes.Entrance |
                ModularSceneryObjectTypes.Exit |
                ModularSceneryObjectTypes.Door |
                ModularSceneryObjectTypes.Trigger |
                ModularSceneryObjectTypes.Light;

            var ray = this.GetPickingRay();
            float minDist = 1.2f;

            //Test items into the camera frustum and nearest to the player
            var items =
                this.scenery.GetObjectsInVolume(sphere, objTypes, false, true)
                .Where(i => this.Camera.Frustum.Contains(i.Item.GetBoundingBox()) != ContainmentType.Disjoint)
                .Where(i =>
                {
                    if (i.Item.PickNearest(ray, out var res))
                    {
                        return true;
                    }
                    else
                    {
                        var bbox = i.Item.GetBoundingBox();
                        var center = bbox.GetCenter();
                        var extents = bbox.GetExtents();
                        extents *= minDist;

                        var sBbox = new BoundingBox(center - extents, center + extents);

                        return sBbox.Intersects(ref ray);
                    }
                })
                .ToList();

            if (items.Any())
            {
                //Sort by distance to the picking ray
                items.Sort((i1, i2) =>
                {
                    float d1 = CalcItemPickingDistance(ray, i1);
                    float d2 = CalcItemPickingDistance(ray, i2);

                    return d1.CompareTo(d2);
                });

                this.SetSelectedItem(items.First());
            }
            else
            {
                this.SetSelectedItem(null);
            }
        }
        private float CalcItemPickingDistance(Ray ray, ModularSceneryItem item)
        {
            if (item.Item.PickNearest(ray, out var res))
            {
                return res.Distance;
            }
            else
            {
                var sph = item.Item.GetBoundingSphere();

                return Intersection.DistanceFromPointToLine(ray, sph.Center);
            }
        }

        private void SetSelectedItem(ModularSceneryItem item)
        {
            if (item == this.selectedItem)
            {
                return;
            }

            this.selectedItem = item;

            if (item == null)
            {
                this.selectedItemDrawer.Clear();

                PrepareMessage(false, null);

                return;
            }

            if (item.Object.Type == ModularSceneryObjectTypes.Entrance)
            {
                var msg = "The door locked when you closed it.\r\nYou must find an exit...";

                PrepareMessage(true, msg);
            }
            else if (item.Object.Type == ModularSceneryObjectTypes.Exit)
            {
                var msg = "Press space to exit...";

                PrepareMessage(true, msg);
            }
            else if (item.Object.Type == ModularSceneryObjectTypes.Door)
            {
                var msg = string.Format("Press space to {0} the door...", item.Item.Visible ? "open" : "close");

                PrepareMessage(true, msg);
            }
            else if (item.Object.Type == ModularSceneryObjectTypes.Trigger)
            {
                SetSelectedItemTrigger(item);
            }
            else if (item.Object.Type == ModularSceneryObjectTypes.Light)
            {
                SetSelectedItemLight(item);
            }
        }
        private void SetSelectedItemTrigger(ModularSceneryItem item)
        {
            var triggers = this.scenery.GetTriggersByObject(item);
            if (triggers.Any())
            {
                int index = 1;
                var msg = string.Join(", ", triggers.Select(t => $"Press {index++} to {t.Name} the {item.Object.Name}"));

                PrepareMessage(true, msg);
            }
        }
        private void SetSelectedItemLight(ModularSceneryItem item)
        {
            var lights = item.Item.Lights;
            if (lights.Any())
            {
                var msg = string.Format("Press space to {0} the light...", lights.First().Enabled ? "turn off" : "turn on");

                PrepareMessage(true, msg);
            }
        }
        private void PrepareMessage(bool show, string text)
        {
            if (show)
            {
                if (messages.Text != text)
                {
                    messages.Text = text;
                    messages.CenterHorizontally();
                    messages.CenterVertically();
                    messages.Visible = true;
                }
            }
            else
            {
                if (messages.Text != text)
                {
                    messages.Text = text;
                    messages.Visible = false;
                }
            }
        }
        private void UpdateEntityExit(ModularSceneryItem item)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                Task.Run(async () =>
                {
                    var effect = this.AudioManager.CreateEffectInstance(soundDoor, item.Item, this.Camera);
                    if (effect != null)
                    {
                        effect.Play();

                        await Task.Delay(effect.Duration);
                    }

                    string nextLevel = item.Object.NextLevel;
                    if (!string.IsNullOrEmpty(nextLevel))
                    {
                        await this.LoadResourcesAsync(Guid.NewGuid(), this.ChangeToLevel(nextLevel));
                    }
                    else
                    {
                        this.Game.SetScene<SceneStart>();
                    }
                });
            }
        }
        private void UpdateEntityTrigger(ModularSceneryItem item)
        {
            var triggers = this.scenery
                .GetTriggersByObject(item)
                .ToArray();

            if (triggers.Any())
            {
                int keyIndex = ReadKeyIndex();
                if (keyIndex > 0 && keyIndex <= triggers.Length)
                {
                    this.AudioManager.CreateEffectInstance(soundLadder)?.Play();
                    this.scenery.ExecuteTrigger(item, triggers[keyIndex - 1]);
                }
            }
        }
        private void UpdateEntityLight(ModularSceneryItem item)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                bool enabled = false;

                var lights = item.Item.Lights;
                if (lights.Any())
                {
                    enabled = !lights.First().Enabled;

                    foreach (var light in lights)
                    {
                        light.Enabled = enabled;
                    }
                }

                var emitters = item.Emitters;
                if (emitters?.Length > 0)
                {
                    foreach (var emitter in emitters)
                    {
                        emitter.Visible = enabled;
                    }
                }

                messages.Text = string.Format("Press space to {0} the light...", enabled ? "turn off" : "turn on");
                messages.CenterHorizontally();
                messages.CenterVertically();
            }
        }

        /// <summary>
        /// Reads the keyboard looking for the first numeric key pressed
        /// </summary>
        /// <returns>Returns the first just released numeric key value</returns>
        private int ReadKeyIndex()
        {
            if (this.Game.Input.KeyJustReleased(Keys.D0)) return 0;
            if (this.Game.Input.KeyJustReleased(Keys.D1)) return 1;
            if (this.Game.Input.KeyJustReleased(Keys.D2)) return 2;
            if (this.Game.Input.KeyJustReleased(Keys.D3)) return 3;
            if (this.Game.Input.KeyJustReleased(Keys.D4)) return 4;
            if (this.Game.Input.KeyJustReleased(Keys.D5)) return 5;
            if (this.Game.Input.KeyJustReleased(Keys.D6)) return 6;
            if (this.Game.Input.KeyJustReleased(Keys.D7)) return 7;
            if (this.Game.Input.KeyJustReleased(Keys.D8)) return 8;
            if (this.Game.Input.KeyJustReleased(Keys.D9)) return 9;

            return -1;
        }

        private void RefreshNavigation()
        {
            var fileName = this.scenery.CurrentLevel.Name + nmFile;

            //Refresh the navigation mesh
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            Task.WhenAll(this.UpdateNavigationGraph());
        }
        private void SaveGraphToFile()
        {
            Task.Run(() =>
            {
                if (!taskRunning)
                {
                    taskRunning = true;
                    try
                    {
                        var fileName = this.scenery.CurrentLevel.Name + ntFile;

                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }

                        var loader = new LoaderObj();
                        var tris = this.GetTrianglesForNavigationGraph();
                        loader.Save(tris, fileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        taskRunning = false;
                    }
                }
            });
        }

        private async Task ChangeToLevel(string name)
        {
            gameReady = false;

            this.Camera.Position = cameraInitialPosition;
            this.Camera.Interest = cameraInitialInterest;

            this.Lights.ClearPointLights();
            this.Lights.ClearSpotLights();

            this.AudioManager.Stop();
            this.AudioManager.ClearEffects();

            this.ClearNPCs();
            this.ClearDebugDrawers();

            this.SetSelectedItem(null);

            await this.scenery.LoadLevel(name);

            this.ConfigureNavigationGraph();

            await this.UpdateNavigationGraph();

            this.StartEntities();

            var pos = this.scenery.CurrentLevel.StartPosition;
            var dir = this.scenery.CurrentLevel.LookingVector;
            pos.Y += playerAgentType.Height;
            this.Camera.Position = pos;
            this.Camera.Interest = pos + dir;

            this.Lights.Add(this.torch);

            this.AudioManager.Start();

            gameReady = true;
        }

        private void ConfigureNavigationGraph()
        {
            this.PathFinderDescription.Input.ClearConnections();

            if (this.scenery.CurrentLevel.Name == "Lvl1")
            {
                this.PathFinderDescription.Input.AddConnection(
                    new Vector3(-8.98233700f, 4.76837158e-07f, 0.0375497341f),
                    new Vector3(-11.0952349f, -4.76837158e-07f, 0.00710105896f),
                    1,
                    1,
                    GraphConnectionAreaTypes.Jump,
                    GraphConnectionFlagTypes.All);

                this.PathFinderDescription.Input.AddConnection(
                    new Vector3(17, 0, -14),
                    new Vector3(16, 0, -15),
                    0.15f,
                    0,
                    GraphConnectionAreaTypes.Ground,
                    GraphConnectionFlagTypes.All);
            }

            this.PaintConnections();
        }
        private void StartEntities()
        {
            //Rat holes
            this.ratHoles = this.scenery
                .GetObjectsByName("Dn_Rat_Hole_1")
                .Select(o => o.Item.Manipulator.Position)
                .ToArray();

            //Jails
            this.StartEntitiesJails();

            //Doors
            this.StartEntitiesDoors();

            //Ladders
            this.StartEntitiesLadders();

            //NPCs
            this.StartNPCs();

            //Furniture obstacles
            this.StartEntitiesObstacles();

            //Sounds
            this.StartEntitiesAudio();
        }
        private void StartEntitiesJails()
        {
            var jails = this.scenery
                .GetObjectsByName("Dn_Jail_1")
                .Select(o => o.Item);

            AnimationPath def = new AnimationPath();
            def.Add("default");

            foreach (var jail in jails)
            {
                jail.AnimationController.SetPath(new AnimationPlan(def));
                jail.InvalidateCache();
            }
        }
        private void StartEntitiesDoors()
        {
            var doors = this.scenery
                .GetObjectsByName("Dn_Door_1")
                .Select(o => o.Item);

            AnimationPath def = new AnimationPath();
            def.Add("default");

            foreach (var door in doors)
            {
                door.AnimationController.SetPath(new AnimationPlan(def));
                door.InvalidateCache();
            }
        }
        private void StartEntitiesLadders()
        {
            var ladders = this.scenery
                .GetObjectsByName("Dn_Anim_Ladder")
                .Select(o => o.Item);

            AnimationPath def = new AnimationPath();
            def.Add("pull");

            foreach (var ladder in ladders)
            {
                ladder.AnimationController.SetPath(new AnimationPlan(def));
                ladder.InvalidateCache();
            }
        }
        private void StartNPCs()
        {
            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("stand");

            if (this.scenery.CurrentLevel.Name != "Lvl1")
            {
                this.human.Visible = false;
            }

            for (int i = 0; i < this.human.InstanceCount; i++)
            {
                this.human[i].Manipulator.SetPosition(31, 0, i == 0 ? -31 : -29, true);
                this.human[i].Manipulator.SetRotation(-MathUtil.PiOverTwo, 0, 0, true);

                this.human[i].AnimationController.AddPath(new AnimationPlan(p0));
                this.human[i].AnimationController.Start(i * 1f);
                this.human[i].AnimationController.TimeDelta = 0.5f + (i * 0.1f);
            }

            this.human.Visible = true;
        }
        private void StartEntitiesAudio()
        {
            //Rat sound
            this.ratSoundInstance = this.AudioManager.CreateEffectInstance(ratSoundMove, this.rat, this.Camera);

            //Torchs
            this.StartEntitiesAudioTorchs();

            //Big fires
            this.StartEntitiesAudioBigFires();
        }
        private void StartEntitiesAudioTorchs()
        {
            var torchs = this.scenery
                .GetObjectsByName("Dn_Torch")
                .Select(o => o.Item);

            int index = 0;
            foreach (var item in torchs)
            {
                string effectName = $"torch{index++}";

                this.AudioManager.AddEffectParams(
                    effectName,
                    new GameAudioEffectParameters
                    {
                        SoundName = soundTorch,
                        DestroyWhenFinished = false,
                        Volume = 0.05f,
                        IsLooped = true,
                        UseAudio3D = true,
                        EmitterRadius = 2,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });

                this.AudioManager.CreateEffectInstance(effectName, item, this.Camera).Play();
            }
        }
        private void StartEntitiesAudioBigFires()
        {
            List<ModelInstance> fires = new List<ModelInstance>();
            fires.AddRange(this.scenery.GetObjectsByName("Dn_Temple_Fire_1").Select(o => o.Item));
            fires.AddRange(this.scenery.GetObjectsByName("Dn_Big_Lamp_1").Select(o => o.Item));

            int index = 0;
            foreach (var item in fires)
            {
                string effectName = $"bigFire{index++}";

                this.AudioManager.AddEffectParams(
                    effectName,
                    new GameAudioEffectParameters
                    {
                        SoundName = soundTorch,
                        DestroyWhenFinished = false,
                        Volume = 1,
                        IsLooped = true,
                        UseAudio3D = true,
                        EmitterRadius = 5,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });

                this.AudioManager.CreateEffectInstance(effectName, item, this.Camera).Play();
            }
        }
        private void StartEntitiesObstacles()
        {
            obstacles.Clear();

            //Object obstacles
            var sceneryObjects = this.scenery
                .GetObjectsByType(ModularSceneryObjectTypes.Furniture | ModularSceneryObjectTypes.Door)
                .Select(o => o.Item);

            foreach (var item in sceneryObjects)
            {
                var obb = OrientedBoundingBoxExtensions.FromPoints(item.GetPoints(), item.Manipulator.FinalTransform);

                int index = this.AddObstacle(obb);
                if (index >= 0)
                {
                    obstacles.Add(new ObstacleInfo { Index = index, Item = item, Obstacle = obb });
                }
            }

            //Human obstacles
            for (int i = 0; i < this.human.InstanceCount; i++)
            {
                var pos = this.human[i].Manipulator.Position;
                var bc = new BoundingCylinder(pos, 0.8f, 1.5f);

                int index = this.AddObstacle(bc);
                if (index >= 0)
                {
                    obstacles.Add(new ObstacleInfo { Index = index, Item = this.human[i], Obstacle = bc });
                }
            }

            PaintObstacles();
        }

        private void ClearNPCs()
        {
            this.ratActive = false;
            this.rat.Visible = false;
            this.ratController.Clear();

            this.human.Visible = false;
        }
        private void ClearDebugDrawers()
        {
            bboxesDrawer.Clear();
            ratDrawer.Clear();
            graphDrawer.Clear();
            obstacleDrawer.Clear();
            connectionDrawer.Clear();
        }

        private void PaintObstacles()
        {
            this.obstacleDrawer.Clear(obstacleColor);

            foreach (var item in obstacles)
            {
                var obstacle = item.Obstacle;

                IEnumerable<Triangle> obstacleTris = null;

                if (obstacle is BoundingCylinder bc)
                {
                    obstacleTris = Triangle.ComputeTriangleList(Topology.TriangleList, bc, 32);
                }
                else if (obstacle is BoundingBox bbox)
                {
                    obstacleTris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox);
                }
                else if (obstacle is OrientedBoundingBox obb)
                {
                    obstacleTris = Triangle.ComputeTriangleList(Topology.TriangleList, obb);
                }

                if (obstacleTris?.Any() == true)
                {
                    this.obstacleDrawer.AddPrimitives(obstacleColor, obstacleTris);
                }
            }
        }
        private void PaintConnections()
        {
            this.connectionDrawer.Clear(connectionColor);

            var conns = this.PathFinderDescription.Input.GetConnections();

            foreach (var conn in conns)
            {
                var arclines = Line3D.CreateArc(conn.Start, conn.End, 0.25f, 8);
                this.connectionDrawer.AddPrimitives(connectionColor, arclines);

                var cirlinesF = Line3D.CreateCircle(conn.Start, conn.Radius, 32);
                this.connectionDrawer.AddPrimitives(connectionColor, cirlinesF);

                if (conn.Direction == 1)
                {
                    var cirlinesT = Line3D.CreateCircle(conn.End, conn.Radius, 32);
                    this.connectionDrawer.AddPrimitives(connectionColor, cirlinesT);
                }

                this.connectionDrawer.Visible = true;
            }
        }

        private void UpdateGraphDebug(AgentType agent)
        {
            var nodes = this.BuildGraphNodeDebugAreas(agent);

            this.graphDrawer.Clear();
            this.graphDrawer.SetPrimitives(nodes);

            this.UpdateDebugInfo();
        }
        private Dictionary<Color4, IEnumerable<Triangle>> BuildGraphNodeDebugAreas(AgentType agent)
        {
            Dictionary<Color4, IEnumerable<Triangle>> res = new Dictionary<Color4, IEnumerable<Triangle>>();

            var nodes = this.GetNodes(agent).OfType<GraphNode>();
            if (nodes.Any())
            {
                foreach (var node in nodes)
                {
                    var color = node.Color;
                    var tris = node.Triangles;

                    if (!res.ContainsKey(color))
                    {
                        res.Add(color, new List<Triangle>(tris));
                    }
                    else
                    {
                        ((List<Triangle>)res[color]).AddRange(tris);
                    }
                }
            }

            return res;
        }

        private void CreateWind(int index)
        {
            int duration = 100;

            Manipulator3D man = new Manipulator3D();
            man.SetPosition(windPosition, true);

            var soundEffect = this.soundWinds[index];

            var windInstance = AudioManager.CreateEffectInstance(soundEffect, man, this.Camera);
            if (windInstance != null)
            {
                windInstance.Play();

                float durationVariation = Helper.RandomGenerator.NextFloat(0.5f, 1.0f);
                duration = (int)(windInstance.Duration.TotalMilliseconds * durationVariation);
            }

            index = Helper.RandomGenerator.Next(0, soundWinds.Length + 1);
            index %= soundWinds.Length;

            Task.Run(async () =>
            {
                await Task.Delay(duration);

                if (AudioManager != null)
                {
                    CreateWind(index);
                }
            }).ConfigureAwait(false);
        }
    }
}
