namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Updatable object
    /// </summary>
    public abstract class Updatable : BaseSceneObject, IUpdatable
    {
        /// <summary>
        /// Active
        /// </summary>
        public virtual bool Active { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Updatable(string name, Scene scene, SceneObjectDescription description) :
            base(name, scene, description)
        {
            Active = description.StartsActive;
        }

        /// <summary>
        /// Updates object state before the Update call
        /// </summary>
        /// <param name="context">Update context</param>
        public virtual void EarlyUpdate(UpdateContext context)
        {

        }
        /// <summary>
        /// Updates object state
        /// </summary>
        /// <param name="context">Update context</param>
        public abstract void Update(UpdateContext context);
        /// <summary>
        /// Updates object state after the Update call
        /// </summary>
        /// <param name="context">Update context</param>
        public virtual void LateUpdate(UpdateContext context)
        {

        }
    }
}
