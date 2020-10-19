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
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Ray pickable object</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public static bool PickNearest<T>(IRayPickable<T> obj, Ray ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            return PickNearest(obj, ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Gets nearest picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Ray pickable object</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
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
            result.Item = tri;
            result.Distance = d;

            return true;
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Ray pickable object</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public static bool PickFirst<T>(IRayPickable<T> obj, Ray ray, out PickingResult<T> result) where T : IRayIntersectable
        {
            return PickFirst(obj, ray, RayPickingParams.Default, out result);
        }
        /// <summary>
        /// Gets first picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Ray pickable object</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
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
            result.Item = tri;
            result.Distance = d;

            return true;
        }
        /// <summary>
        /// Get all picking positions of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Ray pickable object</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public static bool PickAll<T>(IRayPickable<T> obj, Ray ray, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            return PickAll(obj, ray, RayPickingParams.Default, out results);
        }
        /// <summary>
        /// Gets all picking position of giving ray
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Ray pickable object</param>
        /// <param name="ray">Picking ray</param>
        /// <param name="rayPickingParams">Ray picking params</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground position found</returns>
        public static bool PickAll<T>(IRayPickable<T> obj, Ray ray, RayPickingParams rayPickingParams, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            results = null;

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
            List<PickingResult<T>> picks = new List<PickingResult<T>>(pos.Length);

            for (int i = 0; i < pos.Length; i++)
            {
                picks.Add(new PickingResult<T>
                {
                    Position = pos[i],
                    Item = tri[i],
                    Distance = ds[i]
                });
            }

            results = picks;

            return true;
        }
    }
}
