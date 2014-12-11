using System;
using Engine;
using SharpDX;
using SharpDX.DirectInput;

namespace Skybox
{
    public class TestScene3D : Scene3D
    {
        private float globalScale = 5f;

        private Terrain ruins = null;
        private Model lamp = null;
        private TextControl title = null;
        private TextControl fps = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Game.Input.LockToCenter = true;
            this.Game.Input.HideMouse();

            TerrainDescription desc = new TerrainDescription()
            {
                AddSkydom = true,
                SkydomTexture = "sunset.dds",
            };

            this.ruins = this.AddTerrain("ruinas.dae", desc);
            this.lamp = this.AddModel("Poly.dae");

            this.Lights.PointLightEnabled = true;
            this.Lights.PointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            this.Lights.PointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            this.Lights.PointLight.Attributes = new Vector3(1.0f, 0.0f, 0.0f);
            this.Lights.PointLight.Range = 20.0f;

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.title.Text = "Collada Scene with Skybox";
            this.title.Position = Vector2.Zero;

            this.fps = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.fps.Text = null;
            this.fps.Position = new Vector2(0, 24);

            this.InitializeCamera();
            this.InitializeTerrain();
        }
        private void InitializeTerrain()
        {
            this.ruins.Manipulator.SetScale(globalScale);
            this.lamp.Manipulator.SetScale(0.01f * globalScale);
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 50.0f * globalScale;
            this.Camera.Position = Vector3.UnitY + this.ruins.Manipulator.Position * globalScale;
            this.Camera.Interest = Vector3.UnitY + Vector3.UnitZ + this.ruins.Manipulator.Position * globalScale;
            this.Camera.MovementDelta = 8f;
            this.Camera.SlowMovementDelta = 4f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera();

            Vector3 position = Vector3.Zero;

            float d = globalScale * 0.5f;

            position.X = 3.0f * d * (float)Math.Cos(0.4f * this.Game.GameTime.TotalSeconds);
            position.Y = globalScale;
            position.Z = 3.0f * d * (float)Math.Sin(0.4f * this.Game.GameTime.TotalSeconds);

            this.Lights.PointLight.Position = position;
            this.lamp.Manipulator.SetPosition(position);

            this.fps.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera()
        {
            if (this.Game.Input.KeyJustReleased(Key.Home))
            {
                if (this.Game.Input.KeyPressed(Key.LeftShift) || this.Game.Input.KeyPressed(Key.RightShift))
                {
                    this.InitializeCamera();
                }
            }

            bool slow = this.Game.Input.KeyPressed(Key.LeftShift);

            if (this.Game.Input.KeyPressed(Key.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Key.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, slow);
            }

            this.Camera.RotateMouse(
                this.Game.GameTime,
                this.Game.Input.MouseX,
                this.Game.Input.MouseY);
        }
    }
}
