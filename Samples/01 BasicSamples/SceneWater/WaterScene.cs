﻿using Engine;
using Engine.Tween;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace BasicSamples.SceneWater
{
    public class WaterScene : Scene
    {
        private const string GlowString = "lfGlow.png";
        private const string Flare1String = "lfFlare1.png";
        private const string Flare2String = "lfFlare2.png";
        private const string Flare3String = "lfFlare3.png";

        private const float fogStart = 150f;
        private const float fogRange = 200f;

        private readonly int mapSize = 256;
        private readonly float terrainSize = 1000f;
        private readonly float terrainHeight = 20;

        public WaterScene(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(80, 10, 100f);
            Camera.LookTo(0, 0, 0);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Lights.BaseFogColor = Color.White;

            Renderer.PostProcessingObjectsEffects.AddBloomLow();

            GameEnvironment.TimeOfDay.BeginAnimation(5, 00, 00, 10f);

            InitializeComponents();
        }
        private void InitializeComponents()
        {
            LoadResourcesAsync(
            [
                InitializeLensFlare(),
                InitializeSky(),
                InitializeWater(),
                InitializeSeaBottom(),
            ],
            (res) => { res.ThrowExceptions(); });
        }
        private async Task InitializeLensFlare()
        {
            var lfDesc = new LensFlareDescription()
            {
                ContentPath = @"Common/LensFlare",
                GlowTexture = GlowString,
                Flares =
                [
                    new (-0.7f, 4.7f, new Color( 50, 100,  25), Flare3String),
                    new (-0.5f, 2.7f, new Color( 50,  25,  50), Flare1String),
                    new (-0.3f, 1.7f, new Color(200,  50,  50), Flare2String),

                    new ( 0.1f, 1.6f, new Color( 25,  25,  25), Flare3String),
                    new ( 0.3f, 1.7f, new Color(100, 255, 200), Flare1String),
                    new ( 0.6f, 1.9f, new Color( 50, 100,  50), Flare2String),
                    new ( 0.7f, 2.4f, new Color( 50, 200, 200), Flare2String),

                    new ( 1.2f, 3.0f, new Color(100,  50,  50), Flare1String),
                    new ( 1.5f, 4.5f, new Color( 50, 100,  50), Flare1String),
                    new ( 2.0f, 6.4f, new Color( 25,  50, 100), Flare3String),
                ]
            };

            await AddComponentEffect<LensFlare, LensFlareDescription>("Flares", "Flares", lfDesc);
        }
        private async Task InitializeSky()
        {
            var desc = new SkyScatteringDescription()
            {
                Resolution = SkyScatteringResolutions.High
            };

            await AddComponentSky<SkyScattering, SkyScatteringDescription>("Sky", "Sky", desc);
        }
        private async Task InitializeWater()
        {
            var desc = WaterDescription.CreateOcean(terrainSize, 0f);

            await AddComponentEffect<Water, WaterDescription>("Water", "Water", desc);
        }
        private async Task InitializeSeaBottom()
        {
            // Generates a random terrain using perlin noise
            var nmDesc = new NoiseMapDescriptor
            {
                MapWidth = mapSize,
                MapHeight = mapSize,
                Scale = 0.5f,
                Lacunarity = 2f,
                Persistance = 0.5f,
                Octaves = 4,
                Offset = Vector2.One,
                Seed = 150,
            };
            var noiseMap = NoiseMap.CreateNoiseMap(nmDesc);

            var heightCurve = new Curve();
            heightCurve.Keys.Add(0, 0);
            heightCurve.Keys.Add(0.4f, 0f);
            heightCurve.Keys.Add(1f, 1f);

            float cellSize = terrainSize / mapSize;

            var textures = new HeightmapTexturesDescription
            {
                ContentPath = "SceneWater",
                TexturesLR = ["Diffuse.jpg"],
                NormalMaps = ["Normal.jpg"],
                Scale = 0.0333f,
            };
            var groundDesc = GroundDescription.FromHeightmap(noiseMap, cellSize, terrainHeight, heightCurve, textures, 2);
            groundDesc.Heightmap.UseFalloff = true;
            groundDesc.Heightmap.Transform = Matrix.Translation(0, -terrainHeight * 0.99f, 0);

            await AddComponentGround<Scenery, GroundDescription>("SeaBottom", "Sea Bottom", groundDesc);
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (Game.Input.KeyJustReleased(Keys.F))
            {
                ToggleFog();
            }

            UpdateCamera(gameTime);
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

            float gradient = (-Vector3.Dot(Lights.KeyLight.Direction, Camera.Direction) + 1f) * 0.5f;
            gradient = Math.Min(0.5f, gradient);
            Renderer.PostProcessingObjectsEffects.BloomForce = ScaleFuncs.CubicEaseIn(gradient);
        }

        private void ToggleFog()
        {
            Lights.FogStart = MathUtil.IsZero(Lights.FogStart) ? fogStart : 0f;
            Lights.FogRange = MathUtil.IsZero(Lights.FogRange) ? fogRange : 0f;
        }
    }
}
