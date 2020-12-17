using Engine;
using Engine.PostProcessing;
using SharpDX;
using System.Threading.Tasks;

namespace SceneTest.SceneWater
{
    public class SceneWater : Scene
    {
        private const float fogStart = 150f;
        private const float fogRange = 200f;

        private readonly int mapSize = 256;
        private readonly float terrainSize = 1000f;
        private readonly float terrainHeight = 20;

        public SceneWater(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
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

            Lights.BaseFogColor = Color.White;

            await LoadResourcesAsync(new[] {
                InitializeLensFlare(),
                InitializeSky(),
                InitializeWater(),
                InitializeSeaBottom(),
            });

            Renderer.SetPostProcessingEffect(PostProcessingEffects.Bloom, PostProcessBloomParams.Low);

            GameEnvironment.TimeOfDay.BeginAnimation(5, 00, 00, 10f);
            //Environment.TimeOfDay.SetTimeOfDay(7, 00, 00)
        }
        private async Task InitializeLensFlare()
        {
            var lfDesc = new LensFlareDescription()
            {
                ContentPath = @"Common/LensFlare",
                GlowTexture = "lfGlow.png",
                Flares = new[]
                {
                    new LensFlareDescription.Flare(-0.7f, 4.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare(-0.5f, 2.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare(-0.3f, 1.7f, new Color(200,  50,  50), "lfFlare2.png"),

                    new LensFlareDescription.Flare( 0.1f, 1.6f, new Color( 25,  25,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 0.3f, 1.7f, new Color(100, 255, 200), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 0.6f, 1.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.7f, 2.4f, new Color( 50, 200, 200), "lfFlare2.png"),

                    new LensFlareDescription.Flare( 1.2f, 3.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.5f, 4.5f, new Color( 50, 100,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 2.0f, 6.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            };
            await this.AddComponentLensFlare("Flares", lfDesc, SceneObjectUsages.None);
        }
        private async Task InitializeSky()
        {
            await this.AddComponentSkyScattering("Sky", new SkyScatteringDescription()
            {
                Resolution = SkyScatteringResolutions.High
            });
        }
        private async Task InitializeWater()
        {
            var wDesc = WaterDescription.CreateOcean(terrainSize, 0f);

            await this.AddComponentWater("Water", wDesc);
        }
        private async Task InitializeSeaBottom()
        {
            // Generates a random terrain using perlin noise
            NoiseMapDescriptor nmDesc = new NoiseMapDescriptor
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

            Curve heightCurve = new Curve();
            heightCurve.Keys.Add(0, 0);
            heightCurve.Keys.Add(0.4f, 0f);
            heightCurve.Keys.Add(1f, 1f);

            float cellSize = terrainSize / mapSize;

            var textures = new HeightmapTexturesDescription
            {
                ContentPath = "SceneWater",
                TexturesLR = new[] { "Diffuse.jpg" },
                NormalMaps = new[] { "Normal.jpg" },
                Scale = 0.0333f,
            };
            GroundDescription groundDesc = GroundDescription.FromHeightmap(noiseMap, cellSize, terrainHeight, heightCurve, textures, 2);
            groundDesc.Heightmap.UseFalloff = true;
            groundDesc.Heightmap.Transform = Matrix.Translation(0, -terrainHeight * 0.99f, 0);

            await this.AddComponentScenery("Sea Bottom", groundDesc);
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.F))
            {
                ToggleFog();
            }

            UpdateCamera(gameTime);

            base.Update(gameTime);
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

        private void ToggleFog()
        {
            Lights.FogStart = Lights.FogStart == 0f ? fogStart : 0f;
            Lights.FogRange = Lights.FogRange == 0f ? fogRange : 0f;
        }
    }
}
