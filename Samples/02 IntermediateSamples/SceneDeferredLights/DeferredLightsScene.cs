using Engine;
using Engine.Animation;
using Engine.Collada;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneDeferredLights
{
    public class DeferredLightsScene : WalkableScene
    {
        private const int MaxGridDrawer = 10000;

        private readonly string titleMask = "{0}: {1} directionals, {2} points and {3} spots. Shadows {4}";

        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.10f;

        private UITextArea title = null;
        private UITextArea load = null;
        private UITextArea help = null;
        private UITextArea statistics = null;
        private Sprite upperPanel = null;

        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Scenery terrain = null;

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

        private bool onlyModels = true;

        private readonly Dictionary<string, AnimationPlan> animations = new();

        private bool gameReady = false;

        public DeferredLightsScene(Game game)
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

                    AttachToGround(terrain);
                    AttachToGround(tree);
                    AttachToGround(trees);

                    StartAnimations();

                    StartTerrain();

                    StartItems(out Vector3 cameraPosition, out int modelCount);

                    cameraPosition /= modelCount;
                    Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
                    Camera.LookTo(cameraPosition + Vector3.Up);
                    Camera.NearPlaneDistance = near;
                    Camera.FarPlaneDistance = far;

                    gameReady = true;
                });
        }
        private static async Task<double> InitializeAndTrace(Func<Task> action)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();
            await action();
            sw.Stop();
            return sw.Elapsed.TotalSeconds;
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default("SceneDeferredLights/Resources/target.png", 15, 15, true);
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc);
        }
        private async Task InitializeSkydom()
        {
            var desc = SkydomDescription.Sphere(@"SceneDeferredLights/Resources/sunset.dds", far);

            await AddComponentSky<Skydom, SkydomDescription>("Sky", "Sky", desc);
        }
        private async Task InitializeHelicopters()
        {
            var desc1 = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                TextureIndex = 2,
                Content = ContentDescription.FromFile("SceneDeferredLights/Resources", "m24.json"),
            };
            helicopter = await AddComponent<Model, ModelDescription>("Helicopter", "Helicopter", desc1);
            Lights.AddRange(helicopter.Lights);

            var desc2 = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 2,
                Content = ContentDescription.FromFile("SceneDeferredLights/Resources", "m24.json"),
            };
            helicopters = await AddComponent<ModelInstanced, ModelInstancedDescription>("Bunch of Helicopters", "Bunch of Helicopters", desc2);
            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                Lights.AddRange(helicopters[i].Lights);
            }

            await Task.CompletedTask;
        }
        private async Task InitializeTerrain()
        {
            terrain = await AddComponentGround<Scenery, GroundDescription>("Terrain", "Terrain", GroundDescription.FromFile("SceneDeferredLights/Resources", "terrain.json", 2));
        }
        private async Task InitializeGardener()
        {
            var desc = new GroundGardenerDescription()
            {
                ContentPath = "SceneDeferredLights/Resources/Vegetation",
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
            await AddComponentEffect<GroundGardener, GroundGardenerDescription>("Vegetation", "Vegetation", desc);
        }
        private async Task InitializeTrees()
        {
            var desc1 = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                Content = ContentDescription.FromFile("SceneDeferredLights/resources/trees", "birch_a.json"),
            };
            tree = await AddComponent<Model, ModelDescription>("Lonely tree", "Lonely tree", desc1);

            var desc2 = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                Instances = 10,
                Content = ContentDescription.FromFile("SceneDeferredLights/resources/trees", "birch_b.json"),
            };
            trees = await AddComponent<ModelInstanced, ModelInstancedDescription>("Bunch of trees", "Bunch of trees", desc2);
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
            var dLoad = new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow };
            var dHelp = new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow };
            var dStats = new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.Red };

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", dTitle);
            load = await AddComponentUI<UITextArea, UITextAreaDescription>("Load", "Load", dLoad);
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", dHelp);
            statistics = await AddComponentUI<UITextArea, UITextAreaDescription>("Statistics", "Statistics", dStats);

            upperPanel = await AddComponentUI<Sprite, SpriteDescription>("Upperpanel", "Upperpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), LayerUI - 1);

            bufferDrawer = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("DebugBuferDrawer", "DebugBuferDrawer", UITextureRendererDescription.Default());
            bufferDrawer.Visible = false;
        }
        private async Task InitializeDebug()
        {
            var lineDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 1000,
            };
            lineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Lines", "DEBUG++ Lines", lineDrawerDesc);
            lineDrawer.Visible = false;

            var terrainGraphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = MaxGridDrawer,
            };
            terrainGraphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Terrain Graph", "DEBUG++ Terrain Graph", terrainGraphDrawerDesc);
            terrainGraphDrawer.Visible = false;

            var graphDrawerDesc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 50000,
            };
            graphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DEBUG++ Graph", "DEBUG++ Graph", graphDrawerDesc);
            graphDrawer.Visible = false;
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

            if (FindTopGroundPosition<Triangle>(20, -20, out var hPos))
            {
                var p = hPos.Position;
                p.Y += 10f;
                helicopter.Manipulator.SetPosition(p, true);
                helicopter.Manipulator.SetScale(0.15f, true);
                cameraPosition += p;
                modelCount++;
            }

            helicopter.AnimationController.Start(animations["default"]);
            helicopter.AnimationController.TimeDelta = 3f;

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

                helicopters[i].AnimationController.Start(animations["default"]);
                helicopters[i].AnimationController.TimeDelta = 3f;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!gameReady)
            {
                return;
            }

            UpdateInputCamera(gameTime);
            UpdayeInputLights();
            UpdateInputObjectsVisibility();
            UpdateInputHelicopterTexture();
            UpdateInputDebug();
            UpdateInputDeferredMap();

            UpdateLights(gameTime);
        }
        private void UpdateInputCamera(GameTime gameTime)
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

            if (Game.Input.KeyPressed(Keys.Space))
            {
                lineDrawer.SetPrimitives(Color.Yellow, Line3D.CreateFromVertices(GeometryUtil.CreateFrustum(Topology.LineList, Camera.Frustum)));
                lineDrawer.Visible = true;
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
                helicopter.Active = helicopter.Visible = !helicopter.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F10))
            {
                helicopters.Active = helicopters.Visible = !helicopters.Visible;
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F11))
            {
                terrainGraphDrawer.Visible = !terrainGraphDrawer.Visible;
                graphDrawer.Visible = !graphDrawer.Visible;
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
            bufferDrawer.Channel = ColorChannels.NoAlpha;
            help.Text = "Colors";

            bufferDrawer.Visible = true;
        }
        private void UpdateDebugNormalMap()
        {
            bufferType = SceneRendererResults.NormalMap;

            var normalMap = Renderer.GetResource(bufferType);

            if (bufferDrawer.Texture == normalMap &&
                bufferDrawer.Channel == ColorChannels.NoAlpha)
            {
                //Specular Power
                bufferDrawer.Texture = normalMap;
                bufferDrawer.Channel = ColorChannels.Alpha;
                help.Text = "Specular Power";
            }
            else
            {
                //Normals
                bufferDrawer.Texture = normalMap;
                bufferDrawer.Channel = ColorChannels.NoAlpha;
                help.Text = "Normals";
            }
            bufferDrawer.Visible = true;
        }
        private void UpdateDebugDepthMap()
        {
            bufferType = SceneRendererResults.DepthMap;

            var depthMap = Renderer.GetResource(bufferType);

            if (bufferDrawer.Texture == depthMap &&
                bufferDrawer.Channel == ColorChannels.NoAlpha)
            {
                //Specular Factor
                bufferDrawer.Texture = depthMap;
                bufferDrawer.Channel = ColorChannels.Alpha;
                help.Text = "Specular Intensity";
            }
            else
            {
                //Position
                bufferDrawer.Texture = depthMap;
                bufferDrawer.Channel = ColorChannels.NoAlpha;
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
                    bufferDrawer.Channel = ColorChannels.Red;
                    bufferDrawer.Visible = true;
                }
                else
                {
                    uint tIndex = bufferDrawer.TextureIndex;
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
                bufferDrawer.Channel = ColorChannels.NoAlpha;
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
                Lights.GetDirectionalShadowCastingLights(GameEnvironment, Camera.Position).Count());

            if (FrameCounters.Statistics.Length == 0)
            {
                statistics.Text = "No statistics";
            }
            else if (textIntex < 0)
            {
                statistics.Text = "Press . for more statistics";
                textIntex = -1;
            }
            else if (textIntex >= FrameCounters.Statistics.Length)
            {
                statistics.Text = "Press , for more statistics";
                textIntex = FrameCounters.Statistics.Length;
            }
            else
            {
                statistics.Text = string.Format(
                    "{0} - {1}",
                    FrameCounters.Statistics[textIntex],
                    FrameCounters.GetStatistics(textIntex));
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
                    var lightPosition = new Vector3((i * sep) - l, 1f, (x * sep) - l);

                    if (FindTopGroundPosition((i * sep) - l, (x * sep) - l, out PickingResult<Triangle> r))
                    {
                        lightPosition = r.Position;
                        lightPosition.Y += 1f;
                    }

                    var color = new Color3(Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1));

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
                    Color.Yellow.RGB(),
                    Color.Yellow.RGB(),
                    true,
                    SceneLightSpotDescription.Create(lightPosition, Vector3.Down, 25, 25, 25f));

                Lights.Add(spotLight);

                lineDrawer.Active = true;
                lineDrawer.Visible = true;
            }
        }
    }
}
