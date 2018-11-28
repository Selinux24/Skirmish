using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> load = null;
        private SceneObject<TextDrawer> help = null;
        private SceneObject<TextDrawer> statistics = null;

        private Agent tankAgentType = null;
        private SceneObject<GameAgent<SteerManipulatorController>> tankAgent1 = null;
        private SceneObject<GameAgent<SteerManipulatorController>> tankAgent2 = null;
        private SceneObject<Model> helicopter = null;
        private SceneObject<ModelInstanced> helicopters = null;
        private SceneObject<Scenery> terrain = null;

        private SceneObject<Model> tree = null;
        private SceneObject<ModelInstanced> trees = null;

        private SceneObject<SpriteTexture> bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private SceneObject<PrimitiveListDrawer<Line3D>> lineDrawer = null;
        private SceneObject<PrimitiveListDrawer<Triangle>> terrainGraphDrawer = null;

        private readonly Random rnd = new Random(0);
        private bool onlyModels = true;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        public TestScene3D(Game game)
            : base(game, SceneModes.DeferredLightning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeCursor();

            InitializeUI();

            #region Models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Skydom
            {
                sw.Restart();

                InitializeSkydom();

                sw.Stop();
                loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Helicopter
            {
                sw.Restart();

                InitializeHelicopter();

                sw.Stop();
                loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Helicopters
            {
                sw.Restart();

                InitializeHelicopters();

                sw.Stop();
                loadingText += string.Format("helicopters: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Tank
            {
                sw.Restart();

                InitializeTanks();

                sw.Stop();
                loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Terrain
            {
                sw.Restart();

                InitializeTerrain();

                sw.Stop();
                loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Gardener
            {
                sw.Restart();

                InitializeGardener();

                sw.Stop();
                loadingText += string.Format("gardener: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Tree
            {
                sw.Restart();

                InitializeTree();

                sw.Stop();
                loadingText += string.Format("tree: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Trees
            {
                sw.Restart();

                InitializeTrees();

                sw.Stop();
                loadingText += string.Format("trees: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #endregion

            #region Lights

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;

            this.Lights.KeyLight.CastShadow = false;
            this.Lights.BackLight.CastShadow = false;
            this.Lights.FillLight.CastShadow = false;

            #endregion

            InitializeDebug();

            this.SetGround(this.terrain, true);
            this.AttachToGround(this.tree, false);
            this.AttachToGround(this.trees, false);

            this.title.Instance.Text = "Deferred Ligthning test";
            this.load.Instance.Text = loadingText;
            this.help.Instance.Text = "";
            this.statistics.Instance.Text = "";
        }
        private void InitializeCursor()
        {
            var cursorDesc = new CursorDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };
            this.AddComponent<Cursor>(cursorDesc, SceneObjectUsages.UI, layerHUD + 1);
        }
        private void InitializeSkydom()
        {
            var desc = new SkydomDescription()
            {
                Name = "Sky",
                ContentPath = "Resources",
                Radius = far,
                Texture = "sunset.dds",
            };
            this.AddComponent<Skydom>(desc);
        }
        private void InitializeHelicopter()
        {
            var desc = new ModelDescription()
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
            this.helicopter = this.AddComponent<Model>(desc);
            this.Lights.AddRange(this.helicopter.Instance.Lights);
        }
        private void InitializeHelicopters()
        {
            var desc = new ModelInstancedDescription()
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
            this.helicopters = this.AddComponent<ModelInstanced>(desc);
            for (int i = 0; i < this.helicopters.Count; i++)
            {
                this.Lights.AddRange(this.helicopters.Instance[i].Lights);
            }
        }
        private void InitializeTanks()
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
            var tank1 = this.AddComponent<Model>(desc);
            tank1.Transform.SetScale(0.2f, true);
            var tank2 = this.AddComponent<Model>(desc);
            tank2.Transform.SetScale(0.2f, true);

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

            var tankbbox = tank1.Instance.GetBoundingBox();
            this.tankAgentType = new Agent()
            {
                Height = tankbbox.GetY(),
                Radius = tankbbox.GetX() * 0.5f,
                MaxClimb = tankbbox.GetY() * 0.55f,
            };

            var agent1 = new GameAgent<SteerManipulatorController>(this.tankAgentType, tank1, tankController1);
            var agent2 = new GameAgent<SteerManipulatorController>(this.tankAgentType, tank2, tankController2);
            this.tankAgent1 = this.AddComponent(agent1, new SceneObjectDescription() { });
            this.tankAgent2 = this.AddComponent(agent2, new SceneObjectDescription() { });

            this.Lights.AddRange(this.tankAgent1.Instance.Lights);
            this.Lights.AddRange(this.tankAgent2.Instance.Lights);
        }
        private void InitializeTerrain()
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
            this.terrain = this.AddComponent<Scenery>(desc);
        }
        private void InitializeGardener()
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
            this.AddComponent<GroundGardener>(desc);
        }
        private void InitializeTree()
        {
            var desc = new ModelDescription()
            {
                Name = "Lonely tree",
                Static = true,
                CastShadow = true,
                AlphaEnabled = true,
                DepthEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/trees",
                    ModelContentFilename = "birch_a.xml",
                }
            };
            this.tree = this.AddComponent<Model>(desc);
        }
        private void InitializeTrees()
        {
            var desc = new ModelInstancedDescription()
            {
                Name = "Bunch of trees",
                Static = true,
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
            this.trees = this.AddComponent<ModelInstanced>(desc);
        }
        private void InitializeUI()
        {
            var dTitle = TextDrawerDescription.Generate("Tahoma", 18, Color.White);
            var dLoad = TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow);
            var dHelp = TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow);
            var dStats = TextDrawerDescription.Generate("Lucida Casual", 10, Color.Red);

            this.title = this.AddComponent<TextDrawer>(dTitle, SceneObjectUsages.UI, layerHUD);
            this.load = this.AddComponent<TextDrawer>(dLoad, SceneObjectUsages.UI, layerHUD);
            this.help = this.AddComponent<TextDrawer>(dHelp, SceneObjectUsages.UI, layerHUD);
            this.statistics = this.AddComponent<TextDrawer>(dStats, SceneObjectUsages.UI, layerHUD);

            this.title.Instance.Position = Vector2.Zero;
            this.load.Instance.Position = new Vector2(0, this.title.Instance.Top + this.title.Instance.Height + 2);
            this.help.Instance.Position = new Vector2(0, this.load.Instance.Top + this.load.Instance.Height + 2);
            this.statistics.Instance.Position = new Vector2(0, this.help.Instance.Top + this.help.Instance.Height + 2);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.statistics.Instance.Top + this.statistics.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };
            this.AddComponent<Sprite>(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private void InitializeDebug()
        {
            {
                int width = (int)(this.Game.Form.RenderWidth * 0.33f);
                int height = (int)(this.Game.Form.RenderHeight * 0.33f);
                int smLeft = this.Game.Form.RenderWidth - width;
                int smTop = this.Game.Form.RenderHeight - height;

                var desc = new SpriteTextureDescription()
                {
                    Left = smLeft,
                    Top = smTop,
                    Width = width,
                    Height = height,
                    Channel = SpriteTextureChannels.NoAlpha,
                };
                this.bufferDrawer = this.AddComponent<SpriteTexture>(desc, SceneObjectUsages.UI, layerEffects);
                this.bufferDrawer.Visible = false;
            }

            {
                var desc = new PrimitiveListDrawerDescription<Line3D>()
                {
                    DepthEnabled = true,
                    Count = 1000,
                };
                this.lineDrawer = this.AddComponent<PrimitiveListDrawer<Line3D>>(desc, SceneObjectUsages.None, layerEffects);
                this.lineDrawer.Visible = false;
            }

            {
                var desc = new PrimitiveListDrawerDescription<Triangle>()
                {
                    Count = MaxGridDrawer,
                };
                this.terrainGraphDrawer = this.AddComponent<PrimitiveListDrawer<Triangle>>(desc, SceneObjectUsages.None, layerEffects);
                this.terrainGraphDrawer.Visible = false;
            }
        }

        public override void Initialized()
        {
            base.Initialized();

            StartNodes();

            StartAnimations();

            StartTerrain();

            StartItems(out Vector3 cameraPosition, out int modelCount);

            cameraPosition /= (float)modelCount;
            this.Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            this.Camera.LookTo(cameraPosition + Vector3.Up);
            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;
        }
        private void StartNodes()
        {
            var nodes = this.GetNodes(this.tankAgentType);
            if (nodes != null && nodes.Length > 0)
            {
                Random clrRnd = new Random(1);
                Color[] regions = new Color[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
                }

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = (GraphNode)nodes[i];
                    var color = node.Color;
                    var tris = node.Triangles;

                    this.terrainGraphDrawer.Instance.AddPrimitives(color, tris);
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
            {
                if (this.FindTopGroundPosition(20, -20, out PickingResult<Triangle> r))
                {
                    this.tree.Transform.SetPosition(r.Position);
                    this.tree.Transform.SetScale(0.5f);
                }
            }

            {
                for (int i = 0; i < this.trees.Count; i++)
                {
                    if (this.FindTopGroundPosition((i * 10) - 35, 17, out PickingResult<Triangle> r))
                    {
                        this.trees.Instance[i].Manipulator.SetScale(0.5f, true);
                        this.trees.Instance[i].Manipulator.SetPosition(r.Position, true);
                    }
                }
            }

            var nvSettings = BuildSettings.Default;
            nvSettings.Agents = new[] { this.tankAgentType };

            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription(nvSettings, nvInput);
        }
        private void StartItems(out Vector3 cameraPosition, out int modelCount)
        {
            cameraPosition = Vector3.Zero;
            modelCount = 0;

            #region Tanks
            {
                if (this.FindTopGroundPosition(20, 40, out PickingResult<Triangle> r))
                {
                    this.tankAgent1.Transform.SetPosition(r.Position);
                    this.tankAgent1.Transform.SetNormal(r.Item.Normal);
                    cameraPosition += r.Position;
                    modelCount++;
                }
            }

            {
                if (this.FindTopGroundPosition(15, 35, out PickingResult<Triangle> r))
                {
                    this.tankAgent2.Transform.SetPosition(r.Position);
                    this.tankAgent2.Transform.SetNormal(r.Item.Normal);
                    cameraPosition += r.Position;
                    modelCount++;
                }
            }
            #endregion

            #region Helicopter
            {
                if (this.FindTopGroundPosition(20, -20, out PickingResult<Triangle> r))
                {
                    var p = r.Position;
                    p.Y += 10f;
                    this.helicopter.Transform.SetPosition(p, true);
                    this.helicopter.Transform.SetScale(0.15f, true);
                    cameraPosition += p;
                    modelCount++;
                }

                this.helicopter.Instance.AnimationController.AddPath(this.animations["default"]);
                this.helicopter.Instance.AnimationController.TimeDelta = 3f;
                this.helicopter.Instance.AnimationController.Start();
            }
            #endregion

            #region Helicopters
            {
                for (int i = 0; i < this.helicopters.Count; i++)
                {
                    if (this.FindTopGroundPosition((i * 10) - 20, 20, out PickingResult<Triangle> r))
                    {
                        var p = r.Position;
                        p.Y += 10f;
                        this.helicopters.Instance[i].Manipulator.SetPosition(p, true);
                        this.helicopters.Instance[i].Manipulator.SetScale(0.15f, true);
                        cameraPosition += p;
                        modelCount++;
                    }

                    this.helicopters.Instance[i].AnimationController.AddPath(this.animations["default"]);
                    this.helicopters.Instance[i].AnimationController.TimeDelta = 3f;
                    this.helicopters.Instance[i].AnimationController.Start();
                }
            }
            #endregion
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

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            UpdateInputCamera(gameTime, shift);
            UpdayeInputLights(shift);
            UpdateInputObjectsVisibility();
            UpdateInputHelicopterTexture();
            UpdateInputGraph();
            UpdateInputDebug(shift);

            UpdateLights(gameTime);
        }
        private void UpdateInputCamera(GameTime gameTime, bool shift)
        {
#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

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
                this.lineDrawer.Instance.SetPrimitives(Color.Yellow, Line3D.CreateWiredFrustum(this.Camera.Frustum));
                this.lineDrawer.Visible = true;
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
                this.helicopter.Instance.TextureIndex++;

                if (this.helicopter.Instance.TextureIndex >= this.helicopter.Instance.TextureCount)
                {
                    //Loop
                    this.helicopter.Instance.TextureIndex = 0;
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

                this.lineDrawer.Instance.SetPrimitives(Color.White, this.spotLight.GetVolume(10));
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
            this.bufferDrawer.Instance.Texture = colorMap;
            this.bufferDrawer.Instance.Channels = SpriteTextureChannels.NoAlpha;
            this.help.Instance.Text = "Colors";

            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugNormalMap()
        {
            var normalMap = this.Renderer.GetResource(SceneRendererResults.NormalMap);

            if (this.bufferDrawer.Instance.Texture == normalMap &&
                this.bufferDrawer.Instance.Channels == SpriteTextureChannels.NoAlpha)
            {
                //Specular Power
                this.bufferDrawer.Instance.Texture = normalMap;
                this.bufferDrawer.Instance.Channels = SpriteTextureChannels.Alpha;
                this.help.Instance.Text = "Specular Power";
            }
            else
            {
                //Normals
                this.bufferDrawer.Instance.Texture = normalMap;
                this.bufferDrawer.Instance.Channels = SpriteTextureChannels.NoAlpha;
                this.help.Instance.Text = "Normals";
            }
            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugDepthMap()
        {
            var depthMap = this.Renderer.GetResource(SceneRendererResults.DepthMap);

            if (this.bufferDrawer.Instance.Texture == depthMap &&
                this.bufferDrawer.Instance.Channels == SpriteTextureChannels.NoAlpha)
            {
                //Specular Factor
                this.bufferDrawer.Instance.Texture = depthMap;
                this.bufferDrawer.Instance.Channels = SpriteTextureChannels.Alpha;
                this.help.Instance.Text = "Specular Intensity";
            }
            else
            {
                //Position
                this.bufferDrawer.Instance.Texture = depthMap;
                this.bufferDrawer.Instance.Channels = SpriteTextureChannels.NoAlpha;
                this.help.Instance.Text = "Position";
            }
            this.bufferDrawer.Visible = true;
        }
        private void UpdateDebugShadowMap(bool shift)
        {
            var shadowMap = this.Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);

            if (shadowMap != null)
            {
                //Shadow map
                if (!this.help.Instance.Text.StartsWith("Shadow map"))
                {
                    this.bufferDrawer.Instance.Texture = shadowMap;
                    this.bufferDrawer.Instance.TextureIndex = 0;
                    this.bufferDrawer.Instance.Channels = SpriteTextureChannels.Red;
                    this.bufferDrawer.Visible = true;
                }
                else
                {
                    int tIndex = this.bufferDrawer.Instance.TextureIndex;
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

                    this.bufferDrawer.Instance.TextureIndex = tIndex;
                }

                this.help.Instance.Text = string.Format("Shadow map {0}", this.bufferDrawer.Instance.TextureIndex);
            }
            else
            {
                this.help.Instance.Text = "The Shadow map is empty";
            }
        }
        private void UpdateDebugLightMap()
        {
            var lightMap = this.Renderer.GetResource(SceneRendererResults.LightMap);

            if (lightMap != null)
            {
                //Light map
                this.bufferDrawer.Instance.Texture = lightMap;
                this.bufferDrawer.Instance.Channels = SpriteTextureChannels.NoAlpha;
                this.bufferDrawer.Visible = true;
                this.help.Instance.Text = "Light map";
            }
            else
            {
                this.help.Instance.Text = "The Light map is empty";
            }
        }
        private void UpdateDebugBufferDrawer()
        {
            if (this.bufferDrawer.ScreenTransform.Position == Vector2.Zero)
            {
                int width = (int)(this.Game.Form.RenderWidth * 0.33f);
                int height = (int)(this.Game.Form.RenderHeight * 0.33f);
                int smLeft = this.Game.Form.RenderWidth - width;
                int smTop = this.Game.Form.RenderHeight - height;

                this.bufferDrawer.ScreenTransform.SetPosition(smLeft, smTop);
                this.bufferDrawer.Instance.ResizeSprite(width, height);
            }
            else
            {
                this.bufferDrawer.ScreenTransform.SetPosition(Vector2.Zero);
                this.bufferDrawer.Instance.ResizeSprite(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.Game.Form.IsFullscreen)
            {
                this.load.Instance.Text = this.Game.RuntimeText;
            }

            this.title.Instance.Text = string.Format(
                this.titleMask,
                this.GetRenderMode(),
                this.Lights.DirectionalLights.Length,
                this.Lights.PointLights.Length,
                this.Lights.SpotLights.Length,
                this.Lights.GetDirectionalShadowCastingLights().Length);

            if (Counters.Statistics.Length == 0)
            {
                this.statistics.Instance.Text = "No statistics";
            }
            else if (this.textIntex < 0)
            {
                this.statistics.Instance.Text = "Press . for more statistics";
                this.textIntex = -1;
            }
            else if (this.textIntex >= Counters.Statistics.Length)
            {
                this.statistics.Instance.Text = "Press , for more statistics";
                this.textIntex = Counters.Statistics.Length;
            }
            else
            {
                this.statistics.Instance.Text = string.Format(
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

            this.Lights.AddRange(this.tankAgent1.Instance.Lights);
            this.Lights.AddRange(this.tankAgent2.Instance.Lights);
            this.Lights.AddRange(this.helicopter.Instance.Lights);
            for (int i = 0; i < this.helicopters.Count; i++)
            {
                this.Lights.AddRange(this.helicopters.Instance[i].Lights);
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

                        var color = new Color4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1.0f);

                        var pointLight = new SceneLightPoint(
                            string.Format("Point {0}", this.Lights.PointLights.Length),
                            castShadows,
                            color,
                            color,
                            true,
                            SceneLightPointDescription.Create(lightPosition, 5f, 10f))
                        {
                            State = rnd.NextFloat(0, 1) >= 0.5f ? 1 : -1
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

                Vector3 direction = -Vector3.Normalize(lightPosition);

                this.spotLight = new SceneLightSpot(
                    "Spot the dog",
                    castShadows,
                    Color.Yellow,
                    Color.Yellow,
                    true,
                    SceneLightSpotDescription.Create(lightPosition, direction, 25, 25, 25f));

                this.Lights.Add(this.spotLight);

                this.lineDrawer.Active = true;
                this.lineDrawer.Visible = true;
            }
        }
    }
}
