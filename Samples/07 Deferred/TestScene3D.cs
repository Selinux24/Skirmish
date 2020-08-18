using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding.RecastNavigation;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Engine.UI;
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

        private UITextArea title = null;
        private UITextArea load = null;
        private UITextArea help = null;
        private UITextArea statistics = null;

        private Agent tankAgentType = null;
        private readonly List<GameAgent<SteerManipulatorController>> tankAgents = new List<GameAgent<SteerManipulatorController>>();
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Scenery terrain = null;

        private Graph graph = null;
        private Crowd crowd = null;

        private Model tree = null;
        private ModelInstanced trees = null;

        private UITextureRenderer bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Triangle> terrainGraphDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;

        private bool onlyModels = true;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

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

            _ = this.LoadResourcesAsync(
                new[]
                {
                    InitializeCursor(),
                    InitializeUI()
                },
                () =>
                {
                    this.title.Text = "Deferred Ligthning test";
                    this.help.Text = "";
                    this.statistics.Text = "";

                    _ = this.LoadResourcesAsync(
                        new[]
                        {
                            InitializeAndTrace(InitializeSkydom),
                            InitializeAndTrace(InitializeHelicopters),
                            InitializeAndTrace(InitializeTanks),
                            InitializeAndTrace(InitializeTerrain),
                            InitializeAndTrace(InitializeGardener),
                            InitializeAndTrace(InitializeTrees),
                            InitializeDebug()
                        },
                        () =>
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
                            nmsettings.PartitionType = SamplePartitionTypes.Layers;
                            nmsettings.EdgeMaxError = 1.0f;
                            nmsettings.BuildMode = BuildModes.Tiled;
                            nmsettings.TileSize = 32;

                            var nmInput = new InputGeometry(GetTrianglesForNavigationGraph);

                            this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription(nmsettings, nmInput);

                            Task.WhenAll(this.UpdateNavigationGraph());

                            gameReady = true;
                        });
                });
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
            var cursorDesc = new UICursorDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };
            await this.AddComponentUICursor(cursorDesc, layerHUD + 1);
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
            var desc = new ModelInstancedDescription()
            {
                Name = "Tanks",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "Resources",
                    ModelContentFilename = "leopard.xml",
                },
                Instances = 5,
            };
            var tanks = await this.AddComponentModelInstanced(desc);

            tanks[0].Manipulator.SetScale(0.2f, true);
            var tankbbox = tanks[0].GetBoundingBox();

            this.tankAgentType = new Agent()
            {
                Height = tankbbox.Height,
                Radius = Math.Max(tankbbox.Width, tankbbox.Depth) * 0.5f,
                MaxClimb = tankbbox.Height * 0.55f,
            };

            for (int i = 0; i < tanks.InstanceCount; i++)
            {
                InitializeTank(tanks[i]);
            }
        }
        private void InitializeTank(ModelInstance tank)
        {
            tank.Manipulator.SetScale(0.2f, true);

            var tankController = new SteerManipulatorController()
            {
                MaximumForce = 0.5f,
                MaximumSpeed = 7.5f,
                ArrivingRadius = 7.5f,
            };

            var tankAgent = new GameAgent<SteerManipulatorController>(this.tankAgentType, tank, tankController);

            tankAgents.Add(tankAgent);

            this.Lights.AddRange(tankAgent.Lights);
        }
        private async Task InitializeTerrain()
        {
            this.terrain = await this.AddComponentScenery(GroundDescription.FromFile("Resources", "terrain.xml", 2));
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
                DepthEnabled = true,
                BlendMode = BlendModes.DefaultTransparent,
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
                DepthEnabled = true,
                BlendMode = BlendModes.DefaultTransparent,
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
            var dTitle = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) };
            var dLoad = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) };
            var dHelp = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) };
            var dStats = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 10, Color.Red) };

            this.title = await this.AddComponentUITextArea(dTitle, layerHUD);
            this.load = await this.AddComponentUITextArea(dLoad, layerHUD);
            this.help = await this.AddComponentUITextArea(dHelp, layerHUD);
            this.statistics = await this.AddComponentUITextArea(dStats, layerHUD);

            this.title.SetPosition(Vector2.Zero);
            this.load.SetPosition(new Vector2(0, this.title.Top + this.title.Height + 2));
            this.help.SetPosition(new Vector2(0, this.load.Top + this.load.Height + 2));
            this.statistics.SetPosition(new Vector2(0, this.help.Top + this.help.Height + 2));

            var spDesc = new SpriteDescription()
            {
                Width = this.Game.Form.RenderWidth,
                Height = this.statistics.Top + this.statistics.Height + 3,
                TintColor = new Color4(0, 0, 0, 0.75f),
            };
            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeDebug()
        {
            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.bufferDrawer = await this.AddComponentUITextureRenderer(
                new UITextureRendererDescription()
                {
                    Left = 0,
                    Top = 0,
                    Width = this.Game.Form.RenderWidth,
                    Height = this.Game.Form.RenderHeight,
                    Channel = UITextureRendererChannels.NoAlpha,
                },
                layerEffects);
            this.bufferDrawer.Visible = false;
            this.bufferDrawer.Scale = 0.33f;
            this.bufferDrawer.Left = smLeft;
            this.bufferDrawer.Top = smTop;

            this.lineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    DepthEnabled = false,
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
                DepthEnabled = true,
                Count = 50000,
            };
            this.graphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(graphDrawerDesc);

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 10000
            };
            this.volumesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(volumesDrawerDesc);
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

            for (int i = 0; i < tankAgents.Count; i++)
            {
                if (this.FindTopGroundPosition((i * 10) - (tankAgents.Count * 10 / 2), 40, out PickingResult<Triangle> t1Pos))
                {
                    tankAgents[i].Manipulator.SetPosition(t1Pos.Position);
                    tankAgents[i].Manipulator.SetNormal(t1Pos.Item.Normal);
                    cameraPosition += t1Pos.Position;
                    modelCount++;
                }
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

            this.tankAgents.ForEach(a => a.Update(new Engine.Common.UpdateContext() { GameTime = gameTime }));

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            UpdateInputCamera(gameTime, shift);
            UpdateInputMouse(shift);
            UpdayeInputLights(shift);
            UpdateInputObjectsVisibility();
            UpdateInputHelicopterTexture();
            UpdateInputGraph();
            UpdateInputDebug(shift);

            UpdateDebugProximityGridDrawer();

            UpdateLights(gameTime);
            UpdateTanks();
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
        private void UpdateInputMouse(bool shift)
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

                    if (shift)
                    {
                        graph.RequestMoveAgent(crowd, tankAgents[0].CrowdAgent, tankAgentType, r.Position);
                    }
                    else
                    {
                        graph.RequestMoveCrowd(crowd, tankAgentType, r.Position);
                    }
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
                this.tankAgents[0].Active = this.tankAgents[0].Visible = !this.tankAgents[0].Visible;

                for (int i = 1; i < tankAgents.Count; i++)
                {
                    this.tankAgents[i].Active = this.tankAgents[i].Visible = this.tankAgents[0].Visible;
                }
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
        private void UpdateTanks()
        {
            if (crowd == null)
            {
                return;
            }

            for (int i = 0; i < tankAgents.Count; i++)
            {
                var cag = tankAgents[i].CrowdAgent;
                var pPos = tankAgents[i].Manipulator.Position;

                if (!Vector3.NearEqual(cag.NPos, pPos, new Vector3(0.001f)))
                {
                    var tDir = cag.NPos - pPos;
                    tankAgents[i].Manipulator.SetPosition(cag.NPos);
                    tankAgents[i].Manipulator.LookAt(cag.NPos + tDir);
                }
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
                this.lineDrawer.Clear(Color.White);
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
            this.bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
            this.help.Text = "Colors";

            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugNormalMap()
        {
            var normalMap = this.Renderer.GetResource(SceneRendererResults.NormalMap);

            if (this.bufferDrawer.Texture == normalMap &&
                this.bufferDrawer.Channels == UITextureRendererChannels.NoAlpha)
            {
                //Specular Power
                this.bufferDrawer.Texture = normalMap;
                this.bufferDrawer.Channels = UITextureRendererChannels.Alpha;
                this.help.Text = "Specular Power";
            }
            else
            {
                //Normals
                this.bufferDrawer.Texture = normalMap;
                this.bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
                this.help.Text = "Normals";
            }
            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugDepthMap()
        {
            var depthMap = this.Renderer.GetResource(SceneRendererResults.DepthMap);

            if (this.bufferDrawer.Texture == depthMap &&
                this.bufferDrawer.Channels == UITextureRendererChannels.NoAlpha)
            {
                //Specular Factor
                this.bufferDrawer.Texture = depthMap;
                this.bufferDrawer.Channels = UITextureRendererChannels.Alpha;
                this.help.Text = "Specular Intensity";
            }
            else
            {
                //Position
                this.bufferDrawer.Texture = depthMap;
                this.bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
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
                    this.bufferDrawer.Channels = UITextureRendererChannels.Red;
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
                this.bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
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
            if (this.bufferDrawer.Scale == 0.33f)
            {
                this.bufferDrawer.Left = 0;
                this.bufferDrawer.Top = 0;
                this.bufferDrawer.Scale = 1f;
            }
            else
            {
                int width = (int)(this.Game.Form.RenderWidth * 0.33f);
                int height = (int)(this.Game.Form.RenderHeight * 0.33f);
                int smLeft = this.Game.Form.RenderWidth - width;
                int smTop = this.Game.Form.RenderHeight - height;

                this.bufferDrawer.Left = smLeft;
                this.bufferDrawer.Top = smTop;
                this.bufferDrawer.Scale = 0.33f;
            }
        }
        private void UpdateDebugProximityGridDrawer()
        {
            if (crowd == null)
            {
                return;
            }

            List<Line3D> lines = new List<Line3D>();

            var grid = crowd.GetGrid();

            var rect = grid.GetBounds();

            Vector2 c0 = new Vector2(rect.Left, rect.Top);
            Vector2 c1 = new Vector2(rect.Right, rect.Top);
            Vector2 c2 = new Vector2(rect.Right, rect.Bottom);
            Vector2 c3 = new Vector2(rect.Left, rect.Bottom);
            Vector2 ct = rect.Center;

            this.FindFirstGroundPosition(c0.X, c0.Y, out var r0);
            this.FindFirstGroundPosition(c1.X, c1.Y, out var r1);
            this.FindFirstGroundPosition(c2.X, c2.Y, out var r2);
            this.FindFirstGroundPosition(c3.X, c3.Y, out var r3);
            this.FindFirstGroundPosition(ct.X, ct.Y, out var rt);


            lines.AddRange(Line3D.CreateWiredSquare(new[] { r0.Position, r1.Position, r2.Position, r3.Position }));

            float r = Vector3.Distance(r0.Position, r2.Position) * 0.5f;
            grid.QueryItems(rt.Position, r, out var items);
            foreach (var item in items)
            {
                lines.AddRange(Line3D.CreateCircle(item.RealPosition, item.Radius, 16));
            }

            lines.AddRange(Line3D.CreateCircle(rt.Position, r, 16));

            lineDrawer.SetPrimitives(Color.Red, lines);
            lineDrawer.Visible = true;
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

            for (int i = 0; i < tankAgents.Count; i++)
            {
                this.Lights.AddRange(tankAgents[i].Lights);
            }
            this.Lights.AddRange(this.helicopter.Lights);
            for (int i = 0; i < this.helicopters.InstanceCount; i++)
            {
                this.Lights.AddRange(this.helicopters[i].Lights);
            }

            if (modelsOnly)
            {
                return;
            }

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

            CrowdParameters settings = new CrowdParameters(tankAgentType, tankAgents.Count);

            crowd = graph.AddCrowd(settings);

            var par = new CrowdAgentParams()
            {
                Radius = tankAgentType.Radius,
                Height = tankAgentType.Height,
                MaxAcceleration = 12f,
                MaxSpeed = 15f,
                CollisionQueryRange = tankAgentType.Radius * 12,
                PathOptimizationRange = tankAgentType.Radius * 30,
                UpdateFlags =
                    UpdateFlagTypes.DT_CROWD_OBSTACLE_AVOIDANCE |
                    UpdateFlagTypes.DT_CROWD_ANTICIPATE_TURNS,
                SeparationWeight = 2,
                ObstacleAvoidanceType = 0,
                QueryFilterTypeIndex = 0
            };

            for (int i = 0; i < tankAgents.Count; i++)
            {
                tankAgents[i].CrowdAgent = graph.AddCrowdAgent(crowd, tankAgents[i].Manipulator.Position, par);

                graph.EnableDebugInfo(crowd, tankAgents[i].CrowdAgent);
            }
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
