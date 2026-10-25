using System.Numerics;

namespace Engine
{
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
