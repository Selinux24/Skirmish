using System.Collections.Generic;

namespace Engine.Content
{
    /// <summary>
    /// Model content dictionary by level of detail
    /// </summary>
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
        {
            int lastLod = 1;
            for (int i = 0; i < geo.Length; i++)
            {
                if (optimize) geo[i].Optimize();

                this.Add((LevelOfDetailEnum)lastLod, geo[i]);

                lastLod = Helper.NextPowerOfTwo(lastLod + 1);
            }
        }
    }
}
