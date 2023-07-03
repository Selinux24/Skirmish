using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding
{
    using Engine.Common;

    /// <summary>
    /// Walkable scene
    /// </summary>
    public class WalkableScene : Scene
    {
        /// <summary>
        /// Gets the current scene geometry for navigation
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="obj">Scene object</param>
        /// <returns>Returns the primitive list</returns>
        private static IEnumerable<T> GetGeometryForNavigationGraph<T>(IDrawable obj) where T : IRayIntersectable
        {
            if (obj is IComposed composed)
            {
                return composed
                    .GetComponents<IRayPickable<T>>()
                    .SelectMany(GetGeometryForNavigationGraph);
            }

            if (obj is IRayPickable<T> pickable)
            {
                return GetGeometryForNavigationGraph(pickable);
            }

            return Enumerable.Empty<T>();
        }
        /// <summary>
        /// Gets the current scene geometry for navigation
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="pickable">Pickable object</param>
        /// <returns>Returns the primitive list</returns>
        private static IEnumerable<T> GetGeometryForNavigationGraph<T>(IRayPickable<T> pickable) where T : IRayIntersectable
        {
            if (pickable is ITransformable3D transformable)
            {
                transformable.Manipulator.UpdateInternals(true);
            }

            return pickable.GetGeometry(GeometryTypes.PathFinding);
        }
        /// <summary>
        /// Gets the new agent position when target position is walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="position">Test position</param>
        /// <param name="adjustHeight">Set whether use the agent height or not when resolving the final position. Usually true when the camera sets the agent's position</param>
        /// <returns>Returns the new agent position</returns>
        private static Vector3 GetPositionWalkable(AgentType agent, Vector3 prevPosition, Vector3 newPosition, Vector3 position, bool adjustHeight)
        {
            var finalPosition = position;

            if (adjustHeight)
            {
                finalPosition.Y += agent.Height;
            }

            var moveP = newPosition - prevPosition;
            var moveV = finalPosition - prevPosition;
            if (moveV.LengthSquared() > moveP.LengthSquared())
            {
                finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
            }

            return finalPosition;
        }

        /// <summary>
        /// Path finder
        /// </summary>
        protected PathFinderDescription PathFinderDescription { get; set; }
        /// <summary>
        /// Graph used for pathfinding
        /// </summary>
        protected IGraph NavigationGraph { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game instance</param>
        public WalkableScene(Game game) : base(game)
        {

        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                NavigationGraph?.Dispose();
                NavigationGraph = null;
            }
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            NavigationGraph?.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Point position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Vector2 position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Y, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(Vector3 position, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            return GetTopDownRay(position.X, position.Z, pickingParams);
        }
        /// <summary>
        /// Gets vertical ray from scene's top and down vector with x and z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="pickingParams">Picking parameters</param>
        /// <returns>Returns vertical ray from scene's top and down vector with x and z coordinates</returns>
        public PickingRay GetTopDownRay(float x, float z, PickingHullTypes pickingParams = PickingHullTypes.Default)
        {
            var bbox = GetBoundingBox(SceneObjectUsages.Ground);

            float maxY = bbox.Maximum.Y + 1.0f;

            return new PickingRay(new Vector3(x, maxY, z), Vector3.Down, pickingParams);
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition<T>(float x, float z, out PickingResult<T> result) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            if (this.PickNearest<T>(ray, SceneObjectUsages.Ground, out var res))
            {
                result = res.PickingResult;

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue
            };

            return false;
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition<T>(float x, float z, out PickingResult<T> result) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            if (this.PickFirst<T>(ray, SceneObjectUsages.Ground, out var res))
            {
                result = res.PickingResult;

                return true;
            }

            result = new PickingResult<T>
            {
                Distance = float.MaxValue
            };

            return false;
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="results">Picking results</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition<T>(float x, float z, out IEnumerable<PickingResult<T>> results) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(x, z);

            if (this.PickAll<T>(ray, SceneObjectUsages.Ground, out var res))
            {
                results = res.SelectMany(r => r.PickingResults);

                return true;
            }

            results = Enumerable.Empty<PickingResult<T>>();

            return false;
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="result">Picking result</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition<T>(Vector3 from, out PickingResult<T> result) where T : IRayIntersectable
        {
            var ray = GetTopDownRay(from.X, from.Z);

            bool picked = this.PickAll<T>(ray, SceneObjectUsages.Ground, out var pResults);
            if (picked)
            {
                result = pResults
                    .SelectMany(r => r.PickingResults)
                    .OrderBy(r => Vector3.DistanceSquared(from, r.Position))
                    .First();

                return true;
            }

            result = new PickingResult<T>()
            {
                Distance = float.MaxValue,
            };

            return false;
        }

        /// <summary>
        /// Set ground geometry
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        public void SetGround(IDrawable obj)
        {
            BoundingBox = null;

            obj.Usage = SceneObjectUsages.Ground;
        }
        /// <summary>
        /// Attach geometry to ground
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="x">X position</param>
        /// <param name="z">Z position</param>
        /// <param name="transform">Transform</param>
        public void AttachToGround(IDrawable obj)
        {
            BoundingBox = null;

            obj.Usage = SceneObjectUsages.Object;
        }

        /// <summary>
        /// Updates the navigation graph
        /// </summary>
        public virtual async Task UpdateNavigationGraph()
        {
            if (PathFinderDescription == null)
            {
                SetNavigationGraph(null);

                return;
            }

            var graph = await PathFinderDescription.Build();

            SetNavigationGraph(graph);

            NavigationGraphUpdated();
        }
        /// <summary>
        /// Sets a navigation graph
        /// </summary>
        /// <param name="graph">Navigation graph</param>
        public virtual void SetNavigationGraph(IGraph graph)
        {
            NavigationGraphUpdating();

            if (NavigationGraph != null)
            {
                NavigationGraph.Updating -= GraphUpdating;
                NavigationGraph.Updated -= GraphUpdated;

                NavigationGraph.Dispose();
                NavigationGraph = null;
            }

            if (graph != null)
            {
                NavigationGraph = graph;
                NavigationGraph.Updating += GraphUpdating;
                NavigationGraph.Updated += GraphUpdated;
            }

            NavigationGraphUpdated();
        }

        /// <summary>
        /// Graph updating event
        /// </summary>
        /// <param name="sender">Sender graph</param>
        /// <param name="e">Event args</param>
        private void GraphUpdating(object sender, EventArgs e)
        {
            Logger.WriteInformation(this, $"{nameof(GraphUpdating)} - Triggered by {sender}");

            Logger.WriteInformation(this, $"{nameof(GraphUpdating)} - {nameof(NavigationGraphUpdating)} Call");
            NavigationGraphUpdating();
            Logger.WriteInformation(this, $"{nameof(GraphUpdating)} - {nameof(NavigationGraphUpdating)} End");
        }
        /// <summary>
        /// Graph updated event
        /// </summary>
        /// <param name="sender">Sender graph</param>
        /// <param name="e">Event args</param>
        private void GraphUpdated(object sender, EventArgs e)
        {
            Logger.WriteInformation(this, $"{nameof(GraphUpdated)} - Triggered by {sender}");

            Logger.WriteInformation(this, $"{nameof(GraphUpdated)} - {nameof(NavigationGraphUpdated)} Call");
            NavigationGraphUpdated();
            Logger.WriteInformation(this, $"{nameof(GraphUpdated)} - {nameof(NavigationGraphUpdated)} End");
        }
        /// <summary>
        /// Fires when graph is updating
        /// </summary>
        public virtual void NavigationGraphUpdating()
        {

        }
        /// <summary>
        /// Fires when graph is updated
        /// </summary>
        public virtual void NavigationGraphUpdated()
        {

        }

        /// <summary>
        /// Gets the objects triangle list for navigation graph construction
        /// </summary>
        /// <returns>Returns a triangle list</returns>
        public virtual IEnumerable<Triangle> GetTrianglesForNavigationGraph()
        {
            var navComponents = Components.Get<IDrawable>(c => !c.HasOwner && c.Visible);

            var allTris = navComponents.SelectMany(GetGeometryForNavigationGraph<Triangle>).ToArray();

            var navTris = allTris.Distinct();

            var bounds = PathFinderDescription.Settings.Bounds;
            if (bounds.HasValue)
            {
                navTris = navTris.Where(t =>
                {
                    return Intersection.BoxContainsTriangle(bounds.Value, t) != ContainmentType.Disjoint;
                });
            }

            return navTris.ToArray();
        }

        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <param name="useGround">Find nearest real ground position for "from" and "to" parameters, and all path results</param>
        /// <returns>Return path if exists</returns>
        public virtual PathFindingPath FindPath(AgentType agent, Vector3 from, Vector3 to, bool useGround = false)
        {
            if (NavigationGraph?.Initialized != true)
            {
                return null;
            }

            var (From, To) = LocatePathEndpoints(from, to, useGround);

            var path = NavigationGraph.FindPath(agent, From, To);

            Logger.WriteTrace(this, $"FindPath path result: {path.Count()} nodes.");

            if (path.Count() <= 1)
            {
                return null;
            }

            return ComputeGroundPositions(path, useGround);
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <param name="useGround">Find nearest real ground position for "from" and "to" parameters, and all path results</param>
        /// <returns>Return path if exists</returns>
        public virtual async Task<PathFindingPath> FindPathAsync(AgentType agent, Vector3 from, Vector3 to, bool useGround = false)
        {
            if (NavigationGraph?.Initialized != true)
            {
                return null;
            }

            var (From, To) = LocatePathEndpoints(from, to, useGround);

            var path = await NavigationGraph.FindPathAsync(agent, From, To);

            Logger.WriteTrace(this, $"FindPathAsync path result: {path.Count()} nodes.");

            if (path.Count() <= 1)
            {
                return null;
            }

            return ComputeGroundPositions(path, useGround);
        }
        /// <summary>
        /// Locates the navigation end point positions
        /// </summary>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <param name="useGround">Find nearest real ground position for "from" and "to" parameters, and all path results</param>
        /// <returns>Returns the navigation end point positions</returns>
        private (Vector3 From, Vector3 To) LocatePathEndpoints(Vector3 from, Vector3 to, bool useGround)
        {
            Vector3 groundFrom = from;
            Vector3 groundTo = to;

            if (!useGround)
            {
                return (groundFrom, groundTo);
            }

            Logger.WriteTrace(this, $"FindPathAsync looking for nearest ground end-point positions {from} -> {to}.");

            if (FindNearestGroundPosition(from, out PickingResult<Triangle> rFrom))
            {
                groundFrom = rFrom.Position;
            }
            if (FindNearestGroundPosition(to, out PickingResult<Triangle> rTo))
            {
                groundTo = rTo.Position;
            }

            Logger.WriteTrace(this, $"FindPathAsync Found nearest ground end-point positions {groundFrom} -> {groundTo}.");

            return (groundFrom, groundTo);
        }
        /// <summary>
        /// Generates the path
        /// </summary>
        /// <param name="path">Path positions</param>
        /// <param name="useGround">Find nearest real ground position for "from" and "to" parameters, and all path results</param>
        /// <returns>Returns the path-finding path</returns>
        private PathFindingPath ComputeGroundPositions(IEnumerable<Vector3> path, bool useGround)
        {
            List<Vector3> positions = new(path);
            List<Vector3> normals = new(Helper.CreateArray(path.Count(), Vector3.Up));

            if (useGround)
            {
                Logger.WriteTrace(this, "FindPath compute ground positions.");

                for (int i = 0; i < positions.Count; i++)
                {
                    if (FindNearestGroundPosition<Triangle>(positions[i], out var r))
                    {
                        positions[i] = r.Position;
                        normals[i] = r.Primitive.Normal;
                    }
                }

                Logger.WriteTrace(this, "FindPath ground positions computed.");
            }

            return new PathFindingPath(positions, normals);
        }

        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="position">Position</param>
        /// <param name="distanceThreshold">Distance threshold</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public virtual bool IsWalkable(AgentType agent, Vector3 position, float distanceThreshold, out Vector3? nearest)
        {
            if (NavigationGraph == null)
            {
                nearest = position;

                return true;
            }

            return NavigationGraph.IsWalkable(agent, position, distanceThreshold, out nearest);
        }
        /// <summary>
        /// Gets final position for agents walking over the ground if exists
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="adjustHeight">Set whether use the agent height or not when resolving the final position. Usually true when the camera sets the agent's position</param>
        /// <param name="finalPosition">Returns the final position if exists</param>
        /// <returns>Returns true if final position found</returns>
        public virtual bool Walk(AgentType agent, Vector3 prevPosition, Vector3 newPosition, bool adjustHeight, out Vector3 finalPosition)
        {
            finalPosition = prevPosition;

            if (prevPosition == newPosition)
            {
                return false;
            }

            bool found = FindAllGroundPosition<Triangle>(newPosition.X, newPosition.Z, out var results);
            if (!found)
            {
                return false;
            }

            Vector3 newFeetPosition = newPosition;

            if (adjustHeight)
            {
                float offset = agent.Height;
                newFeetPosition.Y -= offset;

                results = results
                    .Where(r => Vector3.Distance(r.Position, newFeetPosition) < offset)
                    .OrderBy(r => r.Distance).ToArray();
            }

            var positions = results.Select(r => r.Position).ToArray();
            float threshold = Vector3.Distance(prevPosition, newPosition);

            foreach (var position in positions)
            {
                if (IsWalkable(agent, position, threshold, out var nearest))
                {
                    finalPosition = GetPositionWalkable(agent, prevPosition, newPosition, position, adjustHeight);

                    return true;
                }
                else if (nearest.HasValue)
                {
                    //Not walkable but nearest position found
                    finalPosition = GetPositionNonWalkable(agent, prevPosition, newPosition, nearest.Value, adjustHeight);

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Gets the new agent position when target position is not walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="position">Test position</param>
        /// <param name="adjustHeight">Set whether use the agent height or not when resolving the final position. Usually true when the camera sets the agent's position</param>
        /// <returns>Returns the new agent position</returns>
        private Vector3 GetPositionNonWalkable(AgentType agent, Vector3 prevPosition, Vector3 newPosition, Vector3 position, bool adjustHeight)
        {
            //Find nearest ground position
            Vector3 finalPosition;
            if (FindNearestGroundPosition(position, out PickingResult<Triangle> nearestResult))
            {
                //Use nearest ground position found
                finalPosition = nearestResult.Position;
            }
            else
            {
                //Use nearest position provided by path finding graph
                finalPosition = position;
            }

            if (adjustHeight)
            {
                //Adjust height
                finalPosition.Y += agent.Height;
            }

            var moveP = newPosition - prevPosition;
            var moveV = finalPosition - prevPosition;
            if (moveV.LengthSquared() > moveP.LengthSquared())
            {
                finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
            }

            return finalPosition;
        }

        /// <summary>
        /// Adds cylinder obstacle
        /// </summary>
        /// <param name="cylinder">Cylinder</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(BoundingCylinder cylinder)
        {
            return NavigationGraph?.AddObstacle(cylinder) ?? -1;
        }
        /// <summary>
        /// Adds AABB obstacle
        /// </summary>
        /// <param name="bbox">AABB</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(BoundingBox bbox)
        {
            return NavigationGraph?.AddObstacle(bbox) ?? -1;
        }
        /// <summary>
        /// Adds OBB obstacle
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <returns>Returns the obstacle Id</returns>
        public virtual int AddObstacle(OrientedBoundingBox obb)
        {
            return NavigationGraph?.AddObstacle(obb) ?? -1;
        }
        /// <summary>
        /// Removes obstable by id
        /// </summary>
        /// <param name="obstacle">Obstacle id</param>
        public virtual void RemoveObstacle(int obstacle)
        {
            NavigationGraph?.RemoveObstacle(obstacle);
        }

        /// <summary>
        /// Updates the graph at position
        /// </summary>
        /// <param name="position">Position</param>
        public virtual async void UpdateGraph(Vector3 position)
        {
            await PathFinderDescription?.Input.Refresh();

            NavigationGraph?.UpdateAt(position);
        }
        /// <summary>
        /// Updates the graph at positions in the specified list
        /// </summary>
        /// <param name="positions">Positions list</param>
        public virtual async void UpdateGraph(IEnumerable<Vector3> positions)
        {
            if (positions?.Any() != true)
            {
                return;
            }

            await PathFinderDescription?.Input.Refresh();

            NavigationGraph?.UpdateAt(positions);
        }

        /// <summary>
        /// Gets a random point over the ground
        /// </summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="offset">Search offset</param>
        /// <returns>Returns a position over the ground</returns>
        public Vector3 GetRandomPoint(Random rnd, Vector3 offset)
        {
            var bbox = GetBoundingBox(SceneObjectUsages.Ground);

            return GetRandomPoint(rnd, offset, bbox);
        }
        /// <summary>
        /// Gets a random point over the ground
        /// </summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="offset">Search offset</param>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns a position over the ground</returns>
        public Vector3 GetRandomPoint(Random rnd, Vector3 offset, BoundingBox bbox)
        {
            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                if (FindTopGroundPosition(v.X, v.Z, out PickingResult<Triangle> r))
                {
                    return r.Position + offset;
                }
            }
        }
        /// <summary>
        /// Gets a random point over the ground
        /// </summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="offset">Search offset</param>
        /// <param name="bsph">Bounding sphere</param>
        /// <returns>Returns a position over the ground</returns>
        public Vector3 GetRandomPoint(Random rnd, Vector3 offset, BoundingSphere bsph)
        {
            while (true)
            {
                float dist = rnd.NextFloat(0, bsph.Radius);

                Vector3 dir = new(rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1), rnd.NextFloat(-1, 1));

                Vector3 v = bsph.Center + (dist * Vector3.Normalize(dir));

                if (FindTopGroundPosition(v.X, v.Z, out PickingResult<Triangle> r))
                {
                    return r.Position + offset;
                }
            }
        }

        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the path finder grid nodes</returns>
        public virtual IEnumerable<IGraphNode> GetNodes(AgentType agent)
        {
            return NavigationGraph?.GetNodes(agent) ?? Enumerable.Empty<IGraphNode>();
        }
    }
}
