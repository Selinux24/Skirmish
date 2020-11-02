
namespace Engine.Common
{
    /// <summary>
    /// Material algorithms
    /// </summary>
    public enum SpecularAlgorithms : uint
    {
        /// <summary>
        /// Specular Phong
        /// </summary>
        Phong = 0,
        /// <summary>
        /// Specular Blinn-Phong
        /// </summary>
        BlinnPhong = 1,
        /// <summary>
        /// Specular Cook-Torrance
        /// </summary>
        CookTorrance = 2,
    }
}
