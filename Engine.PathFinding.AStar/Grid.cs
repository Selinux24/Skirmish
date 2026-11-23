using Engine.Common;
using Engine.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class Grid(GridGenerationSettings settings, PathFinderInput input) : IGraph
    {
        /// <summary>
        /// Node list
        /// </summary>
        private readonly List<GridNode> nodes = [];

        /// <inheritdoc/>
        public bool Initialized { get; set; }
        /// <inheritdoc/>
        public bool EnableDebug { get; set; }
        /// <summary>
        /// Geometry input
        /// </summary>
        public PathFinderInput Input { get; private set; } = input ?? throw new ArgumentNullException(nameof(input), "Must specify the path finder input helper.");
        /// <summary>
        /// Build settings
        /// </summary>
        public GridGenerationSettings Settings { get; private set; } = settings ?? throw new ArgumentNullException(nameof(settings), "Must specify the grid generation settings.");
        /// <summary>
        /// Graph node list
        /// </summary>
        public GridNode[] Nodes
        {
            get
            {
                return [.. nodes];
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
        public static Grid CreateGrid(PathFinderSettings settings, PathFinderInput input, Triangle[] triangles, Action<float> progressCallback)
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

                    bool intersects = RayPickingHelper.PickAllFromlist(triangles, ref ray, out var picks);
                    if (intersects)
                    {
                        info = new GridCollisionInfo[picks.Length];

                        for (int i = 0; i < picks.Length; i++)
                        {
                            var pick = picks[i];

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
                        info = [];
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
        private static void FillConnections(List<GridNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var nodeA = nodes[i];
                if (nodeA.FullConnected)
                {
                    continue;
                }

                for (int n = i + 1; n < nodes.Count; n++)
                {
                    var nodeB = nodes[n];
                    if (nodeB.FullConnected)
                    {
                        continue;
                    }

                    nodeA.TryConnect(nodeB);
                }
            }
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
        public void SetNodes(GridNode[] nodeList)
        {
            ArgumentNullException.ThrowIfNull(nodeList);

            nodes.Clear();

            if (nodeList.Length == 0)
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
        public Vector3? FindRandomPoint(AgentType agent, ref Vector3 position, float radius)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public IGraphNode[] GetNodes(AgentType agent)
        {
            return [.. nodes.Cast<IGraphNode>()];
        }
        /// <inheritdoc/>
        public T[] GetNodes<T>(AgentType agent) where T : IGraphNode
        {
            return [.. nodes.Cast<T>()];
        }
        /// <inheritdoc/>
        public IGraphNode FindNode(AgentType agent, ref Vector3 point)
        {
            float minDistance = float.MaxValue;
            IGraphNode bestNode = null;

            foreach (var node in nodes)
            {
                if (!node.Contains(ref point))
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
        public T FindNode<T>(AgentType agent, ref Vector3 point) where T : IGraphNode
        {
            var node = FindNode(agent, ref point);
            if (node == null) return default;
            if (node is T gNode) return gNode;
            return default;
        }
        /// <inheritdoc/>
        public Vector3[] FindPath(AgentType agent, ref Vector3 from, ref Vector3 to)
        {
            return AStarQuery.FindPath(agent, this, from, to);
        }
        /// <inheritdoc/>
        public async Task<Vector3[]> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            return await Task.Run(() => FindPath(agent, ref from, ref to));
        }
        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, ref Vector3 position, float distanceThreshold)
        {
            foreach (var n in nodes)
            {
                if (n.Contains(ref position)) return true;
            }

            return false;
        }
        /// <inheritdoc/>
        public bool IsWalkable(AgentType agent, ref Vector3 position, float distanceThreshold, out Vector3? nearest)
        {
            bool contains = false;
            nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var node in nodes)
            {
                if (!node.Contains(ref position))
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
        public void CreateAt(ref Vector3 position, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void CreateAt(ref BoundingBox bbox, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void CreateAt(Vector3[] positions, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void UpdateAt(ref Vector3 position, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void UpdateAt(ref BoundingBox bbox, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void UpdateAt(Vector3[] positions, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void RemoveAt(ref Vector3 position, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void RemoveAt(ref BoundingBox bbox, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void RemoveAt(Vector3[] positions, Action<GraphUpdateStates> callback = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int AddObstacle(ref BoundingCylinder cylinder)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public int AddObstacle(ref BoundingBox bbox)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public int AddObstacle(ref OrientedBoundingBox obb)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void RemoveObstacle(int obstacleId)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void UpdateObstacles(Action<GraphUpdateStates> callback = null)
        {
            //Updates the internal state
        }

        /// <inheritdoc/>
        public IGraphDebug GetDebugInfo(AgentType agent)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Nodes: {nodes.Count}; Size: {Settings.NodeSize:0.00};";
        }
    }
}
