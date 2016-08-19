
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Attached object
    /// </summary>
    public class GroundAttachedObject
    {
        /// <summary>
        /// Model
        /// </summary>
        public ModelBase Model = null;
        /// <summary>
        /// Gets or sets whether the object must be included in picking calculations
        /// </summary>
        public bool EvaluateForPicking = true;
        /// <summary>
        /// Gets or sets whether the object must be included in path finding calculations
        /// </summary>
        public bool EvaluateForPathFinding = true;
        /// <summary>
        /// Gets or sets whether the object must use the bounding volume for picking calculations, instead of real triangles
        /// </summary>
        public bool UseVolumeForPicking = false;
        /// <summary>
        /// Gets or sets whether the object must use the bounding volume for path finding calculations, instead of real triangles
        /// </summary>
        public bool UseVolumeForPathFinding = false;
    }
}
