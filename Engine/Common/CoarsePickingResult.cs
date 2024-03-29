﻿using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Coarse picking result
    /// </summary>
    public struct CoarsePickingResult
    {
        /// <summary>
        /// Picked object
        /// </summary>
        public ISceneObject SceneObject { get; set; }
        /// <summary>
        /// Picking distance
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// Picking position
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sceneObject">Picked object</param>
        /// <param name="distance">Picking distance</param>
        /// <param name="position">Picking position</param>
        public CoarsePickingResult(ISceneObject sceneObject, float distance, Vector3 position)
        {
            SceneObject = sceneObject;
            Distance = distance;
            Position = position;
        }
    }
}
