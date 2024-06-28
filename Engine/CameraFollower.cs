using SharpDX;

namespace Engine
{
    /// <summary>
    /// Camera follower
    /// </summary>
    public class CameraFollower : IFollower
    {
        /// <summary>
        /// Manipulator to follow
        /// </summary>
        private readonly IManipulator3D manipulator;
        /// <summary>
        /// Position offset
        /// </summary>
        private readonly Vector3 positionOffset = Vector3.Zero;
        /// <summary>
        /// View offset
        /// </summary>
        private readonly Vector3 interestOffset = Vector3.ForwardLH;

        /// <inheritdoc/>
        public Vector3 Position { get; private set; }
        /// <inheritdoc/>
        public Vector3 Interest { get; private set; }
        /// <inheritdoc/>
        public float Velocity { get; set; } = 1f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public CameraFollower(IManipulator3D manipulator)
        {
            this.manipulator = manipulator;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        /// <param name="position">Position offset</param>
        /// <param name="interest">Interest offset</param>
        /// <param name="velocity">Velocity</param>
        public CameraFollower(IManipulator3D manipulator, Vector3 position, Vector3 interest, float velocity)
        {
            this.manipulator = manipulator;
            positionOffset = position;
            interestOffset = interest;
            Velocity = velocity;
        }

        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            Matrix transform = manipulator.LocalTransform;

            Vector3 desiredPosition = Vector3.TransformCoordinate(positionOffset, transform);
            Vector3 position = Vector3.Lerp(Position, desiredPosition, Velocity * gameTime.ElapsedSeconds);
            Vector3 interest = position - Vector3.TransformNormal(interestOffset, transform);

            Position = position;
            Interest = interest;
        }
    }
}
