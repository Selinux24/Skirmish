using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Audio source
    /// </summary>
    public abstract class GameAudioSource
    {
        /// <summary>
        /// Audio source
        /// </summary>
        protected IManipulator source;

        /// <summary>
        /// Forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return source?.Forward ?? Vector3.ForwardLH;
            }
        }
        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return source?.Up ?? Vector3.Up;
            }
        }
        /// <summary>
        /// Position coordinate
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return source?.Position ?? Vector3.Zero;
            }
        }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return source?.Velocity ?? Vector3.Zero;
            }
        }
        /// <summary>
        /// Sound radius
        /// </summary>
        public float Radius { get; set; }
        /// <summary>
        /// Cone
        /// </summary>
        public GameAudioConeDescription? Cone { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceDescription">Source description</param>
        protected GameAudioSource(GameAudioSourceDescription sourceDescription)
        {
            Radius = sourceDescription.Radius;
            Cone = sourceDescription.Cone;
        }

        /// <summary>
        /// Sets the game audio source
        /// </summary>
        /// <param name="source">Manipulator instance</param>
        public void SetSource(IManipulator source)
        {
            this.source = source;
        }
        /// <summary>
        /// Sets the game audio source
        /// </summary>
        /// <param name="source">Scene object instance</param>
        public void SetSource<T>(SceneObject<T> source)
        {
            this.source = source?.Transform;
        }
    }

}
