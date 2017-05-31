using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DeferredTest
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

        private Cursor cursor = null;
        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;
        private TextDrawer statistics = null;
        private Sprite backPannel = null;

        private Model tank = null;
        private ManipulatorController tankController = null;
        private NavigationMeshAgentType tankAgent = new NavigationMeshAgentType();
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Skydom skydom = null;
        private Scenery terrain = null;
        private GroundGardener gardener = null;

        Model tree = null;
        ModelInstanced trees = null;

        private SpriteTexture bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private LineListDrawer lineDrawer = null;
        private TriangleListDrawer terrainGraphDrawer = null;

        private Random rnd = new Random(0);
        private int pointOffset = 0;
        private int spotOffset = 0;
        private bool onlyModels = true;

        private Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;

            #region Cursor

            SpriteDescription cursorDesc = new SpriteDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };
            this.cursor = this.AddCursor(cursorDesc);

            #endregion

            #region Models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Skydom

            sw.Restart();
            this.skydom = this.AddSkydom(new SkydomDescription()
            {
                Name = "Sky",
                ContentPath = "Resources",
                Radius = far,
                Texture = "sunset.dds",
            });
            sw.Stop();
            loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopter

            sw.Restart();
            this.helicopter = this.AddModel(
                "Resources",
                "helicopter.xml",
                new ModelDescription()
                {
                    Name = "Helicopter",
                    CastShadow = true,
                    TextureIndex = 2,
                });
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopters

            sw.Restart();
            this.helicopters = this.AddInstancingModel(
                "Resources",
                "helicopter.xml",
                new ModelInstancedDescription()
                {
                    Name = "Bunch of Helicopters",
                    CastShadow = true,
                    Instances = 2,
                });
            sw.Stop();
            loadingText += string.Format("helicopters: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopters animation plans

            var ap = new AnimationPath();
            ap.AddLoop("roll");
            this.animations.Add("default", new AnimationPlan(ap));

            #endregion

            #region Tank

            sw.Restart();
            this.tank = this.AddModel(
                "Resources",
                "leopard.xml",
                new ModelDescription()
                {
                    Name = "Tank",
                    CastShadow = true,
                });
            this.Lights.AddRange(this.tank.Lights);
            this.tankController = new SteerManipulatorController()
            {
                ArrivingRadius = 5f,
                MaximumForce = 0.01f,
            };
            sw.Stop();
            loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Terrain

            sw.Restart();

            this.tank.Manipulator.SetScale(0.2f, true);

            var tankbbox = this.tank.GetBoundingBox();
            tankAgent.Height = tankbbox.GetY();
            tankAgent.Radius = tankbbox.GetX() * 0.5f;
            tankAgent.MaxClimb = tankbbox.GetY() * 0.55f;

            var navSettings = NavigationMeshGenerationSettings.Default;
            navSettings.Agents = new[]
            {
                tankAgent,
            };

            this.terrain = this.AddScenery(
                "Resources",
                "terrain.xml",
                new GroundDescription()
                {
                    Name = "Terrain",
                    Quadtree = new GroundDescription.QuadtreeDescription()
                    {
                        MaximumDepth = 2,
                    },
                    PathFinder = new GroundDescription.PathFinderDescription()
                    {
                        Settings = navSettings,
                    },
                });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Gardener

            sw.Restart();

            this.gardener = this.AddGardener(
                new GroundGardenerDescription()
                {
                    ContentPath = "Resources/Vegetation",
                    ChannelRed = new GroundGardenerDescription.Channel()
                    {
                        VegetarionTextures = new[] { "grass.png" },
                        Saturation = 20f,
                        StartRadius = 0f,
                        EndRadius = 50f,
                        MinSize = Vector2.One * 0.20f,
                        MaxSize = Vector2.One * 0.25f,
                    }
                });
            sw.Stop();
            loadingText += string.Format("gardener: {0} ", sw.Elapsed.TotalSeconds);

            this.gardener.ParentGround = this.terrain;

            #endregion

            #region Trees

            sw.Restart();
            this.tree = this.AddModel(
                "resources/trees",
                "birch_a.xml",
                new ModelDescription()
                {
                    Name = "Lonely tree",
                    Static = true,
                    CastShadow = true,
                    AlphaEnabled = true,
                    DepthEnabled = true,
                });
            sw.Stop();
            loadingText += string.Format("tree: {0} ", sw.Elapsed.TotalSeconds);

            sw.Restart();
            this.trees = this.AddInstancingModel(
                "resources/trees",
                "birch_b.xml",
                new ModelInstancedDescription()
                {
                    Name = "Bunch of trees",
                    Static = true,
                    CastShadow = true,
                    AlphaEnabled = true,
                    DepthEnabled = true,
                    Instances = 10,
                });
            sw.Stop();
            loadingText += string.Format("trees: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #endregion

            #region Texts

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White), layerHUD);
            this.load = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), layerHUD);
            this.help = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), layerHUD);
            this.statistics = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 10, Color.Red), layerHUD);

            this.title.Text = "Deferred Ligthning test";
            this.load.Text = loadingText;
            this.help.Text = "";
            this.statistics.Text = "";

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

            this.backPannel = this.AddSprite(spDesc, layerHUD - 1);

            #endregion

            #region Object locations

            Vector3 cameraPosition = Vector3.Zero;
            int modelCount = 0;

            #region Tank
            {
                Vector3 p;
                Triangle t;
                float d;
                if (this.terrain.FindTopGroundPosition(20, 40, out p, out t, out d))
                {
                    this.tank.Manipulator.SetPosition(p);
                    this.tank.Manipulator.SetNormal(t.Normal);
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
                if (this.terrain.FindTopGroundPosition(20, -20, out p, out t, out d))
                {
                    p.Y += 10f;
                    this.helicopter.Manipulator.SetPosition(p, true);
                    cameraPosition += p;
                    modelCount++;
                }

                this.helicopter.AnimationController.AddPath(this.animations["default"]);
                this.helicopter.AnimationController.Start();
            }
            #endregion

            #region Helicopters
            {
                for (int i = 0; i < this.helicopters.Count; i++)
                {
                    Vector3 p;
                    Triangle t;
                    float d;
                    if (this.terrain.FindTopGroundPosition((i * 10) - 20, 20, out p, out t, out d))
                    {
                        p.Y += 10f;
                        this.helicopters[i].Manipulator.SetPosition(p, true);
                        cameraPosition += p;
                        modelCount++;
                    }

                    this.helicopters[i].AnimationController.AddPath(this.animations["default"]);
                    this.helicopters[i].AnimationController.Start();
                }
            }
            #endregion

            #region Tree
            {
                Vector3 p;
                Triangle t;
                float d;
                if (this.terrain.FindTopGroundPosition(20, -20, out p, out t, out d))
                {
                    this.tree.Manipulator.SetScale(0.5f, true);
                    this.tree.Manipulator.SetPosition(p, true);
                    cameraPosition += p;
                    modelCount++;
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
                    if (this.terrain.FindTopGroundPosition((i * 10) - 35, 17, out p, out t, out d))
                    {
                        this.trees[i].Manipulator.SetScale(0.5f, true);
                        this.trees[i].Manipulator.SetPosition(p, true);
                        cameraPosition += p;
                        modelCount++;
                    }
                }
            }
            #endregion

            cameraPosition /= (float)modelCount;
            this.Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            this.Camera.LookTo(cameraPosition + Vector3.Up);

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

            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.bufferDrawer = this.AddSpriteTexture(new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = SpriteTextureChannelsEnum.NoAlpha,
            },
            layerEffects);

            this.bufferDrawer.Visible = false;

            #endregion

            #region Light line drawer

            this.lineDrawer = this.AddLineListDrawer(new LineListDrawerDescription() { DepthEnabled = true }, 1000, layerEffects);
            this.lineDrawer.Visible = false;

            #endregion

            #region Terrain grapth drawer

            this.terrainGraphDrawer = this.AddTriangleListDrawer(new TriangleListDrawerDescription(), MaxGridDrawer, layerEffects);
            this.terrainGraphDrawer.Visible = false;

            var nodes = this.terrain.GetNodes(tankAgent);
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

                    this.terrainGraphDrawer.AddTriangles(color, tris);
                }
            }

            #endregion

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
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            base.Update(gameTime);

            Ray cursorRay = this.GetPickingRay();

            #region Cursor picking and positioning

            Vector3 position;
            Triangle triangle;
            float distance;
            bool picked = this.terrain.PickNearest(ref cursorRay, true, out position, out triangle, out distance);

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F12))
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

            {
                if (this.Game.Input.KeyJustReleased(Keys.F1))
                {
                    var colorMap = this.Renderer.GetResource(SceneRendererResultEnum.ColorMap);

                    //Colors
                    this.bufferDrawer.Texture = colorMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.help.Text = "Colors";

                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F2))
                {
                    var normalMap = this.Renderer.GetResource(SceneRendererResultEnum.NormalMap);

                    if (this.bufferDrawer.Texture == normalMap &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Power
                        this.bufferDrawer.Texture = normalMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Power";
                    }
                    else
                    {
                        //Normals
                        this.bufferDrawer.Texture = normalMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Normals";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F3))
                {
                    var depthMap = this.Renderer.GetResource(SceneRendererResultEnum.DepthMap);

                    if (this.bufferDrawer.Texture == depthMap &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Factor
                        this.bufferDrawer.Texture = depthMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Intensity";
                    }
                    else
                    {
                        //Position
                        this.bufferDrawer.Texture = depthMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Position";
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
                    this.bufferDrawer.Texture = shadowMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Red;
                    this.bufferDrawer.Visible = true;
                    this.help.Text = "Shadow map";
                }
                else
                {
                    this.help.Text = "The Shadow map is empty";
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                var lightMap = this.Renderer.GetResource(SceneRendererResultEnum.LightMap);

                if (lightMap != null)
                {
                    //Light map
                    this.bufferDrawer.Texture = lightMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.bufferDrawer.Visible = true;
                    this.help.Text = "Light map";
                }
                else
                {
                    this.help.Text = "The Light map is empty";
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
                this.tank.Active = this.tank.Visible = !this.tank.Visible;
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
                this.helicopter.TextureIndex++;

                if (this.helicopter.TextureIndex >= this.helicopter.TextureCount)
                {
                    //Loop
                    this.helicopter.TextureIndex = 0;
                }
            }

            #endregion

            #region Tank

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (picked)
                {
                    var p = this.terrain.FindPath(this.tankAgent, this.tank.Manipulator.Position, position, true, this.tank.Manipulator.LinearVelocity * gameTime.ElapsedSeconds);
                    if (p != null)
                    {
                        this.tankController.Follow(new SegmentPath(p.ReturnPath.ToArray()));
                        this.tank.Manipulator.LinearVelocity = 8;
                    }
                }
            }

            this.tankController.UpdateManipulator(gameTime, this.tank.Manipulator);

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
                this.lineDrawer.SetLines(Color.Yellow, Line3D.CreateWiredFrustum(this.Camera.Frustum));
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

                this.lineDrawer.SetLines(Color.White, this.spotLight.GetVolume(10));
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
                this.RenderMode,
                this.Lights.DirectionalLights.Length,
                this.Lights.PointLights.Length,
                this.Lights.SpotLights.Length,
                this.Lights.ShadowCastingLights.Length);

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

        private void CreateLights(bool modelsOnly)
        {
            this.Lights.ClearPointLights();
            this.Lights.ClearSpotLights();
            this.spotLight = null;

            this.Lights.AddRange(this.tank.Lights);

            if (!modelsOnly)
            {
                {
                    Vector3 lightPosition;
                    Triangle lightTriangle;
                    float lightDistance;
                    if (this.terrain.FindTopGroundPosition(0, 1, out lightPosition, out lightTriangle, out lightDistance))
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
                        if (!this.terrain.FindTopGroundPosition((i * sep) - l, (x * sep) - l, out lightPosition, out lightTriangle, out lightDistance))
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
