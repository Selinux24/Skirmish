
namespace Engine.Common
{
    /// <summary>
    /// Updatable object
    /// </summary>
    public abstract class Updatable<T> : BaseSceneObject<T>, IUpdatable where T : SceneObjectDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        protected Updatable(Scene scene, string id, string name) :
            base(scene, id, name)
        {

        }

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
