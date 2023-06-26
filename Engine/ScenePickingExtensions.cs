using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Scene picking extensions
    /// </summary>
    public static class ScenePickingExtensions
    {
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, PickingRay ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            var cmpList = scene.GetComponentsByUsage(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            var coarseInt = RayPickingHelper.PickCoarse(cmpList, ray);
            if (!coarseInt.Any())
            {
                return false;
            }

            if (ray.RayPickingParams.HasFlag(PickingHullTypes.Coarse))
            {
                result = coarseInt
                    .OrderBy(c => c.Distance)
                    .Select(c => new ScenePickingResult<T>
                    {
                        SceneObject = c.SceneObject,
                        PickingResult = new PickingResult<T>
                        {
                            Distance = c.Distance,
                            Position = c.Position,
                        },
                    })
                    .First();

                return true;
            }

            return RayPickingHelper.PickNearest(coarseInt.Select(v => v.SceneObject), ray, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, PickingRay ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Filter by usage
            var cmpList = scene.GetComponentsByUsage(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            //Coarse filter
            var coarseInt = RayPickingHelper.PickCoarse(cmpList, ray);
            if (!coarseInt.Any())
            {
                return false;
            }

            if (ray.RayPickingParams.HasFlag(PickingHullTypes.Coarse))
            {
                result = coarseInt
                    .Select(c => new ScenePickingResult<T>
                    {
                        SceneObject = c.SceneObject,
                        PickingResult = new PickingResult<T>
                        {
                            Distance = c.Distance,
                            Position = c.Position,
                        },
                    })
                    .First();

                return true;
            }

            return RayPickingHelper.PickFirst(coarseInt.Select(v => v.SceneObject), ray, out result);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="usage">Component usage</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, PickingRay ray, SceneObjectUsages usage, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            results = Enumerable.Empty<ScenePickingResultMultiple<T>>();

            var cmpList = scene.GetComponentsByUsage(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            var coarseInt = RayPickingHelper.PickCoarse(cmpList, ray);
            if (!coarseInt.Any())
            {
                return false;
            }

            if (ray.RayPickingParams.HasFlag(PickingHullTypes.Coarse))
            {
                results = coarseInt
                    .Select(c => new ScenePickingResultMultiple<T>
                    {
                        SceneObject = c.SceneObject,
                        PickingResults = new[]
                        {
                            new PickingResult<T>
                            {
                                Distance = c.Distance,
                                Position = c.Position,
                            }
                        },
                    })
                    .ToArray();

                return true;
            }

            return RayPickingHelper.PickAll(coarseInt.Select(v => v.SceneObject), ray, out results);
        }
    }

    /// <summary>
    /// Scene pinking results
    /// </summary>
    /// <typeparam name="T"><see cref="IRayIntersectable"/> item type</typeparam>
    public struct ScenePickingResult<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Scene object
        /// </summary>
        public ISceneObject SceneObject { get; set; }
        /// <summary>
        /// Picking results
        /// </summary>
        public PickingResult<T> PickingResult { get; set; }
    }

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
