using Engine;
using SharpDX;

namespace Collada
{
    public class Helicopter
    {
        private Vector3 pilotPosition = new Vector3(-0.15f, 1.15f, -1.7f);
        private Vector3 copilotPosition = new Vector3(0.15f, 1.15f, -1.7f);
        private Vector3 leftMachineGunPosition = new Vector3(-0.15f, 1f, 2f);
        private Vector3 rightMachineGunPosition = new Vector3(0.15f, 1f, 2f);

        private Vector3 pilotView = new Vector3(0, 0, -1);
        private Vector3 copilotView = new Vector3(0, 0, -1);
        private Vector3 leftMachineGunView = Vector3.Normalize(new Vector3(-20, -2, -10));
        private Vector3 rightMachineGunView = Vector3.Normalize(new Vector3(20, -2, -10));

        private Scenery terrain = null;
        private float pathTime = 0;
        private float pathVelocity = 1f;
        private float pathHeight = 0f;
        private BezierPath path = null;

        public Vector3 Position = Vector3.Zero;
        public Vector3 View = Vector3.ForwardLH;

        public Manipulator3D Manipulator = null;

        public Helicopter(Manipulator3D manipulator)
        {
            this.Manipulator = manipulator;

            this.SetPilot();
        }

        public void SetPilot()
        {
            this.Position = this.pilotPosition;
            this.View = this.pilotView;
        }

        public void SetCopilot()
        {
            this.Position = this.copilotPosition;
            this.View = this.copilotView;
        }

        public void SetLeftMachineGun()
        {
            this.Position = this.leftMachineGunPosition;
            this.View = this.leftMachineGunView;
        }

        public void SetRightMachineGun()
        {
            this.Position = this.rightMachineGunPosition;
            this.View = this.rightMachineGunView;
        }

        public void SetPath(Scenery terrain, Vector3[] points, float velocity, float pathHeight)
        {
            this.terrain = terrain;
            this.pathTime = 0f;
            this.pathVelocity = velocity;
            this.pathHeight = pathHeight;
            this.path = new BezierPath();
            this.path.SetControlPoints(points, 0.25f);
        }

        public void Update(GameTime gameTime)
        {
            if (this.path != null && this.pathVelocity > 0f)
            {
                float time = gameTime.ElapsedSeconds * this.pathVelocity;

                if (this.pathTime + time <= this.path.Length)
                {
                    Vector3 p0 = this.path.GetPosition(this.pathTime);
                    Vector3 p1 = this.path.GetPosition(this.pathTime + time);

                    this.terrain.FindTopGroundPosition(p0.X, p0.Z, out p0);
                    this.terrain.FindTopGroundPosition(p1.X, p1.Z, out p1);

                    p0.Y += this.pathHeight;
                    p1.Y += this.pathHeight;

                    Vector3 cfw = this.Manipulator.Forward;
                    Vector3 nfw = Vector3.Normalize(p1 - p0);
                    cfw.Y = 0f;
                    nfw.Y = 0f;

                    float pitch = Vector3.DistanceSquared(p0, p1) * (10f / this.pathVelocity);
                    float roll = Helper.Angle(Vector3.Normalize(nfw), Vector3.Normalize(cfw), Vector3.Up) * (50f / this.pathVelocity);

                    pitch = MathUtil.Clamp(pitch, -MathUtil.PiOverFour, MathUtil.PiOverFour);
                    roll = MathUtil.Clamp(roll, -MathUtil.PiOverFour, MathUtil.PiOverFour);

                    roll *= pitch / MathUtil.PiOverFour;

                    Quaternion r =
                        Helper.LookAt(p1, p0) *
                        Quaternion.RotationYawPitchRoll(0, -pitch, roll);

                    r = Quaternion.Slerp(this.Manipulator.Rotation, r, 0.5f);

                    this.Manipulator.SetPosition(p0);
                    this.Manipulator.SetRotation(r);

                    this.pathTime += time;
                }
            }
        }
    }
}
