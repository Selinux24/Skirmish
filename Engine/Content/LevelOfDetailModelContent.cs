﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Content
{
    /// <summary>
    /// Model content dictionary by level of detail
    /// </summary>
    [Serializable]
    public class LevelOfDetailModelContent : Dictionary<LevelOfDetail, ContentData>
    {
        /// <summary>
        /// Builds a content dictionary by level of detail
        /// </summary>
        /// <param name="geo">Model content list</param>
        /// <param name="optimize">Sets whether the content must be optimized or not</param>
        /// <returns>Returns the content dictionary by level of detail</returns>
        public static LevelOfDetailModelContent Build(IEnumerable<ContentData> geo, bool optimize)
        {
            LevelOfDetailModelContent res = new LevelOfDetailModelContent();

            int lastLod = 1;
            foreach (var iGeo in geo)
            {
                if (optimize) iGeo.Optimize();

                res.Add((LevelOfDetail)lastLod, iGeo);

                lastLod = Helper.NextPowerOfTwo(lastLod + 1);
            }

            return res;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public LevelOfDetailModelContent()
            : base()
        {

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
