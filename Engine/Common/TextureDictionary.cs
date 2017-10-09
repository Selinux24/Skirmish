using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    /// <summary>
    /// Texture by material dictionary
    /// </summary>
    [Serializable]
    public class TextureDictionary : Dictionary<string, EngineShaderResourceView>
    {
        /// <summary>
        /// Gets textures by image name
        /// </summary>
        /// <param name="image">Image name</param>
        /// <returns>Return texture by image name if exists</returns>
        public new EngineShaderResourceView this[string image]
        {
            get
            {
                if (!string.IsNullOrEmpty(image))
                {
                    if (!base.ContainsKey(image))
                    {
                        throw new KeyNotFoundException(string.Format("Texture resource not found: {0}", image));
                    }

                    return base[image];
                }

                return null;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public TextureDictionary()
            : base()
        {

        }
        /// <summary>
        /// Constructor de serialización
        /// </summary>
        /// <param name="info">Info</param>
        /// <param name="context">Context</param>
        protected TextureDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
