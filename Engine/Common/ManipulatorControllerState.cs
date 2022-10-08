
namespace Engine.Common
{
    /// <summary>
    /// Manipulator controller state
    /// </summary>
    public abstract class ManipulatorControllerState : IGameState
    {
        /// <summary>
        /// Following path
        /// </summary>
        public IControllerPath Path { get; set; }
        /// <summary>
        /// Path time
        /// </summary>
        public float PathTime { get; set; }
        /// <summary>
        /// Current velocity
        /// </summary>
        public Direction3 Velocity { get; set; }
        /// <summary>
        /// Maximum speed
        /// </summary>
        public float MaximumSpeed { get; set; }
        /// <summary>
        /// Maximum force
        /// </summary>
        public float MaximumForce { get; set; }
    }
}
