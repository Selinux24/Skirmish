
namespace Engine.Common
{
    /// <summary>
    /// Material draw information
    /// </summary>
    public struct MaterialShadowDrawInfo
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly MaterialShadowDrawInfo Empty = new MaterialShadowDrawInfo();

        /// <summary>
        /// Material
        /// </summary>
        public IMeshMaterial Material { get; set; }
    }
}
