using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.Content.FmtObj;
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
        private const string GlowString = "lfGlow.png";
        private const string Flare1String = "lfFlare1.png";
        private const string Flare2String = "lfFlare2.png";
        private const string Flare3String = "lfFlare3.png";

        private const float near = 0.5f;
        private const float far = 3000f;
        private const float fogStart = 500f;
        private const float fogRange = 500f;

        private float time = 0.23f;

        private UIControlTweener uiTweener;

        private bool playerFlying = true;
        private SceneLightSpot lantern = null;

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

        private PrimitiveListDrawer<Triangle> bboxesTriDrawer = null;
        private PrimitiveListDrawer<Line3D> bboxesDrawer = null;
        private PrimitiveListDrawer<Line3D> lightsDrawer = null;
        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;

        private SkyScattering skydom = null;

        private Terrain terrain = null;
        private bool showTerrainDEBUG = false;
        private bool terrainInitializedDEBUG = false;

        private Foliage fGrass = null;
        private Foliage fFlowers = null;
        private const float areaSize = 512;
        private readonly BoundingBox? fGrassArea = new BoundingBox(new Vector3(-areaSize * 2, -areaSize, -areaSize), new Vector3(0, areaSize, areaSize));
        private readonly BoundingBox? fFlowersArea = new BoundingBox(new Vector3(0, -areaSize, -areaSize), new Vector3(areaSize * 2, areaSize, areaSize));
        private bool showFoliageDEBUG = false;

        private BoundingFrustum cameraFrustum;
        private bool showCameraFrustumDEBUG = false;

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
        private Color4 soldierColorDEBUG = new(Color.Orange.ToColor3(), 0.6f);

        private ModelInstanced troops = null;

        private ModelInstanced helicopterI = null;
        private ModelInstanced bradleyI = null;
        private ModelInstanced buildings = null;
        private Model watchTower = null;
        private ModelInstanced containers = null;

        private bool drawLightsDEBUG = false;
        private bool drawLightsVolumesDEBUG = false;

        private PrimitiveListDrawer<Triangle> graphDrawer = null;
        private bool updatingNodes = false;

        private readonly GraphAgentType agent = new()
        {
            Name = "Soldier",
            MaxSlope = 45,
        };

        private readonly Dictionary<string, AnimationPlan> animations = [];

        private UITextureRenderer bufferDrawer = null;

        private bool uiReady = false;
        private bool gameReady = false;

        private bool udaptingGraph = false;

        public HeightmapScene(Game game)
            : base(game)
        {
            SetMouse(false);
        }
        private void SetMouse(bool visible)
        {
            if (visible)
            {
#if DEBUG
                Game.VisibleMouse = true;
                Game.LockMouse = false;
#else
                Game.VisibleMouse = true;
                Game.LockMouse = false;
#endif
            }
            else
            {
#if DEBUG
                Game.VisibleMouse = false;
                Game.LockMouse = false;
#else
                Game.VisibleMouse = false;
                Game.LockMouse = true;
#endif
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Camera.SetPosition(new Vector3(10000, 10000, 10000));
            Camera.SetInterest(new Vector3(10001, 10000, 10000));

            LoadingTaskUI();
        }

        private void LoadingTaskUI()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeTweener,
                    InitializeUIAssets,
                ],
                LoadingTaskUICompleted);

            LoadResources(group);
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
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeRocks,
                    InitializeTrees,
                    InitializeTrees2,
                    InitializeSoldier,
                    InitializeTroops,
                    InitializeM24,
                    InitializeBradley,
                    InitializeBuildings,
                    InitializeWatchTower,
                    InitializeContainers,
                    InitializeTorchs,
                    InitializeTerrain,
                    InitializeLensFlare,
                    InitializeSkydom,
                    InitializeClouds,
                    InitializeParticles,
                    InitializeDebugAssets,
                ],
                LoadingTaskGameAssetsCompleted);

            LoadResources(group);
        }
        private async Task InitializeRocks()
        {
            var rDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                PathFindingHull = PickingHullTypes.Hull,
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
                PathFindingHull = PickingHullTypes.Hull,
                Instances = 200,
                BlendMode = BlendModes.OpaqueTransparent,
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
                PathFindingHull = PickingHullTypes.Hull,
                Instances = 200,
                BlendMode = BlendModes.OpaqueTransparent,
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
                PathFindingHull = PickingHullTypes.Fast,
                Instances = 3,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/m24", @"m24.json"),
                StartsVisible = false,
            };
            helicopterI = await AddComponent<ModelInstanced, ModelInstancedDescription>("M24", "M24", mDesc);
        }
        private async Task InitializeBradley()
        {
            var mDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                PathFindingHull = PickingHullTypes.Fast,
                Instances = 5,
                Content = ContentDescription.FromFile(@"SceneHeightmap/Resources/Bradley", @"Bradley.json"),
                StartsVisible = false,
            };
            bradleyI = await AddComponent<ModelInstanced, ModelInstancedDescription>("Bradley", "Bradley", mDesc);
        }
        private async Task InitializeBuildings()
        {
            var mDesc = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                PathFindingHull = PickingHullTypes.Default,
                BlendMode = BlendModes.OpaqueAlpha,
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
                PathFindingHull = PickingHullTypes.Default,
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
                PathFindingHull = PickingHullTypes.Fast,
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
            torchs = await AddComponent<ModelInstanced, ModelInstancedDescription>("Torchs", "Torchs", tcDesc);
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
                    NormalMaps = ["normal001.dds", "normal002.dds"],

                    UseAlphaMapping = true,
                    AlphaMap = "alpha001.dds",
                    ColorTextures = ["dirt001.dds", "dirt002.dds", "dirt004.dds", "stone001.dds"],

                    UseSlopes = false,
                    SlopeRanges = new Vector2(0.005f, 0.25f),
                    TexturesLR = ["dirt0lr.dds", "dirt1lr.dds", "dirt2lr.dds"],
                    TexturesHR = ["dirt0hr.dds"],

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
                GlowTexture = GlowString,
                Flares =
                [
                    new (-0.5f, 0.7f, new Color( 50,  25,  50), Flare1String),
                    new ( 0.3f, 0.4f, new Color(100, 255, 200), Flare1String),
                    new ( 1.2f, 1.0f, new Color(100,  50,  50), Flare1String),
                    new ( 1.5f, 1.5f, new Color( 50, 100,  50), Flare1String),

                    new (-0.3f, 0.7f, new Color(200,  50,  50), Flare2String),
                    new ( 0.6f, 0.9f, new Color( 50, 100,  50), Flare2String),
                    new ( 0.7f, 0.4f, new Color( 50, 200, 200), Flare2String),

                    new (-0.7f, 0.7f, new Color( 50, 100,  25), Flare3String),
                    new ( 0.0f, 0.6f, new Color( 25,  25,  25), Flare3String),
                    new ( 2.0f, 1.4f, new Color( 25,  50, 100), Flare3String),
                ]
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
            bboxesDrawer = await AddComponentUI<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DEBUG++ bounding boxes",
                "DEBUG++ bounding boxes",
                new() { Count = 50000, StartsVisible = false, }, LayerUI - 1);

            bboxesTriDrawer = await AddComponentUI<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "DEBUG++ bounding boxes faces",
                "DEBUG++ bounding boxes faces",
                new() { Count = 1000, DepthEnabled = true, StartsVisible = false, }, LayerUI - 1);

            lightsDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DEBUG++ Lights",
                "DEBUG++ Lights",
                new() { Count = 50000, StartsVisible = false, DepthEnabled = true }, LayerEffects + 1);

            lightsVolumeDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DEBUG++ Light Volumes",
                "DEBUG++ Light Volumes",
                new() { Count = 50000, StartsVisible = false, }, LayerEffects + 1);

            graphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "DEBUG++ Graph",
                "DEBUG++ Graph",
                new() { Count = 50000, }, LayerEffects + 1);
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
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeGrass,
                    InitializeFlowers,
                    SetAnimationDictionaries,
                    SetPositionOverTerrain,
                ],
                LoadingTaskTerrainObjectsCompleted);

            LoadResources(group);
        }
        private async Task InitializeGrass()
        {
            var vDesc = new FoliageDescription()
            {
                ContentPath = "SceneHeightmap/Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map.png",
                PlantingArea = fGrassArea,

                BlendMode = BlendModes.Opaque,

                ColliderType = ColliderTypes.None,
                PathFindingHull = PickingHullTypes.None,
                PickingHull = PickingHullTypes.None,
                CullingVolumeType = CullingVolumeTypes.None,
                CastShadow = ShadowCastingAlgorihtms.All,

                ChannelRed = new FoliageDescription.Channel()
                {
                    VegetationTextures = ["grass_v.dds"],
                    Density = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 100f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1.5f, 1.5f),
                    Seed = 1,
                    WindEffect = 1f,
                    Instances = GroundGardenerPatchInstances.Four,
                },
                ChannelGreen = new FoliageDescription.Channel()
                {
                    VegetationTextures = ["grass_d.dds"],
                    VegetationNormalMaps = ["grass_n.dds"],
                    Density = 1f,
                    StartRadius = 0f,
                    EndRadius = 100f,
                    MinSize = new Vector2(0.5f, 0.5f),
                    MaxSize = new Vector2(1f, 1f),
                    Seed = 2,
                    WindEffect = 1f,
                    Instances = GroundGardenerPatchInstances.Two,
                },
                ChannelBlue = new FoliageDescription.Channel()
                {
                    VegetationTextures = ["grass1.png"],
                    Density = 0.1f,
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
            fGrass = await AddComponentEffect<Foliage, FoliageDescription>("Grass", "Grass", vDesc);
        }
        private async Task InitializeFlowers()
        {
            var vDesc2 = new FoliageDescription()
            {
                ContentPath = "SceneHeightmap/Resources/Scenery/Foliage/Billboard",
                VegetationMap = "map_flowers.png",
                PlantingArea = fFlowersArea,

                BlendMode = BlendModes.Opaque,

                ColliderType = ColliderTypes.None,
                PathFindingHull = PickingHullTypes.None,
                PickingHull = PickingHullTypes.None,
                CullingVolumeType = CullingVolumeTypes.None,
                CastShadow = ShadowCastingAlgorihtms.All,

                ChannelRed = new FoliageDescription.Channel()
                {
                    VegetationTextures = ["flower0.dds"],
                    Density = 0.5f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f) * 0.3f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 1,
                    WindEffect = 1f,
                },
                ChannelGreen = new FoliageDescription.Channel()
                {
                    VegetationTextures = ["flower1.dds"],
                    Density = 0.5f,
                    StartRadius = 0f,
                    EndRadius = 150f,
                    MinSize = new Vector2(1f, 1f) * 0.3f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 0.5f,
                    Seed = 2,
                    WindEffect = 1f,
                },
                ChannelBlue = new FoliageDescription.Channel()
                {
                    VegetationTextures = ["flower2.dds"],
                    Density = 0.1f,
                    StartRadius = 0f,
                    EndRadius = 140f,
                    MinSize = new Vector2(1f, 1f) * 0.3f,
                    MaxSize = new Vector2(1.5f, 1.5f) * 1f,
                    Seed = 3,
                    WindEffect = 1f,
                },
            };
            fFlowers = await AddComponentEffect<Foliage, FoliageDescription>("Flowers", "Flowers", vDesc2);
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
            Vector3[] iPos =
            [
                new (4, -2, MathUtil.PiOverFour),
                new (5, -5, MathUtil.PiOverTwo),
                new (-4, -2, -MathUtil.PiOverFour),
                new (-5, -5, -MathUtil.PiOverTwo),
            ];

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

            SetPathFindingInfo();
        }
        private void SetPathFindingInfo()
        {
            //Agent
            var sbbox = soldier.GetBoundingBox();
            agent.Radius = MathF.Round(sbbox.Width * 0.5f, 1);
            agent.Height = MathF.Round(sbbox.Height, 1);
            agent.MaxClimb = MathF.Round(sbbox.Height * 0.75f, 1);
            agent.MaxSlope = 45;

            //Navigation settings
            var nmsettings = BuildSettings.Default;

            //Rasterization
            nmsettings.CellSize = MathF.Min(1f, agent.Radius);
            nmsettings.CellHeight = MathF.Min(1f, agent.Height);

            //Partitioning
            nmsettings.PartitionType = SamplePartitionTypes.Watershed;

            //Tiling
            nmsettings.BuildMode = BuildModes.Tiled;
            nmsettings.TileSize = 128;
            nmsettings.UseTileCache = true;
            nmsettings.BuildAllTiles = false;

            var nminput = new InputGeometry(GetTrianglesForNavigationGraph);

            PathFinderDescription = new(nmsettings, nminput, [agent]);

            EnqueueNavigationGraphUpdate();
        }

        public override void Update(IGameTime gameTime)
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

        private void UpdateInput(IGameTime gameTime)
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
        private void UpdateCamera(IGameTime gameTime)
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
        private Vector3 UpdateFlyingCamera(IGameTime gameTime)
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

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, !Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, !Game.Input.ShiftPressed);
            }

            return Camera.Position;
        }
        private Vector3 UpdateWalkingCamera(IGameTime gameTime)
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
        private void UpdateInputDebugInfo(IGameTime gameTime)
        {
            UpdateInputWind();

            UpdateInputDrawers();

            UpdateInputNavigation();

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
                showTerrainDEBUG = !showTerrainDEBUG;

                bboxesDrawer.Visible = bboxesDrawer.Active = showTerrainDEBUG || showFoliageDEBUG;
                bboxesTriDrawer.Visible = bboxesTriDrawer.Active = showTerrainDEBUG || showFoliageDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                showSoldierDEBUG = !showSoldierDEBUG;

                if (soldierTris != null) soldierTris.Visible = showSoldierDEBUG;
                if (soldierLines != null) soldierLines.Visible = showSoldierDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                drawLightsDEBUG = !drawLightsDEBUG;

                lightsDrawer.Active = lightsDrawer.Visible = drawLightsDEBUG;
                lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = drawLightsVolumesDEBUG = false;
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                drawLightsVolumesDEBUG = !drawLightsVolumesDEBUG;

                lightsDrawer.Active = lightsDrawer.Visible = drawLightsDEBUG = false;
                lightsVolumeDrawer.Active = lightsVolumeDrawer.Visible = drawLightsVolumesDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                showFoliageDEBUG = !showFoliageDEBUG;

                bboxesDrawer.Visible = bboxesDrawer.Active = showTerrainDEBUG || showFoliageDEBUG;
                bboxesTriDrawer.Visible = bboxesTriDrawer.Active = showTerrainDEBUG || showFoliageDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.M))
            {
                cameraFrustum = Camera.Frustum;

                showCameraFrustumDEBUG = !showCameraFrustumDEBUG;

                bboxesDrawer.Visible = bboxesDrawer.Active = showCameraFrustumDEBUG;
                bboxesTriDrawer.Visible = bboxesTriDrawer.Active = showCameraFrustumDEBUG;
            }
        }
        private void UpdateInputNavigation()
        {
            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                graphDrawer.Visible = !graphDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F8))
            {
                //Save navigation triangles
                SetMouse(true);

                try
                {
                    System.Windows.Forms.SaveFileDialog dlg = new()
                    {
                        Filter = "obj files (*.obj)|*.obj|All files (*.*)|*.*",
                        FilterIndex = 1,
                        RestoreDirectory = true
                    };

                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        LoaderObj.Save(GetTrianglesForNavigationGraph(), dlg.FileName);
                    }
                }
                finally
                {
                    SetMouse(false);
                }
            }
        }
        private void UpdateInputLights()
        {
            if (Game.Input.KeyJustReleased(Keys.G))
            {
                Lights.KeyLight.CastShadow = !Lights.KeyLight.CastShadow;
            }
        }
        private void UpdateInputFog()
        {
            if (Game.Input.KeyJustReleased(Keys.F))
            {
                ToggleFog();
            }
        }
        private void UpdateInputTimeOfDay(IGameTime gameTime)
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
            if (!Game.Input.KeyJustReleased(Keys.F9))
            {
                return;
            }

            var shadowMap = Renderer.GetResource(SceneRendererResults.ShadowMapDirectional);
            if (shadowMap == null)
            {
                return;
            }

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

        private void UpdateState(IGameTime gameTime)
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

            var x = d * MathF.Cos(v * Game.GameTime.TotalSeconds);
            var z = d * MathF.Sin(v * Game.GameTime.TotalSeconds);

            spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
            spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));

            spotLight1.Enabled = false;
            spotLight2.Enabled = false;

            if (lantern.Enabled)
            {
                lantern.Position = Camera.Position + (Camera.Left * 2);
                lantern.Direction = Camera.Direction;
            }
        }
        private void UpdateWind(IGameTime gameTime)
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

            fGrass?.SetWind(windDirection, windStrength);
            fFlowers?.SetWind(windDirection, windStrength);
        }
        private void UpdateDust(IGameTime gameTime)
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
            Lights.FogStart = MathUtil.IsZero(Lights.FogStart) ? fogStart : 0f;
            Lights.FogRange = MathUtil.IsZero(Lights.FogRange) ? fogRange : 0f;
        }
        private void UpdateDrawers()
        {
            UpdateDebugDrawers();

            if (showSoldierDEBUG)
            {
                UpdateDebugSoldierTris();
            }

            if (drawLightsDEBUG)
            {
                UpdateDebugLightDrawing();
            }

            if (drawLightsVolumesDEBUG)
            {
                UpdateDebugLightCullingVolumes();
            }
        }
        private void UpdateDebugDrawers()
        {
            Color4 terrainColor = new(1.0f, 0.0f, 0.0f, 0.55f);

            Color4 frustumLColor = new(Color.LightPink.ToColor3(), 0.75f);
            Color4 frustumTColor = new(Color.LightPink.ToColor3(), 0.25f);

            lightsVolumeDrawer.Clear();
            lightsVolumeDrawer.Clear();

            if (showTerrainDEBUG)
            {
                UpdateDebugTerrain(terrainColor);
            }

            if (showFoliageDEBUG)
            {
                UpdateDebugGardeners();
            }

            if (showCameraFrustumDEBUG)
            {
                UpdateCameraFrustum(frustumLColor, frustumTColor);
            }
        }
        private void UpdateDebugTerrain(Color4 color)
        {
            if (terrainInitializedDEBUG)
            {
                return;
            }

            var terrainBoxes = terrain.GetBoundingBoxes(5);
            var listBoxes = Line3D.CreateBoxes(terrainBoxes);
            bboxesDrawer.SetPrimitives(color, listBoxes);

            terrainInitializedDEBUG = true;
        }
        private void UpdateCameraFrustum(Color4 lColor, Color4 tColor)
        {
            var lines = Line3D.CreateFrustum(cameraFrustum);
            var tris = GeometryUtil.CreateFrustum(Topology.TriangleList, cameraFrustum);

            bboxesDrawer.SetPrimitives(lColor, lines);
            bboxesTriDrawer.SetPrimitives(tColor, Triangle.ComputeTriangleList(tris.Vertices, tris.Indices));
        }
        private void UpdateDebugGardeners()
        {
            terrainInitializedDEBUG = false;

            if (fGrass.Visible)
            {
                UpdateDebugGardener(fGrass);
            }

            if (fFlowers.Visible)
            {
                UpdateDebugGardener(fFlowers);
            }
        }
        private void UpdateDebugGardener(Foliage g)
        {
            float minY = float.MaxValue;

            var nodes = g.GetVisibleNodes();
            for (int i = 0; i < nodes.Length; i++)
            {
                //Adjust box
                var box = nodes[i].BoundingBox;
                var vedges = box.GetEdges().Skip(8).ToArray();
                if (!FindTopGroundPosition<Triangle>(vedges[0].Point1.X, vedges[0].Point1.Z, out var p0)) continue;
                if (!FindTopGroundPosition<Triangle>(vedges[1].Point1.X, vedges[1].Point1.Z, out var p1)) continue;
                if (!FindTopGroundPosition<Triangle>(vedges[2].Point1.X, vedges[2].Point1.Z, out var p2)) continue;
                if (!FindTopGroundPosition<Triangle>(vedges[3].Point1.X, vedges[3].Point1.Z, out var p3)) continue;
                var abox = SharpDX.BoundingBox.FromPoints([p0.Position, p1.Position, p2.Position, p3.Position]);

                var colL = Helper.IntToCol(nodes[i].Id, 128);
                var colT = Helper.IntToCol(nodes[i].Id, 64);

                var lines = Line3D.CreateBox(abox);
                bboxesDrawer.AddPrimitives(colL, lines);

                var tris = Triangle.ComputeTriangleList(abox);
                bboxesTriDrawer.AddPrimitives(colT, tris);
                bboxesTriDrawer.AddPrimitives(colT, Triangle.ReverseNormal(tris));

                minY = Math.Min(minY, abox.Minimum.Y);
            }

            //Adjust gbox
            var gbbox = g.GetPlantingBounds();
            gbbox.Minimum.Y = minY;
            var col = Helper.IntToCol(0, 128);
            var glines = Line3D.CreateBox(gbbox);
            bboxesDrawer.AddPrimitives(col, glines);
        }
        private void UpdateDebugSoldierTris()
        {
            var tris = soldier.GetGeometry();

            if (soldierTris == null)
            {
                var desc = new PrimitiveListDrawerDescription<Triangle>()
                {
                    DepthEnabled = false,
                    Primitives = tris.ToArray(),
                    Color = soldierColorDEBUG
                };
                var t = AddComponent<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("SoldierTris", "SoldierTris", desc);
                soldierTris = t.ConfigureAwait(true).GetAwaiter().GetResult();
            }
            else
            {
                soldierTris.SetPrimitives(soldierColorDEBUG, tris);
            }

            var bboxes = new[]
            {
                soldier.GetBoundingBox(true),
                troops[0].GetBoundingBox(true),
                troops[1].GetBoundingBox(true),
                troops[2].GetBoundingBox(true),
                troops[3].GetBoundingBox(true),
            };

            var lines = Line3D.CreateBoxes(bboxes);

            if (soldierLines == null)
            {
                var desc = new PrimitiveListDrawerDescription<Line3D>()
                {
                    Primitives = lines.ToArray(),
                    Color = soldierColorDEBUG
                };
                var t = AddComponent<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("SoldierLines", "SoldierLines", desc);
                soldierLines = t.ConfigureAwait(true).GetAwaiter().GetResult();
            }
            else
            {
                soldierLines.SetPrimitives(soldierColorDEBUG, lines);
            }
        }
        private void UpdateDebugLightDrawing()
        {
            lightsDrawer.Clear();

            foreach (var spot in Lights.SpotLights)
            {
                var lines = spot.GetVolume(12);

                lightsDrawer.AddPrimitives(new Color4(spot.DiffuseColor, 0.15f), lines);
            }

            foreach (var point in Lights.PointLights)
            {
                var lines = point.GetVolume(6, 12);

                lightsDrawer.AddPrimitives(new Color4(point.DiffuseColor, 0.15f), lines);
            }
        }
        private void UpdateDebugLightCullingVolumes()
        {
            lightsVolumeDrawer.Clear();

            var lColor = new Color4(Color.MediumPurple.RGB(), 0.15f);

            BoundingSphere[] sphList =
            [
                .. Lights.SpotLights.Select(s => s.BoundingSphere).ToArray(),
                .. Lights.PointLights.Select(s => s.BoundingSphere).ToArray(),
            ];

            foreach (var sph in sphList)
            {
                int slices = Math.Clamp((int)(sph.Radius * 3), 3, 10);
                int stacks = Math.Clamp((int)(sph.Radius * 3), 3, 10);

                var lines = Line3D.CreateSphere(sph, slices, stacks);

                lightsVolumeDrawer.AddPrimitives(lColor, lines);
            }
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
                        var color = Helper.IntToCol(node.Id, 128);
                        graphDrawer.AddPrimitives(color, node.Triangles);
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
