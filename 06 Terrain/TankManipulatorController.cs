using Engine;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain
{
    public class TankManipulatorController : SteerManipulatorController
    {
        public override void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator)
        {
            if (this.HasPath)
            {
                var target = this.path.GetNextControlPoint(this.path.Length);
                var position = manipulator.Position;
                var rotation = manipulator.Rotation;
                float dToTarget = (target - position).Length();

                if (dToTarget > this.ArrivingThreshold)
                {
                    float maxSpeed = this.MaximumSpeed * gameTime.ElapsedSeconds;
                    float maxForce = this.MaximumForce * gameTime.ElapsedSeconds;

                    var next = this.path.GetNextControlPoint(this.pathTime + maxSpeed);

                    // A vector pointing from the location to the target
                    var desired = (next - position);
                    float dToNext = desired.Length();
                    if (dToNext != 0)
                    {
                        if (dToTarget < this.ArrivingRadius)
                        {
                            var m = dToTarget.Map(0, this.ArrivingRadius, 0, maxSpeed);
                            desired = Vector3.Normalize(desired) * m;
                        }
                        else
                        {
                            desired = Vector3.Normalize(desired) * maxSpeed;
                        }

                        // Steering = Desired minus Velocity
                        var steer = desired - this.Velocity;

                        // Limit to maximum steering force
                        steer = steer.Limit(maxForce);

                        // Update velocity
                        var newVelocity = this.Velocity + steer;

                        // Limit speed
                        newVelocity = newVelocity.Limit(maxSpeed);

                        var newTime = this.pathTime + newVelocity.Length();
                        var newPosition = this.path.GetPosition(newTime);
                        var newNormal = this.path.GetNormal(newTime);
                        var newTarget = newPosition + (newPosition - position);

                        manipulator.RotateTo(newTarget, newNormal, true, 0.01f, true);
                        if (Helper.Angle(rotation, manipulator.Rotation) <= 0.005f)
                        {
                            manipulator.SetPosition(newPosition);

                            this.pathTime = newTime;
                            this.Velocity = newVelocity;
                        }
                    }
                }
                else
                {
                    this.Clear();
                }
            }
        }
    }
}
