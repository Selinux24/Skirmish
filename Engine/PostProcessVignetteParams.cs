
namespace Engine
{
    /// <summary>
    /// Post-process Vignette parameters
    /// </summary>
    public class PostProcessVignetteParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default Vignette parameters
        /// </summary>
        public static PostProcessVignetteParams Default
        {
            get
            {
                return new PostProcessVignetteParams
                {
                    Outer = 1f,
                    Inner = 0.05f,
                };
            }
        }
        /// <summary>
        /// Thin Vignette parameters
        /// </summary>
        public static PostProcessVignetteParams Thin
        {
            get
            {
                return new PostProcessVignetteParams
                {
                    Outer = 1f,
                    Inner = 0.66f,
                };
            }
        }
        /// <summary>
        /// Strong Vignette parameters
        /// </summary>
        public static PostProcessVignetteParams Strong
        {
            get
            {
                return new PostProcessVignetteParams
                {
                    Outer = 0.5f,
                    Inner = 0.1f,
                };
            }
        }

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
