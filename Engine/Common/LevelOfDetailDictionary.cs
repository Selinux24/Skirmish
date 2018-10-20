using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Common
{
    /// <summary>
    /// Drawing data by level of detail dictionary
    /// </summary>
    [Serializable]
    public class LevelOfDetailDictionary : Dictionary<LevelOfDetail, DrawingData>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LevelOfDetailDictionary()
            : base()
        {

        }
        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected LevelOfDetailDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
