﻿using Engine;
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

namespace SceneTest.SceneTest
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
            Game.GameStatusCollected += GameStatusCollected;
            Game.VisibleMouse = false;
            Game.LockMouse = true;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 2000;
            Camera.SlowMovementDelta = 100f;
            Camera.MovementDelta = 500f;

            await LoadResourcesAsync(
                InitializeUI(),
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    RefreshUI();

                    progressBar.Visible = true;
                    progressBar.ProgressValue = 0;

                    await LoadControls();
                });
        }
        private async Task InitializeUI()
        {
            var titleDesc = UITextAreaDescription.DefaultFromFamily("Tahoma", 18);
            titleDesc.TextForeColor = Color.Yellow;
            titleDesc.TextShadowColor = Color.Orange;

            title = await this.AddComponentUITextArea("Title", titleDesc, layerHUD);
            title.Text = "Scene Test - Textures";
            title.SetPosition(Vector2.Zero);

            var runtimeDesc = UITextAreaDescription.DefaultFromFamily("Tahoma", 10);
            runtimeDesc.TextForeColor = Color.Yellow;
            runtimeDesc.TextShadowColor = Color.Orange;

            runtime = await this.AddComponentUITextArea("Runtime", runtimeDesc, layerHUD);
            runtime.Text = "";
            runtime.SetPosition(new Vector2(5, title.Top + title.Height + 3));

            spr = await this.AddComponentSprite("Backpanel", new SpriteDescription()
            {
                Width = Game.Form.RenderWidth,
                Height = runtime.Top + runtime.Height + 3,
                BaseColor = new Color4(0, 0, 0, 0.75f),
            }, SceneObjectUsages.UI, layerHUD - 1);

            var buttonFont = TextDrawerDescription.FromFamily("Lucida Console", 12);

            var buttonDesc = UIButtonDescription.DefaultTwoStateButton("SceneTest/UI/button_on.png", "SceneTest/UI/button_off.png");
            buttonDesc.Width = 100;
            buttonDesc.Height = 40;
            buttonDesc.Font = buttonFont;
            buttonDesc.Text = "Close";
            buttonDesc.TextForeColor = Color.Yellow;
            buttonDesc.TextShadowColor = Color.Orange;
            buttonDesc.TextHorizontalAlign = HorizontalTextAlign.Center;
            buttonDesc.TextVerticalAlign = VerticalTextAlign.Middle;

            butClose = await this.AddComponentUIButton("ButClose", buttonDesc, layerHUD);
            butClose.JustReleased += (sender, eventArgs) => { Game.SetScene<SceneStart.SceneStart>(); };
            butClose.Visible = false;

            blackPan = await this.AddComponentUIPanel("BlackPanel", new UIPanelDescription
            {
                Background = new SpriteDescription
                {
                    BaseColor = Color.Black,
                },
                Left = 0,
                Top = 0,
                Width = Game.Form.RenderWidth,
                Height = Game.Form.RenderHeight,
            }, layerHUD + 1);

            var pbDesc = UIProgressBarDescription.DefaultFromFamily("Consolas", 18);
            pbDesc.Top = Game.Form.RenderHeight - 60;
            pbDesc.Left = 100;
            pbDesc.Width = Game.Form.RenderWidth - 200;
            pbDesc.Height = 30;
            pbDesc.BaseColor = new Color(0, 0, 0, 0.5f);
            pbDesc.ProgressColor = Color.Green;

            progressBar = await this.AddComponentUIProgressBar("ProgressBar", pbDesc, layerHUD + 2);

            var cursorDesc = UICursorDescription.Default("Common/pointer.png", 48, 48, new Vector2(-14, -6));
            cursor = await this.AddComponentUICursor("Cursor", cursorDesc, layerHUD * 2);
            cursor.Visible = false;
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

            await LoadResourcesAsync(
                taskList,
                async (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    PlantTrees();

                    GameEnvironment.TimeOfDay.BeginAnimation(9, 00, 00, 0.1f);

                    Camera.Goto(-20 + xDelta, 10 + yDelta, -40f + zDelta);
                    Camera.LookTo(0 + xDelta, 0 + yDelta, 0 + zDelta);

                    blackPan.Hide(4000);
                    progressBar.Hide(2000);

                    await Task.Delay(1000);

                    gameReady = true;
                });
        }
        private async Task InitializeSkyEffects()
        {
            await this.AddComponentLensFlare("Flares", new LensFlareDescription()
            {
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

            await this.AddComponentSkyScattering("Sky", new SkyScatteringDescription());

            skyPlane = await this.AddComponentSkyPlane("Clouds", new SkyPlaneDescription()
            {
                ContentPath = "SceneTest/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                SkyMode = SkyPlaneModes.Perturbed,
            });
        }
        private async Task InitializeScenery()
        {
            scenery = await this.AddComponentScenery("Scenery", GroundDescription.FromFile("SceneTest/scenery", "Clif.xml"));
        }
        private async Task InitializeTrees()
        {
            tree = await this.AddComponentModel(
                "Tree",
                new ModelDescription()
                {
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

            treesI = await this.AddComponentModelInstanced("TreeI", desc);
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

            var floorAsphalt = await this.AddComponentModel("Floor", desc);
            floorAsphalt.Manipulator.SetPosition(xDelta, yDelta, zDelta);

            floorAsphaltI = await this.AddComponentModelInstanced("FloorI", descI);

            floorAsphaltI[0].Manipulator.SetPosition((-l * 2) + xDelta, yDelta, 0 + zDelta);
            floorAsphaltI[1].Manipulator.SetPosition((+l * 2) + xDelta, yDelta, 0 + zDelta);
            floorAsphaltI[2].Manipulator.SetPosition(0 + xDelta, yDelta, (-l * 2) + zDelta);
            floorAsphaltI[3].Manipulator.SetPosition(0 + xDelta, yDelta, (+l * 2) + zDelta);

            floorAsphaltI[4].Manipulator.SetPosition((-l * 2) + xDelta, yDelta, (-l * 2) + zDelta);
            floorAsphaltI[5].Manipulator.SetPosition((+l * 2) + xDelta, yDelta, (-l * 2) + zDelta);
            floorAsphaltI[6].Manipulator.SetPosition((-l * 2) + xDelta, yDelta, (+l * 2) + zDelta);
            floorAsphaltI[7].Manipulator.SetPosition((+l * 2) + xDelta, yDelta, (+l * 2) + zDelta);
        }
        private async Task InitializeBuildingObelisk()
        {
            this.buildingObelisk = await this.AddComponentModel(
                "Obelisk",
                new ModelDescription()
                {
                    CastShadow = true,
                    SphericVolume = false,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });

            buildingObeliskI = await this.AddComponentModelInstanced(
                "ObeliskI",
                new ModelInstancedDescription()
                {
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

            buildingObelisk.Manipulator.SetPosition(0 + xDelta, baseHeight + yDelta, 0 + zDelta);
            buildingObelisk.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            buildingObelisk.Manipulator.SetScale(10);

            buildingObeliskI[0].Manipulator.SetPosition((-spaceSize * 2) + xDelta, baseHeight + yDelta, 0 + zDelta);
            buildingObeliskI[1].Manipulator.SetPosition((+spaceSize * 2) + xDelta, baseHeight + yDelta, 0 + zDelta);
            buildingObeliskI[2].Manipulator.SetPosition(0 + xDelta, baseHeight + yDelta, (-spaceSize * 2) + zDelta);
            buildingObeliskI[3].Manipulator.SetPosition(0 + xDelta, baseHeight + yDelta, (+spaceSize * 2) + zDelta);

            buildingObeliskI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            buildingObeliskI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            buildingObeliskI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            buildingObeliskI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            buildingObeliskI[0].Manipulator.SetScale(10);
            buildingObeliskI[1].Manipulator.SetScale(10);
            buildingObeliskI[2].Manipulator.SetScale(10);
            buildingObeliskI[3].Manipulator.SetScale(10);
        }
        private async Task InitializeCharacterSoldier()
        {
            characterSoldier = await this.AddComponentModel(
                "Soldier",
                new ModelDescription()
                {
                    TextureIndex = 1,
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            characterSoldierI = await this.AddComponentModelInstanced(
                "SoldierI",
                new ModelInstancedDescription()
                {
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
            animations.Add("default", new AnimationPlan(p1));

            characterSoldier.Manipulator.SetPosition((+s - 10) + xDelta, baseHeight + yDelta, -s + zDelta);
            characterSoldier.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            characterSoldier.AnimationController.AddPath(animations["default"]);
            characterSoldier.AnimationController.Start(0);

            characterSoldierI[0].Manipulator.SetPosition((-spaceSize * 2 + s) + xDelta, baseHeight + yDelta, -s + zDelta);
            characterSoldierI[1].Manipulator.SetPosition((+spaceSize * 2 + s) + xDelta, baseHeight + yDelta, -s + zDelta);
            characterSoldierI[2].Manipulator.SetPosition(+s + xDelta, baseHeight + yDelta, (-spaceSize * 2 - s) + zDelta);
            characterSoldierI[3].Manipulator.SetPosition(+s + xDelta, baseHeight + yDelta, (+spaceSize * 2 - s) + zDelta);

            characterSoldierI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            characterSoldierI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            characterSoldierI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            characterSoldierI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            characterSoldierI[0].AnimationController.AddPath(animations["default"]);
            characterSoldierI[1].AnimationController.AddPath(animations["default"]);
            characterSoldierI[2].AnimationController.AddPath(animations["default"]);
            characterSoldierI[3].AnimationController.AddPath(animations["default"]);

            characterSoldierI[0].AnimationController.Start(1);
            characterSoldierI[1].AnimationController.Start(2);
            characterSoldierI[2].AnimationController.Start(3);
            characterSoldierI[3].AnimationController.Start(4);
        }
        private async Task InitializeVehicles()
        {
            vehicle = await this.AddComponentModel(
                "Challenger",
                new ModelDescription()
                {
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/vehicles/Challenger",
                        ModelContentFilename = "Challenger.xml",
                    }
                });

            vehicleI = await this.AddComponentModelInstanced(
                "LeopardI",
                new ModelInstancedDescription()
                {
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

            vehicle.Manipulator.SetPosition(s + xDelta, baseHeight + yDelta, -10 + zDelta);
            vehicle.Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);

            vehicleI[0].Manipulator.SetPosition(-spaceSize * 2 + xDelta, baseHeight + yDelta, -spaceSize * 2 + zDelta);
            vehicleI[1].Manipulator.SetPosition(+spaceSize * 2 + xDelta, baseHeight + yDelta, -spaceSize * 2 + zDelta);
            vehicleI[2].Manipulator.SetPosition(-spaceSize * 2 + xDelta, baseHeight + yDelta, +spaceSize * 2 + zDelta);
            vehicleI[3].Manipulator.SetPosition(+spaceSize * 2 + xDelta, baseHeight + yDelta, +spaceSize * 2 + zDelta);

            vehicleI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            vehicleI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            vehicleI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            vehicleI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            List<ISceneLight> lights = new List<ISceneLight>();

            lights.AddRange(vehicle.Lights);

            for (int i = 0; i < vehicleI.InstanceCount; i++)
            {
                lights.AddRange(vehicleI[i].Lights);
            }

            Lights.AddRange(lights);
        }
        private async Task InitializeLamps()
        {
            lamp = await this.AddComponentModel(
                "Lamp",
                new ModelDescription()
                {
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                });

            lampI = await this.AddComponentModelInstanced(
                "LampI",
                new ModelInstancedDescription()
                {
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

            lamp.Manipulator.SetPosition(0 + xDelta, spaceSize + yDelta, (-spaceSize * dist) + zDelta);
            lamp.Manipulator.SetRotation(0, pitch, 0);

            lampI[0].Manipulator.SetPosition(-spaceSize * 2 + xDelta, spaceSize + yDelta, -spaceSize * dist + zDelta);
            lampI[1].Manipulator.SetPosition(+spaceSize * 2 + xDelta, spaceSize + yDelta, -spaceSize * dist + zDelta);
            lampI[2].Manipulator.SetPosition(-spaceSize * dist + xDelta, spaceSize + yDelta, -spaceSize * 2 + zDelta);
            lampI[3].Manipulator.SetPosition(-spaceSize * dist + xDelta, spaceSize + yDelta, +spaceSize * 2 + zDelta);

            lampI[0].Manipulator.SetRotation(0, pitch, 0);
            lampI[1].Manipulator.SetRotation(0, pitch, 0);
            lampI[2].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);
            lampI[3].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);

            List<ISceneLight> lights = new List<ISceneLight>();

            lights.AddRange(lamp.Lights);

            for (int i = 0; i < lampI.InstanceCount; i++)
            {
                lights.AddRange(lampI[i].Lights);
            }

            Lights.AddRange(lights);
        }
        private async Task InitializeStreetLamps()
        {
            streetlamp = await this.AddComponentModel(
                "Street Lamp",
                new ModelDescription()
                {
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                });

            streetlampI = await this.AddComponentModelInstanced(
                "Street LampI",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    SphericVolume = false,
                    Instances = 9,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                });

            streetlamp.Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -2f + zDelta);

            streetlampI[0].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -1f + zDelta);
            streetlampI[1].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, 0 + zDelta);
            streetlampI[2].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 1f + zDelta);
            streetlampI[3].Manipulator.SetPosition(-spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 2f + zDelta);

            streetlampI[4].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -2f + zDelta);
            streetlampI[5].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * -1f + zDelta);
            streetlampI[6].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, 0 + zDelta);
            streetlampI[7].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 1f + zDelta);
            streetlampI[8].Manipulator.SetPosition(+spaceSize + xDelta, baseHeight + yDelta, -spaceSize * 2f + zDelta);

            streetlampI[4].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            streetlampI[5].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            streetlampI[6].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            streetlampI[7].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            streetlampI[8].Manipulator.SetRotation(MathUtil.Pi, 0, 0);

            List<ISceneLight> lights = new List<ISceneLight>();

            lights.AddRange(streetlamp.Lights);

            for (int i = 0; i < streetlampI.InstanceCount; i++)
            {
                lights.AddRange(streetlampI[i].Lights);
            }

            Lights.AddRange(lights);
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

            container = await this.AddComponentModel(
                "Container",
                new ModelDescription()
                {
                    CastShadow = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/container",
                        ModelContentFilename = "Container.xml",
                    }
                });

            containerI = await this.AddComponentModelInstanced(
                "ContainerI",
                new ModelInstancedDescription()
                {
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

            var bboxTmp = container.GetBoundingBox();
            float scaleX = areaSize * 2 / xSize / bboxTmp.Width;
            float scaleZ = areaSize * 2 / zSize / bboxTmp.Depth;
            Vector3 scale = new Vector3(scaleX, (scaleX + scaleZ) / 2f, scaleZ);

            container.Manipulator.SetScale(scale);
            container.Manipulator.UpdateInternals(true);
            var bbox = container.GetBoundingBox();
            float sx = bbox.Width;
            float sy = bbox.Height;
            float sz = bbox.Depth;

            container.Manipulator.SetPosition(s + 12 + xDelta, baseHeight + yDelta, 30 + zDelta);
            container.Manipulator.SetRotation(MathUtil.PiOverTwo * 2.1f, 0, 0);

            Random prnd = new Random(1000);

            for (int i = 0; i < containerI.InstanceCount; i++)
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

                    containerI[i].TextureIndex = textureIndex;

                    containerI[i].Manipulator.SetPosition(x, y, z);
                    containerI[i].Manipulator.SetRotation(angle, 0, 0);
                    containerI[i].Manipulator.SetScale(scale);
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

                    containerI[i].TextureIndex = textureIndex;

                    containerI[i].Manipulator.SetPosition(x, y, z);
                    containerI[i].Manipulator.SetRotation(angle, 0, 0);
                    containerI[i].Manipulator.SetScale(scale);
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
                Primitives = cubeTris.ToArray(),
                Color = Color.Red,
                DepthEnabled = true,
            };

            await this.AddComponentPrimitiveListDrawer("Test Cube", desc);
        }
        private async Task InitializeParticles()
        {
            var pPlume = ParticleSystemDescription.InitializeSmokePlume("SceneTest/particles", "smoke.png", 10f);
            var pFire = ParticleSystemDescription.InitializeFire("SceneTest/particles", "fire.png", 10f);
            var pDust = ParticleSystemDescription.InitializeDust("SceneTest/particles", "smoke.png", 10f);
            var pProjectile = ParticleSystemDescription.InitializeProjectileTrail("SceneTest/particles", "smoke.png", 10f);
            var pExplosion = ParticleSystemDescription.InitializeExplosion("SceneTest/particles", "fire.png", 10f);
            var pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("SceneTest/particles", "smoke.png", 10f);

            pDescriptions.Add("Plume", pPlume);
            pDescriptions.Add("Fire", pFire);
            pDescriptions.Add("Dust", pDust);
            pDescriptions.Add("Projectile", pProjectile);
            pDescriptions.Add("Explosion", pExplosion);
            pDescriptions.Add("SmokeExplosion", pSmokeExplosion);

            pManager = await this.AddComponentParticleManager("ParticleManager", ParticleManagerDescription.Default());

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

            particlePlumes = new IParticleSystem[positions.Length];
            List<Triangle> markers = new List<Triangle>();
            for (int i = 0; i < positions.Length; i++)
            {
                particlePlumes[i] = pManager.AddParticleSystem(
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
                Primitives = markers.ToArray(),
                Color = new Color4(Color.Yellow.ToColor3(), 0.3333f),
            };
            await this.AddComponentPrimitiveListDrawer("Marker Cubes", desc);
        }
        private async Task InitializaDebug()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>() { DepthEnabled = true, Count = 20000 };
            lightsVolumeDrawer = await this.AddComponentPrimitiveListDrawer("DebugLightsVolumeDrawer", desc);
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

        public override void OnReportProgress(LoadResourceProgress value)
        {
            progressValue = Math.Max(progressValue, value.Progress);

            if (progressBar != null)
            {
                progressBar.ProgressValue = progressValue;
                progressBar.Caption.Text = $"{(int)(progressValue * 100f)}%";
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            bool shift = Game.Input.ShiftPressed;

            UpdateInputCamera(gameTime, shift);
            UpdateInputDebug();

            UpdateWind(gameTime);
            UpdateSkyEffects();
            UpdateParticles();
            UpdateDebug();
        }
        private void UpdateInputCamera(GameTime gameTime, bool shift)
        {
            if (!cursor.Visible)
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, !shift);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, !shift);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, !shift);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, !shift);
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                drawDrawVolumes = !drawDrawVolumes;
                drawCullVolumes = false;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
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
                Game.CollectGameStatus = true;
            }

            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                ToggleLockMouse();
            }
        }

        private void UpdateWind(GameTime gameTime)
        {
            if (nextWindChange <= 0)
            {
                nextWindChange = Helper.RandomGenerator.NextFloat(0, 120);

                var limits = Vector2.One * 100f;
                nextWind = Helper.RandomGenerator.NextVector2(-limits, limits);
            }

            if (wind != nextWind)
            {
                wind = Vector2.Lerp(wind, nextWind, 0.001f);
                nextWindChange -= gameTime.ElapsedSeconds;
            }
        }
        private void UpdateSkyEffects()
        {
            skyPlane.Direction = Vector2.Normalize(wind);
            skyPlane.Velocity = Math.Min(1f, MathUtil.Lerp(skyPlane.Velocity, wind.Length() / 100f, 0.001f));
        }
        private void UpdateParticles()
        {
            for (int i = 0; i < particlePlumes.Length; i++)
            {
                var gravity = new Vector3(plumeGravity.X - wind.X, plumeGravity.Y, plumeGravity.Z - wind.Y);

                var parameters = particlePlumes[i].GetParameters();

                parameters.Gravity = gravity;
                parameters.MaxHorizontalVelocity = plumeMaxHorizontalVelocity;

                particlePlumes[i].SetParameters(parameters);
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

            var pLines = new List<Line3D>();
            var count = pManager.Count;
            for (int i = 0; i < count; i++)
            {
                pLines.AddRange(Line3D.CreateWiredBox(pManager.GetParticleSystem(i).Emitter.GetBoundingBox()));
            }
            lightsVolumeDrawer.AddPrimitives(new Color4(0, 0, 1, 0.75f), pLines);

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
        private void UpdateDebug()
        {
            runtime.Text = Game.RuntimeText;

            if (drawDrawVolumes)
            {
                UpdateLightDrawingVolumes();
            }

            if (drawCullVolumes)
            {
                UpdateLightCullingVolumes();
            }
        }

        private void RefreshUI()
        {
            spr.Top = 0;
            spr.Left = 0;
            spr.Width = Game.Form.RenderWidth;
            spr.Height = runtime.Top + runtime.Height + 3;

            butClose.Top = 1;
            butClose.Left = Game.Form.RenderWidth - butClose.Width - 1;
        }
        private void ToggleLockMouse()
        {
            cursor.Visible = !cursor.Visible;
            butClose.Visible = cursor.Visible;

            Game.Input.LockMouse = !cursor.Visible;
        }

        public override void GameGraphicsResized()
        {
            RefreshUI();
        }

        private void GameStatusCollected(object sender, GameStatusCollectedEventArgs e)
        {
            var lines = e.Trace.ReadStatus();
            if (lines.Any())
            {
                var file = $"frame.{Game.GameTime.Ticks}.txt";

                File.WriteAllLines(file, lines);
            }
        }
    }
}
