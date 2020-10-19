using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Scene point light without direction
    /// </summary>
    public interface ISceneLightPoint : ISceneLight
    {
        /// <summary>
        /// Position
        /// </summary>
        Vector3 Position { get; set; }
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
        /// Gets the light volume
        /// </summary>
        /// <param name="sliceCount">Sphere slice count (vertical subdivisions - meridians)</param>
        /// <param name="stackCount">Sphere stack count (horizontal subdivisions - parallels)</param>
        /// <returns>Returns a line list representing the light volume</returns>
        IEnumerable<Line3D> GetVolume(int sliceCount, int stackCount);
    }
}
