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
        /// <param name="rayLength">Ray length</param>
        /// <returns>Returns a list of ray pickable objects order by distance to ray origin</returns>
        public static IEnumerable<VolumePickingResult> PickVolumes(IEnumerable<ISceneObject> collection, Ray ray, float rayLength)
        {
            List<VolumePickingResult> coarse = new List<VolumePickingResult>();

            foreach (var obj in collection)
            {
                if (obj is IComposed componsed)
                {
                    var pickComponents = componsed.GetComponents<IRayPickable<Triangle>>();
                    foreach (var pickable in pickComponents)
                    {
                        bool picked = RayPickingHelper.TestVolumes(pickable, ray, rayLength, out var d, out var p);
                        if (picked)
                        {
                            coarse.Add(new VolumePickingResult(obj, d, p));
                        }
                    }
                }
                else if (obj is IRayPickable<Triangle> pickable)
                {
                    bool picked = RayPickingHelper.TestVolumes(pickable, ray, rayLength, out var d, out var p);
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
        /// <param name="rayLength">Ray length</param>
        /// <param name="distance">Returns the picking distance if any intersection exists</param>
        /// <param name="position">Returns the picking intersection position if exists</param>
        /// <returns>Returns true if exists intersection between the ray and the bounding volume of the object, into the maximum distance</returns>
        public static bool TestVolumes<T>(IRayPickable<T> obj, Ray ray, float rayLength, out float distance, out Vector3 position) where T : IRayIntersectable
        {
            distance = float.MaxValue;
            position = Vector3.Zero;

            var bsph = obj.GetBoundingSphere();
            var intersects = Collision.RayIntersectsSphere(ref ray, ref bsph, out float d);
            if (intersects)
            {
                float maxDistance = rayLength <= 0 ? float.MaxValue : rayLength;
                if (d <= maxDistance)
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
        /// <param name="collection">Picked volumes</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IEnumerable<ISceneObject> collection, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            if (collection?.Any() != true)
            {
                return false;
            }

            bool picked = false;
            float bestDistance = rayLength <= 0 ? float.MaxValue : rayLength;

            foreach (var obj in collection)
            {
                //Test for best picking results
                if (PickNearest<T>(obj, ray, bestDistance, rayPickingParams, out var r))
                {
                    //Update result and best distance
                    result = new ScenePickingResult<T>
                    {
                        SceneObject = obj,
                        PickingResult = r,
                    };
                    bestDistance = r.Distance;
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
        public static bool PickNearest<T>(IRayPickable<T> obj, Ray ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(obj, ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IRayPickable<T> obj, Ray ray, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            var bsph = obj.GetBoundingSphere();
            if (!bsph.Intersects(ref ray))
            {
                // Coarse exit
                return false;
            }

            var triangles = obj.GetVolume(rayPickingParams.HasFlag(RayPickingParams.Geometry));
            if (!triangles.Any())
            {
                // There are no triangles in the volume
                return false;
            }

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);
            if (!Intersection.IntersectNearest(ray, triangles, facingOnly, out var pos, out var tri, out var d))
            {
                // There are no intersected triangles in the volume triangles
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
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(ISceneObject obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                bool picked = PickNearest<T>(composed, ray, rayLength, rayPickingParams, out var res);

                result = res;

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                bool picked = PickNearest(pickable, ray, rayLength, rayPickingParams, out var res);

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
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IRayPickable<T> obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
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
        public static bool PickNearest<T>(IComposed obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
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
        /// <param name="collection">Picked volumes</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IEnumerable<ISceneObject> collection, Ray ray, float rayLength, RayPickingParams rayPickingParams, out ScenePickingResult<T> result) where T : IRayIntersectable
        {
            result = new ScenePickingResult<T>();

            //Find first coincidence
            foreach (var obj in collection)
            {
                var picked = PickFirst<T>(obj, ray, rayLength, rayPickingParams, out var res);
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
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IRayPickable<T> obj, Ray ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(obj, ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IRayPickable<T> obj, Ray ray, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            var bsph = obj.GetBoundingSphere();
            if (!bsph.Intersects(ref ray))
            {
                // Coarse exit
                return false;
            }

            var triangles = obj.GetVolume(rayPickingParams.HasFlag(RayPickingParams.Geometry));
            if (!triangles.Any())
            {
                // There are no triangles in the volume
                return false;
            }

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);
            if (!Intersection.IntersectFirst(ray, triangles, facingOnly, out var pos, out var tri, out var d))
            {
                // There are no intersected triangles in the volume triangles
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
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(ISceneObject obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                bool picked = PickFirst<T>(composed, ray, rayLength, rayPickingParams, out var res);

                result = res;

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                bool picked = PickFirst(pickable, ray, rayLength, rayPickingParams, out var res);

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
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Ray picking parameters</param>
        /// <param name="result">Resulting picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IComposed obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
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
        public static bool PickFirst<T>(IRayPickable<T> obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out PickingResult<T> result) where T : IRayIntersectable
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
        /// <param name="collection">Picked volumes</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IEnumerable<ISceneObject> collection, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<ScenePickingResultMultiple<T>> results) where T : IRayIntersectable
        {
            results = Enumerable.Empty<ScenePickingResultMultiple<T>>();

            results = collection
                .AsParallel()
                .WithDegreeOfParallelism(GameEnvironment.DegreeOfParalelism)
                .Select(obj =>
                {
                    bool picked = PickAll<T>(obj, ray, rayLength, rayPickingParams, out var res);

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
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IRayPickable<T> obj, Ray ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            return PickAll(obj, ray, RayPickingParams.Default, out results);
        }
        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IRayPickable<T> obj, Ray ray, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            results = Enumerable.Empty<PickingResult<T>>();

            var bsph = obj.GetBoundingSphere();
            if (!bsph.Intersects(ref ray))
            {
                // Coarse exit
                return false;
            }

            var triangles = obj.GetVolume(rayPickingParams.HasFlag(RayPickingParams.Geometry));
            if (!triangles.Any())
            {
                // There are no triangles in the volume
                return false;
            }

            bool facingOnly = rayPickingParams.HasFlag(RayPickingParams.FacingOnly);
            if (!Intersection.IntersectAll(ray, triangles, facingOnly, out var pos, out var tri, out var ds))
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
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Ray</param>
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(ISceneObject obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                var picked = PickAll<T>(composed, ray, rayLength, rayPickingParams, out var res);

                results = res;

                return picked;
            }

            if (obj is IRayPickable<T> pickable)
            {
                var picked = PickAll(pickable, ray, rayLength, rayPickingParams, out var res);

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
        /// <param name="rayLength">Ray length</param>
        /// <param name="rayPickingParams">Picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IComposed obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
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
        public static bool PickAll<T>(IRayPickable<T> obj, Ray ray, float rayLength, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
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
}
