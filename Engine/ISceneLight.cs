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
        Color3 DiffuseColor { get; set; }
        /// <summary>
        /// Specular color
        /// </summary>
        Color3 SpecularColor { get; set; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        int ShadowMapIndex { get; set; }
        /// <summary>
        /// Shadow map count
        /// </summary>
        uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        Matrix[] FromLightVP { get; set; }
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
        /// Light position
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// Clears all light shadow parameters
        /// </summary>
        void ClearShadowParameters();
        /// <summary>
        /// Sets the shadow parameters
        /// </summary>
        /// <param name="camera">Current camera</param>
        /// <param name="assignedShadowMap">Assigned shadow map index</param>
        void SetShadowParameters(Camera camera, int assignedShadowMap);
        /// <summary>
        /// Gets the light volume for culling test
        /// </summary>
        ICullingVolume GetLightVolume();

        /// <summary>
        /// Clones the light
        /// </summary>
        /// <returns>Returns a new cloned instance of the light</returns>
        ISceneLight Clone();
    }
}
