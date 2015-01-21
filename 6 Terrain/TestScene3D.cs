using System.Collections.Generic;
using Engine;
using Engine.Common;
using SharpDX;

namespace Terrain
{
    public class TestScene3D : Scene3D
    {
        private Engine.Terrain terrain = null;
        private Engine.LineListDrawer lineDrawerOk = null;
        private Engine.LineListDrawer lineDrawerErr = null;

        public TestScene3D(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 5000f;

            this.terrain = this.AddTerrain("terrain.dae", new TerrainDescription() { });

            List<Line> oks = new List<Line>();
            List<Line> errs = new List<Line>();
            for (float x = this.terrain.BoundingBox.Minimum.X + 1; x < this.terrain.BoundingBox.Maximum.X - 1; x += 0.5f)
            {
                for (float z = this.terrain.BoundingBox.Minimum.Z + 1; z < this.terrain.BoundingBox.Maximum.Z - 1; z += 0.5f)
                {
                    Vector3 pos;
                    if (this.terrain.FindGroundPosition(x, z, out pos))
                    {
                        oks.Add(new Line(pos, pos + Vector3.Up));
                    }
                    else
                    {
                        errs.Add(new Line(x, 10, z, x, -10, z));
                    }
                }
            }

            if (oks.Count > 0) this.lineDrawerOk = this.AddLineListDrawer(oks.ToArray(), Color.Green);
            if (errs.Count > 0) this.lineDrawerErr = this.AddLineListDrawer(errs.ToArray(), Color.Red);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                if (this.lineDrawerOk != null) this.lineDrawerOk.Visible = !this.lineDrawerOk.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                if (this.lineDrawerErr != null) this.lineDrawerErr.Visible = !this.lineDrawerErr.Visible;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey) || this.Game.Input.KeyPressed(Keys.RShiftKey);

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

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
        }
    }
}
