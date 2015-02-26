using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    /// <summary>
    /// Quadtree node
    /// </summary>
    public class QuadTreeNode
    {
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox;
        /// <summary>
        /// Children list
        /// </summary>
        public QuadTreeNode[] Children;
        /// <summary>
        /// Triangle list
        /// </summary>
        public Triangle[] Triangles;

        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            position = Vector3.Zero;
            triangle = new Triangle();

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3 pos;
                        Triangle tri;
                        if (Triangle.IntersectNearest(ref ray, this.Triangles, out pos, out tri))
                        {
                            position = pos;
                            triangle = tri;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                SortedDictionary<float, QuadTreeNode> boxHitsByDistance = new SortedDictionary<float, QuadTreeNode>();

                #region Find children contacts by distance to hit in bounding box

                foreach (QuadTreeNode node in this.Children)
                {
                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        while (boxHitsByDistance.ContainsKey(d))
                        {
                            // avoid duplicate keys
                            d += 0.0001f;
                        }

                        boxHitsByDistance.Add(d, node);
                    }
                }

                #endregion

                if (boxHitsByDistance.Count > 0)
                {
                    bool intersect = false;

                    #region Find closest triangle node by node, from closest to farthest

                    Vector3 bestHit = Vector3.Zero;
                    Triangle bestTri = new Triangle();
                    float bestD = float.MaxValue;

                    foreach (QuadTreeNode node in boxHitsByDistance.Values)
                    {
                        Vector3 thisHit;
                        Triangle thisTri;
                        if (node.PickNearest(ref ray, out thisHit, out thisTri))
                        {
                            // check that the intersection is closer than the nearest intersection found thus far
                            float thisD = (ray.Position - thisHit).LengthSquared();
                            if (thisD < bestD)
                            {
                                // if we have found a closer intersection store the new closest intersection
                                bestHit = thisHit;
                                bestTri = thisTri;
                                bestD = thisD;
                                intersect = true;
                            }
                        }
                    }

                    if (intersect)
                    {
                        position = bestHit;
                        triangle = bestTri;
                    }

                    #endregion

                    return intersect;
                }
            }

            return false;
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            position = Vector3.Zero;
            triangle = new Triangle();

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3 pos;
                        Triangle tri;
                        if (Triangle.IntersectFirst(ref ray, this.Triangles, out pos, out tri))
                        {
                            position = pos;
                            triangle = tri;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                #region Find first hit

                foreach (QuadTreeNode node in this.Children)
                {
                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3 thisHit;
                        Triangle thisTri;
                        if (node.PickFirst(ref ray, out thisHit, out thisTri))
                        {
                            position = thisHit;
                            triangle = thisTri;

                            return true;
                        }
                    }
                }

                #endregion
            }

            return false;
        }
        /// <summary>
        /// Pick all position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Hit position</param>
        /// <param name="triangle">Hit triangle</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            positions = null;
            triangles = null;

            if (this.Children == null)
            {
                if (this.Triangles != null && this.Triangles.Length > 0)
                {
                    #region Per bound test

                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref this.BoundingBox, out d))
                    {
                        #region Per triangle test

                        Vector3[] pos;
                        Triangle[] tri;
                        if (Triangle.IntersectAll(ref ray, this.Triangles, out pos, out tri))
                        {
                            positions = pos;
                            triangles = tri;

                            return true;
                        }

                        #endregion
                    }

                    #endregion
                }
            }
            else
            {
                #region Find all intersects

                bool intersect = false;

                List<Vector3> hits = new List<Vector3>();
                List<Triangle> tris = new List<Triangle>();

                foreach (QuadTreeNode node in this.Children)
                {
                    float d;
                    if (Collision.RayIntersectsBox(ref ray, ref node.BoundingBox, out d))
                    {
                        Vector3[] thisHits;
                        Triangle[] thisTris;
                        if (node.PickAll(ref ray, out thisHits, out thisTris))
                        {
                            for (int i = 0; i < thisHits.Length; i++)
                            {
                                if (!hits.Contains(thisHits[i]))
                                {
                                    hits.Add(thisHits[i]);
                                    tris.Add(thisTris[i]);
                                }
                            }

                            intersect = true;
                        }
                    }
                }

                if (intersect)
                {
                    positions = hits.ToArray();
                    triangles = tris.ToArray();
                }

                return intersect;

                #endregion
            }

            return false;
        }
    }
}
