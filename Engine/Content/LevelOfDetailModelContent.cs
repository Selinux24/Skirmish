using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Content
{
    /// <summary>
    /// Model content dictionary by level of detail
    /// </summary>
    [Serializable]
    public class LevelOfDetailModelContent : Dictionary<LevelOfDetail, ModelContent>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LevelOfDetailModelContent()
            : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geo">Model content list</param>
        /// <param name="optimize">Sets whether the content must be optimized or not</param>
        public LevelOfDetailModelContent(IEnumerable<ModelContent> geo, bool optimize)
            : base()
        {
            int lastLod = 1;
            foreach (var iGeo in geo)
            {
                if (optimize) iGeo.Optimize();

                this.Add((LevelOfDetail)lastLod, iGeo);

                lastLod = Helper.NextPowerOfTwo(lastLod + 1);
            }
        }
        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected LevelOfDetailModelContent(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
