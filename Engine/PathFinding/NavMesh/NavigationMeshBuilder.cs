using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Collections;

    /// <summary>
    /// The NavMeshBuilder class converst PolyMesh and PolyMeshDetail into a different data structure suited for pathfinding.
    /// This class will create tiled data.
    /// </summary>
    class NavigationMeshBuilder
    {
        /// <summary>
        /// Gets the file header
        /// </summary>
        public NavigationMeshInfo Header { get; private set; }
        /// <summary>
        /// Gets the PolyMesh vertices
        /// </summary>
        public Vector3[] NavVerts { get; private set; }
        /// <summary>
        /// Gets the PolyMesh polygons
        /// </summary>
        public Poly[] NavPolys { get; private set; }
        /// <summary>
        /// Gets the PolyMeshDetail mesh data (the indices of the vertices and triagles)
        /// </summary>
        public PolyMeshData[] NavDMeshes { get; private set; }
        /// <summary>
        /// Gets the PolyMeshDetail vertices
        /// </summary>
        public Vector3[] NavDVerts { get; private set; }
        /// <summary>
        /// Gets the PolyMeshDetail triangles
        /// </summary>
        public PolyMeshTriangleData[] NavDTris { get; private set; }
        /// <summary>
        /// Gets the bounding volume tree
        /// </summary>
        public BoundingVolumeTree NavBvTree { get; private set; }
        /// <summary>
        /// Gets the offmesh connection data
        /// </summary>
        public OffMeshConnection[] OffMeshCons { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationMeshBuilder" /> class.
        /// Add all the PolyMesh and PolyMeshDetail attributes to the Navigation Mesh.
        /// Then, add Off-Mesh connection support.
        /// </summary>
        /// <param name="polyMesh">The PolyMesh</param>
        /// <param name="polyMeshDetail">The PolyMeshDetail</param>
        /// <param name="offMeshCons">Offmesh connection data</param>
        /// <param name="cellSize"></param>
        /// <param name="cellHeight"></param>
        /// <param name="vertsPerPoly"></param>
        /// <param name="maxClimb"></param>
        /// <param name="buildBoundingVolumeTree"></param>
        /// <param name="agentHeight"></param>
        /// <param name="agentRadius"></param>
        public NavigationMeshBuilder(
            PolyMesh polyMesh,
            PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            float cellSize, float cellHeight, int vertsPerPoly, float maxClimb, bool buildBoundingVolumeTree, float agentHeight, float agentRadius)
        {
            if (polyMesh == null)
            {
                throw new InvalidOperationException("PolyMesh has to be provided.");
            }

            if (polyMesh.VertexCount == 0)
            {
                throw new InvalidOperationException("The provided PolyMesh has no vertices.");
            }

            if (polyMesh.PolyCount == 0)
            {
                throw new InvalidOperationException("The provided PolyMesh has not polys.");
            }

            if (vertsPerPoly > NavigationMeshQuery.VertsPerPolygon)
            {
                throw new InvalidOperationException(string.Format("The number of vertices per polygon is above {0} limit", NavigationMeshQuery.VertsPerPolygon));
            }

            //classify off-mesh connection points
            BoundarySide[] offMeshSides = null;
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                offMeshSides = new BoundarySide[offMeshCons.Length * 2];

                //find height bounds
                float hmin = float.MaxValue;
                float hmax = -float.MaxValue;

                if (polyMeshDetail != null)
                {
                    for (int i = 0; i < polyMeshDetail.VertCount; i++)
                    {
                        float h = polyMeshDetail.Verts[i].Y;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }
                else
                {
                    for (int i = 0; i < polyMesh.VertexCount; i++)
                    {
                        Vertex3i iv = polyMesh.Vertices[i];
                        float h = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }
                }

                hmin -= maxClimb;
                hmax += maxClimb;
                BoundingBox bounds = polyMesh.Bounds;
                bounds.Minimum.Y = hmin;
                bounds.Maximum.Y = hmax;

                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    Vector3 p0 = offMeshCons[i].Pos0;
                    Vector3 p1 = offMeshCons[i].Pos1;

                    offMeshSides[i * 2 + 0] = BoundarySideExtensions.FromPoint(p0, bounds);
                    offMeshSides[i * 2 + 1] = BoundarySideExtensions.FromPoint(p1, bounds);

                    //off-mesh start position isn't touching mesh
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        if (p0.Y < bounds.Minimum.Y || p0.Y > bounds.Maximum.Y)
                        {
                            offMeshSides[i * 2 + 0] = 0;
                        }
                    }

                    //count number of links to allocate
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        offMeshConLinkCount++;
                        storedOffMeshConCount++;
                    }

                    if (offMeshSides[i * 2 + 1] == BoundarySide.Internal)
                    {
                        offMeshConLinkCount++;
                    }
                }
            }

            //off-mesh connections stored as polygons, adjust values
            int totPolyCount = polyMesh.PolyCount + storedOffMeshConCount;
            int totVertCount = polyMesh.VertexCount + storedOffMeshConCount * 2;

            //find portal edges
            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                var p = polyMesh.Polys[i];

                for (int j = 0; j < vertsPerPoly; j++)
                {
                    if (p.Vertices[j] == PolyMesh.NullId)
                    {
                        break;
                    }

                    edgeCount++;

                    if (PolyMesh.IsBoundaryEdge(p.NeighborEdges[j]))
                    {
                        int dir = p.NeighborEdges[j] % 16;
                        if (dir != 15)
                        {
                            portalCount++;
                        }
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            //find unique detail vertices
            int uniqueDetailVertCount = 0;
            int detailTriCount = 0;
            if (polyMeshDetail != null)
            {
                detailTriCount = polyMeshDetail.TrisCount;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = polyMesh.Polys[i].VertexCount;
                    uniqueDetailVertCount += numDetailVerts - numPolyVerts;
                }
            }
            else
            {
                uniqueDetailVertCount = 0;
                detailTriCount = 0;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numPolyVerts = polyMesh.Polys[i].VertexCount;
                    uniqueDetailVertCount += numPolyVerts - 2;
                }
            }

            //store header
            this.Header = new NavigationMeshInfo()
            {
                X = 0,
                Y = 0,
                Layer = 0,
                PolyCount = totPolyCount,
                VertCount = totVertCount,
                MaxLinkCount = maxLinkCount,
                Bounds = polyMesh.Bounds,
                DetailMeshCount = polyMesh.PolyCount,
                DetailVertCount = uniqueDetailVertCount,
                DetailTriCount = detailTriCount,
                OffMeshBase = polyMesh.PolyCount,
                WalkableHeight = agentHeight,
                WalkableRadius = agentRadius,
                WalkableClimb = maxClimb,
                OffMeshConCount = storedOffMeshConCount,
                BvNodeCount = buildBoundingVolumeTree ? polyMesh.PolyCount * 2 : 0,
                BvQuantFactor = 1f / cellSize,
            };

            //allocate data
            this.NavVerts = new Vector3[totVertCount];
            this.NavPolys = new Poly[totPolyCount];
            this.NavDMeshes = new PolyMeshData[polyMesh.PolyCount];
            this.NavDVerts = new Vector3[uniqueDetailVertCount];
            this.NavDTris = new PolyMeshTriangleData[detailTriCount];
            this.OffMeshCons = new OffMeshConnection[storedOffMeshConCount];

            int offMeshVertsBase = polyMesh.VertexCount;
            int offMeshPolyBase = polyMesh.PolyCount;

            //store vertices
            for (int i = 0; i < polyMesh.VertexCount; i++)
            {
                var iv = polyMesh.Vertices[i];
                this.NavVerts[i].X = polyMesh.Bounds.Minimum.X + iv.X * cellSize;
                this.NavVerts[i].Y = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                this.NavVerts[i].Z = polyMesh.Bounds.Minimum.Z + iv.Z * cellSize;
            }

            //off-mesh link vertices
            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        this.NavVerts[offMeshVertsBase + (n * 2 + 0)] = offMeshCons[i].Pos0;
                        this.NavVerts[offMeshVertsBase + (n * 2 + 1)] = offMeshCons[i].Pos1;
                        n++;
                    }
                }
            }

            //store polygons
            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                this.NavPolys[i] = new Poly()
                {
                    VertexCount = 0,
                    Area = polyMesh.Polys[i].Area,
                    PolyType = PolyType.Ground,
                    Vertices = new int[vertsPerPoly],
                    NeighborEdges = new int[vertsPerPoly],
                };

                for (int j = 0; j < vertsPerPoly; j++)
                {
                    if (polyMesh.Polys[i].Vertices[j] == PolyMesh.NullId)
                    {
                        break;
                    }

                    this.NavPolys[i].Vertices[j] = polyMesh.Polys[i].Vertices[j];

                    if (PolyMesh.IsBoundaryEdge(polyMesh.Polys[i].NeighborEdges[j]))
                    {
                        //border or portal edge
                        int dir = polyMesh.Polys[i].NeighborEdges[j] % 16;
                        if (dir == 0xf) //border
                        {
                            this.NavPolys[i].NeighborEdges[j] = 0;
                        }
                        else if (dir == 0) //portal x-
                        {
                            this.NavPolys[i].NeighborEdges[j] = Link.External | 4;
                        }
                        else if (dir == 1) //portal z+
                        {
                            this.NavPolys[i].NeighborEdges[j] = Link.External | 2;
                        }
                        else if (dir == 2) //portal x+
                        {
                            this.NavPolys[i].NeighborEdges[j] = Link.External | 0;
                        }
                        else if (dir == 3) //portal z-
                        {
                            this.NavPolys[i].NeighborEdges[j] = Link.External | 6;
                        }
                    }
                    else
                    {
                        //normal connection
                        this.NavPolys[i].NeighborEdges[j] = polyMesh.Polys[i].NeighborEdges[j] + 1;
                    }

                    this.NavPolys[i].VertexCount++;
                }
            }

            //off-mesh connection vertices
            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        int[] verts = new int[vertsPerPoly];
                        verts[0] = offMeshVertsBase + (n * 2 + 0);
                        verts[1] = offMeshVertsBase + (n * 2 + 1);

                        this.NavPolys[offMeshPolyBase + n] = new Poly()
                        {
                            VertexCount = 2,
                            Vertices = verts,
                            Area = polyMesh.Polys[offMeshCons[i].Poly].Area,
                            PolyType = PolyType.OffMeshConnection,
                        };
                        n++;
                    }
                }
            }

            //store detail meshes and vertices
            if (polyMeshDetail != null)
            {
                int vbase = 0;
                var storedDetailVerts = new List<Vector3>();

                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int vb = polyMeshDetail.Meshes[i].VertexIndex;
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = this.NavPolys[i].VertexCount;
                    this.NavDMeshes[i].VertexIndex = vbase;
                    this.NavDMeshes[i].VertexCount = numDetailVerts - numPolyVerts;
                    this.NavDMeshes[i].TriangleIndex = polyMeshDetail.Meshes[i].TriangleIndex;
                    this.NavDMeshes[i].TriangleCount = polyMeshDetail.Meshes[i].TriangleCount;

                    //Copy detail vertices 
                    //first 'nv' verts are equal to nav poly verts
                    //the rest are detail verts
                    for (int j = 0; j < this.NavDMeshes[i].VertexCount; j++)
                    {
                        storedDetailVerts.Add(polyMeshDetail.Verts[vb + numPolyVerts + j]);
                    }

                    vbase += numDetailVerts - numPolyVerts;
                }

                this.NavDVerts = storedDetailVerts.ToArray();

                //store triangles
                for (int j = 0; j < polyMeshDetail.TrisCount; j++)
                {
                    this.NavDTris[j] = polyMeshDetail.Tris[j];
                }
            }
            else
            {
                //create dummy detail mesh by triangulating polys
                int tbase = 0;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numPolyVerts = this.NavPolys[i].VertexCount;
                    this.NavDMeshes[i].VertexIndex = 0;
                    this.NavDMeshes[i].VertexCount = 0;
                    this.NavDMeshes[i].TriangleIndex = tbase;
                    this.NavDMeshes[i].TriangleCount = numPolyVerts - 2;

                    //triangulate polygon
                    for (int j = 2; j < numPolyVerts; j++)
                    {
                        this.NavDTris[tbase].VertexHash0 = 0;
                        this.NavDTris[tbase].VertexHash1 = j - 1;
                        this.NavDTris[tbase].VertexHash2 = j;

                        //bit for each edge that belongs to the poly boundary
                        this.NavDTris[tbase].Flags = 1 << 2;
                        if (j == 2)
                        {
                            this.NavDTris[tbase].Flags |= 1 << 0;
                        }
                        if (j == numPolyVerts - 1)
                        {
                            this.NavDTris[tbase].Flags |= 1 << 4;
                        }

                        tbase++;
                    }
                }
            }

            //store and create BV tree
            if (buildBoundingVolumeTree)
            {
                //build tree
                this.NavBvTree = PolyMesh.BuildBVT(polyMesh.Vertices, polyMesh.Polys, vertsPerPoly, cellSize, cellHeight);
            }

            //store off-mesh connections
            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < this.OffMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        this.OffMeshCons[n] = new OffMeshConnection();

                        this.OffMeshCons[n].Poly = offMeshPolyBase + n;

                        //copy connection end points
                        this.OffMeshCons[n].Pos0 = offMeshCons[i].Pos0;
                        this.OffMeshCons[n].Pos1 = offMeshCons[i].Pos1;

                        this.OffMeshCons[n].Radius = offMeshCons[i].Radius;
                        this.OffMeshCons[n].Flags = offMeshCons[i].Flags;
                        this.OffMeshCons[n].Side = offMeshSides[i * 2 + 1];
                        this.OffMeshCons[n].Tag = offMeshCons[i].Tag;

                        n++;
                    }
                }
            }
        }
    }
}
