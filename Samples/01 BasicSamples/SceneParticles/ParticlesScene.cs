using BasicSamples.SceneStart;
using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Particles;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicSamples.SceneParticles
{
    /// <summary>
    /// Particles scene test
    /// </summary>
    public class ParticlesScene : Scene
    {
        private const string resourcesFolder = "SceneParticles";
        private const string particleSmokeFileName = "smoke.png";
        private const string particleFireFileName = "fire.png";
        private const string particlePlumeString = "Plume";
        private const string particleFireString = "Fire";
        private const string particleDustString = "Dust";
        private const string particleProjectileString = "Projectile";
        private const string particleExplosionString = "Explosion";
        private const string particleSmokeExplosionString = "SmokeExplosion";
        private const string resourcesFloor = "Common/floors/floor.png";

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea particle1 = null;
        private UITextArea particle2 = null;
        private UIConsole console = null;
        private Sprite backPanel = null;

        private readonly Dictionary<string, ParticleSystemDescription> pDescriptions = [];
        private ParticleManager pManager = null;

        private PrimitiveListDrawer<Line3D> pManagerLineDrawer = null;
        private readonly List<Line3D> lines = [];

        private bool uiReady = false;
        private bool gameReady = false;

        public ParticlesScene(Game game)
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

        public override void Initialize()
        {
            base.Initialize();

            GameEnvironment.Background = Color.CornflowerBlue;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000f;
            Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            Camera.LookTo(Vector3.Zero);

            InitializeUIObjects();
        }

        private void InitializeUIObjects()
        {
            var group = LoadResourceGroup.FromTasks(InitializeUI, InitializeUIObjectsCompleted);

            LoadResources(group);
        }
        private async Task InitializeUI()
        {
            var defaultFont20 = TextDrawerDescription.FromFamily("Arial", 20);
            var defaultFont10 = TextDrawerDescription.FromFamily("Arial", 10);

            var titleDesc = new UITextAreaDescription
            {
                Font = defaultFont20,
                Text = "Particle System Drawing",
                TextForeColor = Color.Yellow,
                TextShadowColor = Color.OrangeRed
            };
            title = await AddComponentUI<UITextArea, UITextAreaDescription>("ui1", "title", titleDesc);

            var runtimeDesc = new UITextAreaDescription
            {
                Font = defaultFont10,
                MaxTextLength = 512,
                TextForeColor = Color.LightBlue,
                TextShadowColor = Color.DarkBlue
            };
            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("ui2", "runtime", runtimeDesc);

            var particleDesc = new UITextAreaDescription
            {
                Font = defaultFont10,
                MaxTextLength = 128,
                TextForeColor = Color.LightBlue,
                TextShadowColor = Color.DarkBlue
            };
            particle1 = await AddComponentUI<UITextArea, UITextAreaDescription>("ui3", "particle1", particleDesc);
            particle2 = await AddComponentUI<UITextArea, UITextAreaDescription>("ui4", "particle2", particleDesc);

            var backPanelDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            backPanel = await AddComponentUI<Sprite, SpriteDescription>("ui5", "Backpanel", backPanelDesc, LayerUI - 1);

            var consoleDesc = UIConsoleDescription.Default(Color.DarkSlateBlue);
            consoleDesc.StartsVisible = false;
            console = await AddComponentUI<UIConsole, UIConsoleDescription>("ui6", "Console", consoleDesc, LayerUI + 1);
        }
        private void InitializeUIObjectsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            Lights.SetAmbient(new SceneLightHemispheric("Pure White", Color3.White, Color3.White, true));

            UpdateLayout();

            uiReady = true;

            InitializeSceneObjects();
        }

        private void InitializeSceneObjects()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeFloor,
                    InitializeModels,
                    InitializeParticleVolumeDrawer,
                ],
                InitializeSceneObjectsCompleted);

            LoadResources(group);
        }
        private async Task InitializeFloor()
        {
            float l = 20f;
            float h = 0f;

            var geo = GeometryUtil.CreatePlane(l, h, Vector3.Up);
            geo.Uvs = geo.Uvs.Select(uv => uv * 10f);
            geo.Transform(Matrix.RotationY(MathUtil.PiOverTwo));

            var material = MaterialBlinnPhongContent.Default;
            material.DiffuseTexture = resourcesFloor;
            material.DiffuseColor = Color.White;

            var desc = new ModelDescription()
            {
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, material),
            };

            await AddComponentGround<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeModels()
        {
            var pPlume = ParticleSystemDescription.InitializeSmokePlume(resourcesFolder, particleSmokeFileName);
            var pFire = ParticleSystemDescription.InitializeFire(resourcesFolder, particleFireFileName);
            var pDust = ParticleSystemDescription.InitializeDust(resourcesFolder, particleSmokeFileName);
            var pProjectile = ParticleSystemDescription.InitializeProjectileTrail(resourcesFolder, particleSmokeFileName);
            var pExplosion = ParticleSystemDescription.InitializeExplosion(resourcesFolder, particleFireFileName);
            var pSmokeExplosion = ParticleSystemDescription.InitializeExplosion(resourcesFolder, particleSmokeFileName);

            pDescriptions.Add(particlePlumeString, pPlume);
            pDescriptions.Add(particleFireString, pFire);
            pDescriptions.Add(particleDustString, pDust);
            pDescriptions.Add(particleProjectileString, pProjectile);
            pDescriptions.Add(particleExplosionString, pExplosion);
            pDescriptions.Add(particleSmokeExplosionString, pSmokeExplosion);

            pManager = await AddComponentEffect<ParticleManager, ParticleManagerDescription>(
                "ParticleManager",
                "ParticleManager",
                ParticleManagerDescription.Default());
        }
        private async Task InitializeParticleVolumeDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
            };
            pManagerLineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "DebugParticleDrawer",
                "DebugParticleDrawer",
                desc);
        }
        private void InitializeSceneObjectsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            gameReady = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (!uiReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<StartScene>();
            }

            if (Game.Input.KeyJustReleased(Keys.Oem5))
            {
                console.Toggle();
            }

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateCamera(gameTime);

            UpdateSystems();

            if (pManagerLineDrawer.Visible)
            {
                DrawVolumes();
            }
        }
        private void UpdateCamera(IGameTime gameTime)
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
        }
        private void UpdateSystems()
        {
            if (Game.Input.KeyJustPressed(Keys.D1))
            {
                AddSmokePlumeSystem();
            }
            if (Game.Input.KeyJustPressed(Keys.D2))
            {
                AddDustSystem();
            }
            if (Game.Input.KeyJustPressed(Keys.D3))
            {
                AddProjectileTrailSystem();
            }
            if (Game.Input.KeyJustPressed(Keys.D4))
            {
                AddExplosionSystem();
            }
            if (Game.Input.KeyJustPressed(Keys.D6))
            {
                AddSmokePlumeSystemGPU(new Vector3(5, 0, 0), new Vector3(-5, 0, 0));
            }
            if (Game.Input.KeyJustPressed(Keys.D7))
            {
                AddSmokePlumeSystemWithWind(Vector3.Normalize(new Vector3(1, 0, 1)), 20f);
            }

            if (Game.Input.KeyJustPressed(Keys.P))
            {
                AddSystem();
            }

            if (Game.Input.KeyJustPressed(Keys.Space))
            {
                pManager.Clear();
            }
        }

        private void AddSystem()
        {
            float percent = Helper.RandomGenerator.NextFloat(0, 1);
            if (percent <= 0.25f)
            {
                AddExplosionSystem();
            }
            else if (percent <= 0.50f)
            {
                AddSmokePlumeSystem();
            }
            else if (percent <= 0.75f)
            {
                AddDustSystem();
            }
            else
            {
                AddProjectileTrailSystem();
            }

            DrawVolumes();
        }
        private void AddExplosionSystem()
        {
            var position = new Vector3(Helper.RandomGenerator.NextFloat(-10, 10), 0, Helper.RandomGenerator.NextFloat(-10, 10));
            var velocity = Vector3.Up;
            float duration = 0.5f;
            float rate = 0.01f;

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };
            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration * 5f,
                EmissionRate = rate * 10f,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particleExplosionString], emitter1);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particleSmokeExplosionString], emitter2);
        }
        private void AddProjectileTrailSystem()
        {
            var emitter = new MovingEmitter()
            {
                EmissionRate = 0.005f,
                AngularVelocity = Helper.RandomGenerator.NextFloat(3, 10),
                Radius = Helper.RandomGenerator.NextFloat(5, 10),
                Duration = 3,
                MaximumDistance = 100f,
            };

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particleProjectileString], emitter);
        }
        private void AddDustSystem()
        {
            var emitter = new MovingEmitter()
            {
                EmissionRate = 0.1f,
                AngularVelocity = Helper.RandomGenerator.NextFloat(0, 1),
                Radius = Helper.RandomGenerator.NextFloat(1, 10),
                Duration = 5,
                MaximumDistance = 250f,
            };

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particleDustString], emitter);
        }
        private void AddSmokePlumeSystem()
        {
            var position = new Vector3(Helper.RandomGenerator.NextFloat(-10, 10), 0, Helper.RandomGenerator.NextFloat(-10, 10));
            var velocity = Vector3.Up;
            float duration = Helper.RandomGenerator.NextFloat(10, 60);
            float rate = Helper.RandomGenerator.NextFloat(0.1f, 1f);

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 0.5f,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };

            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration + (duration * 0.1f),
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 500f,
            };

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particleFireString], emitter1);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particlePlumeString], emitter2);
        }
        private void AddSmokePlumeSystemGPU(Vector3 positionCPU, Vector3 positionGPU)
        {
            Vector3 velocity = Vector3.Up;
            float duration = 60;
            float fireRate = 0.05f;
            float smokeRate = 0.1f;

            var emitter11 = new ParticleEmitter()
            {
                Position = positionCPU,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = fireRate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };
            var emitter21 = new ParticleEmitter()
            {
                Position = positionCPU + (Vector3.Up * 0.5f),
                Velocity = velocity,
                Duration = duration,
                EmissionRate = smokeRate,
                InfiniteDuration = false,
                MaximumDistance = 500f,
            };

            var emitter12 = new ParticleEmitter()
            {
                Position = positionGPU,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = fireRate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };
            var emitter22 = new ParticleEmitter()
            {
                Position = positionGPU + (Vector3.Up * 0.5f),
                Velocity = velocity,
                Duration = duration,
                EmissionRate = smokeRate,
                InfiniteDuration = false,
                MaximumDistance = 500f,
            };

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particleFireString], emitter11);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.GPU, pDescriptions[particleFireString], emitter12);

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particlePlumeString], emitter21);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.GPU, pDescriptions[particlePlumeString], emitter22);
        }
        private void AddSmokePlumeSystemWithWind(Vector3 wind, float force)
        {
            var emitter = new ParticleEmitter()
            {
                Position = new Vector3(0, 0, 0),
                Velocity = Vector3.Up,
                InfiniteDuration = true,
                EmissionRate = 0.001f,
                MaximumDistance = 1000f,
            };

            var pSystem = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions[particlePlumeString], emitter);

            var parameters = pSystem.GetParameters();

            parameters.Gravity = wind * force;

            pSystem.SetParameters(parameters);
        }

        private void DrawVolumes()
        {
            lines.Clear();

            var count = pManager.Count;
            for (int i = 0; i < count; i++)
            {
                var bbox = pManager.GetParticleSystem(i).Emitter.GetBoundingBox();

                lines.AddRange(Line3D.CreateBox(bbox));
            }

            pManagerLineDrawer.SetPrimitives(Color.Red, [.. lines]);
        }

        public override void Draw(IGameTime gameTime)
        {
            base.Draw(gameTime);

            if (!uiReady)
            {
                return;
            }

            runtime.Text = Game.RuntimeText;

            if (!gameReady)
            {
                return;
            }

            var particles1 = pManager.GetParticleSystem(0);
            var particles2 = pManager.GetParticleSystem(1);

            particle1.Text = $"P1 - {particles1}";
            particle2.Text = $"P2 - {particles2}";
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            runtime.SetPosition(0, title.AbsoluteRectangle.Bottom + 5);
            particle1.SetPosition(0, runtime.AbsoluteRectangle.Bottom + 5);
            particle2.SetPosition(0, particle1.AbsoluteRectangle.Bottom + 5);

            backPanel.SetPosition(Vector2.Zero);
            backPanel.Height = particle2.Top + particle2.Height + 5;
            backPanel.Width = Game.Form.RenderWidth;

            console.SetPosition(0, backPanel.AbsoluteRectangle.Bottom);
        }
    }
}
