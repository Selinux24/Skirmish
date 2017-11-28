using Engine;
using SharpDX;
using System;

namespace SceneTest
{
    public class SceneWater : Scene
    {
        private SceneObject<Water> water = null;
        private SceneObject<SkyScattering> sky = null;

        public SceneWater(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

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

            this.water = this.AddComponent<Water>(new WaterDescription()
            {
                PlaneSize = 1000f,
                WaterColor = Color.SandyBrown,
            });
            this.sky = this.AddComponent<SkyScattering>(new SkyScatteringDescription());

            this.TimeOfDay.BeginAnimation(new TimeSpan(5, 00, 00), 10f);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
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
    }
}
