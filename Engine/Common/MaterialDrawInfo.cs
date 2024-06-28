
namespace Engine.Common
{
    /// <summary>
    /// Material draw information
    /// </summary>
    public struct MaterialDrawInfo
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly MaterialDrawInfo Empty = new();

        /// <summary>
        /// Material
        /// </summary>
        public IMeshMaterial Material { get; set; }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; }
    }
}
