using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SceneTest
{
    public class SceneTest : Scene
    {
        private const int layerHUD = 99;

        private readonly float baseHeight = 0.1f;
        private readonly float spaceSize = 40;

        private readonly Random rnd = new Random();

        private SceneObject<Cursor> cursor = null;
        private SceneObject<SpriteButton> butClose = null;

        private Sprite spr = null;
        private TextDrawer title = null;
        private TextDrawer runtime = null;

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

        private SceneObject<PrimitiveListDrawer<Line3D>> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        public SceneTest(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Game.GameStatusCollected += GameStatusCollected;
#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 2000;
            this.Camera.Goto(-20, 10, -40f);
            this.Camera.LookTo(0, 0, 0);

            this.InitializeCursor();
            this.InitializeTextBoxes();
            this.InitializeSpriteButtons();
            this.InitializeSkyEffects();
            this.InitializeScenery();
            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeCharacterSoldier();
            this.InitializeVehicles();
            this.InitializeLamps();
            this.InitializeStreetLamps();
            this.InitializeContainers();
            this.InitializeTestCube();
            this.InitializeParticles();
            this.InitializeWater();

            var desc = new PrimitiveListDrawerDescription<Line3D>() { DepthEnabled = true, Count = 10000 };
            this.lightsVolumeDrawer = this.AddComponent<PrimitiveListDrawer<Line3D>>(desc);

            this.TimeOfDay.BeginAnimation(new TimeSpan(9, 00, 00), 0.1f);
        }
        private void InitializeCursor()
        {
            var cursorDesc = new CursorDescription()
            {
                Name = "Cursor",
                ContentPath = "Common",
                Textures = new[] { "pointer.png" },
                Height = 48,
                Width = 48,
                Centered = false,
                Delta = new Vector2(-14, -6),
                Color = Color.White,
            };
            cursor = this.AddComponent<Cursor>(cursorDesc, SceneObjectUsages.UI, 100);
            cursor.Visible = false;
        }
        private void InitializeTextBoxes()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange), SceneObjectUsages.UI, layerHUD).Instance;
            this.runtime = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange), SceneObjectUsages.UI, layerHUD).Instance;

            this.title.Text = "Scene Test - Textures";
            this.runtime.Text = "";

            this.title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, this.title.Top + this.title.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.runtime.Top + this.runtime.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            spr = this.AddComponent<Sprite>(spDesc, SceneObjectUsages.UI, layerHUD - 1).Instance;
        }
        private void InitializeSpriteButtons()
        {
            this.butClose = this.AddComponent<SpriteButton>(new SpriteButtonDescription()
            {
                TwoStateButton = true,
                TextureReleased = "SceneTest/UI/button_off.png",
                TexturePressed = "SceneTest/UI/button_on.png",

                Width = 100,
                Height = 40,
                TextDescription = new TextDrawerDescription()
                {
                    Font = "Lucida Console",
                    FontSize = 12,
                    TextColor = Color.Yellow,
                    ShadowColor = Color.Orange,
                },
                Text = "Close",
            }, SceneObjectUsages.UI, layerHUD);

            this.butClose.Instance.Click += (sender, eventArgs) => { this.Game.SetScene<SceneStart>(); };
            this.butClose.Visible = false;
        }
        private void InitializeSkyEffects()
        {
            this.AddComponent<LensFlare>(new LensFlareDescription()
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

            this.AddComponent<SkyScattering>(new SkyScatteringDescription() { Name = "Sky" });

            this.skyPlane = this.AddComponent<SkyPlane>(new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "SceneTest/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                SkyMode = SkyPlaneModes.Perturbed,
            }).Instance;
        }
        private void InitializeScenery()
        {
            this.AddComponent<Scenery>(
                new GroundDescription()
                {
                    Name = "Clif",
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/scenery",
                        ModelContentFilename = "Clif.xml",
                    }
                });
        }
        private void InitializeFloorAsphalt()
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
                Static = true,
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                AlphaEnabled = false,
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
                Static = true,
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                AlphaEnabled = false,
                SphericVolume = false,
                UseAnisotropicFiltering = true,
                Instances = 8,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            this.AddComponent<Model>(desc);

            this.floorAsphaltI = this.AddComponent<ModelInstanced>(descI).Instance;

            this.floorAsphaltI[0].Manipulator.SetPosition(-l * 2, 0, 0);
            this.floorAsphaltI[1].Manipulator.SetPosition(l * 2, 0, 0);
            this.floorAsphaltI[2].Manipulator.SetPosition(0, 0, -l * 2);
            this.floorAsphaltI[3].Manipulator.SetPosition(0, 0, l * 2);

            this.floorAsphaltI[4].Manipulator.SetPosition(-l * 2, 0, -l * 2);
            this.floorAsphaltI[5].Manipulator.SetPosition(l * 2, 0, -l * 2);
            this.floorAsphaltI[6].Manipulator.SetPosition(-l * 2, 0, l * 2);
            this.floorAsphaltI[7].Manipulator.SetPosition(l * 2, 0, l * 2);
        }
        private void InitializeBuildingObelisk()
        {
            this.buildingObelisk = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Obelisk",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                }).Instance;

            this.buildingObeliskI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "ObeliskI",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    UseAnisotropicFiltering = true,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                }).Instance;

            this.buildingObelisk.Manipulator.SetPosition(0, baseHeight, 0);
            this.buildingObelisk.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObelisk.Manipulator.SetScale(10);

            this.buildingObeliskI[0].Manipulator.SetPosition(-spaceSize * 2, baseHeight, 0);
            this.buildingObeliskI[1].Manipulator.SetPosition(spaceSize * 2, baseHeight, 0);
            this.buildingObeliskI[2].Manipulator.SetPosition(0, baseHeight, -spaceSize * 2);
            this.buildingObeliskI[3].Manipulator.SetPosition(0, baseHeight, spaceSize * 2);

            this.buildingObeliskI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.buildingObeliskI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObeliskI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.buildingObeliskI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.buildingObeliskI[0].Manipulator.SetScale(10);
            this.buildingObeliskI[1].Manipulator.SetScale(10);
            this.buildingObeliskI[2].Manipulator.SetScale(10);
            this.buildingObeliskI[3].Manipulator.SetScale(10);
        }
        private void InitializeCharacterSoldier()
        {
            this.characterSoldier = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Soldier",
                    TextureIndex = 1,
                    CastShadow = true,
                    Static = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                }).Instance;

            this.characterSoldierI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "SoldierI",
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                }).Instance;

            float s = spaceSize / 2f;

            AnimationPath p1 = new AnimationPath();
            p1.AddLoop("idle1");
            this.animations.Add("default", new AnimationPlan(p1));

            this.characterSoldier.Manipulator.SetPosition(s - 10, baseHeight, -s);
            this.characterSoldier.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldier.AnimationController.AddPath(this.animations["default"]);
            this.characterSoldier.AnimationController.Start(0);

            this.characterSoldierI[0].Manipulator.SetPosition(-spaceSize * 2 + s, baseHeight, -s);
            this.characterSoldierI[1].Manipulator.SetPosition(spaceSize * 2 + s, baseHeight, -s);
            this.characterSoldierI[2].Manipulator.SetPosition(s, baseHeight, -spaceSize * 2 - s);
            this.characterSoldierI[3].Manipulator.SetPosition(s, baseHeight, spaceSize * 2 - s);

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
        private void InitializeVehicles()
        {
            this.vehicle = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Challenger",
                    CastShadow = true,
                    Static = false,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/vehicles/Challenger",
                        ModelContentFilename = "Challenger.xml",
                    }
                }).Instance;

            this.vehicleI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "LeopardI",
                    CastShadow = true,
                    Static = false,
                    SphericVolume = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/vehicles/leopard",
                        ModelContentFilename = "Leopard.xml",
                    }
                }).Instance;

            float s = -spaceSize / 2f;

            this.vehicle.Manipulator.SetPosition(s, baseHeight, -10);
            this.vehicle.Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);

            this.vehicleI[0].Manipulator.SetPosition(-spaceSize * 2, baseHeight, -spaceSize * 2);
            this.vehicleI[1].Manipulator.SetPosition(spaceSize * 2, baseHeight, -spaceSize * 2);
            this.vehicleI[2].Manipulator.SetPosition(-spaceSize * 2, baseHeight, spaceSize * 2);
            this.vehicleI[3].Manipulator.SetPosition(spaceSize * 2, baseHeight, spaceSize * 2);

            this.vehicleI[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.vehicleI[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.vehicleI[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.vehicleI[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            var lights = new List<SceneLight>();

            lights.AddRange(this.vehicle.Lights);

            for (int i = 0; i < this.vehicleI.Count; i++)
            {
                lights.AddRange(this.vehicleI[i].Lights);
            }

            this.Lights.AddRange(lights);
        }
        private void InitializeLamps()
        {
            this.lamp = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Lamp",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                }).Instance;

            this.lampI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "LampI",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                }).Instance;

            float dist = 0.23f;
            float pitch = MathUtil.DegreesToRadians(165) * -1;

            this.lamp.Manipulator.SetPosition(0, spaceSize, -spaceSize * dist);
            this.lamp.Manipulator.SetRotation(0, pitch, 0);

            this.lampI[0].Manipulator.SetPosition(-spaceSize * 2, spaceSize, -spaceSize * dist);
            this.lampI[1].Manipulator.SetPosition(spaceSize * 2, spaceSize, -spaceSize * dist);
            this.lampI[2].Manipulator.SetPosition(-spaceSize * dist, spaceSize, -spaceSize * 2);
            this.lampI[3].Manipulator.SetPosition(-spaceSize * dist, spaceSize, spaceSize * 2);

            this.lampI[0].Manipulator.SetRotation(0, pitch, 0);
            this.lampI[1].Manipulator.SetRotation(0, pitch, 0);
            this.lampI[2].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);
            this.lampI[3].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);

            var lights = new List<SceneLight>();

            lights.AddRange(this.lamp.Lights);

            for (int i = 0; i < this.lampI.Count; i++)
            {
                lights.AddRange(this.lampI[i].Lights);
            }

            this.Lights.AddRange(lights);
        }
        private void InitializeStreetLamps()
        {
            this.streetlamp = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Street Lamp",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                }).Instance;

            this.streetlampI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "Street LampI",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    Instances = 9,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                }).Instance;

            this.streetlamp.Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * -2f);

            this.streetlampI[0].Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * -1f);
            this.streetlampI[1].Manipulator.SetPosition(-spaceSize, baseHeight, 0);
            this.streetlampI[2].Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * 1f);
            this.streetlampI[3].Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * 2f);

            this.streetlampI[4].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * -2f);
            this.streetlampI[5].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * -1f);
            this.streetlampI[6].Manipulator.SetPosition(+spaceSize, baseHeight, 0);
            this.streetlampI[7].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * 1f);
            this.streetlampI[8].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * 2f);

            this.streetlampI[4].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[5].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[6].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[7].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI[8].Manipulator.SetRotation(MathUtil.Pi, 0, 0);

            var lights = new List<SceneLight>();

            lights.AddRange(this.streetlamp.Lights);

            for (int i = 0; i < this.streetlampI.Count; i++)
            {
                lights.AddRange(this.streetlampI[i].Lights);
            }

            this.Lights.AddRange(lights);
        }
        private void InitializeContainers()
        {
            this.container = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Container",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/container",
                        ModelContentFilename = "Container.xml",
                    }
                }).Instance;

            this.containerI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "ContainerI",
                    CastShadow = true,
                    Static = true,
                    SphericVolume = false,
                    Instances = 96,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTest/container",
                        ModelContentFilename = "Container.xml",
                    }
                }).Instance;

            float s = -spaceSize / 2f;

            Random prnd = new Random(1000);

            this.container.Manipulator.SetScale(5f);
            this.container.Manipulator.UpdateInternals(true);
            var bbox = this.container.GetBoundingBox();

            this.container.Manipulator.SetPosition(s + 12, baseHeight, 30);
            this.container.Manipulator.SetRotation(MathUtil.PiOverTwo * 2.1f, 0, 0);

            for (int i = 0; i < this.containerI.Count; i++)
            {
                uint textureIndex = (uint)prnd.Next(0, 6);
                textureIndex %= 5;

                float height = (i < 48 ? 0 : bbox.GetY()) + baseHeight;

                if ((i % 48) < 24)
                {
                    float angle = MathUtil.Pi * prnd.Next(0, 2);
                    float x = ((i % 24) < 12 ? -120f : 120f) + prnd.NextFloat(-1f, 1f);

                    this.containerI[i].TextureIndex = textureIndex;

                    this.containerI[i].Manipulator.SetPosition(x, height, ((i % 12) * bbox.GetZ()) - (120));
                    this.containerI[i].Manipulator.SetRotation(angle, 0, 0);
                    this.containerI[i].Manipulator.SetScale(5f);
                }
                else
                {
                    float angle = MathUtil.Pi * prnd.Next(0, 2);
                    float z = ((i % 24) < 12 ? -120f : 120f) + prnd.NextFloat(-1f, 1f);

                    this.containerI[i].TextureIndex = textureIndex;

                    this.containerI[i].Manipulator.SetPosition(((i % 12) * bbox.GetX()) - (120), height, z);
                    this.containerI[i].Manipulator.SetRotation(angle, 0, 0);
                    this.containerI[i].Manipulator.SetScale(5f);
                }
            }
        }
        private void InitializeTestCube()
        {
            var bbox = new BoundingBox(Vector3.One * -0.5f, Vector3.One * 0.5f);
            var cubeTris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox);
            cubeTris = Triangle.Transform(cubeTris, Matrix.Translation(30, 0.5f, 0));

            var desc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Name = "Test Cube",
                Primitives = cubeTris,
                Color = Color.Red,
                DepthEnabled = true,
            };

            this.AddComponent<PrimitiveListDrawer<Triangle>>(desc);
        }
        private void InitializeParticles()
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

            this.pManager = this.AddComponent<ParticleManager>(new ParticleManagerDescription() { Name = "Particle Manager" }).Instance;

            float d = 500;
            var positions = new Vector3[]
            {
                new Vector3(+d,0,+d),
                new Vector3(+d,0,-d),
                new Vector3(-d,0,+d),
                new Vector3(-d,0,-d),
            };
            this.particlePlumes = new IParticleSystem[positions.Length];
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
            }
        }
        private void InitializeWater()
        {
            WaterDescription waterDesc = WaterDescription.CreateCalm("Ocean", 5000f, -100f);
            this.AddComponent<Water>(waterDesc, SceneObjectUsages.None);
        }

        public override void Initialized()
        {
            base.Initialized();

            this.RefreshUI();
        }
        private void RefreshUI()
        {
            this.spr.Top = 0;
            this.spr.Left = 0;
            this.spr.Width = this.Game.Form.RenderWidth;
            this.spr.Height = this.runtime.Top + this.runtime.Height + 3;

            this.butClose.Instance.Top = 1;
            this.butClose.Instance.Left = this.Game.Form.RenderWidth - this.butClose.Instance.Width - 1;
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

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            this.UpdateCamera(gameTime, shift);
            this.UpdateWind(gameTime);
            this.UpdateSkyEffects();
            this.UpdateParticles();
            this.UpdateDebug();

            this.runtime.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera(GameTime gameTime, bool shift)
        {
            if(!cursor.Visible)
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }
        private void UpdateWind(GameTime gameTime)
        {
            if (this.nextWindChange <= 0)
            {
                this.nextWindChange = this.rnd.NextFloat(0, 120);

                var limits = Vector2.One * 100f;
                this.nextWind = this.rnd.NextVector2(-limits, limits);
            }

            this.wind = Vector2.Lerp(this.wind, this.nextWind, 0.001f);
            this.nextWindChange -= gameTime.ElapsedSeconds;
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

                var parameters = this.particlePlumes[i].Parameters;

                parameters.Gravity = gravity;
                parameters.MaxHorizontalVelocity = plumeMaxHorizontalVelocity;

                this.particlePlumes[i].Parameters = parameters;
            }
        }
        private void UpdateLightDrawingVolumes()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                this.lightsVolumeDrawer.Instance.AddPrimitives(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                this.lightsVolumeDrawer.Instance.AddPrimitives(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void UpdateLightCullingVolumes()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.Instance.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.Instance.AddPrimitives(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void UpdateDebug()
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
                this.cursor.Visible = !cursor.Visible;
                this.butClose.Visible = cursor.Visible;

                this.Game.Input.LockMouse = !cursor.Visible;
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

        protected override void Resized(object sender, EventArgs e)
        {
            this.RefreshUI();
        }

        private void GameStatusCollected(object sender, GameStatusCollectedEventArgs e)
        {
            var lines = e.Trace.Select((i) => $"{i.Key}: {i.Value:0.00}");
            var file = $"frame.{this.Game.GameTime.Ticks}.txt";

            File.WriteAllLines(file, lines);
        }
    }
}
