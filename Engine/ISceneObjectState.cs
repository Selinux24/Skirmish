
namespace Engine
{
    /// <summary>
    /// Scene object state interface
    /// </summary>
    public interface ISceneObjectState : IGameState
    {
        /// <summary>
        /// State's owner id
        /// </summary>
        string Id { get; set; }
    }
}
