
namespace Engine.Common
{
    /// <summary>
    /// Updatable object
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public abstract class Updatable<T>(Scene scene, string id, string name) : BaseSceneObject<T>(scene, id, name), IUpdatable where T : SceneObjectDescription
    {
        /// <inheritdoc/>
        public virtual void EarlyUpdate(UpdateContext context)
        {

        }
        /// <inheritdoc/>
        public virtual void Update(UpdateContext context)
        {

        }
        /// <inheritdoc/>
        public virtual void LateUpdate(UpdateContext context)
        {

        }
    }
}
