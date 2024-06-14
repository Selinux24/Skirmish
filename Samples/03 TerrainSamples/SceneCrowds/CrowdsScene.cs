using Engine;
using Engine.Animation;
using Engine.BuiltIn.PostProcess;
using Engine.Collada;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneCrowds
{
    public class CrowdsScene : WalkableScene
    {
        private const string resourceCursor = "Common/UI/Cursor/target.png";
        private const string resourceSkybox = "Common/Skyboxes/sunset.dds";
        private const string resourceTankFolder = "Common/Agents/Leopard/";
        private const string resourceTankFile = "leopard.json";
        private const string resourceTerrainFolder = "Common/Terrain/Basic/";
        private const string resourceTerrainFile = "terrain.json";
        private const string resourceTreesFolder = "Common/Trees/Birch/";
        private const string resourceTreeBirchAFile = "birch_a.json";
        private const string resourceTreeBirchBFile = "birch_b.json";

        private const int MaxGridDrawer = 10000;

        private const float near = 0.1f;
        private const float far = 1000f;

        private UITextArea title = null;
        private UITextArea help = null;
        private Sprite upperPanel = null;

        private GraphAgentType tankAgentType = null;
        private readonly List<GameAgent<GraphAgentType, SteerManipulatorController>> tankAgents = [];

        private readonly GroupManager<CrowdAgentSettings> crowdManager = new();
        private Crowd crowd = null;

        private Model tree = null;
        private ModelInstanced trees = null;

        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Triangle> terrainGraphDrawer = null;

        private readonly Dictionary<string, AnimationPlan> animations = [];

        private bool objectsReady = false;
        private bool gameReady = false;

        private readonly BuiltInPostProcessState postProcessingState = BuiltInPostProcessState.Empty;

        public CrowdsScene(Game game)
            : base(game)
        {
            Game.VisibleMouse = false;
            Game.LockMouse = false;
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeUIComponents();
        }

        private void InitializeUIComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeCursor,
                    InitializeUI,
                ],
                InitializeUIComponentsComplete);

            LoadResources(group);
        }
        private async Task InitializeCursor()
        {
            var desc = UICursorDescription.Default(resourceCursor, 15, 15, true);

            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", desc);
        }
        private async Task InitializeUI()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            var defaultFont10 = TextDrawerDescription.FromFamily("Tahoma", 10);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;
            defaultFont10.LineAdjust = true;

            var dTitle = new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White };
            var dHelp = new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow, MaxTextLength = 128 };

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", dTitle);
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", dHelp);

            upperPanel = await AddComponentUI<Sprite, SpriteDescription>("Upperpanel", "Upperpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), LayerUI - 1);
        }
        private void InitializeUIComponentsComplete(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            title.Text = "Crowds";
            help.Text = "Loading...";

            UpdateLayout();

            InitializeSceneComponents();
        }

        private void InitializeSceneComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeSkydom,
                    InitializeTanks,
                    InitializeTerrain,
                    InitializeTrees,
                    InitializeDebug,
                ],
                InitializeSceneComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeSkydom()
        {
            var desc = SkydomDescription.Sphere(resourceSkybox, far);

            await AddComponentSky<Skydom, SkydomDescription>("Sky", "Sky", desc);
        }
        private async Task InitializeTanks()
        {
            var desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(resourceTankFolder, resourceTankFile),
                Instances = 5,
            };
            var tanks = await AddComponentAgent<ModelInstanced, ModelInstancedDescription>("Tanks", "Tanks", desc);

            tanks[0].Manipulator.SetScaling(0.2f);
            var tankbbox = tanks[0].GetBoundingBox();

            tankAgentType = new GraphAgentType()
            {
                Height = tankbbox.Height,
                Radius = MathF.Max(tankbbox.Width, tankbbox.Depth) * 0.5f,
                MaxClimb = tankbbox.Height * 0.55f,
                PathFilter = new CrowdQueryFilter(),
            };

            for (int i = 0; i < tanks.InstanceCount; i++)
            {
                InitializeTank(tanks[i]);
            }
        }
        private void InitializeTank(ModelInstance tank)
        {
            tank.Manipulator.SetScaling(0.2f);

            var tankController = new SteerManipulatorController()
            {
                MaximumForce = 0.5f,
                MaximumSpeed = 7.5f,
                ArrivingRadius = 7.5f,
            };

            var tankAgent = new GameAgent<GraphAgentType, SteerManipulatorController>(
                this,
                $"tankAgent.{tank.Id}",
                $"tankAgent",
                tankAgentType,
                tankController,
                tank);

            tankAgents.Add(tankAgent);

            Lights.AddRange(tankAgent.Lights);
        }
        private async Task InitializeTerrain()
        {
            var desc = GroundDescription.FromFile(resourceTerrainFolder, resourceTerrainFile, 2);

            await AddComponentGround<Scenery, GroundDescription>("Terrain", "Terrain", desc);
        }
        private async Task InitializeTrees()
        {
            var desc1 = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.OpaqueTransparent,
                PathFindingHull = PickingHullTypes.Hull,
                Content = ContentDescription.FromFile(resourceTreesFolder, resourceTreeBirchAFile),
            };
            tree = await AddComponentGround<Model, ModelDescription>("Lonely tree", "Lonely tree", desc1);

            var desc2 = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.OpaqueTransparent,
                Instances = 10,
                PathFindingHull = PickingHullTypes.Hull,
                Content = ContentDescription.FromFile(resourceTreesFolder, resourceTreeBirchBFile),
            };
            trees = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Bunch of trees", "Bunch of trees", desc2);
        }
        private async Task InitializeDebug()
        {
            var lineDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 1000,
                BlendMode = BlendModes.Alpha,
                StartsVisible = true,
            };
            lineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Lines", "DEBUG++ Lines", lineDrawerDesc);

            var terrainGraphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = MaxGridDrawer,
                BlendMode = BlendModes.Alpha,
                StartsVisible = false,
            };
            terrainGraphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Terrain Graph", "DEBUG++ Terrain Graph", terrainGraphDrawerDesc);
        }
        private void InitializeSceneComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                help.Text = res.GetErrorMessage();

                return;
            }

            postProcessingState.AddToneMapping(BuiltInToneMappingTones.Uncharted2);
            Renderer.ClearPostProcessingEffects();
            Renderer.PostProcessingObjectsEffects = postProcessingState;

            StartNodes();
            StartAnimations();
            StartTerrain();
            StartItems(out Vector3 cameraPosition, out int modelCount);

            objectsReady = true;

            cameraPosition /= modelCount;
            Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            Camera.LookTo(cameraPosition + Vector3.Up);
            Camera.NearPlaneDistance = near;
            Camera.FarPlaneDistance = far;

            var nmsettings = BuildSettings.Default;
            nmsettings.CellSize = 0.5f;
            nmsettings.CellHeight = 1f;
            nmsettings.PartitionType = SamplePartitionTypes.Layers;
            nmsettings.EdgeMaxError = 1.0f;
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 16;

            var nmInput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new(nmsettings, nmInput, [tankAgentType]);

            EnqueueNavigationGraphUpdate(
                (loaded) =>
                {
                    if (!loaded) return;

                    NavigationGraphLoaded();
                },
                (progress) =>
                {
                    help.Text = $"Loading navigation mesh {progress:0.0%}...";
                });
        }
        private void StartNodes()
        {
            terrainGraphDrawer.Clear();

            var nodes = GetNodes(tankAgentType).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return;
            }

            var clrRnd = Helper.NewGenerator(1);
            Color[] regions = new Color[nodes.Count()];
            for (int i = 0; i < nodes.Count(); i++)
            {
                regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
            }

            foreach (var node in nodes)
            {
                var color = Helper.IntToCol(node.Id, 128);
                terrainGraphDrawer.AddPrimitives(color, node.Triangles);
            }
        }
        private void StartAnimations()
        {
            var ap = new AnimationPath();
            ap.AddLoop("roll");
            animations.Add("default", new AnimationPlan(ap));
        }
        private void StartTerrain()
        {
            if (FindTopGroundPosition<Triangle>(20, -20, out var treePos))
            {
                tree.Manipulator.SetTransform(treePos.Position, Quaternion.Identity, 0.5f);
            }

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - 35, 17, out var pos))
                {
                    trees[i].Manipulator.SetTransform(pos.Position, Vector3.Up, i, 0.5f);
                }
            }
        }
        private void StartItems(out Vector3 cameraPosition, out int modelCount)
        {
            cameraPosition = Vector3.Zero;
            modelCount = 0;

            for (int i = 0; i < tankAgents.Count; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - (tankAgents.Count * 10 / 2), 40, out var t1Pos))
                {
                    tankAgents[i].Manipulator.SetPosition(t1Pos.Position);
                    tankAgents[i].Manipulator.SetNormal(t1Pos.Primitive.Normal);
                    cameraPosition += t1Pos.Position;
                    modelCount++;
                }
            }
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!objectsReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateInputCamera(gameTime);

            if (!gameReady)
            {
                return;
            }

            crowdManager.Update(gameTime);

            UpdateInputMouse();
            UpdateInputDebug();

            UpdateAgents(gameTime);
            UpdateDebugProximityGridDrawer();
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInputMouse()
        {
            if (!Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                return;
            }

            var pRay = GetPickingRay(PickingHullTypes.Perfect);

            if (!this.PickNearest<Triangle>(pRay, SceneObjectUsages.None, out var r))
            {
                return;
            }

            var tri = Line3D.CreateTriangle(r.PickingResult.Primitive);
            var cross = Line3D.CreateCross(r.PickingResult.Position, 0.25f);

            lineDrawer.SetPrimitives(Color.White, tri);
            lineDrawer.SetPrimitives(Color.Red, cross);

            if (Game.Input.ShiftPressed)
            {
                crowd.RequestMove(tankAgents[0].CrowdAgentId, r.PickingResult.Position);
            }
            else
            {
                crowd.RequestMove(r.PickingResult.Position);
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                terrainGraphDrawer.Visible = !terrainGraphDrawer.Visible;
            }
        }

        private void UpdateAgents(IGameTime gameTime)
        {
            tankAgents.ForEach(a => a.Update(new UpdateContext() { GameTime = gameTime }));

            if (crowd == null)
            {
                return;
            }

            for (int i = 0; i < tankAgents.Count; i++)
            {
                var aPos = crowd.GetPosition(tankAgents[i].CrowdAgentId);
                var pPos = tankAgents[i].Manipulator.Position;

                if (Vector3.NearEqual(aPos, pPos, new Vector3(0.001f)))
                {
                    continue;
                }

                var tDir = aPos - pPos;
                tankAgents[i].Manipulator.SetPosition(aPos);
                tankAgents[i].Manipulator.RotateTo(aPos + tDir, Axis.Y, 0.1f);
            }
        }
        private void UpdateDebugProximityGridDrawer()
        {
            if (crowd == null)
            {
                return;
            }

            var lines = new List<Line3D>();

            var grid = crowd.GetGrid();

            var rect = grid.GetBounds();

            var c0 = new Vector2(rect.Left, rect.Top);
            var c1 = new Vector2(rect.Right, rect.Top);
            var c2 = new Vector2(rect.Right, rect.Bottom);
            var c3 = new Vector2(rect.Left, rect.Bottom);
            var ct = rect.Center;

            FindFirstGroundPosition<Triangle>(c0.X, c0.Y, out var r0);
            FindFirstGroundPosition<Triangle>(c1.X, c1.Y, out var r1);
            FindFirstGroundPosition<Triangle>(c2.X, c2.Y, out var r2);
            FindFirstGroundPosition<Triangle>(c3.X, c3.Y, out var r3);
            FindFirstGroundPosition<Triangle>(ct.X, ct.Y, out var rt);

            lines.AddRange(Line3D.CreateSquare([r0.Position, r1.Position, r2.Position, r3.Position]));

            float r = Vector3.Distance(r0.Position, r2.Position) * 0.5f;
            grid.QueryItems(rt.Position, r, out var items);
            foreach (var item in items)
            {
                lines.AddRange(Line3D.CreateCircle(item.RealPosition, item.Radius, 32));
            }

            lines.AddRange(Line3D.CreateCircle(rt.Position, r, 64));

            lineDrawer.SetPrimitives(Color.Orange, lines);
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            help.SetPosition(new Vector2(0, title.Top + title.Height + 2));

            upperPanel.Width = Game.Form.RenderWidth;
            upperPanel.Height = help.Top + help.Height + 3;
        }

        public void NavigationGraphLoaded()
        {
            help.Text = "Point & click over terrain to move the crowd. Press F1 to show the Navigation mesh.";

            UpdateGraphNodes(tankAgentType);

            if (NavigationGraph is not Graph nGraph)
            {
                return;
            }

            crowd = new(nGraph, new(tankAgentType, tankAgents.Count));

            for (int i = 0; i < tankAgents.Count; i++)
            {
                tankAgents[i].CrowdAgentId = crowd.AddAgent(tankAgents[i].Manipulator.Position);
            }

            crowdManager.Add(crowd);

            gameReady = true;
        }
        private void UpdateGraphNodes(GraphAgentType agent)
        {
            terrainGraphDrawer.Clear();

            var nodes = GetNodes(agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return;
            }

            foreach (var node in nodes)
            {
                var color = Helper.IntToCol(node.Id, 128);
                terrainGraphDrawer.AddPrimitives(color, node.Triangles);
            }
        }
    }
}
