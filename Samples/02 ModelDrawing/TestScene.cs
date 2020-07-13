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

        private readonly Dictionary<string, ParticleSystemDescription> pDescriptions = new Dictionary<string, ParticleSystemDescription>();
        private ParticleManager pManager = null;

        private PrimitiveListDrawer<Line3D> pManagerLineDrawer = null;

        private bool gameReady = false;

        public TestScene(Game game)
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000f;
            this.Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            this.Camera.LookTo(Vector3.Zero);

            await this.LoadResourcesAsync(
                new[]
                {
                    this.InitializeTexts(),
                    this.InitializeFloor(),
                    this.InitializeModels(),
                    this.InitializeParticleVolumeDrawer()
                },
                () =>
                {
                    gameReady = true;
                });
        }
        private async Task InitializeTexts()
        {
            this.text = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { Font = "Arial", FontSize = 20, TextColor = Color.Yellow, ShadowColor = Color.OrangeRed } }, layerHUD);
            this.statistics = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.LightBlue, ShadowColor = Color.DarkBlue } }, layerHUD);
            this.text1 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.LightBlue, ShadowColor = Color.DarkBlue } }, layerHUD);
            this.text2 = await this.AddComponentUITextArea(new UITextAreaDescription { Font = new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.LightBlue, ShadowColor = Color.DarkBlue } }, layerHUD);

            this.text.SetPosition(Vector2.One);
            this.statistics.SetPosition(Vector2.One);
            this.text1.SetPosition(Vector2.One);
            this.text2.SetPosition(Vector2.One);

            this.statistics.Top = this.text.Top + this.text.Height + 5;
            this.text1.Top = this.statistics.Top + this.statistics.Height + 5;
            this.text2.Top = this.text1.Top + this.text1.Height + 5;

            var spDesc = new SpriteDescription()
            {
                Width = this.Game.Form.RenderWidth,
                Height = this.text2.Top + this.text2.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
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
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
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

            this.pDescriptions.Add("Plume", pPlume);
            this.pDescriptions.Add("Fire", pFire);
            this.pDescriptions.Add("Dust", pDust);
            this.pDescriptions.Add("Projectile", pProjectile);
            this.pDescriptions.Add("Explosion", pExplosion);
            this.pDescriptions.Add("SmokeExplosion", pSmokeExplosion);

            this.pManager = await this.AddComponentParticleManager(new ParticleManagerDescription() { Name = "Particle Manager" });
        }
        private async Task InitializeParticleVolumeDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            this.pManagerLineDrawer = await this.AddComponentPrimitiveListDrawer(desc, SceneObjectUsages.None, layerEffects);
            this.pManagerLineDrawer.Visible = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Escape)) { this.Game.Exit(); }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            this.UpdateCamera(gameTime);

            this.UpdateSystems();

            if (this.pManagerLineDrawer.Visible)
            {
                this.DrawVolumes();
            }
        }
        private void UpdateCamera(GameTime gameTime)
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
                this.Camera.MoveLeft(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, this.Game.Input.ShiftPressed);
            }
        }
        private void UpdateSystems()
        {
            if (this.Game.Input.KeyJustPressed(Keys.D1))
            {
                this.AddSmokePlumeSystem();
            }
            if (this.Game.Input.KeyJustPressed(Keys.D2))
            {
                this.AddDustSystem();
            }
            if (this.Game.Input.KeyJustPressed(Keys.D3))
            {
                this.AddProjectileTrailSystem();
            }
            if (this.Game.Input.KeyJustPressed(Keys.D4))
            {
                this.AddExplosionSystem();
            }
            if (this.Game.Input.KeyJustPressed(Keys.D6))
            {
                this.AddSmokePlumeSystemGPU(new Vector3(5, 0, 0), new Vector3(-5, 0, 0));
            }
            if (this.Game.Input.KeyJustPressed(Keys.D7))
            {
                this.AddSmokePlumeSystemWithWind(Vector3.Normalize(new Vector3(1, 0, 1)), 20f);
            }

            if (this.Game.Input.KeyJustPressed(Keys.P))
            {
                this.AddSystem();
            }

            if (this.Game.Input.KeyJustPressed(Keys.Space))
            {
                this.pManager.Clear();
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

            this.DrawVolumes();
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Explosion"], emitter1);
            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["SmokeExplosion"], emitter2);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Projectile"], emitter);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Dust"], emitter);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Fire"], emitter1);
            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Plume"], emitter2);
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

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Fire"], emitter11);
            this.pManager.AddParticleSystem(ParticleSystemTypes.GPU, this.pDescriptions["Fire"], emitter12);

            this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Plume"], emitter21);
            this.pManager.AddParticleSystem(ParticleSystemTypes.GPU, this.pDescriptions["Plume"], emitter22);
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

            var pSystem = this.pManager.AddParticleSystem(ParticleSystemTypes.CPU, this.pDescriptions["Plume"], emitter);

            var parameters = pSystem.GetParameters();

            parameters.Gravity = wind * force;

            pSystem.SetParameters(parameters);
        }

        private readonly List<Line3D> lines = new List<Line3D>();
        private void DrawVolumes()
        {
            lines.Clear();

            var count = this.pManager.SystemsCount;
            for (int i = 0; i < count; i++)
            {
                lines.AddRange(Line3D.CreateWiredBox(this.pManager.GetParticleSystem(i).Emitter.GetBoundingBox()));
            }

            this.pManagerLineDrawer.SetPrimitives(Color.Red, lines.ToArray());
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!gameReady)
            {
                return;
            }

            var particle1 = this.pManager.GetParticleSystem(0);
            var particle2 = this.pManager.GetParticleSystem(1);

            this.text.Text = "Model Drawing";
            this.statistics.Text = this.Game.RuntimeText;
            this.text1.Text = $"P1 - {particle1}";
            this.text2.Text = $"P2 - {particle2}";
        }
    }
}
