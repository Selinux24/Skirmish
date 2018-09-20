using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Heightmap
{
    public class TestScene3D : Scene
    {
        private const float near = 0.5f;
        private const float far = 3000f;
        private const float fogStart = 500f;
        private const float fogRange = 500f;

        private const int layerObjects = 0;
        private const int layerTerrain = 1;
        private const int layerFoliage = 2;
        private const int layerEffects = 3;
        private const int layerHUD = 99;
        private const int layerCursor = 100;

        private Random rnd = new Random();

        private float time = 0.23f;

        private Vector3 playerHeight = Vector3.UnitY * 5f;
        private bool playerFlying = true;
        private SceneLightSpot lantern = null;

        private Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private float windNextStrength = 1f;
        private readonly float windStep = 0.001f;
        private float windDuration = 0;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> load = null;
        private SceneObject<TextDrawer> stats = null;
        private SceneObject<TextDrawer> help = null;
        private SceneObject<TextDrawer> help2 = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<Cursor> cursor;
        private SceneObject<LensFlare> lensFlare = null;
        private SceneObject<SkyScattering> skydom = null;
        private SceneObject<SkyPlane> clouds = null;
        private SceneObject<Terrain> terrain = null;
        private SceneObject<GroundGardener> gardener = null;
        private SceneObject<GroundGardener> gardener2 = null;
        private SceneObject<LineListDrawer> bboxesDrawer = null;
        private SceneObject<LineListDrawer> linesDrawer = null;

        private SceneObject<ModelInstanced> torchs = null;
        private SceneLightPoint[] torchLights = null;
        private SceneLightSpot spotLight1 = null;
        private SceneLightSpot spotLight2 = null;

        private SceneObject<ParticleManager> pManager = null;
        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private float nextDust = 0;
        private readonly float dustTime = 0.33f;

        private SceneObject<ModelInstanced> rocks = null;
        private SceneObject<ModelInstanced> trees = null;
        private SceneObject<ModelInstanced> trees2 = null;

        private SceneObject<Model> soldier = null;
        private SceneObject<TriangleListDrawer> soldierTris = null;
        private SceneObject<LineListDrawer> soldierLines = null;
        private bool showSoldierDEBUG = false;

        private SceneObject<ModelInstanced> troops = null;

        private SceneObject<Model> helicopter = null;
        private SceneObject<Model> helicopter2 = null;
        private SceneObject<Model> bradley = null;
        private SceneObject<Model> watchTower = null;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private readonly Dictionary<string, double> initDurationDict = new Dictionary<string, double>();
        private int initDurationIndex = 0;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var taskUI = InitializeUI();
            var taskRocks = InitializeRocks();
            var taskTrees = InitializeTrees();
            var taskTrees2 = InitializeTrees2();
            var taskSoldier = InitializeSoldier();
            var taskTroops = InitializeTroops();
            var taskM24 = InitializeM24();
            var taskHelicopter = InitializeHelicopter();
            var taskBradley = InitializeBradley();
            var taskWatchTower = InitializeWatchTower();
            var taskTorchs = InitializeTorchs();
            var taskTerrain = InitializeTerrain();
            var taskGardener = InitializeGardener();
            var taskGardener2 = InitializeGardener2();
            var taskLensFlare = InitializeLensFlare();
            var taskSkydom = InitializeSkydom();
            var taskClouds = InitializeClouds();
            var taskParticles = InitializeParticles();

            initDurationDict.Add("UI", taskUI.Result);
            initDurationDict.Add("Rocks", taskRocks.Result);
            initDurationDict.Add("Trees", taskTrees.Result);
            initDurationDict.Add("Trees2", taskTrees2.Result);
            initDurationDict.Add("Soldier", taskSoldier.Result);
            initDurationDict.Add("Troops", taskTroops.Result);
            initDurationDict.Add("M24", taskM24.Result);
            initDurationDict.Add("Helicopter", taskHelicopter.Result);
            initDurationDict.Add("Bradley", taskBradley.Result);
            initDurationDict.Add("Watch Tower", taskWatchTower.Result);
            initDurationDict.Add("Torchs", taskTorchs.Result);
            initDurationDict.Add("Terrain", taskTerrain.Result);
            initDurationDict.Add("Gardener", taskGardener.Result);
            initDurationDict.Add("Gardener2", taskGardener2.Result);
            initDurationDict.Add("LensFlare", taskLensFlare.Result);
            initDurationDict.Add("Skydom", taskSkydom.Result);
            initDurationDict.Add("Clouds", taskClouds.Result);
            initDurationDict.Add("Particles", taskParticles.Result);

            sw.Stop();

            initDurationDict.Add("TOTAL", initDurationDict.Select(i => i.Value).Sum());
            initDurationDict.Add("REAL", sw.Elapsed.TotalSeconds);

            initDurationIndex = initDurationDict.Keys.Count - 2;

            SetLoadText(initDurationIndex);
        }
        private Task<double> InitializeUI()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            #region Cursor

            var cursorDesc = new CursorDescription()
            {
                Name = "Cursor",
                Textures = new[] { "target.png" },
                Color = Color.Red,
                Width = 20,
                Height = 20,
            };

            this.cursor = this.AddComponent<Cursor>(cursorDesc, SceneObjectUsageEnum.UI, layerCursor);

            #endregion

            #region Texts

            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.load = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.stats = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.help = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.help2 = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Orange), SceneObjectUsageEnum.UI, layerHUD);

            this.title.Instance.Text = "Heightmap Terrain test";
            this.load.Instance.Text = "";
            this.stats.Instance.Text = "";
            this.help.Instance.Text = "";
            this.help2.Instance.Text = "";

            this.title.Instance.Position = Vector2.Zero;
            this.load.Instance.Position = new Vector2(5, this.title.Instance.Top + this.title.Instance.Height + 3);
            this.stats.Instance.Position = new Vector2(5, this.load.Instance.Top + this.load.Instance.Height + 3);
            this.help.Instance.Position = new Vector2(5, this.stats.Instance.Top + this.stats.Instance.Height + 3);
            this.help2.Instance.Position = new Vector2(5, this.help.Instance.Top + this.help.Instance.Height + 3);

            var spDesc = new SpriteDescription()
            {
                Name = "Background",
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.help2.Instance.Top + this.help2.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);

            #endregion

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
                    ContentFolder = @"Resources/Rocks",
                    ModelContentFilename = @"boulder.xml",
                }
            };
            this.rocks = this.AddComponent<ModelInstanced>(rDesc, SceneObjectUsageEnum.None, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTrees()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();
            var treeDesc = new ModelInstancedDescription()
            {
                Name = "Trees",
                CastShadow = true,
                Static = true,
                Instances = 200,
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees",
                    ModelContentFilename = @"tree.xml",
                }
            };
            this.trees = this.AddComponent<ModelInstanced>(treeDesc, SceneObjectUsageEnum.None, layerTerrain);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTrees2()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();
            var tree2Desc = new ModelInstancedDescription()
            {
                Name = "Trees",
                CastShadow = true,
                Static = true,
                Instances = 200,
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees2",
                    ModelContentFilename = @"tree.xml",
                }
            };
            this.trees2 = this.AddComponent<ModelInstanced>(tree2Desc, SceneObjectUsageEnum.None, layerTerrain);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeSoldier()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var sDesc = new ModelDescription()
            {
                Name = "Soldier",
                TextureIndex = 0,
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Soldier",
                    ModelContentFilename = @"soldier_anim2.xml",
                }
            };
            this.soldier = this.AddComponent<Model>(sDesc, SceneObjectUsageEnum.Agent, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTroops()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var tDesc = new ModelInstancedDescription()
            {
                Name = "Troops",
                Instances = 4,
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Soldier",
                    ModelContentFilename = @"soldier_anim2.xml",
                }
            };
            this.troops = this.AddComponent<ModelInstanced>(tDesc, SceneObjectUsageEnum.Agent, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeM24()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var mDesc = new ModelDescription()
            {
                Name = "M24",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/m24",
                    ModelContentFilename = @"m24.xml",
                }
            };
            this.helicopter = this.AddComponent<Model>(mDesc, SceneObjectUsageEnum.None, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeHelicopter()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var hcDesc = new ModelDescription()
            {
                Name = "Helicopter",
                CastShadow = true,
                Static = false,
                TextureIndex = 2,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Helicopter",
                    ModelContentFilename = @"Helicopter.xml",
                }
            };
            this.helicopter2 = this.AddComponent<Model>(hcDesc, SceneObjectUsageEnum.None, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeBradley()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var mDesc = new ModelDescription()
            {
                Name = "Bradley",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Bradley",
                    ModelContentFilename = @"Bradley.xml",
                }
            };
            this.bradley = this.AddComponent<Model>(mDesc, SceneObjectUsageEnum.None, layerObjects);
            this.Lights.AddRange(this.bradley.Instance.Lights);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeWatchTower()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var mDesc = new ModelDescription()
            {
                Name = "Watch Tower",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Watch Tower",
                    ModelContentFilename = @"Watch Tower.xml",
                }
            };
            this.watchTower = this.AddComponent<Model>(mDesc, SceneObjectUsageEnum.None, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTorchs()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var tcDesc = new ModelInstancedDescription()
            {
                Name = "Torchs",
                Instances = 50,
                CastShadow = false,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Scenery/Objects",
                    ModelContentFilename = @"torch.xml",
                }
            };
            this.torchs = this.AddComponent<ModelInstanced>(tcDesc, SceneObjectUsageEnum.None, layerObjects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeParticles()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            this.pManager = this.AddComponent<ParticleManager>(new ParticleManagerDescription() { Name = "Particle Systems" }, SceneObjectUsageEnum.None, layerEffects);

            this.pFire = ParticleSystemDescription.InitializeFire("resources/particles", "fire.png", 0.5f);
            this.pPlume = ParticleSystemDescription.InitializeSmokePlume("resources/particles", "smoke.png", 0.5f);
            this.pDust = ParticleSystemDescription.InitializeDust("resources/particles", "dust.png", 2f);
            this.pDust.MinHorizontalVelocity = 10f;
            this.pDust.MaxHorizontalVelocity = 15f;
            this.pDust.MinVerticalVelocity = 0f;
            this.pDust.MaxVerticalVelocity = 0f;
            this.pDust.MinColor = new Color(Color.SandyBrown.ToColor3(), 0.05f);
            this.pDust.MaxColor = new Color(Color.SandyBrown.ToColor3(), 0.10f);
            this.pDust.MinEndSize = 2f;
            this.pDust.MaxEndSize = 20f;
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeTerrain()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var hDesc = new HeightmapDescription()
            {
                ContentPath = "Resources/Scenery/Heightmap",
                HeightmapFileName = "desert0hm.bmp",
                ColormapFileName = "desert0cm.bmp",
                CellSize = 15,
                MaximumHeight = 150,
                TextureResolution = 100f,
                Textures = new HeightmapDescription.TexturesDescription()
                {
                    ContentPath = "Textures",
                    NormalMaps = new[] { "normal001.dds", "normal002.dds" },
                    SpecularMaps = new[] { "specular001.dds", "specular002.dds" },

                    UseAlphaMapping = true,
                    AlphaMap = "alpha001.dds",
                    ColorTextures = new[] { "dirt001.dds", "dirt002.dds", "dirt004.dds", "stone001.dds" },

                    UseSlopes = false,
                    SlopeRanges = new Vector2(0.005f, 0.25f),
                    TexturesLR = new[] { "dirt0lr.dds", "dirt1lr.dds", "dirt2lr.dds" },
                    TexturesHR = new[] { "dirt0hr.dds" },

                    Proportion = 0.25f,
                },
                Material = new MaterialDescription
                {
                    Shininess = 10f,
                    SpecularColor = new Color4(0.1f, 0.1f, 0.1f, 1f),
                },
            };
            var gDesc = new GroundDescription()
            {
                Name = "Terrain",
                UseAnisotropic = true,
                Quadtree = new GroundDescription.QuadtreeDescription()
                {
                    MaximumDepth = 5,
                },
                Content = new ContentDescription()
                {
                    HeightmapDescription = hDesc,
                }
            };
            this.terrain = this.AddComponent<Terrain>(gDesc, SceneObjectUsageEnum.None, layerTerrain);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeGardener()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var vDesc = new GroundGardenerDescription()
            {
                Name = "Grass",
                ContentPath = "Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map.png",
                Material = new MaterialDescription()
                {
                    DiffuseColor = Color.Gray,
                },
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass_v.dds" },
                    Saturation = 0.05f,
                    StartRadius = 0f,
                    EndRadius = 100f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1.5f, 1.5f),
                    Seed = 1,
                    WindEffect = 0.3f,
                    Count = 4,
                },
                ChannelGreen = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass_d.dds" },
                    VegetationNormalMaps = new[] { "grass_n.dds" },
                    Saturation = 10f,
                    StartRadius = 0f,
                    EndRadius = 100f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1f, 1f),
                    Seed = 2,
                    WindEffect = 0.2f,
                    Count = 4,
                },
                ChannelBlue = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass1.png" },
                    Saturation = 0.005f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1f, 1f),
                    Delta = new Vector3(0, -0.05f, 0),
                    Seed = 3,
                    WindEffect = 0.3f,
                    Count = 1,
                },
            };
            this.gardener = this.AddComponent<GroundGardener>(vDesc, SceneObjectUsageEnum.None, layerFoliage);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeGardener2()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var vDesc2 = new GroundGardenerDescription()
            {
                Name = "Flowers",
                ContentPath = "Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map_flowers.png",
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "flower0.dds" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f) * 0.5f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 1,
                    WindEffect = 0.5f,
                },
                ChannelGreen = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "flower1.dds" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f) * 0.5f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 2,
                    WindEffect = 0.5f,
                },
                ChannelBlue = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "flower2.dds" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 140f,
                    MinSize = new Vector2(1f, 1f) * 0.5f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 3,
                    WindEffect = 0.5f,
                },
            };
            this.gardener2 = this.AddComponent<GroundGardener>(vDesc2, SceneObjectUsageEnum.None, layerFoliage);
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
                ContentPath = @"Resources/Scenery/Flare",
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
            this.lensFlare = this.AddComponent<LensFlare>(lfDesc, SceneObjectUsageEnum.None, layerEffects);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeSkydom()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var skDesc = new SkyScatteringDescription()
            {
                Name = "Sky",
            };
            this.skydom = this.AddComponent<SkyScattering>(skDesc);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private Task<double> InitializeClouds()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var scDesc = new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "Resources/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                Mode = SkyPlaneMode.Perturbed,
                Direction = new Vector2(1, 1),
            };
            this.clouds = this.AddComponent<SkyPlane>(scDesc);
            sw.Stop();

            return Task.FromResult(sw.Elapsed.TotalSeconds);
        }

        public override void Initialized()
        {
            base.Initialized();

            var taskPathfinding = Task.Run(() => SetPathFindingInfo());
            var taskAnimations = Task.Run(() => SetAnimationDictionaries());
            var taskPositioning = Task.Run(() => SetPositionOverTerrain());
            var taskDebugInfo = Task.Run(() => SetDebugInfo());

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;
            this.Camera.Position = new Vector3(24, 12, 14);
            this.Camera.Interest = new Vector3(0, 10, 0);
            this.Camera.MovementDelta = 45f;
            this.Camera.SlowMovementDelta = 20f;

            this.skydom.Instance.RayleighScattering *= 0.8f;
            this.skydom.Instance.MieScattering *= 0.1f;

            this.TimeOfDay.BeginAnimation(new TimeSpan(4, 55, 00), 1f);

            this.Lights.BaseFogColor = new Color((byte)95, (byte)147, (byte)233) * 0.5f;
            this.ToggleFog();

            this.lantern = new SceneLightSpot("lantern", true, Color.White, Color.White, true, this.Camera.Position, this.Camera.Direction, 25f, 100, 1000);
            this.Lights.Add(this.lantern);

            Task.WaitAll(new[]
            {
                taskPathfinding,
                taskAnimations,
                taskPositioning,
                taskDebugInfo,
            });
        }
        private void SetAnimationDictionaries()
        {
            var hp = new AnimationPath();
            hp.AddLoop("roll");
            this.animations.Add("heli_default", new AnimationPlan(hp));

            var sp = new AnimationPath();
            sp.AddLoop("stand");
            this.animations.Add("soldier_stand", new AnimationPlan(sp));

            var sp1 = new AnimationPath();
            sp1.AddLoop("idle1");
            this.animations.Add("soldier_idle", new AnimationPlan(sp1));
        }
        private void SetPositionOverTerrain()
        {
            Random posRnd = new Random(1024);

            BoundingBox bbox = this.terrain.Instance.GetBoundingBox();

            this.SetGround(this.terrain, true);

            {
                #region Rocks

                for (int i = 0; i < this.rocks.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var scale = 1f;
                        if (i < 5)
                        {
                            scale = posRnd.NextFloat(10f, 30f);
                        }
                        else if (i < 30)
                        {
                            scale = posRnd.NextFloat(2f, 5f);
                        }
                        else
                        {
                            scale = posRnd.NextFloat(0.1f, 1f);
                        }

                        this.rocks.Instance[i].Manipulator.SetPosition(r.Position, true);
                        this.rocks.Instance[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), true);
                        this.rocks.Instance[i].Manipulator.SetScale(scale, true);
                    }
                }

                #endregion

                this.AttachToGround(this.rocks, false);
            }

            {
                #region Forest

                bbox = new BoundingBox(new Vector3(-400, 0, -400), new Vector3(-1000, 1000, -1000));

                for (int i = 0; i < this.trees.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var treePosition = r.Position;
                        treePosition.Y -= posRnd.NextFloat(1f, 5f);

                        this.trees.Instance[i].Manipulator.SetPosition(treePosition, true);
                        this.trees.Instance[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.5f, MathUtil.PiOverFour * 0.5f), 0, true);
                        this.trees.Instance[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                    }
                }

                bbox = new BoundingBox(new Vector3(-300, 0, -300), new Vector3(-1000, 1000, -1000));

                for (int i = 0; i < this.trees2.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var treePosition = r.Position;
                        treePosition.Y -= posRnd.NextFloat(0f, 2f);

                        this.trees2.Instance[i].Manipulator.SetPosition(treePosition, true);
                        this.trees2.Instance[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.15f, MathUtil.PiOverFour * 0.15f), 0, true);
                        this.trees2.Instance[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                    }
                }

                #endregion

                this.AttachToGround(this.trees, false);
                this.AttachToGround(this.trees2, false);
            }

            {
                #region Watch tower

                if (this.FindTopGroundPosition(-40, -40, out PickingResult<Triangle> r))
                {
                    this.watchTower.Transform.SetPosition(r.Position, true);
                    this.watchTower.Transform.SetRotation(MathUtil.Pi / 3f, 0, 0, true);
                    this.watchTower.Transform.SetScale(1.5f, true);
                }

                #endregion

                this.AttachToGround(this.watchTower, false);
            }

            {
                #region Torchs

                if (this.FindTopGroundPosition(5, 5, out PickingResult<Triangle> r))
                {
                    var position = r.Position;

                    this.torchs.Instance[0].Manipulator.SetScale(1f, 1f, 1f, true);
                    this.torchs.Instance[0].Manipulator.SetPosition(position, true);
                    var tbbox = this.torchs.Instance[0].GetBoundingBox();

                    position.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y);

                    this.spotLight1 = new SceneLightSpot(
                        "Red Spot",
                        true,
                        Color.Red,
                        Color.Red,
                        true,
                        position,
                        Vector3.Normalize(Vector3.One * -1f),
                        25,
                        25,
                        100);

                    this.spotLight2 = new SceneLightSpot(
                        "Blue Spot",
                        true,
                        Color.Blue,
                        Color.Blue,
                        true,
                        position,
                        Vector3.Normalize(Vector3.One * -1f),
                        25,
                        25,
                        100);

                    this.Lights.Add(this.spotLight1);
                    this.Lights.Add(this.spotLight2);
                };

                this.torchLights = new SceneLightPoint[this.torchs.Count - 1];
                for (int i = 1; i < this.torchs.Count; i++)
                {
                    Color color = new Color(
                        rnd.NextFloat(0, 1),
                        rnd.NextFloat(0, 1),
                        rnd.NextFloat(0, 1),
                        1);

                    Vector3 position = new Vector3(
                        rnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                        0f,
                        rnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                    this.FindTopGroundPosition(position.X, position.Z, out PickingResult<Triangle> res);

                    var pos = res.Position;
                    this.torchs.Instance[i].Manipulator.SetScale(0.20f, true);
                    this.torchs.Instance[i].Manipulator.SetPosition(pos, true);
                    BoundingBox tbbox = this.torchs.Instance[i].GetBoundingBox();

                    pos.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                    this.torchLights[i - 1] = new SceneLightPoint(
                        string.Format("Torch {0}", i),
                        true,
                        color,
                        color,
                        true,
                        pos,
                        4f,
                        5f);

                    this.Lights.Add(this.torchLights[i - 1]);

                    this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pFire, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.1f });
                    this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pPlume, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.5f });
                }

                #endregion

                this.AttachToGround(this.torchs, false);
            }

            //M24
            {
                if (this.FindTopGroundPosition(100, 50, out PickingResult<Triangle> r))
                {
                    this.helicopter.Transform.SetPosition(r.Position, true);
                    this.helicopter.Transform.SetRotation(MathUtil.Pi / 5f, 0, 0, true);
                }

                this.AttachToGround(this.helicopter, false);
            }

            //Helicopter
            {
                if (this.FindTopGroundPosition(-100, -10, out PickingResult<Triangle> r))
                {
                    this.helicopter2.Transform.SetPosition(r.Position, true);
                    this.helicopter2.Transform.SetRotation(MathUtil.Pi / 3f, 0, 0, true);
                    this.helicopter2.Transform.SetScale(5, true);
                }

                this.helicopter2.Instance.AnimationController.TimeDelta = 2f;
                this.helicopter2.Instance.AnimationController.AddPath(this.animations["heli_default"]);
                this.helicopter2.Instance.AnimationController.Start();

                this.AttachToGround(this.helicopter2, false);
            }

            //Bradley
            {
                if (this.FindTopGroundPosition(20, 200, out PickingResult<Triangle> r))
                {
                    this.bradley.Transform.SetScale(1.2f, true);
                    this.bradley.Transform.SetPosition(r.Position, true);
                    this.bradley.Transform.SetRotation(0, 0, 0, true);
                    this.bradley.Transform.SetNormal(r.Item.Normal);
                }

                this.AttachToGround(this.bradley, false);
            }

            //Player soldier
            {
                if (this.FindTopGroundPosition(0, 0, out PickingResult<Triangle> r))
                {
                    this.soldier.Transform.SetPosition(r.Position, true);
                }

                this.soldier.Instance.AnimationController.AddPath(this.animations["soldier_stand"]);
                this.soldier.Instance.AnimationController.Start();
            }

            //Instanced soldiers
            {
                Vector3[] iPos = new Vector3[]
                {
                    new Vector3(4, -2, MathUtil.PiOverFour),
                    new Vector3(5, -5, MathUtil.PiOverTwo),
                    new Vector3(-4, -2, -MathUtil.PiOverFour),
                    new Vector3(-5, -5, -MathUtil.PiOverTwo),
                };

                for (int i = 0; i < 4; i++)
                {
                    if (this.FindTopGroundPosition(iPos[i].X, iPos[i].Y, out PickingResult<Triangle> r))
                    {
                        this.troops.Instance[i].Manipulator.SetPosition(r.Position, true);
                        this.troops.Instance[i].Manipulator.SetRotation(iPos[i].Z, 0, 0, true);
                        this.troops.Instance[i].TextureIndex = 1;

                        this.troops.Instance[i].AnimationController.TimeDelta = (i + 1) * 0.2f;
                        this.troops.Instance[i].AnimationController.AddPath(this.animations["soldier_idle"]);
                        this.troops.Instance[i].AnimationController.Start(rnd.NextFloat(0f, 8f));
                    }
                }
            }
        }
        private void SetDebugInfo()
        {
            {
                var bboxes = this.terrain.Instance.GetBoundingBoxes(5);
                var listBoxes = Line3D.CreateWiredBox(bboxes);

                var desc = new LineListDrawerDescription()
                {
                    Name = "DEBUG++ Terrain nodes bounding boxes",
                    AlphaEnabled = true,
                    DepthEnabled = true,
                    Dynamic = true,
                    Color = new Color4(1.0f, 0.0f, 0.0f, 0.55f),
                    Lines = listBoxes,
                };
                this.bboxesDrawer = this.AddComponent<LineListDrawer>(desc);
                this.bboxesDrawer.Visible = false;
            }

            {
                var desc = new LineListDrawerDescription()
                {
                    DepthEnabled = true,
                    Count = 1000,
                };
                this.linesDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsageEnum.None, layerEffects);
                this.linesDrawer.Visible = false;
            }
        }
        private void SetPathFindingInfo()
        {
            //Player height
            var sbbox = this.soldier.Instance.GetBoundingBox();
            this.playerHeight.Y = sbbox.Maximum.Y - sbbox.Minimum.Y;
            var agent = new Agent()
            {
                Name = "Soldier",
                Height = this.playerHeight.Y,
                Radius = this.playerHeight.Y * 0.33f,
                MaxClimb = this.playerHeight.Y * 0.5f,
                MaxSlope = 45,
            };

            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = 0.2f;
            nmsettings.CellHeight = 0.15f;

            //Agents
            nmsettings.Agents = new[] { agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypeEnum.Watershed;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModesEnum.TempObstacles;
            nmsettings.TileSize = 16;

            nmsettings.NavmeshBounds = new BoundingBox(sbbox.Minimum * 100f, sbbox.Maximum * 100f);

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new PathFinderDescription(nmsettings, nminput);
        }
        private void ToggleFog()
        {
            this.Lights.FogStart = this.Lights.FogStart == 0f ? fogStart : 0f;
            this.Lights.FogRange = this.Lights.FogRange == 0f ? fogRange : 0f;
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

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            //Input driven
            UpdateCamera(gameTime, shift);
            UpdatePlayer();
            UpdateDebugInfo(gameTime);

            //Auto
            UpdateLights(gameTime);
            UpdateWind(gameTime);
            UpdateDust(gameTime);

            this.help.Instance.Text = string.Format(
                "{0}. Wind {1} {2:0.000} - Next {3:0.000}; {4} Light brightness: {5:0.00};",
                this.Renderer,
                this.windDirection, this.windStrength, this.windNextStrength,
                this.TimeOfDay,
                this.Lights.KeyLight.Brightness);

            this.help2.Instance.Text = string.Format("Picks: {0:0000}|{1:00.000}|{2:00.0000000}; Frustum tests: {3:000}|{4:00.000}|{5:00.00000000}; PlantingTaks: {6:000}",
                Counters.PicksPerFrame, Counters.PickingTotalTimePerFrame, Counters.PickingAverageTime,
                Counters.VolumeFrustumTestPerFrame, Counters.VolumeFrustumTestTotalTimePerFrame, Counters.VolumeFrustumTestAverageTime,
                this.gardener.Instance.PlantingTasks + this.gardener2.Instance.PlantingTasks);
        }
        private void UpdatePlayer()
        {
            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.playerFlying = !this.playerFlying;

                if (this.playerFlying)
                {
                    this.Camera.Following = null;
                }
                else
                {
                    var offset = (this.playerHeight * 1.2f) + (Vector3.ForwardLH * 10f) + (Vector3.Left * 3f);
                    var view = (Vector3.BackwardLH * 4f) + Vector3.Down;
                    this.Camera.Following = new CameraFollower(this.soldier.Transform, offset, view);
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.lantern.Enabled = !this.lantern.Enabled;
            }
        }
        private void UpdateCamera(GameTime gameTime, bool shift)
        {
            if (this.playerFlying)
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
                    this.Camera.MoveLeft(gameTime, !shift);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.Camera.MoveRight(gameTime, !shift);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.Camera.MoveForward(gameTime, !shift);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.Camera.MoveBackward(gameTime, !shift);
                }
            }
            else
            {
#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.soldier.Transform.Rotate(
                        this.Game.Input.MouseXDelta * 0.001f,
                        0, 0);
                }

                float delta = shift ? 8 : 4;

                if (this.Game.Input.KeyPressed(Keys.A))
                {
                    this.soldier.Transform.MoveLeft(gameTime, delta);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.soldier.Transform.MoveRight(gameTime, delta);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.soldier.Transform.MoveForward(gameTime, delta);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.soldier.Transform.MoveBackward(gameTime, delta);
                }

                if (this.FindTopGroundPosition(this.soldier.Transform.Position.X, this.soldier.Transform.Position.Z, out PickingResult<Triangle> r))
                {
                    this.soldier.Transform.SetPosition(r.Position);
                };
            }
        }
        private void UpdateDebugInfo(GameTime gameTime)
        {
            #region Wind

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                this.windStrength += this.windStep;
                if (this.windStrength > 100f) this.windStrength = 100f;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                this.windStrength -= this.windStep;
                if (this.windStrength < 0f) this.windStrength = 0f;
            }

            #endregion

            #region Drawers

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.showSoldierDEBUG = !this.showSoldierDEBUG;

                if (this.soldierTris != null) this.soldierTris.Visible = this.showSoldierDEBUG;
                if (this.soldierLines != null) this.soldierLines.Visible = this.showSoldierDEBUG;
            }

            if (this.showSoldierDEBUG)
            {
                Color color = new Color(Color.Red.ToColor3(), 0.6f);

                var tris = this.soldier.Instance.GetTriangles(true);
                if (this.soldierTris == null)
                {
                    var desc = new TriangleListDrawerDescription()
                    {
                        DepthEnabled = false,
                        Triangles = tris,
                        Color = color
                    };
                    this.soldierTris = this.AddComponent<TriangleListDrawer>(desc);
                }
                else
                {
                    this.soldierTris.Instance.SetTriangles(color, tris);
                }

                BoundingBox[] bboxes = new BoundingBox[]
                {
                    this.soldier.Instance.GetBoundingBox(true),
                    this.troops.Instance[0].GetBoundingBox(true),
                    this.troops.Instance[1].GetBoundingBox(true),
                    this.troops.Instance[2].GetBoundingBox(true),
                    this.troops.Instance[3].GetBoundingBox(true),
                };
                if (this.soldierLines == null)
                {
                    var desc = new LineListDrawerDescription()
                    {
                        Lines = Line3D.CreateWiredBox(bboxes),
                        Color = color
                    };
                    this.soldierLines = this.AddComponent<LineListDrawer>(desc);
                }
                else
                {
                    this.soldierLines.Instance.SetLines(color, Line3D.CreateWiredBox(bboxes));
                }
            }

            #endregion

            #region Lights

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.KeyLight.CastShadow = !this.Lights.KeyLight.CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.linesDrawer.Instance.SetLines(Color.LightPink, this.lantern.GetVolume(10));
                this.linesDrawer.Visible = true;
            }

            #endregion

            #region Fog

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.ToggleFog();
            }

            #endregion

            #region Time of day

            if (this.Game.Input.KeyPressed(Keys.Left))
            {
                this.time -= gameTime.ElapsedSeconds * 0.1f;
                this.TimeOfDay.SetTimeOfDay(this.time % 1f, false);
            }

            if (this.Game.Input.KeyPressed(Keys.Right))
            {
                this.time += gameTime.ElapsedSeconds * 0.1f;
                this.TimeOfDay.SetTimeOfDay(this.time % 1f, false);
            }

            #endregion

            #region Load duration index

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                initDurationIndex++;
                initDurationIndex = initDurationIndex < 0 ? 0 : initDurationIndex;
                initDurationIndex %= initDurationDict.Keys.Count;
                SetLoadText(initDurationIndex);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                initDurationIndex--;
                initDurationIndex = initDurationIndex < 0 ? initDurationDict.Keys.Count - 1 : initDurationIndex;
                initDurationIndex %= initDurationDict.Keys.Count;
                SetLoadText(initDurationIndex);
            }

            #endregion
        }
        private void UpdateWind(GameTime gameTime)
        {
            this.windDuration += gameTime.ElapsedSeconds;
            if (this.windDuration > 10)
            {
                this.windDuration = 0;

                this.windNextStrength = this.windStrength + this.rnd.NextFloat(-0.5f, +0.5f);
                if (this.windNextStrength > 100f) this.windNextStrength = 100f;
                if (this.windNextStrength < 0f) this.windNextStrength = 0f;
            }

            if (this.windNextStrength < this.windStrength)
            {
                this.windStrength -= this.windStep;
                if (this.windNextStrength > this.windStrength) this.windStrength = this.windNextStrength;
            }
            if (this.windNextStrength > this.windStrength)
            {
                this.windStrength += this.windStep;
                if (this.windNextStrength < this.windStrength) this.windStrength = this.windNextStrength;
            }

            this.gardener.Instance.SetWind(this.windDirection, this.windStrength);
            this.gardener2.Instance.SetWind(this.windDirection, this.windStrength);
        }
        private void UpdateDust(GameTime gameTime)
        {
            this.nextDust -= gameTime.ElapsedSeconds;

            if (this.nextDust <= 0)
            {
                this.nextDust = this.dustTime;

                var hbsph = this.helicopter2.Instance.GetBoundingSphere();
                hbsph.Radius *= 0.8f;

                this.GenerateDust(this.rnd, hbsph);
                this.GenerateDust(this.rnd, hbsph);
                this.GenerateDust(this.rnd, hbsph);
            }
        }
        private void GenerateDust(Random rnd, BoundingSphere bsph)
        {
            var pos = GetRandomPoint(rnd, Vector3.Zero, bsph);

            var velocity = Vector3.Normalize(bsph.Center + pos);

            var emitter = new ParticleEmitter()
            {
                EmissionRate = 0.01f,
                Duration = 1,
                Position = pos,
                Velocity = velocity,
            };

            this.pDust.Gravity = (this.windStrength * this.windDirection);

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pDust, emitter);
        }
        private void UpdateLights(GameTime gameTime)
        {
            {
                float d = 1f;
                float v = 5f;

                var x = d * (float)Math.Cos(v * this.Game.GameTime.TotalSeconds);
                var z = d * (float)Math.Sin(v * this.Game.GameTime.TotalSeconds);

                this.spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
                this.spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));
            }

            if (this.lantern.Enabled)
            {
                this.lantern.Position = this.Camera.Position + this.Camera.Left;
                this.lantern.Direction = this.Camera.Direction;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.stats.Instance.Text = this.Game.RuntimeText;
        }
        private void SetLoadText(int index)
        {
            var keys = initDurationDict.Keys.ToArray();
            if (index >= 0 && index < keys.Length)
            {
                this.load.Instance.Text = string.Format("{0}: {1}", keys[index], initDurationDict[keys[index]]);
            }
        }
    }
}
