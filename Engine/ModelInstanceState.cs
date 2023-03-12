
namespace Engine
{
    /// <summary>
    /// Model instance state
    /// </summary>
    public class ModelInstanceState : ISceneObjectState
    {
        /// <summary>
        /// Object id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Instance id
        /// </summary>
        public int InstanceId { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Manipulator state
        /// </summary>
        public IGameState Manipulator { get; set; }
        /// <summary>
        /// Animation controller state
        /// </summary>
        public IGameState AnimationController { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
    }
}
