
namespace Engine.Common
{
    /// <summary>
    /// Updatable object
    /// </summary>
    public abstract class Updatable : BaseSceneObject, IUpdatable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Updatable(string id, string name, Scene scene, SceneDrawableDescription description) :
            base(id, name, scene, description)
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
