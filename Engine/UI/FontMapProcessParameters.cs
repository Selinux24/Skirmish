
namespace Engine.UI
{
    /// <summary>
    /// FontMap process parameters
    /// </summary>
    public struct FontMapProcessParameters
    {
        /// <summary>
        /// Maximum texture size constant
        /// </summary>
        public const int MAXTEXTURESIZE = 1024 * 4;
        /// <summary>
        /// Default line separation in pixels constant
        /// </summary>
        public const int LINESEPARATIONPIXELS = 10;
        /// <summary>
        /// Character separation threshold constant
        /// </summary>
        /// <remarks>Separation = Font size * Thr</remarks>
        public const float CHARSEPARATIONTHR = 0.25f;

        /// <summary>
        /// Default parameters
        /// </summary>
        public static FontMapProcessParameters Default
        {
            get
            {
                return new FontMapProcessParameters
                {
                    MaxTextureSize = MAXTEXTURESIZE,
                    LineSeparationPixels = LINESEPARATIONPIXELS,
                    CharSeparationThr = CHARSEPARATIONTHR,
                };
            }
        }

        /// <summary>
        /// Maximum texture size
        /// </summary>
        public int MaxTextureSize { get; set; }
        /// <summary>
        /// Default line separation in pixels
        /// </summary>
        public int LineSeparationPixels { get; set; }
        /// <summary>
        /// Character separation threshold, based on font size
        /// </summary>
        /// <remarks>Separation = Font size * Thr</remarks>
        public float CharSeparationThr { get; set; }
    }
}
