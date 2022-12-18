
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Updatable interface
    /// </summary>
    public interface IUpdatable : ISceneObject
    {
        /// <summary>
        /// Updates object state before the Update call
        /// </summary>
        /// <param name="context">Update context</param>
        void EarlyUpdate(UpdateContext context);
        /// <summary>
        /// Updates object state
        /// </summary>
        /// <param name="context">Update context</param>
        void Update(UpdateContext context);
        /// <summary>
        /// Updates object state after the Update call
        /// </summary>
        /// <param name="context">Update context</param>
        void LateUpdate(UpdateContext context);
    }
}
