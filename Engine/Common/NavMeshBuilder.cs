using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    using Engine.Geometry;
    using Engine.PathFinding;

    /// <summary>
    /// The NavMeshBuilder class converst PolyMesh and PolyMeshDetail into a different data structure suited for pathfinding.
    /// This class will create tiled data.
    /// </summary>
    public class NavMeshBuilder
    {
        private PolyMesh polyMesh;
        private PolyMeshDetail polyMeshDetail;
        private float cellSize;
        private float cellHeight;
        private int vertsPerPoly;
        private float maxClimb;

        /// <summary>
        /// Gets the file header
        /// </summary>
        public PathfindingCommon.NavMeshInfo Header { get; private set; }
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
        public PolyMeshDetail.MeshData[] NavDMeshes { get; private set; }
        /// <summary>
        /// Gets the PolyMeshDetail vertices
        /// </summary>
        public Vector3[] NavDVerts { get; private set; }
        /// <summary>
        /// Gets the PolyMeshDetail triangles
        /// </summary>
        public PolyMeshDetail.TriangleData[] NavDTris { get; private set; }
        /// <summary>
        /// Gets the bounding volume tree
        /// </summary>
        public BVTree NavBvTree { get; private set; }
        /// <summary>
        /// Gets the offmesh connection data
        /// </summary>
        public OffMeshConnection[] OffMeshCons { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavMeshBuilder" /> class.
        /// Add all the PolyMesh and PolyMeshDetail attributes to the Navigation Mesh.
        /// Then, add Off-Mesh connection support.
        /// </summary>
        /// <param name="polyMesh">The PolyMesh</param>
        /// <param name="polyMeshDetail">The PolyMeshDetail</param>
        /// <param name="offMeshCons">Offmesh connection data</param>
        /// <param name="settings">The settings used to build.</param>
        public NavMeshBuilder(
            PolyMesh polyMesh,
            PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            NavMeshGenerationSettings settings)
            : this(polyMesh, polyMeshDetail, offMeshCons, settings.CellSize, settings.CellHeight, settings.VertsPerPoly, settings.MaxClimb, settings.BuildBoundingVolumeTree, settings.AgentHeight, settings.AgentRadius) { }

        public NavMeshBuilder(
            PolyMesh polyMesh,
            PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            float cellSize, float cellHeight, int vertsPerPoly, float maxClimb, bool buildBoundingVolumeTree, float agentHeight, float agentRadius)
        {
            if (vertsPerPoly > PathfindingCommon.VERTS_PER_POLYGON)
            {
                throw new InvalidOperationException("The number of vertices per polygon is above SharpNav's limit");
            }
            if (polyMesh.VertCount == 0)
            {
                throw new InvalidOperationException("The provided PolyMesh has no vertices.");
            }
            if (polyMesh.PolyCount == 0)
            {
                throw new InvalidOperationException("The provided PolyMesh has not polys.");
            }

            int nvp = vertsPerPoly;

            //classify off-mesh connection points
            BoundarySide[] offMeshSides = new BoundarySide[offMeshCons.Length * 2];
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (offMeshCons.Length > 0)
            {
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
                    for (int i = 0; i < polyMesh.VertCount; i++)
                    {
                        PolyVertex iv = polyMesh.Verts[i];
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
                    }
                    if (offMeshSides[i * 2 + 1] == BoundarySide.Internal)
                    {
                        offMeshConLinkCount++;
                    }
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        storedOffMeshConCount++;
                    }
                }
            }

            //off-mesh connections stored as polygons, adjust values
            int totPolyCount = polyMesh.PolyCount + storedOffMeshConCount;
            int totVertCount = polyMesh.VertCount + storedOffMeshConCount * 2;

            //find portal edges
            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                PolyMesh.Polygon p = polyMesh.Polys[i];
                for (int j = 0; j < nvp; j++)
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

            //allocate data
            this.Header = new PathfindingCommon.NavMeshInfo();
            this.NavVerts = new Vector3[totVertCount];
            this.NavPolys = new Poly[totPolyCount];
            this.NavDMeshes = new PolyMeshDetail.MeshData[polyMesh.PolyCount];
            this.NavDVerts = new Vector3[uniqueDetailVertCount];
            this.NavDTris = new PolyMeshDetail.TriangleData[detailTriCount];
            this.OffMeshCons = new OffMeshConnection[storedOffMeshConCount];

            //store header
            //HACK TiledNavMesh should figure out the X/Y/layer instead of the user maybe?
            this.Header.X = 0;
            this.Header.Y = 0;
            this.Header.Layer = 0;
            this.Header.PolyCount = totPolyCount;
            this.Header.VertCount = totVertCount;
            this.Header.MaxLinkCount = maxLinkCount;
            this.Header.Bounds = polyMesh.Bounds;
            this.Header.DetailMeshCount = polyMesh.PolyCount;
            this.Header.DetailVertCount = uniqueDetailVertCount;
            this.Header.DetailTriCount = detailTriCount;
            this.Header.OffMeshBase = polyMesh.PolyCount;
            this.Header.WalkableHeight = agentHeight;
            this.Header.WalkableRadius = agentRadius;
            this.Header.WalkableClimb = maxClimb;
            this.Header.OffMeshConCount = storedOffMeshConCount;
            this.Header.BvNodeCount = buildBoundingVolumeTree ? polyMesh.PolyCount * 2 : 0;
            this.Header.BvQuantFactor = 1f / cellSize;

            int offMeshVertsBase = polyMesh.VertCount;
            int offMeshPolyBase = polyMesh.PolyCount;

            //store vertices
            for (int i = 0; i < polyMesh.VertCount; i++)
            {
                PolyVertex iv = polyMesh.Verts[i];
                this.NavVerts[i].X = polyMesh.Bounds.Minimum.X + iv.X * cellSize;
                this.NavVerts[i].Y = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                this.NavVerts[i].Z = polyMesh.Bounds.Minimum.Z + iv.Z * cellSize;
            }

            //off-mesh link vertices
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

            //store polygons
            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                this.NavPolys[i] = new Poly();
                this.NavPolys[i].VertCount = 0;
                this.NavPolys[i].Area = polyMesh.Polys[i].Area;
                this.NavPolys[i].PolyType = PolygonType.Ground;
                this.NavPolys[i].Verts = new int[nvp];
                this.NavPolys[i].Neis = new int[nvp];
                for (int j = 0; j < nvp; j++)
                {
                    if (polyMesh.Polys[i].Vertices[j] == PolyMesh.NullId)
                    {
                        break;
                    }

                    this.NavPolys[i].Verts[j] = polyMesh.Polys[i].Vertices[j];
                    if (PolyMesh.IsBoundaryEdge(polyMesh.Polys[i].NeighborEdges[j]))
                    {
                        //border or portal edge
                        int dir = polyMesh.Polys[i].NeighborEdges[j] % 16;
                        if (dir == 0xf) //border
                        {
                            this.NavPolys[i].Neis[j] = 0;
                        }
                        else if (dir == 0) //portal x-
                        {
                            this.NavPolys[i].Neis[j] = Link.External | 4;
                        }
                        else if (dir == 1) //portal z+
                        {
                            this.NavPolys[i].Neis[j] = Link.External | 2;
                        }
                        else if (dir == 2) //portal x+
                        {
                            this.NavPolys[i].Neis[j] = Link.External | 0;
                        }
                        else if (dir == 3) //portal z-
                        {
                            this.NavPolys[i].Neis[j] = Link.External | 6;
                        }
                    }
                    else
                    {
                        //normal connection
                        this.NavPolys[i].Neis[j] = polyMesh.Polys[i].NeighborEdges[j] + 1;
                    }

                    this.NavPolys[i].VertCount++;
                }
            }

            //off-mesh connection vertices
            n = 0;
            for (int i = 0; i < offMeshCons.Length; i++)
            {
                //only store connections which start from this tile
                if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                {
                    this.NavPolys[offMeshPolyBase + n] = new Poly();
                    this.NavPolys[offMeshPolyBase + n].VertCount = 2;
                    this.NavPolys[offMeshPolyBase + n].Verts = new int[nvp];
                    this.NavPolys[offMeshPolyBase + n].Verts[0] = offMeshVertsBase + (n * 2 + 0);
                    this.NavPolys[offMeshPolyBase + n].Verts[1] = offMeshVertsBase + (n * 2 + 1);
                    this.NavPolys[offMeshPolyBase + n].Area = polyMesh.Polys[offMeshCons[i].Poly].Area; //HACK is this correct?
                    this.NavPolys[offMeshPolyBase + n].PolyType = PolygonType.OffMeshConnection;
                    n++;
                }
            }

            //store detail meshes and vertices
            if (polyMeshDetail != null)
            {
                int vbase = 0;
                List<Vector3> storedDetailVerts = new List<Vector3>();
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int vb = polyMeshDetail.Meshes[i].VertexIndex;
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = this.NavPolys[i].VertCount;
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
                    int numPolyVerts = this.NavPolys[i].VertCount;
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
                this.NavBvTree = new BVTree(polyMesh.Verts, polyMesh.Polys, nvp, cellSize, cellHeight);
            }

            //store off-mesh connections
            n = 0;
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
