using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelDrawing
{
    public class TestScene : Scene
    {
        private const int layerHUD = 99;
        private const int layerEffects = 2;

        private UITextArea text = null;
        private UITextArea statistics = null;
        private UITextArea text1 = null;
        private UITextArea text2 = null;
        private UIConsole console = null;
        private Sprite backPanel = null;

        private readonly Dictionary<string, ParticleSystemDescription> pDescriptions = new Dictionary<string, ParticleSystemDescription>();
        private ParticleManager pManager = null;

        private PrimitiveListDrawer<Line3D> pManagerLineDrawer = null;
        private readonly List<Line3D> lines = new List<Line3D>();

        private bool uiReady = false;
        private bool gameReady = false;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000f;
            Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            Camera.LookTo(Vector3.Zero);

            await LoadResourcesAsync(
                InitializeUI(),
                (uiRes) =>
                {
                    if (!uiRes.Completed)
                    {
                        uiRes.ThrowExceptions();
                    }

                    UpdateLayout();

                    uiReady = true;
                });

            await LoadResourcesAsync(
                new[]
                {
                    InitializeFloor(),
                    InitializeModels(),
                    InitializeParticleVolumeDrawer()
                },
                (gameRes) =>
                {
                    if (!gameRes.Completed)
                    {
                        gameRes.ThrowExceptions();
                    }

                    gameReady = true;
                });
        }
        private async Task InitializeUI()
        {
            text = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { FontFamily = "Arial", FontSize = 20, ForeColor = Color.Yellow, ShadowColor = Color.OrangeRed } }, layerHUD);

            statistics = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { FontFamily = "Arial", FontSize = 10, ForeColor = Color.LightBlue, ShadowColor = Color.DarkBlue } }, layerHUD);

            text1 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { FontFamily = "Arial", FontSize = 10, ForeColor = Color.LightBlue, ShadowColor = Color.DarkBlue } }, layerHUD);

            text2 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { FontFamily = "Arial", FontSize = 10, ForeColor = Color.LightBlue, ShadowColor = Color.DarkBlue } }, layerHUD);

            backPanel = await this.AddComponentSprite(SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), SceneObjectUsages.UI, layerHUD - 1);

            console = await this.AddComponentUIConsole(UIConsoleDescription.Default(Color.DarkSlateBlue), layerHUD + 1);
            console.Visible = false;
        }
        private async Task InitializeFloor()
        {
            float l = 10f;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, l) },
                new VertexData{ Position = new Vector3(+l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(l, 0.0f) },
                new VertexData{ Position = new Vector3(+l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(l, l) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            var material = MaterialContent.Default;
            material.AmbientColor = Color.White * 0.4f;
            material.DiffuseTexture = "resources/floor.png";

            var content = ModelContent.GenerateTriangleList(vertices, indices, material);

            var desc = new ModelDescription()
            {
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromModelContent(content),
            };

            await this.AddComponentModel(desc, SceneObjectUsages.Ground);
        }
        private async Task InitializeModels()
        {
            var pPlume = ParticleSystemDescription.InitializeSmokePlume("resources", "smoke.png");
            var pFire = ParticleSystemDescription.InitializeFire("resources", "fire.png");
            var pDust = ParticleSystemDescription.InitializeDust("resources", "smoke.png");
            var pProjectile = ParticleSystemDescription.InitializeProjectileTrail("resources", "smoke.png");
            var pExplosion = ParticleSystemDescription.InitializeExplosion("resources", "fire.png");
            var pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("resources", "smoke.png");

            pDescriptions.Add("Plume", pPlume);
            pDescriptions.Add("Fire", pFire);
            pDescriptions.Add("Dust", pDust);
            pDescriptions.Add("Projectile", pProjectile);
            pDescriptions.Add("Explosion", pExplosion);
            pDescriptions.Add("SmokeExplosion", pSmokeExplosion);

            pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Manager" });
        }
        private async Task InitializeParticleVolumeDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            pManagerLineDrawer = await this.AddComponentPrimitiveListDrawer(desc, SceneObjectUsages.None, layerEffects);
            pManagerLineDrawer.Visible = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!uiReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
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
        private void UpdateCamera(GameTime gameTime)
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
            Vector3 position = new Vector3(Helper.RandomGenerator.NextFloat(-10, 10), 0, Helper.RandomGenerator.NextFloat(-10, 10));
            Vector3 velocity = Vector3.Up;
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

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Explosion"], emitter1);
            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["SmokeExplosion"], emitter2);
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

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Projectile"], emitter);
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

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Dust"], emitter);
        }
        private void AddSmokePlumeSystem()
        {
            Vector3 position = new Vector3(Helper.RandomGenerator.NextFloat(-10, 10), 0, Helper.RandomGenerator.NextFloat(-10, 10));
            Vector3 velocity = Vector3.Up;
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

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Fire"], emitter1);
            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Plume"], emitter2);
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

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Fire"], emitter11);
            pManager.AddParticleSystem(ParticleSystemTypes.GPU, pDescriptions["Fire"], emitter12);

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Plume"], emitter21);
            pManager.AddParticleSystem(ParticleSystemTypes.GPU, pDescriptions["Plume"], emitter22);
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

            var pSystem = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Plume"], emitter);

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
                lines.AddRange(Line3D.CreateWiredBox(pManager.GetParticleSystem(i).Emitter.GetBoundingBox()));
            }

            pManagerLineDrawer.SetPrimitives(Color.Red, lines.ToArray());
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!uiReady)
            {
                return;
            }

            text.Text = "Particle System Drawing";
            statistics.Text = Game.RuntimeText;

            if (!gameReady)
            {
                return;
            }

            var particle1 = pManager.GetParticleSystem(0);
            var particle2 = pManager.GetParticleSystem(1);

            text1.Text = $"P1 - {particle1}";
            text2.Text = $"P2 - {particle2}";
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            text.SetPosition(Vector2.Zero);
            statistics.SetPosition(0, text.Rectangle.Bottom + 5);
            text1.SetPosition(0, statistics.Rectangle.Bottom + 5);
            text2.SetPosition(0, text1.Rectangle.Bottom + 5);

            backPanel.SetPosition(Vector2.Zero);
            backPanel.Height = text2.Top + text2.Height + 5;
            backPanel.Width = Game.Form.RenderWidth;

            console.SetPosition(0, backPanel.Rectangle.Bottom);
        }
    }
}
