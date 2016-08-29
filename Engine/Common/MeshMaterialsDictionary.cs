using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    /// <summary>
    /// Mesh by material dictionary
    /// </summary>
    [Serializable]
    public class MeshMaterialsDictionary : Dictionary<string, Mesh>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MeshMaterialsDictionary()
            : base()
        {

        }
        /// <summary>
        /// Constructor de serialización
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected MeshMaterialsDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
