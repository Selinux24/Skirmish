using SharpDX;
using System;
using System.Collections.Generic;

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
        public static NavMeshBuilder Build(
            Geometry.PolyMesh polyMesh,
            Geometry.PolyMeshDetail polyMeshDetail,
            OffMeshConnection[] offMeshCons,
            float cellSize, float cellHeight, int vertsPerPoly, float maxClimb)
        {
            NavMeshBuilder result = new NavMeshBuilder();

            #region Classify off-mesh connection points

            BoundarySide[] offMeshSides = new BoundarySide[offMeshCons != null ? offMeshCons.Length * 2 : 0];
            int storedOffMeshConCount = 0;
            int offMeshConLinkCount = 0;

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                //find height bounds
                float hmin = float.MaxValue;
                float hmax = -float.MaxValue;

                if (polyMeshDetail != null)
                {
                    #region With detailed mesh

                    for (int i = 0; i < polyMeshDetail.VertCount; i++)
                    {
                        float h = polyMeshDetail.Verts[i].Y;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }

                    #endregion
                }
                else
                {
                    #region With mesh

                    for (int i = 0; i < polyMesh.VertCount; i++)
                    {
                        var iv = polyMesh.Verts[i];
                        float h = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                        hmin = Math.Min(hmin, h);
                        hmax = Math.Max(hmax, h);
                    }

                    #endregion
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
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal) offMeshConLinkCount++;
                    if (offMeshSides[i * 2 + 1] == BoundarySide.Internal) offMeshConLinkCount++;
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal) storedOffMeshConCount++;
                }
            }

            #endregion

            #region Find portal edges

            int edgeCount = 0;
            int portalCount = 0;
            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                var p = polyMesh.Polys[i];
                for (int j = 0; j < vertsPerPoly; j++)
                {
                    if (p.Vertices[j] != Geometry.PolyMesh.NullId)
                    {
                        edgeCount++;

                        if (Geometry.PolyMesh.IsBoundaryEdge(p.NeighborEdges[j]))
                        {
                            int dir = p.NeighborEdges[j] % 16;

                            if (dir != 15) portalCount++;
                        }
                    }
                }
            }

            int maxLinkCount = edgeCount + portalCount * 2 + offMeshConLinkCount * 2;

            #endregion

            //Find unique detail vertices
            int uniqueDetailVertCount = 0;
            int detailTriCount = 0;
            if (polyMeshDetail != null)
            {
                #region With detailed mesh

                detailTriCount = polyMeshDetail.TrisCount;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = polyMesh.Polys[i].VertexCount;
                    uniqueDetailVertCount += numDetailVerts - numPolyVerts;
                }

                #endregion
            }
            else
            {
                #region With mesh

                uniqueDetailVertCount = 0;
                detailTriCount = 0;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numPolyVerts = polyMesh.Polys[i].VertexCount;
                    uniqueDetailVertCount += numPolyVerts - 2;
                }

                #endregion
            }

            //store header
            //HACK TiledNavMesh should figure out the X/Y/layer instead of the user maybe?
            //result.header = new PathfindingCommon.NavMeshInfo();
            //header.X = 0;
            //header.Y = 0;
            //header.Layer = 0;
            //header.PolyCount = totPolyCount;
            //header.VertCount = totVertCount;
            //header.MaxLinkCount = maxLinkCount;
            //header.Bounds = polyMesh.Bounds;
            //header.DetailMeshCount = polyMesh.PolyCount;
            //header.DetailVertCount = uniqueDetailVertCount;
            //header.DetailTriCount = detailTriCount;
            //header.OffMeshBase = polyMesh.PolyCount;
            //header.WalkableHeight = settings.AgentHeight;
            //header.WalkableRadius = settings.AgentRadius;
            //header.WalkableClimb = maxClimb;
            //header.OffMeshConCount = storedOffMeshConCount;
            //header.BvNodeCount = settings.BuildBoundingVolumeTree ? polyMesh.PolyCount * 2 : 0;
            //header.BvQuantFactor = 1f / cellSize;

            //off-mesh connections stored as polygons, adjust values
            int offMeshVertsBase = polyMesh.VertCount;
            int offMeshPolyBase = polyMesh.PolyCount;
            int totPolyCount = polyMesh.PolyCount + storedOffMeshConCount;
            int totVertCount = polyMesh.VertCount + storedOffMeshConCount * 2;

            //Allocate data
            result.NavVerts = new Vector3[totVertCount];
            result.NavPolys = new Poly[totPolyCount];
            result.navDMeshes = new Geometry.PolyMeshDetail.MeshData[polyMesh.PolyCount];
            result.navDVerts = new Vector3[uniqueDetailVertCount];
            result.navDTris = new Geometry.PolyMeshDetail.TriangleData[detailTriCount];
            result.offMeshConnections = new OffMeshConnection[storedOffMeshConCount];

            #region Store vertices

            for (int i = 0; i < polyMesh.VertCount; i++)
            {
                var iv = polyMesh.Verts[i];
                result.NavVerts[i].X = polyMesh.Bounds.Minimum.X + iv.X * cellSize;
                result.NavVerts[i].Y = polyMesh.Bounds.Minimum.Y + iv.Y * cellHeight;
                result.NavVerts[i].Z = polyMesh.Bounds.Minimum.Z + iv.Z * cellSize;
            }

            #endregion

            #region Off-mesh link vertices

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        result.NavVerts[offMeshVertsBase + (n * 2 + 0)] = offMeshCons[i].Pos0;
                        result.NavVerts[offMeshVertsBase + (n * 2 + 1)] = offMeshCons[i].Pos1;
                        n++;
                    }
                }
            }

            #endregion

            #region Store vertices

            for (int i = 0; i < polyMesh.PolyCount; i++)
            {
                result.NavPolys[i] = new Poly();
                result.NavPolys[i].VertCount = 0;
                result.NavPolys[i].Area = polyMesh.Polys[i].Area;
                result.NavPolys[i].PolyType = PolygonType.Ground;
                result.NavPolys[i].Verts = new int[vertsPerPoly];
                result.NavPolys[i].Neis = new int[vertsPerPoly];

                for (int j = 0; j < vertsPerPoly; j++)
                {
                    if (polyMesh.Polys[i].Vertices[j] != Geometry.PolyMesh.NullId)
                    {
                        result.NavPolys[i].Verts[j] = polyMesh.Polys[i].Vertices[j];
                        if (Geometry.PolyMesh.IsBoundaryEdge(polyMesh.Polys[i].NeighborEdges[j]))
                        {
                            //border or portal edge
                            int dir = polyMesh.Polys[i].NeighborEdges[j] % 16;
                            if (dir == 0xf) result.NavPolys[i].Neis[j] = 0; //border
                            else if (dir == 0) result.NavPolys[i].Neis[j] = Link.External | 4; //portal x-
                            else if (dir == 1) result.NavPolys[i].Neis[j] = Link.External | 2; //portal z+
                            else if (dir == 2) result.NavPolys[i].Neis[j] = Link.External | 0; //portal x+
                            else if (dir == 3) result.NavPolys[i].Neis[j] = Link.External | 6; //portal z-
                        }
                        else
                        {
                            //normal connection
                            result.NavPolys[i].Neis[j] = polyMesh.Polys[i].NeighborEdges[j] + 1;
                        }

                        result.NavPolys[i].VertCount++;
                    }
                }
            }

            #endregion

            #region Off-mesh connection vertices

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < offMeshCons.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        result.NavPolys[offMeshPolyBase + n] = new Poly();
                        result.NavPolys[offMeshPolyBase + n].VertCount = 2;
                        result.NavPolys[offMeshPolyBase + n].Verts = new int[vertsPerPoly];
                        result.NavPolys[offMeshPolyBase + n].Verts[0] = offMeshVertsBase + (n * 2 + 0);
                        result.NavPolys[offMeshPolyBase + n].Verts[1] = offMeshVertsBase + (n * 2 + 1);
                        result.NavPolys[offMeshPolyBase + n].Area = polyMesh.Polys[offMeshCons[i].Poly].Area;
                        result.NavPolys[offMeshPolyBase + n].PolyType = PolygonType.OffMeshConnection;
                        n++;
                    }
                }
            }

            #endregion

            //store detail meshes and vertices
            if (polyMeshDetail != null)
            {
                #region With detailed mesh

                int vbase = 0;
                List<Vector3> storedDetailVerts = new List<Vector3>();
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int vb = polyMeshDetail.Meshes[i].VertexIndex;
                    int numDetailVerts = polyMeshDetail.Meshes[i].VertexCount;
                    int numPolyVerts = result.NavPolys[i].VertCount;
                    result.navDMeshes[i].VertexIndex = vbase;
                    result.navDMeshes[i].VertexCount = numDetailVerts - numPolyVerts;
                    result.navDMeshes[i].TriangleIndex = polyMeshDetail.Meshes[i].TriangleIndex;
                    result.navDMeshes[i].TriangleCount = polyMeshDetail.Meshes[i].TriangleCount;

                    //Copy detail vertices 
                    //first 'nv' verts are equal to nav poly verts
                    //the rest are detail verts
                    for (int j = 0; j < result.navDMeshes[i].VertexCount; j++)
                    {
                        storedDetailVerts.Add(polyMeshDetail.Verts[vb + numPolyVerts + j]);
                    }

                    vbase += numDetailVerts - numPolyVerts;
                }

                result.navDVerts = storedDetailVerts.ToArray();

                //store triangles
                for (int j = 0; j < polyMeshDetail.TrisCount; j++)
                {
                    result.navDTris[j] = polyMeshDetail.Tris[j];
                }

                #endregion
            }
            else
            {
                #region With mesh

                //create dummy detail mesh by triangulating polys
                int tbase = 0;
                for (int i = 0; i < polyMesh.PolyCount; i++)
                {
                    int numPolyVerts = result.NavPolys[i].VertCount;
                    result.navDMeshes[i].VertexIndex = 0;
                    result.navDMeshes[i].VertexCount = 0;
                    result.navDMeshes[i].TriangleIndex = tbase;
                    result.navDMeshes[i].TriangleCount = numPolyVerts - 2;

                    //triangulate polygon
                    for (int j = 2; j < numPolyVerts; j++)
                    {
                        result.navDTris[tbase].VertexHash0 = 0;
                        result.navDTris[tbase].VertexHash1 = j - 1;
                        result.navDTris[tbase].VertexHash2 = j;

                        //bit for each edge that belongs to the poly boundary
                        result.navDTris[tbase].Flags = 1 << 2;
                        if (j == 2) result.navDTris[tbase].Flags |= 1 << 0;
                        if (j == numPolyVerts - 1) result.navDTris[tbase].Flags |= 1 << 4;

                        tbase++;
                    }
                }

                #endregion
            }

            #region Store off-mesh connections

            if (offMeshCons != null && offMeshCons.Length > 0)
            {
                int n = 0;
                for (int i = 0; i < result.offMeshConnections.Length; i++)
                {
                    //only store connections which start from this tile
                    if (offMeshSides[i * 2 + 0] == BoundarySide.Internal)
                    {
                        result.offMeshConnections[n] = new OffMeshConnection();

                        result.offMeshConnections[n].Poly = offMeshPolyBase + n;

                        //copy connection end points
                        result.offMeshConnections[n].Pos0 = offMeshCons[i].Pos0;
                        result.offMeshConnections[n].Pos1 = offMeshCons[i].Pos1;

                        result.offMeshConnections[n].Radius = offMeshCons[i].Radius;
                        result.offMeshConnections[n].Flags = offMeshCons[i].Flags;
                        result.offMeshConnections[n].Side = offMeshSides[i * 2 + 1];
                        result.offMeshConnections[n].Tag = offMeshCons[i].Tag;

                        n++;
                    }
                }
            }

            #endregion

            return result;
        }

        private PathfindingCommon.NavMeshInfo header;
        private Geometry.PolyMeshDetail.MeshData[] navDMeshes;
        private Vector3[] navDVerts;
        private Geometry.PolyMeshDetail.TriangleData[] navDTris;
        //private BVTree navBvTree;
        private OffMeshConnection[] offMeshConnections;

        /// <summary>
        /// Gets the file header
        /// </summary>
        public PathfindingCommon.NavMeshInfo Header
        {
            get
            {
                return header;
            }
        }
        public Vector3[] NavVerts;
        public Poly[] NavPolys;
    }
}
