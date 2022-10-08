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
        private readonly Manipulator3D manipulator;
        /// <summary>
        /// Position offset
        /// </summary>
        private readonly Vector3 positionOffset = Vector3.Zero;
        /// <summary>
        /// View offset
        /// </summary>
        private readonly Vector3 interestOffset = Vector3.ForwardLH;

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position { get; private set; }
        /// <summary>
        /// Interest
        /// </summary>
        public Vector3 Interest { get; private set; }
        /// <summary>
        /// Velocity
        /// </summary>
        public float Velocity { get; set; } = 1f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manipulator">Manipulator</param>
        public CameraFollower(Manipulator3D manipulator)
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
        public CameraFollower(Manipulator3D manipulator, Vector3 position, Vector3 interest, float velocity)
        {
            this.manipulator = manipulator;
            positionOffset = position;
            interestOffset = interest;
            Velocity = velocity;
        }

        /// <summary>
        /// Updates the follower position and interest
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
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
