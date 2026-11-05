using Engine.Mathematics;

namespace Engine
{
    /// <summary>
    /// Manipulator 3D state
    /// </summary>
    public class Manipulator3DState : IGameState
    {
        /// <summary>
        /// Final transform for the controller
        /// </summary>
        public Matrix4x4 LocalTransform { get; set; }
        /// <summary>
        /// Rotation component
        /// </summary>
        public Quaternion Rotation { get; set; }
        /// <summary>
        /// Scaling component
        /// </summary>
        public Vector3 Scaling { get; set; }
        /// <summary>
        /// Position component
        /// </summary>
        public Vector3 Position { get; set; }
        /// <summary>
        /// Parent manipulator
        /// </summary>
        public IGameState Parent { get; set; }
    }
}
