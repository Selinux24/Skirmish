using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene pinking results
    /// </summary>
    /// <typeparam name="T"><see cref="IRayIntersectable"/> item type</typeparam>
    public struct ScenePickingResultMultiple<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Scene object
        /// </summary>
        public ISceneObject SceneObject { get; set; }
        /// <summary>
        /// Picking results
        /// </summary>
        public IEnumerable<PickingResult<T>> PickingResults { get; set; }

        /// <summary>
        /// Gets the first result
        /// </summary>
        public readonly PickingResult<T> First()
        {
            return PickingResults.FirstOrDefault();
        }
        /// <summary>
        /// Gets the las result
        /// </summary>
        /// <returns></returns>
        public readonly PickingResult<T> Last()
        {
            return PickingResults.LastOrDefault();
        }
        /// <summary>
        /// Gets the nearest result to the picking origin
        /// </summary>
        public readonly PickingResult<T> Nearest()
        {
            return PickingResults
                .OrderBy(p => p.Distance)
                .FirstOrDefault();
        }
        /// <summary>
        /// Gets the fartest result to the picking origin
        /// </summary>
        public readonly PickingResult<T> Fartest()
        {
            return PickingResults
                .OrderByDescending(p => p.Distance)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the minimum distance valur to the picking origin
        /// </summary>
        public readonly float GetMinimumDistance()
        {
            return PickingResults.Min(p => p.Distance);
        }
        /// <summary>
        /// Gets the maximum distance valur to the picking origin
        /// </summary>
        public readonly float GetMaximumDistance()
        {
            return PickingResults.Max(p => p.Distance);
        }
    }
}
