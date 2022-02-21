using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Scene picking extensions
    /// </summary>
    public static class ScenePickingExtensions
    {
        /// <summary>
        /// Performs coarse ray picking over the specified collection
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="list">Collection of objects to test</param>
        /// <returns>Returns a list of ray pickable objects order by distance to ray origin</returns>
        private static IEnumerable<(ISceneObject SceneObject, float Distance)> PickCoarse(Ray ray, float rayLength, IEnumerable<ISceneObject> list)
        {
            List<(ISceneObject SceneObject, float Distance)> coarse = new List<(ISceneObject SceneObject, float Distance)>();

            foreach (var gObj in list)
            {
                if (gObj is IComposed componsed)
                {
                    var pickComponents = componsed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        bool picked = TestCoarse(ray, rayLength, pickable, out float d);
                        if (picked)
                        {
                            coarse.Add((gObj, d));
                        }
                    }
                }
                else if (gObj is IRayPickable<Triangle> pickable)
                {
                    bool picked = TestCoarse(ray, rayLength, pickable, out float d);
                    if (picked)
                    {
                        coarse.Add((gObj, d));
                    }
                }
            }

            //Sort by distance
            coarse.Sort((i1, i2) =>
            {
                return i1.Distance.CompareTo(i2.Distance);
            });

            return coarse;
        }
        /// <summary>
        /// Perfors coarse picking between the specified ray and the bounding volume of the object
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="obj">Object</param>
        /// <param name="distance">Returns the picking distance if any intersection exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        private static bool TestCoarse<T>(Ray ray, float rayLength, IRayPickable<T> obj, out float distance) where T : IRayIntersectable
        {
            distance = float.MaxValue;

            var bsph = obj.GetBoundingSphere();
            var intersects = Collision.RayIntersectsSphere(ref ray, ref bsph, out float d);
            if (intersects)
            {
                float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;
                if (d <= maxDistance)
                {
                    distance = d;

                    return true;
                }
            }

            return false;
        }

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

            var coarse = PickCoarse(ray, rayLength, cmpList);
            if (!coarse.Any())
            {
                return false;
            }

            bool picked = false;
            float bestDistance = rayLength <= 0 ? float.MaxValue : rayLength;

            foreach (var (obj, objDistance) in coarse)
            {
                if (objDistance > bestDistance)
                {
                    continue;
                }

                //Test for best picking results
                if (PickNearestInternal<T>(obj, ray, bestDistance, rayPickingParams, out var r))
                {
                    //Update result and best distance
                    result = r;
                    bestDistance = r.PickingResult.Distance;
                    picked = true;
                }
            }

            return picked;
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickNearestInternal<T>(ISceneObject obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                bool picked = PickNearestComposed<T>(composed, ray, rayLength, rayPickingParams, out var res);

                result = new ScenePickingResult<T>
                {
                    SceneObject = obj,
                    PickingResult = res,
                };

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                bool picked = PickNearestSingle<T>(pickable, ray, rayLength, rayPickingParams, out var res);

                result = new ScenePickingResult<T>
                {
                    SceneObject = obj,
                    PickingResult = res,
                };

                return picked;
            }

            result = new ScenePickingResult<T>()
            {
                SceneObject = obj,
                PickingResult = new PickingResult<T>()
                {
                    Distance = float.MaxValue,
                },
            };

            return false;
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickNearestSingle<T>(IRayPickable<T> obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            var picked = obj.PickNearest(ray, rayPickingParams, out var r);
            if (picked)
            {
                float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;
                if (r.Distance < maxDistance)
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
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickNearestComposed<T>(IComposed obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            bool pickedNearest = false;
            float dist = rayLength <= 0 ? float.MaxValue : rayLength;

            var pickComponents = obj.GetComponents<IRayPickable<T>>();

            foreach (var pickable in pickComponents)
            {
                var picked = pickable.PickNearest(ray, rayPickingParams, out var r);
                if (picked && r.Distance < dist)
                {
                    dist = r.Distance;

                    result = r;
                    pickedNearest = true;
                }
            }

            return pickedNearest;
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
            var coarse = PickCoarse(ray, rayLength, cmpList).Select(o => o.SceneObject);
            if (!coarse.Any())
            {
                return false;
            }

            //Find first coincidence
            foreach (var obj in coarse)
            {
                var picked = PickFirstInternal<T>(obj, ray, rayLength, rayPickingParams, out var res);
                if (picked)
                {
                    result = res;

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
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirstInternal<T>(ISceneObject obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                bool picked = PickFirstComposed<T>(composed, ray, rayLength, rayPickingParams, out var res);

                result = new ScenePickingResult<T>
                {
                    SceneObject = obj,
                    PickingResult = res,
                };

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                bool picked = PickFirstSingle<T>(pickable, ray, rayLength, rayPickingParams, out var res);

                result = new ScenePickingResult<T>
                {
                    SceneObject = obj,
                    PickingResult = res,
                };

                return picked;
            }

            result = new ScenePickingResult<T>()
            {
                SceneObject = obj,
                PickingResult = new PickingResult<T>(),
            };

            return false;
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirstComposed<T>(IComposed obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;

            var pickComponents = obj.GetComponents<IRayPickable<T>>();
            foreach (var pickable in pickComponents)
            {
                if (pickable.PickFirst(ray, rayPickingParams, out var r) && r.Distance <= maxDistance)
                {
                    result = r;

                    return true;
                }
            }

            result = new PickingResult<T>();

            return false;
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirstSingle<T>(IRayPickable<T> obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;

            bool picked = obj.PickFirst(ray, rayPickingParams, out var r);
            if (picked && r.Distance <= maxDistance)
            {
                result = r;

                return true;
            }

            result = new PickingResult<T>();

            return false;
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

            var coarse = PickCoarse(ray, rayLength, cmpList).Select(o => o.SceneObject);
            if (!coarse.Any())
            {
                return false;
            }

            results = coarse
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(obj =>
                {
                    bool picked = PickAllInternal<T>(obj, ray, rayLength, rayPickingParams, out var res);

                    return new { Picked = picked, Results = res };
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
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllInternal<T>(ISceneObject obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResultMultiple<T> results) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                var picked = PickAllComposed<T>(composed, ray, rayLength, rayPickingParams, out var res);

                results = new ScenePickingResultMultiple<T>
                {
                    SceneObject = obj,
                    PickingResults = res
                };

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                var picked = PickAllSingle(pickable, ray, rayLength, rayPickingParams, out var res);

                results = new ScenePickingResultMultiple<T>
                {
                    SceneObject = obj,
                    PickingResults = res
                };

                return picked;
            }

            results = new ScenePickingResultMultiple<T>()
            {
                SceneObject = obj,
                PickingResults = Enumerable.Empty<PickingResult<T>>(),
            };

            return false;
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllComposed<T>(IComposed obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;

            results = obj
                .GetComponents<IRayPickable<T>>()
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(pickable =>
                {
                    bool picked = pickable.PickAll(ray, rayPickingParams, out var r);
                    if (!picked)
                    {
                        return new { Picked = false, Results = Enumerable.Empty<PickingResult<T>>() };
                    }

                    var inboundResults = r.Where(i => i.Distance <= maxDistance).ToArray();

                    return new
                    {
                        Picked = inboundResults.Any(),
                        Results = inboundResults.AsEnumerable()
                    };
                })
                .Where(r => r.Picked)
                .SelectMany(r => r.Results)
                .ToArray();

            return results.Any();
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllSingle<T>(IRayPickable<T> obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            bool picked = obj.PickAll(ray, rayPickingParams, out var r);
            if (!picked)
            {
                results = Enumerable.Empty<PickingResult<T>>();

                return false;
            }

            float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;

            results = r.Where(i => i.Distance <= maxDistance).ToArray();

            return results.Any();
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
