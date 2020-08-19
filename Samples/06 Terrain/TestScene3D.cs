using Engine;
using Engine.Animation;
using Engine.Audio;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
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
        private const int MaxPickingTest = 10000;
        private const int MaxGridDrawer = 10000;

        private readonly int layerHud = 99;
        private readonly int layerGameHud = 50;
        private readonly int layerObjects = 0;
        private readonly int layerTerrain = 1;
        private readonly int layerEffects = 2;

        private bool walkMode = false;
        private readonly float walkerVelocity = 8f;
        private ISceneObject followTarget;
        private bool follow = false;
        private Agent walkerAgentType = null;

        private bool useDebugTex = false;
        private SceneRendererResults shadowResult = SceneRendererResults.ShadowMapDirectional;
        private UITextureRenderer shadowMapDrawer = null;
        private EngineShaderResourceView debugTex = null;
        private int graphIndex = -1;

        private UITextArea stats = null;
        private UITextArea counters1 = null;
        private UITextArea counters2 = null;

        private UIProgressBar hProgressBar = null;
        private UIProgressBar t1ProgressBar = null;
        private UIProgressBar t2ProgressBar = null;

        private Model cursor3D = null;
        private UICursor cursor2D = null;

        private Model tankP1 = null;
        private Model tankP2 = null;
        private Agent tankAgentType = null;
        private Vector3 tankLeftCat = Vector3.Zero;
        private Vector3 tankRightCat = Vector3.Zero;

        private Scenery terrain = null;
        private GroundGardener gardener = null;
        private readonly Vector3 windDirection = Vector3.UnitX;
        private readonly float windStrength = 1f;
        private readonly List<Line3D> oks = new List<Line3D>();
        private readonly List<Line3D> errs = new List<Line3D>();

        private Model heliport = null;
        private Model garage = null;
        private Model building = null;
        private ModelInstanced obelisk = null;
        private ModelInstanced rocks = null;
        private ModelInstanced tree1 = null;
        private ModelInstanced tree2 = null;
        private readonly Color4 objColor = Color.Magenta;
        private bool objNotSet = true;

        private Model helicopter = null;
        private readonly HeliManipulatorController helicopterController = null;
        private readonly Vector3 helicopterHeightOffset = (Vector3.Up * 15f);
        private readonly Color4 curvesColor = Color.Red;
        private readonly Color4 pointsColor = Color.Blue;
        private readonly Color4 segmentsColor = new Color4(Color.Cyan.ToColor3(), 0.8f);
        private readonly Color4 hAxisColor = Color.YellowGreen;
        private readonly Color4 wAxisColor = Color.White;

        private PrimitiveListDrawer<Line3D> staticObjLineDrawer = null;
        private PrimitiveListDrawer<Line3D> movingObjLineDrawer = null;
        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;
        private PrimitiveListDrawer<Line3D> curveLineDrawer = null;
        private PrimitiveListDrawer<Line3D> terrainLineDrawer = null;
        private PrimitiveListDrawer<Line3D> terrainPointDrawer = null;
        private PrimitiveListDrawer<Triangle> terrainGraphDrawer = null;

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
        private ParticleManager pManager = null;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private string heliEffect;
        private GameAudioEffect heliEffectInstance;
        private string heliDestroyedEffect;
        private string tank1Effect;
        private string tank2Effect;
        private GameAudioEffect tank1EffectInstance;
        private GameAudioEffect tank2EffectInstance;
        private string tank1DestroyedEffect;
        private string tank2DestroyedEffect;
        private string tank1ShootingEffect;
        private string tank2ShootingEffect;
        private string[] impactEffects;
        private string[] damageEffects;

        private bool started = false;
        private readonly List<AIAgent> agents = new List<AIAgent>();

        private bool gameReady = false;

        public TestScene3D(Game game)
            : base(game, SceneModes.ForwardLigthning)
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
                this.debugTex?.Dispose();
                this.debugTex = null;
            }

            base.Dispose(disposing);
        }

        public override async Task Initialize()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000f;

            await this.LoadResourcesAsync(InitializeUI());

            List<Task> loadTasks = new List<Task>()
            {
                InitializeWalker(),
                InitializeDebug(),
                InitializeParticles(),
                InitializeLensFlare(),
                InitializeHelicopter(),
                InitializeTanks(),
                InitializeHeliport(),
                InitializeGarage(),
                InitializeBuildings(),
                InitializeObelisk(),
                InitializeRocks(),
                InitializeTrees(),
                InitializeSkydom(),
                InitializeClouds(),
                InitializeTerrain(),
                InitializeGardener(),
            };

            await this.LoadResourcesAsync(
                loadTasks.ToArray(),
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    InitializeAudio();

                    InitializeLights();

                    this.agentManager = new Brain(this);

                    this.gardener.SetWind(this.windDirection, this.windStrength);

                    this.AudioManager.MasterVolume = 1f;
                    this.AudioManager.Start();

                    this.Camera.Goto(this.heliport.Manipulator.Position + Vector3.One * 25f);
                    this.Camera.LookTo(0, 10, 0);

                    Task.WhenAll(InitializePathFinding());
                });
        }
        private async Task<TaskResult> InitializeUI()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, this.layerHud);
            this.stats = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 12, Color.Yellow) }, this.layerHud);
            this.counters1 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 10, Color.GreenYellow) }, this.layerHud);
            this.counters2 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Lucida Sans", 10, Color.GreenYellow) }, this.layerHud);

            title.Text = "Terrain collision and trajectories test";
            this.stats.Text = "";
            this.counters1.Text = "";
            this.counters2.Text = "";

            title.SetPosition(Vector2.Zero);
            this.stats.SetPosition(new Vector2(0, 46));
            this.counters1.SetPosition(new Vector2(0, 68));
            this.counters2.SetPosition(new Vector2(0, 90));

            var spDesc = new SpriteDescription()
            {
                Name = "Back Pannel",
                Width = this.Game.Form.RenderWidth,
                Height = this.counters2.Top + this.counters2.Height + 3,
                TintColor = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHud - 1);

            var spbDesc = new UIProgressBarDescription()
            {
                Name = "Progress bar",
                Width = 50,
                Height = 5,
                BaseColor = Color.Red,
                ProgressColor = Color.Green,
            };

            this.hProgressBar = await this.AddComponentUIProgressBar(spbDesc, layerGameHud);
            this.t1ProgressBar = await this.AddComponentUIProgressBar(spbDesc, layerGameHud);
            this.t2ProgressBar = await this.AddComponentUIProgressBar(spbDesc, layerGameHud);

            this.hProgressBar.Top = 120;
            this.t1ProgressBar.Top = 120;
            this.t2ProgressBar.Top = 120;

            this.hProgressBar.Left = 5;
            this.t1ProgressBar.Left = 135;
            this.t2ProgressBar.Left = 270;

            this.hProgressBar.Visible = false;
            this.t1ProgressBar.Visible = false;
            this.t2ProgressBar.Visible = false;

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
            this.cursor3D = await this.AddComponentModel(c3DDesc, SceneObjectUsages.UI, this.layerHud);

            var c2DDesc = new UICursorDescription()
            {
                Name = "Cursor2D",
                ContentPath = "resources/Cursor",
                Textures = new[] { "target.png" },
                TintColor = Color.Red,
                Width = 16,
                Height = 16,
            };
            this.cursor2D = await this.AddComponentUICursor(c2DDesc, this.layerHud + 1);
            this.cursor2D.TintColor = Color.Red;
            this.cursor2D.Visible = false;

            sw.Stop();
            return new TaskResult()
            {
                Text = "UI",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeWalker()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            await Task.Run(() =>
            {
                this.walkerAgentType = new Agent()
                {
                    Name = "Walker type",
                    Height = 1f,
                    Radius = 0.2f,
                    MaxClimb = 0.9f,
                };
            });

            sw.Stop();
            return new TaskResult()
            {
                Text = "Walker",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeDebug()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "++DEBUG++ Lights",
                    DepthEnabled = true,
                    Count = 5000,
                },
                SceneObjectUsages.None,
                this.layerEffects);
            this.lightsVolumeDrawer.Visible = false;

            #region DEBUG Shadow Map

            int width = 300;
            int height = 300;
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;
            var stDescription = new UITextureRendererDescription()
            {
                Name = "++DEBUG++ Shadow Map",
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = UITextureRendererChannels.Red,
            };
            this.shadowMapDrawer = await this.AddComponentUITextureRenderer(stDescription, this.layerHud);
            this.shadowMapDrawer.Visible = false;
            this.shadowMapDrawer.DeferredEnabled = false;

            this.debugTex = this.Game.ResourceManager.RequestResource(@"Resources\uvtest.png");

            #endregion

            #region DEBUG Path finding Graph

            this.terrainGraphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Name = "++DEBUG++ Path finding Graph",
                    Count = MaxGridDrawer
                },
                SceneObjectUsages.None,
                this.layerEffects);
            this.terrainGraphDrawer.Visible = false;

            #endregion

            #region DEBUG Picking test

            this.terrainPointDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "++DEBUG++ Picking test",
                    Count = MaxPickingTest
                },
                SceneObjectUsages.None,
                this.layerEffects);
            this.terrainPointDrawer.Visible = false;

            #endregion

            #region DEBUG Trajectory

            this.curveLineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "++DEBUG++ Trajectory",
                    Count = 20000
                },
                SceneObjectUsages.None,
                this.layerEffects);
            this.curveLineDrawer.Visible = false;

            #endregion

            #region DEBUG Helicopter manipulator

            this.movingObjLineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "++DEBUG++ Helicopter manipulator",
                    Count = 1000
                },
                SceneObjectUsages.None,
                this.layerEffects);
            this.movingObjLineDrawer.Visible = false;

            #endregion

            #region DEBUG static volumes

            this.staticObjLineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "++DEBUG++ Static Volumes",
                    Count = 20000
                },
                SceneObjectUsages.None,
                layerEffects);
            this.staticObjLineDrawer.Visible = false;

            #endregion

            #region DEBUG Ground position test

            this.terrainLineDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "++DEBUG++ Ground position test",
                    Count = 10000
                },
                SceneObjectUsages.None,
                this.layerEffects);
            this.terrainLineDrawer.Visible = false;

            #endregion

            sw.Stop();
            return new TaskResult()
            {
                Text = "Debug",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeLensFlare()
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
            await this.AddComponentLensFlare(lfDesc, SceneObjectUsages.None, this.layerEffects);

            sw.Stop();
            return new TaskResult()
            {
                Text = "LensFlare",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeHelicopter()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var hDesc = new ModelDescription()
            {
                Name = "Helicopter",
                CastShadow = true,
                TextureIndex = 0,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Helicopter",
                    ModelContentFilename = "M24.xml",
                }
            };
            this.helicopter = await this.AddComponentModel(hDesc, SceneObjectUsages.Agent, this.layerObjects);
            this.helicopter.Visible = false;
            this.helicopter.Manipulator.SetScale(0.15f);
            this.helicopter.Manipulator.UpdateInternals(true);

            this.Lights.AddRange(this.helicopter.Lights);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Helicopter",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeTanks()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var tDesc = new ModelDescription()
            {
                Name = "Tank",
                CastShadow = true,
                Optimize = false,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Leopard",
                    ModelContentFilename = "Leopard.xml",
                },
                TransformNames = new[] { "Barrel-mesh", "Turret-mesh", "Hull-mesh" },
                TransformDependences = new[] { 1, 2, -1 },
            };
            this.tankP1 = await this.AddComponentModel(tDesc, SceneObjectUsages.Agent, this.layerObjects);
            this.tankP2 = await this.AddComponentModel(tDesc, SceneObjectUsages.Agent, this.layerObjects);

            this.tankP1.Visible = false;
            this.tankP2.Visible = false;

            this.tankP1.Manipulator.SetScale(0.2f, true);
            this.tankP1.Manipulator.UpdateInternals(true);

            this.tankP2.Manipulator.SetScale(0.2f, true);
            this.tankP2.Manipulator.UpdateInternals(true);

            var tankbbox = this.tankP1.GetBoundingBox();

            // Initialize dust generation relative positions
            this.tankLeftCat = new Vector3(tankbbox.Maximum.X, tankbbox.Minimum.Y, tankbbox.Maximum.Z);
            this.tankRightCat = new Vector3(tankbbox.Minimum.X, tankbbox.Minimum.Y, tankbbox.Maximum.Z);

            // Initialize agent
            this.tankAgentType = new Agent()
            {
                Name = "Tank type",
                Height = tankbbox.Height,
                Radius = tankbbox.Width * 0.5f,
                MaxClimb = tankbbox.Height * 0.1f,
            };

            this.Lights.AddRange(this.tankP1.Lights);
            this.Lights.AddRange(this.tankP2.Lights);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Tanks",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeHeliport()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var hpDesc = new ModelDescription()
            {
                Name = "Heliport",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Heliport",
                    ModelContentFilename = "Heliport.xml",
                }
            };
            this.heliport = await this.AddComponentModel(hpDesc, SceneObjectUsages.None, this.layerObjects);
            this.heliport.Visible = false;
            this.AttachToGround(this.heliport, true);

            this.Lights.AddRange(this.heliport.Lights);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Heliport",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeGarage()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var gDesc = new ModelDescription()
            {
                Name = "Garage",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Garage",
                    ModelContentFilename = "Garage.xml",
                }
            };
            this.garage = await this.AddComponentModel(gDesc, SceneObjectUsages.None, this.layerObjects);
            this.garage.Visible = false;
            this.AttachToGround(this.garage, true);

            this.Lights.AddRange(this.garage.Lights);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Garage",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeBuildings()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var gDesc = new ModelDescription()
            {
                Name = "Buildings",
                CastShadow = true,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Buildings",
                    ModelContentFilename = "Building_1.xml",
                }
            };
            this.building = await this.AddComponentModel(gDesc, SceneObjectUsages.None, this.layerObjects);
            this.building.Visible = false;
            this.AttachToGround(this.building, true);

            this.Lights.AddRange(this.building.Lights);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Buildings",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeObelisk()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var oDesc = new ModelInstancedDescription()
            {
                Name = "Obelisk",
                CastShadow = true,
                Instances = 4,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Obelisk",
                    ModelContentFilename = "Obelisk.xml",
                }
            };
            this.obelisk = await this.AddComponentModelInstanced(oDesc, SceneObjectUsages.None, this.layerObjects);
            this.obelisk.Visible = false;
            this.AttachToGround(this.obelisk, true);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Obelisk",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeRocks()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var rDesc = new ModelInstancedDescription()
            {
                Name = "Rocks",
                CastShadow = true,
                Instances = 250,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Rocks",
                    ModelContentFilename = "boulder.xml",
                }
            };
            this.rocks = await this.AddComponentModelInstanced(rDesc, SceneObjectUsages.None, this.layerObjects);
            this.rocks.Visible = false;
            this.AttachToGround(this.rocks, false);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Rocks",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeTrees()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var t1Desc = new ModelInstancedDescription()
            {
                Name = "birch_a",
                CastShadow = true,
                BlendMode = BlendModes.DefaultTransparent,
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
                BlendMode = BlendModes.DefaultTransparent,
                Instances = 100,
                Content = new ContentDescription()
                {
                    ContentFolder = "resources/Trees",
                    ModelContentFilename = "birch_b.xml",
                }
            };
            this.tree1 = await this.AddComponentModelInstanced(t1Desc, SceneObjectUsages.None, this.layerTerrain);
            this.tree2 = await this.AddComponentModelInstanced(t2Desc, SceneObjectUsages.None, this.layerTerrain);
            this.tree1.Visible = false;
            this.tree2.Visible = false;

            this.AttachToGround(this.tree1, false);
            this.AttachToGround(this.tree2, false);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Trees",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeSkydom()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            await this.AddComponentSkydom(new SkydomDescription()
            {
                Name = "Skydom",
                ContentPath = "resources/Skydom",
                Texture = "sunset.dds",
                Radius = this.Camera.FarPlaneDistance,
            });

            sw.Stop();
            return new TaskResult()
            {
                Text = "Skydom",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeClouds()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            await this.AddComponentSkyPlane(new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "Resources/clouds",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                SkyMode = SkyPlaneModes.Perturbed,
                MaxBrightness = 0.8f,
                MinBrightness = 0.1f,
                Repeat = 5,
                Velocity = 1,
                Direction = new Vector2(1, 1),
            });

            sw.Stop();
            return new TaskResult()
            {
                Text = "Clouds",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeTerrain()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var terrainDescription = GroundDescription.FromFile("resources/Terrain", "two_levels.xml", 1);
            this.terrain = await this.AddComponentScenery(terrainDescription, SceneObjectUsages.Ground, this.layerTerrain);
            this.SetGround(this.terrain, true);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Terrain",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeGardener()
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
            this.gardener = await this.AddComponentGroundGardener(grDesc, SceneObjectUsages.None, this.layerTerrain);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Gardener",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeParticles()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            this.pPlume = ParticleSystemDescription.InitializeSmokePlume("resources/particles", "smoke.png");
            this.pFire = ParticleSystemDescription.InitializeFire("resources/particles", "fire.png");
            this.pDust = ParticleSystemDescription.InitializeDust("resources/particles", "smoke.png");
            this.pProjectile = ParticleSystemDescription.InitializeProjectileTrail("resources/particles", "smoke.png");
            this.pExplosion = ParticleSystemDescription.InitializeExplosion("resources/particles", "fire.png");
            this.pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("resources/particles", "smoke.png");

            this.pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Manager" }, SceneObjectUsages.None, layerEffects);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Particles",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializePathFinding()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            Random posRnd = new Random(1);

            await this.InitializePositionRocks(posRnd);
            await this.InitializePositionTrees(posRnd);

            await Task.WhenAll(
                this.InitializePositionHeliport(),
                this.InitializePositionGarage(),
                this.InitializePositionBuildings(),
                this.InitializePositionObelisk());

            var navSettings = BuildSettings.Default;
            navSettings.Agents = new[]
            {
                walkerAgentType,
                tankAgentType,
            };
            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new PathFinderDescription(navSettings, nvInput);

            sw.Stop();

            await this.UpdateNavigationGraph();

            return new TaskResult()
            {
                Text = "PathFinding",
                Duration = sw.Elapsed,
            };
        }
        private void InitializeLights()
        {
            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[0].CastShadow = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = true;
        }
        private void InitializeAudio()
        {
            string forestEffect = "Forest";
            heliEffect = "Helicopter";
            heliDestroyedEffect = "HelicopterDestroyed";
            tank1Effect = "Tank1";
            tank2Effect = "Tank2";
            tank1DestroyedEffect = "Tank1Destroyed";
            tank2DestroyedEffect = "Tank2Destroyed";
            tank1ShootingEffect = "Tank1Shooting";
            tank2ShootingEffect = "Tank2Shooting";
            impactEffects = new[] { "Impact1", "Impact2", "Impact3", "Impact4" };
            damageEffects = new[] { "Damage1", "Damage2", "Damage3", "Damage4" };

            AudioManager.LoadSound(forestEffect, "Resources/Audio/Effects", "wind_birds_forest_01.wav");
            AudioManager.LoadSound(heliEffect, "Resources/Audio/Effects", "heli.wav");
            AudioManager.LoadSound(heliDestroyedEffect, "Resources/Audio/Effects", "explosion_helicopter_close_01.wav");
            AudioManager.LoadSound("Tank", "Resources/Audio/Effects", "tank_engine.wav");
            AudioManager.LoadSound("TankDestroyed", "Resources/Audio/Effects", "explosion_vehicle_small_close_01.wav");
            AudioManager.LoadSound("TankShooting", "Resources/Audio/Effects", "machinegun-shooting.wav");
            AudioManager.LoadSound(impactEffects[0], "Resources/Audio/Effects", "metal_grate_large_01.wav");
            AudioManager.LoadSound(impactEffects[1], "Resources/Audio/Effects", "metal_grate_large_02.wav");
            AudioManager.LoadSound(impactEffects[2], "Resources/Audio/Effects", "metal_grate_large_03.wav");
            AudioManager.LoadSound(impactEffects[3], "Resources/Audio/Effects", "metal_grate_large_04.wav");
            AudioManager.LoadSound(damageEffects[0], "Resources/Audio/Effects", "metal_pipe_large_01.wav");
            AudioManager.LoadSound(damageEffects[1], "Resources/Audio/Effects", "metal_pipe_large_02.wav");
            AudioManager.LoadSound(damageEffects[2], "Resources/Audio/Effects", "metal_pipe_large_03.wav");
            AudioManager.LoadSound(damageEffects[3], "Resources/Audio/Effects", "metal_pipe_large_04.wav");

            AudioManager.AddEffectParams(
                forestEffect,
                new GameAudioEffectParameters
                {
                    SoundName = forestEffect,
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                heliEffect,
                new GameAudioEffectParameters
                {
                    SoundName = heliEffect,
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 200,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                heliDestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = heliDestroyedEffect,
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tank1Effect,
                new GameAudioEffectParameters
                {
                    SoundName = "Tank",
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 150,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tank2Effect,
                new GameAudioEffectParameters
                {
                    SoundName = "Tank",
                    DestroyWhenFinished = false,
                    IsLooped = true,
                    UseAudio3D = true,
                    EmitterRadius = 150,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tank1DestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankDestroyed",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tank2DestroyedEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankDestroyed",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tank1ShootingEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankShooting",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                tank2ShootingEffect,
                new GameAudioEffectParameters
                {
                    SoundName = "TankShooting",
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                impactEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                impactEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                impactEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                impactEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = impactEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            AudioManager.AddEffectParams(
                damageEffects[0],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[0],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                damageEffects[1],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[1],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                damageEffects[2],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[2],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });
            AudioManager.AddEffectParams(
                damageEffects[3],
                new GameAudioEffectParameters
                {
                    SoundName = damageEffects[3],
                    IsLooped = false,
                    UseAudio3D = true,
                    EmitterRadius = 250,
                    UseReverb = true,
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            var forestEffectInstance = AudioManager.CreateEffectInstance(forestEffect);
            forestEffectInstance.Play();
        }
        private async Task InitializePositionRocks(Random posRnd)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < this.rocks.InstanceCount; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        float scale;
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

                        var rockInstance = this.rocks[i];

                        rockInstance.Manipulator.SetPosition(r.Position);
                        rockInstance.Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi));
                        rockInstance.Manipulator.SetScale(scale);
                        rockInstance.Manipulator.UpdateInternals(true);
                    }
                }
                this.rocks.Visible = true;
            });
        }
        private async Task InitializePositionTrees(Random posRnd)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < this.tree1.InstanceCount; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var treeInstance = this.tree1[i];

                        treeInstance.Manipulator.SetPosition(r.Position);
                        treeInstance.Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0);
                        treeInstance.Manipulator.SetScale(posRnd.NextFloat(0.25f, 0.75f));
                        treeInstance.Manipulator.UpdateInternals(true);
                    }
                }
                this.tree1.Visible = true;

                for (int i = 0; i < this.tree2.InstanceCount; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                    if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var treeInstance = this.tree2[i];

                        treeInstance.Manipulator.SetPosition(r.Position);
                        treeInstance.Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0);
                        treeInstance.Manipulator.SetScale(posRnd.NextFloat(0.25f, 0.75f));
                        treeInstance.Manipulator.UpdateInternals(true);
                    }
                }
                this.tree2.Visible = true;
            });
        }
        private async Task InitializePositionHeliport()
        {
            await Task.Run(() =>
            {
                if (this.FindTopGroundPosition(75, 75, out PickingResult<Triangle> r))
                {
                    this.heliport.Manipulator.SetPosition(r.Position);
                    this.heliport.Manipulator.UpdateInternals(true);
                }
                this.heliport.Visible = true;
            });
        }
        private async Task InitializePositionGarage()
        {
            await Task.Run(() =>
            {
                if (this.FindTopGroundPosition(-10, -40, out PickingResult<Triangle> r))
                {
                    this.garage.Manipulator.SetPosition(r.Position);
                    this.garage.Manipulator.SetRotation(MathUtil.PiOverFour * 0.5f + MathUtil.Pi, 0, 0);
                    this.garage.Manipulator.UpdateInternals(true);
                }
                this.garage.Visible = true;
            });
        }
        private async Task InitializePositionBuildings()
        {
            await Task.Run(() =>
            {
                if (this.FindTopGroundPosition(-30, -40, out PickingResult<Triangle> r))
                {
                    this.building.Manipulator.SetPosition(r.Position);
                    this.building.Manipulator.SetRotation(MathUtil.PiOverFour * 0.5f + MathUtil.Pi, 0, 0);
                    this.building.Manipulator.UpdateInternals(true);
                }
                this.building.Visible = true;
            });
        }
        private async Task InitializePositionObelisk()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < this.obelisk.InstanceCount; i++)
                {
                    int ox = i == 0 || i == 2 ? 1 : -1;
                    int oy = i == 0 || i == 1 ? 1 : -1;

                    if (this.FindTopGroundPosition(ox * 50, oy * 50, out PickingResult<Triangle> r))
                    {
                        var obeliskInstance = this.obelisk[i];

                        obeliskInstance.Manipulator.SetPosition(r.Position);
                        obeliskInstance.Manipulator.SetScale(1.5f);
                        obeliskInstance.Manipulator.UpdateInternals(true);
                    }
                }
                this.obelisk.Visible = true;
            });
        }

        public override void NavigationGraphUpdated()
        {
            gameReady = true;

            Task.Run(async () =>
            {
                await StartHelicopter();
                await StartTanks();
                await StartDebug();

                started = true;
            });
        }
        private async Task StartHelicopter()
        {
            await Task.Run(() =>
            {
                // Set position
                var sceneryUsage = SceneObjectUsages.CoarsePathFinding | SceneObjectUsages.FullPathFinding;
                var ray = this.GetTopDownRay(this.heliport.Manipulator.Position);
                if (this.PickNearest(ray, RayPickingParams.Geometry, sceneryUsage, out PickingResult<Triangle> r))
                {
                    this.helicopter.Manipulator.SetPosition(r.Position);
                    this.helicopter.Manipulator.SetNormal(r.Item.Normal);
                }

                var hp = new AnimationPath();
                hp.AddLoop("roll");
                this.animations.Add("heli_default", new AnimationPlan(hp));

                // Register animation paths
                AnimationPath ap = new AnimationPath();
                ap.AddLoop("default");
                this.animations.Add("default", new AnimationPlan(ap));

                // Set animation
                this.helicopter.AnimationController.SetPath(this.animations["heli_default"]);
                this.helicopter.AnimationController.TimeDelta = 3f;
                this.helicopter.AnimationController.Start();

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
                agents.Add(this.helicopterAgent);

                // Register events
                this.helicopterAgent.Moving += Agent_Moving;
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
                this.helicopterAgent.ActiveAI = true;

                //Show
                this.helicopter.Visible = true;
            });
        }
        private async Task StartTanks()
        {
            await Task.Run(() =>
            {
                var sceneryUsage = SceneObjectUsages.CoarsePathFinding | SceneObjectUsages.FullPathFinding;

                if (this.PickNearest(this.GetTopDownRay(-60, -60), RayPickingParams.Geometry, sceneryUsage, out PickingResult<Triangle> r1))
                {
                    this.tankP1.Manipulator.SetPosition(r1.Position);
                    this.tankP1.Manipulator.SetNormal(r1.Item.Normal);
                }

                if (this.PickNearest(this.GetTopDownRay(-70, 70), RayPickingParams.Geometry, sceneryUsage, out PickingResult<Triangle> r2))
                {
                    this.tankP2.Manipulator.SetPosition(r2.Position);
                    this.tankP2.Manipulator.SetNormal(r2.Item.Normal);
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

                agents.Add(this.tankP1Agent);
                agents.Add(this.tankP2Agent);

                // Register events
                this.tankP1Agent.Moving += Agent_Moving;
                this.tankP1Agent.Attacking += Agent_Attacking;
                this.tankP1Agent.Damaged += Agent_Damaged;
                this.tankP1Agent.Destroyed += Agent_Destroyed;
                this.agentManager.AddAgent(1, this.tankP1Agent);

                this.tankP2Agent.Moving += Agent_Moving;
                this.tankP2Agent.Attacking += Agent_Attacking;
                this.tankP2Agent.Damaged += Agent_Damaged;
                this.tankP2Agent.Destroyed += Agent_Destroyed;
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
                this.tankP1Agent.ActiveAI = true;

                this.tankP2Agent.PatrolBehavior.InitPatrollingBehavior(t2CheckPoints, 10, 5);
                this.tankP2Agent.AttackBehavior.InitAttackingBehavior(7, 10);
                this.tankP2Agent.RetreatBehavior.InitRetreatingBehavior(new Vector3(-10, 0, -40), 10);
                this.tankP2Agent.ActiveAI = true;

                //Show
                this.tankP1.Visible = true;
                this.tankP2.Visible = true;
            });
        }
        private async Task StartDebug()
        {
            await Task.Run(() =>
            {
                // Ground position test
                var bbox = this.terrain.GetBoundingBox();

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
                    this.terrainLineDrawer.AddPrimitives(Color.Green, this.oks.ToArray());
                }
                if (this.errs.Count > 0)
                {
                    this.terrainLineDrawer.AddPrimitives(Color.Red, this.errs.ToArray());
                }

                // Axis
                this.curveLineDrawer.SetPrimitives(this.wAxisColor, Line3D.CreateAxis(Matrix.Identity, 20f));
                this.curveLineDrawer.Visible = false;
            });
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

            var pickingRay = this.GetPickingRay();

            UpdateInputPlayer();
            UpdateInputCamera(gameTime, pickingRay);
            UpdateInputDrawers();
            UpdateInputHelicopterTexture();
            UpdateInputDebug(pickingRay);
            UpdateInputGraph();

            if (started)
            {
                UpdateCursor(pickingRay);
                UpdateTanks(pickingRay);
                UpdateHelicopter();
                UpdateDebug();

                agents.ForEach(a => a.Update(gameTime));
            }
        }
        private void UpdateInputPlayer()
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

                    var pos = this.heliport.Manipulator.Position;
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
        private void UpdateInputCamera(GameTime gameTime, Ray pickingRay)
        {
            if (this.walkMode)
            {
                this.UpdateInputWalker(gameTime);
            }
            else
            {
                this.UpdateInputFree(gameTime, pickingRay);
            }
        }
        private void UpdateInputWalker(GameTime gameTime)
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

            if (this.Walk(this.walkerAgentType, prevPos, this.Camera.Position, true, out Vector3 walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }
        }
        private void UpdateInputFree(GameTime gameTime, Ray pickingRay)
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

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                if (this.follow)
                {
                    this.followTarget = null;
                    this.follow = false;
                }

                if (this.PickNearest(pickingRay, 0, RayPickingParams.Default, SceneObjectUsages.Agent, out ISceneObject agent))
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

            if (this.follow &&
                this.followTarget is IRayPickable<Triangle> pickable &&
                this.followTarget is ITransformable3D transform)
            {
                var sph = pickable.GetBoundingSphere();
                this.Camera.LookTo(sph.Center);
                this.Camera.Goto(sph.Center + (transform.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
            }
        }
        private void UpdateInputHelicopterTexture()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.helicopter.TextureIndex++;
                if (this.helicopter.TextureIndex > 2) this.helicopter.TextureIndex = 2;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.helicopter.TextureIndex--;
                if (this.helicopter.TextureIndex < 0) this.helicopter.TextureIndex = 0;
            }
        }
        private void UpdateInputDebug(Ray pickingRay)
        {
            if (this.Game.Input.KeyJustReleased(Keys.C))
            {
                this.Lights.KeyLight.CastShadow = !this.Lights.KeyLight.CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up) && !this.Game.Input.ShiftPressed)
            {
                this.shadowResult = SceneRendererResults.ShadowMapDirectional;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Down) && !this.Game.Input.ShiftPressed)
            {
                this.shadowResult = SceneRendererResults.ShadowMapDirectional;
            }

            if (this.Game.Input.KeyJustReleased(Keys.D1))
            {
                this.walkMode = !this.walkMode;
                this.DEBUGUpdateGraphDrawer();
                this.walkMode = !this.walkMode;
            }

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                var visible = this.terrainGraphDrawer.Visible;
                if (visible)
                {
                    this.terrainPointDrawer.Clear();

                    if (this.PickNearest(pickingRay, RayPickingParams.Default, out PickingResult<Triangle> r))
                    {
                        this.DEBUGPickingPosition(r.Position);
                    }
                }
            }
        }
        private void UpdateInputDrawers()
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

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.shadowMapDrawer.Visible = !this.shadowMapDrawer.Visible;
                this.shadowResult = SceneRendererResults.ShadowMapDirectional;
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
                if (!this.drawDrawVolumes && !this.drawCullVolumes)
                {
                    this.drawDrawVolumes = true;
                    this.drawCullVolumes = false;
                }
                else if (this.drawDrawVolumes && !this.drawCullVolumes)
                {
                    this.drawDrawVolumes = false;
                    this.drawCullVolumes = true;
                }
                else if (!this.drawDrawVolumes)
                {
                    this.drawDrawVolumes = false;
                    this.drawCullVolumes = false;
                }
            }
        }
        private void UpdateInputGraph()
        {
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
        }
        private void UpdateCursor(Ray pickingRay)
        {
            if (!this.walkMode && this.terrain.PickNearest(pickingRay, out PickingResult<Triangle> r))
            {
                this.cursor3D.Manipulator.SetPosition(r.Position);
            }
        }
        private void UpdateTanks(Ray pickingRay)
        {
            if (this.Game.Input.LeftMouseButtonPressed)
            {
                //Draw path before set it to the agent
                DrawTankPath(pickingRay);
            }

            if (this.Game.Input.LeftMouseButtonJustReleased && !this.Game.Input.RightMouseButtonPressed)
            {
                //Calc path and set it to the agent
                UpdateTankPath(pickingRay);
            }

            bool shift = this.Game.Input.KeyPressed(Keys.ShiftKey);
            if (shift)
            {
                if (this.Game.Input.KeyJustReleased(Keys.D1))
                {
                    this.tankP1Agent.ActiveAI = !this.tankP1Agent.ActiveAI;
                }
                if (this.Game.Input.KeyJustReleased(Keys.D2))
                {
                    this.tankP2Agent.ActiveAI = !this.tankP2Agent.ActiveAI;
                }
            }

            this.SetStatsScreenPosition(this.tankP1Agent, 4, this.t1ProgressBar);
            this.SetStatsScreenPosition(this.tankP2Agent, 4, this.t2ProgressBar);

            this.DEBUGDrawTankPath(
                tankP1Agent.Manipulator.Position,
                tankP1Agent.Manipulator.Forward * 10f,
                tankP1Agent.Controller.SamplePath().ToArray(),
                Color.Yellow, Color.GreenYellow);

            this.DEBUGDrawTankPath(
                tankP2Agent.Manipulator.Position,
                tankP2Agent.Manipulator.Forward * 10f,
                tankP2Agent.Controller.SamplePath().ToArray(),
                Color.Firebrick, Color.Coral);
        }
        private void UpdateHelicopter()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                Curve3D curve = this.GenerateHelicopterPath();
                this.helicopter.AnimationController.SetPath(this.animations["heli_default"]);
                this.DEBUGDrawHelicopterPath(curve);
            }

            this.SetStatsScreenPosition(this.helicopterAgent, 4, this.hProgressBar);
        }
        private void UpdateDebug()
        {
            if (this.drawDrawVolumes)
            {
                this.DEBUGDrawLightMarks();
            }

            if (this.drawCullVolumes)
            {
                this.DEBUGDrawLightVolumes();
            }

            if (this.curveLineDrawer.Visible)
            {
                Matrix rot = Matrix.RotationQuaternion(this.helicopter.Manipulator.Rotation) * Matrix.Translation(this.helicopter.Manipulator.Position);
                this.curveLineDrawer.SetPrimitives(this.hAxisColor, Line3D.CreateAxis(rot, 5f));
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

            var tp = this.helicopterAgent.Target;
            if (tp.HasValue)
            {
                this.DEBUGPickingPosition(tp.Value);
            }
        }
        private void SetStatsScreenPosition(AIAgent agent, float height, UIProgressBar pb)
        {
            var screenPosition = this.GetScreenCoordinates(agent.Manipulator.Position, out bool centerInside);
            var top = this.GetScreenCoordinates(agent.Manipulator.Position + new Vector3(0, height, 0), out bool topInside);

            if (centerInside || topInside)
            {
                pb.Visible = agent.Stats.Life > 0;

                screenPosition.X = top.X - (pb.Width * 0.5f);
                screenPosition.Y = top.Y;

                pb.Top = (int)screenPosition.Y;
                pb.Left = (int)screenPosition.X;
            }
            else
            {
                pb.Visible = false;
            }
        }
        private void DrawTankPath(Ray pickingRay)
        {
            var picked = this.PickNearest(pickingRay, RayPickingParams.Default, out PickingResult<Triangle> r);
            if (picked)
            {
                var t1Position = this.tankP1.Manipulator.Position;

                var result = this.FindPath(this.tankAgentType, t1Position, r.Position);
                if (result != null)
                {
                    this.DEBUGDrawTankPath(
                        null,
                        null,
                        result.ReturnPath.ToArray(),
                        Color.Red, Color.IndianRed);
                }
            }
        }
        private void UpdateTankPath(Ray pickingRay)
        {
            var picked = this.PickNearest(pickingRay, RayPickingParams.Default, out PickingResult<Triangle> r);
            if (picked)
            {
                Task.Run(async () =>
                {
                    var path = await this.FindPathAsync(this.tankAgentType, this.tankP1.Manipulator.Position, r.Position, true, 0.25f);
                    if (path != null)
                    {
                        this.tankP1Agent.Clear();
                        this.tankP1Agent.FollowPath(path, 10);
                    }
                });
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.shadowMapDrawer.Texture = this.useDebugTex ? this.debugTex : this.Renderer.GetResource(this.shadowResult);

            #region Texts

            this.stats.Text = this.Game.RuntimeText;

            string txt1 = string.Format(
                "Buffers active: {0} {1} Kbs, reads: {2}, writes: {3}; {4} - Result: {5}; Primitives: {6}",
                Counters.Buffers,
                Counters.BufferBytes / 1024,
                Counters.BufferReads,
                Counters.BufferWrites,
                this.GetRenderMode(),
                this.shadowResult,
                Counters.PrimitivesPerFrame);
            this.counters1.Text = txt1;

            string txt2 = string.Format(
                "IA Input Layouts: {0}, Primitives: {1}, VB: {2}, IB: {3}, Terrain Patches: {4}; T1.{5}  /  T2.{6}  /  H.{7}",
                Counters.IAInputLayoutSets,
                Counters.IAPrimitiveTopologySets,
                Counters.IAVertexBuffersSets,
                Counters.IAIndexBufferSets,
                this.terrain.VisiblePatchesCount,
                this.tankP1Agent,
                this.tankP2Agent,
                this.helicopterAgent);
            this.counters2.Text = txt2;

            this.hProgressBar.ProgressValue = (1f - this.helicopterAgent?.Stats.Damage ?? 0);
            this.t1ProgressBar.ProgressValue = (1f - this.tankP1Agent?.Stats.Damage ?? 0);
            this.t2ProgressBar.ProgressValue = (1f - this.tankP2Agent?.Stats.Damage ?? 0);

            #endregion
        }

        private Curve3D GenerateHelicopterPath()
        {
            Curve3D curve = new Curve3D
            {
                PreLoop = CurveLoopType.Constant,
                PostLoop = CurveLoopType.Constant
            };

            Vector3[] cPoints = new Vector3[15];

            if (this.helicopterController != null && this.helicopterController.HasPath)
            {
                for (int i = 0; i < cPoints.Length - 2; i++)
                {
                    cPoints[i] = this.GetRandomPoint(Helper.RandomGenerator, this.helicopterHeightOffset);
                }
            }
            else
            {
                cPoints[0] = this.helicopter.Manipulator.Position;
                cPoints[1] = this.helicopter.Manipulator.Position + (Vector3.Up * 5f) + (this.helicopter.Manipulator.Forward * 10f);

                for (int i = 2; i < cPoints.Length - 2; i++)
                {
                    cPoints[i] = this.GetRandomPoint(Helper.RandomGenerator, this.helicopterHeightOffset);
                }
            }

            var hPos = this.heliport.Manipulator.Position;
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

            //Start sounds
            if (e.Active == helicopterAgent && heliEffectInstance == null)
            {
                heliEffectInstance = AudioManager.CreateEffectInstance(heliEffect, this.helicopter, this.Camera);
                heliEffectInstance.Play();
            }

            if (e.Active == tankP1Agent && tank1EffectInstance == null)
            {
                tank1EffectInstance = AudioManager.CreateEffectInstance(tank1Effect, this.tankP1, this.Camera);
                tank1EffectInstance?.Play();
            }

            if (e.Active == tankP2Agent && tank2EffectInstance == null)
            {
                tank2EffectInstance = AudioManager.CreateEffectInstance(tank2Effect, this.tankP2, this.Camera);
                tank2EffectInstance?.Play();
            }
        }
        private void Agent_Attacking(object sender, BehaviorEventArgs e)
        {
            int index = Helper.RandomGenerator.Next(0, 4);
            index %= 3;
            AudioManager.CreateEffectInstance(impactEffects[index], e.Passive.Manipulator, this.Camera)?.Play();

            this.AddProjectileTrailSystem(e.Active, e.Passive, 50f);
        }
        private void Agent_Damaged(object sender, BehaviorEventArgs e)
        {
            if (e.Active == tankP1Agent)
            {
                AudioManager.CreateEffectInstance(tank1ShootingEffect, e.Active.Manipulator, this.Camera)?.Play();
            }
            if (e.Active == tankP2Agent)
            {
                AudioManager.CreateEffectInstance(tank2ShootingEffect, e.Active.Manipulator, this.Camera)?.Play();
            }

            int index = Helper.RandomGenerator.Next(0, 4);
            index %= 3;
            AudioManager.CreateEffectInstance(damageEffects[index], e.Passive.Manipulator, this.Camera)?.Play();

            this.AddExplosionSystem(e.Passive);
            this.AddExplosionSystem(e.Passive);
            this.AddSmokeSystem(e.Passive, false);
        }
        private void Agent_Destroyed(object sender, BehaviorEventArgs e)
        {
            if (e.Passive == this.helicopterAgent)
            {
                heliEffectInstance?.Stop();

                AudioManager.CreateEffectInstance(heliDestroyedEffect, e.Passive.Manipulator, this.Camera)?.Play();

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
                if (e.Passive == this.tankP1Agent)
                {
                    tank1EffectInstance?.Stop();
                    AudioManager.CreateEffectInstance(tank1DestroyedEffect, e.Passive.Manipulator, this.Camera)?.Play();
                }
                if (e.Passive == this.tankP2Agent)
                {
                    tank2EffectInstance?.Stop();
                    AudioManager.CreateEffectInstance(tank2DestroyedEffect, e.Passive.Manipulator, this.Camera)?.Play();
                }

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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pExplosion, emitter1);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pExplosion, emitter1);
            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pSmokeExplosion, emitter2);
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

                plumeFire = this.pManager.GetParticleSystem("plumeFire");
                plumeSmoke = this.pManager.GetParticleSystem("plumeSmoke");
            }

            float duration = Helper.RandomGenerator.NextFloat(6, 36);
            float rate = Helper.RandomGenerator.NextFloat(0.1f, 1f);

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

                this.pManager.AddParticleSystem(plumeFireSystemName, ParticleSystemTypes.CPU, this.pFire, emitter1);
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

                this.pManager.AddParticleSystem(plumeSmokeSystemName, ParticleSystemTypes.CPU, this.pPlume, emitter2);
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
                smoke = this.pManager.GetParticleSystem(smokeSystemName);
            }

            float duration = Helper.RandomGenerator.NextFloat(5, 15);

            if (smoke == null)
            {
                var emitter = new MovingEmitter(agent.Manipulator, Vector3.Zero)
                {
                    Velocity = Vector3.Up,
                    Duration = duration + (duration * 0.1f),
                    EmissionRate = Helper.RandomGenerator.NextFloat(0.1f, 1f),
                    InfiniteDuration = false,
                    MaximumDistance = 500f,
                };

                this.pManager.AddParticleSystem(smokeSystemName, ParticleSystemTypes.CPU, this.pPlume, emitter);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDust, emitter);
        }
        private void AddProjectileTrailSystem(AIAgent agent, AIAgent target, float speed)
        {
            var targetDelta = Helper.RandomGenerator.NextVector3(-Vector3.One, Vector3.One);

            var emitter = new LinealEmitter(agent.Manipulator.Position, target.Manipulator.Position + targetDelta, speed)
            {
                EmissionRate = 0.0001f,
                MaximumDistance = 100f,
            };

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pProjectile, emitter);
        }

        private void DEBUGPickingPosition(Vector3 position)
        {
            if (this.FindAllGroundPosition(position.X, position.Z, out PickingResult<Triangle>[] results))
            {
                var positions = results.Select(r => r.Position).ToArray();
                var triangles = results.Select(r => r.Item).ToArray();

                this.terrainPointDrawer.SetPrimitives(Color.Magenta, Line3D.CreateCrossList(positions, 1f));
                this.terrainPointDrawer.SetPrimitives(Color.DarkCyan, Line3D.CreateWiredTriangle(triangles));
                if (positions.Length > 1)
                {
                    this.terrainPointDrawer.SetPrimitives(Color.Cyan, new Line3D(positions[0], positions[positions.Length - 1]));
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

            this.curveLineDrawer.SetPrimitives(this.curvesColor, Line3D.CreatePath(path.ToArray()));
            this.curveLineDrawer.SetPrimitives(this.pointsColor, Line3D.CreateCrossList(curve.Points, 0.5f));
            this.curveLineDrawer.SetPrimitives(this.segmentsColor, Line3D.CreatePath(curve.Points));
        }
        private void DEBUGDrawTankPath(Vector3? from, Vector3? direction, Vector3[] path, Color pathColor, Color arrowColor)
        {
            this.terrainPointDrawer.Clear(pathColor);
            this.terrainPointDrawer.Clear(arrowColor);

            if (path?.Any() == true)
            {
                int count = Math.Min(path.Length, MaxPickingTest);

                Line3D[] lines = new Line3D[count];

                for (int i = 1; i < count; i++)
                {
                    lines[i] = new Line3D(path[i - 1], path[i]);
                }

                this.terrainPointDrawer.SetPrimitives(pathColor, lines);
            }

            if (from.HasValue && direction.HasValue)
            {
                var arrow = Line3D.CreateArrow(from.Value, from.Value + direction.Value, 10f);
                this.terrainPointDrawer.SetPrimitives(arrowColor, arrow);
            }
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var agent = this.walkMode ? this.walkerAgentType : this.tankAgentType;

            var nodes = this.GetNodes(agent).OfType<GraphNode>();
            if (nodes.Any())
            {
                if (this.graphIndex <= -1)
                {
                    this.graphIndex = -1;

                    this.terrainGraphDrawer.Clear();

                    foreach (var node in nodes)
                    {
                        this.terrainGraphDrawer.AddPrimitives(node.Color, node.Triangles);
                    }
                }
                else
                {
                    if (this.graphIndex >= nodes.Count())
                    {
                        this.graphIndex = nodes.Count() - 1;
                    }

                    if (this.graphIndex < nodes.Count())
                    {
                        this.terrainGraphDrawer.Clear();

                        var node = nodes.ToArray()[this.graphIndex];

                        this.terrainGraphDrawer.SetPrimitives(node.Color, node.Triangles);
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
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                this.lightsVolumeDrawer.AddPrimitives(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                this.lightsVolumeDrawer.AddPrimitives(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawLightMarks()
        {
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 10, 10);

                this.lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 10, 10);

                this.lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawStaticVolumes()
        {
            List<Line3D> lines = new List<Line3D>();
            lines.AddRange(Line3D.CreateWiredBox(this.heliport.GetBoundingBox()));
            lines.AddRange(Line3D.CreateWiredBox(this.garage.GetBoundingBox()));
            for (int i = 0; i < this.obelisk.InstanceCount; i++)
            {
                var instance = this.obelisk[i];

                lines.AddRange(Line3D.CreateWiredBox(instance.GetBoundingBox()));
            }
            for (int i = 0; i < this.rocks.InstanceCount; i++)
            {
                var instance = this.rocks[i];

                lines.AddRange(Line3D.CreateWiredBox(instance.GetBoundingBox()));
            }
            for (int i = 0; i < this.tree1.InstanceCount; i++)
            {
                var instance = this.tree1[i];

                lines.AddRange(Line3D.CreateWiredTriangle(instance.GetVolume(false)));
            }

            for (int i = 0; i < this.tree2.InstanceCount; i++)
            {
                var instance = this.tree2[i];

                lines.AddRange(Line3D.CreateWiredTriangle(instance.GetVolume(false)));
            }

            this.staticObjLineDrawer.SetPrimitives(objColor, lines.ToArray());
        }
        private void DEBUGDrawMovingVolumes()
        {
            var hsph = this.helicopter.GetBoundingSphere();
            this.movingObjLineDrawer.SetPrimitives(new Color4(Color.White.ToColor3(), 0.55f), Line3D.CreateWiredSphere(new[] { hsph, }, 50, 20));

            var t1sph = this.tankP1.GetBoundingBox();
            var t2sph = this.tankP2.GetBoundingBox();
            this.movingObjLineDrawer.SetPrimitives(new Color4(Color.YellowGreen.ToColor3(), 0.55f), Line3D.CreateWiredBox(new[] { t1sph, t2sph, }));
        }
    }

    class TaskResult
    {
        public TimeSpan Duration { get; set; }
        public string Text { get; set; }
    }
}
