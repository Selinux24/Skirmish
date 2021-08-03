
namespace Engine
{
    /// <summary>
    /// Object has game state interface
    /// </summary>
    public interface IHasGameState
    {
        /// <summary>
        /// Gets the instance's game state
        /// </summary>
        /// <returns>Returns the instance's game state</returns>
        IGameState GetState();
        /// <summary>
        /// Sets the instance's game state
        /// </summary>
        /// <param name="state">Game state</param>
        void SetState(IGameState state);
    }
}
