using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    /// <summary>
    /// Material by name dictionary
    /// </summary>
    [Serializable]
    public class MaterialDictionary : Dictionary<string, IMeshMaterial>
    {
        /// <summary>
        /// Default material
        /// </summary>
        private static readonly IMeshMaterial Default = MeshMaterial.DefaultBlinnPhong;

        /// <summary>
        /// Gets material description by name
        /// </summary>
        /// <param name="material">Material name</param>
        /// <returns>Return material description by name if exists</returns>
        public new IMeshMaterial this[string material]
        {
            get
            {
                if (!string.IsNullOrEmpty(material) && ContainsKey(material))
                {
                    return base[material];
                }

                return Default;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialDictionary()
            : base()
        {

        }
        /// <summary>
        /// Constructor de serialización
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected MaterialDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
