using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Collections;
    using Engine.Common;

    /// <summary>
    /// The MeshTile contains the map data for pathfinding
    /// </summary>
    class MeshTile : IEquatable<MeshTile>
    {
        private const int MinMask = unchecked((int)0xfffffffe);

        private PolyIdManager idManager;
        private PolyId baseRef;

        /// <summary>
        /// 
        /// </summary>
        public Point Location { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int Layer { get; private set; }
        /// <summary>
        /// Gets or sets the counter describing modifications to the tile
        /// </summary>
        public int Salt { get; set; }
        /// <summary>
        /// Gets or sets the PolyMesh polygons
        /// </summary>
        public Poly[] Polygons { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PolygonCount { get; set; }
        /// <summary>
        /// Gets or sets the PolyMesh vertices
        /// </summary>
        public Vector3[] Vertices { get; set; }
        /// <summary>
        /// Gets or sets the PolyMeshDetail data
        /// </summary>
        public PolyMeshData[] DetailMeshes { get; set; }
        /// <summary>
        /// Gets or sets the PolyMeshDetail vertices
        /// </summary>
        public Vector3[] DetailVertices { get; set; }
        /// <summary>
        /// Gets or sets the PolyMeshDetail triangles
        /// </summary>
        public PolyMeshTriangleData[] DetailTriangles { get; set; }
        /// <summary>
        /// Gets or sets the OffmeshConnections
        /// </summary>
        public OffMeshConnection[] OffMeshConnections { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int OffMeshConnectionCount { get; set; }
        /// <summary>
        /// Gets or sets the bounding volume tree
        /// </summary>
        public BoundingVolumeTree BoundingVolumeTree { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float BoundingVolumeTreeQuantFactor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BoundingVolumeTreeNodeCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public BoundingBox Bounds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public float WalkableClimb { get; set; }
        public object polygons { get; private set; }

        /// <summary>
        /// Find the slab endpoints based off of the 'side' value.
        /// </summary>
        /// <param name="va">Vertex A</param>
        /// <param name="vb">Vertex B</param>
        /// <param name="bmin">Minimum bounds</param>
        /// <param name="bmax">Maximum bounds</param>
        /// <param name="side">The side</param>
        public static void CalcSlabEndPoints(Vector3 va, Vector3 vb, Vector2 bmin, Vector2 bmax, BoundarySide side)
        {
            if (side == BoundarySide.PlusX || side == BoundarySide.MinusX)
            {
                if (va.Z < vb.Z)
                {
                    bmin.X = va.Z;
                    bmin.Y = va.Y;

                    bmax.X = vb.Z;
                    bmax.Y = vb.Y;
                }
                else
                {
                    bmin.X = vb.Z;
                    bmin.Y = vb.Y;

                    bmax.X = va.Z;
                    bmax.Y = va.Y;
                }
            }
            else if (side == BoundarySide.PlusZ || side == BoundarySide.MinusZ)
            {
                if (va.X < vb.X)
                {
                    bmin.X = va.X;
                    bmin.Y = va.Y;

                    bmax.X = vb.X;
                    bmax.Y = vb.Y;
                }
                else
                {
                    bmin.X = vb.X;
                    bmin.Y = vb.Y;

                    bmax.X = va.X;
                    bmax.Y = va.Y;
                }
            }
        }
        /// <summary>
        /// Return the proper slab coordinate value depending on the 'side' value.
        /// </summary>
        /// <param name="va">Vertex A</param>
        /// <param name="side">The side</param>
        /// <returns>Slab coordinate value</returns>
        public static float GetSlabCoord(Vector3 va, BoundarySide side)
        {
            if (side == BoundarySide.PlusX || side == BoundarySide.MinusX)
            {
                return va.X;
            }
            else if (side == BoundarySide.PlusZ || side == BoundarySide.MinusZ)
            {
                return va.Z;
            }

            return 0;
        }
        /// <summary>
        /// Check if two slabs overlap.
        /// </summary>
        /// <param name="amin">Minimum bounds A</param>
        /// <param name="amax">Maximum bounds A</param>
        /// <param name="bmin">Minimum bounds B</param>
        /// <param name="bmax">Maximum bounds B</param>
        /// <param name="px">Point's x</param>
        /// <param name="py">Point's y</param>
        /// <returns>True if slabs overlap</returns>
        public static bool OverlapSlabs(Vector2 amin, Vector2 amax, Vector2 bmin, Vector2 bmax, float px, float py)
        {
            //Check for horizontal overlap
            //Segment shrunk a little so that slabs which touch at endpoints aren't connected
            float minX = Math.Max(amin.X + px, bmin.X + px);
            float maxX = Math.Min(amax.X - px, bmax.X - px);
            if (minX > maxX)
            {
                return false;
            }

            //Check vertical overlap
            float leftSlope = (amax.Y - amin.Y) / (amax.X - amin.X);
            float leftConstant = amin.Y - leftSlope * amin.X;
            float rightSlope = (bmax.Y - bmin.Y) / (bmax.X - bmin.X);
            float rightConstant = bmin.Y - rightSlope * bmin.X;
            float leftMinY = leftSlope * minX + leftConstant;
            float leftMaxY = leftSlope * maxX + leftConstant;
            float rightMinY = rightSlope * minX + rightConstant;
            float rightMaxY = rightSlope * maxX + rightConstant;
            float dmin = rightMinY - leftMinY;
            float dmax = rightMaxY - leftMaxY;

            //Crossing segments always overlap
            if (dmin * dmax < 0)
            {
                return true;
            }

            //Check for overlap at endpoints
            float threshold = (py * 2) * (py * 2);
            if (dmin * dmin <= threshold || dmax * dmax <= threshold)
            {
                return true;
            }

            return false;
        }

        public static bool operator ==(MeshTile left, MeshTile right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }

            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(MeshTile left, MeshTile right)
        {
            return !(left == right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="layer"></param>
        /// <param name="manager"></param>
        /// <param name="baseRef"></param>
        public MeshTile(Point location, int layer, PolyIdManager manager, PolyId baseRef)
        {
            this.Location = location;
            this.Layer = layer;
            this.idManager = manager;
            this.baseRef = baseRef;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="layer"></param>
        /// <param name="manager"></param>
        /// <param name="baseRef"></param>
        public MeshTile(int x, int y, int layer, PolyIdManager manager, PolyId baseRef)
            : this(new Point(x, y), layer, manager, baseRef) { }

        /// <summary>
        /// Allocate links for each of the tile's polygons' vertices
        /// </summary>
        public void ConnectIntLinks()
        {
            //Iterate through all the polygons
            for (int i = 0; i < PolygonCount; i++)
            {
                Poly p = Polygons[i];

                //Avoid Off-Mesh Connection polygons
                if (p.PolyType == PolyType.OffMeshConnection)
                {
                    continue;
                }

                //Build edge links
                for (int j = p.VertexCount - 1; j >= 0; j--)
                {
                    //Skip hard and non-internal edges
                    if (p.NeighborEdges[j] == 0 || Link.IsExternal(p.NeighborEdges[j]))
                    {
                        continue;
                    }

                    PolyId id;
                    this.idManager.SetPolyIndex(ref baseRef, p.NeighborEdges[j] - 1, out id);

                    //Initialize a new link
                    Link link = new Link();
                    link.Reference = id;
                    link.Edge = j;
                    link.Side = BoundarySide.Internal;
                    link.BMin = link.BMax = 0;
                    p.Links.Add(link);
                }
            }
        }
        /// <summary>
        /// Begin creating off-mesh links between the tile polygons.
        /// </summary>
        public void BaseOffMeshLinks()
        {
            //Base off-mesh connection start points
            for (int i = 0; i < this.OffMeshConnectionCount; i++)
            {
                int con = i;
                OffMeshConnection omc = this.OffMeshConnections[con];

                Vector3 extents = new Vector3(omc.Radius, this.WalkableClimb, omc.Radius);

                //Find polygon to connect to
                Vector3 p = omc.Pos0;
                Vector3 nearestPt;
                PolyId reference = FindNearestPoly(p, extents, out nearestPt);
                if (reference == PolyId.Null)
                {
                    continue;
                }

                //Do extra checks
                if ((nearestPt.X - p.X) * (nearestPt.X - p.X) + (nearestPt.Z - p.Z) * (nearestPt.Z - p.Z) >
                    this.OffMeshConnections[con].Radius * this.OffMeshConnections[con].Radius)
                {
                    continue;
                }

                Poly poly = this.Polygons[omc.Poly];

                //Make sure location is on current mesh
                this.Vertices[poly.Vertices[0]] = nearestPt;

                Link link1 = new Link();
                link1.Reference = reference;
                link1.Edge = 0;
                link1.Side = BoundarySide.Internal;
                poly.Links.Add(link1);

                //Start end-point always conects back to off-mesh connection
                int landPolyIdx = this.idManager.DecodePolyIndex(ref reference);
                PolyId id;
                this.idManager.SetPolyIndex(ref baseRef, OffMeshConnections[con].Poly, out id);

                Link link2 = new Link();
                link2.Reference = id;
                link2.Edge = 0xff;
                link2.Side = BoundarySide.Internal;
                Polygons[landPolyIdx].Links.Add(link2);
            }
        }
        /// <summary>
        /// Connect polygons from two different tiles.
        /// </summary>
        /// <param name="target">Target Tile</param>
        /// <param name="side">Polygon edge</param>
        public void ConnectExtLinks(MeshTile target, BoundarySide side)
        {
            //Connect border links
            for (int i = 0; i < this.PolygonCount; i++)
            {
                int numPolyVerts = this.Polygons[i].VertexCount;

                for (int j = 0; j < numPolyVerts; j++)
                {
                    //Skip non-portal edges
                    if ((this.Polygons[i].NeighborEdges[j] & Link.External) == 0)
                    {
                        continue;
                    }

                    BoundarySide dir = (BoundarySide)(this.Polygons[i].NeighborEdges[j] & 0xff);
                    if (side != BoundarySide.Internal && dir != side)
                    {
                        continue;
                    }

                    //Create new links
                    Vector3 va = this.Vertices[this.Polygons[i].Vertices[j]];
                    Vector3 vb = this.Vertices[this.Polygons[i].Vertices[(j + 1) % numPolyVerts]];
                    BoundarySide opSide = dir.GetOpposite();
                    PolyId[] neighbors;
                    float[] neighborAreas;
                    target.FindConnectingPolys(va, vb, opSide, out neighbors, out neighborAreas);

                    for (int k = 0; k < neighbors.Length; k++)
                    {
                        Link link = new Link()
                        {
                            Reference = neighbors[k],
                            Edge = j,
                            Side = dir,
                        };

                        this.Polygons[i].Links.Add(link);

                        //Compress portal limits to a value
                        if (dir == BoundarySide.PlusX || dir == BoundarySide.MinusX)
                        {
                            float tmin = (neighborAreas[k * 2 + 0] - va.Z) / (vb.Z - va.Z);
                            float tmax = (neighborAreas[k * 2 + 1] - va.Z) / (vb.Z - va.Z);

                            if (tmin > tmax)
                            {
                                Helper.Swap(ref tmin, ref tmax);
                            }

                            link.BMin = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                            link.BMax = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                        }
                        else if (dir == BoundarySide.PlusZ || dir == BoundarySide.MinusZ)
                        {
                            float tmin = (neighborAreas[k * 2 + 0] - va.X) / (vb.X - va.X);
                            float tmax = (neighborAreas[k * 2 + 1] - va.X) / (vb.X - va.X);

                            if (tmin > tmax)
                            {
                                Helper.Swap(ref tmin, ref tmax);
                            }

                            link.BMin = (int)(MathUtil.Clamp(tmin, 0.0f, 1.0f) * 255.0f);
                            link.BMax = (int)(MathUtil.Clamp(tmax, 0.0f, 1.0f) * 255.0f);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Connect Off-Mesh links between polygons from two different tiles.
        /// </summary>
        /// <param name="target">Target Tile</param>
        /// <param name="side">Polygon edge</param>
        public void ConnectExtOffMeshLinks(MeshTile target, BoundarySide side)
        {
            //Connect off-mesh links, specifically links which land from target tile to this tile
            BoundarySide oppositeSide = side.GetOpposite();

            //Iterate through all the off-mesh connections of target tile
            for (int i = 0; i < target.OffMeshConnectionCount; i++)
            {
                OffMeshConnection targetCon = target.OffMeshConnections[i];
                if (targetCon.Side != oppositeSide)
                {
                    continue;
                }

                Poly targetPoly = target.Polygons[targetCon.Poly];

                //Skip off-mesh connections which start location could not be connected at all
                if (targetPoly.Links.Count == 0)
                {
                    continue;
                }

                Vector3 extents = new Vector3(targetCon.Radius, target.WalkableClimb, targetCon.Radius);

                //Find polygon to connect to
                Vector3 p = targetCon.Pos1;
                Vector3 nearestPt;
                PolyId reference = this.FindNearestPoly(p, extents, out nearestPt);
                if (reference == PolyId.Null)
                {
                    continue;
                }

                //Further checks
                if ((nearestPt.X - p.X) * (nearestPt.X - p.X) + (nearestPt.Z - p.Z) * (nearestPt.Z - p.Z) >
                    (targetCon.Radius * targetCon.Radius))
                {
                    continue;
                }

                //Make sure the location is on the current mesh
                target.Vertices[targetPoly.Vertices[1]] = nearestPt;

                //Link off-mesh connection to target poly
                Link link = new Link();
                link.Reference = reference;
                link.Edge = i;
                link.Side = oppositeSide;
                target.Polygons[i].Links.Add(link);

                //link target poly to off-mesh connection
                if ((targetCon.Flags & OffMeshConnectionFlags.Bidirectional) != 0)
                {
                    int landPolyIdx = this.idManager.DecodePolyIndex(ref reference);
                    PolyId id = target.baseRef;
                    this.idManager.SetPolyIndex(ref id, targetCon.Poly, out id);

                    Link bidiLink = new Link();
                    bidiLink.Reference = id;
                    bidiLink.Edge = 0xff;
                    bidiLink.Side = side;
                    this.Polygons[landPolyIdx].Links.Add(bidiLink);
                }
            }
        }
        /// <summary>
        /// Search for neighbor polygons in the tile.
        /// </summary>
        /// <param name="va">Vertex A</param>
        /// <param name="vb">Vertex B</param>
        /// <param name="side">Polygon edge</param>
        /// <param name="connections">Resulting Connection polygon</param>
        /// <param name="connectionAreas">Resulting Connection area</param>
        public void FindConnectingPolys(Vector3 va, Vector3 vb, BoundarySide side, out PolyId[] connections, out float[] connectionAreas)
        {
            connections = null;
            connectionAreas = null;

            List<PolyId> con = new List<PolyId>();
            List<float> conarea = new List<float>();

            Vector2 amin = Vector2.Zero;
            Vector2 amax = Vector2.Zero;
            CalcSlabEndPoints(va, vb, amin, amax, side);
            float apos = GetSlabCoord(va, side);

            //Remove links pointing to 'side' and compact the links array
            Vector2 bmin = Vector2.Zero;
            Vector2 bmax = Vector2.Zero;

            //Iterate through all the tile's polygons
            for (int i = 0; i < this.PolygonCount; i++)
            {
                int numPolyVerts = this.Polygons[i].VertexCount;

                //Iterate through all the vertices
                for (int j = 0; j < numPolyVerts; j++)
                {
                    //Skip edges which do not point to the right side
                    if (this.Polygons[i].NeighborEdges[j] != (Link.External | (int)side))
                    {
                        continue;
                    }

                    //Grab two adjacent vertices
                    Vector3 vc = this.Vertices[this.Polygons[i].Vertices[j]];
                    Vector3 vd = this.Vertices[this.Polygons[i].Vertices[(j + 1) % numPolyVerts]];
                    float bpos = GetSlabCoord(vc, side);

                    //Segments are not close enough
                    if (Math.Abs(apos - bpos) > 0.01f)
                    {
                        continue;
                    }

                    //Check if the segments touch
                    CalcSlabEndPoints(vc, vd, bmin, bmax, side);

                    //Skip if slabs don't overlap
                    if (!OverlapSlabs(amin, amax, bmin, bmax, 0.01f, WalkableClimb))
                    {
                        continue;
                    }

                    //Add return value
                    conarea.Add(Math.Max(amin.X, bmin.X));
                    conarea.Add(Math.Min(amax.X, bmax.X));

                    PolyId id;
                    this.idManager.SetPolyIndex(ref baseRef, i, out id);
                    con.Add(id);

                    break;
                }
            }

            connections = con.ToArray();
            connectionAreas = conarea.ToArray();
        }
        /// <summary>
        /// Find the closest polygon possible in the tile under certain constraints.
        /// </summary>
        /// <param name="tile">Current tile</param>
        /// <param name="center">Center starting point</param>
        /// <param name="extents">Range of search</param>
        /// <param name="nearestPt">Resulting nearest point</param>
        /// <returns>Polygon Reference which contains nearest point</returns>
        public PolyId FindNearestPoly(Vector3 center, Vector3 extents, out Vector3 nearestPt)
        {
            nearestPt = Vector3.Zero;

            BoundingBox bounds;
            bounds.Minimum = center - extents;
            bounds.Maximum = center + extents;

            //Get nearby polygons from proximity grid
            PolyId[] polys;
            int polyCount = this.QueryPolygons(bounds, out polys);

            //Find nearest polygon amongst the nearby polygons
            PolyId nearest = PolyId.Null;
            float nearestDistanceSqr = float.MaxValue;

            //Iterate throuh all the polygons
            for (int i = 0; i < polyCount; i++)
            {
                PolyId reference = polys[i];
                Vector3 closestPtPoly;
                this.ClosestPointOnPoly(this.idManager.DecodePolyIndex(ref reference), center, out closestPtPoly);
                float d = (center - closestPtPoly).LengthSquared();
                if (d < nearestDistanceSqr)
                {
                    nearestPt = closestPtPoly;
                    nearestDistanceSqr = d;
                    nearest = reference;
                }
            }

            return nearest;
        }
        /// <summary>
        /// Find all the polygons within a certain bounding box.
        /// </summary>
        /// <param name="tile">Current tile</param>
        /// <param name="qbounds">The bounds</param>
        /// <param name="polygons">List of polygons</param>
        /// <returns>Number of polygons found</returns>
        public int QueryPolygons(BoundingBox qbounds, out PolyId[] polygons)
        {
            List<PolyId> polyList = new List<PolyId>();

            if (this.BoundingVolumeTree.Count != 0)
            {
                //Clamp query box to world box
                Vector3 tbmin = this.Bounds.Minimum;
                Vector3 tbmax = this.Bounds.Maximum;
                Vector3 qbmin = qbounds.Minimum;
                Vector3 qbmax = qbounds.Maximum;
                float bminx = MathUtil.Clamp(qbmin.X, tbmin.X, tbmax.X) - tbmin.X;
                float bminy = MathUtil.Clamp(qbmin.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
                float bminz = MathUtil.Clamp(qbmin.Z, tbmin.Z, tbmax.Z) - tbmin.Z;
                float bmaxx = MathUtil.Clamp(qbmax.X, tbmin.X, tbmax.X) - tbmin.X;
                float bmaxy = MathUtil.Clamp(qbmax.Y, tbmin.Y, tbmax.Y) - tbmin.Y;
                float bmaxz = MathUtil.Clamp(qbmax.Z, tbmin.Z, tbmax.Z) - tbmin.Z;

                BoundingBoxi b;
                b.Min.X = (int)(bminx * this.BoundingVolumeTreeQuantFactor) & MinMask;
                b.Min.Y = (int)(bminy * this.BoundingVolumeTreeQuantFactor) & MinMask;
                b.Min.Z = (int)(bminz * this.BoundingVolumeTreeQuantFactor) & MinMask;
                b.Max.X = (int)(bmaxx * this.BoundingVolumeTreeQuantFactor + 1) | 1;
                b.Max.Y = (int)(bmaxy * this.BoundingVolumeTreeQuantFactor + 1) | 1;
                b.Max.Z = (int)(bmaxz * this.BoundingVolumeTreeQuantFactor + 1) | 1;

                //traverse tree
                int node = 0;
                int end = this.BoundingVolumeTreeNodeCount;
                while (node < end)
                {
                    var bvNode = this.BoundingVolumeTree[node];
                    bool overlap = BoundingBoxi.Overlapping(ref b, ref bvNode.Bounds);
                    bool isLeafNode = bvNode.Index >= 0;

                    if (isLeafNode && overlap)
                    {
                        PolyId polyRef;
                        this.idManager.SetPolyIndex(ref baseRef, bvNode.Index, out polyRef);
                        polyList.Add(polyRef);
                    }

                    if (overlap || isLeafNode)
                    {
                        node++;
                    }
                    else
                    {
                        int escapeIndex = -bvNode.Index;
                        node += escapeIndex;
                    }
                }
            }
            else
            {
                BoundingBox b;

                for (int i = 0; i < this.PolygonCount; i++)
                {
                    var poly = this.Polygons[i];

                    //don't return off-mesh connection polygons
                    if (poly.PolyType == PolyType.OffMeshConnection)
                    {
                        continue;
                    }

                    //calculate polygon bounds
                    b.Maximum = b.Minimum = this.Vertices[poly.Vertices[0]];
                    for (int j = 1; j < poly.VertexCount; j++)
                    {
                        Vector3 v = this.Vertices[poly.Vertices[j]];
                        Vector3.Min(ref b.Minimum, ref v, out b.Minimum);
                        Vector3.Max(ref b.Maximum, ref v, out b.Maximum);
                    }

                    if (qbounds.Contains(ref b) != ContainmentType.Disjoint)
                    {
                        PolyId polyRef;
                        this.idManager.SetPolyIndex(ref baseRef, i, out polyRef);
                        polyList.Add(polyRef);
                    }
                }
            }

            polygons = polyList.ToArray();

            return polyList.Count;
        }
        /// <summary>
        /// Given a point, find the closest point on that poly.
        /// </summary>
        /// <param name="poly">The current polygon.</param>
        /// <param name="pos">The current position</param>
        /// <param name="closest">Reference to the closest point</param>
        public void ClosestPointOnPoly(Poly poly, Vector3 pos, out Vector3 closest)
        {
            closest = Vector3.Zero;

            int indexPoly = 0;
            for (int i = 0; i < this.Polygons.Length; i++)
            {
                if (this.Polygons[i] == poly)
                {
                    indexPoly = i;
                    break;
                }
            }

            this.ClosestPointOnPoly(indexPoly, pos, out closest);
        }
        /// <summary>
        /// Given a point, find the closest point on that poly.
        /// </summary>
        /// <param name="indexPoly">The current poly's index</param>
        /// <param name="pos">The current position</param>
        /// <param name="closest">Reference to the closest point</param>
        public void ClosestPointOnPoly(int indexPoly, Vector3 pos, out Vector3 closest)
        {
            closest = Vector3.Zero;

            Poly poly = this.Polygons[indexPoly];

            //Off-mesh connections don't have detail polygons
            if (this.Polygons[indexPoly].PolyType == PolyType.OffMeshConnection)
            {
                this.ClosestPointOnPolyOffMeshConnection(poly, pos, out closest);
                return;
            }

            this.ClosestPointOnPolyBoundary(poly, pos, out closest);

            float h;
            if (this.ClosestHeight(indexPoly, pos, out h))
            {
                closest.Y = h;
            }
        }
        /// <summary>
        /// Given a point, find the closest point on that poly.
        /// </summary>
        /// <param name="poly">The current polygon.</param>
        /// <param name="pos">The current position</param>
        /// <param name="closest">Reference to the closest point</param>
        public void ClosestPointOnPolyBoundary(Poly poly, Vector3 pos, out Vector3 closest)
        {
            //Clamp point to be inside the polygon
            Vector3[] verts = this.GetVertices(poly);
            float[] edgeDistance;
            float[] edgeT;
            bool inside = Intersection.PointToPolygonEdgeSquared(pos, verts, out edgeDistance, out edgeT);
            if (inside)
            {
                //Point is inside the polygon
                closest = pos;
            }
            else
            {
                //Point is outside the polygon
                //Clamp to nearest edge
                float minDistance = float.MaxValue;
                int minIndex = -1;
                for (int i = 0; i < verts.Length; i++)
                {
                    if (edgeDistance[i] < minDistance)
                    {
                        minDistance = edgeDistance[i];
                        minIndex = i;
                    }
                }

                Vector3 va = verts[minIndex];
                Vector3 vb = verts[(minIndex + 1) % verts.Length];
                closest = Vector3.Lerp(va, vb, edgeT[minIndex]);
            }
        }
        /// <summary>
        /// Find the distance from a point to a triangle.
        /// </summary>
        /// <param name="indexPoly">Current polygon's index</param>
        /// <param name="pos">Current position</param>
        /// <param name="h">Resulting height</param>
        /// <returns>True, if a height is found. False, if otherwise.</returns>
        public bool ClosestHeight(int indexPoly, Vector3 pos, out float h)
        {
            var poly = this.Polygons[indexPoly];
            var pd = this.DetailMeshes[indexPoly];

            //find height at the location
            for (int j = 0; j < this.DetailMeshes[indexPoly].TriangleCount; j++)
            {
                var t = DetailTriangles[pd.TriangleIndex + j];
                var v = new Vector3[3];

                for (int k = 0; k < 3; k++)
                {
                    if (t[k] < poly.VertexCount)
                    {
                        v[k] = this.Vertices[poly.Vertices[t[k]]];
                    }
                    else
                    {
                        v[k] = this.DetailVertices[pd.VertexIndex + (t[k] - poly.VertexCount)];
                    }
                }

                if (Intersection.PointToTriangle(pos, v[0], v[1], v[2], out h))
                {
                    return true;
                }
            }

            h = float.MaxValue;

            return false;
        }
        /// <summary>
        /// Find the closest point on an offmesh connection, which is in between the two points.
        /// </summary>
        /// <param name="poly">Current polygon</param>
        /// <param name="pos">Current position</param>
        /// <param name="closest">Resulting point that is closest.</param>
        public void ClosestPointOnPolyOffMeshConnection(Poly poly, Vector3 pos, out Vector3 closest)
        {
            Vector3 v0 = this.Vertices[poly.Vertices[0]];
            Vector3 v1 = this.Vertices[poly.Vertices[1]];
            float d0 = (pos - v0).Length();
            float d1 = (pos - v1).Length();
            float u = d0 / (d0 + d1);
            closest = Vector3.Lerp(v0, v1, u);
        }
        /// <summary>
        /// Gets the vertices of the specified polygon
        /// </summary>
        /// <param name="polygon">Polygon</param>
        /// <returns>Returns the vertex collection of the polygon</returns>
        public Vector3[] GetVertices(Poly polygon)
        {
            var verts = new List<Vector3>();

            for (int i = 0; i < polygon.VertexCount; i++)
            {
                verts.Add(this.Vertices[polygon.Vertices[i]]);
            }

            return verts.ToArray();
        }

        public bool Equals(MeshTile other)
        {
            return this.Location == other.Location && this.Layer == other.Layer;
        }

        public override bool Equals(object obj)
        {
            MeshTile other = obj as MeshTile;
            if (other != null)
            {
                return this.Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            //FNV hash
            unchecked
            {
                int h1 = (int)2166136261;
                int h2 = (int)16777619;
                h1 = (h1 * h2) ^ this.Location.X;
                h1 = (h1 * h2) ^ this.Location.Y;
                h1 = (h1 * h2) ^ this.Layer;
                return h1;
            }
        }

        public override string ToString()
        {
            return string.Format("Location: {0}; Layer: {1};", this.Location, this.Layer);
        }
    }
}
