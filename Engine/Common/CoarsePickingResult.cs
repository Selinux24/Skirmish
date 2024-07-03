using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Coarse picking result
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="sceneObject">Picked object</param>
    /// <param name="distance">Picking distance</param>
    /// <param name="position">Picking position</param>
    public struct CoarsePickingResult(ISceneObject sceneObject, float distance, Vector3 position)
    {
        /// <summary>
        /// Picked object
        /// </summary>
        public ISceneObject SceneObject { get; set; } = sceneObject;
        /// <summary>
        /// Picking distance
        /// </summary>
        public float Distance { get; set; } = distance;
        /// <summary>
        /// Picking position
        /// </summary>
        public Vector3 Position { get; set; } = position;
    }
}
