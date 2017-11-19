using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Deferred
{
    public class TestScene3D : Scene
    {
        private const int MaxGridDrawer = 10000;

        private string titleMask = "{0}: {1} directionals, {2} points and {3} spots. Shadows {4}";

        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.10f;
        private const int layerObjects = 0;
        private const int layerTerrain = 1;
        private const int layerEffects = 2;
        private const int layerHUD = 99;

        private SceneObject<Cursor> cursor = null;
        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> load = null;
        private SceneObject<TextDrawer> help = null;
        private SceneObject<TextDrawer> statistics = null;
        private SceneObject<Sprite> backPannel = null;

        private NavigationMeshAgentType tankAgentType = null;
        private SceneObject<GameAgent<SteerManipulatorController>> tankAgent1 = null;
        private SceneObject<GameAgent<SteerManipulatorController>> tankAgent2 = null;
        private SceneObject<Model> helicopter = null;
        private SceneObject<ModelInstanced> helicopters = null;
        private SceneObject<Skydom> skydom = null;
        private SceneObject<Scenery> terrain = null;
        private SceneObject<GroundGardener> gardener = null;

        private SceneObject<Model> tree = null;
        private SceneObject<ModelInstanced> trees = null;

        private SceneObject<SpriteTexture> bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private SceneObject<LineListDrawer> lineDrawer = null;
        private SceneObject<TriangleListDrawer> terrainGraphDrawer = null;

        private Random rnd = new Random(0);
        private int pointOffset = 0;
        private int spotOffset = 0;
        private bool onlyModels = true;

        private Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private AgentCrowd crowd = null;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            #region Cursor
            {
                var cursorDesc = new CursorDescription()
                {
                    Textures = new[] { "target.png" },
                    Width = 16,
                    Height = 16,
                };
                this.cursor = this.AddComponent<Cursor>(cursorDesc, SceneObjectUsageEnum.UI, layerHUD + 1);
            }
            #endregion

            #region Models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Skydom
            {
                sw.Restart();

                var desc = new SkydomDescription()
                {
                    Name = "Sky",
                    ContentPath = "Resources",
                    Radius = far,
                    Texture = "sunset.dds",
                };
                this.skydom = this.AddComponent<Skydom>(desc);

                sw.Stop();
                loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Helicopter
            {
                sw.Restart();

                var desc = new ModelDescription()
                {
                    Name = "Helicopter",
                    CastShadow = true,
                    TextureIndex = 2,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "helicopter.xml",
                    }
                };
                this.helicopter = this.AddComponent<Model>(desc);

                sw.Stop();
                loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Helicopters
            {
                sw.Restart();

                var desc = new ModelInstancedDescription()
                {
                    Name = "Bunch of Helicopters",
                    CastShadow = true,
                    Instances = 2,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources",
                        ModelContentFilename = "helicopter.xml",
                    }
                };
                this.helicopters = this.AddComponent<ModelInstanced>(desc);

                sw.Stop();
                loadingText += string.Format("helicopters: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Helicopters animation plans
            {
                var ap = new AnimationPath();
                ap.AddLoop("roll");
                this.animations.Add("default", new AnimationPlan(ap));
            }
            #endregion

            #region Tank
            {
                sw.Restart();

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
                this.tankAgentType = new NavigationMeshAgentType()
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

                sw.Stop();
                loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Terrain
            {
                sw.Restart();

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

                sw.Stop();
                loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Gardener
            {
                sw.Restart();

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
                this.gardener = this.AddComponent<GroundGardener>(desc);

                sw.Stop();
                loadingText += string.Format("gardener: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #region Trees
            {
                sw.Restart();

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

                sw.Stop();
                loadingText += string.Format("tree: {0} ", sw.Elapsed.TotalSeconds);
            }

            {
                sw.Restart();

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

                sw.Stop();
                loadingText += string.Format("trees: {0} ", sw.Elapsed.TotalSeconds);
            }
            #endregion

            #endregion

            #region Texts
            {
                var dTitle = TextDrawerDescription.Generate("Tahoma", 18, Color.White);
                var dLoad = TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow);
                var dHelp = TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow);
                var dStats = TextDrawerDescription.Generate("Lucida Casual", 10, Color.Red);

                this.title = this.AddComponent<TextDrawer>(dTitle, SceneObjectUsageEnum.UI, layerHUD);
                this.load = this.AddComponent<TextDrawer>(dLoad, SceneObjectUsageEnum.UI, layerHUD);
                this.help = this.AddComponent<TextDrawer>(dHelp, SceneObjectUsageEnum.UI, layerHUD);
                this.statistics = this.AddComponent<TextDrawer>(dStats, SceneObjectUsageEnum.UI, layerHUD);

                this.title.Instance.Text = "Deferred Ligthning test";
                this.load.Instance.Text = loadingText;
                this.help.Instance.Text = "";
                this.statistics.Instance.Text = "";

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
                this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
            }
            #endregion

            #region Lights

            this.Lights.KeyLight.Enabled = true;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = false;

            this.pointOffset = this.Lights.PointLights.Length;
            this.spotOffset = this.Lights.SpotLights.Length;

            #endregion

            #region Debug

            #region Buffer Drawer
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
                    Channel = SpriteTextureChannelsEnum.NoAlpha,
                };
                this.bufferDrawer = this.AddComponent<SpriteTexture>(desc, SceneObjectUsageEnum.UI, layerEffects);

                this.bufferDrawer.Visible = false;
            }
            #endregion

            #region Light line drawer
            {
                var desc = new LineListDrawerDescription()
                {
                    DepthEnabled = true,
                    Count = 1000,
                };
                this.lineDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, layerEffects);
                this.lineDrawer.Visible = false;
            }
            #endregion

            #region Terrain grapth drawer
            {
                var desc = new TriangleListDrawerDescription()
                {
                    Count = MaxGridDrawer,
                };
                this.terrainGraphDrawer = this.AddComponent<TriangleListDrawer>(desc, SceneObjectUsageEnum.None, layerEffects);
                this.terrainGraphDrawer.Visible = false;
            }

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
                    var node = (NavigationMeshNode)nodes[i];
                    var color = regions[node.PolyId];
                    var poly = node.Poly;
                    var tris = poly.Triangulate();

                    this.terrainGraphDrawer.Instance.AddTriangles(color, tris);
                }
            }

            #endregion

            #endregion

            var navSettings = NavigationMeshGenerationSettings.Default;
            navSettings.Agents = new[] { this.tankAgentType };

            this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription()
            {
                Settings = navSettings,
            };

            this.SetGround(this.terrain, true);
            this.AttachToGround(this.tree, false);
            this.AttachToGround(this.trees, false);

            #region Tree
            {
                Vector3 p;
                Triangle t;
                float d;
                if (this.FindTopGroundPosition(20, -20, out p, out t, out d))
                {
                    this.tree.Transform.SetPosition(p);
                    this.tree.Transform.SetScale(0.5f);
                }
            }
            #endregion

            #region Trees
            {
                for (int i = 0; i < this.trees.Count; i++)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (this.FindTopGroundPosition((i * 10) - 35, 17, out p, out t, out d))
                    {
                        this.trees.Instance[i].Manipulator.SetScale(0.5f, true);
                        this.trees.Instance[i].Manipulator.SetPosition(p, true);
                    }
                }
            }
            #endregion
        }
        public override void Initialized()
        {
            base.Initialized();

            Vector3 cameraPosition = Vector3.Zero;
            int modelCount = 0;

            #region Tanks
            {
                Vector3 p;
                Triangle t;
                float d;
                if (this.FindTopGroundPosition(20, 40, out p, out t, out d))
                {
                    this.tankAgent1.Transform.SetPosition(p);
                    this.tankAgent1.Transform.SetNormal(t.Normal);
                    cameraPosition += p;
                    modelCount++;
                }
            }

            {
                Vector3 p;
                Triangle t;
                float d;
                if (this.FindTopGroundPosition(15, 35, out p, out t, out d))
                {
                    this.tankAgent2.Transform.SetPosition(p);
                    this.tankAgent2.Transform.SetNormal(t.Normal);
                    cameraPosition += p;
                    modelCount++;
                }
            }
            #endregion

            #region Helicopter
            {
                Vector3 p;
                Triangle t;
                float d;
                if (this.FindTopGroundPosition(20, -20, out p, out t, out d))
                {
                    p.Y += 10f;
                    this.helicopter.Transform.SetPosition(p, true);
                    cameraPosition += p;
                    modelCount++;
                }

                this.helicopter.Instance.AnimationController.AddPath(this.animations["default"]);
                this.helicopter.Instance.AnimationController.Start();
            }
            #endregion

            #region Helicopters
            {
                for (int i = 0; i < this.helicopters.Count; i++)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (this.FindTopGroundPosition((i * 10) - 20, 20, out p, out t, out d))
                    {
                        p.Y += 10f;
                        this.helicopters.Instance[i].Manipulator.SetPosition(p, true);
                        cameraPosition += p;
                        modelCount++;
                    }

                    this.helicopters.Instance[i].AnimationController.AddPath(this.animations["default"]);
                    this.helicopters.Instance[i].AnimationController.Start();
                }
            }
            #endregion

            cameraPosition /= (float)modelCount;
            this.Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            this.Camera.LookTo(cameraPosition + Vector3.Up);
            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;

            this.crowd = new AgentCrowd(this, this.navigationGraph as NavigationMesh, this.tankAgentType);
            crowd.AddAgent(this.tankAgent1.Instance);
            crowd.AddAgent(this.tankAgent2.Instance);
        }
        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            base.Update(gameTime);

            Ray cursorRay = this.GetPickingRay();

            #region Cursor picking and positioning

            Vector3 position;
            Triangle triangle;
            float distance;
            bool picked = this.PickNearest(ref cursorRay, true, SceneObjectUsageEnum.Ground, out position, out triangle, out distance);

            #endregion

            #region Tanks

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (picked)
                {
                    //var p = this.FindPath(
                    //    this.tankAgent.Instance.AgentType,
                    //    this.tankAgent.Transform.Position, position, true, this.tankAgent.Instance.MaximumSpeed * gameTime.ElapsedSeconds);
                    //if (p != null)
                    //{
                    //    this.tankAgent.Instance.FollowPath(p);
                    //}

                    this.crowd.MoveTo(position, 1f);
                }
            }

            #endregion

            #region Camera

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
                this.Camera.MoveLeft(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.Space))
            {
                this.lineDrawer.Instance.SetLines(Color.Yellow, Line3D.CreateWiredFrustum(this.Camera.Frustum));
                this.lineDrawer.Visible = true;
            }

            #endregion

            #region Lights

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

                this.CreateLights(this.onlyModels);
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.animateLights = !this.animateLights;
            }

            if (this.spotLight != null)
            {
                if (this.Game.Input.KeyPressed(Keys.Left))
                {
                    this.spotLight.Position += (Vector3.Left) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Right))
                {
                    this.spotLight.Position += (Vector3.Right) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Up))
                {
                    this.spotLight.Position += (Vector3.ForwardLH) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Down))
                {
                    this.spotLight.Position += (Vector3.BackwardLH) * gameTime.ElapsedSeconds * 10f;
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

                this.lineDrawer.Instance.SetLines(Color.White, this.spotLight.GetVolume(10));
            }
            else
            {
                this.lineDrawer.Visible = false;
            }

            if (animateLights)
            {
                if (this.Lights.PointLights.Length > 0)
                {
                    for (int i = 1; i < this.Lights.PointLights.Length; i++)
                    {
                        var l = this.Lights.PointLights[i];

                        if ((int)l.State == 1) l.Radius += (0.5f * gameTime.ElapsedSeconds * 50f);
                        if ((int)l.State == -1) l.Radius -= (2f * gameTime.ElapsedSeconds * 50f);

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
            }

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F12))
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

            {
                if (this.Game.Input.KeyJustReleased(Keys.F1))
                {
                    var colorMap = this.Renderer.GetResource(SceneRendererResultEnum.ColorMap);

                    //Colors
                    this.bufferDrawer.Instance.Texture = colorMap;
                    this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.help.Instance.Text = "Colors";

                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F2))
                {
                    var normalMap = this.Renderer.GetResource(SceneRendererResultEnum.NormalMap);

                    if (this.bufferDrawer.Instance.Texture == normalMap &&
                        this.bufferDrawer.Instance.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Power
                        this.bufferDrawer.Instance.Texture = normalMap;
                        this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Instance.Text = "Specular Power";
                    }
                    else
                    {
                        //Normals
                        this.bufferDrawer.Instance.Texture = normalMap;
                        this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Instance.Text = "Normals";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F3))
                {
                    var depthMap = this.Renderer.GetResource(SceneRendererResultEnum.DepthMap);

                    if (this.bufferDrawer.Instance.Texture == depthMap &&
                        this.bufferDrawer.Instance.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Factor
                        this.bufferDrawer.Instance.Texture = depthMap;
                        this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Instance.Text = "Specular Intensity";
                    }
                    else
                    {
                        //Position
                        this.bufferDrawer.Instance.Texture = depthMap;
                        this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Instance.Text = "Position";
                    }
                    this.bufferDrawer.Visible = true;
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.terrainGraphDrawer.Visible = !this.terrainGraphDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                var shadowMap = this.Renderer.GetResource(SceneRendererResultEnum.ShadowMapStatic);

                if (shadowMap != null)
                {
                    //Shadow map
                    this.bufferDrawer.Instance.Texture = shadowMap;
                    this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.Red;
                    this.bufferDrawer.Visible = true;
                    this.help.Instance.Text = "Shadow map";
                }
                else
                {
                    this.help.Instance.Text = "The Shadow map is empty";
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                var lightMap = this.Renderer.GetResource(SceneRendererResultEnum.LightMap);

                if (lightMap != null)
                {
                    //Light map
                    this.bufferDrawer.Instance.Texture = lightMap;
                    this.bufferDrawer.Instance.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.bufferDrawer.Visible = true;
                    this.help.Instance.Text = "Light map";
                }
                else
                {
                    this.help.Instance.Text = "The Light map is empty";
                }
            }

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

            #endregion

            this.crowd.Update(gameTime);
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
                this.Lights.ShadowCastingLights.Length);

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

        private void CreateLights(bool modelsOnly)
        {
            this.Lights.ClearPointLights();
            this.Lights.ClearSpotLights();
            this.spotLight = null;

            this.Lights.AddRange(this.tankAgent1.Instance.Lights);
            this.Lights.AddRange(this.tankAgent2.Instance.Lights);

            if (!modelsOnly)
            {
                {
                    Vector3 lightPosition;
                    Triangle lightTriangle;
                    float lightDistance;
                    if (this.FindTopGroundPosition(0, 1, out lightPosition, out lightTriangle, out lightDistance))
                    {
                        lightPosition.Y += 10f;

                        Vector3 direction = -Vector3.Normalize(lightPosition);

                        this.spotLight = new SceneLightSpot(
                            "Spot the dog",
                            false,
                            Color.Yellow,
                            Color.Yellow,
                            true,
                            lightPosition,
                            direction,
                            25,
                            25,
                            25f);

                        this.Lights.Add(this.spotLight);

                        this.lineDrawer.Active = true;
                        this.lineDrawer.Visible = true;
                    }
                }

                int sep = 10;
                int f = 12;
                int l = (f - 1) * sep;
                l -= (l / 2);

                for (int i = 0; i < f; i++)
                {
                    for (int x = 0; x < f; x++)
                    {
                        Vector3 lightPosition;
                        Triangle lightTriangle;
                        float lightDistance;
                        if (!this.FindTopGroundPosition((i * sep) - l, (x * sep) - l, out lightPosition, out lightTriangle, out lightDistance))
                        {
                            lightPosition = new Vector3((i * sep) - l, 1f, (x * sep) - l);
                        }
                        else
                        {
                            lightPosition.Y += 1f;
                        }

                        var color = new Color4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1.0f);

                        var pointLight = new SceneLightPoint(
                            string.Format("Point {0}", this.Lights.PointLights.Length),
                            false,
                            color,
                            color,
                            true,
                            lightPosition,
                            5f,
                            10f);

                        pointLight.State = rnd.NextFloat(0, 1) >= 0.5f ? 1 : -1;

                        this.Lights.Add(pointLight);
                    }
                }
            }
        }
    }
}
