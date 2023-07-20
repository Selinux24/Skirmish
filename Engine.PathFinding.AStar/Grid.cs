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
                return nodes.ToArray();
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
        /// Adds a new node
        /// </summary>
        /// <param name="node">Node</param>
        public void Add(GridNode node)
        {
            nodes.Add(node);
        }
        /// <summary>
        /// Adds a list of nodes
        /// </summary>
        /// <param name="nodes">Node list</param>
        public void AddRange(IEnumerable<GridNode> nodes)
        {
            if (nodes?.Any() != true)
            {
                return;
            }

            this.nodes.AddRange(nodes);
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

            foreach (var node in nodes)
            {
                if (node.Contains(point, out float distance) && distance < minDistance)
                {
                    minDistance = distance;
                    bestNode = node;
                }
            }

            return bestNode;
        }

        /// <inheritdoc/>
        public IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            return nodes.Cast<IGraphNode>().ToArray();
        }
        /// <inheritdoc/>
        public IEnumerable<Vector3> FindPath(AgentType agent, Vector3 from, Vector3 to)
        {
            return AStarQuery.FindPath(this, from, to);
        }
        /// <inheritdoc/>
        public async Task<IEnumerable<Vector3>> FindPathAsync(AgentType agent, Vector3 from, Vector3 to)
        {
            IEnumerable<Vector3> result = null;

            await Task.Run(() =>
            {
                result = AStarQuery.FindPath(this, from, to);
            });

            return result ?? Enumerable.Empty<Vector3>();
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
                if (node.Contains(position, out float distance))
                {
                    contains = true;

                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = node.Center;
                    }
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
        public void Update(GameTime gameTime)
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
            return $"Nodes {nodes.Count}; Side {Settings.NodeSize:0.00};";
        }
    }
}
