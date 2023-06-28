
namespace Engine
{
    /// <summary>
    /// Scene object interface
    /// </summary>
    public interface ISceneObject
    {
        /// <summary>
        /// Object id
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// Scene
        /// </summary>
        Scene Scene { get; }
        /// <summary>
        /// Object usage
        /// </summary>
        SceneObjectUsages Usage { get; set; }
        /// <summary>
        /// Processing layer
        /// </summary>
        int Layer { get; set; }
        /// <summary>
        /// Gets whether the current object has owner or not
        /// </summary>
        bool HasOwner { get; }
        /// <summary>
        /// Gets or sets the current object's owner
        /// </summary>
        ISceneObject Owner { get; set; }
    }
}
