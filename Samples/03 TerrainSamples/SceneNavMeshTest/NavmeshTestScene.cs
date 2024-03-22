﻿using Engine;
using Engine.Collada;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TerrainSamples.Mapping;
using TerrainSamples.SceneStart;

namespace TerrainSamples.SceneNavMeshTest
{
    /// <summary>
    /// Navigation mesh test scene
    /// </summary>
    class NavmeshTestScene : WalkableScene
    {
        private readonly string resourcesFolder = "SceneNavmeshTest/resources";

        private readonly InputMapper inputMapper;

        public InputEntry GameWindowedLook { get; set; }
        public InputEntry GameExit { get; set; }
        public InputEntry GameHelp { get; set; }
        public InputEntry CamLeft { get; set; }
        public InputEntry CamRight { get; set; }
        public InputEntry CamFwd { get; set; }
        public InputEntry CamBwd { get; set; }
        public InputEntry CamUp { get; set; }
        public InputEntry CamDown { get; set; }
        public InputEntry NmRndPointCircle { get; set; }
        public InputEntry NmRndPoint { get; set; }
        public InputEntry GContac1Point { get; set; }
        public InputEntry GContac2Point { get; set; }
        public InputEntry GBuild { get; set; }
        public InputEntry GPartition { get; set; }
        public InputEntry GTileCache { get; set; }
        public InputEntry GSave { get; set; }
        public InputEntry GLoad { get; set; }

        private readonly Player agent = new()
        {
            Name = "Player",
            Height = 0.2f,
            Radius = 0.1f,
            MaxClimb = 0.5f,
            MaxSlope = 50f,
            Velocity = 3f,
            VelocitySlow = 1f,
        };

        private UIControlTweener uiTweener;
        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea debug = null;
        private UITextArea help = null;
        private UITextArea message = null;

        private UIPanel mainPanel = null;
        private UIPanel debugPanel = null;

        private readonly string buttonFonts = "Verdana, Consolas";
        private readonly Color sceneButtonColor = Color.AdjustSaturation(Color.CornflowerBlue, 1.5f);

        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Triangle> triangleDrawer = null;

        private Model inputGeometry = null;
        private Model debugGeometry = null;
        private readonly BuildSettings nmsettings = BuildSettings.Default;

        private float? lastElapsedSeconds = null;
        private TimeSpan enqueueTime = TimeSpan.Zero;
        private string loadState = null;

        private bool uiReady = false;
        private bool gameReady = false;
        private readonly StateManager stateManager = new();

        private readonly Color4 pointColor = new(Color.White.ToVector3(), 1f);
        private readonly Color4 walkableColor = new(Color.Green.RGB(), 0.75f);
        private readonly Color4 unwalkableColor = new(Color.Red.RGB(), 0.75f);

        private readonly Color4 rndColor = new(Color.White.ToVector3(), 0.5f);
        private readonly Color4 circleColor = new(Color.Orange.ToVector3(), 0.5f);
        private readonly Color4 pickInColor = new(Color.LightGreen.ToVector3(), 0.5f);
        private readonly Color4 pickOutColor = new(Color.Pink.ToVector3(), 0.5f);
        private readonly Color4 obsColor = new(Color.Yellow.RGB(), 0.5f);

        private readonly List<ObstacleMarker> obstacles = [];
        private readonly List<AreaMarker> areas = [];
        private GraphDebugTypes debugType = GraphDebugTypes.Nodes;

        private string pathFinderStartMessage = null;
        private string pathFinderHelpMessage = null;
        private Vector3 lastPosition = Vector3.Zero;
        private Vector3? pathFindingStart = null;
        private Vector3? pathFindingEnd = null;
        private readonly Color pathFindingColorPath = new(128, 0, 255, 255);
        private readonly Color pathFindingColorStart = new(97, 0, 255, 255);
        private readonly Color pathFindingColorEnd = new(0, 97, 255, 255);

        public NavmeshTestScene(Game game) : base(game)
        {
            inputMapper = new InputMapper(Game);

            GameEnvironment.Background = new Color4(0.09f, 0.09f, 0.09f, 1f);

            Game.VisibleMouse = true;
            Game.LockMouse = false;

            Camera.MovementDelta = 25f;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            LoadResourcesAsync(
                [
                    InitializeTweener(),
                    InitializeText(),
                    InitializeUI(),
                ],
                InitializeComponentsCompleted);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);

            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeText()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = "Navigation Mesh Test Scene";

            debug = await AddComponentUI<UITextArea, UITextAreaDescription>("Debug", "Debug", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Green });
            debug.Text = null;

            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow });
            help.Text = null;
            help.Visible = false;

            message = await AddComponentUI<UITextArea, UITextAreaDescription>("Message", "Message", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Orange, GrowControlWithText = false, TextHorizontalAlign = TextHorizontalAlign.Right });
            message.Text = null;
            message.Visible = false;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", spDesc, LayerUI - 1);
        }
        private async Task InitializeUI()
        {
            var btnFont = TextDrawerDescription.FromFamily(buttonFonts, 10, FontMapStyles.Bold, true);
            btnFont.ContentPath = resourcesFolder;

            var btnDesc = UIButtonDescription.DefaultTwoStateButton(btnFont, "buttons.png", new Vector4(55, 171, 545, 270) / 600f, new Vector4(55, 171, 545, 270) / 600f);
            btnDesc.ContentPath = resourcesFolder;
            btnDesc.ColorReleased = new Color4(sceneButtonColor.RGB(), 0.8f);
            btnDesc.ColorPressed = new Color4(sceneButtonColor.RGB() * 1.2f, 0.9f);
            btnDesc.TextForeColor = Color.Gold;
            btnDesc.TextHorizontalAlign = TextHorizontalAlign.Center;
            btnDesc.TextVerticalAlign = TextVerticalAlign.Middle;
            btnDesc.StartsVisible = false;

            var btnTiles = await InitializeButton("btnTiles", "Tiles", btnDesc, () => stateManager.StartState(States.Tiles));
            var btnObstacle = await InitializeButton("btnObstacle", "Obstacles", btnDesc, () =>
            {
                if (!nmsettings.UseTileCache)
                {
                    ShowMessage("Tile cache must be enabled.");

                    return;
                }

                stateManager.StartState(States.AddObstacle);
            });
            var btnArea = await InitializeButton("btnArea", "Areas", btnDesc, () => stateManager.StartState(States.AddArea));
            var btnPathFinding = await InitializeButton("btnPathFinding", "Path Finding", btnDesc, () => stateManager.StartState(States.PathFinding));
            var btnDebug = await InitializeButton("btnDebug", "Debug", btnDesc, () => stateManager.StartState(States.Debug));

            UIButton[] mainBtns = [btnTiles, btnObstacle, btnArea, btnPathFinding, btnDebug];

            var panDesc = UIPanelDescription.Default(Color.Transparent);
            mainPanel = await AddComponentUI<UIPanel, UIPanelDescription>("MainPanel", "MainPanel", panDesc);
            mainPanel.Spacing = 10;
            mainPanel.Padding = 15;
            mainPanel.SetGridLayout(GridLayout.FixedColumns(mainBtns.Length));
            mainPanel.Visible = false;

            foreach (var b in mainBtns)
            {
                mainPanel.AddChild(b, false);
            }

            var enumValues = Enum
                .GetValues<GraphDebugTypes>()
                .Except([GraphDebugTypes.None]);

            var debugDesc = UIPanelDescription.Default(Color.Transparent);
            debugPanel = await AddComponentUI<UIPanel, UIPanelDescription>("DebugPanel", "DebugPanel", debugDesc);
            debugPanel.Spacing = 10;
            debugPanel.Padding = 15;
            debugPanel.SetGridLayout(GridLayout.FixedRows(enumValues.Count()));
            debugPanel.Visible = false;

            foreach (var dType in enumValues)
            {
                var btn = await InitializeButton($"btnDebug{dType}", $"{dType}", btnDesc, async () =>
                {
                    if (debugType == dType)
                    {
                        stateManager.StartState(States.Default);

                        return;
                    }

                    await Task.Delay(100);

                    debugType = dType;

                    DrawGraphNodes(agent);

                    stateManager.StartState(States.Default);

                });
                debugPanel.AddChild(btn, false);
            }
        }
        private async Task<UIButton> InitializeButton(string name, string caption, UIButtonDescription desc, Action clickAction = null)
        {
            var button = await AddComponentUI<UIButton, UIButtonDescription>(name, name, desc, LayerUI);
            button.Caption.Text = caption;

            if (clickAction != null)
            {
                button.MouseClick += async (ctrl, args) =>
                {
                    if (!gameReady)
                    {
                        return;
                    }

                    if (!args.Buttons.HasFlag(MouseButtons.Left))
                    {
                        return;
                    }

                    await Task.Delay(100);

                    clickAction();
                };
            }

            return button;
        }
        private void InitializeComponentsCompleted(LoadResourcesResult resUi)
        {
            if (!resUi.Completed)
            {
                resUi.ThrowExceptions();
            }

            stateManager.InitializeState(States.Default, StartDefaultState, UpdateGameStateDefault);
            stateManager.InitializeState(States.Tiles, StartTilesState, UpdateGameStateTiles);
            stateManager.InitializeState(States.AddObstacle, StartAddObstacleState, UpdateGameStateAddObstacle);
            stateManager.InitializeState(States.AddArea, StartAddAreaState, UpdateGameStateAddArea);
            stateManager.InitializeState(States.PathFinding, StartPathFindingState, UpdateGameStatePathFinding);
            stateManager.InitializeState(States.Debug, StartDebugState, UpdateGameStateDebug);

            UpdateLayout();
            InitializeInputMapping();
            InitializeLights();

            Camera.NearPlaneDistance = 0.01f;
            Camera.FarPlaneDistance *= 2;

            uiReady = true;

            InitializeMapData();
        }
        private void InitializeInputMapping()
        {
            InputMapperDescription mapperDescription = new()
            {
                InputEntries =
                [
                    new("GameWindowedLook", MouseButtons.Right),
                    new("GameExit", Keys.Escape),
                    new("GameHelp", Keys.F1),
                    new("CamLeft", Keys.A),
                    new("CamRight", Keys.D),
                    new("CamFwd", Keys.W),
                    new("CamBwd", Keys.S),
                    new("CamUp", Keys.Space),
                    new("CamDown", Keys.C),
                    new("NmRndPointCircle", MouseButtons.Middle),
                    new("NmRndPoint", Keys.R),
                    new("GContac1Point", MouseButtons.Left),
                    new("GContac2Point", MouseButtons.Right),
                    new("GBuild", Keys.B),
                    new("GPartition", Keys.P),
                    new("GTileCache", Keys.T),
                    new("GSave", Keys.F5),
                    new("GLoad", Keys.F6),
                ]
            };

            if (!inputMapper.LoadMapping(mapperDescription, out string errorMessage))
            {
                ShowMessage($"Error configuring key mapping: {errorMessage}. Press Escape to exit.", 15000);

                GameExit = new(Game, Keys.Escape);

                return;
            }

            GameWindowedLook = inputMapper.Get("GameWindowedLook");
            GameExit = inputMapper.Get("GameExit");
            GameHelp = inputMapper.Get("GameHelp");
            CamLeft = inputMapper.Get("CamLeft");
            CamRight = inputMapper.Get("CamRight");
            CamFwd = inputMapper.Get("CamFwd");
            CamBwd = inputMapper.Get("CamBwd");
            CamUp = inputMapper.Get("CamUp");
            CamDown = inputMapper.Get("CamDown");
            NmRndPointCircle = inputMapper.Get("NmRndPointCircle");
            NmRndPoint = inputMapper.Get("NmRndPoint");
            GContac1Point = inputMapper.Get("GContac1Point");
            GContac2Point = inputMapper.Get("GContac2Point");
            GBuild = inputMapper.Get("GBuild");
            GPartition = inputMapper.Get("GPartition");
            GTileCache = inputMapper.Get("GTileCache");
            GSave = inputMapper.Get("GSave");
            GLoad = inputMapper.Get("GLoad");

            help.Text = GetHelpText();

            pathFinderStartMessage = $"Press {GContac1Point} to add the start point. Then, press {GContac1Point} to add the end point.";
            pathFinderHelpMessage = $"Press {GContac1Point} to add the end point. {GContac2Point} to change the start point. {GameExit} to close the path finding tool.";
        }
        private void InitializeLights()
        {
            Lights.KeyLight.CastShadow = false;
            Lights.BackLight.Enabled = true;
            Lights.FillLight.Enabled = true;
        }

        private void InitializeMapData()
        {
            LoadResourcesAsync(
                [
                    InitializeNavmesh(),
                    InitializeDebug()
                ],
                InitializeMapDataCompleted);
        }
        private async Task InitializeNavmesh()
        {
            var contentDesc = ContentDescription.FromFile(resourcesFolder, "testSimpleMap.json");
            var desc = new ModelDescription()
            {
                TextureIndex = 0,
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = contentDesc,
                PathFindingHull = PickingHullTypes.Perfect,
            };

            inputGeometry = await AddComponentGround<Model, ModelDescription>("NavMesh", "NavMesh", desc);
            inputGeometry.Manipulator.SetRotation(-MathUtil.PiOverTwo, 0, 0);

            //Rasterization
            nmsettings.CellSize = 0.1f;
            nmsettings.CellHeight = 0.1f;

            //Agents
            nmsettings.Agents = new[] { agent };

            //Region
            nmsettings.RegionMinSize = 8;
            nmsettings.RegionMergeSize = 20;

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Monotone;

            //Filtering
            nmsettings.FilterLedgeSpans = false;
            nmsettings.FilterLowHangingObstacles = false;
            nmsettings.FilterWalkableLowHeightSpans = false;

            //Polygonization
            nmsettings.EdgeMaxLength = 12f;
            nmsettings.EdgeMaxError = 1.3f;
            nmsettings.VertsPerPoly = 6;

            //Detail mesh
            nmsettings.DetailSampleDist = 6;
            nmsettings.DetailSampleMaxError = 1;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 64;

            //Debugging
            nmsettings.EnableDebugInfo = true;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
        }
        private async Task InitializeDebug()
        {
            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 10000
            };
            lineDrawer = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Volumes", "DEBUG++ Volumes", volumesDrawerDesc);

            var markDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
            };
            triangleDrawer = await AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Marks", "DEBUG++ Marks", markDrawerDesc);
        }
        private void InitializeMapDataCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            var bbox = inputGeometry.GetBoundingBox();
            var center = bbox.GetCenter();
            float maxD = Math.Max(Math.Max(bbox.Width, bbox.Height), bbox.Depth);

            Camera.SetInterest(center);
            Camera.SetPosition(center + new Vector3(1f, 1.2f, 1f) * maxD * 0.8f);

            EnqueueGraph();
        }
        public override void NavigationGraphLoaded()
        {
            var mapTime = DateTime.Now.TimeOfDay;
            loadState = null;

            lastElapsedSeconds = (float)(mapTime - enqueueTime).TotalMilliseconds / 1000.0f;

            mainPanel.Visible = true;

            DrawGraphNodes(agent);

            obstacles.Clear();
            DrawGraphObjects();

            gameReady = true;
        }
        public override void NavigationGraphUpdated()
        {
            DrawGraphNodes(agent);
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (!uiReady)
            {
                return;
            }

            if (GameHelp.JustReleased)
            {
                help.Visible = !help.Visible;
            }

            UpdateCameraInput();

            UpdateLoadingText();

            if (!gameReady)
            {
                return;
            }

            UpdateGameState();
        }
        private void UpdateCameraInput()
        {
            if (CamLeft.Pressed)
            {
                Camera.MoveLeft(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (CamRight.Pressed)
            {
                Camera.MoveRight(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (CamFwd.Pressed)
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (CamBwd.Pressed)
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (CamUp.Pressed)
            {
                Camera.MoveUp(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (CamDown.Pressed)
            {
                Camera.MoveDown(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.MouseWheelDelta > 0)
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.MouseWheelDelta < 0)
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (GameWindowedLook.Pressed)
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
        }
        private void UpdateLoadingText()
        {
            string partition = nmsettings.UseTileCache ? "Using TileCache" : $"Partition {nmsettings.PartitionType}";
            string loading = loadState ?? $"Build Time: {lastElapsedSeconds:0.00000} seconds.";
            debug.Text = $"Build {nmsettings.BuildMode} | {partition} => {loading}";
        }
        private void UpdateGameState()
        {
            if (InputProcessedByUI)
            {
                return;
            }

            stateManager.UpdateState();
        }
        private void UpdateGameStateDefault()
        {
            if (GameExit.JustReleased)
            {
                Game.SetScene<StartScene>();
            }

            UpdateNavmeshInput();
            UpdateGraphInput();
        }
        private void UpdateGameStateTiles()
        {
            if (GameExit.JustReleased)
            {
                stateManager.StartState(States.Default);
            }

            if (!GContac1Point.JustReleased)
            {
                return;
            }

            ToggleTile(lastPosition);
        }
        private void ToggleTile(Vector3 tilePosition)
        {
            lastElapsedSeconds = null;
            loadState = $"Updating tile at {tilePosition}.";

            bool remove = Game.Input.ShiftPressed;
            bool create = Game.Input.ControlPressed;

            var sw = Stopwatch.StartNew();
            try
            {
                if (create)
                {
                    NavigationGraph.CreateAt(tilePosition);
                    return;
                }

                if (remove)
                {
                    NavigationGraph.RemoveAt(tilePosition);
                    return;
                }

                NavigationGraph.UpdateAt(tilePosition);
            }
            finally
            {
                sw.Stop();
                lastElapsedSeconds = sw.ElapsedMilliseconds / 1000.0f;
                loadState = null;
            }
        }
        private void UpdateGameStateAddObstacle()
        {
            if (GameExit.JustReleased)
            {
                stateManager.StartState(States.Default);
            }

            if (!GContac1Point.JustReleased)
            {
                return;
            }

            bool remove = Game.Input.ShiftPressed;
            if (remove)
            {
                UpdateObstacleRemove();
            }
            else
            {
                UpdateObstacleAdd();
            }
        }
        private void UpdateObstacleAdd()
        {
            var pRay = GetPickingRay(PickingHullTypes.Perfect);
            if (!this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                return;
            }

            var p = r.PickingResult.Position;
            float h = 1f;
            var center = new Vector3(p.X, p.Y + (h * 0.5f), p.Z);
            var cy = new BoundingCylinder(center, 0.5f, h);
            int id = NavigationGraph.AddObstacle(cy);
            var obs = new ObstacleMarker()
            {
                Id = id,
                Obstacle = cy,
            };
            obstacles.Add(obs);
            DrawGraphObjects();

            NavigationGraph.UpdateAt(p);

            stateManager.StartState(States.Default);
        }
        private void UpdateObstacleRemove()
        {
            var pRay = GetPickingRay(PickingHullTypes.Perfect);
            var ray = (Ray)pRay;

            foreach (var obs in obstacles)
            {
                if (obs.Bbox.Intersects(ref ray))
                {
                    NavigationGraph.RemoveObstacle(obs.Id);
                    obstacles.Remove(obs);
                    DrawGraphObjects();

                    NavigationGraph.UpdateAt(obs.Obstacle.Center);

                    stateManager.StartState(States.Default);
                    break;
                }
            }
        }
        private void UpdateGameStateAddArea()
        {
            if (GameExit.JustReleased)
            {
                stateManager.StartState(States.Default);
            }

            if (!GContac1Point.JustReleased)
            {
                return;
            }

            bool remove = Game.Input.ShiftPressed;
            if (remove)
            {
                UpdateAreaRemove();
            }
            else
            {
                UpdateAreaAdd();
            }
        }
        private void UpdateAreaAdd()
        {
            var pRay = GetPickingRay(PickingHullTypes.Perfect);
            if (!this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                return;
            }

            float hmin = 0.1f;
            float hmax = 6f;
            var center = r.PickingResult.Position;
            center.Y = 0;
            float radius = 2.5f;
            var circle = GeometryUtil.CreateCircle(Topology.LineList, center, radius, 12);
            int id = PathFinderDescription.Input.AddArea(new GraphAreaPolygon(circle.Vertices, -hmin, hmax - hmin) { AreaType = GraphAreaTypes.Walkable, });
            var area = new AreaMarker()
            {
                Id = id,
                Center = center,
                Radius = radius,
            };
            areas.Add(area);
            DrawGraphObjects();
            EnqueueGraph();

            stateManager.StartState(States.Default);
        }
        private void UpdateAreaRemove()
        {
            var pRay = GetPickingRay(PickingHullTypes.Perfect);
            var ray = (Ray)pRay;

            foreach (var area in areas)
            {
                var center = area.Center;
                var radius = area.Radius;
                var plane = new Plane(center, Vector3.Up);
                if (!plane.Intersects(ref ray, out Vector3 p))
                {
                    continue;
                }
                if (Vector3.Distance(p, center) > radius)
                {
                    continue;
                }

                PathFinderDescription.Input.RemoveArea(area.Id);
                areas.Remove(area);
                DrawGraphObjects();
                EnqueueGraph();

                stateManager.StartState(States.Default);
                break;
            }
        }
        private void UpdateGameStateDebug()
        {
            if (GameExit.JustReleased)
            {
                stateManager.StartState(States.Default);
            }
        }
        private void UpdateGameStatePathFinding()
        {
            if (GameExit.JustReleased)
            {
                stateManager.StartState(States.Default);
            }

            if (!GContac1Point.JustReleased && !GContac2Point.JustReleased)
            {
                return;
            }

            var pRay = GetPickingRay(PickingHullTypes.Perfect);
            if (!this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                return;
            }

            if (GContac2Point.JustReleased || pathFindingStart == null)
            {
                pathFindingStart = r.PickingResult.Position;

                UpdatePathFindingData();

                return;
            }

            pathFindingEnd = r.PickingResult.Position;

            UpdatePathFindingData();
        }
        private void UpdatePathFindingData()
        {
            lineDrawer.Clear(pathFindingColorStart);
            lineDrawer.Clear(pathFindingColorEnd);
            lineDrawer.Clear(pathFindingColorPath);

            ShowMessage(pathFinderHelpMessage);

            var start = pathFindingStart;
            var end = pathFindingEnd;

            if (start != null)
            {
                DrawCircle(start.Value, agent.Radius * 2, pathFindingColorStart);
                DrawPlayer(start.Value, pathFindingColorStart);
            }

            if (end != null)
            {
                DrawCircle(end.Value, agent.Radius * 2, pathFindingColorEnd);
                DrawPlayer(end.Value, pathFindingColorEnd);
            }

            if (start != null && end != null)
            {
                var path = FindPath(agent, start.Value, end.Value, false);
                DrawPath(path, pathFindingColorPath);
            }
        }
        private void UpdateNavmeshInput()
        {
            if (NmRndPointCircle.JustReleased)
            {
                UpdateFindRandomPointCircleInput();
            }

            if (NmRndPoint.JustReleased)
            {
                UpdateFindRandomPointInput();
            }
        }
        private void UpdateFindRandomPointCircleInput()
        {
            lineDrawer.Clear(rndColor);
            lineDrawer.Clear(circleColor);
            lineDrawer.Clear(pickInColor);
            lineDrawer.Clear(pickOutColor);

            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (!this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                return;
            }

            DrawPoint(r.PickingResult.Position, 0.25f, rndColor);
            DrawTriangle(r.PickingResult.Primitive, rndColor);

            float radius = 5;

            DrawCircle(r.PickingResult.Position, radius, circleColor);

            var pt = NavigationGraph.FindRandomPoint(agent, r.PickingResult.Position, radius);
            if (!pt.HasValue)
            {
                return;
            }

            float dist = Vector3.Distance(r.PickingResult.Position, pt.Value);
            var color = dist < radius ? pickInColor : pickOutColor;
            DrawPoint(pt.Value, 2.5f, color);
        }
        private void UpdateFindRandomPointInput()
        {
            lineDrawer.Clear(pickInColor);

            var pt = NavigationGraph.FindRandomPoint(agent);
            if (pt.HasValue)
            {
                DrawPoint(pt.Value, 2.5f, pickInColor);
            }
        }
        private void UpdateGraphInput()
        {
            bool updateGraph = false;

            if (GSave.JustReleased)
            {
                UpdateGraphSaveInput();

                return;
            }

            if (GLoad.JustReleased)
            {
                UpdateGraphLoadInput();

                return;
            }

            if (GContac1Point.JustReleased)
            {
                UpdateContactInput();

                return;
            }

            if (GBuild.JustReleased)
            {
                updateGraph = ChangeBuilMode(!Game.Input.ShiftPressed);
            }

            if (GPartition.JustReleased)
            {
                updateGraph = ChangePartitionType(!Game.Input.ShiftPressed);
            }

            if (GTileCache.JustReleased)
            {
                updateGraph = ChangeUseTileCache(!nmsettings.UseTileCache);
            }

            if (updateGraph)
            {
                EnqueueGraph();
            }
        }
        private void UpdateGraphSaveInput()
        {
            if (lastElapsedSeconds == null)
            {
                ShowOnGraphLoadingMessage();

                return;
            }

            using var dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.FileName = @"test.grf";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Task.Run(() => PathFinderDescription.Save(dlg.FileName, NavigationGraph));
            }
        }
        private void UpdateGraphLoadInput()
        {
            if (lastElapsedSeconds == null)
            {
                ShowOnGraphLoadingMessage();

                return;
            }

            using var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.FileName = @"test.grf";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadNavigationGraphFromFile(dlg.FileName);
            }
        }
        private void UpdateContactInput()
        {
            lineDrawer.Clear(walkableColor);
            lineDrawer.Clear(unwalkableColor);
            lineDrawer.Clear(pointColor);

            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (!this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                return;
            }

            lastPosition = r.PickingResult.Position;

            bool walkable = IsWalkable(agent, lastPosition, agent.MaxClimb, out var nearest);
            var pColor = walkable ? walkableColor : unwalkableColor;

            if (nearest.HasValue)
            {
                DrawCircle(nearest.Value + new Vector3(0.02f), 0.1f, pointColor);
            }

            DrawPoint(lastPosition, 0.1f, pColor);
            DrawTriangle(r.PickingResult.Primitive, pColor);

            DrawGraphNodes(agent);
        }
        private bool ChangeBuilMode(bool next)
        {
            if (lastElapsedSeconds == null)
            {
                ShowOnGraphLoadingMessage();

                return false;
            }

            var buildModes = Enum.GetNames(typeof(BuildModes)).Length;
            BuildModes newBuildMode;
            if (next)
            {
                newBuildMode = (BuildModes)Helper.Next((int)nmsettings.BuildMode, buildModes);
            }
            else
            {
                newBuildMode = (BuildModes)Helper.Prev((int)nmsettings.BuildMode, buildModes);
            }

            nmsettings.BuildMode = newBuildMode;

            if (nmsettings.BuildMode == BuildModes.Solo && nmsettings.UseTileCache)
            {
                ShowMessage($"TileCache disabled due to change to Build {newBuildMode}.");

                nmsettings.UseTileCache = false;
            }

            return true;
        }
        private bool ChangePartitionType(bool next)
        {
            if (lastElapsedSeconds == null)
            {
                ShowOnGraphLoadingMessage();

                return false;
            }

            var sampleTypes = Enum.GetNames(typeof(SamplePartitionTypes)).Length;
            SamplePartitionTypes newPartitionType;
            if (next)
            {
                newPartitionType = (SamplePartitionTypes)Helper.Next((int)nmsettings.PartitionType, sampleTypes);
            }
            else
            {
                newPartitionType = (SamplePartitionTypes)Helper.Prev((int)nmsettings.PartitionType, sampleTypes);
            }

            if (nmsettings.UseTileCache)
            {
                ShowMessage("Partition type cannot be changed with TileCache Enabled.");

                return false;
            }

            nmsettings.PartitionType = newPartitionType;

            return true;
        }
        private bool ChangeUseTileCache(bool useTileCache)
        {
            if (lastElapsedSeconds == null)
            {
                ShowOnGraphLoadingMessage();

                return false;
            }

            if (nmsettings.UseTileCache == useTileCache)
            {
                return false;
            }

            if (nmsettings.BuildMode == BuildModes.Solo)
            {
                ShowMessage($"TileCache cannot be activated with Build mode {nmsettings.BuildMode} Enabled.");

                return false;
            }

            nmsettings.UseTileCache = useTileCache;

            return true;
        }

        private void DrawPoint(Vector3 position, float size, Color4 color)
        {
            var cross = Line3D.CreateCross(position, size);
            lineDrawer.AddPrimitives(color, cross);
        }
        private void DrawTriangle(Triangle triangle, Color4 color)
        {
            var tri = Line3D.CreateTriangle(triangle);
            lineDrawer.AddPrimitives(color, tri);
        }
        private void DrawCircle(Vector3 position, float radius, Color4 color)
        {
            var circle = Line3D.CreateCircle(position, radius, 12);
            lineDrawer.AddPrimitives(color, circle);
        }
        private void DrawPlayer(Vector3 position, Color4 color)
        {
            var basePosition = position;
            basePosition.Y += agent.Height * 0.5f;

            var cylinder = Line3D.CreateCylinder(basePosition, agent.Radius, agent.Height, 12);
            lineDrawer.AddPrimitives(color, cylinder);
        }
        private void DrawPath(PathFindingPath path, Color4 color)
        {
            var pathLines = Line3D.CreateLineList(path?.Positions ?? []);
            lineDrawer.AddPrimitives(color, pathLines);
        }
        private void DrawGraphNodes(AgentType agent)
        {
            Components.RemoveComponent(debugGeometry);

            LoadResourcesAsync(LoadDebugModel(agent, debugType));
        }
        private void DrawGraphObjects()
        {
            triangleDrawer.Clear();

            foreach (var obs in obstacles)
            {
                triangleDrawer.AddPrimitives(obsColor, Triangle.ComputeTriangleList(Topology.TriangleList, obs.Obstacle, 12));
            }

            foreach (var area in areas)
            {
                var g = GeometryUtil.CreateCircle(Topology.TriangleList, area.Center, area.Radius, 12);

                triangleDrawer.AddPrimitives(obsColor, Triangle.ComputeTriangleList(Topology.TriangleList, g.Vertices, g.Indices));
            }
        }
        private async Task LoadDebugModel(AgentType agent, GraphDebugTypes debug)
        {
            var debugInfoList = GetDebugInfo(agent)?.GetInfo((int)debug, lastPosition)?.GetValues() ?? [];
            if (!debugInfoList.Any())
            {
                return;
            }

            var material = new MaterialPhongContent()
            {
                DiffuseColor = Color4.White,
                EmissiveColor = MaterialConstants.EmissiveColor,
                AmbientColor = Color3.White,
                SpecularColor = MaterialConstants.SpecularColor,
                Shininess = MaterialConstants.Shininess,
                IsTransparent = false,
            };

            ContentData content = new();

            int i = 0;
            foreach (var di in debugInfoList)
            {
                string matName = $"mat_{i++}";
                content.AddMaterialContent(matName, material);

                var geo = GenerateVertexData(di.Topology, matName, di.Data);
                if (geo != null)
                {
                    content.ImportMaterial(matName, matName, geo);
                }
            }

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(content),
                CastShadow = ShadowCastingAlgorihtms.None,
                PathFindingHull = PickingHullTypes.None,
                CullingVolumeType = CullingVolumeTypes.None,
                ColliderType = ColliderTypes.None,
                PickingHull = PickingHullTypes.None,
                TextureIndex = 0,
                UseAnisotropicFiltering = false,
                Optimize = false,
                DepthEnabled = true,
                StartsVisible = true,
            };

            debugGeometry = await AddComponentEffect<Model, ModelDescription>("debugGeometry", "debugGeometry", desc);
        }
        private static SubMeshContent GenerateVertexData(Topology topology, string materialName, Dictionary<Color4, IEnumerable<Vector3>> data)
        {
            if (topology == Topology.TriangleList)
            {
                List<VertexData> vertices = [];

                foreach (var color in data.Keys)
                {
                    Vector3[] dverts = [.. data[color]];

                    for (int i = 0; i < dverts.Length; i += 3)
                    {
                        Vector3[] verts = [dverts[i], dverts[i + 1], dverts[i + 2]];
                        var norm = new Plane(dverts[i], dverts[i + 1], dverts[i + 2]).Normal;

                        var vData = verts.Select(v => new VertexData() { Color = color, Position = v, Normal = norm });
                        vertices.AddRange(vData.ToArray());
                    }
                }

                SubMeshContent geo = new(topology, materialName, false, false, Matrix.Identity);
                geo.SetVertices(vertices);
                return geo;
            }
            else if (topology == Topology.LineList)
            {
                List<VertexData> vertices = [];

                foreach (var color in data.Keys)
                {
                    Vector3[] dverts = [.. data[color]];

                    for (int i = 0; i < dverts.Length; i += 2)
                    {
                        Vector3[] verts = [dverts[i], dverts[i + 1]];

                        var vData = verts.Select(v => new VertexData() { Color = color, Position = v });
                        vertices.AddRange(vData.ToArray());
                    }
                }

                SubMeshContent geo = new(topology, materialName, false, false, Matrix.Identity);
                geo.SetVertices(vertices);
                return geo;
            }

            return null;
        }

        private void EnqueueGraph()
        {
            mainPanel.Visible = false;

            lastElapsedSeconds = null;
            loadState = "Updating navigation graph.";

            enqueueTime = DateTime.Now.TimeOfDay;
            EnqueueNavigationGraphUpdate();
        }
        private void ShowMessage(string text, long duration = 5000)
        {
            message.Text = text;
            message.Visible = true;
            uiTweener.FadeOff(message, duration);
        }
        private void ShowOnGraphLoadingMessage()
        {
            ShowMessage("Graph already loading. Please wait for results.");
        }
        private string GetHelpText()
        {
            return @$"Camera: {CamFwd} {CamLeft} {CamBwd} {CamRight} {CamUp} & {CamDown} to move, Mouse To look (Press {GameWindowedLook} in windowed mode). 
{GBuild}: Change Build Mode (SHIFT reverse).
{GPartition}: Change Partition Type (SHIFT reverse).
{GTileCache}: Toggle using Tile Cache.
{GSave}: Saves the graph to a file.
{GLoad}: Loads the graph from a file.
{GContac1Point}: Update current tile (SHIFT remove, CTRL add).
{NmRndPointCircle}: Finds random point around circle (5 units).
{NmRndPoint}: Finds random over navmesh";
        }

        private void StartDefaultState()
        {
            mainPanel.Visible = true;
            debugPanel.Visible = false;
        }
        private void StartTilesState()
        {
            mainPanel.Visible = false;
            debugPanel.Visible = false;

            ShowMessage($"Press {GContac1Point} to update a tile. SHIFT {GContac1Point} to remove a tile. CTRL {GContac1Point} to add a tile.");
        }
        private void StartAddObstacleState()
        {
            mainPanel.Visible = false;
            debugPanel.Visible = false;

            ShowMessage($"Press {GContac1Point} to add obstacle. SHIFT {GContac1Point} to remove.");
        }
        private void StartAddAreaState()
        {
            mainPanel.Visible = false;
            debugPanel.Visible = false;

            ShowMessage($"Press {GContac1Point} to add area. SHIFT {GContac1Point} to remove.");
        }
        private void StartPathFindingState()
        {
            mainPanel.Visible = false;
            debugPanel.Visible = false;

            pathFindingStart = null;
            pathFindingEnd = null;

            lineDrawer.Clear(pathFindingColorPath);
            lineDrawer.Clear(pathFindingColorStart);
            lineDrawer.Clear(pathFindingColorEnd);

            ShowMessage(pathFinderStartMessage);
        }
        private void StartDebugState()
        {
            mainPanel.Visible = false;
            debugPanel.Visible = true;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();
            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            debug.SetPosition(new Vector2(0, title.Top + title.Height + 3));
            help.SetPosition(new Vector2(0, debug.Top + debug.Height + 3));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = debug.Top + debug.Height + 3;
            message.Width = Game.Form.RenderWidth;
            message.Anchor = Anchors.BottomRight;

            float cellH = Game.Form.RenderHeight * 0.0666f;
            float cellW = Game.Form.RenderWidth * 0.1f;

            mainPanel.Height = cellH + mainPanel.Padding.Top;
            mainPanel.Width = cellW * mainPanel.GetGridLayout().Columns;
            mainPanel.Top = panel.Top + panel.Height;
            mainPanel.Anchor = Anchors.Right;

            debugPanel.Height = cellH * debugPanel.GetGridLayout().Rows;
            debugPanel.Width = cellW + debugPanel.Padding.Left;
            debugPanel.Top = panel.Top + panel.Height;
            debugPanel.Anchor = Anchors.Right;
        }
    }
}
