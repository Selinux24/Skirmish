﻿
namespace Engine
{
    /// <summary>
    /// IControllable interface
    /// </summary>
    public interface IControllable
    {
        /// <summary>
        /// Returns true if the controller is following a path
        /// </summary>
        bool HasPath { get; }

        /// <summary>
        /// Computes current position and orientation in the curve
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator to update</param>
        void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator);

        /// <summary>
        /// Sets the path to follow
        /// </summary>
        /// <param name="newPath">Path to follow</param>
        /// <param name="time">Path initial time</param>
        void Follow(IControllerPath newPath, float time = 0f);
        /// <summary>
        /// Clears current path
        /// </summary>
        void Clear();
    }
}
