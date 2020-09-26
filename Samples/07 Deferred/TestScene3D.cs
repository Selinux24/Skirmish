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
        private Sprite upperPanel = null;

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
        private bool bufferDrawerFullscreen = false;
        private SceneRendererResults bufferType = SceneRendererResults.None;
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
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Lights.KeyLight.Enabled = false;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = true;

            Lights.KeyLight.CastShadow = false;
            Lights.BackLight.CastShadow = false;
            Lights.FillLight.CastShadow = false;

            await LoadResourcesAsync(
                new[]
                {
                    InitializeCursor(),
                    InitializeUI()
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    title.Text = "Deferred Ligthning test";
                    help.Text = "";
                    statistics.Text = "";

                    UpdateLayout();
                });

            await LoadResourcesAsync(
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
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    SetGround(terrain, true);
                    AttachToGround(tree, false);
                    AttachToGround(trees, false);

                    StartNodes();

                    StartAnimations();

                    StartTerrain();

                    StartItems(out Vector3 cameraPosition, out int modelCount);

                    cameraPosition /= modelCount;
                    Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
                    Camera.LookTo(cameraPosition + Vector3.Up);
                    Camera.NearPlaneDistance = near;
                    Camera.FarPlaneDistance = far;

                    var nmsettings = BuildSettings.Default;
                    nmsettings.CellSize = 0.5f;
                    nmsettings.CellHeight = 0.25f;
                    nmsettings.Agents = new[] { tankAgentType };
                    nmsettings.PartitionType = SamplePartitionTypes.Layers;
                    nmsettings.EdgeMaxError = 1.0f;
                    nmsettings.BuildMode = BuildModes.Tiled;
                    nmsettings.TileSize = 32;

                    var nmInput = new InputGeometry(GetTrianglesForNavigationGraph);

                    PathFinderDescription = new Engine.PathFinding.PathFinderDescription(nmsettings, nmInput);

                    Task.WhenAll(UpdateNavigationGraph());

                    gameReady = true;
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
            helicopter = await this.AddComponentModel(desc1);
            Lights.AddRange(helicopter.Lights);

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
            helicopters = await this.AddComponentModelInstanced(desc2);
            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                Lights.AddRange(helicopters[i].Lights);
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

            tankAgentType = new Agent()
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

            var tankAgent = new GameAgent<SteerManipulatorController>(tankAgentType, tank, tankController);

            tankAgents.Add(tankAgent);

            Lights.AddRange(tankAgent.Lights);
        }
        private async Task InitializeTerrain()
        {
            terrain = await this.AddComponentScenery(GroundDescription.FromFile("Resources", "terrain.xml", 2));
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
            tree = await this.AddComponentModel(desc1);

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
            trees = await this.AddComponentModelInstanced(desc2);
        }
        private async Task InitializeUI()
        {
            var dTitle = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) };
            var dLoad = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) };
            var dHelp = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) };
            var dStats = new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 10, Color.Red) };

            title = await this.AddComponentUITextArea(dTitle, layerHUD);
            load = await this.AddComponentUITextArea(dLoad, layerHUD);
            help = await this.AddComponentUITextArea(dHelp, layerHUD);
            statistics = await this.AddComponentUITextArea(dStats, layerHUD);

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            upperPanel = await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            var bufferDrawerDesc = new UITextureRendererDescription()
            {
                Channel = UITextureRendererChannels.NoAlpha,
            };
            bufferDrawer = await this.AddComponentUITextureRenderer(bufferDrawerDesc, layerEffects);
            bufferDrawer.Visible = false;
        }
        private async Task InitializeDebug()
        {
            var lineDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Lines",
                Count = 1000,
            };
            lineDrawer = await this.AddComponentPrimitiveListDrawer(lineDrawerDesc, SceneObjectUsages.None, layerEffects + 1);
            lineDrawer.Visible = false;

            var terrainGraphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Terrain Graph",
                Count = MaxGridDrawer,
            };
            terrainGraphDrawer = await this.AddComponentPrimitiveListDrawer(terrainGraphDrawerDesc, SceneObjectUsages.None, layerEffects + 1);
            terrainGraphDrawer.Visible = false;

            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "DEBUG++ Graph",
                DepthEnabled = true,
                Count = 50000,
            };
            graphDrawer = await this.AddComponentPrimitiveListDrawer(graphDrawerDesc, SceneObjectUsages.None, layerEffects + 1);
            graphDrawer.Visible = false;

            var volumesDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Name = "DEBUG++ Volumes",
                Count = 10000
            };
            volumesDrawer = await this.AddComponentPrimitiveListDrawer(volumesDrawerDesc, SceneObjectUsages.None, layerEffects + 1);
            volumesDrawer.Visible = false;
        }

        private void StartNodes()
        {
            var nodes = GetNodes(tankAgentType).OfType<GraphNode>();
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
                    terrainGraphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
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
                tree.Manipulator.SetPosition(treePos.Position);
                tree.Manipulator.SetScale(0.5f);
            }

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - 35, 17, out var pos))
                {
                    trees[i].Manipulator.SetScale(0.5f, true);
                    trees[i].Manipulator.SetPosition(pos.Position, true);
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
                    tankAgents[i].Manipulator.SetNormal(t1Pos.Item.Normal);
                    cameraPosition += t1Pos.Position;
                    modelCount++;
                }
            }

            if (FindTopGroundPosition<Triangle>(20, -20, out var hPos))
            {
                var p = hPos.Position;
                p.Y += 10f;
                helicopter.Manipulator.SetPosition(p, true);
                helicopter.Manipulator.SetScale(0.15f, true);
                cameraPosition += p;
                modelCount++;
            }

            helicopter.AnimationController.AddPath(animations["default"]);
            helicopter.AnimationController.TimeDelta = 3f;
            helicopter.AnimationController.Start();

            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - 20, 20, out var r))
                {
                    var p = r.Position;
                    p.Y += 10f;
                    helicopters[i].Manipulator.SetPosition(p, true);
                    helicopters[i].Manipulator.SetScale(0.15f, true);
                    cameraPosition += p;
                    modelCount++;
                }

                helicopters[i].AnimationController.AddPath(animations["default"]);
                helicopters[i].AnimationController.TimeDelta = 3f;
                helicopters[i].AnimationController.Start();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            tankAgents.ForEach(a => a.Update(new Engine.Common.UpdateContext() { GameTime = gameTime }));

            bool shift = Game.Input.KeyPressed(Keys.LShiftKey);

            UpdateInputCamera(gameTime);
            UpdateInputMouse();
            UpdayeInputLights();
            UpdateInputObjectsVisibility();
            UpdateInputHelicopterTexture();
            UpdateInputDebug();
            UpdateInputDeferredMap();

            UpdateDebugProximityGridDrawer();

            UpdateLights(gameTime);
            UpdateTanks();
        }
        private void UpdateInputCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
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

            if (Game.Input.KeyPressed(Keys.Space))
            {
                lineDrawer.SetPrimitives(Color.Yellow, Line3D.CreateWiredFrustum(Camera.Frustum));
                lineDrawer.Visible = true;
            }
        }
        private void UpdateInputMouse()
        {
            if (Game.Input.LeftMouseButtonJustReleased)
            {
                var pRay = GetPickingRay();
                var rayPParams = RayPickingParams.FacingOnly | RayPickingParams.Perfect;

                if (PickNearest<Triangle>(pRay, rayPParams, out var r))
                {
                    var tri = Line3D.CreateWiredTriangle(r.Item);
                    var cross = Line3D.CreateCross(r.Position, 0.25f);

                    volumesDrawer.SetPrimitives(Color.White, tri);
                    volumesDrawer.SetPrimitives(Color.Red, cross);
                    volumesDrawer.Visible = true;


                    if (Game.Input.ShiftPressed)
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
        private void UpdayeInputLights()
        {
            if (Game.Input.KeyJustReleased(Keys.F))
            {
                Lights.BaseFogColor = new Color((byte)54, (byte)56, (byte)68);
                Lights.FogStart = Lights.FogStart == 0f ? far * fogStart : 0f;
                Lights.FogRange = Lights.FogRange == 0f ? far * fogRange : 0f;
            }

            if (Game.Input.KeyJustReleased(Keys.G))
            {
                Lights.DirectionalLights[0].CastShadow = !Lights.DirectionalLights[0].CastShadow;
            }

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                onlyModels = !onlyModels;

                CreateLights(onlyModels, !Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyJustReleased(Keys.P))
            {
                animateLights = !animateLights;
            }
        }
        private void UpdateInputHelicopterTexture()
        {
            if (Game.Input.KeyJustReleased(Keys.Oemcomma))
            {
                textIntex--;
            }

            if (Game.Input.KeyJustReleased(Keys.OemPeriod))
            {
                textIntex++;
            }

            if (Game.Input.KeyJustReleased(Keys.T))
            {
                helicopter.TextureIndex++;

                if (helicopter.TextureIndex >= helicopter.TextureCount)
                {
                    //Loop
                    helicopter.TextureIndex = 0;
                }
            }
        }
        private void UpdateInputDeferredMap()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                UpdateDebugColorMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                UpdateDebugNormalMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                UpdateDebugDepthMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                UpdateDebugShadowMap(Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                UpdateDebugLightMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                UpdateDebugBufferDrawer();
            }
        }
        private void UpdateInputObjectsVisibility()
        {
            if (Game.Input.KeyJustReleased(Keys.F7))
            {
                bufferDrawer.Visible = !bufferDrawer.Visible;
                help.Visible = bufferDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F8))
            {
                terrain.Active = terrain.Visible = !terrain.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F9))
            {
                tankAgents[0].Active = tankAgents[0].Visible = !tankAgents[0].Visible;

                for (int i = 1; i < tankAgents.Count; i++)
                {
                    tankAgents[i].Active = tankAgents[i].Visible = tankAgents[0].Visible;
                }
            }

            if (Game.Input.KeyJustReleased(Keys.F10))
            {
                helicopter.Active = helicopter.Visible = !helicopter.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F11))
            {
                helicopters.Active = helicopters.Visible = !helicopters.Visible;
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F12))
            {
                terrainGraphDrawer.Visible = !terrainGraphDrawer.Visible;
                graphDrawer.Visible = !graphDrawer.Visible;
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
            if (spotLight != null)
            {
                UpdateSpotlight(gameTime);

                lineDrawer.SetPrimitives(Color.White, spotLight.GetVolume(10));
            }
            else
            {
                lineDrawer.Clear(Color.White);
            }

            if (animateLights && Lights.PointLights.Length > 0)
            {
                UpdatePointLightsAnimation(gameTime);
            }
        }
        private void UpdateSpotlight(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.Left))
            {
                var v = Camera.Left;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                var v = Camera.Right;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Up))
            {
                var v = Camera.Forward;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Down))
            {
                var v = Camera.Backward;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += (v) * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.PageUp))
            {
                spotLight.Position += (Vector3.Up) * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.PageDown))
            {
                spotLight.Position += (Vector3.Down) * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Add))
            {
                spotLight.Intensity += gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Subtract))
            {
                spotLight.Intensity -= gameTime.ElapsedSeconds * 10f;

                spotLight.Intensity = Math.Max(0f, spotLight.Intensity);
            }
        }
        private void UpdatePointLightsAnimation(GameTime gameTime)
        {
            for (int i = 1; i < Lights.PointLights.Length; i++)
            {
                var l = Lights.PointLights[i];

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

        private void UpdateDebugMap()
        {
            if (bufferType == SceneRendererResults.None)
            {
                bufferDrawer.Texture = null;
            }
            else
            {
                bufferDrawer.Texture = Renderer.GetResource(bufferType);
            }
        }
        private void UpdateDebugColorMap()
        {
            bufferType = SceneRendererResults.ColorMap;

            var colorMap = Renderer.GetResource(bufferType);

            //Colors
            bufferDrawer.Texture = colorMap;
            bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
            help.Text = "Colors";

            bufferDrawer.Visible = true;
        }
        private void UpdateDebugNormalMap()
        {
            bufferType = SceneRendererResults.NormalMap;

            var normalMap = Renderer.GetResource(bufferType);

            if (bufferDrawer.Texture == normalMap &&
                bufferDrawer.Channels == UITextureRendererChannels.NoAlpha)
            {
                //Specular Power
                bufferDrawer.Texture = normalMap;
                bufferDrawer.Channels = UITextureRendererChannels.Alpha;
                help.Text = "Specular Power";
            }
            else
            {
                //Normals
                bufferDrawer.Texture = normalMap;
                bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
                help.Text = "Normals";
            }
            bufferDrawer.Visible = true;
        }
        private void UpdateDebugDepthMap()
        {
            bufferType = SceneRendererResults.DepthMap;

            var depthMap = Renderer.GetResource(bufferType);

            if (bufferDrawer.Texture == depthMap &&
                bufferDrawer.Channels == UITextureRendererChannels.NoAlpha)
            {
                //Specular Factor
                bufferDrawer.Texture = depthMap;
                bufferDrawer.Channels = UITextureRendererChannels.Alpha;
                help.Text = "Specular Intensity";
            }
            else
            {
                //Position
                bufferDrawer.Texture = depthMap;
                bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
                help.Text = "Position";
            }
            bufferDrawer.Visible = true;
        }
        private void UpdateDebugShadowMap(bool shift)
        {
            bufferType = SceneRendererResults.ShadowMapDirectional;

            var shadowMap = Renderer.GetResource(bufferType);

            if (shadowMap != null)
            {
                //Shadow map
                if (!help.Text.StartsWith("Shadow map"))
                {
                    bufferDrawer.Texture = shadowMap;
                    bufferDrawer.TextureIndex = 0;
                    bufferDrawer.Channels = UITextureRendererChannels.Red;
                    bufferDrawer.Visible = true;
                }
                else
                {
                    int tIndex = bufferDrawer.TextureIndex;
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

                    bufferDrawer.TextureIndex = tIndex;
                }

                help.Text = string.Format("Shadow map {0}", bufferDrawer.TextureIndex);
            }
            else
            {
                help.Text = "The Shadow map is empty";
            }
        }
        private void UpdateDebugLightMap()
        {
            bufferType = SceneRendererResults.LightMap;

            var lightMap = Renderer.GetResource(bufferType);

            if (lightMap != null)
            {
                //Light map
                bufferDrawer.Texture = lightMap;
                bufferDrawer.Channels = UITextureRendererChannels.NoAlpha;
                bufferDrawer.Visible = true;
                help.Text = "Light map";
            }
            else
            {
                help.Text = "The Light map is empty";
            }
        }
        private void UpdateDebugBufferDrawer()
        {
            bufferDrawerFullscreen = !bufferDrawerFullscreen;

            UpdateLayout();
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

            FindFirstGroundPosition<Triangle>(c0.X, c0.Y, out var r0);
            FindFirstGroundPosition<Triangle>(c1.X, c1.Y, out var r1);
            FindFirstGroundPosition<Triangle>(c2.X, c2.Y, out var r2);
            FindFirstGroundPosition<Triangle>(c3.X, c3.Y, out var r3);
            FindFirstGroundPosition<Triangle>(ct.X, ct.Y, out var rt);

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

            if (Game.Form.IsFullscreen)
            {
                load.Text = Game.RuntimeText;
            }

            title.Text = string.Format(
                titleMask,
                GetRenderMode(),
                Lights.DirectionalLights.Length,
                Lights.PointLights.Length,
                Lights.SpotLights.Length,
                Lights.GetDirectionalShadowCastingLights().Count());

            if (Counters.Statistics.Length == 0)
            {
                statistics.Text = "No statistics";
            }
            else if (textIntex < 0)
            {
                statistics.Text = "Press . for more statistics";
                textIntex = -1;
            }
            else if (textIntex >= Counters.Statistics.Length)
            {
                statistics.Text = "Press , for more statistics";
                textIntex = Counters.Statistics.Length;
            }
            else
            {
                statistics.Text = string.Format(
                    "{0} - {1}",
                    Counters.Statistics[textIntex],
                    Counters.GetStatistics(textIntex));
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();

            UpdateDebugMap();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            load.SetPosition(new Vector2(0, title.Top + title.Height + 2));
            help.SetPosition(new Vector2(0, load.Top + load.Height + 2));
            statistics.SetPosition(new Vector2(0, help.Top + help.Height + 2));

            upperPanel.Width = Game.Form.RenderWidth;
            upperPanel.Height = statistics.Top + statistics.Height + 3;

            if (bufferDrawerFullscreen)
            {
                bufferDrawer.Width = Game.Form.RenderWidth;
                bufferDrawer.Height = Game.Form.RenderHeight;
                bufferDrawer.Left = 0;
                bufferDrawer.Top = 0;
            }
            else
            {
                bufferDrawer.Width = (int)(Game.Form.RenderWidth * 0.33f);
                bufferDrawer.Height = (int)(Game.Form.RenderHeight * 0.33f);
                bufferDrawer.Left = Game.Form.RenderWidth - bufferDrawer.Width;
                bufferDrawer.Top = Game.Form.RenderHeight - bufferDrawer.Height;
            }
        }

        private void CreateLights(bool modelsOnly, bool castShadows)
        {
            Lights.ClearPointLights();
            Lights.ClearSpotLights();
            spotLight = null;

            for (int i = 0; i < tankAgents.Count; i++)
            {
                Lights.AddRange(tankAgents[i].Lights);
            }
            Lights.AddRange(helicopter.Lights);
            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                Lights.AddRange(helicopters[i].Lights);
            }

            if (modelsOnly)
            {
                return;
            }

            SetLightsPosition(castShadows);

            int sep = 10;
            int f = 12;
            int l = (f - 1) * sep;
            l -= (l / 2);

            for (int i = 0; i < f; i++)
            {
                for (int x = 0; x < f; x++)
                {
                    Vector3 lightPosition = new Vector3((i * sep) - l, 1f, (x * sep) - l);

                    if (FindTopGroundPosition((i * sep) - l, (x * sep) - l, out PickingResult<Triangle> r))
                    {
                        lightPosition = r.Position;
                        lightPosition.Y += 1f;
                    }

                    var color = new Color4(Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1), 1.0f);

                    var pointLight = new SceneLightPoint(
                        string.Format("Point {0}", Lights.PointLights.Length),
                        castShadows,
                        color,
                        color,
                        true,
                        SceneLightPointDescription.Create(lightPosition, 5f, 10f))
                    {
                        State = Helper.RandomGenerator.NextFloat(0, 1) >= 0.5f ? 1 : -1
                    };

                    Lights.Add(pointLight);
                }
            }
        }
        private void SetLightsPosition(bool castShadows)
        {
            if (FindTopGroundPosition(0, 1, out PickingResult<Triangle> r))
            {
                var lightPosition = r.Position;
                lightPosition.Y += 10f;

                spotLight = new SceneLightSpot(
                    "Spot the dog",
                    castShadows,
                    Color.Yellow,
                    Color.Yellow,
                    true,
                    SceneLightSpotDescription.Create(lightPosition, Vector3.Down, 25, 25, 25f));

                Lights.Add(spotLight);

                lineDrawer.Active = true;
                lineDrawer.Visible = true;
            }
        }

        public override void NavigationGraphUpdated()
        {
            UpdateGraphNodes(tankAgentType);

            graph = NavigationGraph as Graph;
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
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
        }
    }
}
