using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Mesh material data collection
    /// </summary>
    public class MeshMaterialDataCollection : IEnumerable<(string Name, MeshMaterialData Data)>
    {
        /// <summary>
        /// Internal list
        /// </summary>
        private readonly List<(string Name, MeshMaterialData Data)> materialList = new();

        /// <summary>
        /// Gets the internal material name list
        /// </summary>
        public string[] MaterialNames
        {
            get
            {
                return materialList.Select(m => m.Name).ToArray();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<(string Name, MeshMaterialData Data)> GetEnumerator()
        {
            return materialList.GetEnumerator();
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return materialList.GetEnumerator();
        }

        /// <summary>
        /// Adds a new material data to the collection
        /// </summary>
        /// <param name="name">Material name</param>
        /// <param name="materialData">Material data</param>
        public void Add(string name, MeshMaterialData materialData)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var current = materialList.FindIndex(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (current < 0)
            {
                materialList.Add((name, materialData));

                return;
            }

            materialList[current] = (name, materialData);
        }
        /// <summary>
        /// Gets the material list
        /// </summary>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return materialList.Select(m => m.Data.Material).ToArray();
        }
        /// <summary>
        /// Gets the material list by name
        /// </summary>
        /// <param name="name">Mesh material name</param>
        public IEnumerable<IMeshMaterial> GetMaterials(string name)
        {
            var index = materialList.FindIndex(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                yield return materialList[index].Data.Material;
            }
        }
        /// <summary>
        /// Gets the first material by name
        /// </summary>
        /// <param name="name">Mesh material name</param>
        /// <returns>Returns the material by name</returns>
        public IMeshMaterial GetFirstMaterial(string name)
        {
            var index = materialList.FindIndex(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                return materialList[index].Data.Material;
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
            var index = materialList.FindIndex(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                materialList[index].Data.Material = material;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Replaces the materials by name
        /// </summary>
        /// <param name="name">Mesh material name</param>
        /// <param name="material">Material</param>
        /// <returns>Returns true if the materials were replaced</returns>
        public bool ReplaceMaterials(string name, IMeshMaterial material)
        {
            var matDataList = materialList.FindAll(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)).Select(m => m.Data);
            if (!matDataList.Any())
            {
                return false;
            }

            foreach (var data in matDataList)
            {
                data.Material = material;
            }

            return true;
        }
        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            materialList.Clear();
        }
        /// <summary>
        /// Gets the material with the specified name
        /// </summary>
        /// <param name="name">Material name</param>
        public MeshMaterialData GetMaterialData(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            int index = materialList.FindIndex(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            return materialList[index].Data;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Materials: {MaterialNames?.Join("|")}";
        }
    }
}
