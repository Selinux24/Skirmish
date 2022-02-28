using SharpDX;
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
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, float.MaxValue, RayPickingParams.Default, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, float.MaxValue, RayPickingParams.Default, usage, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, float.MaxValue, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, float.MaxValue, rayPickingParams, usage, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, float rayLength, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, rayLength, RayPickingParams.Default, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, float rayLength, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, rayLength, RayPickingParams.Default, usage, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(scene, ray, rayLength, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(this Scene scene, Ray ray, float rayLength, RayPickingParams rayPickingParams, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            var cmpList = scene.GetComponentsByUsage(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            var volumes = RayPickingHelper.PickVolumes(cmpList, ray, rayLength);
            if (!volumes.Any())
            {
                return false;
            }

            if (rayPickingParams.HasFlag(RayPickingParams.Volumes))
            {
                result = volumes
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

            return RayPickingHelper.PickNearest(volumes.Select(v => v.SceneObject), ray, rayLength, rayPickingParams, out result);
        }

        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, float.MaxValue, RayPickingParams.Default, SceneObjectUsages.None, out result);
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
        public static bool PickFirst<T>(this Scene scene, Ray ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, float.MaxValue, RayPickingParams.Default, usage, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, float.MaxValue, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, float.MaxValue, rayPickingParams, usage, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, float rayLength, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, rayLength, RayPickingParams.Default, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, float rayLength, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, rayLength, RayPickingParams.Default, usage, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(scene, ray, rayLength, rayPickingParams, SceneObjectUsages.None, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(this Scene scene, Ray ray, float rayLength, RayPickingParams rayPickingParams, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Filter by usage
            var cmpList = scene.GetComponentsByUsage(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            //Coarse filter
            var volumes = RayPickingHelper.PickVolumes(cmpList, ray, rayLength);
            if (!volumes.Any())
            {
                return false;
            }

            if (rayPickingParams.HasFlag(RayPickingParams.Volumes))
            {
                result = volumes
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

            return RayPickingHelper.PickFirst(volumes.Select(v => v.SceneObject), ray, rayLength, rayPickingParams, out result);
        }

        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, float.MaxValue, RayPickingParams.Default, SceneObjectUsages.None, out results);
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
        public static bool PickAll<T>(this Scene scene, Ray ray, SceneObjectUsages usage, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, float.MaxValue, RayPickingParams.Default, usage, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, RayPickingParams rayPickingParams, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, float.MaxValue, rayPickingParams, SceneObjectUsages.None, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, RayPickingParams rayPickingParams, SceneObjectUsages usage, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, float.MaxValue, rayPickingParams, usage, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, float rayLength, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, rayLength, RayPickingParams.Default, SceneObjectUsages.None, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="usage">Component usage</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, float rayLength, SceneObjectUsages usage, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, rayLength, RayPickingParams.Default, usage, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            return PickAll(scene, ray, rayLength, rayPickingParams, SceneObjectUsages.None, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="usage">Component usage</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(this Scene scene, Ray ray, float rayLength, RayPickingParams rayPickingParams, SceneObjectUsages usage, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            results = Enumerable.Empty<ScenePickingResultMultiple<T>>();

            var cmpList = scene.GetComponentsByUsage(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            var volumes = RayPickingHelper.PickVolumes(cmpList, ray, rayLength);
            if (!volumes.Any())
            {
                return false;
            }

            if (rayPickingParams.HasFlag(RayPickingParams.Volumes))
            {
                results = volumes
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

            return RayPickingHelper.PickAll(volumes.Select(v => v.SceneObject), ray, rayLength, rayPickingParams, out results);
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
        public PickingResult<T> First()
        {
            return PickingResults.FirstOrDefault();
        }
        /// <summary>
        /// Gets the las result
        /// </summary>
        /// <returns></returns>
        public PickingResult<T> Last()
        {
            return PickingResults.LastOrDefault();
        }
        /// <summary>
        /// Gets the nearest result to the picking origin
        /// </summary>
        public PickingResult<T> Nearest()
        {
            return PickingResults
                .OrderBy(p => p.Distance)
                .FirstOrDefault();
        }
        /// <summary>
        /// Gets the fartest result to the picking origin
        /// </summary>
        public PickingResult<T> Fartest()
        {
            return PickingResults
                .OrderByDescending(p => p.Distance)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the minimum distance valur to the picking origin
        /// </summary>
        public float GetMinimumDistance()
        {
            return PickingResults.Min(p => p.Distance);
        }
        /// <summary>
        /// Gets the maximum distance valur to the picking origin
        /// </summary>
        public float GetMaximumDistance()
        {
            return PickingResults.Max(p => p.Distance);
        }
    }
}
