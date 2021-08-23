using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// The instance use materials for render
    /// </summary>
    public interface IUseMaterials
    {
        /// <summary>
        /// Gets the instance materials list
        /// </summary>
        IEnumerable<IMeshMaterial> Materials { get; }

        /// <summary>
        /// Gets a material by mesh material name
        /// </summary>
        /// <param name="meshMaterialName">Name of the mesh material</param>
        /// <returns>Returns the mesh material</returns>
        IMeshMaterial GetMaterial(string meshMaterialName);
        /// <summary>
        /// Replaces the specified material
        /// </summary>
        /// <param name="meshMaterialName">Name of the mesh material to replace</param>
        /// <param name="material">Material</param>
        void ReplaceMaterial(string meshMaterialName, IMeshMaterial material);
    }
}
