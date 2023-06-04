using Engine;
using Engine.Animation;
using Engine.Audio;
using Engine.BuiltIn.PostProcess;
using Engine.Common;
using Engine.Content;
using Engine.Content.FmtObj;
using Engine.Modular;
using Engine.Modular.Persistence;
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

namespace Collada.ModularDungeon
{
    public class SceneModularDungeon : WalkableScene
    {
        private const float maxDistance = 35;

        private readonly string resourcesFolder = "modulardungeon/resources";
        private readonly bool isOnePageDungeon;
        private readonly string dungeonDefFile;
        private readonly string dungeonMapFile;

        private Sprite dungeonMap = null;
        private UIProgressBar pbLevels = null;
        private UIDialog dialog = null;
        private UIConsole console = null;

        private readonly Color ambientUp = new Color(255, 224, 255, 255);
        private readonly Color ambientDown = new Color(0, 31, 0, 255);

        private Player playerAgentType = null;
        private readonly Color3 agentTorchLight = new Color(255, 249, 224).RGB();
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
        private Vector3[] ratHoles = Array.Empty<Vector3>();

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
        private IAudioEffect ratSoundInstance = null;

        private readonly BuiltInPostProcessState postProcessingState = BuiltInPostProcessState.Empty;

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

        public SceneModularDungeon(Game game, bool isOnePageDungeon, string dungeonDefFile, string dungeonMapFile)
            : base(game)
        {
            this.isOnePageDungeon = isOnePageDungeon;
            this.dungeonDefFile = dungeonDefFile;
            this.dungeonMapFile = dungeonMapFile;

            Logger.SetCustomFilter(l => { return l.CallerTypeName == nameof(SceneModularDungeon); });


#if DEBUG
            Game.VisibleMouse = true;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            postProcessingState.AddToneMapping(BuiltInToneMappingTones.SimpleReinhard);
            postProcessingState.AddBlurVignette();
            postProcessingState.AddBloomLow();
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            LoadUI();
        }
        public override void OnReportProgress(LoadResourceProgress value)
        {
            if (value.Id == "LoadAssets")
            {
                Logger.WriteDebug(this, $"Level progress {value.Progress * 100f:0}%");

                pbLevels.Caption.Text = $"{value.Progress * 100f:0}%";
                pbLevels.ProgressValue = value.Progress;

                return;
            }

            Logger.WriteDebug(this, $"Ignored {value.Progress * 100f:0}%");
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
                    string hash = await PathFinderDescription.GetHash();

                    var graph = await PathFinderDescription.Load(fileName, hash);
                    if (graph != null)
                    {
                        SetNavigationGraph(graph);

                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(this, $"Bad graph file. Generating navigation mesh. {ex.Message}", ex);
                }
            }

            await base.UpdateNavigationGraph();

            _ = Task.Run(async () =>
            {
                try
                {
                    Logger.WriteDebug(this, $"Saving graph file. {fileName}");

                    await PathFinderDescription.Save(fileName, NavigationGraph);
                }
                catch (Exception ex)
                {
                    Logger.WriteError(this, $"Error saving graph file. {ex.Message}", ex);
                }
            });
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
                Game.SetScene<Start.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);

                InitializePostProcessing();
            }

            if (!userInterfaceInitialized)
            {
                return;
            }

            if (!gameAssetsInitialized)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.M))
            {
                ToggleMap();
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

            UpdateDebugInput(gameTime);
            UpdateGraphInput();
            UpdateRatInput();
            UpdatePlayerInput();
            UpdateEntitiesInput();

            UpdatePlayerState(gameTime);
            UpdateSelection();
        }
        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }

        private void LoadUI()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeUI(),
                    InitializeMapTexture()
                },
                LoadUICompleted);
        }
        private async Task InitializeUI()
        {
            console = await AddComponentUI<UIConsole, UIConsoleDescription>("ui1", "Console", UIConsoleDescription.Default(), LayerUI + 1);
            console.Visible = false;

            pbLevels = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("ui2", "PbLevels", UIProgressBarDescription.Default(Color.Transparent, Color.Green));
            pbLevels.Visible = false;

            var messagesFont = TextDrawerDescription.FromFamily("Viner Hand ITC, Microsoft Sans Serif", 48);
            var messagesDesc = UITextAreaDescription.Default(messagesFont);
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("ui3", "Messages", messagesDesc, LayerUI + 1);
            messages.Text = null;
            messages.TextForeColor = Color.Red;
            messages.TextShadowColor = Color.DarkRed;
            messages.SetPosition(new Vector2(0, 0));
            messages.Visible = false;

            var dialogDesc = UIDialogDescription.Default(Game.Form.RenderWidth * 0.5f, Game.Form.RenderHeight * 0.5f);
            dialogDesc.DialogButtons = UIDialogButtons.Accept;
            dialog = await AddComponentUI<UIDialog, UIDialogDescription>("ui4", "Dialog", dialogDesc, LayerUI + 1);
            dialog.OnAcceptHandler += (s, e) =>
            {
                dialog.CloseDialog(async () =>
                {
                    dialog.Hide(100);

                    await Task.Delay(1000);

                    Game.SetScene<Start.SceneStart>();
                });
            };
            dialog.Visible = false;

            var drawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
                BlendMode = BlendModes.Opaque | BlendModes.Additive,
            };
            selectedItemDrawer = await AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("ui5", "SelectedItemsDrawer", drawerDesc);
            selectedItemDrawer.Visible = false;
        }
        private async Task InitializeMapTexture()
        {
            if (string.IsNullOrWhiteSpace(dungeonMapFile))
            {
                return;
            }

            string onePageResourcesFolder = Path.Combine(resourcesFolder, "onepagedungeons");

            dungeonMap = await AddComponentUI<Sprite, SpriteDescription>("map1", "DungeonMap", SpriteDescription.Default(Path.Combine(onePageResourcesFolder, dungeonMapFile)), LayerUI + 2);
            dungeonMap.Visible = false;
        }
        private void LoadUICompleted(LoadResourcesResult res)
        {
            try
            {
                if (!res.Completed)
                {
                    res.ThrowExceptions();
                }

                userInterfaceInitialized = true;

                UpdateLayout();

                LoadAssets();
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, ex);

                PrepareMessage(true, $"Error loading UI: {ex.Message}{Environment.NewLine}Press Esc to return to the start screen.");
            }
        }

        private void LoadAssets()
        {
            pbLevels.Visible = true;

            var tasks = new[]
            {
                InitializeDebug(),
                InitializeDungeon(),
                InitializePlayer(),
                InitializeNPCs(),
                InitializeAudio(),
            };
            var resourceGroup = LoadResourceGroup.FromTasks("LoadAssets", tasks);

            LoadResourcesAsync(resourceGroup, LoadAssetsCompleted);
        }
        private async Task InitializeDebug()
        {
            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
            };
            graphDrawer = await AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("db1", "DEBUG++ Graph", graphDrawerDesc);
            graphDrawer.Visible = false;

            var bboxesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Color = new Color4(1.0f, 0.0f, 0.0f, 0.25f),
                Count = 10000,
            };
            bboxesDrawer = await AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("db2", "DEBUG++ Bounding volumes", bboxesDrawerDesc);
            bboxesDrawer.Visible = false;

            var ratDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Color = new Color4(0.0f, 1.0f, 1.0f, 0.25f),
                Count = 10000,
            };
            ratDrawer = await AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("db3", "DEBUG++ Rat", ratDrawerDesc);
            ratDrawer.Visible = false;

            var obstacleDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                DepthEnabled = false,
                Count = 10000,
            };
            obstacleDrawer = await AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("db4", "DEBUG++ Obstacles", obstacleDrawerDesc);
            obstacleDrawer.Visible = false;

            var connectionDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Color = connectionColor,
                Count = 10000,
            };
            connectionDrawer = await AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("db5", "DEBUG++ Connections", connectionDrawerDesc);
            connectionDrawer.Visible = false;
        }
        private async Task InitializeDungeon()
        {
            ModularSceneryDescription desc;
            if (isOnePageDungeon)
            {
                string onePageResourcesFolder = Path.Combine(resourcesFolder, "onepagedungeons");

                desc = await LoadOnePageDungeon(Path.Combine(onePageResourcesFolder, dungeonDefFile));
            }
            else
            {
                desc = ModularSceneryDescription.FromFolder(Path.Combine(resourcesFolder, dungeonDefFile));
            }

            scenery = await AddComponentGround<ModularScenery, ModularSceneryDescription>("Scenery", "Scenery", desc);
            scenery.TriggerEnd += TriggerEnds;

            SetGround(scenery, true);
        }
        private async Task<ModularSceneryDescription> LoadOnePageDungeon(string fileName)
        {
            var dn = Engine.Content.OnePageDungeon.DungeonFile.Load(fileName);

            ObjectStateTransition toOpen = new ObjectStateTransition
            {
                State = "open",
            };
            ObjectStateTransition toClose = new ObjectStateTransition
            {
                State = "close",
            };

            ObjectState openState = new ObjectState
            {
                Name = "open",
                Transitions = new[] { toClose },
            };
            ObjectState closeState = new ObjectState
            {
                Name = "close",
                Transitions = new[] { toOpen },
            };

            ObjectAction openAction = new ObjectAction
            {
                Name = "open",
                StateFrom = "close",
                StateTo = "open",
                AnimationPlan = "open",
                Items = new[] { new ObjectActionItem { Action = "open" } },
            };

            ObjectAction closeAction = new ObjectAction
            {
                Name = "close",
                StateFrom = "open",
                StateTo = "close",
                AnimationPlan = "close",
                Items = new[] { new ObjectActionItem { Action = "close" } },
            };

            ObjectAnimationPlan openPlan = new ObjectAnimationPlan
            {
                Name = "open",
                Paths = new[] { new ObjectAnimationPath { Name = "open" } }
            };
            ObjectAnimationPlan closePlan = new ObjectAnimationPlan
            {
                Name = "close",
                Paths = new[] { new ObjectAnimationPath { Name = "close" } }
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
                UseAnisotropic = true,
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                Content = ContentDescription.FromFile(resourcesFolder, "basicdungeon/assets.json"),
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
            rat = await AddComponent<Model, ModelDescription>(
                "char1",
                "Rat",
                new ModelDescription()
                {
                    TextureIndex = 0,
                    CastShadow = ShadowCastingAlgorihtms.All,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(Path.Combine(resourcesFolder, "characters/rat"), "rat.json"),
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

            rat.AnimationController.Start(ratPaths["walk"]);
            rat.AnimationController.TimeDelta = 1.5f;
        }
        private async Task InitializeHuman()
        {
            human = await AddComponent<ModelInstanced, ModelInstancedDescription>(
                "char2",
                "Human Instanced",
                new ModelInstancedDescription()
                {
                    CastShadow = ShadowCastingAlgorihtms.All,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile(Path.Combine(resourcesFolder, "characters/human2"), "Human2.json"),
                });

            human.Visible = false;
        }
        private async Task InitializeAudio()
        {
            AudioManager.MasterVolume = 1;
            AudioManager.UseMasteringLimiter = true;
            AudioManager.SetMasteringLimit(15, 1500);

            //Sounds
            soundDoor = "door";
            soundLadder = "ladder";
            AudioManager.LoadSound(soundDoor, Path.Combine(resourcesFolder, "audio/effects"), "door.wav");
            AudioManager.LoadSound(soundLadder, Path.Combine(resourcesFolder, "audio/effects"), "ladder.wav");

            string soundWind1 = "wind1";
            string soundWind2 = "wind2";
            string soundWind3 = "wind3";
            AudioManager.LoadSound(soundWind1, Path.Combine(resourcesFolder, "audio/effects"), "Wind1_S.wav");
            AudioManager.LoadSound(soundWind2, Path.Combine(resourcesFolder, "audio/effects"), "Wind2_S.wav");
            AudioManager.LoadSound(soundWind3, Path.Combine(resourcesFolder, "audio/effects"), "Wind3_S.wav");
            soundWinds = new[] { soundWind1, soundWind2, soundWind3 };

            ratSoundMove = "mouseMove";
            ratSoundTalk = "mouseTalk";
            AudioManager.LoadSound(ratSoundMove, Path.Combine(resourcesFolder, "audio/effects"), "mouse1.wav");
            AudioManager.LoadSound(ratSoundTalk, Path.Combine(resourcesFolder, "audio/effects"), "mouse2.wav");

            soundTorch = "torch";
            AudioManager.LoadSound(soundTorch, Path.Combine(resourcesFolder, "audio/effects"), "loop_torch.wav");

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
        private void LoadAssetsCompleted(LoadResourcesResult res)
        {
            try
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
            }
            catch (AggregateException ex)
            {
                Logger.WriteError(this, ex);

                var exceptions = ex.Flatten().InnerExceptions
                    .Select(e => e.Message)
                    .ToArray();

                string msg = $"Error loading Assets: {ex.Message}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, exceptions)}";
                dialog.ShowDialog(msg, () => { dialog.Show(100); });
            }
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
            Lights.HemisphericLigth = new SceneLightHemispheric("hemi_light", ambientDown.RGB(), ambientUp.RGB(), true);
            Lights.KeyLight.Enabled = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;

            Lights.BaseFogColor = GameEnvironment.Background = Color.Black;
            Lights.FogRange = 10f;
            Lights.FogStart = maxDistance - 15f;

            var desc = SceneLightPointDescription.Create(Vector3.Zero, 10f, 0.5f);

            torch = new SceneLightPoint("player_torch", true, agentTorchLight, agentTorchLight, true, desc);
            Lights.Add(torch);
        }
        private void InitializePostProcessing()
        {
            Renderer.ClearPostProcessingEffects();
            Renderer.PostProcessingObjectsEffects = postProcessingState;
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

                bboxesDrawer.SetPrimitives(color, Line3D.CreateFromVertices(GeometryUtil.CreateBoxes(Topology.LineList, item.ToArray())));
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
            if (!items.Any())
            {
                return;
            }

            var boxes = items.Select(i => SharpDX.BoundingBox.FromPoints(i.GetPoints().ToArray()));
            bboxesDrawer.SetPrimitives(color, Line3D.CreateFromVertices(GeometryUtil.CreateBoxes(Topology.LineList, boxes)));
        }
        private void TriggerEnds(object sender, ModularSceneryTriggerEventArgs e)
        {
            if (!e.Items.Any())
            {
                return;
            }

            var obs = obstacles.Where(o => e.Items.Select(i => i.Item).Contains(o.Item));
            if (!obs.Any())
            {
                return;
            }

            //Refresh affected obstacles (if any)
            obs.ToList().ForEach(o =>
            {
                var obb = SharpDXExtensions.FromPoints(o.Item.GetPoints(true));

                RemoveObstacle(o.Index);
                o.Index = AddObstacle(obb);
                o.Obstacle = obb;
            });

            PaintObstacles();
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
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
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
        private void UpdateDebugInput(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                graphDrawer.Visible = !graphDrawer.Visible;
                connectionDrawer.Visible = graphDrawer.Visible;
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
                var frustum = Line3D.CreateFromVertices(GeometryUtil.CreateFrustum(Topology.LineList, Camera.Frustum));

                bboxesDrawer.SetPrimitives(Color.White, frustum);
            }

            if (Game.Input.KeyJustReleased(Keys.Oem5))
            {
                console.Toggle();
            }

            if (Game.Input.KeyPressed(Keys.Left))
            {
                postProcessingState.BloomIntensity = MathUtil.Clamp(postProcessingState.BloomIntensity - gameTime.ElapsedSeconds, 0, 100);
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                postProcessingState.BloomIntensity = MathUtil.Clamp(postProcessingState.BloomIntensity + gameTime.ElapsedSeconds, 0, 100);
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

        private void UpdatePlayerState(GameTime gameTime)
        {
            postProcessingState.VignetteInner = 0.66f + ((float)Math.Sin(gameTime.TotalSeconds * 2f) * 0.1f);
        }
        private void UpdateSelection()
        {
            if (selectedItem == null)
            {
                return;
            }

            var tris = selectedItem.Item.GetTriangles();
            if (tris.Any())
            {
                Color4 sItemColor = Color.LightYellow;
                sItemColor.Alpha = 0.3333f;

                Logger.WriteDebug(this, $"Processing {tris.Count()} triangles in the selected item drawer");

                selectedItemDrawer.SetPrimitives(sItemColor, tris);
                selectedItemDrawer.Visible = true;
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

                ratDrawer.SetPrimitives(Color.White, Line3D.CreateFromVertices(GeometryUtil.CreateBox(Topology.LineList, bbox)));
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
                        Ray rRay = ray;
                        return sBbox.Intersects(ref rRay);
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

                SetSelectedItem(items[0]);
            }
            else
            {
                SetSelectedItem(null);
            }
        }
        private static float CalcItemPickingDistance(PickingRay ray, ModularSceneryItem item)
        {
            if (item.Item.PickNearest(ray, out var res))
            {
                return res.Distance;
            }
            else
            {
                var sph = item.Item.GetBoundingSphere();
                Intersection.ClosestPointInRay(ray, sph.Center, out float distance);
                return distance;
            }
        }

        private void SetSelectedItem(ModularSceneryItem item)
        {
            if (item == selectedItem)
            {
                return;
            }

            selectedItem = item;

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
            var triggers = scenery
                .GetTriggersByObject(item)
                .Where(t => t.Actions.Any())
                .ToArray();

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
                messages.Anchor = Anchors.Center;
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
                        Game.SetScene<Start.SceneStart>();
                    }
                });
            }
        }
        private void UpdateEntityTrigger(ModularSceneryItem item)
        {
            var triggers = scenery
                .GetTriggersByObject(item)
                .Where(t => t.Actions.Any())
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
                messages.Anchor = Anchors.Center;
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

        private void ToggleMap()
        {
            if (dungeonMap == null)
            {
                return;
            }

            if (dungeonMap.Visible)
            {
                dungeonMap.Hide(1000);
            }
            else
            {
                dungeonMap.Show(1000);
            }
        }

        private void RefreshNavigation()
        {
            var fileName = scenery.CurrentLevel.Name + nmFile;

            //Refresh the navigation mesh
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            _ = UpdateNavigationGraph();
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

                        var tris = GetTrianglesForNavigationGraph();
                        LoaderObj.Save(tris, fileName);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteError(this, $"SaveGraphToFile: {ex.Message}", ex);
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

            var resourceGroup = LoadResourceGroup.FromTasks("LoadAssets", ChangeToLevelAsync(name));

            LoadResourcesAsync(resourceGroup, ChangeToLevelCompleted);
        }
        private async Task ChangeToLevelAsync(string name)
        {
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
        }
        private void ChangeToLevelCompleted(LoadResourcesResult res)
        {
            try
            {
                if (!res.Completed)
                {
                    res.ThrowExceptions();
                }

                StartEntities();

                Vector3 pos = scenery.CurrentLevel.StartPosition;
                Vector3 dir = scenery.CurrentLevel.LookingVector;
                pos.Y += playerAgentType.Height;
                Camera.Position = pos;
                Camera.Interest = pos + dir;

                Lights.Add(torch);

                AudioManager.Start();

                InitializePostProcessing();

                levelInitialized = true;

                pbLevels.Hide(1000);

                gameReady = true;
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, ex);

                PrepareMessage(true, $"Error loading level: {ex.Message}{Environment.NewLine}Press Esc to return to the start screen.");
            }
        }

        private void ConfigureNavigationGraph()
        {
            PathFinderDescription.Input.ClearConnections();

            if (scenery.CurrentLevel.Name == "Lvl1")
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

            if (scenery.CurrentLevel.Name != "Lvl1")
            {
                human.Visible = false;
            }

            for (int i = 0; i < human.InstanceCount; i++)
            {
                human[i].Manipulator.SetPosition(31, 0, i == 0 ? -31 : -29, true);
                human[i].Manipulator.SetRotation(-MathUtil.PiOverTwo, 0, 0, true);

                human[i].AnimationController.Start(new AnimationPlan(p0), i * 1f);
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
                var obb = SharpDXExtensions.FromPoints(item.GetPoints());

                int index = AddObstacle(obb);
                if (index >= 0)
                {
                    obstacles.Add(new ObstacleInfo { Index = index, Item = item, Obstacle = obb });
                }
            }

            //Human obstacles
            for (int i = 0; i < human.InstanceCount; i++)
            {
                float h = 1.5f;
                float r = 0.8f;
                var center = human[i].Manipulator.Position + (Vector3.Up * h * 0.5f);
                var bc = new BoundingCylinder(center, r, h);

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

            foreach (var obstacle in obstacles.Select(o => o.Obstacle))
            {
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

            var nodes = GetNodes(agent).OfType<GraphNode>();
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

        private void UpdateLayout()
        {
            console.Top = 0;
            console.Left = 0;
            console.Width = Game.Form.RenderWidth;

            pbLevels.Top = Game.Form.RenderHeight - 50;
            pbLevels.Left = 100;
            pbLevels.Width = Game.Form.RenderWidth - 200;
            pbLevels.Height = 20;

            if (dungeonMap != null)
            {
                dungeonMap.Height = Game.Form.RenderHeight * 0.9f;
                dungeonMap.Width = dungeonMap.Height * 1.5f;
                dungeonMap.Top = (Game.Form.RenderHeight - dungeonMap.Height) * 0.5f;
                dungeonMap.Left = (Game.Form.RenderWidth - dungeonMap.Width) * 0.5f;
            }
        }
    }
}
