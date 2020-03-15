using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding.RecastNavigation;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Deferred
{
    public class TestScene3D : Scene
    {
        private const int MaxGridDrawer = 10000;

        private readonly string titleMask = "{0}: {1} directionals, {2} points and {3} spots. Shadows {4}";

        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.10f;
        private const int layerEffects = 2;
        private const int layerHUD = 99;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;
        private TextDrawer statistics = null;

        private Agent tankAgentType = null;
        private GameAgent<SteerManipulatorController> tankAgent1 = null;
        private GameAgent<SteerManipulatorController> tankAgent2 = null;
        private int id1;
        private int id2;
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Scenery terrain = null;

        private Graph graph = null;
        private Crowd crowd = null;

        private Model tree = null;
        private ModelInstanced trees = null;

        private SpriteTexture bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Triangle> terrainGraphDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;

        private bool onlyModels = true;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private readonly List<IUpdatable> agents = new List<IUpdatable>();

        private Guid uiId = Guid.NewGuid();
        private Guid assetsId = Guid.NewGuid();
        private bool gameReady = false;

        public TestScene3D(Game game)
            : base(game, SceneModes.DeferredLightning)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;

            this.Lights.KeyLight.CastShadow = false;
            this.Lights.BackLight.CastShadow = false;
            this.Lights.FillLight.CastShadow = false;

            await InitializeCursor();
            await InitializeUI();

            await this.LoadResourcesAsync(uiId,
                InitializeCursor(),
                InitializeUI()
                );

            await this.LoadResourcesAsync(assetsId,
                InitializeAndTrace(InitializeSkydom),
                InitializeAndTrace(InitializeHelicopters),
                InitializeAndTrace(InitializeTanks),
                InitializeAndTrace(InitializeTerrain),
                InitializeAndTrace(InitializeGardener),
                InitializeAndTrace(InitializeTrees),
                InitializeDebug()
                );
        }
        private async Task<double> InitializeAndTrace(Func<Task> action)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            await action();
            sw.Stop();
            return sw.Elapsed.TotalSeconds;
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = new CursorDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };
            await this.AddComponentCursor(cursorDesc, SceneObjectUsages.UI, layerHUD + 1);
        }
        private async Task InitializeSkydom()
        {
            var desc = new SkydomDescription()
            {
                Name = "Sky",
                ContentPath = "Resources",
                Radius = far,
                Texture = "sunset.dds",
            };
            await this.AddComponentSkydom(desc);
        }
        private async Task InitializeHelicopters()
        {
            var desc1 = new ModelDescription()
            {
                Name = "Helicopter",
                CastShadow = true,
                TextureIndex = 2,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "m24.xml",
                }
            };
            this.helicopter = await this.AddComponentModel(desc1);
            this.Lights.AddRange(this.helicopter.Lights);

            var desc2 = new ModelInstancedDescription()
            {
                Name = "Bunch of Helicopters",
                CastShadow = true,
                Instances = 2,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "m24.xml",
                }
            };
            this.helicopters = await this.AddComponentModelInstanced(desc2);
            for (int i = 0; i < this.helicopters.InstanceCount; i++)
            {
                this.Lights.AddRange(this.helicopters[i].Lights);
            }

            await Task.CompletedTask;
        }
        private async Task InitializeTanks()
        {
            var desc = new ModelDescription()
            {
                Name = "Tank",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "leopard.xml",
                }
            };
            var tank1 = await this.AddComponentModel(desc);
            tank1.Manipulator.SetScale(0.2f, true);
            var tank2 = await this.AddComponentModel(desc);
            tank2.Manipulator.SetScale(0.2f, true);

            var tankController1 = new SteerManipulatorController()
            {
                MaximumForce = 0.5f,
                MaximumSpeed = 7.5f,
                ArrivingRadius = 7.5f,
            };
            var tankController2 = new SteerManipulatorController()
            {
                MaximumForce = 0.5f,
                MaximumSpeed = 7.5f,
                ArrivingRadius = 7.5f,
            };

            var tankbbox = tank1.GetBoundingBox();
            var tanksph = tank1.GetBoundingSphere();
            this.tankAgentType = new Agent()
            {
                Height = tankbbox.GetY(),
                Radius = tanksph.Radius,
                MaxClimb = tankbbox.GetY() * 0.55f,
            };

            this.tankAgent1 = new GameAgent<SteerManipulatorController>(this.tankAgentType, tank1, tankController1);
            this.tankAgent2 = new GameAgent<SteerManipulatorController>(this.tankAgentType, tank2, tankController2);
            agents.Add(this.tankAgent1);
            agents.Add(this.tankAgent2);

            this.Lights.AddRange(this.tankAgent1.Lights);
            this.Lights.AddRange(this.tankAgent2.Lights);
        }
        private async Task InitializeTerrain()
        {
            var desc = new GroundDescription()
            {
                Name = "Terrain",
                Quadtree = new GroundDescription.QuadtreeDescription()
                {
                    MaximumDepth = 2,
                },
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "terrain.xml",
                }
            };
            this.terrain = await this.AddComponentScenery(desc);
        }
        private async Task InitializeGardener()
        {
            var desc = new GroundGardenerDescription()
            {
                ContentPath = "Resources/Vegetation",
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass.png" },
                    Saturation = 20f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = Vector2.One * 0.20f,
                    MaxSize = Vector2.One * 0.25f,
                }
            };
            await this.AddComponentGroundGardener(desc);
        }
        private async Task InitializeTrees()
        {
            var desc1 = new ModelDescription()
            {
                Name = "Lonely tree",
                CastShadow = true,
                AlphaEnabled = true,
                DepthEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/trees",
                    ModelContentFilename = "birch_a.xml",
                }
            };
            this.tree = await this.AddComponentModel(desc1);

            var desc2 = new ModelInstancedDescription()
            {
                Name = "Bunch of trees",
                CastShadow = true,
                AlphaEnabled = true,
                DepthEnabled = true,
                Instances = 10,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/trees",
                    ModelContentFilename = "birch_b.xml",
                }
            };
            this.trees = await this.AddComponentModelInstanced(desc2);
        }
        private async Task InitializeUI()
        {
            var dTitle = TextDrawerDescription.Generate("Tahoma", 18, Color.White);
            var dLoad = TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow);
            var dHelp = TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow);
            var dStats = TextDrawerDescription.Generate("Lucida Casual", 10, Color.Red);

            this.title = await this.AddComponentTextDrawer(dTitle, SceneObjectUsages.UI, layerHUD);
            this.load = await this.AddComponentTextDrawer(dLoad, SceneObjectUsages.UI, layerHUD);
            this.help = await this.AddComponentTextDrawer(dHelp, SceneObjectUsages.UI, layerHUD);
            this.statistics = await this.AddComponentTextDrawer(dStats, SceneObjectUsages.UI, layerHUD);

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, this.title.Top + this.title.Height + 2);
            this.help.Position = new Vector2(0, this.load.Top + this.load.Height + 2);
            this.statistics.Position = new Vector2(0, this.help.Top + this.help.Height + 2);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.statistics.Top + this.statistics.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };
            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeDebug()
        {
            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.bufferDrawer = await this.AddComponentSpriteTexture(
                new SpriteTextureDescription()
                {
                    Left = smLeft,
                    Top = smTop,
                    Width = width,
                    Height = height,
                    Channel = SpriteTextureChannels.NoAlpha,
                },
                SceneObjectUsages.UI,
                layerEffects);
            this.bufferDrawer.Visible = false;

            this.lineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    DepthEnabled = true,
                    Count = 1000,
                },
                SceneObjectUsages.None,
                layerEffects);
            this.lineDrawer.Visible = false;

            this.terrainGraphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Count = MaxGridDrawer,
                },
                SceneObjectUsages.None,
                layerEffects);
            this.terrainGraphDrawer.Visible = false;

            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                DepthEnabled = true,
                Count = 50000,
            };
            this.graphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(graphDrawerDesc);

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                AlphaEnabled = true,
                Count = 10000
            };
            this.volumesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(volumesDrawerDesc);
        }

        public override void GameResourcesLoaded(Guid id)
        {
            if (id == uiId)
            {
                this.title.Text = "Deferred Ligthning test";
                this.help.Text = "";
                this.statistics.Text = "";
            }

            if (id == assetsId)
            {
                this.SetGround(this.terrain, true);
                this.AttachToGround(this.tree, false);
                this.AttachToGround(this.trees, false);

                StartNodes();

                StartAnimations();

                StartTerrain();

                StartItems(out Vector3 cameraPosition, out int modelCount);

                cameraPosition /= (float)modelCount;
                this.Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
                this.Camera.LookTo(cameraPosition + Vector3.Up);
                this.Camera.NearPlaneDistance = near;
                this.Camera.FarPlaneDistance = far;

                var nmsettings = BuildSettings.Default;
                nmsettings.CellSize = 0.5f;
                nmsettings.CellHeight = 0.25f;
                nmsettings.Agents = new[] { this.tankAgentType };
                nmsettings.PartitionType = SamplePartitionTypes.Watershed;
                nmsettings.EdgeMaxError = 1.0f;
                nmsettings.BuildMode = BuildModes.Tiled;
                nmsettings.TileSize = 32;

                var nmInput = new InputGeometry(GetTrianglesForNavigationGraph);

                this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription(nmsettings, nmInput);

                Task.WhenAll(this.UpdateNavigationGraph());

                gameReady = true;
            }
        }
        private void StartNodes()
        {
            var nodes = this.GetNodes(this.tankAgentType).OfType<GraphNode>();
            if (nodes.Any())
            {
                Random clrRnd = new Random(1);
                Color[] regions = new Color[nodes.Count()];
                for (int i = 0; i < nodes.Count(); i++)
                {
                    regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
                }

                foreach (var node in nodes)
                {
                    this.terrainGraphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
        }
        private void StartAnimations()
        {
            var ap = new AnimationPath();
            ap.AddLoop("roll");
            this.animations.Add("default", new AnimationPlan(ap));
        }
        private void StartTerrain()
        {
            if (this.FindTopGroundPosition(20, -20, out PickingResult<Triangle> treePos))
            {
                this.tree.Manipulator.SetPosition(treePos.Position);
                this.tree.Manipulator.SetScale(0.5f);
            }

            for (int i = 0; i < this.trees.InstanceCount; i++)
            {
                if (this.FindTopGroundPosition((i * 10) - 35, 17, out PickingResult<Triangle> pos))
                {
                    this.trees[i].Manipulator.SetScale(0.5f, true);
                    this.trees[i].Manipulator.SetPosition(pos.Position, true);
                }
            }
        }
        private void StartItems(out Vector3 cameraPosition, out int modelCount)
        {
            cameraPosition = Vector3.Zero;
            modelCount = 0;

            if (this.FindTopGroundPosition(20, 40, out PickingResult<Triangle> t1Pos))
            {
                this.tankAgent1.Manipulator.SetPosition(t1Pos.Position);
                this.tankAgent1.Manipulator.SetNormal(t1Pos.Item.Normal);
                cameraPosition += t1Pos.Position;
                modelCount++;
            }

            if (this.FindTopGroundPosition(15, 35, out PickingResult<Triangle> t2Pos))
            {
                this.tankAgent2.Manipulator.SetPosition(t2Pos.Position);
                this.tankAgent2.Manipulator.SetNormal(t2Pos.Item.Normal);
                cameraPosition += t2Pos.Position;
                modelCount++;
            }

            if (this.FindTopGroundPosition(20, -20, out PickingResult<Triangle> hPos))
            {
                var p = hPos.Position;
                p.Y += 10f;
                this.helicopter.Manipulator.SetPosition(p, true);
                this.helicopter.Manipulator.SetScale(0.15f, true);
                cameraPosition += p;
                modelCount++;
            }

            this.helicopter.AnimationController.AddPath(this.animations["default"]);
            this.helicopter.AnimationController.TimeDelta = 3f;
            this.helicopter.AnimationController.Start();

            for (int i = 0; i < this.helicopters.InstanceCount; i++)
            {
                if (this.FindTopGroundPosition((i * 10) - 20, 20, out PickingResult<Triangle> r))
                {
                    var p = r.Position;
                    p.Y += 10f;
                    this.helicopters[i].Manipulator.SetPosition(p, true);
                    this.helicopters[i].Manipulator.SetScale(0.15f, true);
                    cameraPosition += p;
                    modelCount++;
                }

                this.helicopters[i].AnimationController.AddPath(this.animations["default"]);
                this.helicopters[i].AnimationController.TimeDelta = 3f;
                this.helicopters[i].AnimationController.Start();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            this.agents.ForEach(a => a.Update(new Engine.Common.UpdateContext() { GameTime = gameTime }));

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            UpdateInputCamera(gameTime, shift);
            UpdateInputMouse();
            UpdayeInputLights(shift);
            UpdateInputObjectsVisibility();
            UpdateInputHelicopterTexture();
            UpdateInputGraph();
            UpdateInputDebug(shift);

            UpdateLights(gameTime);

            if (crowd == null)
            {
                return;
            }

            var cag1 = crowd.GetAgent(id1);
            var cag2 = crowd.GetAgent(id2);

            this.tankAgent1.Manipulator.SetPosition(cag1.NPos);
            this.tankAgent2.Manipulator.SetPosition(cag2.NPos);
        }
        private void UpdateInputCamera(GameTime gameTime, bool shift)
        {
#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                gameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.Space))
            {
                this.lineDrawer.SetPrimitives(Color.Yellow, Line3D.CreateWiredFrustum(this.Camera.Frustum));
                this.lineDrawer.Visible = true;
            }
        }
        private void UpdateInputMouse()
        {
            if (this.Game.Input.LeftMouseButtonJustReleased)
            {
                var pRay = this.GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (this.PickNearest(pRay, rayPParams, out PickingResult<Triangle> r))
                {
                    var tri = Line3D.CreateWiredTriangle(r.Item);
                    this.volumesDrawer.SetPrimitives(Color.White, tri);

                    var cross = Line3D.CreateCross(r.Position, 0.25f);
                    this.volumesDrawer.SetPrimitives(Color.Red, cross);

                    graph.RequestMoveCrowd(crowd, tankAgentType, r.Position);
                }
            }
        }
        private void UpdayeInputLights(bool shift)
        {
            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.Lights.BaseFogColor = new Color((byte)54, (byte)56, (byte)68);
                this.Lights.FogStart = this.Lights.FogStart == 0f ? far * fogStart : 0f;
                this.Lights.FogRange = this.Lights.FogRange == 0f ? far * fogRange : 0f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.onlyModels = !this.onlyModels;

                this.CreateLights(this.onlyModels, !shift);
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.animateLights = !this.animateLights;
            }
        }
        private void UpdateInputObjectsVisibility()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.bufferDrawer.Visible = !this.bufferDrawer.Visible;
                this.help.Visible = this.bufferDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.terrain.Active = this.terrain.Visible = !this.terrain.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                this.tankAgent1.Active = this.tankAgent1.Visible = !this.tankAgent1.Visible;
                this.tankAgent2.Active = this.tankAgent2.Visible = this.tankAgent1.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F10))
            {
                this.helicopter.Active = this.helicopter.Visible = !this.helicopter.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F11))
            {
                this.helicopters.Active = this.helicopters.Visible = !this.helicopters.Visible;
            }
        }
        private void UpdateInputHelicopterTexture()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Oemcomma))
            {
                this.textIntex--;
            }

            if (this.Game.Input.KeyJustReleased(Keys.OemPeriod))
            {
                this.textIntex++;
            }

            if (this.Game.Input.KeyJustReleased(Keys.T))
            {
                this.helicopter.TextureIndex++;

                if (this.helicopter.TextureIndex >= this.helicopter.TextureCount)
                {
                    //Loop
                    this.helicopter.TextureIndex = 0;
                }
            }
        }
        private void UpdateInputGraph()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.terrainGraphDrawer.Visible = !this.terrainGraphDrawer.Visible;
            }
        }
        private void UpdateInputDebug(bool shift)
        {
            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.UpdateDebugColorMap();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.UpdateDebugNormalMap();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.UpdateDebugDepthMap();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.UpdateDebugShadowMap(shift);
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.UpdateDebugLightMap();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F12))
            {
                this.UpdateDebugBufferDrawer();
            }
        }

        private void UpdateLights(GameTime gameTime)
        {
            if (this.spotLight != null)
            {
                this.UpdateSpotlight(gameTime);

                this.lineDrawer.SetPrimitives(Color.White, this.spotLight.GetVolume(10));
            }
            else
            {
                this.lineDrawer.Visible = false;
            }

            if (this.animateLights && this.Lights.PointLights.Length > 0)
            {
                this.UpdatePointLightsAnimation(gameTime);
            }
        }
        private void UpdateSpotlight(GameTime gameTime)
        {
            if (this.Game.Input.KeyPressed(Keys.Left))
            {
                var v = this.Camera.Left;
                v.Y = 0;
                v.Normalize();
                this.spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.Right))
            {
                var v = this.Camera.Right;
                v.Y = 0;
                v.Normalize();
                this.spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.Up))
            {
                var v = this.Camera.Forward;
                v.Y = 0;
                v.Normalize();
                this.spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.Down))
            {
                var v = this.Camera.Backward;
                v.Y = 0;
                v.Normalize();
                this.spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.PageUp))
            {
                this.spotLight.Position += (Vector3.Up) * gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.PageDown))
            {
                this.spotLight.Position += (Vector3.Down) * gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                this.spotLight.Intensity += gameTime.ElapsedSeconds * 10f;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                this.spotLight.Intensity -= gameTime.ElapsedSeconds * 10f;

                this.spotLight.Intensity = Math.Max(0f, this.spotLight.Intensity);
            }
        }
        private void UpdatePointLightsAnimation(GameTime gameTime)
        {
            for (int i = 1; i < this.Lights.PointLights.Length; i++)
            {
                var l = this.Lights.PointLights[i];

                if ((int?)l.State == 1) l.Radius += (0.5f * gameTime.ElapsedSeconds * 50f);
                if ((int?)l.State == -1) l.Radius -= (2f * gameTime.ElapsedSeconds * 50f);

                if (l.Radius <= 0)
                {
                    l.Radius = 0;
                    l.State = 1;
                }

                if (l.Radius >= 50)
                {
                    l.Radius = 50;
                    l.State = -1;
                }

                l.Intensity = l.Radius * 0.1f;
            }
        }

        private void UpdateDebugColorMap()
        {
            var colorMap = this.Renderer.GetResource(SceneRendererResults.ColorMap);

            //Colors
            this.bufferDrawer.Texture = colorMap;
            this.bufferDrawer.Channels = SpriteTextureChannels.NoAlpha;
            this.help.Text = "Colors";

            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugNormalMap()
        {
            var normalMap = this.Renderer.GetResource(SceneRendererResults.NormalMap);

            if (this.bufferDrawer.Texture == normalMap &&
                this.bufferDrawer.Channels == SpriteTextureChannels.NoAlpha)
            {
                //Specular Power
                this.bufferDrawer.Texture = normalMap;
                this.bufferDrawer.Channels = SpriteTextureChannels.Alpha;
                this.help.Text = "Specular Power";
            }
            else
            {
                //Normals
                this.bufferDrawer.Texture = normalMap;
                this.bufferDrawer.Channels = SpriteTextureChannels.NoAlpha;
                this.help.Text = "Normals";
            }
            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugDepthMap()
        {
            var depthMap = this.Renderer.GetResource(SceneRendererResults.DepthMap);

            if (this.bufferDrawer.Texture == depthMap &&
                this.bufferDrawer.Channels == SpriteTextureChannels.NoAlpha)
            {
                //Specular Factor
                this.bufferDrawer.Texture = depthMap;
                this.bufferDrawer.Channels = SpriteTextureChannels.Alpha;
                this.help.Text = "Specular Intensity";
            }
            else
            {
                //Position
                this.bufferDrawer.Texture = depthMap;
                this.bufferDrawer.Channels = SpriteTextureChannels.NoAlpha;
                this.help.Text = "Position";
            }
            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugShadowMap(bool shift)
        {
            var shadowMap = this.Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);

            if (shadowMap != null)
            {
                //Shadow map
                if (!this.help.Text.StartsWith("Shadow map"))
                {
                    this.bufferDrawer.Texture = shadowMap;
                    this.bufferDrawer.TextureIndex = 0;
                    this.bufferDrawer.Channels = SpriteTextureChannels.Red;
                    this.bufferDrawer.Visible = true;
                }
                else
                {
                    int tIndex = this.bufferDrawer.TextureIndex;
                    if (!shift)
                    {
                        tIndex++;
                        tIndex %= 6;
                    }
                    else
                    {
                        tIndex--;
                        if (tIndex < 0)
                        {
                            tIndex = 5;
                        }
                    }

                    this.bufferDrawer.TextureIndex = tIndex;
                }

                this.help.Text = string.Format("Shadow map {0}", this.bufferDrawer.TextureIndex);
            }
            else
            {
                this.help.Text = "The Shadow map is empty";
            }
        }
        private void UpdateDebugLightMap()
        {
            var lightMap = this.Renderer.GetResource(SceneRendererResults.LightMap);

            if (lightMap != null)
            {
                //Light map
                this.bufferDrawer.Texture = lightMap;
                this.bufferDrawer.Channels = SpriteTextureChannels.NoAlpha;
                this.bufferDrawer.Visible = true;
                this.help.Text = "Light map";
            }
            else
            {
                this.help.Text = "The Light map is empty";
            }
        }
        private void UpdateDebugBufferDrawer()
        {
            if (this.bufferDrawer.Manipulator.Position == Vector2.Zero)
            {
                int width = (int)(this.Game.Form.RenderWidth * 0.33f);
                int height = (int)(this.Game.Form.RenderHeight * 0.33f);
                int smLeft = this.Game.Form.RenderWidth - width;
                int smTop = this.Game.Form.RenderHeight - height;

                this.bufferDrawer.Manipulator.SetPosition(smLeft, smTop);
                this.bufferDrawer.ResizeSprite(width, height);
            }
            else
            {
                this.bufferDrawer.Manipulator.SetPosition(Vector2.Zero);
                this.bufferDrawer.ResizeSprite(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.Game.Form.IsFullscreen)
            {
                this.load.Text = this.Game.RuntimeText;
            }

            this.title.Text = string.Format(
                this.titleMask,
                this.GetRenderMode(),
                this.Lights.DirectionalLights.Length,
                this.Lights.PointLights.Length,
                this.Lights.SpotLights.Length,
                this.Lights.GetDirectionalShadowCastingLights().Count());

            if (Counters.Statistics.Length == 0)
            {
                this.statistics.Text = "No statistics";
            }
            else if (this.textIntex < 0)
            {
                this.statistics.Text = "Press . for more statistics";
                this.textIntex = -1;
            }
            else if (this.textIntex >= Counters.Statistics.Length)
            {
                this.statistics.Text = "Press , for more statistics";
                this.textIntex = Counters.Statistics.Length;
            }
            else
            {
                this.statistics.Text = string.Format(
                    "{0} - {1}",
                    Counters.Statistics[this.textIntex],
                    Counters.GetStatistics(this.textIntex));
            }
        }

        private void CreateLights(bool modelsOnly, bool castShadows)
        {
            this.Lights.ClearPointLights();
            this.Lights.ClearSpotLights();
            this.spotLight = null;

            this.Lights.AddRange(this.tankAgent1.Lights);
            this.Lights.AddRange(this.tankAgent2.Lights);
            this.Lights.AddRange(this.helicopter.Lights);
            for (int i = 0; i < this.helicopters.InstanceCount; i++)
            {
                this.Lights.AddRange(this.helicopters[i].Lights);
            }

            if (!modelsOnly)
            {
                this.SetLightsPosition(castShadows);

                int sep = 10;
                int f = 12;
                int l = (f - 1) * sep;
                l -= (l / 2);

                for (int i = 0; i < f; i++)
                {
                    for (int x = 0; x < f; x++)
                    {
                        Vector3 lightPosition = new Vector3((i * sep) - l, 1f, (x * sep) - l);

                        if (this.FindTopGroundPosition((i * sep) - l, (x * sep) - l, out PickingResult<Triangle> r))
                        {
                            lightPosition = r.Position;
                            lightPosition.Y += 1f;
                        }

                        var color = new Color4(Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1), 1.0f);

                        var pointLight = new SceneLightPoint(
                            string.Format("Point {0}", this.Lights.PointLights.Length),
                            castShadows,
                            color,
                            color,
                            true,
                            SceneLightPointDescription.Create(lightPosition, 5f, 10f))
                        {
                            State = Helper.RandomGenerator.NextFloat(0, 1) >= 0.5f ? 1 : -1
                        };

                        this.Lights.Add(pointLight);
                    }
                }
            }
        }
        private void SetLightsPosition(bool castShadows)
        {
            if (this.FindTopGroundPosition(0, 1, out PickingResult<Triangle> r))
            {
                var lightPosition = r.Position;
                lightPosition.Y += 10f;

                this.spotLight = new SceneLightSpot(
                    "Spot the dog",
                    castShadows,
                    Color.Yellow,
                    Color.Yellow,
                    true,
                    SceneLightSpotDescription.Create(lightPosition, Vector3.Down, 25, 25, 25f));

                this.Lights.Add(this.spotLight);

                this.lineDrawer.Active = true;
                this.lineDrawer.Visible = true;
            }
        }


        public override void NavigationGraphUpdated()
        {
            this.UpdateGraphNodes(this.tankAgentType);

            graph = this.NavigationGraph as Graph;
            if (graph == null)
            {
                return;
            }

            crowd = graph.AddCrowd(10, tankAgentType);

            var par = new CrowdAgentParams()
            {
                Radius = tankAgentType.Radius,
                Height = tankAgentType.Height,
                MaxAcceleration = 8,
                MaxSpeed = 3.5f,
                CollisionQueryRange = tankAgentType.Radius * 12,
                PathOptimizationRange = tankAgentType.Radius * 30,
                UpdateFlags = UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE,
                ObstacleAvoidanceType = 0,
                SeparationWeight = 2,
            };

            id1 = crowd.AddAgent(tankAgent1.Manipulator.Position, par);
            id2 = crowd.AddAgent(tankAgent2.Manipulator.Position, par);
        }
        private void UpdateGraphNodes(Agent agent)
        {
            try
            {
                var nodes = this.GetNodes(agent).OfType<GraphNode>();
                if (nodes.Any())
                {
                    this.graphDrawer.Clear();

                    foreach (var node in nodes)
                    {
                        this.graphDrawer.AddPrimitives(node.Color, node.Triangles);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
