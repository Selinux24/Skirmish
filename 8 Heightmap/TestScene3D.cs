using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HeightmapTest
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

        private Random rnd = new Random();

        private float time = 0.23f;

        private Vector3 playerHeight = Vector3.UnitY * 5f;
        private bool playerFlying = true;
        private SceneLightSpot lantern = null;

        private Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private float windNextStrength = 1f;
        private float windStep = 0.001f;
        private float windDuration = 0;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer stats = null;
        private TextDrawer help = null;
        private TextDrawer help2 = null;
        private Sprite backPannel = null;

        private Cursor cursor;
        private LensFlare lensFlare = null;
        private SkyScattering skydom = null;
        private SkyPlane clouds = null;
        private Terrain terrain = null;
        private GroundGardener gardener = null;
        private GroundGardener gardener2 = null;
        private LineListDrawer bboxesDrawer = null;

        private ModelInstanced torchs = null;
        private SceneLightPoint[] torchLights = null;
        private SceneLightSpot spotLight1 = null;
        private SceneLightSpot spotLight2 = null;

        private ParticleManager pManager = null;
        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private float nextDust = 0;
        private float dustTime = 0.33f;

        private ModelInstanced rocks = null;
        private ModelInstanced trees = null;
        private ModelInstanced trees2 = null;

        private Model soldier = null;
        private TriangleListDrawer soldierTris = null;
        private LineListDrawer soldierLines = null;
        private bool showSoldierDEBUG = false;

        private ModelInstanced troops = null;

        private Model helicopter = null;
        private Model helicopter2 = null;

        private Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            Random rnd = new Random(1);

            #region Cursor

            var cursorDesc = new SpriteDescription()
            {
                Textures = new[] { "target.png" },
                Width = 20,
                Height = 20,
            };

            this.cursor = this.AddCursor(cursorDesc, layerHUD);

            #endregion

            #region Texts

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White), layerHUD);
            this.load = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), layerHUD);
            this.stats = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), layerHUD);
            this.help = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), layerHUD);
            this.help2 = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Orange), layerHUD);

            this.title.Text = "Heightmap Terrain test";
            this.load.Text = "";
            this.stats.Text = "";
            this.help.Text = "";
            this.help2.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(5, this.title.Top + this.title.Height + 3);
            this.stats.Position = new Vector2(5, this.load.Top + this.load.Height + 3);
            this.help.Position = new Vector2(5, this.stats.Top + this.stats.Height + 3);
            this.help2.Position = new Vector2(5, this.help.Top + this.help.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.help2.Top + this.help2.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddSprite(spDesc, layerHUD - 1);

            #endregion

            #region Models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Rocks

            sw.Restart();
            var rDesc = new ModelInstancedDescription()
            {
                Name = "Rocks",
                CastShadow = true,
                Static = true,
                Instances = 250,
            };
            this.rocks = this.AddInstancingModel(@"Resources/Rocks", @"boulder.xml", rDesc, true, layerObjects);
            sw.Stop();
            loadingText += string.Format("Rocks: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Trees

            sw.Restart();
            var treeDesc = new ModelInstancedDescription()
            {
                Name = "Trees",
                CastShadow = true,
                Static = true,
                Instances = 200,
                AlphaEnabled = true,
            };
            this.trees = this.AddInstancingModel(@"Resources/Trees", @"tree.xml", treeDesc, true, layerTerrain);
            sw.Stop();
            loadingText += string.Format("Trees: {0} ", sw.Elapsed.TotalSeconds);

            sw.Restart();
            var tree2Desc = new ModelInstancedDescription()
            {
                Name = "Trees",
                CastShadow = true,
                Static = true,
                Instances = 200,
                AlphaEnabled = true,
            };
            this.trees2 = this.AddInstancingModel(@"Resources/Trees2", @"tree.xml", tree2Desc, true, layerTerrain);
            sw.Stop();
            loadingText += string.Format("Trees2: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Soldier

            sw.Restart();
            var sDesc = new ModelDescription()
            {
                Name = "Soldier",
                TextureIndex = 0,
                CastShadow = true,
            };
            this.soldier = this.AddModel(@"Resources/Soldier", @"soldier_anim2.xml", sDesc, true, layerObjects);
            sw.Stop();
            loadingText += string.Format("Soldier: {0} ", sw.Elapsed.TotalSeconds);

            this.playerHeight.Y = this.soldier.GetBoundingBox().Maximum.Y - this.soldier.GetBoundingBox().Minimum.Y;

            #endregion

            #region Troops

            sw.Restart();
            var tDesc = new ModelInstancedDescription()
            {
                Name = "Troops",
                Instances = 4,
                CastShadow = true,
            };
            this.troops = this.AddInstancingModel(@"Resources/Soldier", @"soldier_anim2.xml", tDesc, true, layerObjects);
            sw.Stop();
            loadingText += string.Format("Troops: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region M24

            sw.Restart();
            var mDesc = new ModelDescription()
            {
                Name = "M24",
                CastShadow = true,
            };
            this.helicopter = this.AddModel(@"Resources/m24", @"m24.xml", mDesc, true, layerObjects);
            sw.Stop();
            loadingText += string.Format("M24: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopter

            sw.Restart();
            var hcDesc = new ModelDescription()
            {
                Name = "Helicopter",
                CastShadow = true,
                Static = false,
                TextureIndex = 2,
            };
            this.helicopter2 = this.AddModel("resources/Helicopter", "Helicopter.xml", hcDesc, true, layerObjects);
            sw.Stop();
            loadingText += string.Format("Helicopter: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Torchs

            sw.Restart();
            var tcDesc = new ModelInstancedDescription()
            {
                Name = "Torchs",
                Instances = 50,
                CastShadow = true,
            };
            this.torchs = this.AddInstancingModel(@"Resources/Scenery/Objects", @"torch.xml", tcDesc, true, layerObjects);
            sw.Stop();
            loadingText += string.Format("Torchs: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Particle Systems

            this.pManager = this.AddParticleManager(new ParticleManagerDescription() { Name = "Particle Systems" }, layerEffects);

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

            #endregion

            #region Terrain

            sw.Restart();
            var pfSettings = NavigationMeshGenerationSettings.Default;
            pfSettings.CellHeight = 5f;
            pfSettings.CellSize = 5f;
            var hDesc =
                new HeightmapDescription()
                {
                    ContentPath = "Resources/Scenery/Heightmap",
                    HeightmapFileName = "desert0hm.bmp",
                    ColormapFileName = "desert0cm.bmp",
                    CellSize = 15,
                    MaximumHeight = 150,
                    TextureResolution = 75f,
                    Textures = new HeightmapDescription.TexturesDescription()
                    {
                        ContentPath = "Textures",
                        NormalMaps = new[] { "normal001.dds", "normal002.dds" },

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
            var gDesc =
                new GroundDescription()
                {
                    Name = "Terrain",
                    Quadtree = new GroundDescription.QuadtreeDescription()
                    {
                        MaximumDepth = 5,
                    },
                    //PathFinder = new GroundDescription.PathFinderDescription()
                    //{
                    //    Settings = pfSettings,
                    //},
                    DelayGeneration = true,
                };
            this.terrain = this.AddTerrain(hDesc, gDesc, true, layerTerrain);
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Gardener

            sw.Restart();
            var vDesc = new GroundGardenerDescription()
            {
                ContentPath = "Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map.png",
                Material = new MaterialDescription()
                {
                    DiffuseColor = Color.Gray,
                },
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "grass0.png" },
                    Saturation = 0.05f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f),
                    MaxSize = new Vector2(1.5f, 2f),
                    Seed = 1,
                    WindEffect = 0.8f,
                },
                ChannelGreen = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "grass2.png" },
                    Saturation = 10f,
                    StartRadius = 0f,
                    EndRadius = 140f,
                    MinSize = new Vector2(1f, 0.5f),
                    MaxSize = new Vector2(2f, 1f),
                    Seed = 2,
                    WindEffect = 0.2f,
                },
                ChannelBlue = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "grass1.png" },
                    Saturation = 0.05f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(2f, 2f),
                    MaxSize = new Vector2(2.5f, 3f),
                    Seed = 3,
                    WindEffect = 0.3f,
                },
            };
            this.gardener = this.AddGardener(vDesc, layerFoliage);
            sw.Stop();
            loadingText += string.Format("gardener: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Gardener 2

            sw.Restart();
            var vDesc2 = new GroundGardenerDescription()
            {
                ContentPath = "Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map_flowers.png",
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "flower0.dds" },
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
                    VegetarionTextures = new[] { "flower1.dds" },
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
                    VegetarionTextures = new[] { "flower2.dds" },
                    Saturation = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 140f,
                    MinSize = new Vector2(1f, 1f) * 0.5f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 3,
                    WindEffect = 0.5f,
                },
            };
            this.gardener2 = this.AddGardener(vDesc2, layerFoliage);
            sw.Stop();
            loadingText += string.Format("gardener2: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Lens flare

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
            this.lensFlare = this.AddLensFlare(lfDesc, layerEffects);
            sw.Stop();
            loadingText += string.Format("Flares: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Skydom

            sw.Restart();
            var skDesc = new SkyScatteringDescription()
            {
                Name = "Sky",
            };
            this.skydom = this.AddSkyScattering(skDesc);
            sw.Stop();
            loadingText += string.Format("Sky: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Clouds

            sw.Restart();
            var scDesc = new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "Resources/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                Mode = SkyPlaneMode.Perturbed,
                MaxBrightness = 0.8f,
                MinBrightness = 0.5f,
                Repeat = 5,
                Velocity = 1f,
                Direction = new Vector2(1, 1),
            };
            this.clouds = this.AddSkyPlane(scDesc);
            sw.Stop();
            loadingText += string.Format("Clouds: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            this.load.Text = loadingText;

            #endregion

            #region Animations

            var hp = new AnimationPath();
            hp.AddLoop("roll");
            this.animations.Add("heli_default", new AnimationPlan(hp));

            var sp = new AnimationPath();
            sp.AddLoop("stand");
            this.animations.Add("soldier_stand", new AnimationPlan(sp));

            var sp1 = new AnimationPath();
            sp1.AddLoop("idle1");
            this.animations.Add("soldier_idle", new AnimationPlan(sp1));

            #endregion

            #region Positioning

            Random posRnd = new Random(1024);

            BoundingBox bbox = this.terrain.GetBoundingBox();

            {
                #region Rocks

                for (int i = 0; i < this.rocks.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                    Vector3 rockPosition;
                    Triangle rockTri;
                    float rockDist;
                    if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out rockPosition, out rockTri, out rockDist))
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

                        this.rocks[i].Manipulator.SetPosition(rockPosition, true);
                        this.rocks[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), true);
                        this.rocks[i].Manipulator.SetScale(scale, true);
                    }
                }

                #endregion
            }

            {
                #region Forest

                bbox = new BoundingBox(new Vector3(-400, 0, -400), new Vector3(-1000, 1000, -1000));

                for (int i = 0; i < this.trees.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                    Vector3 treePosition;
                    Triangle treeTri;
                    float treeDist;
                    if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out treePosition, out treeTri, out treeDist))
                    {
                        treePosition.Y -= posRnd.NextFloat(1f, 5f);

                        this.trees[i].Manipulator.SetPosition(treePosition, true);
                        this.trees[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.5f, MathUtil.PiOverFour * 0.5f), 0, true);
                        this.trees[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                    }
                }

                bbox = new BoundingBox(new Vector3(-300, 0, -300), new Vector3(-1000, 1000, -1000));

                for (int i = 0; i < this.trees2.Count; i++)
                {
                    var pos = this.GetRandomPoint(posRnd, Vector3.Zero, bbox);

                    Vector3 treePosition;
                    Triangle treeTri;
                    float treeDist;
                    if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out treePosition, out treeTri, out treeDist))
                    {
                        treePosition.Y -= posRnd.NextFloat(0f, 2f);

                        this.trees2[i].Manipulator.SetPosition(treePosition, true);
                        this.trees2[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.15f, MathUtil.PiOverFour * 0.15f), 0, true);
                        this.trees2[i].Manipulator.SetScale(posRnd.NextFloat(1.5f, 2.5f), true);
                    }
                }

                #endregion
            }

            {
                #region Torchs

                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(5, 5, out position, out triangle, out distance))
                {
                    this.torchs[0].Manipulator.SetScale(1f, 1f, 1f, true);
                    this.torchs[0].Manipulator.SetPosition(position, true);
                    BoundingBox tbbox = this.torchs[0].GetBoundingBox();

                    position.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                    this.spotLight1 = new SceneLightSpot(
                        "Red Spot",
                        false,
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
                        false,
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

                    Vector3 pos = new Vector3(
                        rnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                        0f,
                        rnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                    Triangle t;
                    float d;
                    this.terrain.FindTopGroundPosition(pos.X, pos.Z, out pos, out t, out d);

                    this.torchs[i].Manipulator.SetScale(0.20f, true);
                    this.torchs[i].Manipulator.SetPosition(pos, true);
                    BoundingBox tbbox = this.torchs[i].GetBoundingBox();

                    pos.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                    this.torchLights[i - 1] = new SceneLightPoint(
                        string.Format("Torch {0}", i),
                        false,
                        color,
                        color,
                        true,
                        pos,
                        4f,
                        5f);

                    this.Lights.Add(this.torchLights[i - 1]);

                    this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pFire, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.1f });
                    this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pPlume, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.5f });
                }

                #endregion
            }

            //M24
            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(100, 50, out position, out triangle, out distance))
                {
                    this.helicopter.Manipulator.SetPosition(position, true);
                    this.helicopter.Manipulator.SetRotation(MathUtil.Pi / 5f, 0, 0, true);
                }
            }

            //Helicopter
            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(-100, -10, out position, out triangle, out distance))
                {
                    this.helicopter2.Manipulator.SetPosition(position, true);
                    this.helicopter2.Manipulator.SetRotation(MathUtil.Pi / 3f, 0, 0, true);
                    this.helicopter2.Manipulator.SetScale(5, true);
                }

                this.helicopter2.AnimationController.TimeDelta = 2f;
                this.helicopter2.AnimationController.AddPath(this.animations["heli_default"]);
                this.helicopter2.AnimationController.Start();
            }

            this.terrain.AttachFullPathFinding(new ModelBase[] { this.helicopter, this.helicopter2 }, false);
            this.terrain.AttachCoarsePathFinding(new ModelBase[] { this.torchs, this.rocks, this.trees, this.trees2 }, false);
            this.terrain.UpdateInternals();

            this.gardener.ParentGround = this.terrain;
            this.gardener2.ParentGround = this.terrain;
            this.lensFlare.ParentGround = this.terrain;

            //Player soldier
            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(0, 0, out position, out triangle, out distance))
                {
                    this.soldier.Manipulator.SetPosition(position, true);
                }

                this.soldier.AnimationController.AddPath(this.animations["soldier_stand"]);
                this.soldier.AnimationController.Start();
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
                    Vector3 position;
                    Triangle triangle;
                    float distance;
                    if (this.terrain.FindTopGroundPosition(iPos[i].X, iPos[i].Y, out position, out triangle, out distance))
                    {
                        this.troops[i].Manipulator.SetPosition(position, true);
                        this.troops[i].Manipulator.SetRotation(iPos[i].Z, 0, 0, true);
                        this.troops[i].TextureIndex = 1;

                        this.troops[i].AnimationController.TimeDelta = (i + 1) * 0.2f;
                        this.troops[i].AnimationController.AddPath(this.animations["soldier_idle"]);
                        this.troops[i].AnimationController.Start(rnd.NextFloat(0f, 8f));
                    }
                }
            }

            #endregion

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;
            this.Camera.Position = new Vector3(24, 12, 14);
            this.Camera.Interest = new Vector3(0, 10, 0);
            //this.Camera.MovementDelta *= 10f;

            this.skydom.RayleighScattering *= 0.8f;
            this.skydom.MieScattering *= 0.1f;

            this.TimeOfDay.BeginAnimation(new TimeSpan(7, 55, 00), 1f);

            this.Lights.BaseFogColor = new Color((byte)95, (byte)147, (byte)233) * 0.5f;
            this.ToggleFog();

            this.lantern = new SceneLightSpot("lantern", false, Color.White, Color.White, true, this.Camera.Position, this.Camera.Forward, 25f, 100, 50);
            this.Lights.Add(this.lantern);

            #region Debug

            var bboxes = this.terrain.GetBoundingBoxes(5);
            var listBoxes = Line3D.CreateWiredBox(bboxes);

            var bboxesDrawerDesc = new LineListDrawerDescription()
            {
                DepthEnabled = true,
            };
            this.bboxesDrawer = this.AddLineListDrawer(bboxesDrawerDesc, listBoxes, new Color(1.0f, 0.0f, 0.0f, 0.5f));
            this.bboxesDrawer.Visible = false;

            #endregion
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            Ray cursorRay = this.GetPickingRay();

            #region Walk / Fly

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.playerFlying = !this.playerFlying;

                if (this.playerFlying)
                {
                    this.Fly();
                }
                else
                {
                    this.Walk();
                }
            }

            #endregion

            #region Camera

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
                    this.Camera.MoveLeft(gameTime, false);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.Camera.MoveRight(gameTime, false);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.Camera.MoveForward(gameTime, false);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.Camera.MoveBackward(gameTime, false);
                }
            }
            else
            {
#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.soldier.Manipulator.Rotate(
                        this.Game.Input.MouseXDelta * 0.001f,
                        0, 0);
                }

                if (this.Game.Input.KeyPressed(Keys.A))
                {
                    this.soldier.Manipulator.MoveLeft(gameTime, 4);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.soldier.Manipulator.MoveRight(gameTime, 4);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.soldier.Manipulator.MoveForward(gameTime, 4);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.soldier.Manipulator.MoveBackward(gameTime, 4);
                }

                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(this.soldier.Manipulator.Position.X, this.soldier.Manipulator.Position.Z, out position, out triangle, out distance))
                {
                    this.soldier.Manipulator.SetPosition(position);
                };
            }

            #endregion

            #region Wind

            this.windDuration += gameTime.ElapsedSeconds;
            if (this.windDuration > 10)
            {
                this.windDuration = 0;

                this.windNextStrength = this.windStrength + this.rnd.NextFloat(-0.5f, +0.5f);
                if (this.windNextStrength > 100f) this.windNextStrength = 100f;
                if (this.windNextStrength < 0f) this.windNextStrength = 0f;
            }

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

            #endregion

            #region Particles

            this.nextDust -= gameTime.ElapsedSeconds;

            if (this.nextDust <= 0)
            {
                this.nextDust = this.dustTime;

                var hbsph = this.helicopter2.GetBoundingSphere();
                hbsph.Radius *= 0.8f;

                this.GenerateDust(this.rnd, hbsph);
                this.GenerateDust(this.rnd, hbsph);
                this.GenerateDust(this.rnd, hbsph);
            }

            #endregion

            #region Lights

            {
                float d = 1f;
                float v = 5f;

                var x = d * (float)Math.Cos(v * this.Game.GameTime.TotalSeconds);
                var z = d * (float)Math.Sin(v * this.Game.GameTime.TotalSeconds);

                this.spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
                this.spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.ToggleFog();
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.lantern.Enabled = !this.lantern.Enabled;
            }

            if (this.lantern.Enabled)
            {
                this.lantern.Position = this.Camera.Position;
                this.lantern.Direction = this.Camera.Forward;
            }

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

            #region Debug

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

                var tris = this.soldier.GetTriangles(true);
                if (this.soldierTris == null)
                {
                    this.soldierTris = this.AddTriangleListDrawer(new TriangleListDrawerDescription() { DepthEnabled = false }, tris, color);
                }
                else
                {
                    this.soldierTris.SetTriangles(color, tris);
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
                    this.soldierLines = this.AddLineListDrawer(new LineListDrawerDescription(), Line3D.CreateWiredBox(bboxes), color);
                }
                else
                {
                    this.soldierLines.SetLines(color, Line3D.CreateWiredBox(bboxes));
                }
            }

            #endregion

            base.Update(gameTime);

            this.help.Text = string.Format(
                "{0}. Wind {1} {2:0.000} - Next {3:0.000}; {4} Light brightness: {5:0.00};",
                this.Renderer,
                this.windDirection, this.windStrength, this.windNextStrength,
                this.TimeOfDay,
                this.Lights.KeyLight.Brightness);

            this.help2.Text = string.Format("Picks: {0:0000}|{1:00.000}|{2:00.0000000}; Frustum tests: {3:000}|{4:00.000}|{5:00.00000000}; PlantingTaks: {6:000}", 
                Counters.PicksPerFrame, Counters.PickingTotalTimePerFrame, Counters.PickingAverageTime,
                Counters.VolumeFrustumTestPerFrame, Counters.VolumeFrustumTestTotalTimePerFrame, Counters.VolumeFrustumTestAverageTime,
                this.gardener.PlantingTasks + this.gardener2.PlantingTasks);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.stats.Text = this.Game.RuntimeText;
        }

        private void Fly()
        {
            this.Camera.Following = null;
        }
        private void Walk()
        {
            var offset = (this.playerHeight * 1.2f) + (Vector3.ForwardLH * 10f) + (Vector3.Left * 3f);
            var view = (Vector3.BackwardLH * 4f) + Vector3.Down;
            this.Camera.Following = new CameraFollower(this.soldier.Manipulator, offset, view);
        }
        private void ToggleFog()
        {
            this.Lights.FogStart = this.Lights.FogStart == 0f ? fogStart : 0f;
            this.Lights.FogRange = this.Lights.FogRange == 0f ? fogRange : 0f;
        }

        private Vector3 GetRandomPoint(Random rnd, Vector3 offset, BoundingBox bbox)
        {
            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                Vector3 p;
                Triangle t;
                float d;
                if (terrain.FindTopGroundPosition(v.X, v.Z, out p, out t, out d))
                {
                    return p + offset;
                }
            }
        }
        private Vector3 GetRandomPoint(Random rnd, Vector3 offset, BoundingSphere bsph)
        {
            while (true)
            {
                float dist = rnd.NextFloat(0, bsph.Radius);

                Vector3 dir = new Vector3(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1));

                Vector3 v = bsph.Center + (dist * Vector3.Normalize(dir));

                Vector3 p;
                Triangle t;
                float d;
                if (terrain.FindTopGroundPosition(v.X, v.Z, out p, out t, out d))
                {
                    return p + offset;
                }
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
    }
}
