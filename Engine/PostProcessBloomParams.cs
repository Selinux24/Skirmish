
namespace Engine
{
    /// <summary>
    /// Post-process Bloom parameters
    /// </summary>
    public struct PostProcessBloomParams : IDrawerPostProcessParams
    {
        /// <summary>
        /// Default Bloom parameters
        /// </summary>
        public static PostProcessBloomParams Default
        {
            get
            {
                return new PostProcessBloomParams
                {
                    BlurSize = 1f / 512f,
                    Intensity = 0.25f,
                };
            }
        }
        /// <summary>
        /// Low Bloom parameters
        /// </summary>
        public static PostProcessBloomParams Low
        {
            get
            {
                return new PostProcessBloomParams
                {
                    BlurSize = 1f / 512f,
                    Intensity = 0.15f,
                };
            }
        }
        /// <summary>
        /// High Bloom parameters
        /// </summary>
        public static PostProcessBloomParams High
        {
            get
            {
                return new PostProcessBloomParams
                {
                    BlurSize = 1f / 512f,
                    Intensity = 0.35f,
                };
            }
        }

        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity { get; set; }
        /// <summary>
        /// Blur size
        /// </summary>
        public float BlurSize { get; set; }
    }
}
