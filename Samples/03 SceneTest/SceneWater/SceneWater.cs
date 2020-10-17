using Engine;
using SharpDX;
using System.Threading.Tasks;

namespace SceneTest.SceneWater
{
    public class SceneWater : Scene
    {
        private const float fogStart = 150f;
        private const float fogRange = 200f;

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

            await LoadResourcesAsync(InitializeAssets());

            GameEnvironment.TimeOfDay.BeginAnimation(5, 00, 00, 10f);
            //Environment.TimeOfDay.SetTimeOfDay(7, 00, 00)
        }
        private async Task InitializeAssets()
        {
            await this.AddComponentWater("Water", new WaterDescription()
            {
                PlaneSize = 1000f,
                HeightmapIterations = 8,
                GeometryIterations = 4,
                ColorIterations = 8,
            });
            await this.AddComponentSkyScattering("Sky", new SkyScatteringDescription()
            {
                Resolution = SkyScatteringResolutions.High
            });

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
            if (Game.Input.RightMouseButtonPressed)
#endif
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }

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
