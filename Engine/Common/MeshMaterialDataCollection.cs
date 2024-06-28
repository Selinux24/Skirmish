using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material data collection
    /// </summary>
    public class MeshMaterialDataCollection : DrawingDataCollection<MeshMaterialData>
    {
        /// <summary>
        /// Gets the material list
        /// </summary>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return GetValues().Select(v => v.Material).ToArray();
        }
        /// <summary>
        /// Gets the material list by name
        /// </summary>
        /// <param name="name">Mesh material name</param>
        public IEnumerable<IMeshMaterial> GetMaterials(string name)
        {
            var value = GetValue(name);
            if (value == null)
            {
                yield break;
            }

            yield return value.Material;
        }
        /// <summary>
        /// Gets the first material by name
        /// </summary>
        /// <param name="name">Mesh material name</param>
        /// <returns>Returns the material by name</returns>
        public IMeshMaterial GetFirstMaterial(string name)
        {
            var value = GetValue(name);
            if (value != null)
            {
                return value.Material;
            }

            return null;
        }
        /// <summary>
        /// Replaces the first material
        /// </summary>
        /// <param name="name">Mesh material name</param>
        /// <param name="material">Material</param>
        /// <returns>Returns true if the material is replaced</returns>
        public bool ReplaceFirstMaterial(string name, IMeshMaterial material)
        {
            var value = GetValue(name);
            if (value != null)
            {
                value.Material = material;

                return true;
            }

            return false;
        }
    }
}
