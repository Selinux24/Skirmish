using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;

namespace SceneTest
{
    public class SceneTextures : Scene
    {
        private const int layerHUD = 99;

        private float baseHeight = 0.1f;
        private float spaceSize = 40;

        private Random rnd = new Random();

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> runtime = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<LensFlare> lensFlare = null;

        private SceneObject<Scenery> clif = null;

        private SceneObject<Model> floorAsphalt = null;
        private SceneObject<ModelInstanced> floorAsphaltI = null;

        private SceneObject<Model> buildingObelisk = null;
        private SceneObject<ModelInstanced> buildingObeliskI = null;

        private SceneObject<Model> characterSoldier = null;
        private SceneObject<ModelInstanced> characterSoldierI = null;

        private SceneObject<Model> vehicleLeopard = null;
        private SceneObject<ModelInstanced> vehicleLeopardI = null;

        private SceneObject<Model> lamp = null;
        private SceneObject<ModelInstanced> lampI = null;

        private SceneObject<Model> streetlamp = null;
        private SceneObject<ModelInstanced> streetlampI = null;

        private SceneObject<SkyScattering> sky = null;
        private SceneObject<SkyPlane> skyPlane = null;

        private SceneObject<TriangleListDrawer> testCube = null;

        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private ParticleSystemDescription pProjectile = null;
        private ParticleSystemDescription pExplosion = null;
        private ParticleSystemDescription pSmokeExplosion = null;
        private SceneObject<ParticleManager> pManager = null;

        private IParticleSystem[] particlePlumes = null;
        private Vector3 plumeGravity = new Vector3(0, 5, 0);
        private float plumeMaxHorizontalVelocity = 25f;
        private Vector2 wind = new Vector2(0, 0);
        private Vector2 nextWind = new Vector2();
        private float nextWindChange = 0;

        private Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        private SceneObject<LineListDrawer> lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        public SceneTextures(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

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

            GameEnvironment.LODDistanceLow *= 2;
            GameEnvironment.LODDistanceMedium *= 2;
            GameEnvironment.LODDistanceHigh *= 2;
            GameEnvironment.LODDistanceMinimum *= 2;

            this.InitializeTextBoxes();
            this.InitializeSkyEffects();
            this.InitializeScenery();
            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeCharacterSoldier();
            this.InitializeVehiclesLeopard();
            this.InitializeLamps();
            this.InitializeStreetLamps();
            this.InitializeTestCube();
            this.InitializeParticles();

            var desc = new LineListDrawerDescription() { DepthEnabled = true, Count = 10000 };
            this.lightsVolumeDrawer = this.AddComponent<LineListDrawer>(desc);

            this.TimeOfDay.BeginAnimation(new TimeSpan(5, 00, 00), 5f);
        }

        private void InitializeTextBoxes()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange), SceneObjectUsageEnum.UI, layerHUD);
            this.runtime = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange), SceneObjectUsageEnum.UI, layerHUD);

            this.title.Instance.Text = "Scene Test - Textures";
            this.runtime.Instance.Text = "";

            this.title.Instance.Position = Vector2.Zero;
            this.runtime.Instance.Position = new Vector2(5, this.title.Instance.Top + this.title.Instance.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.runtime.Instance.Top + this.runtime.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
        }
        private void InitializeSkyEffects()
        {
            this.lensFlare = this.AddComponent<LensFlare>(new LensFlareDescription()
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

            this.sky = this.AddComponent<SkyScattering>(new SkyScatteringDescription() { Name = "Sky" });

            this.skyPlane = this.AddComponent<SkyPlane>(new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "SceneTextures/sky",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                Mode = SkyPlaneMode.Perturbed,
            });
        }
        private void InitializeScenery()
        {
            this.clif = this.AddComponent<Scenery>(
                new GroundDescription()
                {
                    Name = "Clif",
                    CastShadow = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTextures/scenery",
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
            mat.DiffuseTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_specular.dds";

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

            this.floorAsphalt = this.AddComponent<Model>(desc);

            this.floorAsphaltI = this.AddComponent<ModelInstanced>(descI);

            this.floorAsphaltI.Instance[0].Manipulator.SetPosition(-l * 2, 0, 0);
            this.floorAsphaltI.Instance[1].Manipulator.SetPosition(l * 2, 0, 0);
            this.floorAsphaltI.Instance[2].Manipulator.SetPosition(0, 0, -l * 2);
            this.floorAsphaltI.Instance[3].Manipulator.SetPosition(0, 0, l * 2);

            this.floorAsphaltI.Instance[4].Manipulator.SetPosition(-l * 2, 0, -l * 2);
            this.floorAsphaltI.Instance[5].Manipulator.SetPosition(l * 2, 0, -l * 2);
            this.floorAsphaltI.Instance[6].Manipulator.SetPosition(-l * 2, 0, l * 2);
            this.floorAsphaltI.Instance[7].Manipulator.SetPosition(l * 2, 0, l * 2);
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
                        ContentFolder = "SceneTextures/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });

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
                        ContentFolder = "SceneTextures/buildings/obelisk",
                        ModelContentFilename = "Obelisk.xml",
                    }
                });

            this.buildingObelisk.Transform.SetPosition(0, baseHeight, 0);
            this.buildingObelisk.Transform.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObelisk.Transform.SetScale(10);

            this.buildingObeliskI.Instance[0].Manipulator.SetPosition(-spaceSize * 2, baseHeight, 0);
            this.buildingObeliskI.Instance[1].Manipulator.SetPosition(spaceSize * 2, baseHeight, 0);
            this.buildingObeliskI.Instance[2].Manipulator.SetPosition(0, baseHeight, -spaceSize * 2);
            this.buildingObeliskI.Instance[3].Manipulator.SetPosition(0, baseHeight, spaceSize * 2);

            this.buildingObeliskI.Instance[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.buildingObeliskI.Instance[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObeliskI.Instance[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.buildingObeliskI.Instance[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.buildingObeliskI.Instance[0].Manipulator.SetScale(10);
            this.buildingObeliskI.Instance[1].Manipulator.SetScale(10);
            this.buildingObeliskI.Instance[2].Manipulator.SetScale(10);
            this.buildingObeliskI.Instance[3].Manipulator.SetScale(10);
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
                        ContentFolder = "SceneTextures/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            this.characterSoldierI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "SoldierI",
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTextures/character/soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            float s = spaceSize / 2f;

            AnimationPath p1 = new AnimationPath();
            p1.AddLoop("idle1");
            this.animations.Add("default", new AnimationPlan(p1));

            this.characterSoldier.Transform.SetPosition(s - 10, baseHeight, -s);
            this.characterSoldier.Transform.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldier.Instance.AnimationController.AddPath(this.animations["default"]);
            this.characterSoldier.Instance.AnimationController.Start(0);

            this.characterSoldierI.Instance[0].Manipulator.SetPosition(-spaceSize * 2 + s, baseHeight, -s);
            this.characterSoldierI.Instance[1].Manipulator.SetPosition(spaceSize * 2 + s, baseHeight, -s);
            this.characterSoldierI.Instance[2].Manipulator.SetPosition(s, baseHeight, -spaceSize * 2 - s);
            this.characterSoldierI.Instance[3].Manipulator.SetPosition(s, baseHeight, spaceSize * 2 - s);

            this.characterSoldierI.Instance[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.characterSoldierI.Instance[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldierI.Instance[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.characterSoldierI.Instance[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.characterSoldierI.Instance[0].AnimationController.AddPath(this.animations["default"]);
            this.characterSoldierI.Instance[1].AnimationController.AddPath(this.animations["default"]);
            this.characterSoldierI.Instance[2].AnimationController.AddPath(this.animations["default"]);
            this.characterSoldierI.Instance[3].AnimationController.AddPath(this.animations["default"]);

            this.characterSoldierI.Instance[0].AnimationController.Start(1);
            this.characterSoldierI.Instance[1].AnimationController.Start(2);
            this.characterSoldierI.Instance[2].AnimationController.Start(3);
            this.characterSoldierI.Instance[3].AnimationController.Start(4);
        }
        private void InitializeVehiclesLeopard()
        {
            this.vehicleLeopard = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Leopard",
                    CastShadow = true,
                    Static = false,
                    SphericVolume = false,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTextures/vehicles/leopard",
                        ModelContentFilename = "Leopard.xml",
                    }
                });

            this.vehicleLeopardI = this.AddComponent<ModelInstanced>(
                new ModelInstancedDescription()
                {
                    Name = "LeopardI",
                    CastShadow = true,
                    Static = false,
                    SphericVolume = false,
                    Instances = 4,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SceneTextures/vehicles/leopard",
                        ModelContentFilename = "Leopard.xml",
                    }
                });

            float s = -spaceSize / 2f;

            this.vehicleLeopard.Transform.SetPosition(s, baseHeight, 0);
            this.vehicleLeopard.Transform.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);

            this.vehicleLeopardI.Instance[0].Manipulator.SetPosition(-spaceSize * 2, baseHeight, -spaceSize * 2);
            this.vehicleLeopardI.Instance[1].Manipulator.SetPosition(spaceSize * 2, baseHeight, -spaceSize * 2);
            this.vehicleLeopardI.Instance[2].Manipulator.SetPosition(-spaceSize * 2, baseHeight, spaceSize * 2);
            this.vehicleLeopardI.Instance[3].Manipulator.SetPosition(spaceSize * 2, baseHeight, spaceSize * 2);

            this.vehicleLeopardI.Instance[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.vehicleLeopardI.Instance[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.vehicleLeopardI.Instance[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.vehicleLeopardI.Instance[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.Lights.AddRange(this.vehicleLeopard.Instance.Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instance[0].Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instance[1].Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instance[2].Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instance[3].Lights);
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
                        ContentFolder = "SceneTextures/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                });

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
                        ContentFolder = "SceneTextures/lamps",
                        ModelContentFilename = "lamp.xml",
                    }
                });

            float dist = 0.23f;
            float pitch = MathUtil.DegreesToRadians(165) * -1;

            this.lamp.Transform.SetPosition(0, spaceSize, -spaceSize * dist);
            this.lamp.Transform.SetRotation(0, pitch, 0);

            this.lampI.Instance[0].Manipulator.SetPosition(-spaceSize * 2, spaceSize, -spaceSize * dist);
            this.lampI.Instance[1].Manipulator.SetPosition(spaceSize * 2, spaceSize, -spaceSize * dist);
            this.lampI.Instance[2].Manipulator.SetPosition(-spaceSize * dist, spaceSize, -spaceSize * 2);
            this.lampI.Instance[3].Manipulator.SetPosition(-spaceSize * dist, spaceSize, spaceSize * 2);

            this.lampI.Instance[0].Manipulator.SetRotation(0, pitch, 0);
            this.lampI.Instance[1].Manipulator.SetRotation(0, pitch, 0);
            this.lampI.Instance[2].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);
            this.lampI.Instance[3].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0);

            this.Lights.AddRange(this.lamp.Instance.Lights);
            this.Lights.AddRange(this.lampI.Instance[0].Lights);
            this.Lights.AddRange(this.lampI.Instance[1].Lights);
            this.Lights.AddRange(this.lampI.Instance[2].Lights);
            this.Lights.AddRange(this.lampI.Instance[3].Lights);
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
                        ContentFolder = "SceneTextures/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                });

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
                        ContentFolder = "SceneTextures/lamps",
                        ModelContentFilename = "streetlamp.xml",
                    }
                });

            this.streetlamp.Transform.SetPosition(-spaceSize, baseHeight, -spaceSize * -2f);

            this.streetlampI.Instance[0].Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * -1f);
            this.streetlampI.Instance[1].Manipulator.SetPosition(-spaceSize, baseHeight, 0);
            this.streetlampI.Instance[2].Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * 1f);
            this.streetlampI.Instance[3].Manipulator.SetPosition(-spaceSize, baseHeight, -spaceSize * 2f);

            this.streetlampI.Instance[4].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * -2f);
            this.streetlampI.Instance[5].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * -1f);
            this.streetlampI.Instance[6].Manipulator.SetPosition(+spaceSize, baseHeight, 0);
            this.streetlampI.Instance[7].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * 1f);
            this.streetlampI.Instance[8].Manipulator.SetPosition(+spaceSize, baseHeight, -spaceSize * 2f);

            this.streetlampI.Instance[4].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI.Instance[5].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI.Instance[6].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI.Instance[7].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            this.streetlampI.Instance[8].Manipulator.SetRotation(MathUtil.Pi, 0, 0);

            this.Lights.AddRange(this.streetlamp.Instance.Lights);
            this.Lights.AddRange(this.streetlampI.Instance[0].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[1].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[2].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[3].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[4].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[5].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[6].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[7].Lights);
            this.Lights.AddRange(this.streetlampI.Instance[8].Lights);
        }
        private void InitializeTestCube()
        {
            var bbox = new BoundingBox(
                -Vector3.One + Vector3.UnitY + (Vector3.UnitX * 20),
                +Vector3.One + Vector3.UnitY + (Vector3.UnitX * 20));
            var cubeTris = Triangle.ComputeTriangleList(SharpDX.Direct3D.PrimitiveTopology.TriangleList, bbox);

            var desc = new TriangleListDrawerDescription()
            {
                Name = "Test Cube",
                Triangles = cubeTris,
                Color = Color.Red,
                DepthEnabled = true,
            };

            this.testCube = this.AddComponent<TriangleListDrawer>(desc);
        }
        private void InitializeParticles()
        {
            this.pPlume = ParticleSystemDescription.InitializeSmokePlume("SceneTextures/particles", "smoke.png", 10f);
            this.pFire = ParticleSystemDescription.InitializeFire("SceneTextures/particles", "fire.png", 10f);
            this.pDust = ParticleSystemDescription.InitializeDust("SceneTextures/particles", "smoke.png", 10f);
            this.pProjectile = ParticleSystemDescription.InitializeProjectileTrail("SceneTextures/particles", "smoke.png", 10f);
            this.pExplosion = ParticleSystemDescription.InitializeExplosion("SceneTextures/particles", "fire.png", 10f);
            this.pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("SceneTextures/particles", "smoke.png", 10f);

            this.pManager = this.AddComponent<ParticleManager>(new ParticleManagerDescription() { Name = "Particle Manager" });

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
                this.particlePlumes[i] = this.pManager.Instance.AddParticleSystem(
                    ParticleSystemTypes.CPU,
                    this.pPlume,
                    new ParticleEmitter()
                    {
                        Position = positions[i],
                        InfiniteDuration = true,
                        EmissionRate = 0.05f,
                        MaximumDistance = 1000f,
                    });
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            #region Wind

            this.UpdateWind(gameTime);
            this.UpdateSkyEffects();
            this.UpdateParticles();

            #endregion

            #region Debug

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

            if (this.drawDrawVolumes)
            {
                this.UpdateLightDrawingVolumes();
            }

            if (this.drawCullVolumes)
            {
                this.UpdateLightCullingVolumes();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            #endregion

            base.Update(gameTime);

            this.runtime.Instance.Text = this.Game.RuntimeText;
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
            this.skyPlane.Instance.Direction = Vector2.Normalize(this.wind);
            this.skyPlane.Instance.Velocity = Math.Min(1f, MathUtil.Lerp(this.skyPlane.Instance.Velocity, this.wind.Length() / 100f, 0.001f));
        }
        private void UpdateParticles()
        {
            for (int i = 0; i < this.particlePlumes.Length; i++)
            {
                var gravity = new Vector3(plumeGravity.X - wind.X, plumeGravity.Y, plumeGravity.Z - wind.Y);

                this.particlePlumes[i].Parameters.Gravity = gravity;
                this.particlePlumes[i].Parameters.MaxHorizontalVelocity = plumeMaxHorizontalVelocity;
            }
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
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
        private void UpdateLightDrawingVolumes()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void UpdateLightCullingVolumes()
        {
            this.lightsVolumeDrawer.Instance.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 12, 5);

                this.lightsVolumeDrawer.Instance.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
    }
}
