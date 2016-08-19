using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.PathFinding;

    /// <summary>
    /// Ground class
    /// </summary>
    /// <remarks>Used for picking tests and navigation over surfaces</remarks>
    public abstract class Ground : Drawable, IGround
    {
        /// <summary>
        /// Terrain attached objects
        /// </summary>
        protected readonly List<GroundAttachedObject> GroundObjects = new List<GroundAttachedObject>();
        /// <summary>
        /// Quadtree
        /// </summary>
        protected QuadTree pickingQuadtree = null;
        /// <summary>
        /// Graph used for pathfinding
        /// </summary>
        protected IGraph navigationGraph = null;

        /// <summary>
        /// Instance description used for creation
        /// </summary>
        public readonly GroundDescription Description = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public Ground(Game game, GroundDescription description)
            : base(game)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out Vector3 position)
        {
            Triangle tri;
            return FindTopGroundPosition(x, z, out position, out tri);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out Vector3 position, out Triangle triangle)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            return this.PickNearest(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out Vector3 position)
        {
            Triangle tri;
            return FindFirstGroundPosition(x, z, out position, out tri);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out Vector3 position, out Triangle triangle)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            return this.PickFirst(ref ray, out position, out triangle);
        }
        /// <summary>
        /// Gets ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out Vector3[] positions)
        {
            Triangle[] triangles;
            return FindAllGroundPosition(x, z, out positions, out triangles);
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out Vector3[] positions, out Triangle[] triangles)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.01f, z),
                Direction = Vector3.Down,
            };

            return this.PickAll(ref ray, out positions, out triangles);
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition(Vector3 from, out Vector3 position)
        {
            Triangle tri;
            return FindNearestGroundPosition(from, out position, out tri);
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition(Vector3 from, out Vector3 position, out Triangle triangle)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(from.X, bbox.Maximum.Y + 0.01f, from.Z),
                Direction = Vector3.Down,
            };

            Vector3[] positions;
            Triangle[] tris;
            if (this.PickAll(ref ray, out positions, out tris))
            {
                int index = -1;
                float distance = float.MaxValue;
                for (int i = 0; i < positions.Length; i++)
                {
                    float d = Vector3.DistanceSquared(from, positions[i]);
                    if (d <= distance)
                    {
                        index = i;
                        distance = d;
                    }
                }

                position = positions[index];
                triangle = tris[index];

                return true;
            }
            else
            {
                position = Vector3.Zero;
                triangle = new Triangle();

                return false;
            }
        }
        /// <summary>
        /// Attach objects to terrain
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachObject(GroundAttachedObject model, bool updateInternals = true)
        {
            this.GroundObjects.Add(model);

            if (updateInternals)
            {
                this.UpdateInternals();
            }
        }

        /// <summary>
        /// Updates internal objects
        /// </summary>
        public abstract void UpdateInternals();
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public abstract bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle);
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public abstract bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle);
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <returns>Returns true if picked positions found</returns>
        public abstract bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles);
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public abstract BoundingSphere GetBoundingSphere();
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public abstract BoundingBox GetBoundingBox();
        
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public virtual PathFindingPath FindPath(Agent agent, Vector3 from, Vector3 to)
        {
            var path = this.navigationGraph.FindPath(agent, from, to);
            if (path != null)
            {
                for (int i = 0; i < path.ReturnPath.Count; i++)
                {
                    Vector3 position;
                    if (FindNearestGroundPosition(path.ReturnPath[i], out position))
                    {
                        path.ReturnPath[i] = position;
                    }
                }
            }

            return path;
        }
        /// <summary>
        /// Gets wether the specified position is walkable
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="position">Position</param>
        /// <param name="nearest">Gets the nearest walkable position</param>
        /// <returns>Returns true if the specified position is walkable</returns>
        public virtual bool IsWalkable(Agent agent, Vector3 position, out Vector3? nearest)
        {
            return this.navigationGraph.IsWalkable(agent, position, out nearest);
        }
        /// <summary>
        /// Gets final position for agents walking over the ground if exists
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <param name="prevPosition">Previous position</param>
        /// <param name="newPosition">New position</param>
        /// <param name="finalPosition">Returns the final position if exists</param>
        /// <returns>Returns true if final position found</returns>
        public virtual bool Walk(Agent agent, Vector3 prevPosition, Vector3 newPosition, out Vector3 finalPosition)
        {
            finalPosition = Vector3.Zero;

            Vector3 walkerPos;
            if (this.FindNearestGroundPosition(newPosition, out walkerPos))
            {
                Vector3? nearest;
                if (this.IsWalkable(agent, walkerPos, out nearest))
                {
                    finalPosition = walkerPos;
                    finalPosition.Y += agent.Height;

                    return true;
                }
                else
                {
                    if (nearest.HasValue)
                    {
                        var p = nearest.Value;
                        p.Y = prevPosition.Y;

                        if (this.FindNearestGroundPosition(p, out walkerPos))
                        {
                            finalPosition = walkerPos;
                            finalPosition.Y += agent.Height;

                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
