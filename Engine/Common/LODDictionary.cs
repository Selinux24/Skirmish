using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    /// <summary>
    /// Drawing data by level of detail dictionary
    /// </summary>
    [Serializable]
    public class LODDictionary : Dictionary<LevelOfDetail, DrawingData>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LODDictionary()
            : base()
        {

        }
        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected LODDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
