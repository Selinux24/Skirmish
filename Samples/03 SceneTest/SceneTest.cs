using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest
{
    public class SceneTest : Scene
    {
        private const int layerHUD = 99;

        private readonly float baseHeight = 0.1f;
        private readonly float spaceSize = 40;

        private readonly float xDelta = 500f;
        private readonly float yDelta = 7f;
        private readonly float zDelta = 0f;

        private UICursor cursor = null;
        private UIButton butClose = null;

        private Sprite spr = null;
        private UITextArea title = null;
        private UITextArea runtime = null;
        private UIPanel blackPan = null;
        private UIProgressBar progressBar = null;
        private float progressValue = 0;

        private Scenery scenery = null;

        private ModelInstanced floorAsphaltI = null;

        private Model buildingObelisk = null;
        private ModelInstanced buildingObeliskI = null;

        private Model characterSoldier = null;
        private ModelInstanced characterSoldierI = null;

        private Model vehicle = null;
        private ModelInstanced vehicleI = null;

        private Model lamp = null;
        private ModelInstanced lampI = null;

        private Model streetlamp = null;
        private ModelInstanced streetlampI = null;

        private Model container = null;
        private ModelInstanced containerI = null;

        private Model tree = null;
        private ModelInstanced treesI = null;

        private SkyPlane skyPlane = null;

        private readonly Dictionary<string, ParticleSystemDescription> pDescriptions = new Dictionary<string, ParticleSystemDescription>();
        private ParticleManager pManager = null;

        private IParticleSystem[] particlePlumes = null;
        private readonly Vector3 plumeGravity = new Vector3(0, 5, 0);
        private readonly float plumeMaxHorizontalVelocity = 25f;
        private Vector2 wind = new Vector2(0, 0);
        private Vector2 nextWind = new Vector2();
        private float nextWindChange = 0;

        private readonly Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private PrimitiveListDrawer<Line3D> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        private bool gameReady = false;

        public SceneTest(Game game) : base(game)
        {

        }

        public override Task Initialize()
        {
            return LoadUserInterface();
        }

        public async Task LoadUserInterface()
        {
            this.Game.GameStatusCollected += GameStatusCollected;
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 2000;
            this.Camera.SlowMovementDelta = 100f;
            this.Camera.MovementDelta = 500f;

            await this.LoadResourcesAsync(
                InitializeUI(),
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    this.RefreshUI();

                    progressBar.Visible = true;
                    progressBar.ProgressValue = 0;

                    await LoadControls();
                });
        }
        private async Task InitializeUI()
        {
            var titleDesc = UITextAreaDescription.FromFamily("Tahoma", 18);
            titleDesc.Font.TextColor = Color.Yellow;
            titleDesc.Font.ShadowColor = Color.Orange;

            this.title = await this.AddComponentUITextArea(titleDesc, layerHUD);
            this.title.Text = "Scene Test - Textures";
            this.title.SetPosition(Vector2.Zero);

            var runtimeDesc = UITextAreaDescription.FromFamily("Tahoma", 10);
            runtimeDesc.Font.TextColor = Color.Yellow;
            runtimeDesc.Font.ShadowColor = Color.Orange;

            this.runtime = await this.AddComponentUITextArea(runtimeDesc, layerHUD);
            this.runtime.Text = "";
            this.runtime.SetPosition(new Vector2(5, this.title.Top + this.title.Height + 3));

            this.spr = await this.AddComponentSprite(new SpriteDescription()
            {
                Width = this.Game.Form.RenderWidth,
                Height = this.runtime.Top + this.runtime.Height + 3,
                TintColor = new Color4(0, 0, 0, 0.75f),
            }, SceneObjectUsages.UI, layerHUD - 1);

            var buttonFont = TextDrawerDescription.FromFamily("Lucida Console", 12, Color.Yellow, Color.Orange);
            buttonFont.HorizontalAlign = HorizontalTextAlign.Center;
            buttonFont.VerticalAlign = VerticalTextAlign.Middle;
            var buttonDesc = UIButtonDescription.DefaultTwoStateButton("SceneTest/UI/button_on.png", "SceneTest/UI/button_off.png", buttonFont);
            buttonDesc.Width = 100;
            buttonDesc.Height = 40;
            buttonDesc.Text = "Close";

            this.butClose = await this.AddComponentUIButton(buttonDesc, layerHUD);
            this.butClose.JustReleased += (sender, eventArgs) => { this.Game.SetScene<SceneStart>(); };
            this.butClose.Visible = false;

            this.blackPan = await this.AddComponentUIPanel(new UIPanelDescription
            {
                Background = new SpriteDescription
                {
                    TintColor = Color.Black,
                },
                Left = 0,
                Top = 0,
                Width = this.Game.Form.RenderWidth,
                Height = this.Game.Form.RenderHeight,
            }, layerHUD + 1);

            var pbDesc = UIProgressBarDescription.DefaultFromFamily("Consolas", 18);
            pbDesc.Name = "Progress Bar";
            pbDesc.Top = this.Game.Form.RenderHeight - 60;
            pbDesc.Left = 100;
            pbDesc.Width = this.Game.Form.RenderWidth - 200;
            pbDesc.Height = 30;
            pbDesc.BaseColor = new Color(0, 0, 0, 0.5f);
            pbDesc.ProgressColor = Color.Green;

            this.progressBar = await this.AddComponentUIProgressBar(pbDesc, layerHUD + 2);

            var cursorDesc = UICursorDescription.Default("Common/pointer.png", 48, 48, new Vector2(-14, -6));
            cursorDesc.Name = "Cursor";

            this.cursor = await this.AddComponentUICursor(cursorDesc, layerHUD * 2);
            this.cursor.Visible = false;
        }

        private async Task LoadControls()
        {
            var taskList = new Task[]
            {
                InitializeSkyEffects(),
                InitializeScenery(),
                InitializeTrees(),
                InitializeFloorAsphalt(),
                InitializeBuildingObelisk(),
                InitializeCharacterSoldier(),
                InitializeVehicles(),
                InitializeLamps(),
                InitializeStreetLamps(),
                InitializeContainers(),
                InitializeTestCube(),
                InitializeParticles(),
                InitializaDebug(),
            };

            await this.LoadResourcesAsync(taskList, async (res) =>
            {
                if (!res.Completed)
                {
                    res.ThrowExceptions();
                }

                this.PlantTrees();

                this.Environment.TimeOfDay.BeginAnimation(9, 00, 00, 0.1f);

                this.Camera.Goto(-20 + xDelta, 10 + yDelta, -40f + zDelta);
                this.Camera.LookTo(0 + xDelta, 0 + yDelta, 0 + zDelta);

                this.blackPan.Hide(4000);
                this.progressBar.Hide(2000);

                await Task.Delay(1000);

                this.gameReady = true;
            });
        }
        private async Task InitializeSkyEffects()
        {
            await this.AddComponentLensFlare(new LensFlareDescription()
            {
                Name = "Flares",
                ContentPath = @"Common/lensFlare",
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
            });

            await this.AddComponentSkyScattering(new SkyScatteringDescription() { Name = "Sky" });

            this.skyPlane = await this.AddComponentSkyPlane(new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "SceneTest/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                SkyMode = SkyPlaneModes.Perturbed,
            });
        }
        private async Task InitializeScenery()
        {
            scenery = await this.AddComponentScenery(GroundDescription.FromFile("SceneTest/scenery", "Clif.xml"));
        }
        private async Task InitializeTrees()
        {
            tree = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Tree",
                    CastShadow = true,
                    SphericVolume = false,
                    UseAnisotropicFiltering = true,
                    BlendMode = BlendModes.DefaultTransparent,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/Trees",
                        ModelContentFilename = "Tree.xml",
                    }
                });

            var desc = new ModelInstancedDescription()
            {
                Name = "TreeI",
                CastShadow = true,
                SphericVolume = false,
                UseAnisotropicFiltering = true,
                BlendMode = BlendModes.DefaultTransparent,
                Instances = 50,
                Content = new ContentDescription()
                {
                    ContentFolder = "SceneTest/Trees",
                    ModelContentFilename = "Tree.xml",
                }
            };

            treesI = await this.AddComponentModelInstanced(desc);
        }
        private async Task InitializeFloorAsphalt()
        {
            float l = spaceSize;
            float h = baseHeight;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 0.0f) },
                new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            MaterialContent mat = MaterialContent.Default;
            mat.DiffuseTexture = "SceneTest/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneTest/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneTest/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Name = "Floor",
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
                SphericVolume = false,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            var descI = new ModelInstancedDescription()
            {
                Name = "FloorI",
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
                SphericVolume = false,
                UseAnisotropicFiltering = true,
                Instances = 8,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            var floorAsphalt = await this.AddComponentModel(desc);
            floorAsphalt.Manipulator.SetPosition(xDelta, yDelta, zDelta);

            this.floorAsphaltI = await this.AddComponentModelInstanced(descI);

            this.floorAsphaltI[0].Manipulator.SetPosition((-l * 2) + xDelta, yDelta, 0 + zDelta);
            this.floorAsphaltI[1].Manipulator.SetPosition((+l * 2) + xDelta, yDelta, 0 + zDelta);
            this.floorAsphaltI[2].Manipulator.SetPosition(0 + xDelta, yDelta, (-l * 2) + zDelta);
            this.floorAsphaltI[3].Manipulator.SetPosition(0 + xDelta, yDelta, (+l * 2) + zDelta);

            this.floorAsphaltI[4].Manipulator.SetPosition((-l * 2) + xDelta, yDelta, (-l * 2) + zDelta);
            this.floorAsphaltI[5].Manipulator.SetPosition((+l * 2) + xDelta, yDelta, (-l * 2) + zDelta);
            this.floorAsphaltI[6].Manipulator.SetPosition((-l * 2) + xDelta, yDelta, (+l * 2) + zDelta);
            this.floorAsphaltI[7].Manipulator.SetPosition((+l * 2) + xDelta, yDelta, (+l * 2) + zDelta);
        }
        private async Task InitializeBuildingObelisk()
        {
            this.buildingObelisk = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Obelisk",
                    CastShadow = true,
                    SphericVolume = false,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });

            this.buildingObeliskI = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "ObeliskI",
                    CastShadow = true,
                    SphericVolume = false,
                    UseAnisotropicFiltering = true,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });

            this.buildingObelisk.Manipulator.SetPosition(0 + xDelta, baseHeight + yDelta, 0 + zDelta);
            this.buildingObelisk.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObelisk.Manipulator.SetScale(10);

            this.buildingObeliskI[0].Manipulator.SetPosition((-spaceSize * 2) + xDelta, baseHeight + yDelta, 0 + zDelta);
            this.buildingObeliskI[1].Manipulator.SetPosition((+spaceSize * 2) + xDelta, baseHeight + yDelta, 0 + zDelta);
            this.buildingObeliskI[2].Manipulator.SetPosition(0 + xDelta, baseHeight + yDelta, (-spaceSize * 2) + zDelta);
            this.buildingObeliskI[3].Manipulator.SetPosition(0 + xDelta, baseHeight + yDelta, (+spaceSize * 2) + zDelta);

            this.buildingObeliskI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.buildingObeliskI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObeliskI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.buildingObeliskI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.buildingObeliskI[0].Manipulator.SetScale(10);
            this.buildingObeliskI[1].Manipulator.SetScale(10);
            this.buildingObeliskI[2].Manipulator.SetScale(10);
            this.buildingObeliskI[3].Manipulator.SetScale(10);
        }
        private async Task InitializeCharacterSoldier()
        {
            this.characterSoldier = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Soldier",
                    TextureIndex = 1,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            this.characterSoldierI = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "SoldierI",
                    CastShadow = true,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            float s = spaceSize / 2f;

            AnimationPath p1 = new AnimationPath();
            p1.AddLoop("idle1");
            this.animations.Add("default", new AnimationPlan(p1));

            this.characterSoldier.Manipulator.SetPosition((+s - 10) + xDelta, baseHeight + yDelta, -s + zDelta);
            this.characterSoldier.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldier.AnimationController.AddPath(this.animations["default"]);
            this.characterSoldier.AnimationController.Start(0);

            this.characterSoldierI[0].Manipulator.SetPosition((-spaceSize * 2 + s) + xDelta, baseHeight + yDelta, -s + zDelta);
            this.characterSoldierI[1].Manipulator.SetPosition((+spaceSize * 2 + s) + xDelta, baseHeight + yDelta, -s + zDelta);
            this.characterSoldierI[2].Manipulator.SetPosition(+s + xDelta, baseHeight + yDelta, (-spaceSize * 2 - s) + zDelta);
            this.characterSoldierI[3].Manipulator.SetPosition(+s + xDelta, baseHeight + yDelta, (+spaceSize * 2 - s) + zDelta);

            this.characterSoldierI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.characterSoldierI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldierI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.characterSoldierI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.characterSoldierI[0].AnimationController.AddPath(this.animations["default"]);
            this.characterSoldierI[1].AnimationController.AddPath(this.animations["default"]);
            this.characterSoldierI[2].AnimationController.AddPath(this.animations["default"]);
            this.characterSoldierI[3].AnimationController.AddPath(this.animations["default"]);

            this.characterSoldierI[0].AnimationController.Start(1);
            this.characterSoldierI[1].AnimationController.Start(2);
            this.characterSoldierI[2].AnimationController.Start(3);
            this.characterSoldierI[3].AnimationController.Start(4);
        }
        private async Task InitializeVehicles()
        {
            this.vehicle = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Challenger",
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/vehicles/Challenger",
                        ModelContentFilename = "Challenger.xml",
                    }
                });

            this.vehicleI = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "LeopardI",
                    CastShadow = true,
                    SphericVolume = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/vehicles/leopard",
                        ModelContentFilename = "Leopard.xml",
                    }
                });

            float s = -spaceSize / 2f;

            this.vehicle.Manipulator.SetPosition(s + xDelta, baseHeight + yDelta, -10 + zDelta);
            this.vehicle.Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);

            this.vehicleI[0].Manipulator.SetPosition(-spaceSize * 2 + xDelta, baseHeight + yDelta, -spaceSize * 2 + zDelta);
            this.vehicleI[1].Manipulator.SetPosition(+spaceSize * 2 + xDelta, baseHeight + yDelta, -spaceSize * 2 + zDelta);
            this.vehicleI[2].Manipulator.SetPosition(-spaceSize * 2 + xDelta, baseHeight + yDelta, +spaceSize * 2 + zDelta);
            this.vehicleI[3].Manipulator.SetPosition(+spaceSize * 2 + xDelta, baseHeight + yDelta, +spaceSize * 2 + zDelta);

            this.vehicleI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.vehicleI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.vehicleI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.vehicleI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            List<ISceneLight> lights = new List<ISceneLight>();

            lights.AddRange(this.vehicle.Lights);

            for (int i = 0; i < this.vehicleI.InstanceCount; i++)
            {
                lights.AddRange(this.vehicleI[i].Lights);
            }

            this.Lights.AddRange(lights);
        }
        private async Task InitializeLamps()
        {
            this.lamp = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Lamp",
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                });

            this.lampI = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "LampI",
                    CastShadow = true,
                    SphericVolume = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                });

            float dist = 0.23f;
            float pitch = MathUtil.DegreesToRadians(165) * -1;

            this.lamp.Manipulator.SetPosition(0 + xDelta, spaceSize + yDelta, (-spaceSize * dist) + zDelta);
            this.lamp.Manipulator.SetRotation(0, pitch, 0);

            this.lampI[0].Manipulator.SetPosition(-spaceSize * 2 + xDelta, spaceSize + yDelta, -spaceSize * dist + zDelta);
            this.lampI[1].Manipulator.SetPosition(+spaceSize * 2 + xDelta, spaceSize + yDelta, -spaceSize * dist + zDelta);
            this.lampI[2].Manipulator.SetPosition(-spaceSize * dist + xDelta, spaceSize + yDelta, -spaceSize * 2 + zDelta);
            this.lampI[3].Manipulator.SetPosition(-spaceSize * dist + xDelta, spaceSize + yDelta, +spaceSize * 2 + zDelta);

            this.lampI[0].Manipulator.SetRotation(0, pitch, 0);
            this.lampI[1].Manipulator.SetRotation(0, pitch, 0);
            this.lampI[2].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);
            this.lampI[3].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);

            List<ISceneLight> lights = new List<ISceneLight>();

            lights.AddRange(this.lamp.Lights);

            for (int i = 0; i < this.lampI.InstanceCount; i++)
            {
                lights.AddRange(this.lampI[i].Lights);
            }

            this.Lights.AddRange(lights);
        }
        private async Task InitializeStreetLamps()
        {
            this.streetlamp = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Street Lamp",
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                });

            this.streetlampI = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Street LampI",
                    CastShadow = true,
                    SphericVolume = false,
                    Instances = 9,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                });

            this.streetlamp.Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -2f + zDelta);

            this.streetlampI[0].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -1f + zDelta);
            this.streetlampI[1].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, 0 + zDelta);
            this.streetlampI[2].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 1f + zDelta);
            this.streetlampI[3].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 2f + zDelta);

            this.streetlampI[4].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -2f + zDelta);
            this.streetlampI[5].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -1f + zDelta);
            this.streetlampI[6].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, 0 + zDelta);
            this.streetlampI[7].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 1f + zDelta);
            this.streetlampI[8].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 2f + zDelta);

            this.streetlampI[4].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[5].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[6].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[7].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[8].Manipulator.SetRotation(MathUtil.Pi, 0, 0);

            List<ISceneLight> lights = new List<ISceneLight>();

            lights.AddRange(this.streetlamp.Lights);

            for (int i = 0; i < this.streetlampI.InstanceCount; i++)
            {
                lights.AddRange(this.streetlampI[i].Lights);
            }

            this.Lights.AddRange(lights);
        }
        private async Task InitializeContainers()
        {
            int xSize = 12;
            int zSize = 17;
            int rows = 5;
            int basementRows = 3;

            int xCount = xSize + 1;
            int zCount = zSize + 1;
            int xRowCount = xCount * 2;
            int zRowCount = zCount * 2;
            int rowSize = xRowCount + zRowCount;
            int instances = rowSize * rows;

            this.container = await this.AddComponentModel(
                new ModelDescription()
                {
                    Name = "Container",
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/container",
                        ModelContentFilename = "Container.xml",
                    }
                });

            this.containerI = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "ContainerI",
                    CastShadow = true,
                    SphericVolume = false,
                    Instances = instances,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/container",
                        ModelContentFilename = "Container.xml",
                    }
                });

            float s = -spaceSize / 2f;
            float areaSize = spaceSize * 3;

            var bboxTmp = this.container.GetBoundingBox();
            float scaleX = areaSize * 2 / xSize / bboxTmp.Width;
            float scaleZ = areaSize * 2 / zSize / bboxTmp.Depth;
            Vector3 scale = new Vector3(scaleX, (scaleX + scaleZ) / 2f, scaleZ);

            this.container.Manipulator.SetScale(scale);
            this.container.Manipulator.UpdateInternals(true);
            var bbox = this.container.GetBoundingBox();
            float sx = bbox.Width;
            float sy = bbox.Height;
            float sz = bbox.Depth;

            this.container.Manipulator.SetPosition(s + 12 + xDelta, baseHeight + yDelta, 30 + zDelta);
            this.container.Manipulator.SetRotation(MathUtil.PiOverTwo * 2.1f, 0, 0);

            Random prnd = new Random(1000);

            for (int i = 0; i < this.containerI.InstanceCount; i++)
            {
                uint textureIndex = (uint)prnd.Next(0, 6);
                textureIndex %= 5;

                float height = (i / rowSize * sy) + baseHeight + yDelta - (sy * basementRows);

                if ((i % rowSize) < zRowCount)
                {
                    float rx = (i % zRowCount < zCount ? -areaSize - (sx / 2f) : areaSize + (sx / 2f)) + prnd.NextFloat(-1f, 1f);
                    float dz = i % zRowCount < zCount ? -(sz / 2f) : (sz / 2f);

                    float x = rx + xDelta;
                    float y = height;
                    float z = (i % zCount * sz) - areaSize + zDelta + dz;
                    float angle = MathUtil.Pi * prnd.Next(0, 2);

                    this.containerI[i].TextureIndex = textureIndex;

                    this.containerI[i].Manipulator.SetPosition(x, y, z);
                    this.containerI[i].Manipulator.SetRotation(angle, 0, 0);
                    this.containerI[i].Manipulator.SetScale(scale);
                }
                else
                {
                    int ci = i - zRowCount;
                    float rz = (ci % xRowCount < xCount ? -areaSize - (sz / 2f) : areaSize + (sz / 2f)) + prnd.NextFloat(-1f, 1f);
                    float dx = ci % xRowCount < xCount ? (sx / 2f) : -(sx / 2f);

                    float x = (ci % xCount * sx) - areaSize + xDelta + dx;
                    float y = height;
                    float z = rz + zDelta;
                    float angle = MathUtil.Pi * prnd.Next(0, 2);

                    this.containerI[i].TextureIndex = textureIndex;

                    this.containerI[i].Manipulator.SetPosition(x, y, z);
                    this.containerI[i].Manipulator.SetRotation(angle, 0, 0);
                    this.containerI[i].Manipulator.SetScale(scale);
                }
            }
        }
        private async Task InitializeTestCube()
        {
            var bbox = new BoundingBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
            var cubeTris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox);
            cubeTris = Triangle.Transform(cubeTris, Matrix.Translation(30 + xDelta, 0.5f + yDelta, 0 + zDelta));

            var desc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "Test Cube",
                Primitives = cubeTris.ToArray(),
                Color = Color.Red,
                DepthEnabled = true,
            };

            await this.AddComponentPrimitiveListDrawer<Triangle>(desc);
        }
        private async Task InitializeParticles()
        {
            var pPlume = ParticleSystemDescription.InitializeSmokePlume("SceneTest/particles", "smoke.png", 10f);
            var pFire = ParticleSystemDescription.InitializeFire("SceneTest/particles", "fire.png", 10f);
            var pDust = ParticleSystemDescription.InitializeDust("SceneTest/particles", "smoke.png", 10f);
            var pProjectile = ParticleSystemDescription.InitializeProjectileTrail("SceneTest/particles", "smoke.png", 10f);
            var pExplosion = ParticleSystemDescription.InitializeExplosion("SceneTest/particles", "fire.png", 10f);
            var pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("SceneTest/particles", "smoke.png", 10f);

            this.pDescriptions.Add("Plume", pPlume);
            this.pDescriptions.Add("Fire", pFire);
            this.pDescriptions.Add("Dust", pDust);
            this.pDescriptions.Add("Projectile", pProjectile);
            this.pDescriptions.Add("Explosion", pExplosion);
            this.pDescriptions.Add("SmokeExplosion", pSmokeExplosion);

            this.pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Manager" });

            float d = 500;
            var positions = new Vector3[]
            {
                new Vector3(+d,0,+d),
                new Vector3(+d,0,-d),
                new Vector3(-d,0,+d),
                new Vector3(-d,0,-d),
            };

            var bbox = new BoundingBox(Vector3.One * -2.5f, Vector3.One * 2.5f);
            var cubeTris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox);

            this.particlePlumes = new IParticleSystem[positions.Length];
            List<Triangle> markers = new List<Triangle>();
            for (int i = 0; i < positions.Length; i++)
            {
                this.particlePlumes[i] = this.pManager.AddParticleSystem(
                    ParticleSystemTypes.CPU,
                    pPlume,
                    new ParticleEmitter()
                    {
                        Position = positions[i],
                        InfiniteDuration = true,
                        EmissionRate = 0.05f,
                        MaximumDistance = 1000f,
                    });

                markers.AddRange(Triangle.Transform(cubeTris, Matrix.Translation(positions[i])));
            }

            var desc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "Marker Cubes",
                Primitives = markers.ToArray(),
                Color = new Color4(Color.Yellow.ToColor3(), 0.3333f),
            };
            await this.AddComponentPrimitiveListDrawer<Triangle>(desc);
        }
        private async Task InitializaDebug()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>() { DepthEnabled = true, Count = 20000 };
            this.lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer<Line3D>(desc);
        }

        private void PlantTrees()
        {
            Vector3 yDelta = Vector3.Down;

            Ray topDownRay = new Ray(new Vector3(350, 1000, 350), Vector3.Down);

            scenery.PickFirst(topDownRay, out var treePos);

            tree.Manipulator.SetPosition(treePos.Position + yDelta);
            tree.Manipulator.SetScale(2);

            foreach (var t in treesI.GetInstances())
            {
                float px = Helper.RandomGenerator.NextFloat(400, 600);
                float pz = Helper.RandomGenerator.NextFloat(400, 600);
                float r = Helper.RandomGenerator.NextFloat(0, MathUtil.TwoPi);
                float y = Helper.RandomGenerator.NextFloat(0, MathUtil.Pi / 16f);
                float s = Helper.RandomGenerator.NextFloat(1.8f, 3.5f);

                topDownRay = new Ray(new Vector3(px, 1000, pz), Vector3.Down);
                scenery.PickFirst(topDownRay, out var treeIPos);

                t.Manipulator.SetPosition(treeIPos.Position + yDelta);
                t.Manipulator.SetRotation(r, y, y);
                t.Manipulator.SetScale(s);
            }
        }

        public override void OnReportProgress(float value)
        {
            progressValue = Math.Max(progressValue, value);

            if (progressBar != null)
            {
                progressBar.ProgressValue = progressValue;
                progressBar.Caption.Text = $"{(int)(progressValue * 100f)}%";
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
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

            bool shift = this.Game.Input.ShiftPressed;

            this.UpdateInputCamera(gameTime, shift);
            this.UpdateInputDebug();

            this.UpdateWind(gameTime);
            this.UpdateSkyEffects();
            this.UpdateParticles();
            this.UpdateDebug();
        }
        private void UpdateInputCamera(GameTime gameTime, bool shift)
        {
            if (!cursor.Visible)
            {
                this.Camera.RotateMouse(
                    gameTime,
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
        private void UpdateInputDebug()
        {
            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.drawDrawVolumes = !this.drawDrawVolumes;
                this.drawCullVolumes = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
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
                this.Game.CollectGameStatus = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Tab))
            {
                this.ToggleLockMouse();
            }
        }

        private void UpdateWind(GameTime gameTime)
        {
            if (this.nextWindChange <= 0)
            {
                this.nextWindChange = Helper.RandomGenerator.NextFloat(0, 120);

                var limits = Vector2.One * 100f;
                this.nextWind = Helper.RandomGenerator.NextVector2(-limits, limits);
            }

            if (this.wind != this.nextWind)
            {
                this.wind = Vector2.Lerp(this.wind, this.nextWind, 0.001f);
                this.nextWindChange -= gameTime.ElapsedSeconds;
            }
        }
        private void UpdateSkyEffects()
        {
            this.skyPlane.Direction = Vector2.Normalize(this.wind);
            this.skyPlane.Velocity = Math.Min(1f, MathUtil.Lerp(this.skyPlane.Velocity, this.wind.Length() / 100f, 0.001f));
        }
        private void UpdateParticles()
        {
            for (int i = 0; i < this.particlePlumes.Length; i++)
            {
                var gravity = new Vector3(plumeGravity.X - wind.X, plumeGravity.Y, plumeGravity.Z - wind.Y);

                var parameters = this.particlePlumes[i].GetParameters();

                parameters.Gravity = gravity;
                parameters.MaxHorizontalVelocity = plumeMaxHorizontalVelocity;

                this.particlePlumes[i].SetParameters(parameters);
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

            var pLines = new List<Line3D>();
            var count = this.pManager.SystemsCount;
            for (int i = 0; i < count; i++)
            {
                pLines.AddRange(Line3D.CreateWiredBox(this.pManager.GetParticleSystem(i).Emitter.GetBoundingBox()));
            }
            this.lightsVolumeDrawer.AddPrimitives(new Color4(0, 0, 1, 0.75f), pLines);

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
        private void UpdateDebug()
        {
            this.runtime.Text = this.Game.RuntimeText;

            if (this.drawDrawVolumes)
            {
                this.UpdateLightDrawingVolumes();
            }

            if (this.drawCullVolumes)
            {
                this.UpdateLightCullingVolumes();
            }
        }

        private void RefreshUI()
        {
            this.spr.Top = 0;
            this.spr.Left = 0;
            this.spr.Width = this.Game.Form.RenderWidth;
            this.spr.Height = this.runtime.Top + this.runtime.Height + 3;

            this.butClose.Top = 1;
            this.butClose.Left = this.Game.Form.RenderWidth - this.butClose.Width - 1;
        }
        private void ToggleLockMouse()
        {
            this.cursor.Visible = !cursor.Visible;
            this.butClose.Visible = cursor.Visible;

            this.Game.Input.LockMouse = !cursor.Visible;
        }

        public override void GameGraphicsResized()
        {
            this.RefreshUI();
        }

        private void GameStatusCollected(object sender, GameStatusCollectedEventArgs e)
        {
            var lines = e.Trace.ReadStatus();
            if (lines.Any())
            {
                var file = $"frame.{this.Game.GameTime.Ticks}.txt";

                File.WriteAllLines(file, lines);
            }
        }
    }
}
