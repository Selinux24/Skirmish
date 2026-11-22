using Engine.Common;
using Engine.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
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
        public static bool PickFirst<T>(this Scene scene, ref PickingRay ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Filter by usage
            var cmpList = scene.Components.Get(usage);
            if (cmpList.Length == 0)
            {
                return false;
            }

            //Coarse filter
            var coarseInt = TestCoarse(cmpList, ref ray);
            if (coarseInt.Length == 0)
            {
                return false;
            }

            if (ray.PickingParams.HasFlag(PickingHullTypes.Coarse))
            {
                result = coarseInt
                    .Select(c => new ScenePickingResult<T>
                    {
                        SceneObject = c.SceneObject,
                        PickingResult = new PickingResult<T>(c.Position, c.Distance, default),
                    })
                    .First();

                return true;
            }

            return PickFirst([.. coarseInt.Select(v => v.SceneObject)], ref ray, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Scene object collection</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirst<T>(ISceneObject[] collection, ref PickingRay ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Find first coincidence
            foreach (var obj in collection)
            {
                var picked = PickFirstInternal<T>(obj, ref ray, out var res);
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
        private static bool PickFirstInternal<T>(ISceneObject obj, ref PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                return PickFirstInternal(composed, ref ray, out result);
            }

            if (obj is IRayPickable<T> pickable)
            {
                return pickable.PickFirst(ref ray, out result);
            }

            result = PickingResult<T>.Empty;
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
        private static bool PickFirstInternal<T>(IComposed obj, ref PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            var pickComponents = obj.GetComponents<IRayPickable<T>>();

            foreach (var pickable in pickComponents)
            {
                if (pickable.PickFirst(ref ray, out var r))
                {
                    result = r;
                    return true;
                }
            }

            result = PickingResult<T>.Empty;
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
        public static bool PickAll<T>(this Scene scene, ref PickingRay ray, SceneObjectUsages usage, out ScenePickingResultMultiple<T>[] results) where T : IRayIntersectable
        {
            results = [];

            var cmpList = scene.Components.Get(usage);
            if (cmpList.Length == 0)
            {
                return false;
            }

            var coarseInt = TestCoarse(cmpList, ref ray);
            if (coarseInt.Length == 0)
            {
                return false;
            }

            if (ray.PickingParams.HasFlag(PickingHullTypes.Coarse))
            {
                results = [.. coarseInt
                    .Select(c =>
                    {
                        PickingResult<T> pr = new(c.Position, c.Distance, default);
                        return new ScenePickingResultMultiple<T>(c.SceneObject, [pr]);
                    })];

                return true;
            }

            return PickAll([.. coarseInt.Select(v => v.SceneObject)], ref ray, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Scene object collection</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAll<T>(ISceneObject[] collection, ref PickingRay ray, out ScenePickingResultMultiple<T>[] results) where T : IRayIntersectable
        {
            ArgumentNullException.ThrowIfNull(collection);

            if (collection.Length == 0)
            {
                results = [];

                return false;
            }

            var r = ray;

            results = collection
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(obj =>
                {
                    bool picked = PickAllInternal<T>(obj, ref r, out var res);

                    return new
                    {
                        Picked = picked,
                        Results = new ScenePickingResultMultiple<T>(obj, res)
                    };
                })
                .Where(r => r.Picked)
                .Select(r => r.Results)
                .ToArray();

            return results.Length != 0;
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllInternal<T>(ISceneObject obj, ref PickingRay ray, out PickingResult<T>[] results) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                return PickAllInternal(composed, ref ray, out results);
            }

            if (obj is IRayPickable<T> pickable)
            {
                return pickable.PickAll(ref ray, out results);
            }

            results = [];

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
        private static bool PickAllInternal<T>(IComposed obj, ref PickingRay ray, out PickingResult<T>[] results) where T : IRayIntersectable
        {
            var r = ray;

            results = [.. obj
                .GetComponents<IRayPickable<T>>()
                .Select(pickable =>
                {
                    bool picked = pickable.PickAll(ref r, out var res);

                    return new
                    {
                        Picked = picked,
                        Results = res.AsEnumerable()
                    };
                })
                .Where(res => res.Picked)
                .SelectMany(res => res.Results)];

            return results.Length != 0;
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
        public static bool PickNearest<T>(this Scene scene, ref PickingRay ray, SceneObjectUsages usage, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            if (!PickAll<T>(scene, ref ray, usage, out var results))
            {
                result = default;

                return false;
            }

            var first = results[0];

            result = new()
            {
                PickingResult = first.Nearest(),
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
        private static CoarsePickingResult[] TestCoarse(ISceneObject[] collection, ref PickingRay ray)
        {
            ArgumentNullException.ThrowIfNull(collection);

            if (collection.Length == 0)
            {
                return [];
            }

            List<CoarsePickingResult> coarse = [];

            foreach (var obj in collection)
            {
                if (obj is IComposed componsed)
                {
                    if (TestCoarse(componsed, ref ray, out var d, out var p))
                    {
                        coarse.Add(new(obj, d, p));
                    }
                }
                else if (obj is IRayPickable<Triangle> pickable)
                {
                    bool picked = TestCoarse(pickable, ref ray, out var d, out var p);
                    if (picked)
                    {
                        coarse.Add(new(obj, d, p));
                    }
                }
            }

            //Sort by distance
            return [.. coarse.OrderBy(c => c.Distance)];
        }
        /// <summary>
        /// Performs picking between the specified ray and the bounding volume of the object
        /// </summary>
        /// <param name="componsed">Componsed object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Returns the picking distance if any intersection exists</param>
        /// <param name="position">Returns the picking intersection position if exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        private static bool TestCoarse(IComposed componsed, ref PickingRay ray, out float distance, out Vector3 position)
        {
            var pickComponents = componsed.GetComponents<IRayPickable<Triangle>>();

            foreach (var pickable in pickComponents)
            {
                if (TestCoarse(pickable, ref ray, out var d, out var p))
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
        private static bool TestCoarse<T>(IRayPickable<T> obj, ref PickingRay ray, out float distance, out Vector3 position) where T : IRayIntersectable
        {
            var bsph = obj.GetBoundingSphere();
            return Collision.RayIntersectsSphere(ref ray, ref bsph, out distance, out position);
        }

        /// <summary>
        /// Gets the unordered first picking position of the given ray list
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="rayList">Picking ray list</param>
        /// <param name="usage">Component usage</param>
        /// <param name="callback">Result callback</param>
        public static Task PickFirstAsync<T>(this Scene scene, PickingRay[] rayList, SceneObjectUsages usage, Action<(bool, ScenePickingResult<T>)[]> callback) where T : IRayIntersectable
        {
            return Task.Run(() =>
            {
                var query = rayList
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .Select(ray =>
                    {
                        bool found = PickFirst<T>(scene, ref ray, usage, out var result);

                        return (found, result);
                    });

                callback(query.ToArray());
            });
        }
        /// <summary>
        /// Gets all picking positions of the given ray list
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="rayList">Picking ray list</param>
        /// <param name="usage">Component usage</param>
        /// <param name="callback">Result callback</param>
        public static Task PickAllAsync<T>(this Scene scene, PickingRay[] rayList, SceneObjectUsages usage, Action<(bool, ScenePickingResultMultiple<T>[])[]> callback) where T : IRayIntersectable
        {
            return Task.Run(() =>
            {
                var query = rayList
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .Select(ray =>
                    {
                        bool found = PickAll<T>(scene, ref ray, usage, out var result);

                        return (found, result);
                    });

                callback(query.ToArray());
            });
        }
        /// <summary>
        /// Gets nearest picking position of the given ray list
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="rayList">Picking ray list</param>
        /// <param name="usage">Component usage</param>
        /// <param name="callback">Result callback</param>
        public static Task PickNearestAsync<T>(this Scene scene, PickingRay[] rayList, SceneObjectUsages usage, Action<(bool, ScenePickingResult<T>)[]> callback) where T : IRayIntersectable
        {
            return Task.Run(() =>
            {
                var query = rayList
                    .AsParallel()
                    .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                    .Select(ray =>
                    {
                        bool found = PickNearest<T>(scene, ref ray, usage, out var result);

                        return (found, result);
                    });

                callback(query.ToArray());
            });
        }
    }
}
