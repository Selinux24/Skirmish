﻿
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Manipulator controller interface
    /// </summary>
    public class BasicManipulatorController : ManipulatorController
    {
        /// <summary>
        /// Arriving threshold
        /// </summary>
        public float ArrivingThreshold { get; set; } = 0.01f;

        /// <inheritdoc/>
        public override void UpdateManipulator(IGameTime gameTime, IManipulator3D manipulator)
        {
            if (!HasPath)
            {
                return;
            }

            var target = path.GetNextControlPoint(path.Length);
            var position = manipulator.Position;
            float dToTarget = (target - position).Length();

            if (dToTarget > ArrivingThreshold)
            {
                float maxSpeed = MaximumSpeed * gameTime.ElapsedSeconds;

                pathTime += maxSpeed;

                var next = path.GetPosition(pathTime);
                Velocity = next - position;

                manipulator.SetPosition(next);
                manipulator.LookAt(next + Velocity);
            }
            else
            {
                Clear();
            }
        }

        /// <inheritdoc/>
        public override IGameState GetState()
        {
            return new BasicManipulatorControllerState
            {
                Path = path,
                PathTime = pathTime,
                Velocity = Velocity,
                MaximumSpeed = MaximumSpeed,
                MaximumForce = MaximumForce,
                ArrivingThreshold = ArrivingThreshold,
            };
        }
        /// <inheritdoc/>
        public override void SetState(IGameState state)
        {
            if (state is not BasicManipulatorControllerState basicManipulator)
            {
                return;
            }

            path = basicManipulator.Path;
            pathTime = basicManipulator.PathTime;
            Velocity = basicManipulator.Velocity;
            MaximumForce = basicManipulator.MaximumForce;
            MaximumSpeed = basicManipulator.MaximumSpeed;
            ArrivingThreshold = basicManipulator.ArrivingThreshold;
        }
    }
}
