
using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene light
    /// </summary>
    public interface ISceneLight
    {
        /// <summary>
        /// Light name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Casts shadows
        /// </summary>
        bool CastShadow { get; set; }
        /// <summary>
        /// The light is marked for shadow cast the next call
        /// </summary>
        bool CastShadowsMarked { get; set; }
        /// <summary>
        /// Diffuse color
        /// </summary>
        Color4 DiffuseColor { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        Color4 SpecularColor { get; set; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        int ShadowMapIndex { get; set; }
        /// <summary>
        /// Free use variable
        /// </summary>
        object State { get; set; }
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        /// <summary>
        /// Parent local transform matrix
        /// </summary>
        Matrix ParentTransform { get; set; }

        /// <summary>
        /// Clears all light shadow parameters
        /// </summary>
        void ClearShadowParameters();
        /// <summary>
        /// Test the light shadow casting based on the viewer position
        /// </summary>
        /// <param name="eyePosition">Viewer eye position</param>
        /// <returns>Returns true if the light can cast shadows</returns>
        bool MarkForShadowCasting(Vector3 eyePosition);
        /// <summary>
        /// Clones the light
        /// </summary>
        /// <returns>Returns a new cloned instance of the light</returns>
        ISceneLight Clone();
    }
}
