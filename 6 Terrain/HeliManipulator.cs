using Engine;
using Engine.Common;
using SharpDX;

namespace TerrainTest
{
    public class HeliManipulator : Manipulator3D
    {
        private float delta;
        private float acceleration;

        public HeliManipulator()
            : base()
        {

        }

        public override void Update(GameTime gameTime)
        {
            if (this.CurveTimeDelta < this.delta)
            {
                this.CurveTimeDelta += this.acceleration * gameTime.ElapsedMilliseconds;
            }

            if (this.CurveTimeDelta > this.delta)
            {
                this.CurveTimeDelta = delta;
            }

            base.Update(gameTime);
        }

        public override void UpdateFollowCurve(GameTime gameTime)
        {
            Vector3 p0 = this.Curve.GetPosition(this.CurveTime);
            Vector3 p1 = this.Curve.GetPosition(this.CurveTime + gameTime.ElapsedSeconds);

            float segmentDelta = Vector3.Distance(p0, p1);

            Vector3 cfw = this.Forward;
            Vector3 nfw = Vector3.Normalize(p1 - p0);
            cfw.Y = 0f;
            nfw.Y = 0f;

            float pitch = Vector3.DistanceSquared(p0, p1) * 10f;
            float roll = Helper.Angle(Vector3.Normalize(nfw), Vector3.Normalize(cfw), Vector3.Up) * 50f;

            pitch = MathUtil.Clamp(pitch, -MathUtil.PiOverFour, MathUtil.PiOverFour);
            roll = MathUtil.Clamp(roll, -MathUtil.PiOverFour, MathUtil.PiOverFour);

            roll *= pitch * 50f;

            Quaternion r =
                Helper.LookAt(p1, p0) *
                Quaternion.RotationYawPitchRoll(0, -pitch, roll);

            r = Quaternion.Slerp(this.Rotation, r, 0.1f);

            this.SetPosition(p0);
            this.SetRotation(r);

            this.CurveTime += gameTime.ElapsedSeconds * this.CurveTimeDelta;
        }

        public void Follow(ICurve curve, float delta = 1f, float acceleration = 0.01f)
        {
            base.Follow(curve, this.CurveTimeDelta);

            this.delta = delta;
            this.acceleration = acceleration;
        }
    }
}
