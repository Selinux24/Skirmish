using Engine;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TerrainSamples.Mapping;
using TerrainSamples.SceneStart;

namespace TerrainSamples.SceneNavmeshTest
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
        public InputEntry GContacPoint { get; set; }
        public InputEntry GBuild { get; set; }
        public InputEntry GPartition { get; set; }
        public InputEntry GTileCache { get; set; }
        public InputEntry GSave { get; set; }
        public InputEntry GLoad { get; set; }

        private Player agent = null;

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea debug = null;
        private UITextArea help = null;
        private UITextArea message = null;
        private UIControlTweener uiTweener;

        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;

        private Model inputGeometry = null;
        private readonly BuildSettings nmsettings = BuildSettings.Default;

        private float? lastElapsedSeconds = null;
        private TimeSpan enqueueTime = TimeSpan.Zero;
        private string loadState = null;

        private bool uiReady = false;
        private bool gameReady = false;

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
                new[]
                {
                    InitializeTweener(),
                    InitializeText(),
                },
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
            help.Text = GetHelpText();
            help.Visible = false;

            message = await AddComponentUI<UITextArea, UITextAreaDescription>("Message", "Message", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Orange });
            message.Text = null;
            message.Visible = false;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", spDesc, LayerUI - 1);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult resUi)
        {
            if (!resUi.Completed)
            {
                resUi.ThrowExceptions();
            }

            UpdateLayout();
            InitializeInputMapping();
            InitializeLights();
            InitializeAgent();

            uiReady = true;

            InitializeMapData();
        }
        private void InitializeInputMapping()
        {
            InputMapperDescription mapperDescription = new()
            {
                InputEntries = new InputEntryDescription[]
                {
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
                    new("GContacPoint", MouseButtons.Left),
                    new("GBuild", Keys.B),
                    new("GPartition", Keys.P),
                    new("GTileCache", Keys.T),
                    new("GSave", Keys.F5),
                    new("GLoad", Keys.F6),
                }
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
            GContacPoint = inputMapper.Get("GContacPoint");
            GBuild = inputMapper.Get("GBuild");
            GPartition = inputMapper.Get("GPartition");
            GTileCache = inputMapper.Get("GTileCache");
            GSave = inputMapper.Get("GSave");
            GLoad = inputMapper.Get("GLoad");
        }
        private void InitializeLights()
        {
            Lights.KeyLight.CastShadow = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
        }
        private void InitializeAgent()
        {
            agent = new Player()
            {
                Name = "Player",
                Height = 0.2f,
                Radius = 0.1f,
                MaxClimb = 0.5f,
                MaxSlope = 50f,
                Velocity = 3f,
                VelocitySlow = 1f,
            };

            Camera.NearPlaneDistance = 0.01f;
            Camera.FarPlaneDistance *= 2;
        }

        private void InitializeMapData()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeNavmesh(),
                    InitializeDebug()
                },
                InitializeMapDataCompleted);
        }
        private async Task InitializeNavmesh()
        {
            var contentDesc = ContentDescription.FromFile(resourcesFolder, "modular_dungeon.json");
            var desc = new ModelDescription()
            {
                TextureIndex = 0,
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = contentDesc,
                PathFindingHull = PickingHullTypes.Perfect,
            };

            inputGeometry = await AddComponentGround<Model, ModelDescription>("NavMesh", "NavMesh", desc);

            //Rasterization
            nmsettings.CellSize = 0.1f;
            nmsettings.CellHeight = 0.1f;

            //Agents
            nmsettings.Agents = new[] { agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Monotone;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 64;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
        }
        private async Task InitializeDebug()
        {
            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
            };
            graphDrawer = await AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Graph", "DEBUG++ Graph", graphDrawerDesc);

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 10000
            };
            volumesDrawer = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Volumes", "DEBUG++ Volumes", volumesDrawerDesc);
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
            Camera.SetPosition(center + new Vector3(1, 0.8f, -1) * maxD * 0.8f);

            EnqueueGraph();
        }
        public override void NavigationGraphLoaded()
        {
            var mapTime = DateTime.Now.TimeOfDay;
            loadState = null;

            lastElapsedSeconds = (float)(mapTime - enqueueTime).TotalMilliseconds / 1000.0f;

            DrawGraphNodes(agent);

            gameReady = true;
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

            if (GameExit.JustReleased)
            {
                Game.SetScene<StartScene>();
            }

            UpdateNavmeshInput();
            UpdateGraphInput();
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

            if (GameWindowedLook.Pressed)
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
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
            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                DrawPoint(r.PickingResult.Position, 0.25f, Color.Red);
                DrawTriangle(r.PickingResult.Primitive, Color.White);

                float radius = 5;

                DrawCircle(r.PickingResult.Position, radius, Color.Orange);

                var pt = NavigationGraph.FindRandomPoint(agent, r.PickingResult.Position, radius);
                if (pt.HasValue)
                {
                    float dist = Vector3.Distance(r.PickingResult.Position, pt.Value);
                    Color color = dist < radius ? Color.LightGreen : Color.Pink;
                    DrawPoint(pt.Value, 2.5f, color);
                }
            }
        }
        private void UpdateFindRandomPointInput()
        {
            var pt = NavigationGraph.FindRandomPoint(agent);
            if (pt.HasValue)
            {
                DrawPoint(pt.Value, 2.5f, Color.LightGreen);
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

            if (GContacPoint.JustReleased)
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
            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (this.PickNearest(pRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
            {
                DrawContact(r.PickingResult.Position, r.PickingResult.Primitive);

                ToggleTile(r.PickingResult.Position);
            }
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
        private void DrawContact(Vector3 position, Triangle triangle)
        {
            volumesDrawer.Clear(Color.Red);
            volumesDrawer.Clear(Color.Green);
            volumesDrawer.Clear(Color.Gray);
            volumesDrawer.Clear(Color.White);

            bool walkable = IsWalkable(agent, position, 0.1f, out var nearest);
            var pColor = walkable ? Color.Green : Color.Red;

            if (nearest.HasValue)
            {
                DrawPoint(nearest.Value + new Vector3(0.02f), 0.45f, Color.Gray);
            }

            DrawPoint(position, 0.25f, pColor);
            DrawTriangle(triangle, Color.White);
        }
        private void UpdateLoadingText()
        {
            string partition = nmsettings.UseTileCache ? "Using TileCache" : $"Partition {nmsettings.PartitionType}";
            string loading = loadState ?? $"Build Time: {lastElapsedSeconds:0.00000} seconds.";
            debug.Text = $"Build {nmsettings.BuildMode} | {partition} => {loading}";
        }

        private void DrawPoint(Vector3 position, float size, Color color)
        {
            var cross = Line3D.CreateCross(position, size);
            volumesDrawer.SetPrimitives(color, cross);
        }
        private void DrawTriangle(Triangle triangle, Color color)
        {
            var tri = Line3D.CreateWiredTriangle(triangle);
            volumesDrawer.SetPrimitives(color, tri);
        }
        private void DrawCircle(Vector3 position, float radius, Color color)
        {
            var circle = Line3D.CreateCircle(position, radius, 12);
            volumesDrawer.SetPrimitives(color, circle);
        }
        private void DrawGraphNodes(AgentType agent)
        {
            var nodes = GetNodes(agent).OfType<GraphNode>();
            if (nodes.Any())
            {
                graphDrawer.Clear();

                foreach (var node in nodes)
                {
                    graphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
        }

        private void EnqueueGraph()
        {
            lastElapsedSeconds = null;
            loadState = "Updating navigation graph.";

            enqueueTime = DateTime.Now.TimeOfDay;
            EnqueueNavigationGraphUpdate();
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

                DrawGraphNodes(agent);
            }
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
            return @$"Camera: {CamFwd} {CamLeft} {CamBwd} {CamRight} {CamUp} & {CamDown} to move, Mouse To look (Press {GameWindowedLook} mouse in windowed mode). 
{GBuild}: Change Build Mode (SHIFT reverse).
{GPartition}: Change Partition Type (SHIFT reverse).
{GTileCache}: Toggle using Tile Cache.
{GSave}: Saves the graph to a file.
{GLoad}: Loads the graph from a file.
{GContacPoint}: Update current tile (SHIFT remove, CTRL add).
{NmRndPointCircle}: Finds random point around circle (5 units).
{NmRndPoint}: Finds random over navmesh";
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
        }
    }
}
