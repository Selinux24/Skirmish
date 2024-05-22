using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneModularDungeon
{
    using Engine;
    using Engine.Animation;
    using Engine.BuiltIn.PostProcess;
    using Engine.Common;
    using Engine.Content;
    using Engine.Content.FmtObj;
    using Engine.Content.OnePageDungeon;
    using Engine.Content.Persistence;
    using Engine.Modular;
    using Engine.PathFinding;
    using Engine.PathFinding.RecastNavigation;
    using Engine.Tween;
    using Engine.UI;
    using Engine.UI.Tween;

    public class ModularDungeonScene : WalkableScene
    {
        private const float maxDistance = 35;

        private const string resourcesFolder = "SceneModularDungeon/resources";
        private const string resourceAudio = "audio/effects";

        private readonly bool isOnePageDungeon;
        private readonly string dungeonDefFile;
        private readonly string dungeonMapFile;
        private readonly string dungeonCnfFile;

        private UIControlTweener uiTweener;
        private SoundEffectsManager soundEffectsManager;
        private DebugDrawers debugDrawers;

        private Sprite dungeonMap = null;
        private UIProgressBar pbLevels = null;
        private UIDialog dialog = null;
        private UIConsole console = null;

        private readonly Color ambientUp = new(255, 224, 255, 255);
        private readonly Color ambientDown = new(72, 72, 255, 255);

        private Player playerAgentType = null;
        private readonly Color3 agentTorchLight = new Color(255, 249, 224).RGB();
        private readonly Vector3 cameraInitialPosition = new(1000, 1000, 1000);
        private readonly Vector3 cameraInitialInterest = new(1001, 1000, 1000);

        private SceneLightPoint torch = null;
        private readonly List<LightController> lightControllers = [];

        private ModularScenery scenery = null;

        private readonly float doorDistance = 3f;
        private UITextArea messages = null;

        private Model rat = null;
        private BasicManipulatorController ratController = null;
        private Player ratAgentType = null;
        private bool ratActive = false;
        private float ratTime = 5f;
        private readonly float nextRatTime = 3f;
        private Vector3[] ratHoles = [];

        private PrimitiveListDrawer<Triangle> selectedItemDrawer = null;
        private Item selectedItem = null;

        private ModelInstanced human = null;

        private readonly string nmFile = "nm.graph";
        private readonly string ntFile = "nm.obj";
        private bool taskRunning = false;

        private readonly List<ObstacleInfo> obstacles = [];

        private Vector3 windPosition = new(60, 0, -20);
        private bool windCreated = false;

        private readonly BuiltInPostProcessState postProcessingState = BuiltInPostProcessState.Empty;

        enum GameStates
        {
            None,
            Player,
            Map,
        }

        private bool userInterfaceInitialized = false;
        private bool gameAssetsInitialized = false;
        private bool levelInitialized = false;
        private bool gameReady = false;
        private GameStates gameState = GameStates.None;

        public ModularDungeonScene(Game game, bool isOnePageDungeon, string dungeonDefFile, string dungeonMapFile, string dungeonCnfFile)
            : base(game)
        {
            this.isOnePageDungeon = isOnePageDungeon;
            this.dungeonDefFile = dungeonDefFile;
            this.dungeonMapFile = dungeonMapFile;
            this.dungeonCnfFile = dungeonCnfFile;

            Logger.SetCustomFilter(l => { return l.CallerTypeName == nameof(ModularDungeonScene); });


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

        private string GetCurrentLeveName()
        {
            return scenery?.CurrentLevel.Name + nmFile;
        }

        public override void Initialize()
        {
            base.Initialize();

            LoadUI();
        }
        public void OnReportProgress(LoadResourceProgress value)
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
        public void NavigationGraphLoading()
        {
            if (scenery?.CurrentLevel == null)
            {
                return;
            }

            var fileName = GetCurrentLeveName();

            LoadNavigationGraphFromFile(fileName);
        }
        public void NavigationGraphLoaded(bool loaded)
        {
            if (!loaded)
            {
                return;
            }

            if (!gameAssetsInitialized)
            {
                return;
            }

            if (scenery?.CurrentLevel != null)
            {
                var fileName = GetCurrentLeveName();

                SaveNavigationGraphToFile(fileName);
            }

            ChangeToLevelCompleted();
        }
        public void NavigationGraphUpdated()
        {
            //Update active paths with the new graph configuration
            if (ratController.HasPath)
            {
                var from = rat.Manipulator.Position;
                var to = ratController.Last;

                CalcPath(ratAgentType, from, to);
            }

            debugDrawers.DrawGraph();
            debugDrawers.DrawScenery(scenery);
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (gameState == GameStates.None)
            {
                UpdateSceneInput();

                return;
            }

            if (!userInterfaceInitialized)
            {
                return;
            }

            if (!gameAssetsInitialized)
            {
                return;
            }

            if (!levelInitialized)
            {
                return;
            }

            if (!gameReady)
            {
                return;
            }

            NavigationGraph?.UpdateObstacles((state) =>
            {
                if (state == GraphUpdateStates.Updated)
                {
                    debugDrawers.DrawGraph();
                    debugDrawers.DrawObstacles(obstacles);
                }
            });

            if (gameState == GameStates.Player)
            {
                UpdateStatePlayer(gameTime);

                UpdateRatController(gameTime);
                UpdateEntities();
                UpdateWind();
                UpdatePlayerState(gameTime);
                UpdateSelection();
            }
            else if (gameState == GameStates.Map)
            {
                UpdateStateMap();
            }

            lightControllers.ForEach(l => l.Update(gameTime));
        }
        private void UpdateStatePlayer(IGameTime gameTime)
        {
            UpdateSceneInput();

            if (Game.Input.KeyJustReleased(Keys.M))
            {
                ToggleMap();
            }

            UpdateDebugInput(gameTime);
            UpdateGraphInput();
            UpdateRatInput();
            UpdatePlayerInput();
            UpdateEntitiesInput();
        }
        private void UpdateStateMap()
        {
            if (Game.Input.KeyJustReleased(Keys.M) || Game.Input.KeyJustReleased(Keys.Escape))
            {
                ToggleMap();
            }
        }
        private void UpdateSceneInput()
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);

                InitializePostProcessing();
            }
        }
        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }

        private void LoadUI()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTweener,
                    InitializeUI,
                    InitializeMapTexture,
                ],
                LoadUICompleted,
                OnReportProgress);

            LoadResources(group);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeUI()
        {
            var consoleDesc = UIConsoleDescription.Default();
            consoleDesc.StartsVisible = false;
            consoleDesc.LogFilterFunc = (l) => l.LogLevel == LogLevel.Debug;
            console = await AddComponentUI<UIConsole, UIConsoleDescription>("ui1", "Console", consoleDesc, LayerUI + 1);

            var pvLevelsDesc = UIProgressBarDescription.Default(Color.Transparent, Color.Green);
            pvLevelsDesc.StartsVisible = false;
            pbLevels = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("ui2", "PbLevels", pvLevelsDesc);

            var messagesFont = TextDrawerDescription.FromFamily("Viner Hand ITC, Microsoft Sans Serif", 48);
            var messagesDesc = UITextAreaDescription.Default(messagesFont);
            messagesDesc.StartsVisible = false;
            messages = await AddComponentUI<UITextArea, UITextAreaDescription>("ui3", "Messages", messagesDesc, LayerUI + 1);
            messages.Text = null;
            messages.TextForeColor = Color.Red;
            messages.TextShadowColor = Color.DarkRed;
            messages.SetPosition(new Vector2(0, 0));

            var dialogDesc = UIDialogDescription.Default(Game.Form.RenderWidth * 0.5f, Game.Form.RenderHeight * 0.5f);
            dialogDesc.DialogButtons = UIDialogButtons.Accept;
            dialogDesc.StartsVisible = false;
            dialog = await AddComponentUI<UIDialog, UIDialogDescription>("ui4", "Dialog", dialogDesc, LayerUI + 1);
            dialog.OnAcceptHandler += (s, e) =>
            {
                dialog.CloseDialog(async () =>
                {
                    uiTweener.Hide(dialog, 100);

                    await Task.Delay(1000);

                    Game.SetScene<SceneStart.StartScene>();
                });
            };

            var drawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
                BlendMode = BlendModes.Opaque | BlendModes.Additive,
                StartsVisible = false,
            };
            selectedItemDrawer = await AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("ui5", "SelectedItemsDrawer", drawerDesc);
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

            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeDebug,
                    InitializeDungeon,
                    InitializeNPCs,
                    InitializeAudio,
                ],
                LoadAssetsCompleted,
                OnReportProgress,
                "LoadAssets");

            LoadResources(group);
        }
        private async Task InitializeDebug()
        {
            debugDrawers = await AddComponent<DebugDrawers>("debugDrawers", "debugDrawers");

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

            await debugDrawers.Initialize([playerAgentType, ratAgentType]);
        }
        private async Task InitializeDungeon()
        {
            ModularSceneryDescription desc;
            if (isOnePageDungeon)
            {
                string onePageResourcesFolder = Path.Combine(resourcesFolder, "onepagedungeons");

                desc = await LoadOnePageDungeon(Path.Combine(onePageResourcesFolder, dungeonDefFile), dungeonCnfFile);
            }
            else
            {
                string contentFolder = Path.Combine(resourcesFolder, dungeonDefFile);
                const string contentFile = "assets.json";
                const string assetMapFile = "assetsmap.json";
                const string levelMapFile = "levels.json";

                desc = ModularSceneryDescription.FromFolder(contentFolder, contentFile, assetMapFile, levelMapFile);
            }

            scenery = await AddComponentGround<ModularScenery, ModularSceneryDescription>("Scenery", "Scenery", desc);
            scenery.TriggerEnd += TriggerEnds;
        }
        private static async Task<ModularSceneryDescription> LoadOnePageDungeon(string dungeonFileName, string dungeonConfigFile)
        {
            var config = DungeonAssetConfiguration.Load(Path.Combine(resourcesFolder, dungeonConfigFile));

            List<ContentData> contentData = [.. await ReadAssetFiles(config.AssetFiles), .. await ReadAssets(config.Assets)];

            var content = contentData.Select(c => new ContentDescription { ContentData = c });

            var dn = Dungeon.Load(dungeonFileName);
            var assetsMap = DungeonCreator.CreateAssets(dn, config);
            var levelsMap = DungeonCreator.CreateLevels(dn, config);

            return new ModularSceneryDescription()
            {
                UseAnisotropic = true,
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.OpaqueTransparent,
                ContentList = content,
                AssetsConfiguration = assetsMap,
                Levels = levelsMap,
            };
        }
        private static async Task<IEnumerable<ContentData>> ReadAssetFiles(IEnumerable<string> assets)
        {
            if (assets?.Any() != true)
            {
                return [];
            }

            var contentData = await Task.WhenAll(assets.Select(a => ContentDataFile.ReadContentData(resourcesFolder, a)));

            return contentData.SelectMany(c => c);
        }
        private static async Task<IEnumerable<ContentData>> ReadAssets(IEnumerable<ContentDataFile> assets)
        {
            if (assets?.Any() != true)
            {
                return [];
            }

            var contentData = await Task.WhenAll(assets.Select(a => ContentDataFile.ReadContentData(resourcesFolder, a)));

            return contentData.SelectMany(c => c);
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

            rat.Manipulator.SetScaling(0.5f);
            rat.Manipulator.SetPosition(0, 0, 0);
            rat.Visible = false;

            var ratPaths = new Dictionary<string, AnimationPlan>();
            ratController = new BasicManipulatorController();

            AnimationPath p0 = new();
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
            soundEffectsManager = await AddComponent<SoundEffectsManager>("audioManager", "audioManager");
            soundEffectsManager.InitializeAudio(Path.Combine(resourcesFolder, resourceAudio));
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

                soundEffectsManager.Start(0.5f);

                ChangeToLevel(null);
            }
            catch (AggregateException ex)
            {
                Logger.WriteError(this, ex);

                var exceptions = ex.Flatten().InnerExceptions
                    .Select(e => e.Message)
                    .ToArray();

                string msg = $"Error loading Assets: {ex.Message}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, exceptions)}";
                dialog.ShowDialog(msg, () => { uiTweener.Show(dialog, 100); });
            }
        }
        private void InitializeEnvironment()
        {
            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = 0.2f;
            nmsettings.CellHeight = 0.2f;

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Layers;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 16;
            nmsettings.UseTileCache = true;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new(nmsettings, nminput, [playerAgentType, ratAgentType]);
        }
        private void InitializeLights()
        {
            Lights.HemisphericLigth = new SceneLightHemispheric("hemi_light", ambientDown.RGB(), ambientUp.RGB(), true);
            Lights.KeyLight.Enabled = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;

            Lights.FogColor = GameEnvironment.Background = Color.Black;
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
            Camera.SetPosition(cameraInitialPosition);
            Camera.SetInterest(cameraInitialInterest);
        }
        private void TriggerEnds(object sender, TriggerEventArgs e)
        {
            if (!e.Items.Any())
            {
                return;
            }

            foreach (var item in e.Items)
            {
                var obs = obstacles.FindAll(o => o.Item == item.Instance);
                if (obs.Count <= 0)
                {
                    continue;
                }

                //Refresh affected obstacles (if any)
                foreach (var o in obs)
                {
                    RemoveObstacle(o.Index);

                    if (item.CurrentState == "close")
                    {
                        var obb = o.Item.GetOrientedBoundingBox();
                        o.Index = AddObstacle(obb);
                        o.Obstacle = obb;
                    }
                }
            }
        }

        private void UpdatePlayerInput()
        {
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

            if (Walk(playerAgentType, Camera.Position, Camera.GetNextPosition(), true, out var walkerPos))
            {
                Camera.Goto(walkerPos);
            }
            else
            {
                Camera.Goto(Camera.Position);
            }

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                torch.Enabled = !torch.Enabled;
            }
        }
        private void UpdateDebugInput(IGameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                debugDrawers.ToggleConnections();
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                debugDrawers.ToggleBoundingBoxes();
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                debugDrawers.ToggleObstacles();
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                debugDrawers.ToggleRat();
            }

            if (Game.Input.KeyJustReleased(Keys.F))
            {
                debugDrawers.DrawCamera(Camera);
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

            if (Game.Input.KeyJustReleased(Keys.F9))
            {
                debugDrawers.SetNextAgentIndex();
                debugDrawers.DrawGraph();
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

            if (selectedItem.Object.Type == ObjectTypes.Exit)
            {
                UpdateEntityExit(selectedItem);
            }

            if (selectedItem.Object.Type == ObjectTypes.Trigger ||
                selectedItem.Object.Type == ObjectTypes.Door)
            {
                UpdateEntityTrigger(selectedItem);
            }

            if (selectedItem.Object.Type == ObjectTypes.Light)
            {
                UpdateEntityLight(selectedItem);
            }
        }

        private void UpdateWind()
        {
            if (!windCreated)
            {
                CreateWind();

                windCreated = true;
            }
        }

        private void UpdatePlayerState(IGameTime gameTime)
        {
            postProcessingState.VignetteInner = 0.66f + (MathF.Sin(gameTime.TotalSeconds * 2f) * 0.1f);
        }
        private void UpdateSelection()
        {
            if (selectedItem == null)
            {
                return;
            }

            var tris = selectedItem.Instance.GetGeometry();
            if (tris.Any())
            {
                Color4 sItemColor = Color.LightYellow;
                sItemColor.Alpha = 0.3333f;

                Logger.WriteDebug(this, $"Processing {tris.Count()} triangles in the selected item drawer");

                selectedItemDrawer.SetPrimitives(sItemColor, tris);
                selectedItemDrawer.Visible = true;
            }
        }
        private void UpdateRatController(IGameTime gameTime)
        {
            ratTime -= gameTime.ElapsedSeconds;

            if (ratHoles.Length == 0)
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

                    soundEffectsManager.StopRatMove();
                    soundEffectsManager.PlayRatTalk(rat);
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

                    soundEffectsManager.PlayRatMove();
                    soundEffectsManager.PlayRatTalk(rat);
                }
            }

            if (rat.Visible)
            {
                debugDrawers.DrawModel(rat);
            }
        }
        private bool CalcPath(AgentType agent, Vector3 from, Vector3 to)
        {
            var path = FindPath(agent, from, to);
            if (!(path?.Count > 0))
            {
                return false;
            }

            path.InsertControlPoint(0, from, Vector3.Up);
            path.AddControlPoint(to, Vector3.Up);

            ratController.Follow(new NormalPath(path.Positions, path.Normals));
            ratController.MaximumSpeed = ratAgentType.Velocity;
            rat.Visible = true;
            rat.AnimationController.Start(0);

            ratActive = true;
            ratTime = nextRatTime;

            debugDrawers.DrawPath(path);

            return true;
        }
        private void UpdateEntities()
        {
            var sphere = new BoundingSphere(Camera.Position, doorDistance);

            var objTypes =
                ObjectTypes.Entrance |
                ObjectTypes.Exit |
                ObjectTypes.Door |
                ObjectTypes.Trigger |
                ObjectTypes.Light;

            var ray = GetPickingRay();
            float minDist = 1.2f;

            //Test items into the camera frustum and nearest to the player
            var items =
                scenery.GetObjectsInVolume(sphere, objTypes, false, true)
                .Where(i => Camera.Frustum.Contains(i.Instance.GetBoundingBox()) != ContainmentType.Disjoint)
                .Where(i =>
                {
                    if (i.Instance.PickNearest(ray, out var res))
                    {
                        return true;
                    }
                    else
                    {
                        var bbox = i.Instance.GetBoundingBox();
                        var center = bbox.GetCenter();
                        var extents = bbox.GetExtents();
                        extents *= minDist;

                        var sBbox = new BoundingBox(center - extents, center + extents);
                        Ray rRay = ray;
                        return sBbox.Intersects(ref rRay);
                    }
                })
                .ToList();

            if (items.Count != 0)
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
        private static float CalcItemPickingDistance(PickingRay ray, Item item)
        {
            if (item.Instance.PickNearest(ray, out var res))
            {
                return res.Distance;
            }
            else
            {
                var sph = item.Instance.GetBoundingSphere();
                Intersection.ClosestPointInRay(ray, sph.Center, out float distance);
                return distance;
            }
        }

        private void SetSelectedItem(Item item)
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

            if (item.Object.Type == ObjectTypes.Entrance)
            {
                var msg = "The door locked when you closed it.\r\nYou must find an exit...";

                PrepareMessage(true, msg);
            }
            else if (item.Object.Type == ObjectTypes.Exit)
            {
                var msg = "Press space to exit...";

                PrepareMessage(true, msg);
            }
            else if (
                item.Object.Type == ObjectTypes.Door ||
                item.Object.Type == ObjectTypes.Trigger)
            {
                SetSelectedItemTrigger(item);
            }
            else if (item.Object.Type == ObjectTypes.Light)
            {
                SetSelectedItemLight(item);
            }
        }
        private void SetSelectedItemTrigger(Item item)
        {
            var triggers = scenery
                .GetTriggersByObject(item)
                .Where(t => t.Actions.Any())
                .ToArray();

            if (triggers.Length != 0)
            {
                int index = 1;
                var msg = string.Join(", ", triggers.Select(t => $"Press {index++} to {t.Name} the {item.Object.Name}"));

                PrepareMessage(true, msg);
            }
        }
        private void SetSelectedItemLight(Item item)
        {
            var lights = item.Instance.Lights;
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
                uiTweener.ClearTween(messages);
                uiTweener.Show(messages, 1000);
            }
            else
            {
                uiTweener.ClearTween(messages);
                uiTweener.Hide(messages, 250);
            }
        }
        private void UpdateEntityExit(Item item)
        {
            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                Task.Run(async () =>
                {
                    var duration = soundEffectsManager.PlayDoor(item.Instance);
                    await Task.Delay(duration);

                    string nextLevel = item.Object.NextLevel;
                    if (!string.IsNullOrEmpty(nextLevel))
                    {
                        ChangeToLevel(nextLevel);
                    }
                    else
                    {
                        Game.SetScene<SceneStart.StartScene>();
                    }
                }).ConfigureAwait(false);
            }
        }
        private void UpdateEntityTrigger(Item item)
        {
            var triggers = scenery
                .GetTriggersByObject(item)
                .Where(t => t.Actions.Any())
                .ToArray();

            if (triggers.Length == 0)
            {
                return;
            }

            int keyIndex = ReadKeyIndex();
            if (keyIndex > 0 && keyIndex <= triggers.Length)
            {
                soundEffectsManager.PlayLadder(item.Instance);
                scenery.ExecuteTrigger(item, triggers[keyIndex - 1]);
            }
        }
        private void UpdateEntityLight(Item item)
        {
            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                bool enabled = false;

                var lights = item.Instance.Lights;
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
                uiTweener.Hide(dungeonMap, 1000);

                gameState = GameStates.Player;
            }
            else
            {
                uiTweener.Show(dungeonMap, 1000);

                gameState = GameStates.Map;
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

            NavigationGraphLoading();

            EnqueueNavigationGraphUpdate(NavigationGraphLoaded);
        }
        private void SaveGraphToFile()
        {
            Task.Run(() =>
            {
                if (taskRunning)
                {
                    return;
                }

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
            });
        }

        private void ChangeToLevel(string name)
        {
            levelInitialized = false;

            gameReady = false;

            Camera.SetPosition(cameraInitialPosition);
            Camera.SetInterest(cameraInitialInterest);

            ClearLevelLights();

            soundEffectsManager.Stop();

            ClearNPCs();

            debugDrawers.Clear();

            SetSelectedItem(null);

            var group = LoadResourceGroup.FromTasks(
                () => ChangeToLevelAsync(name),
                ChangeToLevelResults,
                OnReportProgress,
                "LoadAssets");

            LoadResources(group);
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

            EnqueueNavigationGraphUpdate(NavigationGraphLoaded);
        }
        private void ChangeToLevelResults(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                var ex = res.Flatten();

                Logger.WriteError(this, ex);

                PrepareMessage(true, $"Error loading level: {ex.Message}{Environment.NewLine}Press Esc to return to the start screen.");
            }
        }
        private void ChangeToLevelCompleted()
        {
            try
            {
                StartEntities();

                Vector3 pos = scenery.CurrentLevel.StartPosition;
                Vector3 dir = scenery.CurrentLevel.LookingVector;
                pos.Y += playerAgentType.Height;
                Camera.SetPosition(pos);
                Camera.SetInterest(pos + dir);

                InitializeLevelLights();

                soundEffectsManager.Start(0.5f);

                InitializePostProcessing();

                levelInitialized = true;

                uiTweener.Hide(pbLevels, 1000);

                gameReady = true;

                gameState = GameStates.Player;
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, ex);

                PrepareMessage(true, $"Error loading level: {ex.Message}{Environment.NewLine}Press Esc to return to the start screen.");
            }
        }

        private void ConfigureNavigationGraph()
        {
            PathFinderDescription.ClearConnections();

            if (scenery.CurrentLevel.Name == "Lvl1")
            {
                PathFinderDescription.AddConnection(
                    new Vector3(-8.98233700f, 4.76837158e-07f, 0.0375497341f),
                    new Vector3(-11.0952349f, -4.76837158e-07f, 0.00710105896f),
                    1,
                    true,
                    DungeonAreaTypes.Jump,
                    AgentActionTypes.All);

                PathFinderDescription.AddConnection(
                    new Vector3(17, 0, -14),
                    new Vector3(16, 0, -15),
                    0.15f,
                    false,
                    DungeonAreaTypes.Ground,
                    AgentActionTypes.All);
            }

            var conns = PathFinderDescription.GetConnections();
            debugDrawers.DrawConnections(conns);
        }
        private void StartEntities()
        {
            //Rat holes
            ratHoles = scenery
                .GetObjectsByName("Dn_Rat_Hole_1")
                .Select(o => o.Instance.Manipulator.Position)
                .ToArray();

            //NPCs
            StartNPCs();

            //Obstacles
            StartEntitiesObstacles();

            //Sounds
            StartEntitiesAudio();

            //Debug info
            debugDrawers.DrawGraph();
            debugDrawers.DrawScenery(scenery);
            debugDrawers.DrawObstacles(obstacles);
        }
        private void StartNPCs()
        {
            AnimationPath p0 = new();
            p0.AddLoop("stand");

            if (scenery.CurrentLevel.Name != "Lvl1")
            {
                human.Visible = false;

                return;
            }

            for (int i = 0; i < human.InstanceCount; i++)
            {
                human[i].Manipulator.SetPosition(31, 0, i == 0 ? -31 : -29);
                human[i].Manipulator.SetRotation(-MathUtil.PiOverTwo, 0, 0);

                human[i].AnimationController.Start(new AnimationPlan(p0), i * 1f);
                human[i].AnimationController.TimeDelta = 0.5f + (i * 0.1f);
            }

            human.Visible = true;
        }
        private void StartEntitiesAudio()
        {
            //Rat sound
            soundEffectsManager.CreateRatSounds(rat);

            //Torchs
            StartEntitiesAudioTorchs();

            //Big fires
            StartEntitiesAudioBigFires();
        }
        private void StartEntitiesAudioTorchs()
        {
            var torchs = scenery
                .GetObjectsByName("Dn_Torch")
                .Select(o => o.Instance);

            soundEffectsManager.CreateTorchEmitters(torchs);
        }
        private void StartEntitiesAudioBigFires()
        {
            List<ModelInstance> fires =
            [
                .. scenery.GetObjectsByName("Dn_Temple_Fire_1").Select(o => o.Instance),
                .. scenery.GetObjectsByName("Dn_Big_Lamp_1").Select(o => o.Instance),
            ];

            soundEffectsManager.CreateBigFireEmitters(fires);
        }
        private void StartEntitiesObstacles()
        {
            obstacles.Clear();

            //Object obstacles
            var sceneryObjects = scenery
                .GetObjectsByType(ObjectTypes.Furniture | ObjectTypes.Door)
                .Select(o => o.Instance);

            foreach (var item in sceneryObjects)
            {
                var obb = item.GetOrientedBoundingBox();

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
        }

        private void ClearLevelLights()
        {
            Lights.ClearPointLights();
            Lights.ClearSpotLights();
            lightControllers.Clear();
        }
        private void InitializeLevelLights()
        {
            var controllers = Lights.PointLights
                .Where(l => l.Name.Contains("Dn_Torch") || l.Name.Contains("Dn_Temple"))
                .Select(l =>
                {
                    Vector3 orig = l.Position;

                    return new LightController()
                    {
                        Light = l,
                        PositionFnc = () =>
                        {
                            return orig;
                        },
                    };
                });

            lightControllers.AddRange(controllers);

            Lights.Add(torch);

            lightControllers.Add(new()
            {
                Light = torch,
                PositionFnc = () =>
                {
                    return
                        Camera.Position +
                        (Camera.Direction * 0.5f) +
                        (Camera.Left * 0.2f);
                },
            });
        }

        private void ClearNPCs()
        {
            ratActive = false;
            rat.Visible = false;
            ratController.Clear();

            human.Visible = false;
        }

        private void CreateWind()
        {
            Manipulator3D man = new();
            man.SetPosition(windPosition);

            var windEffectDuration = soundEffectsManager.PlayWind(man);

            float durationVariation = Helper.RandomGenerator.NextFloat(0.5f, 1.0f);
            int duration = (int)(windEffectDuration.TotalMilliseconds * durationVariation);

            Task.Run(async () =>
            {
                await Task.Delay(Math.Max(100, duration));

                CreateWind();

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
