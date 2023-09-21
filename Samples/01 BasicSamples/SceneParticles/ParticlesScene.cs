﻿using BasicSamples.SceneStart;
using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BasicSamples.SceneParticles
{
    public class ParticlesScene : Scene
    {
        private readonly string resourcesFolder = "SceneParticles";

        private UITextArea text = null;
        private UITextArea statistics = null;
        private UITextArea text1 = null;
        private UITextArea text2 = null;
        private UIConsole console = null;
        private Sprite backPanel = null;

        private readonly Dictionary<string, ParticleSystemDescription> pDescriptions = new();
        private ParticleManager pManager = null;

        private PrimitiveListDrawer<Line3D> pManagerLineDrawer = null;
        private readonly List<Line3D> lines = new();

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

        public override async Task Initialize()
        {
            await base.Initialize();

            GameEnvironment.Background = Color.CornflowerBlue;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000f;
            Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            Camera.LookTo(Vector3.Zero);

            InitializeUIObjects();
        }

        private void InitializeUIObjects()
        {
            LoadResourcesAsync(InitializeUI(), InitializeUIObjectsCompleted);
        }
        private async Task InitializeUI()
        {
            var defaultFont20 = TextDrawerDescription.FromFamily("Arial", 20);
            var defaultFont10 = TextDrawerDescription.FromFamily("Arial", 10);

            var textDesc = new UITextAreaDescription { Font = defaultFont20, TextForeColor = Color.Yellow, TextShadowColor = Color.OrangeRed };
            var statisticsDesc = new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue };
            var text1Desc = new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue };
            var text2Desc = new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.LightBlue, TextShadowColor = Color.DarkBlue };

            text = await AddComponentUI<UITextArea, UITextAreaDescription>("ui1", "Text", textDesc);
            statistics = await AddComponentUI<UITextArea, UITextAreaDescription>("ui2", "Statistics", statisticsDesc);
            text1 = await AddComponentUI<UITextArea, UITextAreaDescription>("ui3", "Text1", text1Desc);
            text2 = await AddComponentUI<UITextArea, UITextAreaDescription>("ui4", "Text2", text2Desc);

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
            LoadResourcesAsync(
                new[]
                {
                    InitializeFloor(),
                    InitializeModels(),
                    InitializeParticleVolumeDrawer()
                },
                InitializeSceneObjectsCompleted);
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

            var material = MaterialBlinnPhongContent.Default;
            material.DiffuseTexture = resourcesFolder + "/floor.png";

            var desc = new ModelDescription()
            {
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, material),
            };

            await AddComponentGround<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeModels()
        {
            var pPlume = ParticleSystemDescription.InitializeSmokePlume(resourcesFolder, "smoke.png");
            var pFire = ParticleSystemDescription.InitializeFire(resourcesFolder, "fire.png");
            var pDust = ParticleSystemDescription.InitializeDust(resourcesFolder, "smoke.png");
            var pProjectile = ParticleSystemDescription.InitializeProjectileTrail(resourcesFolder, "smoke.png");
            var pExplosion = ParticleSystemDescription.InitializeExplosion(resourcesFolder, "fire.png");
            var pSmokeExplosion = ParticleSystemDescription.InitializeExplosion(resourcesFolder, "smoke.png");

            pDescriptions.Add("Plume", pPlume);
            pDescriptions.Add("Fire", pFire);
            pDescriptions.Add("Dust", pDust);
            pDescriptions.Add("Projectile", pProjectile);
            pDescriptions.Add("Explosion", pExplosion);
            pDescriptions.Add("SmokeExplosion", pSmokeExplosion);

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

        public override void Update(GameTime gameTime)
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
        private void UpdateCamera(GameTime gameTime)
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
                _ = AddSmokePlumeSystemWithWind(Vector3.Normalize(new Vector3(1, 0, 1)), 20f);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Explosion"], emitter1);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["SmokeExplosion"], emitter2);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Projectile"], emitter);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Dust"], emitter);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Fire"], emitter1);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Plume"], emitter2);
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

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Fire"], emitter11);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.GPU, pDescriptions["Fire"], emitter12);

            _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Plume"], emitter21);
            _ = pManager.AddParticleSystem(ParticleSystemTypes.GPU, pDescriptions["Plume"], emitter22);
        }
        private async Task AddSmokePlumeSystemWithWind(Vector3 wind, float force)
        {
            var emitter = new ParticleEmitter()
            {
                Position = new Vector3(0, 0, 0),
                Velocity = Vector3.Up,
                InfiniteDuration = true,
                EmissionRate = 0.001f,
                MaximumDistance = 1000f,
            };

            var pSystem = await pManager.AddParticleSystem(ParticleSystemTypes.CPU, pDescriptions["Plume"], emitter);

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
                var geom = GeometryUtil.CreateBox(Topology.LineList, bbox);

                lines.AddRange(Line3D.CreateFromVertices(geom));
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
            statistics.SetPosition(0, text.AbsoluteRectangle.Bottom + 5);
            text1.SetPosition(0, statistics.AbsoluteRectangle.Bottom + 5);
            text2.SetPosition(0, text1.AbsoluteRectangle.Bottom + 5);

            backPanel.SetPosition(Vector2.Zero);
            backPanel.Height = text2.Top + text2.Height + 5;
            backPanel.Width = Game.Form.RenderWidth;

            console.SetPosition(0, backPanel.AbsoluteRectangle.Bottom);
        }
    }
}