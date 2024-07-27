using SharpDX;

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
        Matrix Local { get; set; }
    }
}
