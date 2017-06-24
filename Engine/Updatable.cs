namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Updatable object
    /// </summary>
    public abstract class Updatable : BaseSceneObject, IUpdatable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Updatable(Scene scene, SceneObjectDescription description) : base(scene, description)
        {
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="context">Context</param>
        public abstract void Update(UpdateContext context);
    }
}
