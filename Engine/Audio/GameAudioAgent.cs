using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio agent
    /// </summary>
    public class GameAudioAgent
    {
        /// <summary>
        /// Agent's manipulator
        /// </summary>
        private IManipulator manipulator;

        /// <summary>
        /// Forward vector
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return manipulator?.FinalTransform.Forward ?? Vector3.ForwardLH;
            }
        }
        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return manipulator?.FinalTransform.Up ?? Vector3.Up;
            }
        }
        /// <summary>
        /// Position coordinate
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return manipulator?.FinalTransform.TranslationVector ?? Vector3.Zero;
            }
        }
        /// <summary>
        /// Velocity vector
        /// </summary>
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        /// <summary>
        /// Sound radius
        /// </summary>
        public float Radius { get; set; } = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public GameAudioAgent()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emitterDescription">Emitter description</param>
        public GameAudioAgent(GameAudioAgentDescription emitterDescription) : base()
        {
            Radius = emitterDescription.Radius;
        }

        /// <summary>
        /// Sets the game audio agent's manipulator
        /// </summary>
        /// <param name="manipulator">Manipulator instance</param>
        public void SetManipulator(IManipulator manipulator)
        {
            this.manipulator = manipulator;
        }
        /// <summary>
        /// Sets the game audio agent's manipulator
        /// </summary>
        /// <param name="manipulator">Transformable instance</param>
        public void SetManipulator(ITransformable3D transformable)
        {
            this.manipulator = transformable.Manipulator;
        }
        /// <summary>
        /// Sets the game audio agent's manipulator
        /// </summary>
        /// <param name="manipulator">Transformable instance</param>
        public void SetManipulator(ITransformable2D transformable)
        {
            this.manipulator = transformable.Manipulator;
        }
    }
}
