using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    using Engine.Common;

    /// <summary>
    /// Path finder input
    /// </summary>
    public abstract class PathFinderInput
    {
        /// <summary>
        /// Maximum areas
        /// </summary>
        public const int MaxAreas = 256;
        /// <summary>
        /// Maximum connections
        /// </summary>
        public const int MaxConnections = 256;

        /// <summary>
        /// Get triangle function
        /// </summary>
        private readonly Func<IEnumerable<Triangle>> getTrianglesFnc = null;
        /// <summary>
        /// Area list
        /// </summary>
        private readonly List<IGraphArea> areas = new List<IGraphArea>(MaxAreas);
        /// <summary>
        /// Connection list
        /// </summary>
        private readonly List<IGraphConnection> connections = new List<IGraphConnection>(MaxConnections);
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fnc">Get triangle function</param>
        protected PathFinderInput(Func<IEnumerable<Triangle>> fnc)
        {
            getTrianglesFnc = fnc;
        }

        /// <summary>
        /// Gets the triangle list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Triangle> GetTriangles()
        {
            if (getTrianglesFnc != null)
            {
                var tris = getTrianglesFnc();

                BoundingBox = GeometryUtil.CreateBoundingBox(tris);

                return tris ?? new Triangle[] { };
            }
            else
            {
                return new Triangle[] { };
            }
        }

        /// <summary>
        /// Adds a new area to input
        /// </summary>
        /// <param name="verts">Area polygon vertices</param>
        /// <param name="nverts">Vertex count</param>
        /// <param name="minh">Minimum height</param>
        /// <param name="maxh">Maximum height</param>
        /// <param name="area">Area type</param>
        public void AddArea(IEnumerable<Vector3> verts, int nverts, float minh, float maxh, GraphAreaTypes area)
        {
            if (areas.Count >= MaxAreas) return;

            areas.Add(new GraphArea
            {
                Vertices = verts?.ToArray(),
                VertexCount = nverts,
                MinHeight = minh,
                MaxHeight = maxh,
                AreaType = area,
            });
        }
        /// <summary>
        /// Deletes area by id
        /// </summary>
        /// <param name="id">Area id</param>
        public void DeleteArea(int id)
        {
            areas.RemoveAll(a => a.Id == id);
        }
        /// <summary>
        /// Gets area list
        /// </summary>
        /// <returns>Returns the area list</returns>
        public IEnumerable<IGraphArea> GetAreas()
        {
            return areas.ToArray();
        }
        /// <summary>
        /// Gets the area count
        /// </summary>
        /// <returns>Returns the area count</returns>
        public int GetAreaCount()
        {
            return areas.Count;
        }

        /// <summary>
        /// Adds a new connection
        /// </summary>
        /// <param name="spos">Start position</param>
        /// <param name="epos">End position</param>
        /// <param name="rad">Point radius</param>
        /// <param name="bidir">Connection direction</param>
        /// <param name="area">Area type</param>
        /// <param name="flags">Area flags</param>
        public void AddConnection(Vector3 spos, Vector3 epos, float rad, int bidir, GraphConnectionAreaTypes area, GraphConnectionFlagTypes flags)
        {
            if (connections.Count >= MaxConnections) return;

            connections.Add(new GraphConnection
            {
                Radius = rad,
                Direction = bidir,
                AreaType = area,
                FlagTypes = flags,
                Start = spos,
                End = epos,
            });
        }
        /// <summary>
        /// Deletes a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        public void DeleteConnection(int id)
        {
            connections.RemoveAll(c => c.Id == id);
        }
        /// <summary>
        /// Gets the connection list
        /// </summary>
        /// <returns>Returns the connection list</returns>
        public IEnumerable<IGraphConnection> GetConnections()
        {
            return connections.ToArray();
        }
        /// <summary>
        /// Gets the connection count
        /// </summary>
        /// <returns>Returns the connection count</returns>
        public int GetConnectionCount()
        {
            return connections.Count;
        }

        /// <summary>
        /// Creates a new graph from current input
        /// </summary>
        /// <param name="settings">Creation settings</param>
        /// <returns>Returns the new created graph</returns>
        public abstract Task<IGraph> CreateGraph(PathFinderSettings settings);
        /// <summary>
        /// Refresh
        /// </summary>
        public abstract Task Refresh();

        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the loaded graph</returns>
        public abstract Task<IGraph> Load(string fileName);
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public abstract Task Save(string fileName, IGraph graph);
    }
}
