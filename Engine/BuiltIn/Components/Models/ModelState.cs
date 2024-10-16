
namespace Engine.BuiltIn.Components.Models
{
    using Engine.Common;

    /// <summary>
    /// Model state
    /// </summary>
    public class ModelState : BaseSceneObjectState
    {
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
