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
        public virtual bool Active { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Updatable(string name, Scene scene, SceneObjectDescription description) : base(name, scene, description)
        {

        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public abstract void Update(UpdateContext context);
    }
}
