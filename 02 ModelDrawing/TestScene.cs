using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;

namespace ModelDrawing
{
    public class TestScene : Scene
    {
        private const int layerHUD = 99;
        private const int layerEffects = 2;

        private SceneObject<TextDrawer> text = null;
        private SceneObject<TextDrawer> statistics = null;
        private SceneObject<TextDrawer> text1 = null;
        private SceneObject<TextDrawer> text2 = null;

        private ParticleSystemDescription pPlume = null;
        private ParticleSystemDescription pFire = null;
        private ParticleSystemDescription pDust = null;
        private ParticleSystemDescription pProjectile = null;
        private ParticleSystemDescription pExplosion = null;
        private ParticleSystemDescription pSmokeExplosion = null;

        private SceneObject<ParticleManager> pManager = null;
        private SceneObject<LineListDrawer> pManagerLineDrawer = null;

        private readonly Random rnd = new Random();

        public TestScene(Game game)
            : base(game, SceneModes.ForwardLigthning)
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
            this.InitializeParticleVolumeDrawer();
        }
        private void InitializeTexts()
        {
            this.text = this.AddComponent<TextDrawer>(new TextDrawerDescription() { Font = "Arial", FontSize = 20, TextColor = Color.Yellow, ShadowColor = Color.OrangeRed }, SceneObjectUsages.UI, layerHUD);
            this.statistics = this.AddComponent<TextDrawer>(new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.LightBlue, ShadowColor = Color.DarkBlue }, SceneObjectUsages.UI, layerHUD);
            this.text1 = this.AddComponent<TextDrawer>(new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.LightBlue, ShadowColor = Color.DarkBlue }, SceneObjectUsages.UI, layerHUD);
            this.text2 = this.AddComponent<TextDrawer>(new TextDrawerDescription() { Font = "Arial", FontSize = 10, TextColor = Color.LightBlue, ShadowColor = Color.DarkBlue }, SceneObjectUsages.UI, layerHUD);

            this.text.Instance.Position = Vector2.One;
            this.statistics.Instance.Position = Vector2.One;
            this.text1.Instance.Position = Vector2.One;
            this.text2.Instance.Position = Vector2.One;

            this.statistics.Instance.Top = this.text.Instance.Top + this.text.Instance.Height + 5;
            this.text1.Instance.Top = this.statistics.Instance.Top + this.statistics.Instance.Height + 5;
            this.text2.Instance.Top = this.text1.Instance.Top + this.text1.Instance.Height + 5;

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.text2.Instance.Top + this.text2.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.AddComponent<Sprite>(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private void InitializeFloor()
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

            this.AddComponent<Model>(desc, SceneObjectUsages.Ground);
        }
        private void InitializeModels()
        {
            this.pPlume = ParticleSystemDescription.InitializeSmokePlume("resources", "smoke.png");
            this.pFire = ParticleSystemDescription.InitializeFire("resources", "fire.png");
            this.pDust = ParticleSystemDescription.InitializeDust("resources", "smoke.png");
            this.pProjectile = ParticleSystemDescription.InitializeProjectileTrail("resources", "smoke.png");
            this.pExplosion = ParticleSystemDescription.InitializeExplosion("resources", "fire.png");
            this.pSmokeExplosion = ParticleSystemDescription.InitializeExplosion("resources", "smoke.png");

            this.pManager = this.AddComponent<ParticleManager>(new ParticleManagerDescription());
        }
        private void InitializeParticleVolumeDrawer()
        {
            var desc = new LineListDrawerDescription()
            {
                Count = 20000
            };
            this.pManagerLineDrawer = this.AddComponent<LineListDrawer>(desc, SceneObjectUsages.None, layerEffects);
            this.pManagerLineDrawer.Visible = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
                this.AddSmokePlumeSystemGPU();
            }

            if (this.Game.Input.KeyJustPressed(Keys.P))
            {
                this.AddSystem();
            }

            if (this.Game.Input.KeyJustPressed(Keys.Space))
            {
                this.pManager.Instance.Clear();
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

            this.DrawVolumes();
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
                MaximumDistance = 100f,
            };
            var emitter2 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate * 2f,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pExplosion, emitter1);
            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pSmokeExplosion, emitter2);
        }
        private void AddProjectileTrailSystem()
        {
            var emitter = new MovingEmitter()
            {
                EmissionRate = 0.005f,
                AngularVelocity = this.rnd.NextFloat(3, 10),
                Radius = this.rnd.NextFloat(5, 10),
                Duration = 3,
                MaximumDistance = 100f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pProjectile, emitter);
        }
        private void AddDustSystem()
        {
            var emitter = new MovingEmitter()
            {
                EmissionRate = 0.1f,
                AngularVelocity = this.rnd.NextFloat(0, 1),
                Radius = this.rnd.NextFloat(1, 10),
                Duration = 5,
                MaximumDistance = 250f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pDust, emitter);
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

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pFire, emitter1);
            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pPlume, emitter2);
        }
        private void AddSmokePlumeSystemGPU()
        {
            Vector3 position = new Vector3(-5, 0, 0);
            Vector3 velocity = Vector3.Up;
            float duration = 60;
            float rate = 0.1f;

            var emitter11 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };
            var emitter21 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 500f,
            };

            position = new Vector3(5, 0, 0);

            var emitter12 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 100f,
            };
            var emitter22 = new ParticleEmitter()
            {
                Position = position,
                Velocity = velocity,
                Duration = duration,
                EmissionRate = rate,
                InfiniteDuration = false,
                MaximumDistance = 500f,
            };

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pFire, emitter11);
            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.GPU, this.pFire, emitter12);

            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.CPU, this.pPlume, emitter21);
            this.pManager.Instance.AddParticleSystem(ParticleSystemTypes.GPU, this.pPlume, emitter22);
        }

        private readonly List<Line3D> lines = new List<Line3D>();
        private void DrawVolumes()
        {
            lines.Clear();

            var count = this.pManager.Instance.Count;
            for (int i = 0; i < count; i++)
            {
                lines.AddRange(Line3D.CreateWiredBox(this.pManager.Instance.GetParticleSystem(i).Emitter.GetBoundingBox()));
            }

            this.pManagerLineDrawer.Instance.SetLines(Color.Red, lines.ToArray());
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            var particle1 = this.pManager.Instance.GetParticleSystem(0);
            var particle2 = this.pManager.Instance.GetParticleSystem(1);

            this.text.Instance.Text = "Model Drawing";
            this.statistics.Instance.Text = this.Game.RuntimeText;
            this.text1.Instance.Text = string.Format("P1 - {0}", particle1);
            this.text2.Instance.Text = string.Format("P2 - {0}", particle2);
        }
    }
}
