using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.NavMesh
{
    using Engine.Common;

    /// <summary>
    /// Navigation Mesh
    /// </summary>
    public class NavigationMesh : IGraph
    {
        /// <summary>
        /// Padding for bounding box compute
        /// </summary>
        private const float BOUND_PADDING = float.Epsilon * 2f;

        class AgentTypeData
        {
            public TiledNavigationMesh NavigationMesh;
            public NavigationMeshNode[] Nodes;
        }

        /// <summary>
        /// Tiled navigation meshes
        /// </summary>
        private readonly Dictionary<AgentType, AgentTypeData> agentTypeData = new Dictionary<AgentType, AgentTypeData>();

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
            var bbox = ComputeBoundingBox(triangles);

            var nm = new NavigationMesh();

            foreach (var agent in settings.Agents)
            {
                BuildQuery(agent, nm, bbox, triangles, settings);
            }

            return nm;
        }

        private static void BuildQuery(NavigationMeshAgentType agent, NavigationMesh nm, BoundingBox bbox, Triangle[] triangles, NavigationMeshGenerationSettings settings)
        {
            var voxelAgentHeight = settings.GetVoxelAgentHeight(agent);
            var voxelAgentRadius = settings.GetVoxelAgentRadius(agent);
            var voxelMaxClimb = settings.GetVoxelMaxClimb(agent);

            var hf = HeightField.Build(bbox, triangles, settings.CellSize, settings.CellHeight, voxelAgentHeight, voxelMaxClimb);
            var chf = CompactHeightField.Build(hf, settings.RegionMinSize, settings.RegionMergedSize, voxelAgentHeight, voxelAgentRadius, voxelMaxClimb);

            var cs = chf.BuildContourSet(settings.EdgeMaxError, settings.EdgeMaxLength, settings.ContourFlags);
            var pm = new PolyMesh(cs, settings.CellSize, settings.CellHeight, 0, settings.VertsPerPoly);
            var pmd = new PolyMeshDetail(pm, chf, settings.DetailSampleDistance, settings.DetailSampleMaxError);

            var builder = new NavigationMeshBuilder(
                pm,
                pmd,
                new OffMeshConnection[0],
                settings.CellSize,
                settings.CellHeight,
                settings.VertsPerPoly,
                agent.MaxClimb,
                settings.BuildBoundingVolumeTree,
                agent.Height,
                agent.Radius);

            var tnm = new TiledNavigationMesh(builder);

            var nodes = new List<NavigationMeshNode>();
            for (int m = 0; m < pmd.MeshCount; m++)
            {
                var mesh = pmd.Meshes[m];
                var poly = pm.Polys[m];

                int vertIndex = mesh.VertexIndex;
                int triIndex = mesh.TriangleIndex;

                for (int j = 0; j < mesh.TriangleCount; j++)
                {
                    var t = pmd.Tris[triIndex + j];

                    var v0 = pmd.Verts[vertIndex + t.VertexHash0];
                    var v1 = pmd.Verts[vertIndex + t.VertexHash1];
                    var v2 = pmd.Verts[vertIndex + t.VertexHash2];

                    nodes.Add(new NavigationMeshNode(nm, new Polygon(v0, v1, v2), m, poly.RegionId.Id));
                }
            }

            nm.agentTypeData.Add(agent, new AgentTypeData()
            {
                NavigationMesh = tnm,
                Nodes = nodes.ToArray(),
            });
        }
        /// <summary>
        /// Computes a bounding box
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Retusn a bounding box</returns>
        private static BoundingBox ComputeBoundingBox(Triangle[] triangles)
        {
            if (triangles == null || triangles.Length == 0)
            {
                return new BoundingBox();
            }

            var bbox = BoundingBox.FromPoints(triangles[0].GetVertices());

            Array.ForEach(triangles, tri =>
            {
                bbox = BoundingBox.Merge(bbox, BoundingBox.FromPoints(tri.GetVertices()));

                bbox.Minimum.X -= BOUND_PADDING;
                bbox.Minimum.Y -= BOUND_PADDING;
                bbox.Minimum.Z -= BOUND_PADDING;
                bbox.Maximum.X += BOUND_PADDING;
                bbox.Maximum.Y += BOUND_PADDING;
                bbox.Maximum.Z += BOUND_PADDING;
            });

            return bbox;
        }

        /// <summary>
        /// Gets the node list
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collections for the specified list</returns>
        public IGraphNode[] GetNodes(AgentType agent)
        {
            return Array.ConvertAll(this.agentTypeData[agent].Nodes, (n) => { return (IGraphNode)n; });
        }
        /// <summary>
        /// Finds a path over the navigation mesh
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <returns>Returns path between the specified points if exists</returns>
        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            using (var query = new NavigationMeshQuery(this.agentTypeData[agent].NavigationMesh, 2048))
            {
                Vector3[] path;
                if (query.FindPath(from, to, out path))
                {
                    return path;
                }
            }

            return null;
        }
        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public bool IsWalkable(AgentType agent, Vector3 position, out Vector3? nearest)
        {
            using (var query = new NavigationMeshQuery(this.agentTypeData[agent].NavigationMesh, 2048))
            {
                var nmAgent = agent as NavigationMeshAgentType;
                Vector3 extents = nmAgent != null ? nmAgent.MaxClimb * Vector3.One : Vector3.One;

                return query.IsWalkable(position, extents, out nearest);
            }
        }

        /// <summary>
        /// Adds a cylinder obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 position, float radius, float height)
        {
            return -1;
        }
        /// <summary>
        /// Adds a oriented bounding box obstacle
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="halfExtents">half extent vectors</param>
        /// <param name="yRotation">Rotation in the y axis</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 position, Vector3 halfExtents, float yRotation)
        {
            return -1;
        }
        /// <summary>
        /// Adds a bounding box obstacle
        /// </summary>
        /// <param name="minimum">Minimum corner</param>
        /// <param name="maximum">Maximum corner</param>
        /// <returns>Returns the obstacle id</returns>
        public int AddObstacle(Vector3 minimum, Vector3 maximum)
        {
            return -1;
        }
        /// <summary>
        /// Removes an obstacle by obstacle id
        /// </summary>
        /// <param name="obstacle">Obstacle id</param>
        public void RemoveObstacle(int obstacle)
        {

        }

        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Save(string fileName)
        {

        }
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void Load(string fileName)
        {

        }
        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {

        }

        internal Crowds.Crowd AddCrowd(bool adaptative, AgentType agent)
        {
            if (!this.agentTypeData.ContainsKey(agent))
            {
                throw new EngineException("Bad agent type. Include the agent in navigation mesh settings before generation.");
            }

            var navMesh = this.agentTypeData[agent].NavigationMesh;

            return new Crowds.Crowd(10, adaptative, ref navMesh);
        }
        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return "NavigationMesh";
        }
    }
}
