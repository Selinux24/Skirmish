using Engine.Mathematics;
using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Scene pinking results
    /// </summary>
    /// <typeparam name="T"><see cref="IRayIntersectable"/> item type</typeparam>
    public readonly struct ScenePickingResultMultiple<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Picking results
        /// </summary>
        private readonly PickingResult<T>[] pickingResults;

        /// <summary>
        /// Scene object
        /// </summary>
        public ISceneObject SceneObject { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sceneObject">Scene object</param>
        /// <param name="pickingResults">Picking results</param>
        public ScenePickingResultMultiple(ISceneObject sceneObject, PickingResult<T>[] pickingResults)
        {
            SceneObject = sceneObject;
            this.pickingResults = pickingResults ?? [];

            if (this.pickingResults.Length > 1)
            {
                Array.Sort(this.pickingResults, (p1, p2) =>
                {
                    return p1.Distance.CompareTo(p2.Distance);
                });
            }
        }

        /// <summary>
        /// Get picking results
        /// </summary>
        public IReadOnlyCollection<PickingResult<T>> GetResults()
        {
            return pickingResults;
        }

        /// <summary>
        /// Gets the nearest result to the picking origin
        /// </summary>
        public readonly PickingResult<T> Nearest()
        {
            return pickingResults.Length > 0 ? pickingResults[0] : default;
        }
        /// <summary>
        /// Gets the fartest result to the picking origin
        /// </summary>
        public readonly PickingResult<T> Fartest()
        {
            return pickingResults.Length > 0 ? pickingResults[^1] : default;
        }

        /// <summary>
        /// Gets the minimum distance valur to the picking origin
        /// </summary>
        public readonly float GetMinimumDistance()
        {
            return pickingResults.Length > 0 ? pickingResults[0].Distance : default;
        }
        /// <summary>
        /// Gets the maximum distance valur to the picking origin
        /// </summary>
        public readonly float GetMaximumDistance()
        {
            return pickingResults.Length > 0 ? pickingResults[^1].Distance : default;
        }
    }
}
