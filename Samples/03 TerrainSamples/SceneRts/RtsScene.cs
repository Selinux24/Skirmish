﻿using Engine;
using Engine.Animation;
using Engine.Audio;
using Engine.Collada;
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

namespace TerrainSamples.SceneRts
{
    using TerrainSamples.SceneRts.AI;
    using TerrainSamples.SceneRts.AI.Agents;
    using TerrainSamples.SceneRts.Controllers;
    using TerrainSamples.SceneRts.Emitters;
    using TerrainSamples.SceneStart;

    public class RtsScene : WalkableScene
    {
        private const int MaxPickingTest = 10000;
        private const int MaxGridDrawer = 10000;

        readonly string fontFamily = "Microsoft Sans Serif";

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

        private Sprite panel = null;
        private UITextArea title = null;
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
        private readonly List<Line3D> oks = new();
        private readonly List<Line3D> errs = new();

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
        private readonly Vector3 helicopterHeightOffset = Vector3.Up * 15f;
        private readonly Color4 curvesColor = Color.Red;
        private readonly Color4 pointsColor = Color.Blue;
        private readonly Color4 segmentsColor = new(Color.Cyan.ToColor3(), 0.8f);
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

        private readonly Dictionary<string, AnimationPlan> animations = new();

        private string heliEffect;
        private IAudioEffect heliEffectInstance;
        private string heliDestroyedEffect;
        private string tank1Effect;
        private string tank2Effect;
        private IAudioEffect tank1EffectInstance;
        private IAudioEffect tank2EffectInstance;
        private string tank1DestroyedEffect;
        private string tank2DestroyedEffect;
        private string tank1ShootingEffect;
        private string tank2ShootingEffect;
        private string[] impactEffects;
        private string[] damageEffects;

        private bool started = false;

        private bool gameReady = false;

        public RtsScene(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000f;
        }
        ~RtsScene()
        {
            Dispose(false);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                debugTex?.Dispose();
                debugTex = null;
            }

            base.Dispose(disposing);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            InitializeUI();
        }

        private void InitializeUI()
        {
            LoadResourcesAsync(
                InitializeUITitle(),
                InitializeUICompleted);
        }
        private async Task<TaskResult> InitializeUITitle()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var defaultFont18 = TextDrawerDescription.FromFamily(fontFamily, 18);
            var defaultFont12 = TextDrawerDescription.FromFamily(fontFamily, 12);
            var defaultFont10 = TextDrawerDescription.FromFamily(fontFamily, 10);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            stats = await AddComponentUI<UITextArea, UITextAreaDescription>("Stats", "Stats", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow });
            counters1 = await AddComponentUI<UITextArea, UITextAreaDescription>("Counters1", "Counters1", new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.GreenYellow });
            counters2 = await AddComponentUI<UITextArea, UITextAreaDescription>("Counters2", "Counters2", new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.GreenYellow });

            title.Text = "Terrain collision and trajectories test";
            stats.Text = "";
            counters1.Text = "";
            counters2.Text = "";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Back Pannel", "Back Pannel", spDesc, LayerUI - 1);

            var spbDesc = new UIProgressBarDescription()
            {
                Width = 50,
                Height = 5,
                BaseColor = Color.Red,
                ProgressColor = Color.Green,
            };

            hProgressBar = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("HelicopterProgressBar", "HelicopterProgressBar", spbDesc, LayerEffects);
            t1ProgressBar = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("Tank1ProgressBar", "Tank1ProgressBar", spbDesc, LayerEffects);
            t2ProgressBar = await AddComponentUI<UIProgressBar, UIProgressBarDescription>("Tank2ProgressBar", "Tank2ProgressBar", spbDesc, LayerEffects);

            hProgressBar.Top = 120;
            t1ProgressBar.Top = 120;
            t2ProgressBar.Top = 120;

            hProgressBar.Left = 5;
            t1ProgressBar.Left = 135;
            t2ProgressBar.Left = 270;

            hProgressBar.Visible = false;
            t1ProgressBar.Visible = false;
            t2ProgressBar.Visible = false;

            var c3DDesc = new ModelDescription()
            {
                DeferredEnabled = false,
                DepthEnabled = false,
                Content = ContentDescription.FromFile("SceneRts/resources/cursor", "cursor.json"),
            };
            cursor3D = await AddComponentCursor<Model, ModelDescription>("Cursor3D", "Cursor3D", c3DDesc);

            var c2DDesc = UICursorDescription.Default("SceneRts/resources/Cursor/target.png", 16, 16, true, Color.Red);
            cursor2D = await AddComponentCursor<UICursor, UICursorDescription>("Cursor2D", "Cursor2D", c2DDesc);
            cursor2D.BaseColor = Color.Red;
            cursor2D.Visible = false;

            sw.Stop();
            return new TaskResult()
            {
                Text = "UI",
                Duration = sw.Elapsed,
            };
        }
        private void InitializeUICompleted(LoadResourcesResult<TaskResult> res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Renderer.PostProcessingObjectsEffects.AddToneMapping(Engine.BuiltIn.PostProcess.BuiltInToneMappingTones.Uncharted2);

            UpdateLayout();

            InitializeModels();
        }

        private void InitializeModels()
        {
            List<Task> loadTasks = new()
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

            LoadResourcesAsync(
                loadTasks,
                InitializeModelsCompleted);
        }
        private async Task<TaskResult> InitializeWalker()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            await Task.Run(() =>
            {
                walkerAgentType = new Agent()
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

            #region DEBUG Shadow Map

            int width = 300;
            int height = 300;
            int smLeft = Game.Form.RenderWidth - width;
            int smTop = Game.Form.RenderHeight - height;

            var smDesc = UITextureRendererDescription.Default(smLeft, smTop, width, height);
            smDesc.DeferredEnabled = false;
            smDesc.StartsVisible = false;
            shadowMapDrawer = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("++DEBUG++ Shadow Map", "++DEBUG++ Shadow Map", smDesc);
            shadowMapDrawer.Channel = ColorChannels.Red;

            debugTex = await Game.ResourceManager.RequestResource(@"SceneRts/resources/uvtest.png");

            #endregion

            #region DEBUG Lights Volume

            lightsVolumeDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "++DEBUG++ Lights",
                "++DEBUG++ Lights",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 5000,
                    StartsVisible = false,
                });

            #endregion

            #region DEBUG Path finding Graph

            terrainGraphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "++DEBUG++ Path finding Graph",
                "++DEBUG++ Path finding Graph",
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Count = MaxGridDrawer,
                    StartsVisible = false,
                });

            #endregion

            #region DEBUG Picking test

            terrainPointDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "++DEBUG++ Picking test",
                "++DEBUG++ Picking test",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = MaxPickingTest,
                    StartsVisible = false,
                });

            #endregion

            #region DEBUG Trajectory

            curveLineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "++DEBUG++ Trajectory",
                "++DEBUG++ Trajectory",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 20000,
                    StartsVisible = false,
                });

            #endregion

            #region DEBUG Helicopter manipulator

            movingObjLineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "++DEBUG++ Helicopter manipulator",
                "++DEBUG++ Helicopter manipulator",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 1000,
                    StartsVisible = false,
                });

            #endregion

            #region DEBUG static volumes

            staticObjLineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "++DEBUG++ Static Volumes",
                "++DEBUG++ Static Volumes",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 20000,
                    StartsVisible = false,
                });

            #endregion

            #region DEBUG Ground position test

            terrainLineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "++DEBUG++ Ground position test",
                "++DEBUG++ Ground position test",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 10000,
                    StartsVisible = false,
                });

            #endregion

            sw.Stop();
            return new TaskResult()
            {
                Text = "Debug",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeParticles()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            pPlume = ParticleSystemDescription.InitializeSmokePlume("SceneRts/resources/particles", "smoke.png");
            pFire = ParticleSystemDescription.InitializeFire("SceneRts/resources/particles", "fire.png");
            pDust = ParticleSystemDescription.InitializeDust("SceneRts/resources/particles", "smoke.png");
            pProjectile = ParticleSystemDescription.InitializeProjectileTrail("SceneRts/resources/particles", "smoke.png");
            pExplosion = ParticleSystemDescription.InitializeExplosion("SceneRts/resources/particles", "fire.png");
            pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("SceneRts/resources/particles", "smoke.png");

            pManager = await AddComponentEffect<ParticleManager, ParticleManagerDescription>("ParticleManager", "ParticleManager", ParticleManagerDescription.Default());

            sw.Stop();
            return new TaskResult()
            {
                Text = "Particles",
                Duration = sw.Elapsed,
            };
        }
        private async Task<TaskResult> InitializeLensFlare()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            var lfDesc = new LensFlareDescription()
            {
                ContentPath = "SceneRts/resources/Flare",
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
            await AddComponentEffect<LensFlare, LensFlareDescription>("Flares", "Flares", lfDesc);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                TextureIndex = 0,
                Content = ContentDescription.FromFile("SceneRts/resources/Helicopter", "M24.json"),
                CullingVolumeType = CullingVolumeTypes.BoxVolume,
                StartsVisible = false,
            };
            helicopter = await AddComponentAgent<Model, ModelDescription>("Helicopter", "Helicopter", hDesc);
            helicopter.Manipulator.SetScale(0.15f);

            PrepareLights(helicopter.Lights);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                Optimize = false,
                Content = ContentDescription.FromFile("SceneRts/resources/Leopard", "Leopard.json"),
                TransformNames = new[] { "Barrel-mesh", "Turret-mesh", "Hull-mesh" },
                TransformDependences = new[] { 1, 2, -1 },
                StartsVisible = false,
            };
            tankP1 = await AddComponentAgent<Model, ModelDescription>("Tank1", "Tank1", tDesc);
            tankP2 = await AddComponentAgent<Model, ModelDescription>("Tank2", "Tank2", tDesc);

            tankP1.Manipulator.SetScale(0.2f);
            tankP2.Manipulator.SetScale(0.2f);

            var tankbbox = tankP1.GetBoundingBox(true);

            // Initialize dust generation relative positions
            tankLeftCat = new Vector3(tankbbox.Maximum.X, tankbbox.Minimum.Y, tankbbox.Maximum.Z);
            tankRightCat = new Vector3(tankbbox.Minimum.X, tankbbox.Minimum.Y, tankbbox.Maximum.Z);

            // Initialize agent
            tankAgentType = new Agent()
            {
                Name = "Tank type",
                Height = tankbbox.Height,
                Radius = tankbbox.Width * 0.5f,
                MaxClimb = tankbbox.Height * 0.1f,
            };

            PrepareLights(tankP1.Lights);
            PrepareLights(tankP2.Lights);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile("SceneRts/resources/Heliport", "Heliport.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            heliport = await AddComponentGround<Model, ModelDescription>("Heliport", "Heliport", hpDesc);

            PrepareLights(heliport.Lights);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile("SceneRts/resources/Garage", "Garage.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            garage = await AddComponentGround<Model, ModelDescription>("Garage", "Garage", gDesc);

            PrepareLights(garage.Lights);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile("SceneRts/resources/Buildings", "Building_1.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            building = await AddComponentGround<Model, ModelDescription>("Buildings", "Buildings", gDesc);

            PrepareLights(building.Lights);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 4,
                Content = ContentDescription.FromFile("SceneRts/resources/Obelisk", "Obelisk.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            obelisk = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Obelisk", "Obelisk", oDesc);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 250,
                Content = ContentDescription.FromFile("SceneRts/resources/Rocks", "boulder.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            rocks = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Rocks", "Rocks", rDesc);

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
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                Instances = 100,
                Content = ContentDescription.FromFile("SceneRts/resources/Trees", "birch_a.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            var t2Desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.DefaultTransparent,
                Instances = 100,
                Content = ContentDescription.FromFile("SceneRts/resources/Trees", "birch_b.json"),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Hull,
            };
            tree1 = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("birch_a", "birch_a", t1Desc);
            tree2 = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("birch_b", "birch_b", t2Desc);

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

            string fileName = @"SceneRts/resources/Skydom/sunset.dds";

            var skydomDesc = SkydomDescription.Sphere(fileName, Camera.FarPlaneDistance);
            await AddComponentSky<Skydom, SkydomDescription>("Skydom", "Skydom", skydomDesc);

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

            await AddComponentSky<SkyPlane, SkyPlaneDescription>("Clouds", "Clouds", new SkyPlaneDescription()
            {
                ContentPath = "SceneRts/resources/clouds",
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

            var terrainDescription = GroundDescription.FromFile("SceneRts/resources/Terrain", "two_levels.json", 1);
            terrain = await AddComponentGround<Scenery, GroundDescription>("Terrain", "Terrain", terrainDescription);

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
                ContentPath = "SceneRts/resources/Terrain/Foliage/Billboard",
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
                    Instances = GroundGardenerPatchInstances.Four,
                }
            };
            gardener = await AddComponentEffect<GroundGardener, GroundGardenerDescription>("Grass", "Grass", grDesc);

            sw.Stop();
            return new TaskResult()
            {
                Text = "Gardener",
                Duration = sw.Elapsed,
            };
        }
        private async Task InitializeModelsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            StartAudio();

            StartLights();

            agentManager = new Brain(this);

            gardener.SetWind(windDirection, windStrength);

            AudioManager.MasterVolume = 1f;
            AudioManager.Start();

            Camera.Goto(heliport.Manipulator.Position + Vector3.One * 25f);
            Camera.LookTo(0, 10, 0);

            await StartPathFinding();
        }
        private void StartAudio()
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

            AudioManager.LoadSound(forestEffect, "SceneRts/resources/Audio/Effects", "wind_birds_forest_01.wav");
            AudioManager.LoadSound(heliEffect, "SceneRts/resources/Audio/Effects", "heli.wav");
            AudioManager.LoadSound(heliDestroyedEffect, "SceneRts/resources/Audio/Effects", "explosion_helicopter_close_01.wav");
            AudioManager.LoadSound("Tank", "SceneRts/resources/Audio/Effects", "tank_engine.wav");
            AudioManager.LoadSound("TankDestroyed", "SceneRts/resources/Audio/Effects", "explosion_vehicle_small_close_01.wav");
            AudioManager.LoadSound("TankShooting", "SceneRts/resources/Audio/Effects", "machinegun-shooting.wav");
            AudioManager.LoadSound(impactEffects[0], "SceneRts/resources/Audio/Effects", "metal_grate_large_01.wav");
            AudioManager.LoadSound(impactEffects[1], "SceneRts/resources/Audio/Effects", "metal_grate_large_02.wav");
            AudioManager.LoadSound(impactEffects[2], "SceneRts/resources/Audio/Effects", "metal_grate_large_03.wav");
            AudioManager.LoadSound(impactEffects[3], "SceneRts/resources/Audio/Effects", "metal_grate_large_04.wav");
            AudioManager.LoadSound(damageEffects[0], "SceneRts/resources/Audio/Effects", "metal_pipe_large_01.wav");
            AudioManager.LoadSound(damageEffects[1], "SceneRts/resources/Audio/Effects", "metal_pipe_large_02.wav");
            AudioManager.LoadSound(damageEffects[2], "SceneRts/resources/Audio/Effects", "metal_pipe_large_03.wav");
            AudioManager.LoadSound(damageEffects[3], "SceneRts/resources/Audio/Effects", "metal_pipe_large_04.wav");

            AudioManager.AddEffectParams(
                forestEffect,
                new GameAudioEffectParameters
                {
                    SoundName = forestEffect,
                    DestroyWhenFinished = false,
                    IsLooped = true,
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
                    ReverbPreset = ReverbPresets.Forest,
                    Volume = 1f,
                });

            var forestEffectInstance = AudioManager.CreateEffectInstance(forestEffect);
            forestEffectInstance.Play();
        }
        private void StartLights()
        {
            Lights.DirectionalLights[0].Enabled = true;
            Lights.DirectionalLights[0].CastShadow = true;
            Lights.DirectionalLights[1].Enabled = true;
            Lights.DirectionalLights[2].Enabled = true;
        }
        private async Task<TaskResult> StartPathFinding()
        {
            var sw = Stopwatch.StartNew();
            sw.Restart();

            var posRnd = new Random(1);

            await StartRocks(posRnd);
            await StartTrees(posRnd);

            await Task.WhenAll(
                StartHeliport(),
                StartGarage(),
                StartBuildings(),
                StartObelisk());

            var navSettings = BuildSettings.Default;
            navSettings.Agents = new[]
            {
                walkerAgentType,
                tankAgentType,
            };
            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new PathFinderDescription(navSettings, nvInput);

            sw.Stop();

            await UpdateNavigationGraphAsync();

            return new TaskResult()
            {
                Text = "PathFinding",
                Duration = sw.Elapsed,
            };
        }
        private async Task StartRocks(Random posRnd)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < rocks.InstanceCount; i++)
                {
                    if (!GetRandomPoint(posRnd, Vector3.Zero, out var pos))
                    {
                        rocks[i].Visible = false;

                        continue;
                    }

                    if (!FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        continue;
                    }

                    float scale = i switch
                    {
                        < 5 => posRnd.NextFloat(2f, 5f),
                        < 30 => posRnd.NextFloat(0.5f, 2f),
                        _ => posRnd.NextFloat(0.1f, 0.2f)
                    };

                    rocks[i].Manipulator.SetTransform(
                        r.Position,
                        Quaternion.RotationYawPitchRoll(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi)),
                        scale);
                }
                rocks.Visible = true;
            });
        }
        private async Task StartTrees(Random posRnd)
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < tree1.InstanceCount; i++)
                {
                    if (!GetRandomPoint(posRnd, Vector3.Zero, out var pos))
                    {
                        tree1[i].Visible = false;

                        continue;
                    }

                    if (!FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        continue;
                    }

                    tree1[i].Manipulator.SetTransform(
                        r.Position,
                        Quaternion.RotationYawPitchRoll(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0),
                        posRnd.NextFloat(0.25f, 0.75f));
                }
                tree1.Visible = true;

                for (int i = 0; i < tree2.InstanceCount; i++)
                {
                    if (!GetRandomPoint(posRnd, Vector3.Zero, out var pos))
                    {
                        tree2[i].Visible = false;

                        continue;
                    }

                    if (!FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        continue;
                    }

                    tree2[i].Manipulator.SetTransform(
                        r.Position,
                        Quaternion.RotationYawPitchRoll(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0),
                        posRnd.NextFloat(0.25f, 0.75f));
                }
                tree2.Visible = true;
            });
        }
        private async Task StartHeliport()
        {
            await Task.Run(() =>
            {
                if (FindTopGroundPosition(75, 75, out PickingResult<Triangle> r))
                {
                    heliport.Manipulator.SetPosition(r.Position);
                }
                heliport.Visible = true;
                heliport.Lights.ToList().ForEach(l => l.Enabled = true);
            });
        }
        private async Task StartGarage()
        {
            await Task.Run(() =>
            {
                if (FindTopGroundPosition(-10, -40, out PickingResult<Triangle> r))
                {
                    garage.Manipulator.SetPosition(r.Position);
                    garage.Manipulator.SetRotation(MathUtil.PiOverFour * 0.5f + MathUtil.Pi, 0, 0);
                }
                garage.Visible = true;
                garage.Lights.ToList().ForEach(l => l.Enabled = true);
            });
        }
        private async Task StartBuildings()
        {
            await Task.Run(() =>
            {
                if (FindTopGroundPosition(-30, -40, out PickingResult<Triangle> r))
                {
                    building.Manipulator.SetPosition(r.Position);
                    building.Manipulator.SetRotation(MathUtil.PiOverFour * 0.5f + MathUtil.Pi, 0, 0);
                }
                building.Visible = true;
                building.Lights.ToList().ForEach(l => l.Enabled = true);
            });
        }
        private async Task StartObelisk()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < obelisk.InstanceCount; i++)
                {
                    Vector2 o = i switch
                    {
                        0 => new Vector2(1, 1),
                        1 => new Vector2(-1, 1),
                        2 => new Vector2(1, -1),
                        _ => new Vector2(-1, -1),
                    };

                    if (!FindTopGroundPosition(o.X * 50, o.Y * 50, out PickingResult<Triangle> r))
                    {
                        continue;
                    }

                    var obeliskInstance = obelisk[i];

                    obeliskInstance.Manipulator.SetPosition(r.Position);
                    obeliskInstance.Manipulator.SetScale(1.5f);
                    obeliskInstance.Manipulator.SetTransform(
                        r.Position,
                        Quaternion.RotationYawPitchRoll(MathUtil.PiOverFour, 0, 0),
                        1.5f);
                }
                obelisk.Visible = true;
            });
        }
        private void PrepareLights(IEnumerable<ISceneLight> lights)
        {
            if (!lights.Any())
            {
                return;
            }

            lights.OfType<ISceneLightPoint>().ToList().ForEach(l => l.CastShadow = false);
            lights.ToList().ForEach(l => l.Enabled = false);
            Lights.AddRange(lights);
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            stats.SetPosition(new Vector2(0, 46));
            counters1.SetPosition(new Vector2(0, 68));
            counters2.SetPosition(new Vector2(0, 90));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = counters2.Top + counters2.Height + 3;
        }

        public override void NavigationGraphUpdated()
        {
            if (started)
            {
                return;
            }

            started = true;

            Task.Run(() =>
            {
                StartHelicopter();
                StartTanks();
                StartDebug();

                BeginToggleGarageLights();

                gameReady = true;
            });
        }
        private void StartHelicopter()
        {
            var sceneryUsage = SceneObjectUsages.Ground;

            // Set position
            var ray = GetTopDownRay(heliport.Manipulator.Position, PickingHullTypes.Geometry);
            if (this.PickNearest(ray, sceneryUsage, out ScenePickingResult<Triangle> r))
            {
                helicopter.Manipulator.SetPosition(r.PickingResult.Position);
                helicopter.Manipulator.SetNormal(r.PickingResult.Primitive.Normal);
            }

            var hp = new AnimationPath();
            hp.AddLoop("roll");
            animations.Add("heli_default", new AnimationPlan(hp));

            // Register animation paths
            var ap = new AnimationPath();
            ap.AddLoop("default");
            animations.Add("default", new AnimationPlan(ap));

            // Set animation
            helicopter.AnimationController.ReplacePlan(animations["heli_default"]);
            helicopter.AnimationController.TimeDelta = 3f;
            helicopter.AnimationController.Start();

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

            helicopterAgent = new HelicopterAIAgent(agentManager, null, helicopter, hStats);

            // Register events
            helicopterAgent.Moving += Agent_Moving;
            helicopterAgent.Attacking += Agent_Attacking;
            helicopterAgent.Damaged += Agent_Damaged;
            helicopterAgent.Destroyed += Agent_Destroyed;

            // Adds agent to agent manager to team 0
            agentManager.AddAgent(0, helicopterAgent);

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
            helicopterAgent.PatrolBehavior.InitPatrollingBehavior(hCheckPoints, 5, 8);
            helicopterAgent.AttackBehavior.InitAttackingBehavior(15, 10);
            helicopterAgent.RetreatBehavior.InitRetreatingBehavior(new Vector3(75, 0, 75), 12);
            helicopterAgent.ActiveAI = true;

            //Show
            helicopter.Visible = true;
            helicopter.Lights.ToList().ForEach(l => l.Enabled = true);
        }
        private void StartTanks()
        {
            if (this.PickNearest(GetTopDownRay(-60, -60, PickingHullTypes.Geometry), SceneObjectUsages.Ground, out ScenePickingResult<Triangle> r1))
            {
                tankP1.Manipulator.SetPosition(r1.PickingResult.Position);
                tankP1.Manipulator.SetNormal(r1.PickingResult.Primitive.Normal);
            }

            if (this.PickNearest(GetTopDownRay(-70, 70, PickingHullTypes.Geometry), SceneObjectUsages.Ground, out ScenePickingResult<Triangle> r2))
            {
                tankP2.Manipulator.SetPosition(r2.PickingResult.Position);
                tankP2.Manipulator.SetNormal(r2.PickingResult.Primitive.Normal);
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
            tankP1Agent = new TankAIAgent(agentManager, tankAgentType, tankP1, tStats);
            tankP2Agent = new TankAIAgent(agentManager, tankAgentType, tankP2, tStats);

            tankP1Agent.SceneObject.Name = "Tank1";
            tankP2Agent.SceneObject.Name = "Tank2";

            // Register events
            tankP1Agent.Moving += Agent_Moving;
            tankP1Agent.Attacking += Agent_Attacking;
            tankP1Agent.Damaged += Agent_Damaged;
            tankP1Agent.Destroyed += Agent_Destroyed;
            agentManager.AddAgent(1, tankP1Agent);

            tankP2Agent.Moving += Agent_Moving;
            tankP2Agent.Attacking += Agent_Attacking;
            tankP2Agent.Damaged += Agent_Damaged;
            tankP2Agent.Destroyed += Agent_Destroyed;
            agentManager.AddAgent(1, tankP2Agent);

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
                if (FindNearestGroundPosition(t1CheckPoints[i], out PickingResult<Triangle> r))
                {
                    t1CheckPoints[i] = r.Position;
                }
            }

            for (int i = 0; i < t2CheckPoints.Length; i++)
            {
                if (FindNearestGroundPosition(t2CheckPoints[i], out PickingResult<Triangle> r))
                {
                    t2CheckPoints[i] = r.Position;
                }
            }

            // Initialize behaviors
            tankP1Agent.PatrolBehavior.InitPatrollingBehavior(t1CheckPoints, 10, 5);
            tankP1Agent.AttackBehavior.InitAttackingBehavior(7, 10);
            tankP1Agent.RetreatBehavior.InitRetreatingBehavior(new Vector3(-10, 0, -40), 10);
            tankP1Agent.ActiveAI = true;

            tankP2Agent.PatrolBehavior.InitPatrollingBehavior(t2CheckPoints, 10, 5);
            tankP2Agent.AttackBehavior.InitAttackingBehavior(7, 10);
            tankP2Agent.RetreatBehavior.InitRetreatingBehavior(new Vector3(-10, 0, -40), 10);
            tankP2Agent.ActiveAI = true;

            //Show
            tankP1.Visible = true;
            tankP2.Visible = true;
            tankP1.Lights.ToList().ForEach(l => l.Enabled = true);
            tankP2.Lights.ToList().ForEach(l => l.Enabled = true);
        }
        private void StartDebug()
        {
            // Ground position test
            var bbox = terrain.GetBoundingBox();

            float sep = 2.1f;
            for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += sep)
            {
                for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += sep)
                {
                    if (FindTopGroundPosition(x, z, out PickingResult<Triangle> r))
                    {
                        oks.Add(new Line3D(r.Position, r.Position + Vector3.Up));
                    }
                    else
                    {
                        errs.Add(new Line3D(x, 10, z, x, -10, z));
                    }
                }
            }

            if (oks.Count > 0)
            {
                terrainLineDrawer.AddPrimitives(Color.Green, oks.ToArray());
            }
            if (errs.Count > 0)
            {
                terrainLineDrawer.AddPrimitives(Color.Red, errs.ToArray());
            }

            // Axis
            curveLineDrawer.SetPrimitives(wAxisColor, Line3D.CreateAxis(Matrix.Identity, 20f));
            curveLineDrawer.Visible = false;
        }
        private void BeginToggleGarageLights()
        {
            var light = garage.Lights.FirstOrDefault();
            if (light == null)
            {
                return;
            }

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(Helper.RandomGenerator.Next(500, 3000));

                    light.Enabled = !light.Enabled;
                }
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<StartScene>();
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

            var pickingRay = GetPickingRay();

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

                agentManager.Update(gameTime);
            }
        }
        private void UpdateInputPlayer()
        {
            if (Game.Input.KeyJustReleased(Keys.Z))
            {
                walkMode = !walkMode;

                if (walkMode)
                {
                    Camera.Mode = CameraModes.FirstPerson;
                    Camera.MovementDelta = walkerVelocity;
                    Camera.SlowMovementDelta = walkerVelocity * 0.05f;
                    cursor3D.Visible = false;
                    cursor2D.Visible = true;

                    var pos = heliport.Manipulator.Position;
                    if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                    {
                        var pPos = r.Position;
                        pPos.Y += walkerAgentType.Height;
                        Camera.SetPosition(pPos);
                    }
                }
                else
                {
                    Camera.Mode = CameraModes.Free;
                    Camera.MovementDelta = 20.5f;
                    Camera.SlowMovementDelta = 1f;
                    cursor3D.Visible = true;
                    cursor2D.Visible = false;
                }
            }
        }
        private void UpdateInputCamera(GameTime gameTime, PickingRay pickingRay)
        {
            if (walkMode)
            {
                UpdateInputWalker(gameTime);
            }
            else
            {
                UpdateInputFree(gameTime, pickingRay);
            }
        }
        private void UpdateInputWalker(GameTime gameTime)
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

            if (Walk(walkerAgentType, Camera.Position, Camera.GetNextPosition(), true, out var walkerPos))
            {
                Camera.Goto(walkerPos);
            }
            else
            {
                Camera.Goto(Camera.Position);
            }
        }
        private void UpdateInputFree(GameTime gameTime, PickingRay pickingRay)
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

            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                followTarget = null;
                follow = false;
            }

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                bool picked = this.PickNearest<Triangle>(pickingRay, SceneObjectUsages.Agent, out var agent);
                if (picked)
                {
                    followTarget = agent.SceneObject;
                    follow = true;
                }
            }

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

            if (follow &&
                followTarget is IRayPickable<Triangle> pickable &&
                followTarget is ITransformable3D transform)
            {
                var sph = pickable.GetBoundingSphere();
                Camera.LookTo(sph.Center);
                Camera.Goto(sph.Center + (transform.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
            }
        }
        private void UpdateInputHelicopterTexture()
        {
            if (Game.Input.KeyJustReleased(Keys.Right))
            {
                helicopter.TextureIndex++;
                if (helicopter.TextureIndex > 2) helicopter.TextureIndex = 2;
            }
            if (Game.Input.KeyJustReleased(Keys.Left))
            {
                helicopter.TextureIndex--;
                if (helicopter.TextureIndex < 0) helicopter.TextureIndex = 0;
            }
        }
        private void UpdateInputDebug(PickingRay pickingRay)
        {
            UpdateInputDebugShadows();

            UpdateInputDebugObjectsVisibility();

            if (!terrainGraphDrawer.Visible)
            {
                return;
            }

            if (Game.Input.MouseButtonPressed(MouseButtons.Left))
            {
                terrainPointDrawer.Clear();

                if (this.PickNearest(pickingRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r))
                {
                    DEBUGPickingPosition(r.PickingResult.Position);
                }
            }
        }
        private void UpdateInputDebugShadows()
        {
            if (Game.Input.KeyJustReleased(Keys.C))
            {
                Lights.KeyLight.CastShadow = !Lights.KeyLight.CastShadow;
            }
            if (Game.Input.KeyJustReleased(Keys.Up) && !Game.Input.ShiftPressed)
            {
                shadowResult = SceneRendererResults.ShadowMapDirectional;
            }
            if (Game.Input.KeyJustReleased(Keys.Down) && !Game.Input.ShiftPressed)
            {
                shadowResult = SceneRendererResults.ShadowMapDirectional;
            }
        }
        private void UpdateInputDebugObjectsVisibility()
        {
            if (Game.Input.KeyJustReleased(Keys.D1))
            {
                walkMode = !walkMode;
                DEBUGUpdateGraphDrawer();
            }
            if (Game.Input.KeyJustReleased(Keys.D2))
            {
                terrain.Visible = !terrain.Visible;
            }
            if (Game.Input.KeyJustReleased(Keys.D3))
            {
                gardener.Visible = !gardener.Visible;
            }
            if (Game.Input.KeyJustReleased(Keys.D4))
            {
                tree1.Visible = !tree1.Visible;
                tree2.Visible = !tree2.Visible;
            }
            if (Game.Input.KeyJustReleased(Keys.D5))
            {
                rocks.Visible = !rocks.Visible;
            }
            if (Game.Input.KeyJustReleased(Keys.D6))
            {
                garage.Visible = !garage.Visible;
                heliport.Visible = !heliport.Visible;
                building.Visible = !building.Visible;
                obelisk.Visible = !obelisk.Visible;
            }
        }
        private void UpdateInputDrawers()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                terrainLineDrawer.Visible = !terrainLineDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                terrainGraphDrawer.Visible = !terrainGraphDrawer.Visible;
                DEBUGUpdateGraphDrawer();
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                terrainPointDrawer.Visible = !terrainPointDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                curveLineDrawer.Visible = !curveLineDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                movingObjLineDrawer.Visible = !movingObjLineDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F7))
            {
                shadowMapDrawer.Visible = !shadowMapDrawer.Visible;
                shadowResult = SceneRendererResults.ShadowMapDirectional;
            }

            if (Game.Input.KeyJustReleased(Keys.F8))
            {
                useDebugTex = !useDebugTex;
            }

            if (Game.Input.KeyJustReleased(Keys.F9))
            {
                staticObjLineDrawer.Visible = !staticObjLineDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F11))
            {
                if (!drawDrawVolumes && !drawCullVolumes)
                {
                    drawDrawVolumes = true;
                    drawCullVolumes = false;
                }
                else if (drawDrawVolumes && !drawCullVolumes)
                {
                    drawDrawVolumes = false;
                    drawCullVolumes = true;
                }
                else if (!drawDrawVolumes)
                {
                    drawDrawVolumes = false;
                    drawCullVolumes = false;
                }
            }
        }
        private void UpdateInputGraph()
        {
            if (Game.Input.KeyJustReleased(Keys.Add))
            {
                graphIndex++;
                DEBUGUpdateGraphDrawer();
            }
            if (Game.Input.KeyJustReleased(Keys.Subtract))
            {
                graphIndex--;
                DEBUGUpdateGraphDrawer();
            }
        }
        private void UpdateCursor(PickingRay pickingRay)
        {
            if (!walkMode && terrain.PickNearest(pickingRay, out PickingResult<Triangle> r))
            {
                cursor3D.Manipulator.SetPosition(r.Position);
            }
        }
        private void UpdateTanks(PickingRay pickingRay)
        {
            if (Game.Input.MouseButtonPressed(MouseButtons.Left))
            {
                //Draw path before set it to the agent
                DrawTankPath(pickingRay);
            }

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left) && !Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                //Calc path and set it to the agent
                UpdateTankPath(pickingRay);
            }

            if (Game.Input.ShiftPressed)
            {
                if (Game.Input.KeyJustReleased(Keys.D1))
                {
                    tankP1Agent.ActiveAI = !tankP1Agent.ActiveAI;
                }
                if (Game.Input.KeyJustReleased(Keys.D2))
                {
                    tankP2Agent.ActiveAI = !tankP2Agent.ActiveAI;
                }
            }

            SetStatsScreenPosition(tankP1Agent, 4, t1ProgressBar);
            SetStatsScreenPosition(tankP2Agent, 4, t2ProgressBar);

            DEBUGDrawTankPath(tankP1Agent, Color.Yellow, Color.GreenYellow);
            DEBUGDrawTankPath(tankP2Agent, Color.Firebrick, Color.Coral);
        }
        private void UpdateHelicopter()
        {
            if (Game.Input.KeyJustReleased(Keys.Home))
            {
                Curve3D curve = GenerateHelicopterPath();
                helicopter.AnimationController.ReplacePlan(animations["heli_default"]);
                DEBUGDrawHelicopterPath(curve);
            }

            SetStatsScreenPosition(helicopterAgent, 4, hProgressBar);
        }
        private void UpdateDebug()
        {
            if (drawDrawVolumes)
            {
                DEBUGDrawLightMarks();
            }

            if (drawCullVolumes)
            {
                DEBUGDrawLightVolumes();
            }

            if (curveLineDrawer.Visible)
            {
                Matrix rot = Matrix.RotationQuaternion(helicopter.Manipulator.Rotation) * Matrix.Translation(helicopter.Manipulator.Position);
                curveLineDrawer.SetPrimitives(hAxisColor, Line3D.CreateAxis(rot, 5f));
            }

            if (staticObjLineDrawer.Visible && objNotSet)
            {
                DEBUGDrawStaticVolumes();

                objNotSet = false;
            }

            if (movingObjLineDrawer.Visible)
            {
                DEBUGDrawMovingVolumes();
            }

            var tp = helicopterAgent.Target;
            if (tp.HasValue)
            {
                DEBUGPickingPosition(tp.Value);
            }
        }
        private void SetStatsScreenPosition(AIAgent agent, float height, UIProgressBar pb)
        {
            var screenPosition = GetScreenCoordinates(agent.Manipulator.Position, out bool centerInside);
            var top = GetScreenCoordinates(agent.Manipulator.Position + new Vector3(0, height, 0), out bool topInside);

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
        private void DrawTankPath(PickingRay pickingRay)
        {
            var picked = this.PickNearest(pickingRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r);
            if (picked)
            {
                var t1Position = tankP1.Manipulator.Position;

                var result = FindPath(tankAgentType, t1Position, r.PickingResult.Position);
                if (result != null)
                {
                    DEBUGDrawPath(result.Positions, Color.Red);
                }
            }
        }
        private void UpdateTankPath(PickingRay pickingRay)
        {
            var picked = this.PickNearest(pickingRay, SceneObjectUsages.None, out ScenePickingResult<Triangle> r);
            if (picked)
            {
                Task.Run(async () =>
                {
                    var path = await FindPathAsync(tankAgentType, tankP1.Manipulator.Position, r.PickingResult.Position, true);
                    if (path != null)
                    {
                        path.RefinePath(0.25f);

                        tankP1Agent.Clear();
                        tankP1Agent.FollowPath(path, 10);
                    }
                });
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!gameReady)
            {
                return;
            }

            shadowMapDrawer.Texture = useDebugTex ? debugTex : Renderer.GetResource(shadowResult);

            #region Texts

            stats.Text = Game.RuntimeText;

            var counters = FrameCounters.GetFrameCounters(-1);

            string txt1 = string.Format(
                "Buffers active: {0} {1} Kbs, reads: {2}, writes: {3}; {4} - Result: {5}; Primitives: {6}",
                FrameCounters.Buffers,
                FrameCounters.BufferBytes / 1024,
                counters?.BufferReads,
                counters?.BufferWrites,
                GetRenderMode(),
                shadowResult,
                counters?.PrimitivesPerFrame);
            counters1.Text = txt1;

            string txt2 = string.Format(
                "IA Input Layouts: {0}, Primitives: {1}, VB: {2}, IB: {3}, Terrain Patches: {4}; T1.{5}  /  T2.{6}  /  H.{7}",
                counters?.IAInputLayoutSets,
                counters?.IAPrimitiveTopologySets,
                counters?.IAVertexBuffersSets,
                counters?.IAIndexBufferSets,
                terrain.VisiblePatchesCount,
                tankP1Agent,
                tankP2Agent,
                helicopterAgent);
            counters2.Text = txt2;

            hProgressBar.ProgressValue = 1f - helicopterAgent?.Stats.Damage ?? 0;
            t1ProgressBar.ProgressValue = 1f - tankP1Agent?.Stats.Damage ?? 0;
            t2ProgressBar.ProgressValue = 1f - tankP2Agent?.Stats.Damage ?? 0;

            #endregion
        }

        private Curve3D GenerateHelicopterPath()
        {
            var curve = new Curve3D
            {
                PreLoop = CurveLoopType.Constant,
                PostLoop = CurveLoopType.Constant
            };

            Vector3[] cPoints = new Vector3[15];

            if (helicopterController != null && helicopterController.HasPath)
            {
                for (int i = 0; i < cPoints.Length - 2; i++)
                {
                    if (GetRandomPoint(Helper.RandomGenerator, helicopterHeightOffset, out var p))
                    {
                        cPoints[i] = p;
                    }
                }
            }
            else
            {
                cPoints[0] = helicopter.Manipulator.Position;
                cPoints[1] = helicopter.Manipulator.Position + (Vector3.Up * 5f) + (helicopter.Manipulator.Forward * 10f);

                for (int i = 2; i < cPoints.Length - 2; i++)
                {
                    if (GetRandomPoint(Helper.RandomGenerator, helicopterHeightOffset, out var p))
                    {
                        cPoints[i] = p;
                    }
                }
            }

            var hPos = heliport.Manipulator.Position;
            if (FindTopGroundPosition(hPos.X, hPos.Z, out PickingResult<Triangle> r))
            {
                cPoints[^2] = r.Position + helicopterHeightOffset;
                cPoints[^1] = r.Position;
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
            //Start sounds
            if (e.Active == helicopterAgent && heliEffectInstance == null)
            {
                heliEffectInstance = AudioManager.CreateEffectInstance(heliEffect, helicopter, Camera);
                heliEffectInstance.Play();
            }

            if (e.Active == tankP1Agent && tank1EffectInstance == null)
            {
                tank1EffectInstance = AudioManager.CreateEffectInstance(tank1Effect, tankP1, Camera);
                tank1EffectInstance?.Play();
            }

            if (e.Active == tankP2Agent && tank2EffectInstance == null)
            {
                tank2EffectInstance = AudioManager.CreateEffectInstance(tank2Effect, tankP2, Camera);
                tank2EffectInstance?.Play();
            }

            if (Helper.RandomGenerator.NextFloat(0, 1) > 0.8f && (e.Active == tankP1Agent || e.Active == tankP2Agent))
            {
                AddDustSystem(e.Active, tankLeftCat);
                AddDustSystem(e.Active, tankRightCat);
            }
        }
        private void Agent_Attacking(object sender, BehaviorEventArgs e)
        {
            int index = Helper.RandomGenerator.Next(0, 4);
            index %= 3;
            AudioManager.CreateEffectInstance(impactEffects[index], e.Passive.Manipulator, Camera)?.Play();

            AddProjectileTrailSystem(e.Active, e.Passive, 50f);
        }
        private void Agent_Damaged(object sender, BehaviorEventArgs e)
        {
            if (e.Active == tankP1Agent)
            {
                AudioManager.CreateEffectInstance(tank1ShootingEffect, e.Active.Manipulator, Camera)?.Play();
            }
            if (e.Active == tankP2Agent)
            {
                AudioManager.CreateEffectInstance(tank2ShootingEffect, e.Active.Manipulator, Camera)?.Play();
            }

            int index = Helper.RandomGenerator.Next(0, 4);
            index %= 3;
            AudioManager.CreateEffectInstance(damageEffects[index], e.Passive.Manipulator, Camera)?.Play();

            AddExplosionSystem(e.Passive);
            AddExplosionSystem(e.Passive);
            AddSmokeSystem(e.Passive, false);
        }
        private void Agent_Destroyed(object sender, BehaviorEventArgs e)
        {
            if (e.Passive == helicopterAgent)
            {
                heliEffectInstance?.Stop();

                AudioManager.CreateEffectInstance(heliDestroyedEffect, e.Passive.Manipulator, Camera)?.Play();

                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
                AddExplosionSystem(e.Passive, Helper.RandomGenerator.NextVector3(Vector3.One * -1f, Vector3.One));
            }
            else
            {
                if (e.Passive == tankP1Agent)
                {
                    tank1EffectInstance?.Stop();
                    AudioManager.CreateEffectInstance(tank1DestroyedEffect, e.Passive.Manipulator, Camera)?.Play();
                }
                if (e.Passive == tankP2Agent)
                {
                    tank2EffectInstance?.Stop();
                    AudioManager.CreateEffectInstance(tank2DestroyedEffect, e.Passive.Manipulator, Camera)?.Play();
                }

                AddExplosionSystem(e.Passive);
                AddExplosionSystem(e.Passive);
                AddExplosionSystem(e.Passive);
                AddExplosionSystem(e.Passive);
                AddExplosionSystem(e.Passive);
                AddExplosionSystem(e.Passive);
                AddSmokePlumeSystem(e.Passive, true);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pExplosion, emitter1);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pExplosion, emitter1);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pSmokeExplosion, emitter2);
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

                plumeFire = pManager.GetParticleSystem("plumeFire");
                plumeSmoke = pManager.GetParticleSystem("plumeSmoke");
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

                _ = pManager.AddParticleSystem(plumeFireSystemName, ParticleSystemTypes.CPU, pFire, emitter1);
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

                _ = pManager.AddParticleSystem(plumeSmokeSystemName, ParticleSystemTypes.CPU, pPlume, emitter2);
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
                smoke = pManager.GetParticleSystem(smokeSystemName);
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

                _ = pManager.AddParticleSystem(smokeSystemName, ParticleSystemTypes.CPU, pPlume, emitter);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDust, emitter);
        }
        private void AddProjectileTrailSystem(AIAgent agent, AIAgent target, float speed)
        {
            var targetDelta = Helper.RandomGenerator.NextVector3(-Vector3.One, Vector3.One);

            var emitter = new LinealEmitter(agent.Manipulator.Position, target.Manipulator.Position + targetDelta, speed)
            {
                EmissionRate = 0.0001f,
                MaximumDistance = 100f,
            };

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pProjectile, emitter);
        }

        private void DEBUGPickingPosition(Vector3 position)
        {
            if (!terrainPointDrawer.Visible)
            {
                return;
            }

            if (FindAllGroundPosition<Triangle>(position.X, position.Z, out var results))
            {
                var positions = results.Select(r => r.Position).ToArray();
                var triangles = results.Select(r => r.Primitive).ToArray();

                terrainPointDrawer.SetPrimitives(Color.Magenta, Line3D.CreateCross(positions, 1f));
                terrainPointDrawer.SetPrimitives(Color.DarkCyan, Line3D.CreateWiredTriangle(triangles));
                if (positions.Length > 1)
                {
                    terrainPointDrawer.SetPrimitives(Color.Cyan, new Line3D(positions[0], positions[^1]));
                }
            }
        }
        private void DEBUGDrawHelicopterPath(Curve3D curve)
        {
            if (!curveLineDrawer.Visible)
            {
                return;
            }

            List<Vector3> path = new();

            float pass = curve.Length / 500f;

            for (float i = 0; i <= curve.Length; i += pass)
            {
                Vector3 pos = curve.GetPosition(i);

                path.Add(pos);
            }

            curveLineDrawer.SetPrimitives(curvesColor, Line3D.CreateLineList(path.ToArray()));
            curveLineDrawer.SetPrimitives(pointsColor, Line3D.CreateCross(curve.Points, 0.5f));
            curveLineDrawer.SetPrimitives(segmentsColor, Line3D.CreateLineList(curve.Points));
        }
        private void DEBUGDrawPath(IEnumerable<Vector3> path, Color pathColor)
        {
            if (!terrainPointDrawer.Visible)
            {
                return;
            }

            terrainPointDrawer.Clear(pathColor);

            if (path?.Any() == true)
            {
                int count = Math.Min(path.Count(), MaxPickingTest);

                Line3D[] lines = new Line3D[count];

                for (int i = 1; i < count; i++)
                {
                    lines[i] = new Line3D(path.ElementAt(i - 1), path.ElementAt(i));
                }

                terrainPointDrawer.SetPrimitives(pathColor, lines);
            }
        }
        private void DEBUGDrawArrow(Vector3 from, Vector3 direction, Color arrowColor)
        {
            terrainPointDrawer.Clear(arrowColor);
            var arrow = Line3D.CreateArrow(from, from + direction, 10f);
            terrainPointDrawer.SetPrimitives(arrowColor, arrow);
        }
        private void DEBUGDrawTankPath(TankAIAgent tank, Color pathColor, Color arrowColor)
        {
            if (!terrainPointDrawer.Visible)
            {
                return;
            }

            var path = tank.Controller.SamplePath().ToArray();

            DEBUGDrawPath(path, pathColor);

            Vector3 from = tank.Manipulator.Position;
            Vector3 direction = tank.Manipulator.Forward * 10f;

            DEBUGDrawArrow(from, direction, arrowColor);
        }
        private void DEBUGUpdateGraphDrawer()
        {
            if (!terrainGraphDrawer.Visible)
            {
                return;
            }

            var agent = walkMode ? walkerAgentType : tankAgentType;

            var nodes = GetNodes(agent).OfType<GraphNode>();
            if (!nodes.Any())
            {
                graphIndex = -1;

                return;
            }

            if (graphIndex <= -1)
            {
                graphIndex = -1;

                terrainGraphDrawer.Clear();

                foreach (var node in nodes)
                {
                    terrainGraphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
            else
            {
                if (graphIndex >= nodes.Count())
                {
                    graphIndex = nodes.Count() - 1;
                }

                if (graphIndex < nodes.Count())
                {
                    terrainGraphDrawer.Clear();

                    var node = nodes.ToArray()[graphIndex];

                    terrainGraphDrawer.SetPrimitives(node.Color, node.Triangles);
                }
            }
        }
        private void DEBUGDrawLightVolumes()
        {
            if (!lightsVolumeDrawer.Visible)
            {
                return;
            }

            lightsVolumeDrawer.Clear();

            foreach (var spot in Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                lightsVolumeDrawer.AddPrimitives(new Color4(spot.DiffuseColor, 0.15f), lines);
            }

            foreach (var point in Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                lightsVolumeDrawer.AddPrimitives(new Color4(point.DiffuseColor, 0.15f), lines);
            }

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawLightMarks()
        {
            if (!lightsVolumeDrawer.Visible)
            {
                return;
            }

            lightsVolumeDrawer.Clear();

            var spheres =
                Lights.SpotLights.Select(l => l.BoundingSphere)
                .Concat(Lights.PointLights.Select(l => l.BoundingSphere));

            var g = GeometryUtil.CreateSpheres(Topology.LineList, spheres, 10, 10);

            lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), Line3D.CreateFromVertices(g));

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawStaticVolumes()
        {
            if (!staticObjLineDrawer.Visible)
            {
                return;
            }

            var boxes = new List<BoundingBox>
            {
                heliport.GetBoundingBox(),
                garage.GetBoundingBox()
            };
            boxes.AddRange(obelisk.GetInstances().Select(i => i.GetBoundingBox()));
            boxes.AddRange(rocks.GetInstances().Select(i => i.GetBoundingBox()));

            List<Triangle> tris = new();
            tris.AddRange(tree1.GetInstances().SelectMany(i => i.GetGeometry(GeometryTypes.PathFinding)));
            tris.AddRange(tree2.GetInstances().SelectMany(i => i.GetGeometry(GeometryTypes.PathFinding)));

            List<Line3D> lines = new();

            lines.AddRange(Line3D.CreateFromVertices(GeometryUtil.CreateBoxes(Topology.LineList, boxes)));
            lines.AddRange(Line3D.CreateWiredTriangle(tris));

            staticObjLineDrawer.SetPrimitives(objColor, lines.ToArray());
        }
        private void DEBUGDrawMovingVolumes()
        {
            if (!movingObjLineDrawer.Visible)
            {
                return;
            }

            movingObjLineDrawer.Clear();

            var hsph = helicopter.GetBoundingSphere();
            var t1sph = tankP1.GetBoundingSphere();
            var t2sph = tankP2.GetBoundingSphere();
            movingObjLineDrawer.SetPrimitives(new Color4(Color.White.ToColor3(), 0.55f), Line3D.CreateFromVertices(GeometryUtil.CreateSpheres(Topology.LineList, new[] { hsph, t1sph, t2sph }, 50, 20)));

            var hbox = helicopter.GetOrientedBoundingBox();
            var t1box = tankP1.GetOrientedBoundingBox();
            var t2box = tankP2.GetOrientedBoundingBox();
            movingObjLineDrawer.SetPrimitives(new Color4(Color.YellowGreen.ToColor3(), 0.55f), Line3D.CreateFromVertices(GeometryUtil.CreateBoxes(Topology.LineList, new[] { hbox, t1box, t2box, })));
        }
    }
}
