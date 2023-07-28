using SharpDX;

namespace Engine
{
    /// <summary>
    /// Directional light
    /// </summary>
    public interface ISceneLightDirectional : ISceneLight, IHasGameState
    {
        /// <summary>
        /// Light direction
        /// </summary>
        Vector3 Direction { get; set; }
        /// <summary>
        /// Base brightness
        /// </summary>
        float BaseBrightness { get; set; }
        /// <summary>
        /// Light brightness
        /// </summary>
        float Brightness { get; set; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        Matrix ToShadowSpace { get; set; }
        /// <summary>
        /// X cascade offset
        /// </summary>
        Vector4 ToCascadeOffsetX { get; set; }
        /// <summary>
        /// Y cascade offset
        /// </summary>
        Vector4 ToCascadeOffsetY { get; set; }
        /// <summary>
        /// Cascade scale
        /// </summary>
        Vector4 ToCascadeScale { get; set; }

        /// <summary>
        /// Gets light position at specified distance
        /// </summary>
        /// <param name="distance">Distance</param>
        /// <returns>Returns light position at specified distance</returns>
        Vector3 GetPosition(float distance);

        /// <summary>
        /// Sets the shadow parameters
        /// </summary>
        /// <param name="assignedShadowMap">Assigned shadow map index</param>
        /// <param name="shadowMapCount">Shadow map count</param>
        void SetShadowParameters(int assignedShadowMap, uint shadowMapCount);
    }
}
