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
            var cmpList = scene.Components.Get(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            //Coarse filter
            var coarseInt = TestCoarse(cmpList, ray);
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

            return PickFirst(coarseInt.Select(v => v.SceneObject), ray, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Scene object collection</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IEnumerable<ISceneObject> collection, PickingRay ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Find first coincidence
            foreach (var obj in collection)
            {
                var picked = PickFirstInternal<T>(obj, ray, out var res);
                if (picked)
                {
                    result.SceneObject = obj;
                    result.PickingResult = res;

                    //Result found
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirstInternal<T>(ISceneObject obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                return PickFirstInternal(composed, ray, out result);
            }

            if (obj is IRayPickable<T> pickable)
            {
                return pickable.PickFirst(ray, out result);
            }

            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            return false;
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirstInternal<T>(IComposed obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            var pickComponents = obj.GetComponents<IRayPickable<T>>();

            foreach (var pickable in pickComponents)
            {
                if (pickable.PickFirst(ray, out var r))
                {
                    result = r;

                    return true;
                }
            }

            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            return false;
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

            var cmpList = scene.Components.Get(usage);
            if (!cmpList.Any())
            {
                return false;
            }

            var coarseInt = TestCoarse(cmpList, ray);
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

            return PickAll(coarseInt.Select(v => v.SceneObject), ray, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Scene object collection</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IEnumerable<ISceneObject> collection, PickingRay ray, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            if (!collection.Any())
            {
                results = Enumerable.Empty<ScenePickingResultMultiple<T>>();

                return false;
            }

            results = collection
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(obj =>
                {
                    bool picked = PickAllInternal<T>(obj, ray, out var res);

                    return new
                    {
                        Picked = picked,
                        Results = new ScenePickingResultMultiple<T>
                        {
                            SceneObject = obj,
                            PickingResults = res,
                        }
                    };
                })
                .Where(r => r.Picked)
                .Select(r => r.Results)
                .ToArray();

            return results.Any();
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllInternal<T>(ISceneObject obj, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                return PickAllInternal<T>(composed, ray, out results);
            }

            if (obj is IRayPickable<T> pickable)
            {
                return pickable.PickAll(ray, out results);
            }

            results = Enumerable.Empty<PickingResult<T>>();

            return false;
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllInternal<T>(IComposed obj, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            results = obj
                .GetComponents<IRayPickable<T>>()
                .Select(pickable =>
                {
                    bool picked = pickable.PickAll(ray, out var r);

                    return new
                    {
                        Picked = picked,
                        Results = r.AsEnumerable()
                    };
                })
                .Where(r => r.Picked)
                .SelectMany(r => r.Results)
                .ToArray();

            return results.Any();
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
        public static bool PickNearest<T>(this Scene scene, PickingRay ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            if (!PickAll(scene, ray, usage, out IEnumerable<ScenePickingResultMultiple<T>> results))
            {
                result = new ScenePickingResult<T>();

                return false;
            }

            var first = results.First();

            result = new ScenePickingResult<T>
            {
                PickingResult = first.PickingResults.First(),
                SceneObject = first.SceneObject,
            };

            return true;
        }

        /// <summary>
        /// Performs coarse ray picking over the specified scene object collection
        /// </summary>
        /// <param name="collection">Collection to test</param>
        /// <param name="ray">Ray</param>
        /// <returns>Returns a list of ray pickable objects order by distance to ray origin</returns>
        public static IEnumerable<CoarsePickingResult> TestCoarse(IEnumerable<ISceneObject> collection, PickingRay ray)
        {
            if (!collection.Any())
            {
                return Enumerable.Empty<CoarsePickingResult>();
            }

            List<CoarsePickingResult> coarse = new();

            foreach (var obj in collection)
            {
                if (obj is IComposed componsed)
                {
                    if (TestCoarse(componsed, ray, out var d, out var p))
                    {
                        coarse.Add(new CoarsePickingResult(obj, d, p));
                    }
                }
                else if (obj is IRayPickable<Triangle> pickable)
                {
                    bool picked = TestCoarse(pickable, ray, out var d, out var p);
                    if (picked)
                    {
                        coarse.Add(new CoarsePickingResult(obj, d, p));
                    }
                }
            }

            //Sort by distance
            return coarse.OrderBy(c => c.Distance).ToArray();
        }
        /// <summary>
        /// Performs picking between the specified ray and the bounding volume of the object
        /// </summary>
        /// <param name="componsed">Componsed object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Returns the picking distance if any intersection exists</param>
        /// <param name="position">Returns the picking intersection position if exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        private static bool TestCoarse(IComposed componsed, PickingRay ray, out float distance, out Vector3 position)
        {
            var pickComponents = componsed.GetComponents<IRayPickable<Triangle>>();

            foreach (var pickable in pickComponents)
            {
                if (TestCoarse(pickable, ray, out var d, out var p))
                {
                    distance = d;
                    position = p;

                    return true;
                }
            }

            distance = float.MaxValue;
            position = Vector3.Zero;

            return false;
        }
        /// <summary>
        /// Performs picking between the specified ray and the bounding volume of the object
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Returns the picking distance if any intersection exists</param>
        /// <param name="position">Returns the picking intersection position if exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        private static bool TestCoarse<T>(IRayPickable<T> obj, PickingRay ray, out float distance, out Vector3 position) where T : IRayIntersectable
        {
            Ray rRay = ray;
            var bsph = obj.GetBoundingSphere();
            var intersects = Collision.RayIntersectsSphere(ref rRay, ref bsph, out float d);
            if (intersects && d <= ray.MaxDistance)
            {
                distance = d;
                position = ray.Position + (Vector3.Normalize(ray.Direction) * d);

                return true;
            }

            distance = float.MaxValue;
            position = Vector3.Zero;

            return false;
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
