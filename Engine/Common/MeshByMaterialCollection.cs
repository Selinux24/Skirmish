using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Mesh by material collection
    /// </summary>
    public class MeshByMaterialCollection : IEnumerable<(string MaterialName, Mesh Mesh)>
    {
        /// <summary>
        /// Internal list
        /// </summary>
        private readonly List<(string MaterialName, Mesh Mesh)> meshList = new();

        /// <summary>
        /// Gets the mesh by material name
        /// </summary>
        /// <param name="materialName">Material name</param>
        public Mesh this[string materialName]
        {
            get
            {
                var current = meshList.FindIndex(i => string.Equals(i.MaterialName, materialName, StringComparison.OrdinalIgnoreCase));
                if (current < 0)
                {
                    return null;
                }

                return meshList[current].Mesh;
            }
        }
        /// <summary>
        /// Gets the mesh by material count
        /// </summary>
        public int Count
        {
            get
            {
                return meshList.Count;
            }
        }
        /// <summary>
        /// Gets the internal material name list
        /// </summary>
        public string[] MaterialNames
        {
            get
            {
                return meshList.Select(m => m.MaterialName).ToArray();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<(string MaterialName, Mesh Mesh)> GetEnumerator()
        {
            return meshList.GetEnumerator();
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return meshList.GetEnumerator();
        }

        /// <summary>
        /// Adds a new mesh to the collection
        /// </summary>
        /// <param name="materialName">Material name</param>
        /// <param name="mesh">Mesh</param>
        public void Add(string materialName, Mesh mesh)
        {
            if (string.IsNullOrWhiteSpace(materialName))
            {
                return;
            }

            var current = meshList.FindIndex(i => string.Equals(i.MaterialName, materialName, StringComparison.OrdinalIgnoreCase));
            if (current < 0)
            {
                meshList.Add((materialName, mesh));

                return;
            }

            meshList[current] = (materialName, mesh);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"MeshesByMaterial: {MaterialNames?.Join("|")}";
        }
    }
}
