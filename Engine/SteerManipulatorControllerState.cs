
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Steer manipulator controller state
    /// </summary>
    public class SteerManipulatorControllerState : ManipulatorControllerState
    {
        /// <summary>
        /// Arriving radius
        /// </summary>
        public float ArrivingRadius { get; set; }
        /// <summary>
        /// Arriving threshold
        /// </summary>
        public float ArrivingThreshold { get; set; }
    }
}
