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
        /// Test the light shadow casting based on the viewer position
        /// </summary>
        /// <param name="environment">Game environment</param>
        /// <param name="eyePosition">Viewer eye position</param>
        /// <returns>Returns true if the light can cast shadows</returns>
        bool MarkForShadowCasting(GameEnvironment environment, Vector3 eyePosition);
        /// <summary>
        /// Gets the light volume
        /// </summary>
        /// <param name="sliceCount">Cone slice count</param>
        /// <returns>Returns a line list representing the light volume</returns>
        IEnumerable<Line3D> GetVolume(int sliceCount);
    }
}
