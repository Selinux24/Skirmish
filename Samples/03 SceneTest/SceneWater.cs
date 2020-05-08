using Engine;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace SceneTest
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
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(80, 10, 100f);
            this.Camera.LookTo(0, 0, 0);

            this.Lights.BaseFogColor = Color.White;

            await this.LoadResourcesAsync(InitializeAssets());

            this.TimeOfDay.BeginAnimation(new TimeSpan(5, 00, 00), 10f);
        }
        private async Task InitializeAssets()
        {
            await this.AddComponentWater(new WaterDescription()
            {
                Name = "Water",
                PlaneSize = 1000f,
                HeightmapIterations = 8,
                GeometryIterations = 4,
                ColorIterations = 8,
            });
            await this.AddComponentSkyScattering(new SkyScatteringDescription()
            {
                Name = "Sky",
                Resolution = SkyScatteringResolutions.High
            });

            var lfDesc = new LensFlareDescription()
            {
                Name = "Flares",
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
            await this.AddComponentLensFlare(lfDesc, SceneObjectUsages.None);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.ToggleFog();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            this.UpdateCamera(gameTime, shift, rightBtn);

            base.Update(gameTime);
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }

        private void ToggleFog()
        {
            this.Lights.FogStart = this.Lights.FogStart == 0f ? fogStart : 0f;
            this.Lights.FogRange = this.Lights.FogRange == 0f ? fogRange : 0f;
        }
    }
}
