using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.PathFinding.AStar
{
    using Engine.Common;

    /// <summary>
    /// Grid
    /// </summary>
    public class Grid : IGraph
    {
        /// <summary>
        /// Collision info helper
        /// </summary>
        class GridCollisionInfo
        {
            /// <summary>
            /// Collision point
            /// </summary>
            public Vector3 Point;
            /// <summary>
            /// Collision triangle
            /// </summary>
            public Triangle Triangle;
            /// <summary>
            /// Distance to point
            /// </summary>
            public float Distance;

            /// <summary>
            /// Gets text representarion of collision
            /// </summary>
            /// <returns>Returns text representarion of collision</returns>
            public override string ToString()
            {
                return string.Format("{0}", this.Point);
            }
        }

        /// <summary>
        /// On graph updating event
        /// </summary>
        public event EventHandler Updating;
        /// <summary>
        /// On graph updated event
        /// </summary>
        public event EventHandler Updated;

        /// <summary>
        /// Build settings
        /// </summary>
        public GridGenerationSettings BuildSettings { get; private set; }
        /// <summary>
        /// Gets the geometry
        /// </summary>
        public Func<Triangle[]> GetGeometryFunction { get; set; }

        /// <summary>
        /// Gets the total bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }
        /// <summary>
        /// Graph node list
        /// </summary>
        public GridNode[] Nodes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Grid()
        {

        }
        /// <summary>
        /// Resource dispose
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.BuildSettings);
            Helper.Dispose(this.GetGeometryFunction);
            Helper.Dispose(this.Nodes);
        }

        /// <summary>
        /// Gets node wich contains specified point
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>Returns the node wich contains the specified point if exists</returns>
        public GridNode FindNode(Vector3 point)
        {
            float minDistance = float.MaxValue;
            GridNode bestNode = null;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                if (this.Nodes[i].Contains(point, out float distance))
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestNode = this.Nodes[i];
                    }
                }
            }

            return bestNode;
        }

        /// <summary>
        /// Builds the graph
        /// </summary>
        /// <param name="sourceFunction">Geometry source function</param>
        /// <param name="settings">Settings</param>
        /// <returns>Returns the new Graph</returns>
        public void Build(Func<Triangle[]> sourceFunction, PathFinderSettings settings)
        {
            this.BuildSettings = settings as GridGenerationSettings;
            this.GetGeometryFunction = sourceFunction;

            var triangles = sourceFunction();

            this.BoundingBox = GeometryUtil.CreateBoundingBox(triangles);

            List<GridNode> result = new List<GridNode>();

            Dictionary<Vector2, GridCollisionInfo[]> dictionary = new Dictionary<Vector2, GridCollisionInfo[]>();

            float fxSize = (this.BoundingBox.Maximum.X - this.BoundingBox.Minimum.X) / this.BuildSettings.NodeSize;
            float fzSize = (this.BoundingBox.Maximum.Z - this.BoundingBox.Minimum.Z) / this.BuildSettings.NodeSize;

            int xSize = fxSize > (int)fxSize ? (int)fxSize + 1 : (int)fxSize;
            int zSize = fzSize > (int)fzSize ? (int)fzSize + 1 : (int)fzSize;

            for (float x = this.BoundingBox.Minimum.X; x < this.BoundingBox.Maximum.X; x += this.BuildSettings.NodeSize)
            {
                for (float z = this.BoundingBox.Minimum.Z; z < this.BoundingBox.Maximum.Z; z += this.BuildSettings.NodeSize)
                {
                    GridCollisionInfo[] info = null;

                    Ray ray = new Ray()
                    {
                        Position = new Vector3(x, this.BoundingBox.Maximum.Y + 0.01f, z),
                        Direction = Vector3.Down,
                    };

                    bool intersects = Intersection.IntersectAll(
                        ref ray, triangles, true,
                        out Vector3[] pickedPoints,
                        out Triangle[] pickedTriangles,
                        out float[] pickedDistances);

                    if (intersects)
                    {
                        info = new GridCollisionInfo[pickedPoints.Length];

                        for (int i = 0; i < pickedPoints.Length; i++)
                        {
                            info[i] = new GridCollisionInfo() { Point = pickedPoints[i], Triangle = pickedTriangles[i], Distance = pickedDistances[i] };
                        }
                    }
                    else
                    {
                        info = new GridCollisionInfo[] { };
                    }

                    dictionary.Add(new Vector2(x, z), info);
                }
            }

            int gridNodeCount = (xSize - 1) * (zSize - 1);

            GridCollisionInfo[][] collisionValues = new GridCollisionInfo[dictionary.Count][];
            dictionary.Values.CopyTo(collisionValues, 0);

            //Generate grid nodes
            for (int n = 0; n < gridNodeCount; n++)
            {
                int x = n / xSize;
                int z = n - (x * xSize);

                if (x == zSize - 1) continue;
                if (z == xSize - 1) continue;

                //Find node corners
                int i0 = ((x + 0) * zSize) + (z + 0);
                int i1 = ((x + 0) * zSize) + (z + 1);
                int i2 = ((x + 1) * zSize) + (z + 0);
                int i3 = ((x + 1) * zSize) + (z + 1);

                GridCollisionInfo[] coor0 = collisionValues[i0];
                GridCollisionInfo[] coor1 = collisionValues[i1];
                GridCollisionInfo[] coor2 = collisionValues[i2];
                GridCollisionInfo[] coor3 = collisionValues[i3];

                int min = Helper.Min(coor0.Length, coor1.Length, coor2.Length, coor3.Length);
                int max = Helper.Max(coor0.Length, coor1.Length, coor2.Length, coor3.Length);

                if (min == 0)
                {
                    //None
                }
                else if (max == 1 && min == 1)
                {
                    //Unique collision node
                    for (int i = 0; i < max; i++)
                    {
                        Vector3 va = (
                            coor0[i].Triangle.Normal +
                            coor1[i].Triangle.Normal +
                            coor2[i].Triangle.Normal +
                            coor3[i].Triangle.Normal) * 0.25f;

                        GridNode newNode = new GridNode(
                            coor0[i].Point,
                            coor1[i].Point,
                            coor2[i].Point,
                            coor3[i].Point,
                            Helper.Angle(Vector3.Up, va));

                        result.Add(newNode);
                    }
                }
                else
                {
                    //Process multiple point nodes
                    for (int i = 0; i < max; i++)
                    {
                        GridCollisionInfo c0 = i < coor0.Length ? coor0[i] : coor0[coor0.Length - 1];
                        GridCollisionInfo c1 = i < coor1.Length ? coor1[i] : coor1[coor1.Length - 1];
                        GridCollisionInfo c2 = i < coor2.Length ? coor2[i] : coor2[coor2.Length - 1];
                        GridCollisionInfo c3 = i < coor3.Length ? coor3[i] : coor3[coor3.Length - 1];

                        float fmin = Helper.Min(c0.Point.Y, c1.Point.Y, c2.Point.Y, c3.Point.Y);
                        float fmax = Helper.Max(c0.Point.Y, c1.Point.Y, c2.Point.Y, c3.Point.Y);
                        float diff = Math.Abs(fmax - fmin);

                        if (diff <= this.BuildSettings.NodeSize)
                        {
                            Vector3 va = (
                                c0.Triangle.Normal +
                                c1.Triangle.Normal +
                                c2.Triangle.Normal +
                                c3.Triangle.Normal) * 0.25f;

                            GridNode newNode = new GridNode(
                                c0.Point,
                                c1.Point,
                                c2.Point,
                                c3.Point,
                                Helper.Angle(Vector3.Up, va));

                            result.Add(newNode);
                        }
                    }
                }
            }

            //Fill connections
            for (int i = 0; i < result.Count; i++)
            {
                if (!result[i].FullConnected)
                {
                    for (int n = i + 1; n < result.Count; n++)
                    {
                        if (!result[n].FullConnected)
                        {
                            result[i].TryConnect(result[n]);
                        }
                    }
                }
            }

            this.Nodes = result.ToArray();
        }
        /// <summary>
        /// Sets the geometry source function
        /// </summary>
        /// <param name="sourceFunction">Function</param>
        public void SetGeometrySourceFunction(Func<Triangle[]> sourceFunction)
        {
            this.GetGeometryFunction = sourceFunction;
        }

        /// <summary>
        /// Gets the node collection of the grid
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <returns>Returns the node collection of the grid</returns>
        public IGraphNode[] GetNodes(AgentType agent)
        {
            return Array.ConvertAll(this.Nodes, (n) => { return (IGraphNode)n; });
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent type</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public Vector3[] FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            return AStarQuery.FindPath(this, from, to);
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
            nearest = null;

            for (int i = 0; i < this.Nodes.Length; i++)
            {
                float distance;
                if (this.Nodes[i].Contains(position, out distance))
                {
                    nearest = this.Nodes[i].Center;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the graph at specified position
        /// </summary>
        /// <param name="position">Position</param>
        public void UpdateAt(Vector3 position)
        {

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

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation</returns>
        public override string ToString()
        {
            return string.Format("Nodes {0}; Side {1:0.00};", this.Nodes.Length, this.BuildSettings.NodeSize);
        }
    }
}
