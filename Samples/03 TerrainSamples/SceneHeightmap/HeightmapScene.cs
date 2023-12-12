using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.Tween;
using Engine.UI;
using Engine.UI.Tween;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TerrainSamples.SceneHeightmap
{
    public class HeightmapScene : WalkableScene
    {
        private const float near = 0.5f;
        private const float far = 3000f;
        private const float fogStart = 500f;
        private const float fogRange = 500f;

        private float time = 0.23f;

        private UIControlTweener uiTweener;

        private bool playerFlying = true;
        private SceneLightSpot lantern = null;
        private bool lanternFixed = false;

        private readonly Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private float windNextStrength = 1f;
        private readonly float windStep = 0.001f;
        private float windDuration = 0;

        private UIPanel fadePanel;
        private Sprite panel;
        private UITextArea title = null;
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

        private readonly Agent agent = new()
        {
            Name = "Soldier",
            MaxSlope = 45,
        };

        private readonly Dictionary<string, AnimationPlan> animations = new();

        private UITextureRenderer bufferDrawer = null;

        private bool uiReady = false;
        private bool gameReady = false;

        private bool udaptingGraph = false;

        public HeightmapScene(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Camera.SetPosition(new Vector3(10000, 10000, 10000));
            Camera.SetInterest(new Vector3(10001, 10000, 10000));

            LoadingTaskUI();
        }

        private void LoadingTaskUI()
        {
            LoadResourcesAsync(
                new[]
                {
                    InitializeTweener(),
                    InitializeUIAssets()
                },
                LoadingTaskUICompleted);
        }
        private async Task InitializeTweener()
        {
            await AddComponent(new Tweener(this, "Tweener", "Tweener"), SceneObjectUsages.None, 0);
            uiTweener = this.AddUIControlTweener();
        }
        private async Task InitializeUIAssets()
        {
            #region Cursor

            var cursorDesc = UICursorDescription.Default("SceneHeightmap/Resources/target.png", 20, 20, true, Color.Red);
            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc);

            #endregion

            #region Fade panel

            fadePanel = await AddComponentUI<UIPanel, UIPanelDescription>("FadePanel", "FadePanel", UIPanelDescription.Screen(this, Color4.Black * 0.3333f), LayerUIEffects);
            fadePanel.Visible = false;

            #endregion

            #region Texts

            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont11 = TextDrawerDescription.FromFamily("Tahoma", 11);
            defaultFont18.LineAdjust = true;
            defaultFont11.LineAdjust = true;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            stats = await AddComponentUI<UITextArea, UITextAreaDescription>("Stats", "Stats", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow });
            help2 = await AddComponentUI<UITextArea, UITextAreaDescription>("Help2", "Help2", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Orange });

            title.Text = "Heightmap Terrain test";
            stats.Text = "";
            help.Text = "";
            help2.Text = "";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Background", "Background", spDesc, LayerUI - 1);

            #endregion

            #region Debug

            bufferDrawer = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("DebugBufferDrawer", "DebugBufferDrawer", UITextureRendererDescription.Default());
            bufferDrawer.Visible = false;

            #endregion
        }
        private void LoadingTaskUICompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            uiReady = true;

            fadePanel.BaseColor = Color.Black;
            fadePanel.Visible = true;

            LoadingTaskGameAssets();
        }

        private void LoadingTaskGameAssets()
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

            LoadResourcesAsync(
                loadTasks,
                LoadingTaskGameAssetsCompleted);
        }
        private async Task InitializeRocks()
        {
            var rDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 250,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Rocks", @"boulder.json"),
                StartsVisible = false,
            };
            rocks = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Rocks", "Rocks", rDesc);
        }
        private async Task InitializeTrees()
        {
            var treeDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 200,
                BlendMode = BlendModes.DefaultTransparent,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Trees", @"tree.json"),
                StartsVisible = false,
            };
            trees = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Trees", "Trees", treeDesc);
        }
        private async Task InitializeTrees2()
        {
            var tree2Desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 200,
                BlendMode = BlendModes.DefaultTransparent,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Trees2", @"tree.json"),
                StartsVisible = false,
            };
            trees2 = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Trees2", "Trees2", tree2Desc);
        }
        private async Task InitializeSoldier()
        {
            var sDesc = new ModelDescription()
            {
                TextureIndex = 0,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Soldier", @"soldier_anim2.json"),
                StartsVisible = false,
            };
            soldier = await AddComponentAgent<Model, ModelDescription>("Soldier", "Soldier", sDesc);
        }
        private async Task InitializeTroops()
        {
            var tDesc = new ModelInstancedDescription()
            {
                Instances = 4,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Soldier", @"soldier_anim2.json"),
                StartsVisible = false,
            };
            troops = await AddComponentAgent<ModelInstanced, ModelInstancedDescription>("Troops", "Troops", tDesc);
        }
        private async Task InitializeM24()
        {
            var mDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 3,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/m24", @"m24.json"),
                StartsVisible = false,
            };
            helicopterI = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("M24", "M24", mDesc);
        }
        private async Task InitializeBradley()
        {
            var mDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 5,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Bradley", @"Bradley.json"),
                StartsVisible = false,
            };
            bradleyI = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Bradley", "Bradley", mDesc);
        }
        private async Task InitializeBuildings()
        {
            var mDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 5,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/buildings", @"Affgan1.json"),
                StartsVisible = false,
            };
            buildings = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Affgan buildings", "Affgan buildings", mDesc);
        }
        private async Task InitializeWatchTower()
        {
            var mDesc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Watch Tower", @"Watch Tower.json"),
                StartsVisible = false,
            };
            watchTower = await AddComponentGround<Model, ModelDescription>("Watch Tower", "Watch Tower", mDesc);
        }
        private async Task InitializeContainers()
        {
            var desc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                CullingVolumeType = CullingVolumeTypes.BoxVolume,
                Instances = 5,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/container", "Container.json"),
                StartsVisible = false,
            };
            containers = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Container", "Container", desc);
        }
        private async Task InitializeTorchs()
        {
            var tcDesc = new ModelInstancedDescription()
            {
                Instances = 50,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Scenery/Objects", @"torch.json"),
                StartsVisible = false,
            };
            torchs = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Torchs", "Torchs", tcDesc);
        }
        private async Task InitializeParticles()
        {
            pManager = await AddComponent<ParticleManager, ParticleManagerDescription>("ParticleManager", "ParticleManager", ParticleManagerDescription.Default());

            pFire = ParticleSystemDescription.InitializeFire("SceneHeightmap/Resources/particles", "fire.png", 0.5f);
            pPlume = ParticleSystemDescription.InitializeSmokePlume("SceneHeightmap/Resources/particles", "smoke.png", 0.5f);
            pDust = ParticleSystemDescription.InitializeDust("SceneHeightmap/Resources/particles", "dust.png", 2f);
            pDust.MinHorizontalVelocity = 10f;
            pDust.MaxHorizontalVelocity = 15f;
            pDust.MinVerticalVelocity = 0f;
            pDust.MaxVerticalVelocity = 0f;
            pDust.MinColor = new Color(Color.SandyBrown.ToColor3(), 0.05f);
            pDust.MaxColor = new Color(Color.SandyBrown.ToColor3(), 0.10f);
            pDust.MinEndSize = 2f;
            pDust.MaxEndSize = 20f;
        }
        private async Task InitializeTerrain()
        {
            var hDesc = new HeightmapDescription()
            {
                ContentPath = "SceneHeightmap/Resources/Scenery/Heightmap",
                HeightmapFileName = "desert0hm.bmp",
                ColormapFileName = "desert0cm.bmp",
                CellSize = 15,
                MaximumHeight = 150,
                Textures = new HeightmapTexturesDescription()
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
                    Resolution = 100f,
                },
            };
            var gDesc = GroundDescription.FromHeightmapDescription(hDesc, 5);
            terrain = await AddComponentGround<Terrain, GroundDescription>("Terrain", "Terrain", gDesc);
        }
        private async Task InitializeLensFlare()
        {
            var lfDesc = new LensFlareDescription()
            {
                ContentPath = @"SceneHeightmap/Resources/Scenery/Flare",
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
        }
        private async Task InitializeSkydom()
        {
            var skDesc = new SkyScatteringDescription();
            skydom = await AddComponentSky<SkyScattering, SkyScatteringDescription>("Sky", "Sky", skDesc);
        }
        private async Task InitializeClouds()
        {
            var scDesc = new SkyPlaneDescription()
            {
                ContentPath = "SceneHeightmap/Resources/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                SkyMode = SkyPlaneModes.Perturbed,
                Direction = new Vector2(1, 1),
            };
            await AddComponentSky<SkyPlane, SkyPlaneDescription>("Clouds", "Clouds", scDesc);
        }
        private async Task InitializeDebugAssets()
        {
            bboxesDrawer = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DEBUG++ Terrain nodes bounding boxes",
                "DEBUG++ Terrain nodes bounding boxes",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Dynamic = true,
                    Count = 50000,
                    StartsVisible = false,
                });

            bboxesTriDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "DEBUG++ Terrain nodes bounding boxes faces",
                "DEBUG++ Terrain nodes bounding boxes faces",
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Count = 1000,
                    StartsVisible = false,
                });

            linesDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DEBUG++ Lines drawer",
                "DEBUG++ Lines drawer",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 1000,
                    StartsVisible = false,
                });

            lightsVolumeDrawer = await AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DEBUG++ Light Volumes",
                "DEBUG++ Light Volumes",
                new PrimitiveListDrawerDescription<Line3D>()
                {
                    Count = 10000,
                    StartsVisible = false,
                });

            graphDrawer = await AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "DEBUG++ Graph",
                "DEBUG++ Graph",
                new PrimitiveListDrawerDescription<Triangle>()
                {
                    Count = 50000,
                });
        }
        private void LoadingTaskGameAssetsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                stats.Text = res.GetErrorMessage();

                return;
            }

            skydom.RayleighScattering *= 0.8f;
            skydom.MieScattering *= 0.1f;

            GameEnvironment.TimeOfDay.BeginAnimation(8, 55, 00);

            Lights.BaseFogColor = new Color((byte)95, (byte)147, (byte)233) * 0.5f;
            ToggleFog();

            Camera.NearPlaneDistance = near;
            Camera.FarPlaneDistance = far;
            Camera.SetPosition(new Vector3(24, 12, 14));
            Camera.SetInterest(new Vector3(0, 10, 0));
            Camera.MovementDelta = 45f;
            Camera.SlowMovementDelta = 20f;

            LoadingTaskTerrainObjects();
        }

        private void LoadingTaskTerrainObjects()
        {
            var loadTasks = new[]
            {
                InitializeGardener(),
                InitializeGardener2(),
                SetAnimationDictionaries(),
                SetPositionOverTerrain(),
            };

            LoadResourcesAsync(
                loadTasks,
                LoadingTaskTerrainObjectsCompleted);
        }
        private async Task InitializeGardener()
        {
            var vDesc = new GroundGardenerDescription()
            {
                ContentPath = "SceneHeightmap/Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map.png",
                PlantingArea = gardenerArea,
                CastShadow = ShadowCastingAlgorihtms.All,
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
                    Instances = GroundGardenerPatchInstances.Four,
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
                    Instances = GroundGardenerPatchInstances.Two,
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
                    Instances = GroundGardenerPatchInstances.Default,
                },
            };
            gardener = await AddComponentEffect<GroundGardener, GroundGardenerDescription>("Grass", "Grass", vDesc);
        }
        private async Task InitializeGardener2()
        {
            var vDesc2 = new GroundGardenerDescription()
            {
                ContentPath = "SceneHeightmap/Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map_flowers.png",
                PlantingArea = gardenerArea2,
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
            gardener2 = await AddComponentEffect<GroundGardener, GroundGardenerDescription>("Flowers", "Flowers", vDesc2);
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
                m24_1.AddLoop("roll");
                animations.Add("m24_idle", new AnimationPlan(m24_1));

                var m24_2 = new AnimationPath();
                m24_2.AddLoop("roll", 5);
                animations.Add("m24_roll", new AnimationPlan(m24_2));
            });
        }
        private async Task SetPositionOverTerrain()
        {
            var posRnd = Helper.NewGenerator(1024);

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
                if (!GetRandomPoint(posRnd, Vector3.Zero, bbox, out var pos))
                {
                    continue;
                }

                if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    float scale = i switch
                    {
                        < 5 => posRnd.NextFloat(10f, 30f),
                        < 30 => posRnd.NextFloat(2f, 5f),
                        _ => posRnd.NextFloat(0.1f, 1f),
                    };

                    rocks[i].Manipulator.SetPosition(r.Position);
                    rocks[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi));
                    rocks[i].Manipulator.SetScaling(scale);
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetForestPosition(Random posRnd)
        {
            BoundingBox bbox = new(new Vector3(-400, 0, -400), new Vector3(-1000, 1000, -1000));

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                if (!GetRandomPoint(posRnd, Vector3.Zero, bbox, out var pos))
                {
                    trees[i].Visible = false;

                    continue;
                }

                if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    var treePosition = r.Position;
                    treePosition.Y -= posRnd.NextFloat(1f, 5f);

                    trees[i].Manipulator.SetPosition(treePosition);
                    trees[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.5f, MathUtil.PiOverFour * 0.5f), 0);
                    trees[i].Manipulator.SetScaling(posRnd.NextFloat(1.5f, 2.5f));
                }
            }

            bbox = new BoundingBox(new Vector3(-300, 0, -300), new Vector3(-1000, 1000, -1000));

            for (int i = 0; i < trees2.InstanceCount; i++)
            {
                if (!GetRandomPoint(posRnd, Vector3.Zero, bbox, out var pos))
                {
                    trees2[i].Visible = false;

                    continue;
                }

                if (FindTopGroundPosition(pos.X, pos.Z, out PickingResult<Triangle> r))
                {
                    var treePosition = r.Position;
                    treePosition.Y -= posRnd.NextFloat(0f, 2f);

                    trees2[i].Manipulator.SetPosition(treePosition);
                    trees2[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(-MathUtil.PiOverFour * 0.15f, MathUtil.PiOverFour * 0.15f), 0);
                    trees2[i].Manipulator.SetScaling(posRnd.NextFloat(1.5f, 2.5f));
                }
            }

            await Task.CompletedTask;
        }
        private async Task SetWatchTowerPosition()
        {
            if (FindTopGroundPosition(-40, -40, out PickingResult<Triangle> r))
            {
                watchTower.Manipulator.SetPosition(r.Position);
                watchTower.Manipulator.SetRotation(MathUtil.Pi / 3f, 0, 0);
                watchTower.Manipulator.SetScaling(1.5f);
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

                    containers[i].Manipulator.SetScaling(5);
                    containers[i].Manipulator.SetPosition(pos);
                    containers[i].Manipulator.SetRotation(MathUtil.Pi / 16f * (i - 2), 0, 0);
                    containers[i].Manipulator.SetNormal(res.Primitive.Normal);
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

                torchs[0].Manipulator.SetScaling(1f, 1f, 1f);
                torchs[0].Manipulator.SetPosition(position);
                var tbbox = torchs[0].GetBoundingBox();

                position.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y);

                spotLight1 = new SceneLightSpot(
                    "Red Spot",
                    true,
                    Color.Red.RGB(),
                    Color.Red.RGB(),
                    true,
                    SceneLightSpotDescription.Create(position, Vector3.Normalize(Vector3.One * -1f), 25, 25, 100));

                spotLight2 = new SceneLightSpot(
                    "Blue Spot",
                    true,
                    Color.Blue.RGB(),
                    Color.Blue.RGB(),
                    true,
                    SceneLightSpotDescription.Create(position, Vector3.Normalize(Vector3.One * -1f), 25, 25, 100));

                Lights.Add(spotLight1);
                Lights.Add(spotLight2);
            }

            SceneLightPoint[] torchLights = new SceneLightPoint[torchs.InstanceCount - 1];
            for (int i = 1; i < torchs.InstanceCount; i++)
            {
                var color = new Color3(
                    posRnd.NextFloat(0, 1),
                    posRnd.NextFloat(0, 1),
                    posRnd.NextFloat(0, 1));

                var position = new Vector3(
                    posRnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                    0f,
                    posRnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                FindTopGroundPosition(position.X, position.Z, out PickingResult<Triangle> res);

                var pos = res.Position;
                torchs[i].Manipulator.SetScaling(0.20f);
                torchs[i].Manipulator.SetPosition(pos);
                var tbbox = torchs[i].GetBoundingBox();

                pos.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                torchLights[i - 1] = new SceneLightPoint(
                    string.Format("Torch {0}", i),
                    true,
                    color,
                    color,
                    true,
                    SceneLightPointDescription.Create(pos, 4f, 5f));

                Lights.Add(torchLights[i - 1]);

                await pManager.AddParticleSystem(ParticleSystemTypes.CPU, pFire, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.1f });
                await pManager.AddParticleSystem(ParticleSystemTypes.CPU, pPlume, new ParticleEmitter() { Position = pos, InfiniteDuration = true, EmissionRate = 0.5f });
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
                    helicopterI[i].Manipulator.SetScaling(1.25f);
                    helicopterI[i].Manipulator.SetPosition(r.Position);
                    helicopterI[i].Manipulator.SetRotation(hPositions[i].Z, 0, 0);
                    helicopterI[i].Manipulator.SetNormal(r.Primitive.Normal);

                    helicopterI[i].AnimationController.TimeDelta = 0.5f * (i + 1);
                    helicopterI[i].AnimationController.Start(animations["m24_roll"]);
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
                    bradleyI[i].Manipulator.SetScaling(1.2f);
                    bradleyI[i].Manipulator.SetPosition(r.Position);
                    bradleyI[i].Manipulator.SetRotation(bPositions[i].Z, 0, 0);
                    bradleyI[i].Manipulator.SetNormal(r.Primitive.Normal);
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
                    buildings[i].Manipulator.SetScaling(3f);
                    buildings[i].Manipulator.SetPosition(r.Position);
                    buildings[i].Manipulator.SetRotation(bPositions[i].Z, 0, 0);
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
                soldier.Manipulator.SetPosition(res.Last().Position);
                soldier.Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            }

            soldier.AnimationController.Start(animations["soldier_idle"]);

            await Task.CompletedTask;
        }
        private async Task SetSoldiersPosition()
        {
            Vector3[] iPos = new Vector3[]
            {
                new (4, -2, MathUtil.PiOverFour),
                new (5, -5, MathUtil.PiOverTwo),
                new (-4, -2, -MathUtil.PiOverFour),
                new (-5, -5, -MathUtil.PiOverTwo),
            };

            for (int i = 0; i < 4; i++)
            {
                if (FindTopGroundPosition(iPos[i].X, iPos[i].Y, out PickingResult<Triangle> r))
                {
                    troops[i].Manipulator.SetPosition(r.Position);
                    troops[i].Manipulator.SetRotation(iPos[i].Z, 0, 0);
                    troops[i].TextureIndex = 1;

                    troops[i].AnimationController.TimeDelta = (i + 1) * 0.2f;
                    troops[i].AnimationController.Start(animations["soldier_idle"], Helper.RandomGenerator.NextFloat(0f, 8f));
                }
            }

            await Task.CompletedTask;
        }
        private void LoadingTaskTerrainObjectsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                stats.Text = res.GetErrorMessage();

                return;
            }

            var lanternDesc = SceneLightSpotDescription.Create(Camera.Position, Camera.Direction, 25f, 100, 10000);
            lantern = new SceneLightSpot("lantern", true, Color3.White, Color3.White, false, lanternDesc);
            Lights.Add(lantern);

            SetDebugInfo();

            SetPathFindingInfo();
        }
        private void SetDebugInfo()
        {
            var terrainBoxes = terrain.GetBoundingBoxes(5);
            var terrainBoxesDesc = GeometryUtil.CreateBoxes(Topology.LineList, terrainBoxes);
            var listBoxes = Line3D.CreateFromVertices(terrainBoxesDesc);
            bboxesDrawer.AddPrimitives(new Color4(1.0f, 0.0f, 0.0f, 0.55f), listBoxes);

            var a1Lines = Line3D.CreateFromVertices(GeometryUtil.CreateBox(Topology.LineList, gardenerArea.Value));
            bboxesDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.55f), a1Lines);
            var a2Lines = Line3D.CreateFromVertices(GeometryUtil.CreateBox(Topology.LineList, gardenerArea2.Value));
            bboxesDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.55f), a2Lines);

            var tris1 = Triangle.ComputeTriangleList(Topology.TriangleList, gardenerArea.Value);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.35f), tris1);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 1.0f, 0.0f, 0.35f), Triangle.ReverseNormal(tris1));
            var tris2 = Triangle.ComputeTriangleList(Topology.TriangleList, gardenerArea2.Value);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.35f), tris2);
            bboxesTriDrawer.AddPrimitives(new Color4(0.0f, 0.0f, 1.0f, 0.35f), Triangle.ReverseNormal(tris2));
        }
        private void SetPathFindingInfo()
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

            EnqueueNavigationGraphUpdate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!uiReady)
            {
                return;
            }

            if (!gameReady)
            {
                return;
            }

            //Input driven
            UpdateInput(gameTime);

            //Auto
            UpdateState(gameTime);
        }

        private void UpdateInput(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateCamera(gameTime);
            UpdatePlayer();
            UpdateInputDebugInfo(gameTime);
            UpdateInputBuffers();
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

            if (playerFlying)
            {
                return;
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
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                float amount = Game.Input.MouseXDelta;

                soldier.Manipulator.Rotate(gameTime, amount, 0, 0);
            }
#else
            float amount = Game.Input.MouseXDelta;

            soldier.Manipulator.Rotate(gameTime, amount, 0, 0);
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
                    var eyePos = new Vector3(0, agent.Height * 0.8f, 0);
                    var offset = (eyePos * 1.25f) - (Vector3.Right * 1.25f) - (Vector3.BackwardLH * 10f);
                    var view = soldier.Manipulator.Forward;

                    Camera.Following = new CameraFollower(soldier.Manipulator, offset, view, 10f);
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
                    var center = t.Manipulator.Position + (Vector3.Up * bbox.Height * 0.5f);
                    var bc = new BoundingCylinder(center, 1.5f, bbox.Height);

                    NavigationGraph.AddObstacle(bc);
                }
            }
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
                GameEnvironment.TimeOfDay.SetTimeOfDay(time % 1f);
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                time += gameTime.ElapsedSeconds * 0.1f;
                GameEnvironment.TimeOfDay.SetTimeOfDay(time % 1f);
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
                    bufferDrawer.Channel = ColorChannels.Red;

                    if (Game.Input.ShiftPressed)
                    {
                        uint tIndex = bufferDrawer.TextureIndex;

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

        private void UpdateState(GameTime gameTime)
        {
            stats.Text = Game.RuntimeText;

            help.Text = string.Format(
                "{0}. Wind {1} {2:0.000} - Next {3:0.000}; {4} Light brightness: {5:0.00}; CamPos {6}; CamDir {7};",
                Renderer,
                windDirection, windStrength, windNextStrength,
                GameEnvironment.TimeOfDay,
                Lights.KeyLight.Brightness,
                Camera.Position, Camera.Direction);

            var counters = FrameCounters.PickCounters;

            help2.Text = string.Format("Picks: {0:0000}|{1:00.000}|{2:00.0000000}; Frustum tests: {3:000}|{4:00.000}|{5:00.00000000}",
                counters?.PicksPerFrame, counters?.PickingTotalTimePerFrame, counters?.PickingAverageTime,
                counters?.VolumeFrustumTestPerFrame, counters?.VolumeFrustumTestTotalTimePerFrame, counters?.VolumeFrustumTestAverageTime);

            UpdateLights();
            UpdateWind(gameTime);
            UpdateDust(gameTime);
            UpdateDrawers();
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

            gardener?.SetWind(windDirection, windStrength);
            gardener2?.SetWind(windDirection, windStrength);
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
            if (!GetRandomPoint(rnd, Vector3.Zero, bsph, out var pos))
            {
                return;
            }

            var velocity = Vector3.Normalize(bsph.Center + pos);

            var emitter = new ParticleEmitter()
            {
                EmissionRate = 0.01f,
                Duration = 1,
                Position = pos,
                Velocity = velocity,
            };

            pDust.Gravity = (windStrength * windDirection);

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDust, emitter);
        }
        private void ToggleFog()
        {
            Lights.FogStart = Lights.FogStart == 0f ? fogStart : 0f;
            Lights.FogRange = Lights.FogRange == 0f ? fogRange : 0f;
        }
        private void UpdateDrawers()
        {
            if (showSoldierDEBUG)
            {
                UpdateSoldierTris();
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
        private void UpdateSoldierTris()
        {
            var color = new Color(Color.Red.ToColor3(), 0.6f);

            var tris = soldier.GetGeometry();

            if (soldierTris == null)
            {
                var desc = new PrimitiveListDrawerDescription<Triangle>()
                {
                    DepthEnabled = false,
                    Primitives = tris.ToArray(),
                    Color = color
                };
                var t = AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("SoldierTris", "SoldierTris", desc);
                soldierTris = t.ConfigureAwait(true).GetAwaiter().GetResult();
            }
            else
            {
                soldierTris.SetPrimitives(color, tris);
            }

            var bboxes = new[]
            {
                soldier.GetBoundingBox(true),
                troops[0].GetBoundingBox(true),
                troops[1].GetBoundingBox(true),
                troops[2].GetBoundingBox(true),
                troops[3].GetBoundingBox(true),
            };

            var lines = Line3D.CreateFromVertices(GeometryUtil.CreateBoxes(Topology.LineList, bboxes));

            if (soldierLines == null)
            {
                var desc = new PrimitiveListDrawerDescription<Line3D>()
                {
                    Primitives = lines.ToArray(),
                    Color = color
                };
                var t = AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("SoldierLines", "SoldierLines", desc);
                soldierLines = t.ConfigureAwait(true).GetAwaiter().GetResult();
            }
            else
            {
                soldierLines.SetPrimitives(color, lines);
            }
        }
        private void UpdateLightDrawingVolumes()
        {
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
        private void UpdateLightCullingVolumes()
        {
            lightsVolumeDrawer.Clear();

            foreach (var spot in Lights.SpotLights)
            {
                var lines = Line3D.CreateFromVertices(GeometryUtil.CreateSphere(Topology.LineList, spot.BoundingSphere, 12, 5));

                lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in Lights.PointLights)
            {
                var lines = Line3D.CreateFromVertices(GeometryUtil.CreateSphere(Topology.LineList, point.BoundingSphere, 12, 5));

                lightsVolumeDrawer.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = true;
        }

        public override void NavigationGraphLoaded()
        {
            gameReady = true;

            uiTweener.ClearTween(fadePanel);
            uiTweener.TweenAlpha(fadePanel, fadePanel.Alpha, 0, 2000, ScaleFuncs.CubicEaseOut);

            UpdateGraphNodes(agent);
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            if (updatingNodes)
            {
                return;
            }

            updatingNodes = true;

            // Fire and forget
            Task.Run(() =>
            {
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

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            fadePanel.Width = Game.Form.RenderWidth;
            fadePanel.Height = Game.Form.RenderHeight;

            title.SetPosition(Vector2.Zero);
            stats.SetPosition(new Vector2(5, title.Top + title.Height + 3));
            help.SetPosition(new Vector2(5, stats.Top + stats.Height + 3));
            help2.SetPosition(new Vector2(5, help.Top + help.Height + 3));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = help2.Top + help2.Height + 3;

            bufferDrawer.Width = (int)(Game.Form.RenderWidth * 0.33f);
            bufferDrawer.Height = (int)(Game.Form.RenderHeight * 0.33f);
            bufferDrawer.Left = Game.Form.RenderWidth - bufferDrawer.Width;
            bufferDrawer.Top = Game.Form.RenderHeight - bufferDrawer.Height;
        }
    }
}
