
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Cubemap state
    /// </summary>
    public class CubemapState : BaseSceneObjectState
    {
        /// <summary>
        /// Local transform
        /// </summary>
        public Matrix4x4 Local { get; set; }
        /// <summary>
        /// Manipulator
        /// </summary>
        public IGameState Manipulator { get; set; }
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex { get; set; }
    }
}
