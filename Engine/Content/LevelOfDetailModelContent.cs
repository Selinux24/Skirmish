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
        public LevelOfDetailModelContent(ModelContent[] geo, bool optimize)
            : base()
        {
            int lastLod = 1;
            for (int i = 0; i < geo.Length; i++)
            {
                if (optimize) geo[i].Optimize();

                this.Add((LevelOfDetail)lastLod, geo[i]);

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
