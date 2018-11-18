
namespace Engine
{
    /// <summary>
    /// Scene light
    /// </summary>
    public interface ISceneLight
    {
        /// <summary>
        /// Casts shadows
        /// </summary>
        bool CastShadow { get; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        int ShadowMapIndex { get; set; }

        /// <summary>
        /// Clears all light shadow parameters
        /// </summary>
        void ClearShadowParameters();
    }
}
