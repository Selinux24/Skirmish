
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Updatable interface
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        void Update(UpdateContext context);
    }
}
