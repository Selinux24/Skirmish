namespace Engine
{
    /// <summary>
    /// IControllable interface
    /// </summary>
    public interface IControllable
    {
        /// <summary>
        /// Computes current position and orientation in the curve
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator to update</param>
        void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator);

        /// <summary>
        /// Sets the path to follow
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="time">Path initial time</param>
        void Follow(IControllerPath path, float time = 0f);
        /// <summary>
        /// Clears current path
        /// </summary>
        void Clear();
    }
}
