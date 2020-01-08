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
        protected IManipulator agentTransform = null;

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
            Manipulator3D source = new Manipulator3D();
            source.SetPosition(position);

            this.agentTransform = source;
        }
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="source">Manipulator instance</param>
        public void SetSource(IManipulator source)
        {
            this.agentTransform = source;
        }
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="manipulator">Transformable instance</param>
        public void SetSource(ITransformable3D source)
        {
            this.agentTransform = source?.Manipulator;
        }
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="source">Scene object instance</param>
        public void SetSource<T>(SceneObject<T> source)
        {
            this.agentTransform = source?.Transform;
        }
    }
}
