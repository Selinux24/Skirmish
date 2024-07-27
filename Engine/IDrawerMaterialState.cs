
namespace Engine
{
    /// <summary>
    /// Material state interface
    /// </summary>
    public interface IDrawerMaterialState
    {
        /// <summary>
        /// Material
        /// </summary>
        IMeshMaterial Material { get; set; }
    }
}
