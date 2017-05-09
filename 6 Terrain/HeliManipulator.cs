using Engine;
using Engine.Common;
using SharpDX;
using System;

namespace TerrainTest
{
    public class HeliManipulator : Manipulator3D
    {
        private Vector3 velocity;
        private Vector3 acceleration;
        private float maxSpeed = 8f;
        private float maxForce = 0.2f;

        public HeliManipulator()
            : base()
        {

        }

        protected override void UpdateFollowPath(GameTime gameTime)
        {
            float msp = this.maxSpeed * gameTime.ElapsedSeconds;
            float msf = this.maxForce * gameTime.ElapsedSeconds;

            var target = this.Path[this.PathTarget];

            // A vector pointing from the location to the target
            var desired = target - this.position;

            // Scale to maximum speed
            desired = Vector3.Normalize(desired) * msp;

            // Steering = Desired minus velocity
            var steer = desired - this.velocity;
            // Limit to maximum steering force
            if (steer.Length() > msf)
            {
                steer = Vector3.Normalize(steer) * msf;
            }

            this.acceleration += steer;

            this.velocity += this.acceleration;

            if (this.velocity.Length() > msp)
            {
                this.velocity = Vector3.Normalize(this.velocity) * msp;
            }

            this.position += this.velocity;

            this.acceleration = Vector3.Zero;


            if (Helper.WithinEpsilon(this.position, target, 0.5f))
            {
                this.PathTarget++;

                if(this.PathTarget >= this.Path.Length)
                {
                    this.PathTarget = 0;
                }
            }

            this.SetPosition(this.position);
            this.LookAt(this.position - this.velocity, true, 0.1f);
        }
    }
}
