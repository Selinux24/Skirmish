using SharpDX;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio agent interface
    /// </summary>
    public interface IGameAudioAgent
    {
        /// <summary>
        /// Forward vector
        /// </summary>
        Vector3 Forward { get; }
        /// <summary>
        /// Up vector
        /// </summary>
        Vector3 Up { get; }
        /// <summary>
        /// Position coordinate
        /// </summary>
        Vector3 Position { get; }
        /// <summary>
        /// Velocity vector
        /// </summary>
        Vector3 Velocity { get; }

        /// <summary>
        /// Sets the game audio agent position
        /// </summary>
        /// <param name="position">Fixed position</param>
        void SetSource(Vector3 position);
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="source">Manipulator instance</param>
        void SetSource(IManipulator source);
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="manipulator">Transformable instance</param>
        void SetSource(ITransformable3D source);
        /// <summary>
        /// Sets the game audio agent source
        /// </summary>
        /// <param name="source">Scene object instance</param>
        void SetSource<T>(SceneObject<T> source);
    }
}
