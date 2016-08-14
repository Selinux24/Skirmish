using System;
using SharpDX;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Common;

    /// <summary>
    /// Navigation Mesh
    /// </summary>
    public class NavigationMesh : IGraph
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly NavigationMeshQuery Query = null;
        /// <summary>
        /// 
        /// </summary>
        protected readonly NavigationMeshNode[] Nodes = null;

        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavigationMesh Build(VertexData[] vertices, uint[] indices, NavigationMeshGenerationSettings settings)
        {
            int tris = indices.Length / 3;

            Triangle[] triangles = new Triangle[tris];

            int index = 0;
            for (int i = 0; i < tris; i++)
            {
                triangles[i] = new Triangle(
                    vertices[indices[index++]].Position.Value,
                    vertices[indices[index++]].Position.Value,
                    vertices[indices[index++]].Position.Value);
            }

            return Build(triangles, settings);
        }
        /// <summary>
        /// Navigation Mesh Build
        /// </summary>
        /// <param name="triangles">List of triangles</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns a navigation mesh</returns>
        public static NavigationMesh Build(Triangle[] triangles, NavigationMeshGenerationSettings settings)
        {
            BoundingBox bbox = BoundingBox.FromPoints(triangles[0].GetCorners());
            Array.ForEach(triangles, tri => bbox = BoundingBox.Merge(bbox, BoundingBox.FromPoints(tri.GetCorners())));

            var hf = HeightField.Build(bbox, triangles, settings);
            var chf = CompactHeightField.Build(hf, settings);
            var cs = chf.BuildContourSet(settings.MaxEdgeError, settings.MaxEdgeLength, settings.ContourFlags);
            var pm = new PolyMesh(cs, settings.CellSize, settings.CellHeight, 0, settings.VertsPerPoly);
            var pmd = new PolyMeshDetail(pm, chf, settings.SampleDistance, settings.MaxSampleError);
            var nm = new NavigationMesh(pm, pmd, settings);

            return nm;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pm">Poly mesh</param>
        /// <param name="pmd">Poly mesh detail</param>
        /// <param name="settings">Generation settings</param>
        protected NavigationMesh(PolyMesh pm, PolyMeshDetail pmd, NavigationMeshGenerationSettings settings)
        {
            var builder = new NavigationMeshBuilder(
                pm,
                pmd,
                new OffMeshConnection[0],
                settings.CellSize,
                settings.CellHeight,
                settings.VertsPerPoly,
                settings.MaxClimb,
                settings.BuildBoundingVolumeTree,
                settings.AgentHeight,
                settings.AgentRadius);

            var tnm = new TiledNavigationMesh(builder);
            this.Query = new NavigationMeshQuery(tnm, 2048);
            this.Nodes = new NavigationMeshNode[pmd.MeshCount];

            for (int i = 0; i < pmd.MeshCount; i++)
            {
                var mesh = pmd.Meshes[i];
                var poly = pm.Polys[i];

                this.Nodes[i] = new NavigationMeshNode(this, new Polygon(mesh.VertexCount), i, poly.RegionId.Id);

                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    this.Nodes[i].Poly.Points[v] = pmd.Verts[mesh.VertexIndex + v];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IGraphNode[] GetNodes()
        {
            return Array.ConvertAll(this.Nodes, (n) => { return (IGraphNode)n; });
        }
        /// <summary>
        /// Finds a path over the navigation mesh
        /// </summary>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <returns>Returns path between the specified points if exists</returns>
        public PathFindingPath FindPath(Vector3 from, Vector3 to)
        {
            Vector3[] path;
            if (this.Query.FindPath(from, to, out path))
            {
                return new PathFindingPath(path);
            }

            return null;
        }
        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(Vector3 position)
        {
            return this.Query.IsWalkable(position);
        }
        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Nodes: {0}", this.Nodes != null ? this.Nodes.Length : 0);
        }
    }
}
