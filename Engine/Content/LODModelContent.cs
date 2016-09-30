using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Content
{
    /// <summary>
    /// Model content dictionary by level of detail
    /// </summary>
    [Serializable]
    public class LODModelContent : Dictionary<LevelOfDetailEnum, ModelContent>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LODModelContent()
            : base()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="geo">Model content list</param>
        /// <param name="optimize">Sets whether the content must be optimized or not</param>
        public LODModelContent(ModelContent[] geo, bool optimize)
            : base()
        {
            int lastLod = 1;
            for (int i = 0; i < geo.Length; i++)
            {
                if (optimize) geo[i].Optimize();

                this.Add((LevelOfDetailEnum)lastLod, geo[i]);

                lastLod = Helper.NextPowerOfTwo(lastLod + 1);
            }
        }
        /// <summary>
        /// Serialization constructor
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        protected LODModelContent(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
