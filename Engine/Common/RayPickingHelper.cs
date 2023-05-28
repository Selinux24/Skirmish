using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
    /// <summary>
    /// Ray picking helper
    /// </summary>
    public static class RayPickingHelper
    {
        /// <summary>
        /// Picking coarse test
        /// </summary>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="triangles">Returns a triangle list to test</param>
        /// <returns>Returns true if the coarse test results in contact</returns>
        public static bool PickCoarse<T>(IRayPickable<T> obj, PickingRay ray, out IEnumerable<T> triangles) where T : IRayIntersectable
        {
            // Coarse first
            var bsph = obj.GetBoundingSphere();
            Ray rRay = ray;
            bool coarseInt = bsph.Intersects(ref rRay, out float sDist) || sDist > ray.MaxDistance;
            if (!coarseInt || ray.RayPickingParams.HasFlag(RayPickingParams.Coarse))
            {
                // Coarse exit
                triangles = Enumerable.Empty<T>();

                return coarseInt;
            }

            // Next geometry
            PickingHullTypes geometryType = ray.RayPickingParams.HasFlag(RayPickingParams.Hull) ? PickingHullTypes.Hull : PickingHullTypes.Object;
            triangles = obj.GetPickingHull(geometryType);

            return triangles.Any();
        }
        /// <summary>
        /// Performs coarse ray picking over the specified scene object collection
        /// </summary>
        /// <param name="collection">Collection to test</param>
        /// <param name="ray">Ray</param>
        /// <returns>Returns a list of ray pickable objects order by distance to ray origin</returns>
        public static IEnumerable<CoarsePickingResult> PickCoarse(IEnumerable<ISceneObject> collection, PickingRay ray)
        {
            if (!collection.Any())
            {
                return Enumerable.Empty<CoarsePickingResult>();
            }

            List<CoarsePickingResult> coarse = new List<CoarsePickingResult>();

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

        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IRayPickable<T> obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            bool coarseInt = PickCoarse(obj, ray, out var triangles);
            if (!coarseInt || ray.RayPickingParams.HasFlag(RayPickingParams.Coarse))
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return coarseInt;
            }

            return PickNearest(triangles, ray, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IEnumerable<T> collection, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (!PickAll(collection, ray, out var results))
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            //Returns the first result of the results list
            result = results.First();

            return true;
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Scene object collection</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IEnumerable<ISceneObject> collection, PickingRay ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            bool picked = false;
            var pRay = ray; //Copy ray struct

            foreach (var obj in collection)
            {
                //Test for best picking results
                if (PickNearestInternal<T>(obj, pRay, out var r))
                {
                    // Update result
                    result.SceneObject = obj;
                    result.PickingResult = r;

                    // Update best distance
                    pRay.RayLength = r.Distance;
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
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickNearestInternal<T>(ISceneObject obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                return PickNearestInternal(composed, ray, out result);
            }

            if (obj is IRayPickable<T> pickable)
            {
                return pickable.PickNearest(ray, out result);
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
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickNearestInternal<T>(IComposed obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            bool pickedNearest = false;
            float dist = ray.MaxDistance;

            var pickComponents = obj.GetComponents<IRayPickable<T>>();

            foreach (var pickable in pickComponents)
            {
                var picked = pickable.PickNearest(ray, out var r);
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
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IRayPickable<T> obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            bool coarseInt = PickCoarse(obj, ray, out var triangles);
            if (!coarseInt || ray.RayPickingParams.HasFlag(RayPickingParams.Coarse))
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return coarseInt;
            }

            return PickFirst(triangles, ray, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IEnumerable<T> collection, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            foreach (var intersectable in collection)
            {
                if (!intersectable.Intersects(ray, out var pos, out var d))
                {
                    continue;
                }

                result = new PickingResult<T>
                {
                    Position = pos,
                    Primitive = intersectable,
                    Distance = d,
                };

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue,
            };

            return false;
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
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IRayPickable<T> obj, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            bool coarseInt = PickCoarse(obj, ray, out var triangles);
            if (!coarseInt || ray.RayPickingParams.HasFlag(RayPickingParams.Coarse))
            {
                results = Enumerable.Empty<PickingResult<T>>();

                return coarseInt;
            }

            return PickAll(triangles, ray, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IEnumerable<T> collection, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            if (!collection.Any())
            {
                results = Enumerable.Empty<PickingResult<T>>();

                return false;
            }

            //Create a sorted list to store the ray picks sorted by pick distance
            SortedDictionary<float, PickingResult<T>> pickList = new SortedDictionary<float, PickingResult<T>>();

            foreach (var intersectable in collection)
            {
                //Tests the intersection
                var intersects = intersectable.Intersects(ray, out var pos, out var d);
                if (!intersects)
                {
                    //No intersection found
                    continue;
                }

                float k = d;
                while (pickList.ContainsKey(k))
                {
                    //Avoid duplicate distance keys
                    k += 0.0001f;
                }

                PickingResult<T> pick = new PickingResult<T>
                {
                    Position = pos,
                    Primitive = intersectable,
                    Distance = d,
                };

                pickList.Add(k, pick);
            }

            results = pickList.Values.ToArray();

            return results.Any();
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
    }
}
