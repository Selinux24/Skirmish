using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Mesh by mesh name dictionary
    /// </summary>
    /// <remarks>
    /// A mesh could be composed of one or more sub-meshes, depending on the number of different specified materials
    /// Key: mesh name
    /// Value: dictionary of meshes by material
    /// </remarks>
    [Serializable]
    public class MeshDictionary : Dictionary<string, MeshMaterialsDictionary>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MeshDictionary()
            : base()
        {

        }
        /// <summary>
        /// Constructor de serialización
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected MeshDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        /// <summary>
        /// Adds new mesh to dictionary
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="materialName">Material name</param>
        /// <param name="mesh">Mesh object</param>
        public void Add(string meshName, string materialName, Mesh mesh)
        {
            if (!this.ContainsKey(meshName))
            {
                this.Add(meshName, new MeshMaterialsDictionary());
            }

            this[meshName].Add(string.IsNullOrEmpty(materialName) ? ModelContent.NoMaterial : materialName, mesh);
        }
    }
}
