using Engine.Mathematics;
using System.Collections.Generic;

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
        private static bool Test<T>(IRayPickable<T> obj, ref PickingRay ray) where T : IRayIntersectable
        {
            if (ray.PickingParams == PickingHullTypes.None)
            {
                //Halt here
                return false;
            }

            bool testGeom = ray.PickingParams.HasFlag(PickingHullTypes.Geometry);

            bool coarseInt = TestCoarse(obj, ref ray);
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
        private static bool TestCoarse<T>(IRayPickable<T> obj, ref PickingRay ray) where T : IRayIntersectable
        {
            var bsph = obj.GetBoundingSphere();
            return Collision.RayIntersectsSphere(ref ray, ref bsph, out _);
        }

        /// <summary>
        /// Gets the unordered first picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickFirst<T>(IRayPickable<T> obj, ref PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            bool testGeom = ray.PickingParams.HasFlag(PickingHullTypes.Geometry);

            bool intersects = Test(obj, ref ray);
            if (!intersects || !testGeom)
            {
                result = PickingResult<T>.Empty;
                return intersects;
            }

            var triangles = obj.GetGeometry(PickingMode.Picking);

            return PickFirstFromList(triangles, ref ray, out result);
        }

        /// <summary>
        /// Gets all picking positions of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickAll<T>(IRayPickable<T> obj, ref PickingRay ray, out PickingResult<T>[] results) where T : IRayIntersectable
        {
            bool testGeom = ray.PickingParams.HasFlag(PickingHullTypes.Geometry);

            bool intersects = Test(obj, ref ray);
            if (!intersects || !testGeom)
            {
                results = [];

                return intersects;
            }

            var triangles = obj.GetGeometry(PickingMode.Picking);

            return PickAllFromlist(triangles, ref ray, out results);
        }

        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Object to test</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearest<T>(IRayPickable<T> obj, ref PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (!PickAll(obj, ref ray, out var results))
            {
                result = PickingResult<T>.Empty;
                return false;
            }

            if (results.Length == 0)
            {
                result = PickingResult<T>.Empty;
                return true;
            }

            //Returns the first result of the results list
            result = results[0];
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
        public static bool PickFirstFromList<T>(T[] collection, ref PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            foreach (var intersectable in collection)
            {
                if (!intersectable.Intersects(ref ray, out var pos, out var d))
                {
                    continue;
                }

                result = new PickingResult<T>(pos, d, intersectable);
                return true;
            }

            result = PickingResult<T>.Empty;
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
        public static bool PickAllFromlist<T>(T[] collection, ref PickingRay ray, out PickingResult<T>[] results) where T : IRayIntersectable
        {
            if (collection.Length == 0)
            {
                results = [];

                return false;
            }

            //Create a sorted list to store the ray picks sorted by pick distance
            SortedDictionary<float, PickingResult<T>> pickList = [];

            foreach (var intersectable in collection)
            {
                //Tests the intersection
                var intersects = intersectable.Intersects(ref ray, out var pos, out var d);
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

                pickList.Add(k, new(pos, d, intersectable));
            }

            results = [.. pickList.Values];
            return results.Length != 0;
        }
        /// <summary>
        /// Gets nearest picking position of the given ray
        /// </summary>
        /// <typeparam name="T">Intersectable type</typeparam>
        /// <param name="collection">Intersectable list</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if intersection position found</returns>
        public static bool PickNearestFromList<T>(T[] collection, ref PickingRay ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            if (!PickAllFromlist(collection, ref ray, out var results))
            {
                result = PickingResult<T>.Empty;
                return false;
            }

            //Returns the first result of the results list
            result = results[0];
            return true;
        }
    }
}
