
namespace Engine.Common
{
    /// <summary>
    /// The instance use materials for render
    /// </summary>
    public interface IUseMaterials
    {
        /// <summary>
        /// Gets the instance materials list
        /// </summary>
        MeshMaterial[] Materials { get; }
    }
}
