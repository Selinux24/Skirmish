using Engine;
using SharpDX;

namespace TerrainTest
{
    /// <summary>
    /// Helicopter controller
    /// </summary>
    public class HeliManipulatorController : Manipulator3DController
    {
        /// <summary>
        /// Current velocity
        /// </summary>
        private Vector3 velocity;
        /// <summary>
        /// Current acceleration
        /// </summary>
        private Vector3 acceleration;
        /// <summary>
        /// Maximum force
        /// </summary>
        private float maxForce = 0.01f;

        /// <summary>
        /// Constructor
        /// </summary>
        public HeliManipulatorController()
        {

        }

        /// <summary>
        /// Updates the manipulator's view and position
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator</param>
        public override void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            if (this.HasPath)
            {
                if (this.pathTime <= this.path.Length)
                {
                    float maxspeed = manipulator.LinearVelocity * gameTime.ElapsedSeconds;

                    var target = this.path.GetNextControlPoint(this.pathTime);
                    var position = manipulator.Position;

                    // A vector pointing from the location to the target
                    var desired = target - position;
                    float dToTarget = desired.Length();
                    if (dToTarget != 0)
                    {
                        if (dToTarget < 10)
                        {
                            var m = dToTarget.Map(0, 10, 0, maxspeed);
                            desired = Vector3.Normalize(desired) * m;
                        }
                        else
                        {
                            desired = Vector3.Normalize(desired) * maxspeed;
                        }

                        // Steering = Desired minus Velocity
                        var steer = desired - this.velocity;
                        // Limit to maximum steering force
                        if (steer.Length() > maxForce)
                        {
                            steer = Vector3.Normalize(steer) * maxForce;
                        }
                        this.acceleration = steer;

                        // Update velocity
                        this.velocity += this.acceleration;
                        // Limit speed
                        if (this.velocity.Length() > maxspeed)
                        {
                            this.velocity = Vector3.Normalize(this.velocity) * maxspeed;
                        }

                        manipulator.LookAt(position - this.velocity, true, 0.02f);
                        manipulator.UpdateInternals(true);

                        manipulator.SetPosition(position + this.velocity);

                        //var nfw = Vector3.Normalize(position - this.velocity);
                        //var pitch = MathUtil.Pi / 6f;
                        //pitch *= (Vector3.Cross(nfw, this.velocity).Length() / maxspeed);

                        //var prot = Quaternion.Slerp(manipulator.Rotation, Quaternion.RotationYawPitchRoll(0, -pitch, 0), 0.2f);
                        //manipulator.SetRotation(prot, true);
                    }

                    this.pathTime += maxspeed;
                }
                else
                {
                    this.path = null;
                    this.pathTime = 0f;
                }
            }
        }
    }
}
