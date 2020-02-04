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

        private float time = 0.23f;

        private Vector3 playerHeight = Vector3.UnitY * 5f;
        private bool playerFlying = true;
        private SceneLightSpot lantern = null;
        private bool lanternFixed = false;

        private readonly Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private float windNextStrength = 1f;
        private readonly float windStep = 0.001f;
        private float windDuration = 0;

        private TextDrawer stats = null;
        private TextDrawer help = null;
        private TextDrawer help2 = null;

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
        private Model watchTower = null;
        private ModelInstanced containers = null;

        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private PrimitiveListDrawer<Triangle> graphDrawer = null;

        private readonly Agent agent = new Agent()
        {
            Name = "Soldier",
            MaxSlope = 45,
        };

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private SpriteTexture bufferDrawer = null;

        private Guid assetsId = Guid.NewGuid();
        private bool gameReady = false;

        public TestScene3D(Game game)
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override async Task Initialize()
        {
            await InitializeDebug();

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            await InitializeUI();

            List<Task> loadTasks = new List<Task>()
            {
                InitializeRocks(),
                InitializeTrees(),
                InitializeTrees2(),
                InitializeSoldier(),
                InitializeTroops(),
                InitializeM24(),
                InitializeBradley(),
                InitializeWatchTower(),
                InitializeContainers(),
                InitializeTorchs(),
                InitializeTerrain(),
                InitializeGardener(),
                InitializeGardener2(),
                InitializeLensFlare(),
                InitializeSkydom(),
                InitializeClouds(),
                InitializeParticles(),
            };

            await this.Game.LoadResourcesAsync(assetsId, loadTasks.ToArray());
        }
        private async Task<double> InitializeUI()
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

            await this.AddComponentCursor(cursorDesc, SceneObjectUsages.UI, layerCursor);

            #endregion

            #region Texts

            var title = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            this.stats = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.help = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.help2 = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 11, Color.Orange), SceneObjectUsages.UI, layerHUD);

            title.Text = "Heightmap Terrain test";
            this.stats.Text = "";
            this.help.Text = "";
            this.help2.Text = "";

            title.Position = Vector2.Zero;
            this.stats.Position = new Vector2(5, title.Top + title.Height + 3);
            this.help.Position = new Vector2(5, this.stats.Top + this.stats.Height + 3);
            this.help2.Position = new Vector2(5, this.help.Top + this.help.Height + 3);

            var spDesc = new SpriteDescription()
            {
                Name = "Background",
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.help2.Top + this.help2.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            #endregion

            sw.Stop();
            return await Task.FromResult(sw.Elapsed.TotalSeconds);
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
            this.rocks = await this.AddComponentModelInstanced(rDesc, SceneObjectUsages.None, layerObjects);
            this.AttachToGround(this.rocks, false);
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
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees",
                    ModelContentFilename = @"tree.xml",
                }
            };
            this.trees = await this.AddComponentModelInstanced(treeDesc, SceneObjectUsages.None, layerTerrain);
            this.AttachToGround(this.trees, false);
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
                AlphaEnabled = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees2",
                    ModelContentFilename = @"tree.xml",
                }
            };
            this.trees2 = await this.AddComponentModelInstanced(tree2Desc, SceneObjectUsages.None, layerTerrain);
            this.AttachToGround(this.trees2, false);
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
            this.soldier = await this.AddComponentModel(sDesc, SceneObjectUsages.Agent, layerObjects);
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
            this.troops = await this.AddComponentModelInstanced(tDesc, SceneObjectUsages.Agent, layerObjects);
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
            this.helicopterI = await this.AddComponentModelInstanced(mDesc, SceneObjectUsages.None, layerObjects);
            this.AttachToGround(this.helicopterI, true);
            for (int i = 0; i < this.helicopterI.InstanceCount; i++)
            {
                this.Lights.AddRange(this.helicopterI[i].Lights);
            }
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
            this.bradleyI = await this.AddComponentModelInstanced(mDesc, SceneObjectUsages.None, layerObjects);
            this.AttachToGround(this.bradleyI, true);
            for (int i = 0; i < this.bradleyI.InstanceCount; i++)
            {
                this.Lights.AddRange(this.bradleyI[i].Lights);
            }
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
            this.watchTower = await this.AddComponentModel(mDesc, SceneObjectUsages.None, layerObjects);
            this.AttachToGround(this.watchTower, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeContainers()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            this.containers = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
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
                });
            this.AttachToGround(this.containers, false);
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
            this.torchs = await this.AddComponentModelInstanced(tcDesc, SceneObjectUsages.None, layerObjects);
            this.AttachToGround(this.torchs, false);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
        }
        private async Task<double> InitializeParticles()
        {
            Stopwatch sw = Stopwatch.StartNew();

            sw.Restart();
            this.pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Systems" }, SceneObjectUsages.None, layerEffects);

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
            this.terrain = await this.AddComponentTerrain(gDesc, SceneObjectUsages.None, layerTerrain);
            this.SetGround(this.terrain, true);
            sw.Stop();

            return await Task.FromResult(sw.Elapsed.TotalSeconds);
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
            this.gardener = await this.AddComponentGroundGardener(vDesc, SceneObjectUsages.None, layerFoliage);
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
            this.gardener2 = await this.AddComponentGroundGardener(vDesc2, SceneObjectUsages.None, layerFoliage);
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
            this.skydom = await this.AddComponentSkyScattering(skDesc);
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
        private async Task InitializeDebug()
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
            this.bufferDrawer = await this.AddComponentSpriteTexture(desc, SceneObjectUsages.UI, layerEffects);
            this.bufferDrawer.Visible = false;

            this.lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "DEBUG++ Light Volumes",
                    DepthEnabled = true,
                    Count = 10000
                });

            this.graphDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Name = "DEBUG++ Graph",
                    AlphaEnabled = true,
                    Count = 50000,
                });
            this.graphDrawer.Visible = false;
        }

        protected override void GameResourcesLoaded(object sender, GameLoadResourcesEventArgs e)
        {
            if (e.Id == assetsId)
            {
                this.Camera.NearPlaneDistance = near;
                this.Camera.FarPlaneDistance = far;
                this.Camera.Position = new Vector3(24, 12, 14);
                this.Camera.Interest = new Vector3(0, 10, 0);
                this.Camera.MovementDelta = 45f;
                this.Camera.SlowMovementDelta = 20f;

                this.skydom.RayleighScattering *= 0.8f;
                this.skydom.MieScattering *= 0.1f;

                this.TimeOfDay.BeginAnimation(new TimeSpan(8, 55, 00), 1f);

                this.Lights.BaseFogColor = new Color((byte)95, (byte)147, (byte)233) * 0.5f;
                this.ToggleFog();

                var lanternDesc = SceneLightSpotDescription.Create(this.Camera.Position, this.Camera.Direction, 25f, 100, 10000);
                this.lantern = new SceneLightSpot("lantern", true, Color.White, Color.White, true, lanternDesc);
                this.Lights.Add(this.lantern);

                Task.WhenAll(
                    SetAnimationDictionaries(),
                    SetPositionOverTerrain(),
                    SetDebugInfo());

                Task.WhenAll(SetPathFindingInfo());
            }
        }
        private async Task SetAnimationDictionaries()
        {
            await Task.Run(() =>
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

                var m24_1 = new AnimationPath();
                m24_1.AddLoop("fly");
                this.animations.Add("m24_idle", new AnimationPlan(m24_1));

                var m24_2 = new AnimationPath();
                m24_2.AddLoop("fly", 5);
                this.animations.Add("m24_fly", new AnimationPlan(m24_2));
            });
        }
        private async Task SetPositionOverTerrain()
        {
            Random posRnd = new Random(1024);

            var bbox = this.terrain.GetBoundingBox();

            await Task.WhenAll(
                this.SetRocksPosition(posRnd, bbox),
                this.SetForestPosition(posRnd),
                this.SetWatchTowerPosition(),
                this.SetContainersPosition(),
                this.SetTorchsPosition(posRnd, bbox));

            await Task.WhenAll(
                this.SetM24Position(),
                this.SetBradleyPosition());

            //Player soldier
            await this.SetPlayerPosition();

            //NPC soldiers
            await this.SetSoldiersPosition();
        }
        private async Task SetRocksPosition(Random posRnd, BoundingBox bbox)
        {
            for (int i = 0; i < this.rocks.InstanceCount; i++)
            {
                var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
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

                    this.rocks[i].Manipulator.SetPosition(r.Position, true);
                    this.rocks[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), true);
                    this.rocks[i].Manipulator.SetScale(scale, true);
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetForestPosition(Random posRnd)
        {
            BoundingBox bbox = new BoundingBox(new Vector3(-400, 0, -400), new Vector3(-1000, 1000, -1000));

            for (int i = 0; i < this.trees.InstanceCount; i++)
            {
                var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    var treePosition = r.Position;
                    treePosition.Y -= posRnd.NextFloat(1f, 5f);

                    this.trees[i].Manipulator.SetPosition(treePosition, true);
                    this.trees[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.5f, MathUtil.PiOverFour * 0.5f), 0, true);
                    this.trees[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                }
            }

            bbox = new BoundingBox(new Vector3(-300, 0, -300), new Vector3(-1000, 1000, -1000));

            for (int i = 0; i < this.trees2.InstanceCount; i++)
            {
                var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                if (this.FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    var treePosition = r.Position;
                    treePosition.Y -= posRnd.NextFloat(0f, 2f);

                    this.trees2[i].Manipulator.SetPosition(treePosition, true);
                    this.trees2[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.15f, MathUtil.PiOverFour * 0.15f), 0, true);
                    this.trees2[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetWatchTowerPosition()
        {
            if (this.FindTopGroundPosition(-40, -40, out PickingResult<Triangle> r))
            {
                this.watchTower.Manipulator.SetPosition(r.Position, true);
                this.watchTower.Manipulator.SetRotation(MathUtil.Pi / 3f, 0, 0, true);
                this.watchTower.Manipulator.SetScale(1.5f, true);
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

            for (int i = 0; i < this.containers.InstanceCount; i++)
            {
                var position = positions[i];

                if (this.FindTopGroundPosition(position.X, position.Z, out PickingResult<Triangle> res))
                {
                    var pos = res.Position;
                    pos.Y -= 0.5f;

                    this.containers[i].Manipulator.SetScale(5);
                    this.containers[i].Manipulator.SetPosition(pos);
                    this.containers[i].Manipulator.SetRotation(MathUtil.Pi / 16f * (i - 2), 0, 0);
                    this.containers[i].Manipulator.SetNormal(res.Item.Normal);
                    this.containers[i].Manipulator.UpdateInternals(true);
                }

                this.containers[i].TextureIndex = (uint)i;
            }

            await Task.CompletedTask;
        }
        private async Task SetTorchsPosition(Random posRnd, BoundingBox bbox)
        {
            if (this.FindTopGroundPosition(15, 15, out PickingResult<Triangle> r))
            {
                var position = r.Position;

                this.torchs[0].Manipulator.SetScale(1f, 1f, 1f, true);
                this.torchs[0].Manipulator.SetPosition(position, true);
                var tbbox = this.torchs[0].GetBoundingBox();

                position.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y);

                this.spotLight1 = new SceneLightSpot(
                    "Red Spot",
                    true,
                    Color.Red,
                    Color.Red,
                    true,
                    SceneLightSpotDescription.Create(position, Vector3.Normalize(Vector3.One * -1f), 25, 25, 100));

                this.spotLight2 = new SceneLightSpot(
                    "Blue Spot",
                    true,
                    Color.Blue,
                    Color.Blue,
                    true,
                    SceneLightSpotDescription.Create(position, Vector3.Normalize(Vector3.One * -1f), 25, 25, 100));

                this.Lights.Add(this.spotLight1);
                this.Lights.Add(this.spotLight2);
            }

            SceneLightPoint[] torchLights = new SceneLightPoint[this.torchs.InstanceCount - 1];
            for (int i = 1; i < this.torchs.InstanceCount; i++)
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

                this.FindTopGroundPosition(position.X, position.Z, out PickingResult<Triangle> res);

                var pos = res.Position;
                this.torchs[i].Manipulator.SetScale(0.20f, true);
                this.torchs[i].Manipulator.SetPosition(pos, true);
                BoundingBox tbbox = this.torchs[i].GetBoundingBox();

                pos.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                torchLights[i - 1] = new SceneLightPoint(
                    string.Format("Torch {0}", i),
                    true,
                    color,
                    color,
                    true,
                    SceneLightPointDescription.Create(pos, 4f, 5f));

                this.Lights.Add(torchLights[i - 1]);

                this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pFire, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.1f });
                this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pPlume, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.5f });
            }

            await Task.CompletedTask;
        }
        private async Task SetM24Position()
        {
            var hPositions = new[]
            {
                new Vector3(-100, -10, 0),
                new Vector3(-180, -10, 0),
                new Vector3(-260, -10, 0),
            };

            for (int i = 0; i < hPositions.Length; i++)
            {
                if (this.FindTopGroundPosition(hPositions[i].X, hPositions[i].Y, out PickingResult<Triangle> r))
                {
                    this.helicopterI[i].Manipulator.SetPosition(r.Position, true);
                    this.helicopterI[i].Manipulator.SetRotation(hPositions[i].Z, 0, 0, true);
                    this.helicopterI[i].Manipulator.SetNormal(r.Item.Normal);

                    this.helicopterI[i].AnimationController.TimeDelta = 0.5f * (i + 1);
                    this.helicopterI[i].AnimationController.AddPath(this.animations["m24_fly"]);
                    this.helicopterI[i].AnimationController.Start();
                }
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
                if (this.FindTopGroundPosition(bPositions[i].X, bPositions[i].Y, out PickingResult<Triangle> r))
                {
                    this.bradleyI[i].Manipulator.SetScale(1.2f, true);
                    this.bradleyI[i].Manipulator.SetPosition(r.Position, true);
                    this.bradleyI[i].Manipulator.SetRotation(bPositions[i].Z, 0, 0, true);
                    this.bradleyI[i].Manipulator.SetNormal(r.Item.Normal);
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetPlayerPosition()
        {
            if (this.FindAllGroundPosition(-40, -40, out PickingResult<Triangle>[] res))
            {
                this.soldier.Manipulator.SetPosition(res[2].Position, true);
                this.soldier.Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);
            }

            this.soldier.AnimationController.AddPath(this.animations["soldier_idle"]);
            this.soldier.AnimationController.Start();

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
                if (this.FindTopGroundPosition(iPos[i].X, iPos[i].Y, out PickingResult<Triangle> r))
                {
                    this.troops[i].Manipulator.SetPosition(r.Position, true);
                    this.troops[i].Manipulator.SetRotation(iPos[i].Z, 0, 0, true);
                    this.troops[i].TextureIndex = 1;

                    this.troops[i].AnimationController.TimeDelta = (i + 1) * 0.2f;
                    this.troops[i].AnimationController.AddPath(this.animations["soldier_idle"]);
                    this.troops[i].AnimationController.Start(Helper.RandomGenerator.NextFloat(0f, 8f));
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetDebugInfo()
        {
            this.bboxesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Name = "DEBUG++ Terrain nodes bounding boxes",
                    AlphaEnabled = true,
                    DepthEnabled = true,
                    Dynamic = true,
                    Count = 50000,
                });
            this.bboxesDrawer.Visible = false;

            var boxes = this.terrain.GetBoundingBoxes(5);
            var listBoxes = Line3D.CreateWiredBox(boxes);

            this.bboxesDrawer.AddPrimitives(new Color4(1.0f, 0.0f, 0.0f, 0.55f), listBoxes);

            var a1Lines = Line3D.CreateWiredBox(gardenerArea.Value);
            var a2Lines = Line3D.CreateWiredBox(gardenerArea2.Value);

            this.bboxesDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.55f), a1Lines);
            this.bboxesDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.55f), a2Lines);

            this.bboxesTriDrawer = await this.AddComponentPrimitiveListDrawer<Triangle>(
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Name = "DEBUG++ Terrain nodes bounding boxes faces",
                    AlphaEnabled = true,
                    DepthEnabled = true,
                    Count = 1000,
                },
                SceneObjectUsages.None,
                layerEffects);
            this.bboxesTriDrawer.Visible = false;

            var tris1 = Triangle.ComputeTriangleList(Topology.TriangleList, gardenerArea.Value);
            var tris2 = Triangle.ComputeTriangleList(Topology.TriangleList, gardenerArea2.Value);

            this.bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.35f), tris1);
            this.bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.35f), Triangle.Reverse(tris1));

            this.bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.35f), tris2);
            this.bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.35f), Triangle.Reverse(tris2));

            this.linesDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    DepthEnabled = true,
                    Count = 1000,
                },
                SceneObjectUsages.None,
                layerEffects);
            this.linesDrawer.Visible = false;
        }
        private async Task SetPathFindingInfo()
        {
            //Player height
            var sbbox = this.soldier.GetBoundingBox();
            this.playerHeight.Y = sbbox.Maximum.Y - sbbox.Minimum.Y;
            this.agent.Height = this.playerHeight.Y;
            this.agent.Radius = this.playerHeight.Y * 0.33f;
            this.agent.MaxClimb = this.playerHeight.Y * 0.5f;
            this.agent.MaxSlope = 45;

            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = 1f;
            nmsettings.CellHeight = 1f;

            //Agents
            nmsettings.Agents = new[] { agent };

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Watershed;

            //Polygonization
            nmsettings.EdgeMaxError = 1.0f;

            //Tiling
            nmsettings.BuildMode = BuildModes.TempObstacles;
            nmsettings.TileSize = 16;

            nmsettings.Bounds = new BoundingBox(
                new Vector3(-100, -100, -100),
                new Vector3(+100, +100, +100));

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new PathFinderDescription(nmsettings, nminput);

            await this.UpdateNavigationGraph();
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
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            //Input driven
            UpdateCamera(gameTime, shift);
            UpdatePlayer();
            UpdateInputDebugInfo(gameTime);
            UpdateInputBuffers(shift);

            //Auto
            UpdateLights();
            UpdateWind(gameTime);
            UpdateDust(gameTime);
            UpdateGraph(gameTime);
            UpdateDrawers();

            this.help.Text = string.Format(
                "{0}. Wind {1} {2:0.000} - Next {3:0.000}; {4} Light brightness: {5:0.00}; CamPos {6}; CamDir {7};",
                this.Renderer,
                this.windDirection, this.windStrength, this.windNextStrength,
                this.TimeOfDay,
                this.Lights.KeyLight.Brightness,
                this.Camera.Position, this.Camera.Direction);

            this.help2.Text = string.Format("Picks: {0:0000}|{1:00.000}|{2:00.0000000}; Frustum tests: {3:000}|{4:00.000}|{5:00.00000000}; PlantingTaks: {6:000}",
                Counters.PicksPerFrame, Counters.PickingTotalTimePerFrame, Counters.PickingAverageTime,
                Counters.VolumeFrustumTestPerFrame, Counters.VolumeFrustumTestTotalTimePerFrame, Counters.VolumeFrustumTestAverageTime,
                this.gardener.PlantingTasks + this.gardener2.PlantingTasks);
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
                    this.Camera.Following = new CameraFollower(this.soldier.Manipulator, offset, view);
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.lantern.Enabled = !this.lantern.Enabled;
                this.lanternFixed = false;
            }
        }
        private void UpdateCamera(GameTime gameTime, bool shift)
        {
            if (this.playerFlying)
            {
                this.UpdateFlyingCamera(gameTime, shift);
            }
            else
            {
                this.UpdateWalkingCamera(gameTime, shift);
            }
        }
        private void UpdateFlyingCamera(GameTime gameTime, bool shift)
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
        private void UpdateWalkingCamera(GameTime gameTime, bool shift)
        {
#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                this.Game.GameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            float delta = shift ? 8 : 4;

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.soldier.Manipulator.MoveLeft(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.soldier.Manipulator.MoveRight(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.soldier.Manipulator.MoveForward(gameTime, delta);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.soldier.Manipulator.MoveBackward(gameTime, delta);
            }

            if (this.FindTopGroundPosition(this.soldier.Manipulator.Position.X, this.soldier.Manipulator.Position.Z, out PickingResult<Triangle> r))
            {
                this.soldier.Manipulator.SetPosition(r.Position);
            }
        }
        private void UpdateInputDebugInfo(GameTime gameTime)
        {
            this.UpdateInputWind();

            this.UpdateInputDrawers();

            this.UpdateInputLights();

            this.UpdateInputFog();

            this.UpdateInputTimeOfDay(gameTime);
        }
        private void UpdateInputWind()
        {
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
        }
        private void UpdateInputDrawers()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
                this.bboxesTriDrawer.Visible = !this.bboxesTriDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.showSoldierDEBUG = !this.showSoldierDEBUG;

                if (this.soldierTris != null) this.soldierTris.Visible = this.showSoldierDEBUG;
                if (this.soldierLines != null) this.soldierLines.Visible = this.showSoldierDEBUG;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.drawDrawVolumes = !this.drawDrawVolumes;
                this.drawCullVolumes = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.drawCullVolumes = !this.drawCullVolumes;
                this.drawDrawVolumes = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.graphDrawer.Visible = !this.graphDrawer.Visible;
            }
        }
        private void UpdateInputLights()
        {
            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.KeyLight.CastShadow = !this.Lights.KeyLight.CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.lanternFixed = true;
                this.linesDrawer.SetPrimitives(Color.LightPink, this.lantern.GetVolume(10));
                this.linesDrawer.Visible = true;
            }
        }
        private void UpdateInputFog()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.ToggleFog();
            }
        }
        private void UpdateInputTimeOfDay(GameTime gameTime)
        {
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
        }
        private void UpdateInputBuffers(bool shift)
        {
            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                var shadowMap = this.Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);
                if (shadowMap != null)
                {
                    this.bufferDrawer.Texture = shadowMap;
                    this.bufferDrawer.Channels = SpriteTextureChannels.Red;

                    if (shift)
                    {
                        int tIndex = this.bufferDrawer.TextureIndex;

                        tIndex++;
                        tIndex %= 3;

                        this.bufferDrawer.TextureIndex = tIndex;
                    }
                    else
                    {
                        this.bufferDrawer.Visible = !this.bufferDrawer.Visible;
                        this.bufferDrawer.TextureIndex = 0;
                    }
                }
            }
        }
        private void UpdateDrawers()
        {
            if (this.showSoldierDEBUG)
            {
                Color color = new Color(Color.Red.ToColor3(), 0.6f);

                var tris = this.soldier.GetTriangles(true);
                if (this.soldierTris == null)
                {
                    Task.Run(async () =>
                    {
                        var desc = new PrimitiveListDrawerDescription<Triangle>()
                        {
                            DepthEnabled = false,
                            Primitives = tris,
                            Color = color
                        };
                        this.soldierTris = await this.AddComponentPrimitiveListDrawer<Triangle>(desc);
                    }).ConfigureAwait(true);
                }
                else
                {
                    this.soldierTris.SetPrimitives(color, tris);
                }

                BoundingBox[] bboxes = new BoundingBox[]
                {
                    this.soldier.GetBoundingBox(true),
                    this.troops[0].GetBoundingBox(true),
                    this.troops[1].GetBoundingBox(true),
                    this.troops[2].GetBoundingBox(true),
                    this.troops[3].GetBoundingBox(true),
                };
                if (this.soldierLines == null)
                {
                    Task.Run(async () =>
                    {
                        var desc = new PrimitiveListDrawerDescription<Line3D>()
                        {
                            Primitives = Line3D.CreateWiredBox(bboxes).ToArray(),
                            Color = color
                        };
                        this.soldierLines = await this.AddComponentPrimitiveListDrawer<Line3D>(desc);
                    }).ConfigureAwait(true);
                }
                else
                {
                    this.soldierLines.SetPrimitives(color, Line3D.CreateWiredBox(bboxes));
                }
            }

            if (this.drawDrawVolumes)
            {
                this.UpdateLightDrawingVolumes();
            }

            if (this.drawCullVolumes)
            {
                this.UpdateLightCullingVolumes();
            }
        }
        private void UpdateWind(GameTime gameTime)
        {
            this.windDuration += gameTime.ElapsedSeconds;
            if (this.windDuration > 10)
            {
                this.windDuration = 0;

                this.windNextStrength = this.windStrength + Helper.RandomGenerator.NextFloat(-0.5f, +0.5f);
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

            this.gardener.SetWind(this.windDirection, this.windStrength);
            this.gardener2.SetWind(this.windDirection, this.windStrength);
        }
        private void UpdateDust(GameTime gameTime)
        {
            this.nextDust -= gameTime.ElapsedSeconds;

            if (this.nextDust <= 0)
            {
                this.nextDust = this.dustTime;

                var hbsph = this.helicopterI[nextDustHeli++].GetBoundingSphere();

                nextDustHeli %= this.helicopterI.InstanceCount;

                hbsph.Radius *= 0.8f;

                this.GenerateDust(Helper.RandomGenerator, hbsph);
                this.GenerateDust(Helper.RandomGenerator, hbsph);
                this.GenerateDust(Helper.RandomGenerator, hbsph);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDust, emitter);
        }
        private void UpdateLights()
        {
            float d = 1f;
            float v = 5f;

            var x = d * (float)Math.Cos(v * this.Game.GameTime.TotalSeconds);
            var z = d * (float)Math.Sin(v * this.Game.GameTime.TotalSeconds);

            this.spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
            this.spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));

            this.spotLight1.Enabled = false;
            this.spotLight2.Enabled = false;

            if (this.lantern.Enabled && !lanternFixed)
            {
                this.lantern.Position = this.Camera.Position + (this.Camera.Left * 2);
                this.lantern.Direction = this.Camera.Direction;
            }
        }
        private void UpdateLightDrawingVolumes()
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
        private void UpdateLightCullingVolumes()
        {
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void UpdateGraph(GameTime gameTime)
        {
            graphUpdateSeconds -= gameTime.ElapsedSeconds;

            if (graphUpdateRequested && graphUpdateSeconds <= 0f)
            {
                graphUpdateRequested = false;
                graphUpdateSeconds = 0;

                this.UpdateGraphNodes(this.agent);
            }
        }

        private bool graphUpdateRequested = false;
        private float graphUpdateSeconds = 0;
        private void UpdateGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent).OfType<GraphNode>();
            if (nodes.Any())
            {
                this.graphDrawer.Clear();

                foreach (var node in nodes)
                {
                    this.graphDrawer.AddPrimitives(node.Color, node.Triangles);
                }
            }
        }
        private void RequestGraphUpdate(float seconds)
        {
            graphUpdateRequested = true;
            graphUpdateSeconds = seconds;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.stats.Text = this.Game.RuntimeText;
        }

        public override void NavigationGraphUpdated()
        {
            this.RequestGraphUpdate(0.2f);

            gameReady = true;
        }
    }
}
