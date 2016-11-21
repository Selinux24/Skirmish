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

        private Model floor = null;

        private CPUParticleManager pManager = null;
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
                float duration = this.rnd.NextFloat(1, 60);
                float rate = this.rnd.NextFloat(0.1f, 2f);

                this.pManager.AddParticleGenerator(this.pFire, position, Vector3.Up, duration, rate);
                this.pManager.AddParticleGenerator(this.pPlume, position, Vector3.Up, duration + (duration * 0.1f), rate);
            }
            else if (percent <= 0.66f)
            {
                this.pManager.AddParticleGenerator(this.pDust, Vector3.Zero, Vector3.Up, 60, 0.05f);
            }
            else
            {
                this.pManager.AddParticleGenerator(this.pProjectile, Vector3.Up, Vector3.Up, 60, 0.05f);
            }
        }
        private Vector3 GetPosition(float d)
        {
            Vector3 position = Vector3.Zero;
            position.X = 3.0f * d * (float)Math.Cos(0.4f * this.Game.GameTime.TotalSeconds);
            position.Y = 1f;
            position.Z = 3.0f * d * (float)Math.Sin(0.4f * this.Game.GameTime.TotalSeconds);

            return position;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            string particle = this.pManager.ToString();

            this.text.Text = "Model Drawing " + particle;
        }
    }
}
