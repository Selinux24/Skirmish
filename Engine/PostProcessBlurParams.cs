
namespace Engine
{
    /// <summary>
    /// Post-process Blur parameters
    /// </summary>
    public class PostProcessBlurParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default blur parameters
        /// </summary>
        public static PostProcessBlurParams Default
        {
            get
            {
                return new PostProcessBlurParams
                {
                    Directions = 16,
                    Quality = 3,
                    Size = 4,
                };
            }
        }
        /// <summary>
        /// Strong blur parameters
        /// </summary>
        public static PostProcessBlurParams Strong
        {
            get
            {
                return new PostProcessBlurParams
                {
                    Directions = 16,
                    Quality = 3,
                    Size = 8,
                };
            }
        }

        /// <summary>
        /// Directions
        /// </summary>
        /// <remarks>16 by default</remarks>
        public float Directions { get; set; }
        /// <summary>
        /// Quality
        /// </summary>
        /// <remarks>3 by default</remarks>
        public float Quality { get; set; }
        /// <summary>
        /// Size
        /// </summary>
        public float Size { get; set; }
    }
}
