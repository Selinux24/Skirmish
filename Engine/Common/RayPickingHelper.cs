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
        /// Performs volume ray picking over the specified collection
        /// </summary>
        /// <param name="collection">Collection of objects to test</param>
        /// <param name="ray">Ray</param>
        /// <returns>Returns a list of ray pickable objects order by distance to ray origin</returns>
        public static IEnumerable<VolumePickingResult> PickVolumes(IEnumerable<ISceneObject> collection, PickingRay ray)
        {
            List<VolumePickingResult> coarse = new List<VolumePickingResult>();

            foreach (var obj in collection)
            {
                if (obj is IComposed componsed)
                {
                    var pickComponents = componsed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        bool picked = TestVolumes(pickable, ray, out var d, out var p);
                        if (picked)
                        {
                            coarse.Add(new VolumePickingResult(obj, d, p));
                        }
                    }
                }
                else if (obj is IRayPickable<Triangle> pickable)
                {
                    bool picked = TestVolumes(pickable, ray, out var d, out var p);
                    if (picked)
                    {
                        coarse.Add(new VolumePickingResult(obj, d, p));
                    }
                }
            }

            //Sort by distance
            return coarse.OrderBy(c => c.Distance).ToArray();
        }
        /// <summary>
        /// Perfors picking between the specified ray and the bounding volume of the object
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="distance">Returns the picking distance if any intersection exists</param>
        /// <param name="position">Returns the picking intersection position if exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        public static bool TestVolumes<T>(IRayPickable<T> obj, PickingRay ray, out float distance, out Vector3 position) where T : IRayIntersectable
        {
            distance = float.MaxValue;
            position = Vector3.Zero;

            var bsph = obj.GetBoundingSphere();
            var rRay = ray.GetRay();
            var intersects = Collision.RayIntersectsSphere(ref rRay, ref bsph, out float d);
            if (intersects)
            {
                if (d <= ray.MaxDistance)
                {
                    distance = d;
                    position = ray.Position + (Vector3.Normalize(ray.Direction) * d);

                    return true;
                }
            }

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
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            // Coarse first
            var bsph = obj.GetBoundingSphere();
            var rRay = ray.GetRay();
            if (!bsph.Intersects(ref rRay, out float sDist))
            {
                // Coarse exit
                return false;
            }
            if (sDist > ray.MaxDistance)
            {
                // Coarse intersection too far
                return false;
            }

            // Next geometry
            var triangles = obj.GetVolume(ray.RayPickingParams.HasFlag(RayPickingParams.Geometry));
            if (!triangles.Any())
            {
                // There are no geometry to test
                return false;
            }

            if (!Intersection.IntersectNearest(ray, triangles, out var pos, out var tri, out var d))
            {
                // There are no intersection
                return false;
            }

            // Store result
            result.Position = pos;
            result.Primitive = tri;
            result.Distance = d;

            return true;
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Picked volumes</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IEnumerable<ISceneObject> collection, PickingRay ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            if (collection?.Any() != true)
            {
                return false;
            }

            bool picked = false;
            var pRay = ray; //Copy ray struct

            foreach (var obj in collection)
            {
                //Test for best picking results
                if (PickNearest<T>(obj, pRay, out var r))
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
        private static bool PickNearest<T>(ISceneObject obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                bool picked = PickNearestInternal<T>(composed, ray, out var res);

                result = res;

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                bool picked = PickNearestInternal(pickable, ray, out var res);

                result = res;

                return picked;
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
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickNearestInternal<T>(IRayPickable<T> obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            var picked = obj.PickNearest(ray, out var r);
            if (picked)
            {
                result = r;
                return true;
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
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IRayPickable<T> obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            // Coarse first
            var bsph = obj.GetBoundingSphere();
            var rRay = ray.GetRay();
            if (!bsph.Intersects(ref rRay, out float sDist))
            {
                // Coarse exit
                return false;
            }
            if (sDist > ray.MaxDistance)
            {
                // Coarse intersection too far
                return false;
            }

            // Next geometry
            var triangles = obj.GetVolume(ray.RayPickingParams.HasFlag(RayPickingParams.Geometry));
            if (!triangles.Any())
            {
                // There are no geometry to test
                return false;
            }

            if (!Intersection.IntersectFirst(ray, triangles, out var pos, out var tri, out var d))
            {
                // There are no intersection
                return false;
            }

            // Store result
            result.Position = pos;
            result.Primitive = tri;
            result.Distance = d;

            return true;
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Picked volumes</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IEnumerable<ISceneObject> collection, PickingRay ray, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Find first coincidence
            foreach (var obj in collection)
            {
                var picked = PickFirst<T>(obj, ray, out var res);
                if (picked)
                {
                    result = new ScenePickingResult<T>
                    {
                        SceneObject = obj,
                        PickingResult = res,
                    };

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
        private static bool PickFirst<T>(ISceneObject obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                bool picked = PickFirstInternal<T>(composed, ray, out var res);

                result = res;

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                bool picked = PickFirstInternal(pickable, ray, out var res);

                result = res;

                return picked;
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
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickFirstInternal<T>(IRayPickable<T> obj, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj.PickFirst(ray, out var r))
            {
                result = r;

                return true;
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
            results = Enumerable.Empty<PickingResult<T>>();

            // Coarse first
            var bsph = obj.GetBoundingSphere();
            var rRay = ray.GetRay();
            if (!bsph.Intersects(ref rRay, out float sDist))
            {
                // Coarse exit
                return false;
            }
            if (sDist > ray.MaxDistance)
            {
                // Coarse intersection too far
                return false;
            }

            // Next geometry
            var triangles = obj.GetVolume(ray.RayPickingParams.HasFlag(RayPickingParams.Geometry));
            if (!triangles.Any())
            {
                // There are no geometry to test
                return false;
            }

            if (!Intersection.IntersectAll(ray, triangles, out var pos, out var tri, out var ds))
            {
                // There are no intersected triangles in the volume triangles
                return false;
            }

            // Add picks to the resulting collection
            List<PickingResult<T>> picks = new List<PickingResult<T>>(pos.Count());

            for (int i = 0; i < pos.Count(); i++)
            {
                picks.Add(new PickingResult<T>
                {
                    Position = pos.ElementAt(i),
                    Primitive = tri.ElementAt(i),
                    Distance = ds.ElementAt(i)
                });
            }

            results = picks;

            return true;
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="collection">Picked volumes</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IEnumerable<ISceneObject> collection, PickingRay ray, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            results = Enumerable.Empty<ScenePickingResultMultiple<T>>();

            results = collection
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(obj =>
                {
                    bool picked = PickAll<T>(obj, ray, out var res);

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
        private static bool PickAll<T>(ISceneObject obj, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                var picked = PickAllInternal<T>(composed, ray, out var res);

                results = res;

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                var picked = PickAllInternal(pickable, ray, out var res);

                results = res;

                return picked;
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
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(pickable =>
                {
                    bool picked = pickable.PickAll(ray, out var r);
                    if (!picked)
                    {
                        return new { Picked = false, Results = Enumerable.Empty<PickingResult<T>>() };
                    }

                    return new
                    {
                        Picked = r.Any(),
                        Results = r.AsEnumerable()
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
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        private static bool PickAllInternal<T>(IRayPickable<T> obj, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            bool picked = obj.PickAll(ray, out var r);
            if (!picked)
            {
                results = Enumerable.Empty<PickingResult<T>>();

                return false;
            }

            results = r;

            return r.Any();
        }
    }
}
