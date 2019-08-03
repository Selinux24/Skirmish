using System.Collections.Generic;

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
        IEnumerable<MeshMaterial> Materials { get; }
    }
}
