using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;

namespace ModelDrawing
{
    public class TestScene : Scene
    {
        private TextDrawer text = null;

        private Model floor = null;

        private CPUParticleManager pManager = null;
        private List<ParticleEmitter> pEmitters = new List<ParticleEmitter>();

        private CPUParticleSystemDescription pPlume = null;
        private CPUParticleSystemDescription pFire = null;
        private CPUParticleSystemDescription pDust = null;
        private CPUParticleSystemDescription pProjectile = null;

        private Random rnd = new Random();

        public TestScene(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            GameEnvironment.Background = Color.CornflowerBlue;

            var textDesc = new TextDrawerDescription()
            {
                Font = "Arial",
                FontSize = 20,
                TextColor = Color.Yellow,
            };
            this.text = this.AddText(textDesc);
            this.text.Position = Vector2.One;

            this.InitializeFloor();

            this.InitializeModels();

            this.Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            this.Camera.LookTo(Vector3.Zero);
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
            this.pManager = this.AddParticleManager(new CPUParticleManagerDescription());
            this.pPlume = CPUParticleSystemDescription.InitializeSmokePlume("resources", "smoke.png");
            this.pFire = CPUParticleSystemDescription.InitializeFire("resources", "fire.png");
            this.pDust = CPUParticleSystemDescription.InitializeDust("resources", "smoke.png");
            this.pProjectile = CPUParticleSystemDescription.InitializeProjectileTrail("resources", "smoke.png");
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

            if (this.Game.Input.KeyJustPressed(Keys.P))
            {
                this.AddSystem();
            }
        }
        private void AddSystem()
        {
            float percent = this.rnd.NextFloat(0, 1);
            if (percent <= 0.33f)
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
                    EmissionRate = rate,
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
            else if (percent <= 0.66f)
            {
                var emitter = new MovingEmitter()
                {
                    EmissionRate = 0.1f,
                    InfiniteDuration = true,
                    AngularVelocity = 1,
                    Radius = 3,
                };

                this.pManager.AddParticleGenerator(this.pDust, emitter);
            }
            else
            {
                var emitter = new MovingEmitter()
                {
                    EmissionRate = 0.005f,
                    InfiniteDuration = true,
                    AngularVelocity = 3,
                    Radius = 6,
                };

                this.pManager.AddParticleGenerator(this.pProjectile, emitter);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            string particle = this.pManager.ToString();

            this.text.Text = "Model Drawing " + particle;
        }
    }
}
