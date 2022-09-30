
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Material
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Use transparency
        /// </summary>
        bool IsTransparent { get; set; }

        /// <summary>
        /// Converts the material to a material buffer
        /// </summary>
        /// <returns>Returns the material buffer</returns>
        BufferMaterials Convert();
    }
}
