using Engine.Collections.Generic;
using Engine.Common;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Collections.Helpers
{
    /// <summary>
    /// Picking quad-tree node helper
    /// </summary>
    /// <typeparam name="T">Node type</typeparam>
    static class PickingQuadTreeNodeHelper<T> where T : IVertexList, IRayIntersectable
    {
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        public static bool PickNearest(PickingQuadTreeNode<T> node, PickingRay ray, out PickingResult<T> result)
        {
            var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out _);
            if (!inBox)
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            if (node.IsLeaf)
            {
                return RayPickingHelper.PickNearestFromList(node.Items, ray, out result);
            }
            else
            {
                return PickNearestNode(node, ray, out result);
            }
        }
        /// <summary>
        /// Pick nearest position in the node collection
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        private static bool PickNearestNode(PickingQuadTreeNode<T> node, PickingRay ray, out PickingResult<T> result)
        {
            var boxHitsByDistance = FindContacts(node, ray);
            if (boxHitsByDistance.Count == 0)
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            bool intersect = false;

            PickingResult<T> bestHit = new()
            {
                Distance = float.MaxValue,
            };

            foreach (var hitNode in boxHitsByDistance.Values)
            {
                // check that the intersection is closer than the nearest intersection found thus far
                var inItem = PickNearest(hitNode, ray, out var thisHit);
                if (!inItem)
                {
                    continue;
                }

                if (thisHit.Distance < bestHit.Distance)
                {
                    // if we have found a closer intersection store the new closest intersection
                    bestHit = thisHit;

                    intersect = true;
                }
            }

            result = bestHit;

            return intersect;
        }
        /// <summary>
        /// Finds children contacts by distance to hit in bounding box
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <returns>Returns a sorted by distance node list</returns>
        private static SortedDictionary<float, PickingQuadTreeNode<T>> FindContacts(PickingQuadTreeNode<T> node, PickingRay ray)
        {
            SortedDictionary<float, PickingQuadTreeNode<T>> boxHitsByDistance = [];

            foreach (var child in node.Children)
            {
                if (Intersection.RayIntersectsBox(ray, child.BoundingBox, out float d))
                {
                    while (boxHitsByDistance.ContainsKey(d))
                    {
                        // avoid duplicate keys
                        d += 0.0001f;
                    }

                    boxHitsByDistance.Add(d, child);
                }
            }

            return boxHitsByDistance;
        }

        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        public static bool PickFirst(PickingQuadTreeNode<T> node, PickingRay ray, out PickingResult<T> result)
        {
            var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out _);
            if (!inBox)
            {
                result = new PickingResult<T>
                {
                    Distance = float.MaxValue,
                };

                return false;
            }

            if (node.IsLeaf)
            {
                return RayPickingHelper.PickFirstFromList(node.Items, ray, out result);
            }
            else
            {
                return PickFirstNode(node, ray, out result);
            }
        }
        /// <summary>
        /// Pick first position in the node collection
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <param name="result">Pick result</param>
        /// <returns>Returns true if picked position found</returns>
        private static bool PickFirstNode(PickingQuadTreeNode<T> node, PickingRay ray, out PickingResult<T> result)
        {
            foreach (var child in node.Children)
            {
                var inBox = Intersection.RayIntersectsBox(ray, child.BoundingBox, out _);
                if (!inBox)
                {
                    continue;
                }

                var inItem = PickFirst(child, ray, out var thisHit);
                if (!inItem)
                {
                    continue;
                }

                result = thisHit;

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue,
            };

            return false;
        }

        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Pick results</param>
        /// <returns>Returns true if picked position found</returns>
        public static bool PickAll(PickingQuadTreeNode<T> node, PickingRay ray, out IEnumerable<PickingResult<T>> results)
        {
            var inBox = Intersection.RayIntersectsBox(ray, node.BoundingBox, out _);
            if (!inBox)
            {
                results = [];

                return false;
            }

            if (node.IsLeaf)
            {
                return RayPickingHelper.PickAllFromlist(node.Items, ray, out results);
            }
            else
            {
                return PickAllNode(node, ray, out results);
            }
        }
        /// <summary>
        /// Pick all position in the node collection
        /// </summary>
        /// <param name="node">Node</param>
        /// <param name="ray">Ray</param>
        /// <param name="results">Pick results</param>
        /// <returns>Returns true if picked position found</returns>
        private static bool PickAllNode(PickingQuadTreeNode<T> node, PickingRay ray, out IEnumerable<PickingResult<T>> results)
        {
            bool intersect = false;

            List<PickingResult<T>> hits = [];

            foreach (var child in node.Children)
            {
                var inBox = Intersection.RayIntersectsBox(ray, child.BoundingBox, out float d);
                if (!inBox)
                {
                    continue;
                }

                var inItem = PickAll(child, ray, out var thisHits);
                if (!inItem)
                {
                    continue;
                }

                for (int i = 0; i < thisHits.Count(); i++)
                {
                    if (!hits.Contains(thisHits.ElementAt(i)))
                    {
                        hits.Add(thisHits.ElementAt(i));
                    }
                }

                intersect = true;
            }

            results = hits;

            return intersect;
        }
    }
}
