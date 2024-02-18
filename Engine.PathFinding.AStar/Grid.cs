using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid
    /// </summary>
    public class Grid : IGraph
    {
        /// <summary>
        /// Node list
        /// </summary>
        private readonly List<GridNode> nodes = new();

        /// <inheritdoc/>
        public event EventHandler Updating;
        /// <inheritdoc/>
        public event EventHandler Updated;

        /// <inheritdoc/>
        public bool Initialized { get; set; }
        /// <summary>
        /// Geometry input
        /// </summary>
        public PathFinderInput Input { get; private set; }
        /// <summary>
        /// Build settings
        /// </summary>
        public GridGenerationSettings Settings { get; private set; }
        /// <summary>
        /// Graph node list
        /// </summary>
        public IEnumerable<GridNode> Nodes
        {
            get
            {
                return nodes.AsReadOnly();
            }
        }

        /// <summary>
        /// Creates a new grid
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="input">Input</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="progressCallback">Optional progress callback</param>
        /// <returns>Returns the new grid</returns>
        public static Grid CreateGrid(PathFinderSettings settings, PathFinderInput input, IEnumerable<Triangle> triangles, Action<float> progressCallback)
        {
            var grid = new Grid(settings as GridGenerationSettings, input);

            var bbox = GeometryUtil.CreateBoundingBox(triangles);

            var dictionary = new Dictionary<Vector2, GridCollisionInfo[]>();

            float fxSize = (bbox.Maximum.X - bbox.Minimum.X) / grid.Settings.NodeSize;
            float fzSize = (bbox.Maximum.Z - bbox.Minimum.Z) / grid.Settings.NodeSize;

            int xSize = fxSize > (int)fxSize ? (int)fxSize + 1 : (int)fxSize;
            int zSize = fzSize > (int)fzSize ? (int)fzSize + 1 : (int)fzSize;

            float total = bbox.Size.X * bbox.Size.X / grid.Settings.NodeSize;
            int curr = 0;

            for (float x = bbox.Minimum.X; x < bbox.Maximum.X; x += grid.Settings.NodeSize)
            {
                for (float z = bbox.Minimum.Z; z < bbox.Maximum.Z; z += grid.Settings.NodeSize)
                {
                    GridCollisionInfo[] info;

                    var ray = new PickingRay(new Vector3(x, bbox.Maximum.Y + 0.01f, z), Vector3.Down);

                    bool intersects = RayPickingHelper.PickAllFromlist(triangles, ray, out var picks);
                    if (intersects)
                    {
                        info = new GridCollisionInfo[picks.Count()];

                        for (int i = 0; i < picks.Count(); i++)
                        {
                            var pick = picks.ElementAt(i);

                            info[i] = new GridCollisionInfo()
                            {
                                Point = pick.Position,
                                Triangle = pick.Primitive,
                                Distance = pick.Distance,
                            };
                        }
                    }
                    else
                    {
                        info = Array.Empty<GridCollisionInfo>();
                    }

                    dictionary.Add(new Vector2(x, z), info);

                    progressCallback?.Invoke(++curr / total);
                }
            }

            int gridNodeCount = (xSize - 1) * (zSize - 1);

            GridCollisionInfo[][] collisionValues = new GridCollisionInfo[dictionary.Count][];
            dictionary.Values.CopyTo(collisionValues, 0);

            //Generate grid nodes
            var nodeList = GridNode.GenerateGridNodes(gridNodeCount, xSize, zSize, grid.Settings.NodeSize, collisionValues);
            grid.SetNodes(nodeList);
            grid.Initialized = true;

            return grid;
        }
        /// <summary>
        /// Fill node connections
        /// </summary>
        /// <param name="nodes">Grid nodes</param>
        private static void FillConnections(IEnumerable<GridNode> nodes)
        {
            for (int i = 0; i < nodes.Count(); i++)
            {
                var nodeA = nodes.ElementAt(i);
                if (nodeA.FullConnected)
                {
                    continue;
                }

                for (int n = i + 1; n < nodes.Count(); n++)
                {
                    var nodeB = nodes.ElementAt(n);
                    if (nodeB.FullConnected)
                    {
                        continue;
                    }

                    nodeA.TryConnect(nodeB);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Grid(GridGenerationSettings settings, PathFinderInput input)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings), "Must specify the grid generation settings.");
            Input = input ?? throw new ArgumentNullException(nameof(input), "Must specify the path finder input helper.");
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Grid()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Input = null;
                Settings = null;

                nodes.Clear();
            }
        }

        /// <summary>
        /// Adds a list of nodes
        /// </summary>
        /// <param name="nodeList">Node list</param>
        public void SetNodes(IEnumerable<GridNode> nodeList)
        {
            nodes.Clear();

            if (nodeList?.Any() != true)
            {
                return;
            }

            nodes.AddRange(nodeList);

            FillConnections(nodes);
        }

        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public Vector3? FindRandomPoint(AgentType agent, Vector3 position, float radius)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            return nodes.Cast<IGraphNode>().ToArray();
        }
        /// <inheritdoc/>
        public IGraphNode FindNode(AgentType agent, Vector3 point)
        {
            float minDistance = float.MaxValue;
            IGraphNode bestNode = null;

            foreach (var node in nodes)
            {
                if (!node.Contains(point))
                {
                    continue;
                }

                float distance = Vector3.DistanceSquared(point, node.Center);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestNode = node;
                }
            }

            return bestNode;
        }
        /// <inheritdoc/>
        public IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            return AStarQuery.FindPath(agent, this, from, to);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            return await Task.Run(() => FindPath(agent, from, to));
        }
        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, Vector3 position, float distanceThreshold)
        {
            return nodes.Exists(n => n.Contains(position));
        }
        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, Vector3 position, float distanceThreshold, out Vector3? nearest)
        {
            bool contains = false;
            nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var node in nodes)
            {
                if (!node.Contains(position))
                {
                    continue;
                }

                contains = true;

                float distance = Vector3.DistanceSquared(position, node.Center);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = node.Center;
                }
            }

            return contains;
        }

        /// <inheritdoc/>
        public bool CreateAt(Vector3 position)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool CreateAt(BoundingBox bbox)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool CreateAt(IEnumerable<Vector3> positions)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool UpdateAt(Vector3 position)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool UpdateAt(BoundingBox bbox)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool UpdateAt(IEnumerable<Vector3> positions)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool RemoveAt(Vector3 position)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool RemoveAt(BoundingBox bbox)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public bool RemoveAt(IEnumerable<Vector3> positions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int AddObstacle(BoundingCylinder cylinder)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public int AddObstacle(BoundingBox bbox)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public int AddObstacle(OrientedBoundingBox obb)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void RemoveObstacle(int obstacleId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IGraphDebug GetDebugInfo(AgentType agent)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Update(IGameTime gameTime)
        {
            //Updates the internal state
        }
        /// <summary>
        /// Fires the updated event
        /// </summary>
        protected void FireUpdated()
        {
            Updated?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Fires the updating event
        /// </summary>
        protected void FireUpdating()
        {
            Updating?.Invoke(this, new EventArgs());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Nodes: {nodes.Count}; Size: {Settings.NodeSize:0.00};";
        }
    }
}
