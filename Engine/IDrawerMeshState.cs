
namespace Engine
{
    using Engine.Mathematics;

    /// <summary>
    /// Drawer mesh state interface
    /// </summary>
    public interface IDrawerMeshState
    {
        /// <summary>
        /// Local transform
        /// </summary>
        Matrix4x4 Local { get; set; }
    }
}
