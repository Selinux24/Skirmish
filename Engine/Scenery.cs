using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.PathFinding;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Scenery : Ground
    {
        /// <summary>
        /// Geometry
        /// </summary>
        private Model ground = null;
        /// <summary>
        /// Vegetation
        /// </summary>
        private Billboard[] vegetation = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Geometry content</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="description">Terrain description</param>
        public Scenery(Game game, ModelContent content, string contentFolder, GroundDescription description)
            : base(game, description)
        {
            this.DeferredEnabled = this.Description.DeferredEnabled;

            this.ground = new Model(game, content);
            this.ground.Opaque = this.Opaque = this.Description.Opaque;
            this.ground.DeferredEnabled = this.Description.DeferredEnabled;

            if (!this.Description.DelayGeneration)
            {
                this.UpdateInternals();
            }
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        public override void Dispose()
        {
            if (this.ground != null)
            {
                this.ground.Dispose();
                this.ground = null;
            }

            if (this.vegetation != null && this.vegetation.Length > 0)
            {
                for (int i = 0; i < this.vegetation.Length; i++)
                {
                    this.vegetation[i].Dispose();
                }

                this.vegetation = null;
            }
        }
        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.pickingQuadtree == null)
            {
                this.ground.Update(context);

                if (this.vegetation != null && this.vegetation.Length > 0)
                {
                    for (int i = 0; i < this.vegetation.Length; i++)
                    {
                        this.vegetation[i].Update(context);
                    }
                }
            }
            else
            {
                this.ground.Update(context);
            }
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            if (this.pickingQuadtree == null)
            {
                if (!this.ground.Cull)
                {
                    this.ground.Draw(context);

                    if (this.vegetation != null && this.vegetation.Length > 0)
                    {
                        for (int i = 0; i < this.vegetation.Length; i++)
                        {
                            this.vegetation[i].Draw(context);
                        }
                    }
                }
            }
            else
            {
                this.ground.Draw(context);
            }
        }
   
        /// <summary>
        /// Updates internal objects
        /// </summary>
        public override void UpdateInternals()
        {
            var triangles = this.GetTriangles();

            if (this.Description != null && this.Description.Quadtree != null)
            {
                this.pickingQuadtree = QuadTree.Build(this.Game, triangles, this.Description);
            }

            if (this.Description != null && this.Description.PathFinder != null)
            {
                this.navigationGraph = PathFinder.Build(this.Description.PathFinder.Settings, triangles);
            }
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public override bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickNearest(ref ray, out position, out triangle);
            }
            else
            {
                return this.ground.PickNearest(ref ray, out position, out triangle);
            }
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public override bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickFirst(ref ray, out position, out triangle);
            }
            else
            {
                return this.ground.PickFirst(ref ray, out position, out triangle);
            }
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <returns>Returns true if picked positions found</returns>
        public override bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickAll(ref ray, out positions, out triangles);
            }
            else
            {
                return this.ground.PickAll(ref ray, out positions, out triangles);
            }
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public override PathFindingPath FindPath(Vector3 from, Vector3 to)
        {
            var path = this.navigationGraph.FindPath(from, to);
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
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public override BoundingSphere GetBoundingSphere()
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.BoundingSphere;
            }
            else
            {
                BoundingSphere sph = this.ground.GetBoundingSphere();

                for (int i = 0; i < this.GroundObjects.Count; i++)
                {
                    var curr = this.GroundObjects[i];

                    if (curr is Model)
                    {
                        BoundingSphere.Merge(sph, ((Model)curr).GetBoundingSphere());
                    }

                    if (curr is ModelInstanced)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr).Instances.Length; m++)
                        {
                            BoundingSphere.Merge(sph, ((ModelInstanced)curr).Instances[m].GetBoundingSphere());
                        }
                    }
                }

                return sph;
            }
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public override BoundingBox GetBoundingBox()
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.BoundingBox;
            }
            else
            {
                BoundingBox bbox = this.ground.GetBoundingBox();

                for (int i = 0; i < this.GroundObjects.Count; i++)
                {
                    var curr = this.GroundObjects[i];

                    if (curr is Model)
                    {
                        BoundingBox.Merge(bbox, ((Model)curr).GetBoundingBox());
                    }

                    if (curr is ModelInstanced)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr).Instances.Length; m++)
                        {
                            BoundingBox.Merge(bbox, ((ModelInstanced)curr).Instances[m].GetBoundingBox());
                        }
                    }
                }

                return bbox;
            }
        }

        /// <summary>
        /// Gets terrain bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns terrain bounding boxes</returns>
        public BoundingBox[] GetBoundingBoxes(int level = 0)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.GetBoundingBoxes(level);
            }
            else
            {
                List<BoundingBox> res = new List<BoundingBox>();

                res.Add(this.ground.GetBoundingBox());

                for (int i = 0; i < this.GroundObjects.Count; i++)
                {
                    var curr = this.GroundObjects[i];

                    if (curr is Model)
                    {
                        res.Add(((Model)curr).GetBoundingBox());
                    }

                    if (curr is ModelInstanced)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr).Instances.Length; m++)
                        {
                            res.Add(((ModelInstanced)curr).Instances[m].GetBoundingBox());
                        }
                    }
                }

                return res.ToArray();
            }
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list. Empty if the vertex type hasn't position channel</returns>
        public Triangle[] GetTriangles()
        {
            List<Triangle> tris = new List<Triangle>();

            tris.AddRange(this.ground.GetTriangles());

            for (int i = 0; i < this.GroundObjects.Count; i++)
            {
                var curr = this.GroundObjects[i];

                if (curr is Model)
                {
                    tris.AddRange(((Model)curr).GetTriangles());
                }

                if (curr is ModelInstanced)
                {
                    for (int m = 0; m < ((ModelInstanced)curr).Instances.Length; m++)
                    {
                        tris.AddRange(((ModelInstanced)curr).Instances[m].GetTriangles());
                    }
                }
            }

            return tris.ToArray();
        }
        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <returns>Returns the path finder grid nodes</returns>
        public IGraphNode[] GetNodes()
        {
            IGraphNode[] nodes = null;

            if (this.navigationGraph != null)
            {
                nodes = this.navigationGraph.GetNodes();
            }

            return nodes;
        }
    }
}
