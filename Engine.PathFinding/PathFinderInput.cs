﻿using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    /// <summary>
    /// Path finder input
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="fnc">Get triangle function</param>
    public abstract class PathFinderInput(Func<IEnumerable<Triangle>> fnc)
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
        /// Area list
        /// </summary>
        private readonly List<IGraphArea> areas = new(MaxAreas);
        /// <summary>
        /// Connection list
        /// </summary>
        private readonly List<IGraphConnection> connections = new(MaxConnections);
        /// <summary>
        /// Get triangle function
        /// </summary>
        private readonly Func<IEnumerable<Triangle>> getTrianglesFnc = fnc;
        /// <summary>
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }

        /// <summary>
        /// Gets the triangle list
        /// </summary>
        /// <param name="bounds">Bounding box to filter the triangle list</param>
        public async Task<IEnumerable<Triangle>> GetTrianglesAsync(BoundingBox? bounds)
        {
            if (getTrianglesFnc == null)
            {
                return [];
            }

            IEnumerable<Triangle> tris = null;
            BoundingBox bbox = default;

            await Task.Run(() =>
            {
                tris = getTrianglesFnc() ?? [];

                if (bounds.HasValue)
                {
                    tris = tris.Where(t =>
                    {
                        return Intersection.BoxContainsTriangle(bounds.Value, t) != ContainmentType.Disjoint;
                    });
                }

                bbox = GeometryUtil.CreateBoundingBox(tris);
            });

            BoundingBox = bbox;

            return tris;
        }
        /// <summary>
        /// Gets the triangle list
        /// </summary>
        /// <param name="bounds">Bounding box to filter the triangle list</param>
        public IEnumerable<Triangle> GetTriangles(BoundingBox? bounds)
        {
            if (getTrianglesFnc == null)
            {
                return [];
            }

            var tris = getTrianglesFnc() ?? [];

            if (bounds.HasValue)
            {
                tris = tris.Where(t =>
                {
                    return Intersection.BoxContainsTriangle(bounds.Value, t) != ContainmentType.Disjoint;
                });
            }

            BoundingBox = GeometryUtil.CreateBoundingBox(tris);

            return tris;
        }

        /// <summary>
        /// Adds a new area to input
        /// </summary>
        /// <param name="graphArea">Area</param>
        /// <returns>Returns the area id</returns>
        public int AddArea(IGraphArea graphArea)
        {
            if (graphArea == null) return -1;

            if (areas.Count >= MaxAreas) return -1;

            areas.Add(graphArea);

            return graphArea.Id;
        }
        /// <summary>
        /// Gets an area by id
        /// </summary>
        /// <param name="id">Area id</param>
        /// <returns>Returns an area</returns>
        public IGraphArea GetArea(int id)
        {
            return areas.Find(a => a.Id == id);
        }
        /// <summary>
        /// Removes area by id
        /// </summary>
        /// <param name="id">Area id</param>
        public void RemoveArea(int id)
        {
            areas.RemoveAll(a => a.Id == id);
        }
        /// <summary>
        /// Gets area list
        /// </summary>
        /// <returns>Returns the area list</returns>
        public IGraphArea[] GetAreas()
        {
            return [.. areas];
        }
        /// <summary>
        /// Clears all areas
        /// </summary>
        public void ClearAreas()
        {
            areas.Clear();
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
        /// <returns>Returns the connection id</returns>
        public int AddConnection<T, Y>(Vector3 spos, Vector3 epos, float rad, bool bidir, T area, Y flags) where T : Enum where Y : Enum
        {
            if (connections.Count >= MaxConnections) return -1;

            var connection = new GraphConnection
            {
                Radius = rad,
                BiDirectional = bidir,
                Start = spos,
                End = epos,
            };

            connection.SetAreaType(area);
            connection.SetFlagType(flags);

            connections.Add(connection);

            return connection.Id;
        }
        /// <summary>
        /// Gets a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        /// <returns>Returns a connection</returns>
        public IGraphConnection GetConnection(int id)
        {
            return connections.Find(c => c.Id == id);
        }
        /// <summary>
        /// Deletes a connection by id
        /// </summary>
        /// <param name="id">Connection id</param>
        public void RemoveConnection(int id)
        {
            connections.RemoveAll(c => c.Id == id);
        }
        /// <summary>
        /// Gets the connection list
        /// </summary>
        /// <returns>Returns the connection list</returns>
        public IGraphConnection[] GetConnections()
        {
            return [.. connections];
        }
        /// <summary>
        /// Clears all connections
        /// </summary>
        public void ClearConnections()
        {
            connections.Clear();
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
        /// <param name="agentTypes">Agent type list</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new created graph</returns>
        public abstract Task<IGraph> CreateGraphAsync(PathFinderSettings settings, AgentType[] agentTypes, Action<float> progressCallback = null);
        /// <summary>
        /// Creates a new graph from current input
        /// </summary>
        /// <param name="settings">Creation settings</param>
        /// <param name="agentTypes">Agent type list</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new created graph</returns>
        public abstract IGraph CreateGraph(PathFinderSettings settings, AgentType[] agentTypes, Action<float> progressCallback = null);
        /// <summary>
        /// Refresh
        /// </summary>
        /// <param name="settings">Creation settings</param>
        public abstract Task RefreshAsync(PathFinderSettings settings);
        /// <summary>
        /// Refresh
        /// </summary>
        /// <param name="settings">Creation settings</param>
        public abstract void Refresh(PathFinderSettings settings);

        /// <summary>
        /// Gets the input hash
        /// </summary>
        /// <param name="settings">Path finder settings</param>
        /// <returns>Returns the input hash</returns>
        public abstract Task<string> GetHashAsync(PathFinderSettings settings);
        /// <summary>
        /// Gets the input hash
        /// </summary>
        /// <param name="settings">Path finder settings</param>
        /// <returns>Returns the input hash</returns>
        public abstract string GetHash(PathFinderSettings settings);
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        /// <remarks>If hash specified, the input proceed to compare file hash and specified hash. If they are different, the returns null.</remarks>
        public abstract Task<IGraph> LoadAsync(string fileName, string hash = null);
        /// <summary>
        /// Loads the graph from a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="hash">Source hash</param>
        /// <returns>Returns the loaded graph</returns>
        /// <remarks>If hash specified, the input proceed to compare file hash and specified hash. If they are different, the returns null.</remarks>
        public abstract IGraph Load(string fileName, string hash = null);
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public abstract Task SaveAsync(string fileName, IGraph graph);
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph instance</param>
        public abstract void Save(string fileName, IGraph graph);
    }
}
