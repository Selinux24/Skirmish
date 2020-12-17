
namespace Engine
{
    /// <summary>
    /// Post-process Blur + Vignette parameters
    /// </summary>
    public class PostProcessBlurVignetteParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default blur parameters
        /// </summary>
        public static PostProcessBlurVignetteParams Default
        {
            get
            {
                return new PostProcessBlurVignetteParams
                {
                    Directions = 16,
                    Quality = 3,
                    Size = 4,
                    Outer = 1f,
                    Inner = 0.05f,
                };
            }
        }
        /// <summary>
        /// Strong blur parameters
        /// </summary>
        public static PostProcessBlurVignetteParams Strong
        {
            get
            {
                return new PostProcessBlurVignetteParams
                {
                    Directions = 16,
                    Quality = 3,
                    Size = 8,
                    Outer = 1f,
                    Inner = 0.05f,
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
        /// <summary>
        /// Outer vignette ring
        /// </summary>
        /// <remarks>1 by default</remarks>
        public float Outer { get; set; }
        /// <summary>
        /// Inner vignette ring
        /// </summary>
        /// <remarks>0.05 by default</remarks>
        public float Inner { get; set; }
    }
}
