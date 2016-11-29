using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;
using System;

namespace ModelDrawing
{
    public class TestScene : Scene
    {
        private TextDrawer text = null;
        private TextDrawer statistics = null;

        private Model floor = null;

        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private ParticleSystemDescription pProjectile = null;
        private ParticleSystemDescription pExplosion = null;
        private ParticleSystemDescription pSmokeExplosion = null;

        private CPUParticleManager pManager = null;
        private GPUParticleManager pManagerGPU = null;

        private Random rnd = new Random();

        public TestScene(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            GameEnvironment.Background = Color.CornflowerBlue;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000f;
            this.Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            this.Camera.LookTo(Vector3.Zero);

            this.InitializeTexts();
            this.InitializeFloor();
            this.InitializeModels();
        }
        private void InitializeTexts()
        {
            this.text = this.AddText(new TextDrawerDescription() { Font = "Arial", FontSize = 20, TextColor = Color.Yellow });
            this.statistics = this.AddText(new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.DarkBlue, ShadowColor = Color.LightBlue });
            this.text.Position = Vector2.One;
            this.statistics.Position = Vector2.One;
            this.statistics.Top = this.text.Top + this.text.Height + 5;
        }
        private void InitializeFloor()
        {
            float l = 10f;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, -h, -l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, -h, +l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, l) },
                new VertexData{ Position = new Vector3(+l, -h, -l), Normal = Vector3.Up, Texture0 = new Vector2(l, 0.0f) },
                new VertexData{ Position = new Vector3(+l, -h, +l), Normal = Vector3.Up, Texture0 = new Vector2(l, l) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            var material = MaterialContent.Default;
            material.DiffuseTexture = "resources/floor.png";

            var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, material);

            this.floor = this.AddModel(content, new ModelDescription() { });
        }
        private void InitializeModels()
        {
            this.pPlume = ParticleSystemDescription.InitializeSmokePlume("resources", "smoke.png");
            this.pFire = ParticleSystemDescription.InitializeFire("resources", "fire.png");
            this.pDust = ParticleSystemDescription.InitializeDust("resources", "smoke.png");
            this.pProjectile = ParticleSystemDescription.InitializeProjectileTrail("resources", "smoke.png");
            this.pExplosion = ParticleSystemDescription.InitializeExplosion("resources", "fire.png");
            this.pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("resources", "smoke.png");

            this.pManager = this.AddParticleManager(new CPUParticleManagerDescription());
            this.pManagerGPU = this.AddParticleManager(new GPUParticleManagerDescription());
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape)) { this.Game.Exit(); }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }
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
                this.AddSmokePlumeSystemGPU();
            }

            if (this.Game.Input.KeyJustPressed(Keys.P))
            {
                this.AddSystem();
            }
        }
        private void AddSystem()
        {
            float percent = this.rnd.NextFloat(0, 1);
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
        }
        private void AddExplosionSystem()
        {
            Vector3 position = new Vector3(this.rnd.NextFloat(-10, 10), 0, this.rnd.NextFloat(-10, 10));
            Vector3 velocity = Vector3.Up;
            float duration = 0.5f;
            float rate = 0.1f;

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
            };
            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 2f,
                InfiniteDuration = false,
            };

            this.pManager.AddParticleGenerator(this.pExplosion, emitter1);
            this.pManager.AddParticleGenerator(this.pSmokeExplosion, emitter2);
        }
        private void AddProjectileTrailSystem()
        {
            var emitter = new MovingEmitter()
            {
                EmissionRate = 0.005f,
                AngularVelocity = this.rnd.NextFloat(3, 10),
                Radius = this.rnd.NextFloat(5, 10),
                Duration = 3,
            };

            this.pManager.AddParticleGenerator(this.pProjectile, emitter);
        }
        private void AddDustSystem()
        {
            var emitter = new MovingEmitter()
            {
                EmissionRate = 0.1f,
                AngularVelocity = this.rnd.NextFloat(0, 1),
                Radius = this.rnd.NextFloat(1, 10),
                Duration = 5,
            };

            this.pManager.AddParticleGenerator(this.pDust, emitter);
        }
        private void AddSmokePlumeSystem()
        {
            Vector3 position = new Vector3(this.rnd.NextFloat(-10, 10), 0, this.rnd.NextFloat(-10, 10));
            Vector3 velocity = Vector3.Up;
            float duration = this.rnd.NextFloat(10, 60);
            float rate = this.rnd.NextFloat(0.1f, 1f);

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 0.5f,
                InfiniteDuration = false,
            };

            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration + (duration * 0.1f),
                EmissionRate = rate,
                InfiniteDuration = false,
            };

            this.pManager.AddParticleGenerator(this.pFire, emitter1);
            this.pManager.AddParticleGenerator(this.pPlume, emitter2);
        }
        private void AddSmokePlumeSystemGPU()
        {
            Vector3 position = new Vector3(this.rnd.NextFloat(-10, 10), 0, this.rnd.NextFloat(-10, 10));
            Vector3 velocity = Vector3.Up;
            float duration = this.rnd.NextFloat(10, 60);
            float rate = this.rnd.NextFloat(0.1f, 1f);

            var emitter1 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 0.5f,
                InfiniteDuration = false,
            };

            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration + (duration * 0.1f),
                EmissionRate = rate,
                InfiniteDuration = false,
            };

            this.pManagerGPU.AddParticleGenerator(this.pFire, emitter1);
            this.pManagerGPU.AddParticleGenerator(this.pPlume, emitter2);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            string particle = this.pManager.ToString();

            this.text.Text = "Model Drawing " + particle;
            this.statistics.Text = this.Game.RuntimeText;
        }
    }
}
