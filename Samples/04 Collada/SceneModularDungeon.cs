using Engine;
using Engine.Animation;
using Engine.Audio;
using Engine.Common;
using Engine.Content;
using Engine.Content.FmtObj;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using Engine.UI.Tween;
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

        private UITextArea fps = null;
        private UITextArea info = null;
        private UIProgressBar progressBar = null;
        private UIConsole console = null;

        private readonly Color ambientDown = new Color(127, 127, 127, 255);
        private readonly Color ambientUp = new Color(137, 116, 104, 255);

        private Player playerAgentType = null;
        private readonly Color agentTorchLight = new Color(255, 249, 224, 255);
        private readonly Vector3 cameraInitialPosition = new Vector3(1000, 1000, 1000);
        private readonly Vector3 cameraInitialInterest = new Vector3(1001, 1000, 1000);

        private SceneLightPoint torch = null;

        private ModularScenery scenery = null;

        private readonly float doorDistance = 3f;
        private UITextArea messages = null;

        private Model rat = null;
        private BasicManipulatorController ratController = null;
        private Player ratAgentType = null;
        private bool ratActive = false;
        private float ratTime = 5f;
        private readonly float nextRatTime = 3f;
        private Vector3[] ratHoles = new Vector3[] { };

        private PrimitiveListDrawer<Triangle> selectedItemDrawer = null;
        private ModularSceneryItem selectedItem = null;
        private bool selectedItemPainted = false;

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
        private IAudioEffect ratSoundInstance = null;

        private bool userInterfaceInitialized = false;
        private bool gameAssetsInitialized = false;
        private bool levelInitialized = false;
        private bool gameReady = false;

        private AgentType CurrentAgent
        {
            get
            {
                return currentGraph == 0 ? playerAgentType : ratAgentType;
            }
        }

        public SceneModularDungeon(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

#if DEBUG
            Game.VisibleMouse = true;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif
            await LoadResourcesAsync(InitializeUI(), (res) =>
            {
                if (!res.Completed)
                {
                    res.ThrowExceptions();
                }

                userInterfaceInitialized = true;

                Task.WhenAll(InitializeAssets());
            });
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
            if (scenery?.CurrentLevel == null)
            {
                return;
            }

            var fileName = scenery.CurrentLevel.Name + nmFile;

            if (File.Exists(fileName))
            {
                try
                {
                    var graph = await PathFinderDescription.Load(fileName);
                    if (graph != null)
                    {
                        NavigationGraphUpdating();

                        SetNavigationGraph(graph);

                        NavigationGraphUpdated();

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError($"Bad graph file. Generating navigation mesh. {ex.Message}", ex);
                }
            }

            await base.UpdateNavigationGraph();

            try
            {
                await PathFinderDescription.Save(fileName, NavigationGraph);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Error saving graph file. {ex.Message}", ex);
            }
        }
        public override void NavigationGraphUpdated()
        {
            if (!gameAssetsInitialized)
            {
                return;
            }

            //Update active paths with the new graph configuration
            if (ratController.HasPath)
            {
                Vector3 from = rat.Manipulator.Position;
                Vector3 to = ratController.Last;

                CalcPath(ratAgentType, from, to);
            }

            UpdateGraphDebug(CurrentAgent);
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!userInterfaceInitialized)
            {
                return;
            }

            fps.Text = Game.RuntimeText;
            info.Text = string.Format("{0}", GetRenderMode());

            if (!gameAssetsInitialized)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.B))
            {
                ChangeToLevel("Lvl1");
            }

            if (Game.Input.KeyJustReleased(Keys.N))
            {
                ChangeToLevel("Lvl2");
            }

            if (Game.Input.KeyJustReleased(Keys.M))
            {
                ChangeToLevel("Lvl3");
            }

            if (!levelInitialized)
            {
                return;
            }

            if (!gameReady)
            {
                return;
            }

            UpdateRatController(gameTime);
            UpdateEntities();
            UpdateWind();

            UpdateDebugInput();
            UpdateGraphInput();
            UpdateRatInput();
            UpdatePlayerInput();
            UpdateEntitiesInput();

            UpdateSelection();
        }

        private async Task InitializeUI()
        {
            console = await this.AddComponentUIConsole(UIConsoleDescription.Default(), layerHUD + 1);
            console.Visible = false;

            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);
            title.Text = "Collada Modular Dungeon Scene";
            title.SetPosition(Vector2.Zero);

            fps = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            fps.Text = null;
            fps.SetPosition(new Vector2(0, 24));

            info = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, layerHUD);
            info.Text = null;
            info.SetPosition(new Vector2(0, 48));

            var spDesc = new SpriteDescription()
            {
                Name = "Back Panel",
                Width = Game.Form.RenderWidth,
                Height = info.Top + info.Height + 3,
                BaseColor = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            messages = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 48, Color.Red, Color.DarkRed) }, layerHUD);
            messages.Text = null;
            messages.SetPosition(new Vector2(0, 0));
            messages.Visible = false;

            var drawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "Selected Items Drawer",
                CastShadow = false,
                Count = 50000,
                BlendMode = BlendModes.Opaque | BlendModes.Additive,
            };
            selectedItemDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(drawerDesc, SceneObjectUsages.UI, layerHUD);
            selectedItemDrawer.Visible = true;

            var pbDesc = new UIProgressBarDescription
            {
                Name = "Progress Bar",
                Top = Game.Form.RenderHeight - 20,
                Left = 100,
                Width = Game.Form.RenderWidth - 200,
                Height = 10,
                BaseColor = Color.Transparent,
                ProgressColor = Color.Green,
            };
            progressBar = await this.AddComponentUIProgressBar(pbDesc, layerHUD);
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

            await LoadResourcesAsync(tasks.ToArray(), (res) =>
            {
                if (!res.Completed)
                {
                    res.ThrowExceptions();
                }

                gameAssetsInitialized = true;

                InitializeEnvironment();
                InitializeLights();

                StartCamera();

                AudioManager.Start();

                ChangeToLevel(null);
            });
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

            PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
        }
        private void InitializeLights()
        {
            Lights.HemisphericLigth = new SceneLightHemispheric("hemi_light", ambientDown, ambientUp, true);
            Lights.KeyLight.Enabled = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;

            Lights.BaseFogColor = GameEnvironment.Background = Color.Black;
            Lights.FogRange = 10f;
            Lights.FogStart = maxDistance - 15f;

            var desc = SceneLightPointDescription.Create(Vector3.Zero, 10f, 25f);

            torch = new SceneLightPoint("player_torch", true, agentTorchLight, agentTorchLight, true, desc);
            Lights.Add(torch);
        }
        private async Task InitializeDungeon()
        {
            var desc = await LoadOnePageDungeon(@"resources\maze_of_the_purple_god.json");

            scenery = await this.AddComponentModularScenery(desc, SceneObjectUsages.Ground);
            scenery.TriggerEnd += TriggerEnds;

            SetGround(scenery, true);
        }
        private async Task<ModularSceneryDescription> LoadOnePageDungeon(string fileName)
        {
            var dn = Engine.Content.OnePageDungeon.DungeonFile.Load(fileName);

            ModularSceneryObjectStateTransition toOpen = new ModularSceneryObjectStateTransition
            {
                State = "open",
            };
            ModularSceneryObjectStateTransition toClose = new ModularSceneryObjectStateTransition
            {
                State = "close",
            };

            ModularSceneryObjectState openState = new ModularSceneryObjectState
            {
                Name = "open",
                Transitions = new[] { toClose },
            };
            ModularSceneryObjectState closeState = new ModularSceneryObjectState
            {
                Name = "close",
                Transitions = new[] { toOpen },
            };

            ModularSceneryObjectAction openAction = new ModularSceneryObjectAction
            {
                Name = "open",
                StateFrom = "close",
                StateTo = "open",
                AnimationPlan = "open",
                Items = new[] { new ModularSceneryObjectActionItem { Action = "open" } },
            };

            ModularSceneryObjectAction closeAction = new ModularSceneryObjectAction
            {
                Name = "close",
                StateFrom = "open",
                StateTo = "close",
                AnimationPlan = "close",
                Items = new[] { new ModularSceneryObjectActionItem { Action = "close" } },
            };

            ModularSceneryObjectAnimationPlan openPlan = new ModularSceneryObjectAnimationPlan
            {
                Name = "open",
                Paths = new[] { new ModularSceneryObjectAnimationPath { Name = "open" } }
            };
            ModularSceneryObjectAnimationPlan closePlan = new ModularSceneryObjectAnimationPlan
            {
                Name = "close",
                Paths = new[] { new ModularSceneryObjectAnimationPath { Name = "close" } }
            };

            Dictionary<Engine.Content.OnePageDungeon.DoorTypes, string[]> doors = new Dictionary<Engine.Content.OnePageDungeon.DoorTypes, string[]>
            {
                { Engine.Content.OnePageDungeon.DoorTypes.Normal, new[] { "Dn_WoodenDoor_1", "Dn_Door_1" } },
                { Engine.Content.OnePageDungeon.DoorTypes.Archway, new[] { "Dn_WoodenDoor_1", "Dn_Door_1" } },
                { Engine.Content.OnePageDungeon.DoorTypes.Stairs, new[] { "Dn_WoodenDoor_1", "Dn_Door_1" } },
                { Engine.Content.OnePageDungeon.DoorTypes.Portcullis, new[] { "Dn_Jail_1", "Dn_Door_2" } },
                { Engine.Content.OnePageDungeon.DoorTypes.Special, new[] { "Dn_WoodenDoor_1", "Dn_Door_1" } },
                { Engine.Content.OnePageDungeon.DoorTypes.Secret, new[] { "Dn_Jail_1", "Dn_Door_2" } },
                { Engine.Content.OnePageDungeon.DoorTypes.Barred, new[] { "Dn_Jail_1", "Dn_Door_2" } }
            };

            var config = new Engine.Content.OnePageDungeon.DungeonAssetConfiguration()
            {
                PositionDelta = 2,

                Floors = new[] { "Dn_Floor_1" },
                Ceilings = new[] { "Dn_Ceiling_1" },
                Walls = new[] { "Dn_Wall_1", "Dn_Wall_1", "Dn_Wall_1", "Dn_Wall_2" },
                Columns = new[] { "Dn_Column_1", "Dn_Column_1", "Dn_Column_2", "Dn_Column_2", "Dn_Column_1" },

                Doors = doors,
                DoorAnimationPlans = new[] { openPlan, closePlan },
                DoorStates = new[] { closeState, openState },
                DoorActions = new[] { closeAction, openAction },

                RandomSeed = 1000,
            };

            var res = new ModularSceneryDescription()
            {
                Name = "Dungeon",
                UseAnisotropic = true,
                CastShadow = true,
                BlendMode = BlendModes.DefaultTransparent,
                ContentDescription = ContentDescription.FromFile("Resources/SceneModularDungeon", "assets.xml"),
                AssetsConfiguration = Engine.Content.OnePageDungeon.DungeonCreator.CreateAssets(dn, config),
                Levels = Engine.Content.OnePageDungeon.DungeonCreator.CreateLevels(dn, config),
            };

            return await Task.FromResult(res);
        }
        private async Task InitializePlayer()
        {
            playerAgentType = new Player()
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
                InitializeRat(),
                InitializeHuman());
        }
        private async Task InitializeRat()
        {
            rat = await this.AddComponentModel(
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

            ratAgentType = new Player()
            {
                Name = "Rat",
                Height = 0.2f,
                Radius = 0.1f,
                MaxClimb = 0.5f,
                MaxSlope = 50f,
                Velocity = 3f,
                VelocitySlow = 1f,
            };

            rat.Manipulator.SetScale(0.5f, true);
            rat.Manipulator.SetPosition(0, 0, 0, true);
            rat.Visible = false;

            var ratPaths = new Dictionary<string, AnimationPlan>();
            ratController = new BasicManipulatorController();

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");
            ratPaths.Add("walk", new AnimationPlan(p0));

            rat.AnimationController.AddPath(ratPaths["walk"]);
            rat.AnimationController.TimeDelta = 1.5f;
        }
        private async Task InitializeHuman()
        {
            human = await this.AddComponentModelInstanced(
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

            human.Visible = false;
        }
        private async Task InitializeDebug()
        {
            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Graph",
                Count = 50000,
            };
            graphDrawer = await this.AddComponentPrimitiveListDrawer(graphDrawerDesc);
            graphDrawer.Visible = false;

            var bboxesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Bounding volumes",
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.25f),
                Count = 10000,
            };
            bboxesDrawer = await this.AddComponentPrimitiveListDrawer(bboxesDrawerDesc);
            bboxesDrawer.Visible = false;

            var ratDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Rat",
                Color = new Color4(0.0f, 1.0f, 1.0f, 0.25f),
                Count = 10000,
            };
            ratDrawer = await this.AddComponentPrimitiveListDrawer(ratDrawerDesc);
            ratDrawer.Visible = false;

            var obstacleDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Obstacles",
                DepthEnabled = false,
                Count = 10000,
            };
            obstacleDrawer = await this.AddComponentPrimitiveListDrawer(obstacleDrawerDesc);
            obstacleDrawer.Visible = false;

            var connectionDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Connections",
                Color = connectionColor,
                Count = 10000,
            };
            connectionDrawer = await this.AddComponentPrimitiveListDrawer(connectionDrawerDesc);
            connectionDrawer.Visible = false;
        }
        private async Task InitializeAudio()
        {
            AudioManager.MasterVolume = 1;
            AudioManager.UseMasteringLimiter = true;
            AudioManager.SetMasteringLimit(15, 1500);

            //Sounds
            soundDoor = "door";
            soundLadder = "ladder";
            AudioManager.LoadSound(soundDoor, "Resources/SceneModularDungeon/Audio/Effects", "door.wav");
            AudioManager.LoadSound(soundLadder, "Resources/SceneModularDungeon/Audio/Effects", "ladder.wav");

            string soundWind1 = "wind1";
            string soundWind2 = "wind2";
            string soundWind3 = "wind3";
            AudioManager.LoadSound(soundWind1, "Resources/SceneModularDungeon/Audio/Effects", "Wind1_S.wav");
            AudioManager.LoadSound(soundWind2, "Resources/SceneModularDungeon/Audio/Effects", "Wind2_S.wav");
            AudioManager.LoadSound(soundWind3, "Resources/SceneModularDungeon/Audio/Effects", "Wind3_S.wav");
            soundWinds = new[] { soundWind1, soundWind2, soundWind3 };

            ratSoundMove = "mouseMove";
            ratSoundTalk = "mouseTalk";
            AudioManager.LoadSound(ratSoundMove, "Resources/SceneModularDungeon/Audio/Effects", "mouse1.wav");
            AudioManager.LoadSound(ratSoundTalk, "Resources/SceneModularDungeon/Audio/Effects", "mouse2.wav");

            soundTorch = "torch";
            AudioManager.LoadSound(soundTorch, "Resources/SceneModularDungeon/Audio/Effects", "loop_torch.wav");

            //Effects
            AudioManager.AddEffectParams(
                soundDoor,
                new GameAudioEffectParameters
                {
                    SoundName = soundDoor,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    UseAudio3D = true,
                    ReverbPreset = ReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            AudioManager.AddEffectParams(
                soundLadder,
                new GameAudioEffectParameters
                {
                    SoundName = soundLadder,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    UseAudio3D = true,
                    ReverbPreset = ReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            for (int i = 0; i < soundWinds.Length; i++)
            {
                AudioManager.AddEffectParams(
                    soundWinds[i],
                    new GameAudioEffectParameters
                    {
                        DestroyWhenFinished = true,
                        IsLooped = false,
                        SoundName = soundWinds[i],
                        Volume = 1f,
                        UseAudio3D = true,
                        ReverbPreset = ReverbPresets.StoneRoom,
                        EmitterRadius = 15,
                        ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                    });
            }

            AudioManager.AddEffectParams(
                ratSoundMove,
                new GameAudioEffectParameters
                {
                    SoundName = ratSoundMove,
                    DestroyWhenFinished = false,
                    Volume = 1f,
                    IsLooped = true,
                    UseAudio3D = true,
                    ReverbPreset = ReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            AudioManager.AddEffectParams(
                ratSoundTalk,
                new GameAudioEffectParameters
                {
                    SoundName = ratSoundTalk,
                    DestroyWhenFinished = true,
                    Volume = 1f,
                    IsLooped = false,
                    UseAudio3D = true,
                    ReverbPreset = ReverbPresets.StoneRoom,
                    EmitterRadius = 3,
                    ListenerCone = GameAudioConeDescription.DefaultListenerCone,
                });

            await Task.CompletedTask;
        }

        private void StartCamera()
        {
            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = maxDistance;
            Camera.MovementDelta = playerAgentType.Velocity;
            Camera.SlowMovementDelta = playerAgentType.VelocitySlow;
            Camera.Mode = CameraModes.Free;
            Camera.Position = cameraInitialPosition;
            Camera.Interest = cameraInitialInterest;
        }
        private void UpdateDebugInfo()
        {
            //Graph
            bboxesDrawer.Clear();

            //Boxes
            Random rndBoxes = new Random(1);

            var dict = scenery.GetMapVolumes();

            foreach (var item in dict.Values)
            {
                var color = rndBoxes.NextColor().ToColor4();
                color.Alpha = 0.40f;

                bboxesDrawer.SetPrimitives(color, Line3D.CreateWiredBox(item.ToArray()));
            }

            //Objects
            UpdateBoundingBoxes(scenery.GetObjectsByType(ModularSceneryObjectTypes.Entrance).Select(o => o.Item), Color.PaleVioletRed);
            UpdateBoundingBoxes(scenery.GetObjectsByType(ModularSceneryObjectTypes.Exit).Select(o => o.Item), Color.ForestGreen);
            UpdateBoundingBoxes(scenery.GetObjectsByType(ModularSceneryObjectTypes.Trigger).Select(o => o.Item), Color.Cyan);
            UpdateBoundingBoxes(scenery.GetObjectsByType(ModularSceneryObjectTypes.Door).Select(o => o.Item), Color.LightYellow);
            UpdateBoundingBoxes(scenery.GetObjectsByType(ModularSceneryObjectTypes.Light).Select(o => o.Item), Color.MediumPurple);
        }
        private void UpdateBoundingBoxes(IEnumerable<ModelInstance> items, Color color)
        {
            List<Line3D> lines = new List<Line3D>();

            foreach (var item in items)
            {
                var bbox = item.GetBoundingBox(true);

                lines.AddRange(Line3D.CreateWiredBox(bbox));
            }

            bboxesDrawer.SetPrimitives(color, lines);
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

                        RemoveObstacle(o.Index);
                        o.Index = AddObstacle(obb);
                        o.Obstacle = obb;
                    });

                    PaintObstacles();
                }
            }
        }

        private void UpdatePlayerInput()
        {
            var prevPos = Camera.Position;

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }

#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                Game.GameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Walk(playerAgentType, prevPos, Camera.Position, true, out Vector3 walkerPos))
            {
                Camera.Goto(walkerPos);
            }
            else
            {
                Camera.Goto(prevPos);
            }

            if (torch.Enabled)
            {
                torch.Position =
                    Camera.Position +
                    (Camera.Direction * 0.5f) +
                    (Camera.Left * 0.2f);
            }

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                torch.Enabled = !torch.Enabled;
            }
        }
        private void UpdateDebugInput()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                graphDrawer.Visible = !graphDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                bboxesDrawer.Visible = !bboxesDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                obstacleDrawer.Visible = !obstacleDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                ratDrawer.Visible = !ratDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F))
            {
                //Frustum
                var frustum = Line3D.CreateWiredFrustum(Camera.Frustum);

                bboxesDrawer.SetPrimitives(Color.White, frustum);
            }

            if (Game.Input.KeyJustReleased(Keys.Oem5))
            {
                console.Toggle();
            }
        }
        private void UpdateGraphInput()
        {
            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                //Refresh the navigation mesh
                RefreshNavigation();
            }

            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                //Save the navigation triangles to a file
                SaveGraphToFile();
            }

            if (Game.Input.KeyJustReleased(Keys.G))
            {
                currentGraph++;
                currentGraph %= 2;

                UpdateGraphDebug(CurrentAgent);
            }
        }
        private void UpdateRatInput()
        {
            if (Game.Input.KeyJustReleased(Keys.P))
            {
                rat.Visible = false;
                ratActive = false;
                ratController.Clear();
            }
        }
        private void UpdateEntitiesInput()
        {
            if (selectedItem == null)
            {
                return;
            }

            if (selectedItem.Object.Type == ModularSceneryObjectTypes.Exit)
            {
                UpdateEntityExit(selectedItem);
            }

            if (selectedItem.Object.Type == ModularSceneryObjectTypes.Trigger ||
                selectedItem.Object.Type == ModularSceneryObjectTypes.Door)
            {
                UpdateEntityTrigger(selectedItem);
            }

            if (selectedItem.Object.Type == ModularSceneryObjectTypes.Light)
            {
                UpdateEntityLight(selectedItem);
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
            if (selectedItem == null)
            {
                return;
            }

            if (selectedItemPainted && !selectedItem.Item.HasChanged)
            {
                return;
            }

            var tris = selectedItem.Item.GetTriangles();
            if (tris.Any())
            {
                Color4 sItemColor = Color.LightYellow;
                sItemColor.Alpha = 0.3333f;

                Logger.WriteDebug($"Processing {tris.Count()} triangles in the selected item drawer");

                selectedItemDrawer.SetPrimitives(sItemColor, tris);

                selectedItemPainted = true;
            }
        }
        private void UpdateRatController(GameTime gameTime)
        {
            ratTime -= gameTime.ElapsedSeconds;

            if (!ratHoles.Any())
            {
                return;
            }

            if (ratActive)
            {
                ratController.UpdateManipulator(gameTime, rat.Manipulator);
                if (!ratController.HasPath)
                {
                    ratActive = false;
                    ratTime = nextRatTime;
                    rat.Visible = false;
                    ratController.Clear();

                    ratSoundInstance?.Pause();
                    RatTalkPlay();
                }
            }

            if (!ratActive && ratTime <= 0)
            {
                var iFrom = Helper.RandomGenerator.Next(0, ratHoles.Length);
                var iTo = Helper.RandomGenerator.Next(0, ratHoles.Length);
                if (iFrom == iTo) return;

                var from = ratHoles[iFrom];
                var to = ratHoles[iTo];

                rat.Manipulator.SetPosition(from);

                if (CalcPath(ratAgentType, from, to))
                {
                    ratController.UpdateManipulator(gameTime, rat.Manipulator);

                    ratSoundInstance?.Play();
                    RatTalkPlay();
                }
            }

            if (rat.Visible && ratDrawer.Visible)
            {
                var bbox = rat.GetBoundingBox();

                ratDrawer.SetPrimitives(Color.White, Line3D.CreateWiredBox(bbox));
            }
        }
        private bool CalcPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var path = FindPath(agent, from, to);
            if (path?.Count > 0)
            {
                path.InsertControlPoint(0, from, Vector3.Up);
                path.AddControlPoint(to, Vector3.Up);

                ratDrawer.SetPrimitives(Color.Red, Line3D.CreateLineList(path.Positions));

                ratController.Follow(new NormalPath(path.Positions, path.Normals));
                ratController.MaximumSpeed = ratAgentType.Velocity;
                rat.Visible = true;
                rat.AnimationController.Start(0);

                ratActive = true;
                ratTime = nextRatTime;

                return true;
            }

            return false;
        }
        private void RatTalkPlay()
        {
            AudioManager.CreateEffectInstance(ratSoundTalk, rat, Camera)?.Play();
        }
        private void UpdateEntities()
        {
            var sphere = new BoundingSphere(Camera.Position, doorDistance);

            var objTypes =
                ModularSceneryObjectTypes.Entrance |
                ModularSceneryObjectTypes.Exit |
                ModularSceneryObjectTypes.Door |
                ModularSceneryObjectTypes.Trigger |
                ModularSceneryObjectTypes.Light;

            var ray = GetPickingRay();
            float minDist = 1.2f;

            //Test items into the camera frustum and nearest to the player
            var items =
                scenery.GetObjectsInVolume(sphere, objTypes, false, true)
                .Where(i => Camera.Frustum.Contains(i.Item.GetBoundingBox()) != ContainmentType.Disjoint)
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

                SetSelectedItem(items.First());
            }
            else
            {
                SetSelectedItem(null);
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
            if (item == selectedItem)
            {
                return;
            }

            selectedItem = item;
            selectedItemPainted = false;

            if (item == null)
            {
                selectedItemDrawer.Clear();

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
            else if (
                item.Object.Type == ModularSceneryObjectTypes.Door ||
                item.Object.Type == ModularSceneryObjectTypes.Trigger)
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
            var triggers = scenery.GetTriggersByObject(item);
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
                messages.Text = text;
                messages.CenterHorizontally = CenterTargets.Screen;
                messages.CenterVertically = CenterTargets.Screen;
                messages.Visible = true;
                messages.ClearTween();
                messages.Show(1000);
            }
            else
            {
                messages.ClearTween();
                messages.Hide(250);
            }
        }
        private void UpdateEntityExit(ModularSceneryItem item)
        {
            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                Task.Run(async () =>
                {
                    var effect = AudioManager.CreateEffectInstance(soundDoor, item.Item, Camera);
                    if (effect != null)
                    {
                        effect.Play();

                        await Task.Delay(effect.Duration);
                    }

                    string nextLevel = item.Object.NextLevel;
                    if (!string.IsNullOrEmpty(nextLevel))
                    {
                        ChangeToLevel(nextLevel);
                    }
                    else
                    {
                        Game.SetScene<SceneStart>();
                    }
                });
            }
        }
        private void UpdateEntityTrigger(ModularSceneryItem item)
        {
            var triggers = scenery
                .GetTriggersByObject(item)
                .ToArray();

            if (triggers.Any())
            {
                int keyIndex = ReadKeyIndex();
                if (keyIndex > 0 && keyIndex <= triggers.Length)
                {
                    AudioManager.CreateEffectInstance(soundLadder)?.Play();
                    scenery.ExecuteTrigger(item, triggers[keyIndex - 1]);
                }
            }
        }
        private void UpdateEntityLight(ModularSceneryItem item)
        {
            if (Game.Input.KeyJustReleased(Keys.Space))
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
                messages.CenterHorizontally = CenterTargets.Screen;
                messages.CenterVertically = CenterTargets.Screen;
            }
        }

        /// <summary>
        /// Reads the keyboard looking for the first numeric key pressed
        /// </summary>
        /// <returns>Returns the first just released numeric key value</returns>
        private int ReadKeyIndex()
        {
            if (Game.Input.KeyJustReleased(Keys.D0)) return 0;
            if (Game.Input.KeyJustReleased(Keys.D1)) return 1;
            if (Game.Input.KeyJustReleased(Keys.D2)) return 2;
            if (Game.Input.KeyJustReleased(Keys.D3)) return 3;
            if (Game.Input.KeyJustReleased(Keys.D4)) return 4;
            if (Game.Input.KeyJustReleased(Keys.D5)) return 5;
            if (Game.Input.KeyJustReleased(Keys.D6)) return 6;
            if (Game.Input.KeyJustReleased(Keys.D7)) return 7;
            if (Game.Input.KeyJustReleased(Keys.D8)) return 8;
            if (Game.Input.KeyJustReleased(Keys.D9)) return 9;

            return -1;
        }

        private void RefreshNavigation()
        {
            var fileName = scenery.CurrentLevel.Name + nmFile;

            //Refresh the navigation mesh
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            Task.WhenAll(UpdateNavigationGraph());
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
                        var fileName = scenery.CurrentLevel.Name + ntFile;

                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }

                        var loader = new LoaderObj();
                        var tris = GetTrianglesForNavigationGraph();
                        loader.Save(tris, fileName);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError($"SaveGraphToFile: {ex.Message}", ex);
                    }
                    finally
                    {
                        taskRunning = false;
                    }
                }
            });
        }

        private void ChangeToLevel(string name)
        {
            levelInitialized = false;
            _ = LoadResourcesAsync(ChangeToLevelAsync(name), (res) =>
            {
                if (!res.Completed)
                {
                    res.ThrowExceptions();
                }

                levelInitialized = true;
            });
        }
        private async Task ChangeToLevelAsync(string name)
        {
            gameReady = false;

            Camera.Position = cameraInitialPosition;
            Camera.Interest = cameraInitialInterest;

            Lights.ClearPointLights();
            Lights.ClearSpotLights();

            AudioManager.Stop();
            AudioManager.ClearEffects();

            ClearNPCs();
            ClearDebugDrawers();

            SetSelectedItem(null);

            if (string.IsNullOrWhiteSpace(name))
            {
                await scenery.LoadFirstLevel();
            }
            else
            {
                await scenery.LoadLevel(name);
            }

            ConfigureNavigationGraph();

            await UpdateNavigationGraph();

            StartEntities();

            var pos = scenery.CurrentLevel.StartPosition;
            var dir = scenery.CurrentLevel.LookingVector;
            pos.Y += playerAgentType.Height;
            Camera.Position = pos;
            Camera.Interest = pos + dir;

            Lights.Add(torch);

            AudioManager.Start();

            gameReady = true;
        }

        private void ConfigureNavigationGraph()
        {
            PathFinderDescription.Input.ClearConnections();

            if (scenery.CurrentLevel?.Name == "Lvl1")
            {
                PathFinderDescription.Input.AddConnection(
                    new Vector3(-8.98233700f, 4.76837158e-07f, 0.0375497341f),
                    new Vector3(-11.0952349f, -4.76837158e-07f, 0.00710105896f),
                    1,
                    1,
                    GraphConnectionAreaTypes.Jump,
                    GraphConnectionFlagTypes.All);

                PathFinderDescription.Input.AddConnection(
                    new Vector3(17, 0, -14),
                    new Vector3(16, 0, -15),
                    0.15f,
                    0,
                    GraphConnectionAreaTypes.Ground,
                    GraphConnectionFlagTypes.All);
            }

            PaintConnections();
        }
        private void StartEntities()
        {
            //Rat holes
            ratHoles = scenery
                .GetObjectsByName("Dn_Rat_Hole_1")
                .Select(o => o.Item.Manipulator.Position)
                .ToArray();

            //NPCs
            StartNPCs();

            //Obstacles
            StartEntitiesObstacles();

            //Sounds
            StartEntitiesAudio();
        }
        private void StartNPCs()
        {
            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("stand");

            if (scenery.CurrentLevel?.Name != "Lvl1")
            {
                human.Visible = false;
            }

            for (int i = 0; i < human.InstanceCount; i++)
            {
                human[i].Manipulator.SetPosition(31, 0, i == 0 ? -31 : -29, true);
                human[i].Manipulator.SetRotation(-MathUtil.PiOverTwo, 0, 0, true);

                human[i].AnimationController.AddPath(new AnimationPlan(p0));
                human[i].AnimationController.Start(i * 1f);
                human[i].AnimationController.TimeDelta = 0.5f + (i * 0.1f);
            }

            human.Visible = true;
        }
        private void StartEntitiesAudio()
        {
            //Rat sound
            ratSoundInstance = AudioManager.CreateEffectInstance(ratSoundMove, rat, Camera);

            //Torchs
            StartEntitiesAudioTorchs();

            //Big fires
            StartEntitiesAudioBigFires();
        }
        private void StartEntitiesAudioTorchs()
        {
            var torchs = scenery
                .GetObjectsByName("Dn_Torch")
                .Select(o => o.Item);

            int index = 0;
            foreach (var item in torchs)
            {
                string effectName = $"torch{index++}";

                AudioManager.AddEffectParams(
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

                AudioManager.CreateEffectInstance(effectName, item, Camera).Play();
            }
        }
        private void StartEntitiesAudioBigFires()
        {
            List<ModelInstance> fires = new List<ModelInstance>();
            fires.AddRange(scenery.GetObjectsByName("Dn_Temple_Fire_1").Select(o => o.Item));
            fires.AddRange(scenery.GetObjectsByName("Dn_Big_Lamp_1").Select(o => o.Item));

            int index = 0;
            foreach (var item in fires)
            {
                string effectName = $"bigFire{index++}";

                AudioManager.AddEffectParams(
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

                AudioManager.CreateEffectInstance(effectName, item, Camera).Play();
            }
        }
        private void StartEntitiesObstacles()
        {
            obstacles.Clear();

            //Object obstacles
            var sceneryObjects = scenery
                .GetObjectsByType(ModularSceneryObjectTypes.Furniture | ModularSceneryObjectTypes.Door)
                .Select(o => o.Item);

            foreach (var item in sceneryObjects)
            {
                var obb = OrientedBoundingBoxExtensions.FromPoints(item.GetPoints(), item.Manipulator.FinalTransform);

                int index = AddObstacle(obb);
                if (index >= 0)
                {
                    obstacles.Add(new ObstacleInfo { Index = index, Item = item, Obstacle = obb });
                }
            }

            //Human obstacles
            for (int i = 0; i < human.InstanceCount; i++)
            {
                var pos = human[i].Manipulator.Position;
                var bc = new BoundingCylinder(pos, 0.8f, 1.5f);

                int index = AddObstacle(bc);
                if (index >= 0)
                {
                    obstacles.Add(new ObstacleInfo { Index = index, Item = human[i], Obstacle = bc });
                }
            }

            PaintObstacles();
        }

        private void ClearNPCs()
        {
            ratActive = false;
            rat.Visible = false;
            ratController.Clear();

            human.Visible = false;
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
            obstacleDrawer.Clear(obstacleColor);

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
                    obstacleDrawer.AddPrimitives(obstacleColor, obstacleTris);
                }
            }
        }
        private void PaintConnections()
        {
            connectionDrawer.Clear(connectionColor);

            var conns = PathFinderDescription.Input.GetConnections();

            foreach (var conn in conns)
            {
                var arclines = Line3D.CreateArc(conn.Start, conn.End, 0.25f, 8);
                connectionDrawer.AddPrimitives(connectionColor, arclines);

                var cirlinesF = Line3D.CreateCircle(conn.Start, conn.Radius, 32);
                connectionDrawer.AddPrimitives(connectionColor, cirlinesF);

                if (conn.Direction == 1)
                {
                    var cirlinesT = Line3D.CreateCircle(conn.End, conn.Radius, 32);
                    connectionDrawer.AddPrimitives(connectionColor, cirlinesT);
                }

                connectionDrawer.Visible = true;
            }
        }

        private void UpdateGraphDebug(AgentType agent)
        {
            var nodes = BuildGraphNodeDebugAreas(agent);

            graphDrawer.Clear();
            graphDrawer.SetPrimitives(nodes);

            UpdateDebugInfo();
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

            var soundEffect = soundWinds[index];

            var windInstance = AudioManager.CreateEffectInstance(soundEffect, man, Camera);
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
