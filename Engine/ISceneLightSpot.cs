using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Spot light
    /// </summary>
    public interface ISceneLightSpot : ISceneLight, IHasGameState
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Light direction
        /// </summary>
        Vector3 Direction { get; set; }
        /// <summary>
        /// Cone angle in degrees
        /// </summary>
        float FallOffAngle { get; set; }
        /// <summary>
        /// Cone angle in radians
        /// </summary>
        float FallOffAngleRadians { get; set; }
        /// <summary>
        /// Light radius
        /// </summary>
        float Radius { get; set; }
        /// <summary>
        /// Intensity
        /// </summary>
        float Intensity { get; set; }
        /// <summary>
        /// Gets the bounding sphere of the active light
        /// </summary>
        BoundingSphere BoundingSphere { get; }
        /// <summary>
        /// Local matrix
        /// </summary>
        Matrix Local { get; }
        /// <summary>
        /// Shadow map index
        /// </summary>
        uint ShadowMapCount { get; set; }
        /// <summary>
        /// From light view * projection matrix array
        /// </summary>
        Matrix[] FromLightVP { get; set; }

        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <param name="sliceCount">Cone slice count</param>
        /// <returns>Returns a line list representing the light volume</returns>
        IEnumerable<Line3D> GetVolume(int sliceCount);

        /// <summary>
        /// Sets the shadow parameters
        /// </summary>
        /// <param name="fromLightViewProjectionArray">From light view*projection transform array</param>
        /// <param name="assignedShadowMap">Assigned shadow map index</param>
        /// <param name="shadowMapCount">Shadow map count</param>
        void SetShadowParameters(Matrix[] fromLightViewProjectionArray, int assignedShadowMap, uint shadowMapCount);
    }
}
