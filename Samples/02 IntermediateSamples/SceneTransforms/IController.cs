using Engine;

namespace IntermediateSamples.SceneTransforms
{
    /// <summary>
    /// Controller interface
    /// </summary>
    interface IController
    {
        /// <summary>
        /// Active
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Updates the controller
        /// </summary>
        /// <param name="gameTime">Game time</param>
        void UpdateController(IGameTime gameTime);
    }
}
