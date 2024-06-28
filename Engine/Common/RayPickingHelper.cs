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
        /// Picking fast test part
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <returns>Returns true if the test results in contact</returns>
        private static bool Test<T>(IRayPickable<T> obj, PickingRay ray) where T : IRayIntersectable
        {
            if (ray.RayPickingParams == PickingHullTypes.None)
            {
                //Halt here
                return false;
            }

            bool testGeom = ray.RayPickingParams.HasFlag(PickingHullTypes.Geometry);

            bool coarseInt = TestCoarse(obj, ray);
            if (!coarseInt || !testGeom)
            {
                //Halt here
                return coarseInt;
            }

            return true;
        }
        /// <summary>
        /// Picking coarse test
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Ray</param>
        /// <returns>Returns true if the coarse test results in contact</returns>
        private static bool TestCoarse<T>(IRayPickable<T> obj, PickingRay ray) where T : IRayIntersectable
        {
            var bsph = obj.GetBoundingSphere();
            Ray rRay = ray;
            return bsph.Intersects(ref rRay, out float sDist) || sDist > ray.MaxDistance;
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
            bool testGeom = ray.RayPickingParams.HasFlag(PickingHullTypes.Geometry);

            bool intersects = Test(obj, ray);
            if (!intersects || !testGeom)
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return intersects;
            }

            var triangles = obj.GetGeometry(GeometryTypes.Picking);

            return PickFirstFromList(triangles, ray, out result);
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
            bool testGeom = ray.RayPickingParams.HasFlag(PickingHullTypes.Geometry);

            bool intersects = Test(obj, ray);
            if (!intersects || !testGeom)
            {
                results = [];

                return intersects;
            }

            var triangles = obj.GetGeometry(GeometryTypes.Picking);

            return PickAllFromlist(triangles, ray, out results);
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
            if (!PickAll(obj, ray, out var results))
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            //Returns the first result of the results list
            result = results.FirstOrDefault(new PickingResult<T>
            {
                Distance = float.MaxValue,
            });

            return true;
        }

        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirstFromList<T>(IEnumerable<T> collection, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
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
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAllFromlist<T>(IEnumerable<T> collection, PickingRay ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            if (!collection.Any())
            {
                results = [];

                return false;
            }

            //Create a sorted list to store the ray picks sorted by pick distance
            SortedDictionary<float, PickingResult<T>> pickList = [];

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

                PickingResult<T> pick = new()
                {
                    Position = pos,
                    Primitive = intersectable,
                    Distance = d,
                };

                pickList.Add(k, pick);
            }

            results = [.. pickList.Values];

            return results.Any();
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearestFromList<T>(IEnumerable<T> collection, PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (!PickAllFromlist(collection, ray, out var results))
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
    }
}
