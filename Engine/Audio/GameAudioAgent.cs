using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Audio agent
    /// </summary>
    class GameAudioAgent : IGameAudioAgent
    {
        /// <summary>
        /// Agent transform
        /// </summary>
        protected ITransform agentTransform = null;

        /// <summary>
        /// Forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return agentTransform?.Forward ?? Vector3.ForwardLH;
            }
        }
        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return agentTransform?.Up ?? Vector3.Up;
            }
        }
        /// <summary>
        /// Position coordinate
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return agentTransform?.Position ?? Vector3.Zero;
            }
        }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return agentTransform?.Velocity ?? Vector3.Zero;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudioAgent()
        {

        }

        /// <summary>
        /// Sets the game audio agent position
        /// </summary>
        /// <param name="position">Fixed position</param>
        public void SetSource(Vector3 position)
        {
            var source = new Manipulator3D();
            source.SetPosition(position);

            agentTransform = source;
        }
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="source">Manipulator instance</param>
        public void SetSource(ITransform source)
        {
            agentTransform = source;
        }
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="manipulator">Transformable instance</param>
        public void SetSource(ITransformable3D source)
        {
            agentTransform = source?.Manipulator;
        }
    }
}
