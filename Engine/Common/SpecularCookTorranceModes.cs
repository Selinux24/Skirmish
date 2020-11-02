
namespace Engine.Common
{
    /// <summary>
    /// Specular Cook-Torrance modes
    /// </summary>
    public enum SpecularCookTorranceModes : uint
    {
        /// <summary>
        /// Look-up
        /// </summary>
        LookUp = 0,
        /// <summary>
        /// Beckmann
        /// </summary>
        Beckmann = 1,
        /// <summary>
        /// Gaussian
        /// </summary>
        Gaussian = 2,
    }
}
