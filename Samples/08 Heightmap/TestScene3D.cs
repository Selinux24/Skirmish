using Engine;
using Engine.Animation;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
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
        private const int layerHUD = 50;
        private const int layerCursor = 100;

        private float time = 0.23f;

        private bool playerFlying = true;
        private SceneLightSpot lantern = null;
        private bool lanternFixed = false;

        private readonly Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private float windNextStrength = 1f;
        private readonly float windStep = 0.001f;
        private float windDuration = 0;

        private UIPanel fadePanel;
        private UITextArea stats = null;
        private UITextArea help = null;
        private UITextArea help2 = null;

        private SkyScattering skydom = null;
        private Terrain terrain = null;
        private GroundGardener gardener = null;
        private GroundGardener gardener2 = null;
        private PrimitiveListDrawer<Triangle> bboxesTriDrawer = null;
        private PrimitiveListDrawer<Line3D> bboxesDrawer = null;
        private PrimitiveListDrawer<Line3D> linesDrawer = null;
        private const float gardenerAreaSize = 512;
        private readonly BoundingBox? gardenerArea = new BoundingBox(new Vector3(-gardenerAreaSize * 2, -gardenerAreaSize, -gardenerAreaSize), new Vector3(0, gardenerAreaSize, gardenerAreaSize));
        private readonly BoundingBox? gardenerArea2 = new BoundingBox(new Vector3(0, -gardenerAreaSize, -gardenerAreaSize), new Vector3(gardenerAreaSize * 2, gardenerAreaSize, gardenerAreaSize));

        private ModelInstanced torchs = null;
        private SceneLightSpot spotLight1 = null;
        private SceneLightSpot spotLight2 = null;

        private ParticleManager pManager = null;
        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private float nextDust = 0;
        private int nextDustHeli = 0;
        private readonly float dustTime = 0.33f;

        private ModelInstanced rocks = null;
        private ModelInstanced trees = null;
        private ModelInstanced trees2 = null;

        private Model soldier = null;
        private PrimitiveListDrawer<Triangle> soldierTris = null;
        private PrimitiveListDrawer<Line3D> soldierLines = null;
        private bool showSoldierDEBUG = false;

        private ModelInstanced troops = null;

        private ModelInstanced helicopterI = null;
        private ModelInstanced bradleyI = null;
        private ModelInstanced buildings = null;
        private Model watchTower = null;
        private ModelInstanced containers = null;

        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private bool updatingNodes = false;

        private readonly Agent agent = new Agent()
        {
            Name = "Soldier",
            MaxSlope = 45,
        };

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private UITextureRenderer bufferDrawer = null;

        private bool uiReady = false;
        private bool gameReady = false;

        private bool udaptingGraph = false;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            Camera.Position = new Vector3(10000, 10000, 10000);
            Camera.Interest = new Vector3(10001, 10000, 10000);

            await InitializeUI();
        }

        private async Task InitializeUI()
        {
            await LoadResourcesAsync(
                InitializeUIAssets(),
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    uiReady = true;

                    fadePanel.BaseColor = Color.Black;
                    fadePanel.Visible = true;

                    await InitializeGameAssets();
                });
        }
        private async Task<double> InitializeUIAssets()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            #region Cursor

            var cursorDesc = new UICursorDescription()
            {
                Name = "Cursor",
                Textures = new[] { "target.png" },
                BaseColor = Color.Red,
                Width = 20,
                Height = 20,
            };

            await this.AddComponentUICursor(cursorDesc, layerCursor);

            #endregion

            #region Fade panel

            fadePanel = await this.AddComponentUIPanel(UIPanelDescription.Screen(this, Color4.Black * 0.3333f), layerHUD + 1);
            fadePanel.Visible = false;

            #endregion

            #region Texts

            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);
            stats = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 11, Color.Yellow) }, layerHUD);
            help = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 11, Color.Yellow) }, layerHUD);
            help2 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 11, Color.Orange) }, layerHUD);

            title.Text = "Heightmap Terrain test";
            stats.Text = "";
            help.Text = "";
            help2.Text = "";

            title.SetPosition(Vector2.Zero);
            stats.SetPosition(new Vector2(5, title.Top + title.Height + 3));
            help.SetPosition(new Vector2(5, stats.Top + stats.Height + 3));
            help2.SetPosition(new Vector2(5, help.Top + help.Height + 3));

            var spDesc = new SpriteDescription()
            {
                Name = "Background",
                Width = Game.Form.RenderWidth,
                Height = help2.Top + help2.Height + 3,
                BaseColor = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            #endregion

            #region Debug

            int width = (int)(Game.Form.RenderWidth * 0.33f);
            int height = (int)(Game.Form.RenderHeight * 0.33f);
            int smLeft = Game.Form.RenderWidth - width;
            int smTop = Game.Form.RenderHeight - height;

            var desc = new UITextureRendererDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = UITextureRendererChannels.NoAlpha,
            };
            bufferDrawer = await this.AddComponentUITextureRenderer(desc, layerEffects);
            bufferDrawer.Visible = false;

            #endregion

            sw.Stop();
            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }

        private async Task InitializeGameAssets()
        {
            var loadTasks = new[]
            {
                InitializeRocks(),
                InitializeTrees(),
                InitializeTrees2(),
                InitializeSoldier(),
                InitializeTroops(),
                InitializeM24(),
                InitializeBradley(),
                InitializeBuildings(),
                InitializeWatchTower(),
                InitializeContainers(),
                InitializeTorchs(),
                InitializeTerrain(),
                InitializeLensFlare(),
                InitializeSkydom(),
                InitializeClouds(),
                InitializeParticles(),
                InitializeDebugAssets(),
            };

            await LoadResourcesAsync(
                loadTasks,
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    skydom.RayleighScattering *= 0.8f;
                    skydom.MieScattering *= 0.1f;

                    Environment.TimeOfDay.BeginAnimation(8, 55, 00);

                    Lights.BaseFogColor = new Color((byte)95, (byte)147, (byte)233) * 0.5f;
                    ToggleFog();

                    Camera.NearPlaneDistance = near;
                    Camera.FarPlaneDistance = far;
                    Camera.Position = new Vector3(24, 12, 14);
                    Camera.Interest = new Vector3(0, 10, 0);
                    Camera.MovementDelta = 45f;
                    Camera.SlowMovementDelta = 20f;

                    await InitializeTerrainObjects();
                });
        }
        private async Task<double> InitializeRocks()
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
                    ContentFolder = @"Resources/Rocks",
                    ModelContentFilename = @"boulder.xml",
                }
            };
            rocks = await this.AddComponentModelInstanced(rDesc, SceneObjectUsages.None, layerObjects);
            rocks.Visible = false;
            AttachToGround(rocks, false);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeTrees()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();
            var treeDesc = new ModelInstancedDescription()
            {
                Name = "Trees",
                CastShadow = true,
                Instances = 200,
                BlendMode = BlendModes.DefaultTransparent,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees",
                    ModelContentFilename = @"tree.xml",
                }
            };
            trees = await this.AddComponentModelInstanced(treeDesc, SceneObjectUsages.None, layerTerrain);
            trees.Visible = false;
            AttachToGround(trees, false);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeTrees2()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();
            var tree2Desc = new ModelInstancedDescription()
            {
                Name = "Trees2",
                CastShadow = true,
                Instances = 200,
                BlendMode = BlendModes.DefaultTransparent,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees2",
                    ModelContentFilename = @"tree.xml",
                }
            };
            trees2 = await this.AddComponentModelInstanced(tree2Desc, SceneObjectUsages.None, layerTerrain);
            trees2.Visible = false;
            AttachToGround(trees2, false);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeSoldier()
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
            soldier = await this.AddComponentModel(sDesc, SceneObjectUsages.Agent, layerObjects);
            soldier.Visible = false;
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeTroops()
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
            troops = await this.AddComponentModelInstanced(tDesc, SceneObjectUsages.Agent, layerObjects);
            troops.Visible = false;
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeM24()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var mDesc = new ModelInstancedDescription()
            {
                Name = "M24",
                CastShadow = true,
                Instances = 3,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/m24",
                    ModelContentFilename = @"m24.xml",
                },
            };
            helicopterI = await this.AddComponentModelInstanced(mDesc, SceneObjectUsages.None, layerObjects);
            helicopterI.Visible = false;
            AttachToGround(helicopterI, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeBradley()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var mDesc = new ModelInstancedDescription()
            {
                Name = "Bradley",
                CastShadow = true,
                Instances = 5,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Bradley",
                    ModelContentFilename = @"Bradley.xml",
                }
            };
            bradleyI = await this.AddComponentModelInstanced(mDesc, SceneObjectUsages.None, layerObjects);
            bradleyI.Visible = false;
            AttachToGround(bradleyI, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeBuildings()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var mDesc = new ModelInstancedDescription()
            {
                Name = "Affgan buildings",
                CastShadow = true,
                Instances = 5,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/buildings",
                    ModelContentFilename = @"Affgan1.xml",
                }
            };
            buildings = await this.AddComponentModelInstanced(mDesc, SceneObjectUsages.None, layerObjects);
            buildings.Visible = false;
            AttachToGround(buildings, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeWatchTower()
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
            watchTower = await this.AddComponentModel(mDesc, SceneObjectUsages.None, layerObjects);
            watchTower.Visible = false;
            AttachToGround(watchTower, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeContainers()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var desc = new ModelInstancedDescription()
            {
                Name = "Container",
                CastShadow = true,
                SphericVolume = false,
                Instances = 5,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/container",
                    ModelContentFilename = "Container.xml",
                }
            };
            containers = await this.AddComponentModelInstanced(desc);
            containers.Visible = false;
            AttachToGround(containers, false);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeTorchs()
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
            torchs = await this.AddComponentModelInstanced(tcDesc, SceneObjectUsages.None, layerObjects);
            torchs.Visible = false;
            AttachToGround(torchs, false);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeParticles()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Systems" }, SceneObjectUsages.None, layerEffects);

            pFire = ParticleSystemDescription.InitializeFire("resources/particles", "fire.png", 0.5f);
            pPlume = ParticleSystemDescription.InitializeSmokePlume("resources/particles", "smoke.png", 0.5f);
            pDust = ParticleSystemDescription.InitializeDust("resources/particles", "dust.png", 2f);
            pDust.MinHorizontalVelocity = 10f;
            pDust.MaxHorizontalVelocity = 15f;
            pDust.MinVerticalVelocity = 0f;
            pDust.MaxVerticalVelocity = 0f;
            pDust.MinColor = new Color(Color.SandyBrown.ToColor3(), 0.05f);
            pDust.MaxColor = new Color(Color.SandyBrown.ToColor3(), 0.10f);
            pDust.MinEndSize = 2f;
            pDust.MaxEndSize = 20f;
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeTerrain()
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
                Textures = new HeightmapTexturesDescription()
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
                    Resolution = 100f,
                },
                Material = new MaterialDescription
                {
                    Shininess = 10f,
                    DiffuseColor = new Color4(1f, 1f, 1f, 1f),
                    SpecularColor = new Color4(0.1f, 0.1f, 0.1f, 1f),
                },
            };
            var gDesc = GroundDescription.FromHeightmapDescription(hDesc, 5);
            terrain = await this.AddComponentTerrain(gDesc, SceneObjectUsages.None, layerTerrain);
            SetGround(terrain, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeLensFlare()
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
            await this.AddComponentLensFlare(lfDesc, SceneObjectUsages.None, layerEffects);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeSkydom()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var skDesc = new SkyScatteringDescription()
            {
                Name = "Sky",
            };
            skydom = await this.AddComponentSkyScattering(skDesc);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeClouds()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var scDesc = new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "Resources/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                SkyMode = SkyPlaneModes.Perturbed,
                Direction = new Vector2(1, 1),
            };
            await this.AddComponentSkyPlane(scDesc);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeDebugAssets()
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Restart();

            bboxesDrawer = await this.AddComponentPrimitiveListDrawer(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "DEBUG++ Terrain nodes bounding boxes",
                    DepthEnabled = true,
                    Dynamic = true,
                    Count = 50000,
                });
            bboxesDrawer.Visible = false;

            bboxesTriDrawer = await this.AddComponentPrimitiveListDrawer(
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Name = "DEBUG++ Terrain nodes bounding boxes faces",
                    DepthEnabled = true,
                    Count = 1000,
                },
                SceneObjectUsages.None,
                layerEffects);
            bboxesTriDrawer.Visible = false;

            linesDrawer = await this.AddComponentPrimitiveListDrawer(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    DepthEnabled = true,
                    Count = 1000,
                },
                SceneObjectUsages.None,
                layerEffects);
            linesDrawer.Visible = false;

            lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "DEBUG++ Light Volumes",
                    DepthEnabled = true,
                    Count = 10000
                });

            graphDrawer = await this.AddComponentPrimitiveListDrawer(
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Name = "DEBUG++ Graph",
                    Count = 50000,
                });
            graphDrawer.Visible = false;

            sw.Stop();
            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }

        private async Task InitializeTerrainObjects()
        {
            var loadTasks = new[]
            {
                InitializeGardener(),
                InitializeGardener2(),
                SetAnimationDictionaries(),
                SetPositionOverTerrain(),
            };

            await LoadResourcesAsync(
                loadTasks,
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    var lanternDesc = SceneLightSpotDescription.Create(Camera.Position, Camera.Direction, 25f, 100, 10000);
                    lantern = new SceneLightSpot("lantern", true, Color.White, Color.White, false, lanternDesc);
                    Lights.Add(lantern);

                    SetDebugInfo();

                    Task.Run(async () =>
                    {
                        await SetPathFindingInfo();
                    });
                });
        }
        private async Task<double> InitializeGardener()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var vDesc = new GroundGardenerDescription()
            {
                Name = "Grass",
                ContentPath = "Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map.png",
                PlantingArea = gardenerArea,
                CastShadow = false,
                Material = new MaterialDescription()
                {
                    DiffuseColor = Color.Gray,
                },
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass_v.dds" },
                    Saturation = 1f,
                    StartRadius = 0f,
                    EndRadius = 100f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1.5f, 1.5f),
                    Seed = 1,
                    WindEffect = 1f,
                    Count = 4,
                },
                ChannelGreen = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass_d.dds" },
                    VegetationNormalMaps = new[] { "grass_n.dds" },
                    Saturation = 1f,
                    StartRadius = 0f,
                    EndRadius = 100f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1f, 1f),
                    Seed = 2,
                    WindEffect = 1f,
                    Count = 4,
                },
                ChannelBlue = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "grass1.png" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1f, 1f),
                    Delta = new Vector3(0, -0.05f, 0),
                    Seed = 3,
                    WindEffect = 1f,
                    Count = 1,
                },
            };
            gardener = await this.AddComponentGroundGardener(vDesc, SceneObjectUsages.None, layerFoliage);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeGardener2()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            var vDesc2 = new GroundGardenerDescription()
            {
                Name = "Flowers",
                ContentPath = "Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map_flowers.png",
                PlantingArea = gardenerArea2,
                CastShadow = false,
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "flower0.dds" },
                    Saturation = 1f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f) * 0.15f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.25f,
                    Seed = 1,
                    WindEffect = 1f,
                },
                ChannelGreen = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "flower1.dds" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f) * 0.15f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.25f,
                    Seed = 2,
                    WindEffect = 1f,
                },
                ChannelBlue = new GroundGardenerDescription.Channel()
                {
                    VegetationTextures = new[] { "flower2.dds" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 140f,
                    MinSize = new Vector2(1f, 1f) * 0.15f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 3,
                    WindEffect = 1f,
                },
            };
            gardener2 = await this.AddComponentGroundGardener(vDesc2, SceneObjectUsages.None, layerFoliage);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task SetAnimationDictionaries()
        {
            await Task.Run(() =>
            {
                var hp = new AnimationPath();
                hp.AddLoop("roll");
                animations.Add("heli_default", new AnimationPlan(hp));

                var sp = new AnimationPath();
                sp.AddLoop("stand");
                animations.Add("soldier_stand", new AnimationPlan(sp));

                var sp1 = new AnimationPath();
                sp1.AddLoop("idle1");
                animations.Add("soldier_idle", new AnimationPlan(sp1));

                var m24_1 = new AnimationPath();
                m24_1.AddLoop("fly");
                animations.Add("m24_idle", new AnimationPlan(m24_1));

                var m24_2 = new AnimationPath();
                m24_2.AddLoop("fly", 5);
                animations.Add("m24_fly", new AnimationPlan(m24_2));
            });
        }
        private async Task SetPositionOverTerrain()
        {
            Random posRnd = new Random(1024);

            var bbox = terrain.GetBoundingBox();

            await Task.WhenAll(
                SetRocksPosition(posRnd, bbox),
                SetForestPosition(posRnd),
                SetBuildingPosition(),
                SetWatchTowerPosition(),
                SetContainersPosition(),
                SetTorchsPosition(posRnd, bbox));

            await Task.WhenAll(
                SetM24Position(),
                SetBradleyPosition());

            //Player soldier
            await SetPlayerPosition();

            //NPC soldiers
            await SetSoldiersPosition();

            rocks.Visible = true;
            trees.Visible = true;
            trees2.Visible = true;
            watchTower.Visible = true;
            containers.Visible = true;
            torchs.Visible = true;
            helicopterI.Visible = true;
            bradleyI.Visible = true;
            buildings.Visible = true;
            soldier.Visible = true;
            troops.Visible = true;
        }
        private async Task SetRocksPosition(Random posRnd, BoundingBox bbox)
        {
            for (int i = 0; i < rocks.InstanceCount; i++)
            {
                var pos = GetRandomPoint(posRnd, Vector3.Zero, bbox);

                if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    float scale;
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

                    rocks[i].Manipulator.SetPosition(r.Position, true);
                    rocks[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), true);
                    rocks[i].Manipulator.SetScale(scale, true);
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetForestPosition(Random posRnd)
        {
            BoundingBox bbox = new BoundingBox(new Vector3(-400, 0, -400), new Vector3(-1000, 1000, -1000));

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                var pos = GetRandomPoint(posRnd, Vector3.Zero, bbox);

                if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    var treePosition = r.Position;
                    treePosition.Y -= posRnd.NextFloat(1f, 5f);

                    trees[i].Manipulator.SetPosition(treePosition, true);
                    trees[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.5f, MathUtil.PiOverFour * 0.5f), 0, true);
                    trees[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                }
            }

            bbox = new BoundingBox(new Vector3(-300, 0, -300), new Vector3(-1000, 1000, -1000));

            for (int i = 0; i < trees2.InstanceCount; i++)
            {
                var pos = GetRandomPoint(posRnd, Vector3.Zero, bbox);

                if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    var treePosition = r.Position;
                    treePosition.Y -= posRnd.NextFloat(0f, 2f);

                    trees2[i].Manipulator.SetPosition(treePosition, true);
                    trees2[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.15f, MathUtil.PiOverFour * 0.15f), 0, true);
                    trees2[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetWatchTowerPosition()
        {
            if (FindTopGroundPosition(-40, -40, out PickingResult<Triangle> r))
            {
                watchTower.Manipulator.SetPosition(r.Position, true);
                watchTower.Manipulator.SetRotation(MathUtil.Pi / 3f, 0, 0, true);
                watchTower.Manipulator.SetScale(1.5f, true);
            }

            await Task.CompletedTask;
        }
        private async Task SetContainersPosition()
        {
            var positions = new[]
            {
                    new Vector3(85,0,-000),
                    new Vector3(75,0,-030),
                    new Vector3(95,0,-060),
                    new Vector3(75,0,-090),
                    new Vector3(65,0,-120),
                };

            for (int i = 0; i < containers.InstanceCount; i++)
            {
                var position = positions[i];

                if (FindTopGroundPosition(position.X, position.Z, out PickingResult<Triangle> res))
                {
                    var pos = res.Position;
                    pos.Y -= 0.5f;

                    containers[i].Manipulator.SetScale(5);
                    containers[i].Manipulator.SetPosition(pos);
                    containers[i].Manipulator.SetRotation(MathUtil.Pi / 16f * (i - 2), 0, 0);
                    containers[i].Manipulator.SetNormal(res.Item.Normal);
                    containers[i].Manipulator.UpdateInternals(true);
                }

                containers[i].TextureIndex = (uint)i;
            }

            await Task.CompletedTask;
        }
        private async Task SetTorchsPosition(Random posRnd, BoundingBox bbox)
        {
            if (FindTopGroundPosition(15, 15, out PickingResult<Triangle> r))
            {
                var position = r.Position;

                torchs[0].Manipulator.SetScale(1f, 1f, 1f, true);
                torchs[0].Manipulator.SetPosition(position, true);
                var tbbox = torchs[0].GetBoundingBox();

                position.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y);

                spotLight1 = new SceneLightSpot(
                    "Red Spot",
                    true,
                    Color.Red,
                    Color.Red,
                    true,
                    SceneLightSpotDescription.Create(position, Vector3.Normalize(Vector3.One * -1f), 25, 25, 100));

                spotLight2 = new SceneLightSpot(
                    "Blue Spot",
                    true,
                    Color.Blue,
                    Color.Blue,
                    true,
                    SceneLightSpotDescription.Create(position, Vector3.Normalize(Vector3.One * -1f), 25, 25, 100));

                Lights.Add(spotLight1);
                Lights.Add(spotLight2);
            }

            SceneLightPoint[] torchLights = new SceneLightPoint[torchs.InstanceCount - 1];
            for (int i = 1; i < torchs.InstanceCount; i++)
            {
                Color color = new Color(
                    posRnd.NextFloat(0, 1),
                    posRnd.NextFloat(0, 1),
                    posRnd.NextFloat(0, 1),
                    1);

                Vector3 position = new Vector3(
                    posRnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                    0f,
                    posRnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                FindTopGroundPosition(position.X, position.Z, out PickingResult<Triangle> res);

                var pos = res.Position;
                torchs[i].Manipulator.SetScale(0.20f, true);
                torchs[i].Manipulator.SetPosition(pos, true);
                BoundingBox tbbox = torchs[i].GetBoundingBox();

                pos.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                torchLights[i - 1] = new SceneLightPoint(
                    string.Format("Torch {0}", i),
                    true,
                    color,
                    color,
                    true,
                    SceneLightPointDescription.Create(pos, 4f, 5f));

                Lights.Add(torchLights[i - 1]);

                pManager.AddParticleSystem(ParticleSystemTypes.CPU, pFire, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.1f });
                pManager.AddParticleSystem(ParticleSystemTypes.CPU, pPlume, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.5f });
            }

            await Task.CompletedTask;
        }
        private async Task SetM24Position()
        {
            var hPositions = new[]
            {
                new Vector3(-120, -10, MathUtil.Pi * 0.1f),
                new Vector3(-220, -8, MathUtil.Pi * 0.15f),
                new Vector3(-320, -10, 0),
            };

            for (int i = 0; i < hPositions.Length; i++)
            {
                if (FindTopGroundPosition(hPositions[i].X, hPositions[i].Y, out PickingResult<Triangle> r))
                {
                    helicopterI[i].Manipulator.SetScale(1.25f, true);
                    helicopterI[i].Manipulator.SetPosition(r.Position, true);
                    helicopterI[i].Manipulator.SetRotation(hPositions[i].Z, 0, 0, true);
                    helicopterI[i].Manipulator.SetNormal(r.Item.Normal);

                    helicopterI[i].AnimationController.TimeDelta = 0.5f * (i + 1);
                    helicopterI[i].AnimationController.AddPath(animations["m24_fly"]);
                    helicopterI[i].AnimationController.Start();
                }
            }

            for (int i = 0; i < helicopterI.InstanceCount; i++)
            {
                Lights.AddRange(helicopterI[i].Lights);
            }

            await Task.CompletedTask;
        }
        private async Task SetBradleyPosition()
        {
            var bPositions = new[]
            {
                new Vector3(-100, 220, MathUtil.Pi * +0.3f),
                new Vector3(-50, 210, MathUtil.Pi * +0.15f),
                new Vector3(0, 200, MathUtil.Pi * 0),
                new Vector3(50, 210, MathUtil.Pi * -0.15f),
                new Vector3(100, 220, MathUtil.Pi * -0.3f),
            };

            for (int i = 0; i < bPositions.Length; i++)
            {
                if (FindTopGroundPosition(bPositions[i].X, bPositions[i].Y, out PickingResult<Triangle> r))
                {
                    bradleyI[i].Manipulator.SetScale(1.2f, true);
                    bradleyI[i].Manipulator.SetPosition(r.Position, true);
                    bradleyI[i].Manipulator.SetRotation(bPositions[i].Z, 0, 0, true);
                    bradleyI[i].Manipulator.SetNormal(r.Item.Normal);
                }
            }

            for (int i = 0; i < bradleyI.InstanceCount; i++)
            {
                Lights.AddRange(bradleyI[i].Lights);
            }

            await Task.CompletedTask;
        }
        private async Task SetBuildingPosition()
        {
            var bPositions = new[]
            {
                new Vector3(-160, -190, MathUtil.Pi),
                new Vector3(-080, -190, MathUtil.Pi),
                new Vector3(+000, -190, MathUtil.Pi),
                new Vector3(+080, -190, MathUtil.Pi),
                new Vector3(+160, -190, MathUtil.Pi),
            };

            for (int i = 0; i < bPositions.Length; i++)
            {
                if (FindTopGroundPosition(bPositions[i].X, bPositions[i].Y, out PickingResult<Triangle> r))
                {
                    buildings[i].Manipulator.SetScale(3f, true);
                    buildings[i].Manipulator.SetPosition(r.Position, true);
                    buildings[i].Manipulator.SetRotation(bPositions[i].Z, 0, 0, true);
                }
            }

            for (int i = 0; i < buildings.InstanceCount; i++)
            {
                Lights.AddRange(buildings[i].Lights);
            }

            await Task.CompletedTask;
        }
        private async Task SetPlayerPosition()
        {
            if (FindAllGroundPosition<Triangle>(-20, -40, out var res))
            {
                soldier.Manipulator.SetPosition(res.Last().Position, true);
                soldier.Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);
            }

            soldier.AnimationController.AddPath(animations["soldier_idle"]);
            soldier.AnimationController.Start();

            await Task.CompletedTask;
        }
        private async Task SetSoldiersPosition()
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
                if (FindTopGroundPosition(iPos[i].X, iPos[i].Y, out PickingResult<Triangle> r))
                {
                    troops[i].Manipulator.SetPosition(r.Position, true);
                    troops[i].Manipulator.SetRotation(iPos[i].Z, 0, 0, true);
                    troops[i].TextureIndex = 1;

                    troops[i].AnimationController.TimeDelta = (i + 1) * 0.2f;
                    troops[i].AnimationController.AddPath(animations["soldier_idle"]);
                    troops[i].AnimationController.Start(Helper.RandomGenerator.NextFloat(0f, 8f));
                }
            }

            await Task.CompletedTask;
        }
        private void SetDebugInfo()
        {
            var listBoxes = Line3D.CreateWiredBox(terrain.GetBoundingBoxes(5));
            bboxesDrawer.AddPrimitives(new Color4(1.0f, 0.0f, 0.0f, 0.55f), listBoxes);

            var a1Lines = Line3D.CreateWiredBox(gardenerArea.Value);
            bboxesDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.55f), a1Lines);
            var a2Lines = Line3D.CreateWiredBox(gardenerArea2.Value);
            bboxesDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.55f), a2Lines);

            var tris1 = Triangle.ComputeTriangleList(Topology.TriangleList, gardenerArea.Value);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.35f), tris1);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.35f), Triangle.Reverse(tris1));
            var tris2 = Triangle.ComputeTriangleList(Topology.TriangleList, gardenerArea2.Value);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.35f), tris2);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.35f), Triangle.Reverse(tris2));
        }

        private async Task SetPathFindingInfo()
        {
            //Agent
            var sbbox = soldier.GetBoundingBox();
            agent.Height = sbbox.Height;
            agent.Radius = sbbox.Width * 0.5f;
            agent.MaxClimb = sbbox.Height * 0.75f;
            agent.MaxSlope = 45;

            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = 0.2f;
            nmsettings.CellHeight = 0.2f;

            //Agents
            nmsettings.Agents = new[] { agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Watershed;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 64;
            nmsettings.UseTileCache = true;
            nmsettings.BuildAllTiles = false;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new PathFinderDescription(nmsettings, nminput);

            await UpdateNavigationGraph();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!uiReady)
            {
                return;
            }

            stats.Text = Game.RuntimeText;

            help.Text = string.Format(
                "{0}. Wind {1} {2:0.000} - Next {3:0.000}; {4} Light brightness: {5:0.00}; CamPos {6}; CamDir {7};",
                Renderer,
                windDirection, windStrength, windNextStrength,
                Environment.TimeOfDay,
                Lights.KeyLight.Brightness,
                Camera.Position, Camera.Direction);

            help2.Text = string.Format("Picks: {0:0000}|{1:00.000}|{2:00.0000000}; Frustum tests: {3:000}|{4:00.000}|{5:00.00000000}; PlantingTaks: {6:000}",
                Counters.PicksPerFrame, Counters.PickingTotalTimePerFrame, Counters.PickingAverageTime,
                Counters.VolumeFrustumTestPerFrame, Counters.VolumeFrustumTestTotalTimePerFrame, Counters.VolumeFrustumTestAverageTime,
                gardener?.PlantingTasks ?? 0 + gardener2?.PlantingTasks ?? 0);

            if (!gameReady)
            {
                return;
            }

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

            //Input driven
            UpdateCamera(gameTime);
            UpdatePlayer();
            UpdateInputDebugInfo(gameTime);
            UpdateInputBuffers();

            //Auto
            UpdateLights();
            UpdateWind(gameTime);
            UpdateDust(gameTime);
            UpdateDrawers();
        }
        private void UpdatePlayer()
        {
            if (Game.Input.KeyJustReleased(Keys.P))
            {
                playerFlying = !playerFlying;

                if (playerFlying)
                {
                    Camera.Following = null;
                }
                else
                {
                    var eyePos = new Vector3(0, agent.Height, 0);
                    var eyeView = -Vector3.ForwardLH * 4f;
                    var interest = eyePos + eyeView;

                    var offset = (eyePos * 1.1f) - (Vector3.Right * 1.5f) - (Vector3.BackwardLH * 4f);
                    var view = Vector3.Normalize(interest - offset);

                    Camera.Following = new CameraFollower(soldier.Manipulator, offset, view);
                }
            }

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                lantern.Enabled = !lantern.Enabled;
                lanternFixed = false;
            }

            if (Game.Input.KeyJustReleased(Keys.O))
            {
                foreach (var t in troops.GetInstances())
                {
                    var bbox = t.GetBoundingBox();
                    BoundingCylinder bc = new BoundingCylinder(t.Manipulator.Position, 1.5f, bbox.Height);

                    NavigationGraph.AddObstacle(bc);
                }
            }
        }
        private void UpdateCamera(GameTime gameTime)
        {
            Vector3 position;
            if (playerFlying)
            {
                position = UpdateFlyingCamera(gameTime);
            }
            else
            {
                position = UpdateWalkingCamera(gameTime);
            }

            if (!udaptingGraph)
            {
                udaptingGraph = true;

                Task.Run(() =>
                {
                    Vector3 extent = Vector3.One * 20f;
                    NavigationGraph.UpdateAt(new BoundingBox(position - extent, position + extent));

                    udaptingGraph = false;
                });
            }
        }
        private Vector3 UpdateFlyingCamera(GameTime gameTime)
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
                Camera.MoveLeft(gameTime, !Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, !Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, !Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, !Game.Input.ShiftPressed);
            }

            return Camera.Position;
        }
        private Vector3 UpdateWalkingCamera(GameTime gameTime)
        {
            var prevPosition = soldier.Manipulator.Position;
#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
            {
                float amount = Game.Input.MouseXDelta;

                soldier.Manipulator.Rotate(amount * gameTime.ElapsedSeconds * 0.5f, 0, 0);
            }
#else
            float amount = Game.Input.MouseXDelta;

            soldier.Manipulator.Rotate(amount * gameTime.ElapsedSeconds, 0, 0);
#endif

            float delta = Game.Input.ShiftPressed ? 24 : 12;

            if (Game.Input.KeyPressed(Keys.A))
            {
                soldier.Manipulator.MoveLeft(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                soldier.Manipulator.MoveRight(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                soldier.Manipulator.MoveForward(gameTime, delta);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                soldier.Manipulator.MoveBackward(gameTime, delta);
            }

            if (Walk(agent, prevPosition, soldier.Manipulator.Position, false, out var finalPosition))
            {
                soldier.Manipulator.SetPosition(finalPosition);
            }
            else
            {
                soldier.Manipulator.SetPosition(prevPosition);
            }

            soldier.Manipulator.UpdateInternals(true);

            return soldier.Manipulator.Position;
        }
        private void UpdateInputDebugInfo(GameTime gameTime)
        {
            UpdateInputWind();

            UpdateInputDrawers();

            UpdateInputLights();

            UpdateInputFog();

            UpdateInputTimeOfDay(gameTime);
        }
        private void UpdateInputWind()
        {
            if (Game.Input.KeyPressed(Keys.Add))
            {
                windStrength += windStep;
                if (windStrength > 100f) windStrength = 100f;
            }

            if (Game.Input.KeyPressed(Keys.Subtract))
            {
                windStrength -= windStep;
                if (windStrength < 0f) windStrength = 0f;
            }
        }
        private void UpdateInputDrawers()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                bboxesDrawer.Visible = !bboxesDrawer.Visible;
                bboxesTriDrawer.Visible = !bboxesTriDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                showSoldierDEBUG = !showSoldierDEBUG;

                if (soldierTris != null) soldierTris.Visible = showSoldierDEBUG;
                if (soldierLines != null) soldierLines.Visible = showSoldierDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                drawDrawVolumes = !drawDrawVolumes;
                drawCullVolumes = false;
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                drawCullVolumes = !drawCullVolumes;
                drawDrawVolumes = false;
            }

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = false;
            }

            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                graphDrawer.Visible = !graphDrawer.Visible;
            }
        }
        private void UpdateInputLights()
        {
            if (Game.Input.KeyJustReleased(Keys.G))
            {
                Lights.KeyLight.CastShadow = !Lights.KeyLight.CastShadow;
            }

            if (Game.Input.KeyJustReleased(Keys.Space))
            {
                lanternFixed = true;
                linesDrawer.SetPrimitives(Color.LightPink, lantern.GetVolume(10));
                linesDrawer.Visible = true;
            }
        }
        private void UpdateInputFog()
        {
            if (Game.Input.KeyJustReleased(Keys.F))
            {
                ToggleFog();
            }
        }
        private void UpdateInputTimeOfDay(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.Left))
            {
                time -= gameTime.ElapsedSeconds * 0.1f;
                Environment.TimeOfDay.SetTimeOfDay(time % 1f);
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                time += gameTime.ElapsedSeconds * 0.1f;
                Environment.TimeOfDay.SetTimeOfDay(time % 1f);
            }
        }
        private void UpdateInputBuffers()
        {
            if (Game.Input.KeyJustReleased(Keys.F8))
            {
                var shadowMap = Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);
                if (shadowMap != null)
                {
                    bufferDrawer.Texture = shadowMap;
                    bufferDrawer.Channels = UITextureRendererChannels.Red;

                    if (Game.Input.ShiftPressed)
                    {
                        int tIndex = bufferDrawer.TextureIndex;

                        tIndex++;
                        tIndex %= 3;

                        bufferDrawer.TextureIndex = tIndex;
                    }
                    else
                    {
                        bufferDrawer.Visible = !bufferDrawer.Visible;
                        bufferDrawer.TextureIndex = 0;
                    }
                }
            }
        }
        private void UpdateDrawers()
        {
            if (showSoldierDEBUG)
            {
                Color color = new Color(Color.Red.ToColor3(), 0.6f);

                var tris = soldier.GetTriangles();

                if (soldierTris == null)
                {
                    Task.Run(async () =>
                    {
                        var desc = new PrimitiveListDrawerDescription<Triangle>()
                        {
                            DepthEnabled = false,
                            Primitives = tris.ToArray(),
                            Color = color
                        };
                        soldierTris = await this.AddComponentPrimitiveListDrawer<Triangle>(desc);
                    }).ConfigureAwait(true);
                }
                else
                {
                    soldierTris.SetPrimitives(color, tris);
                }

                BoundingBox[] bboxes = new BoundingBox[]
                {
                    soldier.GetBoundingBox(true),
                    troops[0].GetBoundingBox(true),
                    troops[1].GetBoundingBox(true),
                    troops[2].GetBoundingBox(true),
                    troops[3].GetBoundingBox(true),
                };
                if (soldierLines == null)
                {
                    Task.Run(async () =>
                    {
                        var desc = new PrimitiveListDrawerDescription<Line3D>()
                        {
                            Primitives = Line3D.CreateWiredBox(bboxes).ToArray(),
                            Color = color
                        };
                        soldierLines = await this.AddComponentPrimitiveListDrawer<Line3D>(desc);
                    }).ConfigureAwait(true);
                }
                else
                {
                    soldierLines.SetPrimitives(color, Line3D.CreateWiredBox(bboxes));
                }
            }

            if (drawDrawVolumes)
            {
                UpdateLightDrawingVolumes();
            }

            if (drawCullVolumes)
            {
                UpdateLightCullingVolumes();
            }
        }
        private void UpdateWind(GameTime gameTime)
        {
            windDuration += gameTime.ElapsedSeconds;
            if (windDuration > 10)
            {
                windDuration = 0;

                windNextStrength = windStrength + Helper.RandomGenerator.NextFloat(-0.5f, +0.5f);
                if (windNextStrength > 100f) windNextStrength = 100f;
                if (windNextStrength < 0f) windNextStrength = 0f;
            }

            if (windNextStrength < windStrength)
            {
                windStrength -= windStep;
                if (windNextStrength > windStrength) windStrength = windNextStrength;
            }
            if (windNextStrength > windStrength)
            {
                windStrength += windStep;
                if (windNextStrength < windStrength) windStrength = windNextStrength;
            }

            gardener.SetWind(windDirection, windStrength);
            gardener2.SetWind(windDirection, windStrength);
        }
        private void UpdateDust(GameTime gameTime)
        {
            nextDust -= gameTime.ElapsedSeconds;

            if (nextDust <= 0)
            {
                nextDust = dustTime;

                var hbsph = helicopterI[nextDustHeli++].GetBoundingSphere();

                nextDustHeli %= helicopterI.InstanceCount;

                hbsph.Radius *= 0.8f;

                GenerateDust(Helper.RandomGenerator, hbsph);
                GenerateDust(Helper.RandomGenerator, hbsph);
                GenerateDust(Helper.RandomGenerator, hbsph);
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

            pDust.Gravity = (windStrength * windDirection);

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDust, emitter);
        }
        private void UpdateLights()
        {
            float d = 1f;
            float v = 5f;

            var x = d * (float)Math.Cos(v * Game.GameTime.TotalSeconds);
            var z = d * (float)Math.Sin(v * Game.GameTime.TotalSeconds);

            spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
            spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));

            spotLight1.Enabled = false;
            spotLight2.Enabled = false;

            if (lantern.Enabled && !lanternFixed)
            {
                lantern.Position = Camera.Position + (Camera.Left * 2);
                lantern.Direction = Camera.Direction;
            }
        }
        private void UpdateLightDrawingVolumes()
        {
            lightsVolumeDrawer.Clear();

            foreach (var spot in Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                lightsVolumeDrawer.AddPrimitives(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                lightsVolumeDrawer.AddPrimitives(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = true;
        }
        private void UpdateLightCullingVolumes()
        {
            lightsVolumeDrawer.Clear();

            foreach (var spot in Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 12, 5);

                lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 12, 5);

                lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = true;
        }
        private void ToggleFog()
        {
            Lights.FogStart = Lights.FogStart == 0f ? fogStart : 0f;
            Lights.FogRange = Lights.FogRange == 0f ? fogRange : 0f;
        }

        public override void NavigationGraphUpdated()
        {
            gameReady = true;

            fadePanel.ClearTween();
            fadePanel.TweenAlpha(fadePanel.Alpha, 0, 2000, ScaleFuncs.CubicEaseOut);

            UpdateGraphNodes(agent);
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            if (updatingNodes)
            {
                return;
            }

            // Fire and forget
            Task.Run(() =>
            {
                updatingNodes = true;

                var nodes = GetNodes(agent).OfType<GraphNode>();
                if (nodes.Any())
                {
                    graphDrawer.Clear();

                    foreach (var node in nodes)
                    {
                        graphDrawer.AddPrimitives(node.Color, node.Triangles);
                    }
                }

                updatingNodes = false;
            });
        }
    }
}
