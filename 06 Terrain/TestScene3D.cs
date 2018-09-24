using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Terrain
{
    using Terrain.AI;
    using Terrain.AI.Agents;
    using Terrain.Controllers;
    using Terrain.Emitters;

    public class TestScene3D : Scene
    {
        private const int MaxPickingTest = 1000;
        private const int MaxGridDrawer = 10000;

        private readonly int layerHud = 99;
        private readonly int layerGameHud = 50;
        private readonly int layerObjects = 0;
        private readonly int layerTerrain = 1;
        private readonly int layerEffects = 2;

        private readonly Random rnd = new Random();

        private bool walkMode = false;
        private readonly float walkerVelocity = 8f;
        private SceneObject followTarget;
        private bool follow = false;
        private Agent walkerAgentType = null;

        private bool useDebugTex = false;
        private SceneRendererResultEnum shadowResult = SceneRendererResultEnum.ShadowMapDirectional;
        private SceneObject<SpriteTexture> shadowMapDrawer = null;
        private EngineShaderResourceView debugTex = null;
        private int graphIndex = -1;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> load = null;
        private SceneObject<TextDrawer> stats = null;
        private SceneObject<TextDrawer> counters1 = null;
        private SceneObject<TextDrawer> counters2 = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<SpriteProgressBar> hProgressBar = null;
        private SceneObject<SpriteProgressBar> t1ProgressBar = null;
        private SceneObject<SpriteProgressBar> t2ProgressBar = null;

        private SceneObject<Model> cursor3D = null;
        private SceneObject<Cursor> cursor2D = null;

        private SceneObject<Model> tankP1 = null;
        private SceneObject<Model> tankP2 = null;
        private Agent tankAgentType = null;
        private Vector3 tankLeftCat = Vector3.Zero;
        private Vector3 tankRightCat = Vector3.Zero;

        private SceneObject<LensFlare> lensFlare = null;
        private SceneObject<Skydom> skydom = null;
        private SceneObject<SkyPlane> clouds = null;
        private SceneObject<Scenery> terrain = null;
        private SceneObject<GroundGardener> gardener = null;
        private Vector3 windDirection = Vector3.UnitX;
        private readonly float windStrength = 1f;
        private readonly List<Line3D> oks = new List<Line3D>();
        private readonly List<Line3D> errs = new List<Line3D>();

        private SceneObject<Model> heliport = null;
        private SceneObject<Model> garage = null;
        private SceneObject<ModelInstanced> obelisk = null;
        private SceneObject<ModelInstanced> rocks = null;
        private SceneObject<ModelInstanced> tree1 = null;
        private SceneObject<ModelInstanced> tree2 = null;
        private Color4 objColor = Color.Magenta;
        private bool objNotSet = true;

        private SceneObject<Model> helicopter = null;
        private readonly HeliManipulatorController helicopterController = null;
        private Vector3 helicopterHeightOffset = (Vector3.Up * 15f);
        private Color4 gridColor = new Color4(Color.LightSeaGreen.ToColor3(), 0.5f);
        private Color4 curvesColor = Color.Red;
        private Color4 pointsColor = Color.Blue;
        private Color4 segmentsColor = new Color4(Color.Cyan.ToColor3(), 0.8f);
        private Color4 hAxisColor = Color.YellowGreen;
        private Color4 wAxisColor = Color.White;
        private Color4 velocityColor = Color.Green;

        private SceneObject<LineListDrawer> staticObjLineDrawer = null;
        private SceneObject<LineListDrawer> movingObjLineDrawer = null;
        private SceneObject<LineListDrawer> lightsVolumeDrawer = null;
        private SceneObject<LineListDrawer> curveLineDrawer = null;
        private SceneObject<LineListDrawer> terrainLineDrawer = null;
        private SceneObject<LineListDrawer> terrainPointDrawer = null;
        private SceneObject<TriangleListDrawer> terrainGraphDrawer = null;

        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private Brain agentManager = null;
        private TankAIAgent tankP1Agent = null;
        private TankAIAgent tankP2Agent = null;
        private HelicopterAIAgent helicopterAgent = null;

        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private ParticleSystemDescription pProjectile = null;
        private ParticleSystemDescription pExplosion = null;
        private ParticleSystemDescription pSmokeExplosion = null;
        private SceneObject<ParticleManager> pManager = null;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private readonly Dictionary<string, double> initDurationDict = new Dictionary<string, double>();
        private int initDurationIndex = 0;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }
        ~TestScene3D()
        {
            Dispose(false);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.debugTex != null)
                {
                    this.debugTex.Dispose();
                    this.debugTex = null;
                }
            }

            base.Dispose(disposing);
        }

        public override void Initialize()
        {
            base.Initialize();

            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var taskUI = InitializeUI();
            var taskWalker = InitializeWalker();
            var taskDebug = InitializeDebug();
            var taskParticles = InitializeParticles();
            var taskLensFlare = InitializeLensFlare();
            var taskHelicopter = InitializeHelicopter();
            var taskTanks = InitializeTanks();
            var taskHeliport = InitializeHeliport();
            var taskGarage = InitializeGarage();
            var taskObelisk = InitializeObelisk();
            var taskRocks = InitializeRocks();
            var taskTrees = InitializeTrees();
            var taskSkydom = InitializeSkydom();
            var taskClouds = InitializeClouds();
            var taskTerrain = InitializeTerrain();
            var taskGardener = InitializeGardener();

            initDurationDict.Add("UI", taskUI.Result);
            initDurationDict.Add("Walker", taskWalker.Result);
            initDurationDict.Add("Debug", taskDebug.Result);
            initDurationDict.Add("Particles", taskParticles.Result);
            initDurationDict.Add("Lens Flare", taskLensFlare.Result);
            initDurationDict.Add("Helicopter", taskHelicopter.Result);
            initDurationDict.Add("Tanks", taskTanks.Result);
            initDurationDict.Add("Heliport", taskHeliport.Result);
            initDurationDict.Add("Garage", taskGarage.Result);
            initDurationDict.Add("Obelisk", taskObelisk.Result);
            initDurationDict.Add("Rocks", taskRocks.Result);
            initDurationDict.Add("Trees", taskTrees.Result);
            initDurationDict.Add("Skydom", taskSkydom.Result);
            initDurationDict.Add("Clouds", taskClouds.Result);
            initDurationDict.Add("Terrain", taskTerrain.Result);
            initDurationDict.Add("Gardener", taskGardener.Result);

            var taskPathFinding = InitializePathFinding();

            initDurationDict.Add("Path Finding", taskPathFinding.Result);

            initDurationDict.Add("TOTAL", initDurationDict.Select(i => i.Value).Sum());
            initDurationDict.Add("REAL", sw.Elapsed.TotalSeconds);

            initDurationIndex = initDurationDict.Keys.Count - 2;

            SetLoadText(initDurationIndex);

            InitializeLights();

            this.agentManager = new Brain(this);

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000f;
        }
        private Task<double> InitializeUI()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            {
                this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, this.layerHud);
                this.load = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, this.layerHud);
                this.stats = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, this.layerHud);
                this.counters1 = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 10, Color.GreenYellow), SceneObjectUsageEnum.UI, this.layerHud);
                this.counters2 = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 10, Color.GreenYellow), SceneObjectUsageEnum.UI, this.layerHud);

                this.title.Instance.Text = "Terrain collision and trajectories test";
                this.load.Instance.Text = "";
                this.stats.Instance.Text = "";
                this.counters1.Instance.Text = "";
                this.counters2.Instance.Text = "";

                this.title.Instance.Position = Vector2.Zero;
                this.load.Instance.Position = new Vector2(0, 24);
                this.stats.Instance.Position = new Vector2(0, 46);
                this.counters1.Instance.Position = new Vector2(0, 68);
                this.counters2.Instance.Position = new Vector2(0, 90);

                var spDesc = new SpriteDescription()
                {
                    Name = "Back Pannel",
                    AlphaEnabled = true,
                    Width = this.Game.Form.RenderWidth,
                    Height = this.counters2.Instance.Top + this.counters2.Instance.Height + 3,
                    Color = new Color4(0, 0, 0, 0.75f),
                };

                this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHud - 1);

                var spbDesc = new SpriteProgressBarDescription()
                {
                    Name = "Progress bar",
                    Width = 50,
                    Height = 5,
                    BaseColor = Color.Red,
                    ProgressColor = Color.Green,
                };

                this.hProgressBar = this.AddComponent<SpriteProgressBar>(spbDesc, SceneObjectUsageEnum.UI, layerGameHud);
                this.t1ProgressBar = this.AddComponent<SpriteProgressBar>(spbDesc, SceneObjectUsageEnum.UI, layerGameHud);
                this.t2ProgressBar = this.AddComponent<SpriteProgressBar>(spbDesc, SceneObjectUsageEnum.UI, layerGameHud);

                this.hProgressBar.Instance.Top = 120;
                this.t1ProgressBar.Instance.Top = 120;
                this.t2ProgressBar.Instance.Top = 120;

                this.hProgressBar.Instance.Left = 5;
                this.t1ProgressBar.Instance.Left = 135;
                this.t2ProgressBar.Instance.Left = 270;
            }

            {
                var c3DDesc = new ModelDescription()
                {
                    Name = "Cursor3D",
                    DeferredEnabled = false,
                    CastShadow = false,
                    DepthEnabled = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "resources/cursor",
                        ModelContentFilename = "cursor.xml",
                    }
                };
                this.cursor3D = this.AddComponent<Model>(c3DDesc, SceneObjectUsageEnum.UI, this.layerHud);
            }

            {
                var c2DDesc = new CursorDescription()
                {
                    Name = "Cursor2D",
                    ContentPath = "resources/Cursor",
                    Textures = new[] { "target.png" },
                    Color = Color.Red,
                    Width = 16,
                    Height = 16,
                };
                this.cursor2D = this.AddComponent<Cursor>(c2DDesc, SceneObjectUsageEnum.UI, this.layerHud + 1);
                this.cursor2D.Instance.Color = Color.Red;
                this.cursor2D.Visible = false;
            }

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeWalker()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.walkerAgentType = new Agent()
            {
                Name = "Walker type",
                Height = 1f,
                Radius = 0.2f,
                MaxClimb = 0.9f,
            };

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeDebug()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            {
                var desc = new LineListDrawerDescription()
                {
                    Name = "++DEBUG++ Lights",
                    DepthEnabled = true,
                    Count = 5000,
                };
                this.lightsVolumeDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, this.layerEffects);
                this.lightsVolumeDrawer.Visible = false;
            }

            #region DEBUG Shadow Map
            {
                int width = 300;
                int height = 300;
                int smLeft = this.Game.Form.RenderWidth - width;
                int smTop = this.Game.Form.RenderHeight - height;
                var stDescription = new SpriteTextureDescription()
                {
                    Name = "++DEBUG++ Shadow Map",
                    Left = smLeft,
                    Top = smTop,
                    Width = width,
                    Height = height,
                    Channel = SpriteTextureChannelsEnum.Red,
                };
                this.shadowMapDrawer = this.AddComponent<SpriteTexture>(stDescription, SceneObjectUsageEnum.UI, this.layerHud);
                this.shadowMapDrawer.Visible = false;
                this.shadowMapDrawer.DeferredEnabled = false;

                this.debugTex = this.Game.ResourceManager.CreateResource(@"Resources\uvtest.png");
            }
            #endregion

            #region DEBUG Path finding Graph
            {
                var desc = new TriangleListDrawerDescription()
                {
                    Name = "++DEBUG++ Path finding Graph",
                    Count = MaxGridDrawer
                };
                this.terrainGraphDrawer = this.AddComponent<TriangleListDrawer>(desc, SceneObjectUsageEnum.None, this.layerEffects);
                this.terrainGraphDrawer.Visible = false;
            }
            #endregion

            #region DEBUG Picking test
            {
                var desc = new LineListDrawerDescription()
                {
                    Name = "++DEBUG++ Picking test",
                    Count = MaxPickingTest
                };
                this.terrainPointDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, this.layerEffects);
                this.terrainPointDrawer.Visible = false;
            }
            #endregion

            #region DEBUG Trajectory
            {
                var desc = new LineListDrawerDescription()
                {
                    Name = "++DEBUG++ Trajectory",
                    Count = 20000
                };
                this.curveLineDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, this.layerEffects);
                this.curveLineDrawer.Visible = false;
            }
            #endregion

            #region DEBUG Helicopter manipulator
            {
                var desc = new LineListDrawerDescription()
                {
                    Name = "++DEBUG++ Helicopter manipulator",
                    Count = 1000
                };
                this.movingObjLineDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, this.layerEffects);
                this.movingObjLineDrawer.Visible = false;
            }
            #endregion

            #region DEBUG static volumes
            {
                var desc = new LineListDrawerDescription()
                {
                    Name = "++DEBUG++ Static Volumes",
                    Count = 20000
                };
                this.staticObjLineDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, layerEffects);
                this.staticObjLineDrawer.Visible = false;
            }
            #endregion

            #region DEBUG Ground position test
            {
                var desc = new LineListDrawerDescription()
                {
                    Name = "++DEBUG++ Ground position test",
                    Count = 10000
                };
                this.terrainLineDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, this.layerEffects);
                this.terrainLineDrawer.Visible = false;
            }
            #endregion

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeLights()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[0].CastShadow = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = true;

            //this.Lights.ShadowLDDistance = 100f;
            //this.Lights.ShadowHDDistance = 25f;

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeLensFlare()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var lfDesc = new LensFlareDescription()
            {
                Name = "Flares",
                ContentPath = "resources/Flare",
                GlowTexture = "lfGlow.png",
                Flares = new[]
                {
                    new LensFlareDescription.Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 0.3f, 0.4f, new Color(100, 255, 200), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.2f, 1.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "lfFlare1.png"),

                    new LensFlareDescription.Flare(-0.3f, 0.7f, new Color(200,  50,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "lfFlare2.png"),

                    new LensFlareDescription.Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            };
            this.lensFlare = this.AddComponent<LensFlare>(lfDesc, SceneObjectUsageEnum.None, this.layerEffects);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeHelicopter()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var hDesc = new ModelDescription()
            {
                Name = "Helicopter",
                CastShadow = true,
                Static = false,
                TextureIndex = 0,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Helicopter",
                    ModelContentFilename = "M24.xml",
                }
            };
            this.helicopter = this.AddComponent<Model>(hDesc, SceneObjectUsageEnum.Agent, this.layerObjects);

            this.helicopter.Transform.SetScale(0.15f);
            this.helicopter.Transform.UpdateInternals(true);

            this.Lights.AddRange(this.helicopter.Instance.Lights);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTanks()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var tDesc = new ModelDescription()
            {
                Name = "Tank",
                CastShadow = true,
                Static = false,
                Optimize = false,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Leopard",
                    ModelContentFilename = "Leopard.xml",
                },
                TransformNames = new[] { "Barrel-mesh", "Turret-mesh", "Hull-mesh" },
                TransformDependences = new[] { 1, 2, -1 },
            };
            this.tankP1 = this.AddComponent<Model>(tDesc, SceneObjectUsageEnum.Agent, this.layerObjects);
            this.tankP2 = this.AddComponent<Model>(tDesc, SceneObjectUsageEnum.Agent, this.layerObjects);

            this.tankP1.Transform.SetScale(0.2f, true);
            this.tankP1.Transform.UpdateInternals(true);

            this.tankP2.Transform.SetScale(0.2f, true);
            this.tankP2.Transform.UpdateInternals(true);

            var tankbbox = this.tankP1.Geometry.GetBoundingBox();

            // Initialize dust generation relative positions
            this.tankLeftCat = new Vector3(tankbbox.Maximum.X, tankbbox.Minimum.Y, tankbbox.Maximum.Z);
            this.tankRightCat = new Vector3(tankbbox.Minimum.X, tankbbox.Minimum.Y, tankbbox.Maximum.Z);

            // Initialize agent
            this.tankAgentType = new Agent()
            {
                Name = "Tank type",
                Height = tankbbox.GetY(),
                Radius = tankbbox.GetX() * 0.5f,
                MaxClimb = tankbbox.GetY() * 0.1f,
            };

            this.Lights.AddRange(this.tankP1.Instance.Lights);
            this.Lights.AddRange(this.tankP2.Instance.Lights);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeHeliport()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var hpDesc = new ModelDescription()
            {
                Name = "Heliport",
                CastShadow = true,
                Static = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Heliport",
                    ModelContentFilename = "Heliport.xml",
                }
            };
            this.heliport = this.AddComponent<Model>(hpDesc, SceneObjectUsageEnum.None, this.layerObjects);

            this.Lights.AddRange(this.heliport.Instance.Lights);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeGarage()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var gDesc = new ModelDescription()
            {
                Name = "Garage",
                CastShadow = true,
                Static = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Garage",
                    ModelContentFilename = "Garage.xml",
                }
            };
            this.garage = this.AddComponent<Model>(gDesc, SceneObjectUsageEnum.None, this.layerObjects);

            this.Lights.AddRange(this.garage.Instance.Lights);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeObelisk()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var oDesc = new ModelInstancedDescription()
            {
                Name = "Obelisk",
                CastShadow = true,
                Static = true,
                Instances = 4,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Obelisk",
                    ModelContentFilename = "Obelisk.xml",
                }
            };
            this.obelisk = this.AddComponent<ModelInstanced>(oDesc, SceneObjectUsageEnum.None, this.layerObjects);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeRocks()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var rDesc = new ModelInstancedDescription()
            {
                Name = "Rocks",
                CastShadow = true,
                Static = true,
                Instances = 250,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Rocks",
                    ModelContentFilename = "boulder.xml",
                }
            };
            this.rocks = this.AddComponent<ModelInstanced>(rDesc, SceneObjectUsageEnum.None, this.layerObjects);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTrees()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var t1Desc = new ModelInstancedDescription()
            {
                Name = "birch_a",
                CastShadow = true,
                Static = true,
                AlphaEnabled = true,
                Instances = 100,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Trees",
                    ModelContentFilename = "birch_a.xml",
                }
            };
            var t2Desc = new ModelInstancedDescription()
            {
                Name = "birch_b",
                CastShadow = true,
                Static = true,
                AlphaEnabled = true,
                Instances = 100,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Trees",
                    ModelContentFilename = "birch_b.xml",
                }
            };
            this.tree1 = this.AddComponent<ModelInstanced>(t1Desc, SceneObjectUsageEnum.None, this.layerTerrain);
            this.tree2 = this.AddComponent<ModelInstanced>(t2Desc, SceneObjectUsageEnum.None, this.layerTerrain);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeSkydom()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.skydom = this.AddComponent<Skydom>(new SkydomDescription()
            {
                Name = "Skydom",
                ContentPath = "resources/Skydom",
                Texture = "sunset.dds",
                Radius = this.Camera.FarPlaneDistance,
            });

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeClouds()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.clouds = this.AddComponent<SkyPlane>(new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "Resources/clouds",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                Mode = SkyPlaneMode.Perturbed,
                MaxBrightness = 0.8f,
                MinBrightness = 0.1f,
                Repeat = 5,
                Velocity = 1,
                Direction = new Vector2(1, 1),
            });

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTerrain()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var terrainDescription = new GroundDescription()
            {
                Name = "Terrain",
                Quadtree = new GroundDescription.QuadtreeDescription()
                {
                    MaximumDepth = 1,
                },
                CastShadow = true,
                Static = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Terrain",
                    ModelContentFilename = "two_levels.xml",
                }
            };
            this.terrain = this.AddComponent<Scenery>(terrainDescription, SceneObjectUsageEnum.Ground, this.layerTerrain);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeGardener()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var grDesc = new GroundGardenerDescription()
            {
                Name = "Grass",
                ContentPath = "resources/Terrain/Foliage/Billboard",
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass_v.dds" },
                    Saturation = 10f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = new Vector2(0.25f, 0.25f),
                    MaxSize = new Vector2(0.5f, 0.5f),
                    Delta = new Vector3(0.2f, 0f, 0.2f),
                    WindEffect = 0.5f,
                    Seed = 1,
                    Count = 4,
                }
            };
            this.gardener = this.AddComponent<GroundGardener>(grDesc, SceneObjectUsageEnum.None, this.layerTerrain);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeParticles()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.pPlume = ParticleSystemDescription.InitializeSmokePlume("resources/particles", "smoke.png");
            this.pFire = ParticleSystemDescription.InitializeFire("resources/particles", "fire.png");
            this.pDust = ParticleSystemDescription.InitializeDust("resources/particles", "smoke.png");
            this.pProjectile = ParticleSystemDescription.InitializeProjectileTrail("resources/particles", "smoke.png");
            this.pExplosion = ParticleSystemDescription.InitializeExplosion("resources/particles", "fire.png");
            this.pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("resources/particles", "smoke.png");

            this.pManager = this.AddComponent<ParticleManager>(new ParticleManagerDescription() { Name = "Particle Manager" }, SceneObjectUsageEnum.None, layerEffects);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializePathFinding()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            Random posRnd = new Random(1);

            this.SetGround(this.terrain, true);

            //Rocks
            {
                for (int i = 0; i < this.rocks.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var scale = 1f;
                        if (i < 5)
                        {
                            scale = posRnd.NextFloat(2f, 5f);
                        }
                        else if (i < 30)
                        {
                            scale = posRnd.NextFloat(0.5f, 2f);
                        }
                        else
                        {
                            scale = posRnd.NextFloat(0.1f, 0.2f);
                        }

                        var rockInstance = this.rocks.GetComponent<ITransformable3D>(i);

                        rockInstance.Manipulator.SetPosition(r.Position);
                        rockInstance.Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi));
                        rockInstance.Manipulator.SetScale(scale);
                        rockInstance.Manipulator.UpdateInternals(true);
                    }
                }
            }

            //Trees
            {
                for (int i = 0; i < this.tree1.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var treeInstance = this.tree1.GetComponent<ITransformable3D>(i);

                        treeInstance.Manipulator.SetPosition(r.Position);
                        treeInstance.Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0);
                        treeInstance.Manipulator.SetScale(posRnd.NextFloat(0.25f, 0.75f));
                        treeInstance.Manipulator.UpdateInternals(true);
                    }
                }

                for (int i = 0; i < this.tree2.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var treeInstance = this.tree2.GetComponent<ITransformable3D>(i);

                        treeInstance.Manipulator.SetPosition(r.Position);
                        treeInstance.Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0);
                        treeInstance.Manipulator.SetScale(posRnd.NextFloat(0.25f, 0.75f));
                        treeInstance.Manipulator.UpdateInternals(true);
                    }
                }
            }

            //Heliport
            {
                if (this.FindTopGroundPosition(75, 75, out PickingResult<Triangle> r))
                {
                    this.heliport.Transform.SetPosition(r.Position);
                    this.heliport.Transform.UpdateInternals(true);
                }
            }

            //Garage
            {
                if (this.FindTopGroundPosition(-10, -40, out PickingResult<Triangle> r))
                {
                    this.garage.Transform.SetPosition(r.Position);
                    this.garage.Transform.SetRotation(MathUtil.PiOverFour * 0.5f + MathUtil.Pi, 0, 0);
                    this.garage.Transform.UpdateInternals(true);
                }
            }

            //Obelisk
            {
                for (int i = 0; i < this.obelisk.Count; i++)
                {
                    int ox = i == 0 || i == 2 ? 1 : -1;
                    int oy = i == 0 || i == 1 ? 1 : -1;

                    if (this.FindTopGroundPosition(ox * 50, oy * 50, out PickingResult<Triangle> r))
                    {
                        var obeliskInstance = this.obelisk.GetComponent<ITransformable3D>(i);

                        obeliskInstance.Manipulator.SetPosition(r.Position);
                        obeliskInstance.Manipulator.SetScale(1.5f);
                        obeliskInstance.Manipulator.UpdateInternals(true);
                    }
                }
            }

            this.AttachToGround(this.rocks, false);
            this.AttachToGround(this.tree1, false);
            this.AttachToGround(this.tree2, false);
            this.AttachToGround(this.heliport, true);
            this.AttachToGround(this.garage, true);
            this.AttachToGround(this.obelisk, true);

            var navSettings = BuildSettings.Default;
            navSettings.Agents = new[]
            {
                walkerAgentType,
                tankAgentType,
            };
            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new PathFinderDescription(navSettings, nvInput);

            sw.Stop();
            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }

        public override void Initialized()
        {
            base.Initialized();

            StartHelicopter();
            StartTanks();
            StartDebug();

            this.Camera.Goto(this.helicopter.Transform.Position + Vector3.One * 25f);
            this.Camera.LookTo(0, 10, 0);

            this.gardener.Instance.SetWind(this.windDirection, this.windStrength);
        }
        private void StartHelicopter()
        {
            // Set position
            var sceneryUsage = SceneObjectUsageEnum.CoarsePathFinding | SceneObjectUsageEnum.FullPathFinding;
            {
                var ray = this.GetTopDownRay(this.heliport.Transform.Position);
                if (this.PickNearest(ref ray, true, sceneryUsage, out PickingResult<Triangle> r))
                {
                    this.helicopter.Transform.SetPosition(r.Position);
                    this.helicopter.Transform.SetNormal(r.Item.Normal);
                }

                var hp = new AnimationPath();
                hp.AddLoop("roll");
                this.animations.Add("heli_default", new AnimationPlan(hp));
            }

            // Register animation paths
            AnimationPath ap = new AnimationPath();
            ap.AddLoop("default");
            this.animations.Add("default", new AnimationPlan(ap));

            // Set animation
            this.helicopter.Instance.AnimationController.SetPath(this.animations["heli_default"]);
            this.helicopter.Instance.AnimationController.TimeDelta = 3f;
            this.helicopter.Instance.AnimationController.Start();

            // Define weapons
            var h1W = new WeaponDescription() { Name = "Missile", Damage = 100, Cadence = 5f, Range = 100 };
            var h2W = new WeaponDescription() { Name = "Gatling", Damage = 10, Cadence = 0.1f, Range = 50 };

            // Define stats
            var hStats = new HelicopterAIStatsDescription()
            {
                PrimaryWeapon = h1W,
                SecondaryWeapon = h2W,
                Life = 50,
                SightDistance = 100,
                SightAngle = 90,
                FlightHeight = 25,
            };

            this.helicopterAgent = new HelicopterAIAgent(this.agentManager, null, this.helicopter, hStats);

            // Adds agent to scene
            this.AddComponent(this.helicopterAgent, new SceneObjectDescription() { }, SceneObjectUsageEnum.None);

            // Register events
            this.helicopterAgent.Attacking += Agent_Attacking;
            this.helicopterAgent.Damaged += Agent_Damaged;
            this.helicopterAgent.Destroyed += Agent_Destroyed;

            // Adds agent to agent manager to team 0
            this.agentManager.AddAgent(0, this.helicopterAgent);

            // Define patrolling check points
            Vector3[] hCheckPoints = new Vector3[]
            {
                new Vector3(+60, 20, +60),
                new Vector3(+60, 20, -60),
                new Vector3(-70, 20, +70),
                new Vector3(-60, 20, -60),
                new Vector3(+00, 25, +00),
            };

            // Define behaviors
            this.helicopterAgent.PatrolBehavior.InitPatrollingBehavior(hCheckPoints, 5, 8);
            this.helicopterAgent.AttackBehavior.InitAttackingBehavior(15, 10);
            this.helicopterAgent.RetreatBehavior.InitRetreatingBehavior(new Vector3(75, 0, 75), 12);
        }
        private void StartTanks()
        {
            var sceneryUsage = SceneObjectUsageEnum.CoarsePathFinding | SceneObjectUsageEnum.FullPathFinding;

            {
                var ray = this.GetTopDownRay(-60, -60);
                if (this.PickNearest(ref ray, true, sceneryUsage, out PickingResult<Triangle> r))
                {
                    this.tankP1.Transform.SetPosition(r.Position);
                    this.tankP1.Transform.SetNormal(r.Item.Normal);
                }
            }

            {
                var ray = this.GetTopDownRay(-70, 70);
                if (this.PickNearest(ref ray, true, sceneryUsage, out PickingResult<Triangle> r))
                {
                    this.tankP2.Transform.SetPosition(r.Position);
                    this.tankP2.Transform.SetNormal(r.Item.Normal);
                }
            }

            // Define weapons
            var t1W = new WeaponDescription() { Name = "Machine Gun", Damage = 0.5f, Cadence = 0.05f, Range = 50 };
            var t2W = new WeaponDescription() { Name = "Cannon", Damage = 50, Cadence = 2f, Range = 100 };

            // Define stats
            var tStats = new AIStatsDescription()
            {
                PrimaryWeapon = t1W,
                SecondaryWeapon = t2W,
                Life = 300,
                SightDistance = 80,
                SightAngle = 145,
            };

            // Initialize agents
            this.tankP1Agent = new TankAIAgent(this.agentManager, this.tankAgentType, this.tankP1, tStats);
            this.tankP2Agent = new TankAIAgent(this.agentManager, this.tankAgentType, this.tankP2, tStats);

            this.tankP1Agent.SceneObject.Name = "Tank1";
            this.tankP2Agent.SceneObject.Name = "Tank2";

            this.AddComponent(this.tankP1Agent, new SceneObjectDescription() { }, SceneObjectUsageEnum.None);
            this.AddComponent(this.tankP2Agent, new SceneObjectDescription() { }, SceneObjectUsageEnum.None);

            // Register events
            this.tankP1Agent.Moving += Agent_Moving;
            this.tankP1Agent.Attacking += Agent_Attacking;
            this.tankP1Agent.Damaged += Agent_Damaged;
            this.tankP1Agent.Destroyed += Agent_Destroyed;

            this.tankP2Agent.Moving += Agent_Moving;
            this.tankP2Agent.Attacking += Agent_Attacking;
            this.tankP2Agent.Damaged += Agent_Damaged;
            this.tankP2Agent.Destroyed += Agent_Destroyed;
            this.agentManager.AddAgent(1, this.tankP1Agent);
            this.agentManager.AddAgent(1, this.tankP2Agent);

            // Define check-points
            Vector3[] t1CheckPoints = new Vector3[]
            {
                new Vector3(+60, 0, -60),
                new Vector3(-60, 0, -60),
                new Vector3(+60, 0, +60),
                new Vector3(-70, 0, +70),
            };

            Vector3[] t2CheckPoints = new Vector3[]
            {
                new Vector3(+60, 0, -60),
                new Vector3(+60, 0, +60),
                new Vector3(-70, 0, +70),
                new Vector3(-60, 0, -60),
                new Vector3(+00, 0, +00),
            };

            //Adjust check-points
            for (int i = 0; i < t1CheckPoints.Length; i++)
            {
                if (this.FindNearestGroundPosition(t1CheckPoints[i], out PickingResult<Triangle> r))
                {
                    t1CheckPoints[i] = r.Position;
                }
            }

            for (int i = 0; i < t2CheckPoints.Length; i++)
            {
                if (this.FindNearestGroundPosition(t2CheckPoints[i], out PickingResult<Triangle> r))
                {
                    t2CheckPoints[i] = r.Position;
                }
            }

            // Initialize behaviors
            this.tankP1Agent.PatrolBehavior.InitPatrollingBehavior(t1CheckPoints, 10, 5);
            this.tankP1Agent.AttackBehavior.InitAttackingBehavior(7, 10);
            this.tankP1Agent.RetreatBehavior.InitRetreatingBehavior(new Vector3(-10, 0, -40), 10);

            this.tankP2Agent.PatrolBehavior.InitPatrollingBehavior(t2CheckPoints, 10, 5);
            this.tankP2Agent.AttackBehavior.InitAttackingBehavior(7, 10);
            this.tankP2Agent.RetreatBehavior.InitRetreatingBehavior(new Vector3(-10, 0, -40), 10);
        }
        private void StartDebug()
        {
            // Ground position test
            {
                var bbox = this.terrain.Geometry.GetBoundingBox();

                float sep = 2.1f;
                for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += sep)
                {
                    for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += sep)
                    {
                        if (this.FindTopGroundPosition(x, z, out PickingResult<Triangle> r))
                        {
                            this.oks.Add(new Line3D(r.Position, r.Position + Vector3.Up));
                        }
                        else
                        {
                            this.errs.Add(new Line3D(x, 10, z, x, -10, z));
                        }
                    }
                }

                if (this.oks.Count > 0)
                {
                    this.terrainLineDrawer.Instance.AddLines(Color.Green, this.oks.ToArray());
                }
                if (this.errs.Count > 0)
                {
                    this.terrainLineDrawer.Instance.AddLines(Color.Red, this.errs.ToArray());
                }
            }

            // Axis
            this.curveLineDrawer.Instance.SetLines(this.wAxisColor, Line3D.CreateAxis(Matrix.Identity, 20f));
            this.curveLineDrawer.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            base.Update(gameTime);

            var pickingRay = this.GetPickingRay();

            UpdateAgent();
            UpdateCursor(pickingRay);
            UpdateCamera(gameTime, pickingRay);
            UpdateTanks(pickingRay);
            UpdateHelicopter();

            UpdateDebug(pickingRay);
        }
        private void UpdateAgent()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Z))
            {
                this.walkMode = !this.walkMode;

                if (this.walkMode)
                {
                    this.Camera.Mode = CameraModes.FirstPerson;
                    this.Camera.MovementDelta = this.walkerVelocity;
                    this.Camera.SlowMovementDelta = this.walkerVelocity * 0.05f;
                    this.cursor3D.Visible = false;
                    this.cursor2D.Visible = true;

                    var pos = this.heliport.Transform.Position;
                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var pPos = r.Position;
                        pPos.Y += this.walkerAgentType.Height;
                        this.Camera.Position = pPos;
                    }
                }
                else
                {
                    this.Camera.Mode = CameraModes.Free;
                    this.Camera.MovementDelta = 20.5f;
                    this.Camera.SlowMovementDelta = 1f;
                    this.cursor3D.Visible = true;
                    this.cursor2D.Visible = false;
                }

                this.DEBUGUpdateGraphDrawer();
            }
        }
        private void UpdateCursor(Ray pickingRay)
        {
            if (!this.walkMode)
            {
                if (this.terrain.Geometry.PickNearest(ref pickingRay, true, out PickingResult<Triangle> r))
                {
                    this.cursor3D.Transform.SetPosition(r.Position);
                }
            }
        }
        private void UpdateCamera(GameTime gameTime, Ray pickingRay)
        {
            if (this.walkMode)
            {
                #region Walker

#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.Camera.RotateMouse(
                        this.Game.GameTime,
                        this.Game.Input.MouseXDelta,
                        this.Game.Input.MouseYDelta);
                }

                var prevPos = this.Camera.Position;

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

                if (this.Walk(this.walkerAgentType, prevPos, this.Camera.Position, out Vector3 walkerPos))
                {
                    this.Camera.Goto(walkerPos);
                }
                else
                {
                    this.Camera.Goto(prevPos);
                }

                #endregion
            }
            else
            {
                #region Free Camera

#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.Camera.RotateMouse(
                        this.Game.GameTime,
                        this.Game.Input.MouseXDelta,
                        this.Game.Input.MouseYDelta);
                }

                if (this.Game.Input.KeyJustReleased(Keys.Space))
                {
                    if (this.follow)
                    {
                        this.followTarget = null;
                        this.follow = false;
                    }

                    if (this.PickNearest(ref pickingRay, 0, true, SceneObjectUsageEnum.Agent, out SceneObject agent))
                    {
                        this.followTarget = agent;
                        this.follow = true;
                    }
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

                if (this.follow)
                {
                    var pickable = this.followTarget.Get<IRayPickable<Triangle>>();
                    var transform = this.followTarget.Get<ITransformable3D>();

                    var sph = pickable.GetBoundingSphere();
                    this.Camera.LookTo(sph.Center);
                    this.Camera.Goto(sph.Center + (transform.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
                }

                #endregion
            }
        }
        private void UpdateTanks(Ray pickingRay)
        {
            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (this.PickNearest(ref pickingRay, true, out PickingResult<Triangle> r))
                {
                    var t1Position = this.tankP1.Transform.Position;

                    var result = this.FindPath(this.tankAgentType, t1Position, r.Position, false, 0f);
                    if (result != null)
                    {
                        this.DEBUGDrawTankPath(t1Position, result);
                    }
                }
            }

            if (this.Game.Input.LeftMouseButtonJustReleased)
            {
                if (this.PickNearest(ref pickingRay, true, out PickingResult<Triangle> r))
                {
                    var task = Task.Run(() =>
                    {
                        return this.FindPath(this.tankAgentType, this.tankP1.Transform.Position, r.Position, true, 0.25f);
                    });

                    task.ContinueWith((t) =>
                    {
                        if (t.Result != null)
                        {
                            this.tankP1Agent.Clear();
                            this.tankP1Agent.FollowPath(t.Result, 10);

                            this.DEBUGDrawTankPath(this.tankP1.Transform.Position, t.Result);
                        }
                    });
                }
            }

            this.SetStatsScreenPosition(this.tankP1Agent, 4, this.t1ProgressBar);
            this.SetStatsScreenPosition(this.tankP2Agent, 4, this.t2ProgressBar);
        }
        private void UpdateHelicopter()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                Curve3D curve = this.GenerateHelicopterPath();
                this.helicopter.Instance.AnimationController.SetPath(this.animations["heli_default"]);
                this.DEBUGDrawHelicopterPath(curve);
            }

            this.SetStatsScreenPosition(this.helicopterAgent, 4, this.hProgressBar);
        }
        private void UpdateDebug(Ray pickingRay)
        {
            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.terrainLineDrawer.Visible = !this.terrainLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.terrainGraphDrawer.Visible = !this.terrainGraphDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.terrainPointDrawer.Visible = !this.terrainPointDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.curveLineDrawer.Visible = !this.curveLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.movingObjLineDrawer.Visible = !this.movingObjLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.shadowMapDrawer.Visible = !this.shadowMapDrawer.Visible;
                this.shadowResult = SceneRendererResultEnum.ShadowMapDirectional;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.useDebugTex = !this.useDebugTex;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                this.staticObjLineDrawer.Visible = !this.staticObjLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F11))
            {
                if (this.drawDrawVolumes == false && this.drawCullVolumes == false)
                {
                    this.drawDrawVolumes = true;
                    this.drawCullVolumes = false;
                }
                else if (this.drawDrawVolumes == true && this.drawCullVolumes == false)
                {
                    this.drawDrawVolumes = false;
                    this.drawCullVolumes = true;
                }
                else if (this.drawDrawVolumes == false && this.drawCullVolumes == true)
                {
                    this.drawDrawVolumes = false;
                    this.drawCullVolumes = false;
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.graphIndex++;
                this.DEBUGUpdateGraphDrawer();
            }
            if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                this.graphIndex--;
                this.DEBUGUpdateGraphDrawer();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.helicopter.Instance.TextureIndex++;
                if (this.helicopter.Instance.TextureIndex > 2) this.helicopter.Instance.TextureIndex = 2;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.helicopter.Instance.TextureIndex--;
                if (this.helicopter.Instance.TextureIndex < 0) this.helicopter.Instance.TextureIndex = 0;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up) && !this.Game.Input.ShiftPressed)
            {
                this.shadowResult = SceneRendererResultEnum.ShadowMapDirectional;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Down) && !this.Game.Input.ShiftPressed)
            {
                this.shadowResult = SceneRendererResultEnum.ShadowMapDirectional;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            if (this.Game.Input.KeyJustReleased(Keys.C))
            {
                this.Lights.KeyLight.CastShadow = !this.Lights.KeyLight.CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.D1))
            {
                this.walkMode = !this.walkMode;
                this.DEBUGUpdateGraphDrawer();
                this.walkMode = !this.walkMode;
            }

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (this.terrainGraphDrawer.Visible)
                {
                    this.terrainPointDrawer.Instance.Clear();

                    if (this.PickNearest(ref pickingRay, true, out PickingResult<Triangle> r))
                    {
                        this.DEBUGPickingPosition(r.Position);
                    }
                }
            }

            if (this.drawDrawVolumes) this.DEBUGDrawLightMarks();
            if (this.drawCullVolumes) this.DEBUGDrawLightVolumes();


            if (this.curveLineDrawer.Visible)
            {
                Matrix rot = Matrix.RotationQuaternion(this.helicopter.Transform.Rotation) * Matrix.Translation(this.helicopter.Transform.Position);
                this.curveLineDrawer.Instance.SetLines(this.hAxisColor, Line3D.CreateAxis(rot, 5f));
            }

            if (this.staticObjLineDrawer.Visible && objNotSet)
            {
                DEBUGDrawStaticVolumes();

                objNotSet = false;
            }

            if (this.movingObjLineDrawer.Visible)
            {
                this.DEBUGDrawMovingVolumes();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up) && this.Game.Input.ShiftPressed)
            {
                initDurationIndex++;
                initDurationIndex = initDurationIndex < 0 ? 0 : initDurationIndex;
                initDurationIndex %= initDurationDict.Keys.Count;
                SetLoadText(initDurationIndex);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down) && this.Game.Input.ShiftPressed)
            {
                initDurationIndex--;
                initDurationIndex = initDurationIndex < 0 ? initDurationDict.Keys.Count - 1 : initDurationIndex;
                initDurationIndex %= initDurationDict.Keys.Count;
                SetLoadText(initDurationIndex);
            }

            var tp = this.helicopterAgent.Target;
            if (tp.HasValue)
            {
                this.DEBUGPickingPosition(tp.Value);
            }
        }
        private void SetStatsScreenPosition(AIAgent agent, float height, SceneObject<SpriteProgressBar> pb)
        {
            var screenPosition = this.GetScreenCoordinates(agent.Manipulator.Position, out bool inside);
            var top = this.GetScreenCoordinates(agent.Manipulator.Position + new Vector3(0, height, 0), out inside);

            if (inside)
            {
                pb.Visible = true;

                screenPosition.X = top.X - (pb.Instance.Width * 0.5f);
                screenPosition.Y = top.Y;

                pb.Instance.Top = (int)screenPosition.Y;
                pb.Instance.Left = (int)screenPosition.X;
            }
            else
            {
                pb.Visible = false;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.shadowMapDrawer.Instance.Texture = this.useDebugTex ? this.debugTex : this.Renderer.GetResource(this.shadowResult);

            #region Texts

            this.stats.Instance.Text = this.Game.RuntimeText;

            string txt1 = string.Format(
                "Buffers active: {0} {1} Kbs, reads: {2}, writes: {3}; {4} - Result: {5}; Primitives: {6}",
                Counters.Buffers,
                Counters.BufferBytes / 1024,
                Counters.BufferReads,
                Counters.BufferWrites,
                this.GetRenderMode(),
                this.shadowResult,
                Counters.PrimitivesPerFrame);
            this.counters1.Instance.Text = txt1;

            string txt2 = string.Format(
                "IA Input Layouts: {0}, Primitives: {1}, VB: {2}, IB: {3}, Terrain Patches: {4}; T1.{5}  /  T2.{6}  /  H.{7}",
                Counters.IAInputLayoutSets,
                Counters.IAPrimitiveTopologySets,
                Counters.IAVertexBuffersSets,
                Counters.IAIndexBufferSets,
                this.terrain.Instance.VisiblePatchesCount,
                this.tankP1Agent,
                this.tankP2Agent,
                this.helicopterAgent);
            this.counters2.Instance.Text = txt2;

            this.hProgressBar.Instance.ProgressValue = (1f - this.helicopterAgent.Stats.Damage);
            this.t1ProgressBar.Instance.ProgressValue = (1f - this.tankP1Agent.Stats.Damage);
            this.t2ProgressBar.Instance.ProgressValue = (1f - this.tankP2Agent.Stats.Damage);

            #endregion
        }
        private void SetLoadText(int index)
        {
            var keys = initDurationDict.Keys.ToArray();
            if (index >= 0 && index < keys.Length)
            {
                this.load.Instance.Text = string.Format("{0}: {1}", keys[index], initDurationDict[keys[index]]);
            }
        }

        private Curve3D GenerateHelicopterPath()
        {
            Curve3D curve = new Curve3D
            {
                PreLoop = CurveLoopType.Constant,
                PostLoop = CurveLoopType.Constant
            };

            Vector3[] cPoints = new Vector3[15];

            Random rnd = new Random();

            if (this.helicopterController != null && this.helicopterController.HasPath)
            {
                for (int i = 0; i < cPoints.Length - 2; i++)
                {
                    cPoints[i] = this.GetRandomPoint(rnd, this.helicopterHeightOffset);
                }
            }
            else
            {
                cPoints[0] = this.helicopter.Transform.Position;
                cPoints[1] = this.helicopter.Transform.Position + (Vector3.Up * 5f) + (this.helicopter.Transform.Forward * 10f);

                for (int i = 2; i < cPoints.Length - 2; i++)
                {
                    cPoints[i] = this.GetRandomPoint(rnd, this.helicopterHeightOffset);
                }
            }

            var hPos = this.heliport.Transform.Position;
            if (this.FindTopGroundPosition(hPos.X, hPos.Z, out PickingResult<Triangle> r))
            {
                cPoints[cPoints.Length - 2] = r.Position + this.helicopterHeightOffset;
                cPoints[cPoints.Length - 1] = r.Position;
            }

            float time = 0;
            for (int i = 0; i < cPoints.Length; i++)
            {
                if (i > 0) time += Vector3.Distance(cPoints[i - 1], cPoints[i]);

                curve.AddPosition(time, cPoints[i]);
            }

            curve.SetTangents();

            return curve;
        }

        private void Agent_Moving(object sender, BehaviorEventArgs e)
        {
            if (Helper.RandomGenerator.NextFloat(0, 1) > 0.8f)
            {
                this.AddDustSystem(e.Active, this.tankLeftCat);
                this.AddDustSystem(e.Active, this.tankRightCat);
            }
        }
        private void Agent_Attacking(object sender, BehaviorEventArgs e)
        {
            this.AddProjectileTrailSystem(e.Active, e.Passive, 50f);
        }
        private void Agent_Damaged(object sender, BehaviorEventArgs e)
        {
            this.AddExplosionSystem(e.Passive);
            this.AddExplosionSystem(e.Passive);
            this.AddSmokeSystem(e.Passive, false);
        }
        private void Agent_Destroyed(object sender, BehaviorEventArgs e)
        {
            if (e.Passive == this.helicopterAgent)
            {
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                this.AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
            }
            else
            {
                this.AddExplosionSystem(e.Passive);
                this.AddExplosionSystem(e.Passive);
                this.AddExplosionSystem(e.Passive);
                this.AddExplosionSystem(e.Passive);
                this.AddExplosionSystem(e.Passive);
                this.AddExplosionSystem(e.Passive);
                this.AddSmokePlumeSystem(e.Passive, true);
            }
        }

        private void AddExplosionSystem(AIAgent agent, Vector3 random)
        {
            Vector3 velocity = Vector3.Up;
            float duration = 0.5f;
            float rate = 0.1f;

            var emitter1 = new MovingEmitter(agent.Manipulator, random)
            {
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pExplosion, emitter1);
        }
        private void AddExplosionSystem(AIAgent agent)
        {
            Vector3 velocity = Vector3.Up;
            float duration = 0.5f;
            float rate = 0.1f;

            var emitter1 = new MovingEmitter(agent.Manipulator, Vector3.Zero)
            {
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };
            var emitter2 = new MovingEmitter(agent.Manipulator, Vector3.Zero)
            {
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 2f,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pExplosion, emitter1);
            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pSmokeExplosion, emitter2);
        }
        private void AddSmokePlumeSystem(AIAgent agent, bool unique)
        {
            IParticleSystem plumeFire = null;
            IParticleSystem plumeSmoke = null;
            string plumeFireSystemName = null;
            string plumeSmokeSystemName = null;

            if (!unique)
            {
                plumeFireSystemName = "plumeFire." + agent.SceneObject.Name;
                plumeSmokeSystemName = "plumeSmoke." + agent.SceneObject.Name;

                plumeFire = this.pManager.Instance.GetParticleSystem("plumeFire");
                plumeSmoke = this.pManager.Instance.GetParticleSystem("plumeSmoke");
            }

            float duration = this.rnd.NextFloat(6, 36);
            float rate = this.rnd.NextFloat(0.1f, 1f);

            if (plumeFire == null)
            {
                var emitter1 = new MovingEmitter(agent.Manipulator, Vector3.Zero)
                {
                    Velocity = Vector3.Up,
                    Duration = duration,
                    EmissionRate = rate * 0.5f,
                    InfiniteDuration = false,
                    MaximumDistance = 100f,
                };

                this.pManager.Instance.AddParticleSystem(plumeFireSystemName, ParticleSystemTypes.CPU, this.pFire, emitter1);
            }
            else
            {
                plumeFire.Emitter.Duration = duration;
            }

            if (plumeSmoke == null)
            {
                var emitter2 = new MovingEmitter(agent.Manipulator, Vector3.Zero)
                {
                    Velocity = Vector3.Up,
                    Duration = duration + (duration * 0.1f),
                    EmissionRate = rate,
                    InfiniteDuration = false,
                    MaximumDistance = 500f,
                };

                this.pManager.Instance.AddParticleSystem(plumeSmokeSystemName, ParticleSystemTypes.CPU, this.pPlume, emitter2);
            }
            else
            {
                plumeSmoke.Emitter.Duration = duration + (duration * 0.1f);
            }
        }
        private void AddSmokeSystem(AIAgent agent, bool unique)
        {
            IParticleSystem smoke = null;
            string smokeSystemName = null;

            if (!unique)
            {
                smokeSystemName = "smoke." + agent.SceneObject.Name;
                smoke = this.pManager.Instance.GetParticleSystem(smokeSystemName);
            }

            float duration = this.rnd.NextFloat(5, 15);

            if (smoke == null)
            {
                var emitter = new MovingEmitter(agent.Manipulator, Vector3.Zero)
                {
                    Velocity = Vector3.Up,
                    Duration = duration + (duration * 0.1f),
                    EmissionRate = this.rnd.NextFloat(0.1f, 1f),
                    InfiniteDuration = false,
                    MaximumDistance = 500f,
                };

                this.pManager.Instance.AddParticleSystem(smokeSystemName, ParticleSystemTypes.CPU, this.pPlume, emitter);
            }
            else
            {
                smoke.Emitter.Duration = duration + (duration * 0.1f);
            }
        }
        private void AddDustSystem(AIAgent agent, Vector3 delta)
        {
            var emitter = new MovingEmitter(agent.Manipulator, delta)
            {
                Duration = 5f,
                EmissionRate = 1f,
                MaximumDistance = 250f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pDust, emitter);
        }
        private void AddProjectileTrailSystem(AIAgent agent, AIAgent target, float speed)
        {
            var targetDelta = this.rnd.NextVector3(-Vector3.One, Vector3.One);

            var emitter = new LinealEmitter(agent.Manipulator.Position, target.Manipulator.Position + targetDelta, speed)
            {
                EmissionRate = 0.0001f,
                MaximumDistance = 100f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pProjectile, emitter);
        }

        private void DEBUGPickingPosition(Vector3 position)
        {
            if (this.FindAllGroundPosition(position.X, position.Z, out PickingResult<Triangle>[] results))
            {
                var positions = results.Select(r => r.Position).ToArray();
                var triangles = results.Select(r => r.Item).ToArray();

                this.terrainPointDrawer.Instance.SetLines(Color.Magenta, Line3D.CreateCrossList(positions, 1f));
                this.terrainPointDrawer.Instance.SetLines(Color.DarkCyan, Line3D.CreateWiredTriangle(triangles));
                if (positions.Length > 1)
                {
                    this.terrainPointDrawer.Instance.SetLines(Color.Cyan, new Line3D(positions[0], positions[positions.Length - 1]));
                }
            }
        }
        private void DEBUGDrawHelicopterPath(Curve3D curve)
        {
            List<Vector3> path = new List<Vector3>();

            float pass = curve.Length / 500f;

            for (float i = 0; i <= curve.Length; i += pass)
            {
                Vector3 pos = curve.GetPosition(i);

                path.Add(pos);
            }

            this.curveLineDrawer.Instance.SetLines(this.curvesColor, Line3D.CreatePath(path.ToArray()));
            this.curveLineDrawer.Instance.SetLines(this.pointsColor, Line3D.CreateCrossList(curve.Points, 0.5f));
            this.curveLineDrawer.Instance.SetLines(this.segmentsColor, Line3D.CreatePath(curve.Points));
        }
        private void DEBUGDrawTankPath(Vector3 from, PathFindingPath path)
        {
            int count = Math.Min(path.ReturnPath.Count, MaxPickingTest);

            Line3D[] lines = new Line3D[count + 1];

            for (int i = 0; i < count; i++)
            {
                Line3D line;
                if (i == 0)
                {
                    line = new Line3D(from, path.ReturnPath[i]);
                }
                else
                {
                    line = new Line3D(path.ReturnPath[i - 1], path.ReturnPath[i]);
                }

                lines[i] = line;
            }

            this.terrainPointDrawer.Instance.SetLines(Color.Red, lines);
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var agent = this.walkMode ? this.walkerAgentType : this.tankAgentType;

            var nodes = this.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                if (this.graphIndex <= -1)
                {
                    this.graphIndex = -1;

                    this.terrainGraphDrawer.Instance.Clear();

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = (GraphNode)nodes[i];
                        var color = node.Color;
                        var tris = node.Triangles;

                        this.terrainGraphDrawer.Instance.AddTriangles(color, tris);
                    }
                }
                else
                {
                    if (this.graphIndex >= nodes.Length)
                    {
                        this.graphIndex = nodes.Length - 1;
                    }

                    if (this.graphIndex < nodes.Length)
                    {
                        this.terrainGraphDrawer.Instance.Clear();

                        var node = (GraphNode)nodes[this.graphIndex];
                        var color = node.Color;
                        var tris = node.Triangles;

                        this.terrainGraphDrawer.Instance.SetTriangles(color, tris);
                    }
                }
            }
            else
            {
                this.graphIndex = -1;
            }
        }
        private void DEBUGDrawLightVolumes()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawLightMarks()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 10, 10);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 10, 10);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawStaticVolumes()
        {
            List<Line3D> lines = new List<Line3D>();
            lines.AddRange(Line3D.CreateWiredBox(this.heliport.Geometry.GetBoundingBox()));
            lines.AddRange(Line3D.CreateWiredBox(this.garage.Geometry.GetBoundingBox()));
            for (int i = 0; i < this.obelisk.Count; i++)
            {
                var instance = this.obelisk.GetComponent<IRayPickable<Triangle>>(i);

                lines.AddRange(Line3D.CreateWiredBox(instance.GetBoundingBox()));
            }
            for (int i = 0; i < this.rocks.Count; i++)
            {
                var instance = this.rocks.GetComponent<IRayPickable<Triangle>>(i);

                lines.AddRange(Line3D.CreateWiredBox(instance.GetBoundingBox()));
            }
            for (int i = 0; i < this.tree1.Count; i++)
            {
                var instance = this.tree1.GetComponent<IRayPickable<Triangle>>(i);

                lines.AddRange(Line3D.CreateWiredTriangle(instance.GetVolume(false)));
            }

            for (int i = 0; i < this.tree2.Count; i++)
            {
                var instance = this.tree2.GetComponent<IRayPickable<Triangle>>(i);

                lines.AddRange(Line3D.CreateWiredTriangle(instance.GetVolume(false)));
            }

            this.staticObjLineDrawer.Instance.SetLines(objColor, lines.ToArray());
        }
        private void DEBUGDrawMovingVolumes()
        {
            var hsph = this.helicopter.Geometry.GetBoundingSphere();
            this.movingObjLineDrawer.Instance.SetLines(new Color4(Color.White.ToColor3(), 0.55f), Line3D.CreateWiredSphere(new[] { hsph, }, 50, 20));

            var t1sph = this.tankP1.Geometry.GetBoundingBox();
            var t2sph = this.tankP2.Geometry.GetBoundingBox();
            this.movingObjLineDrawer.Instance.SetLines(new Color4(Color.YellowGreen.ToColor3(), 0.55f), Line3D.CreateWiredBox(new[] { t1sph, t2sph, }));
        }
    }
}
