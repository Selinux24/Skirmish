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
        public bool Active { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Updatable(Scene scene, SceneObjectDescription description) : base(scene, description)
        {

        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void Update(UpdateContext context)
        {
            
        }
    }
}
