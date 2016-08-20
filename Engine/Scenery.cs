using SharpDX;
using System.Collections.Generic;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Collections;
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
            if (this.Description != null && this.Description.Quadtree != null)
            {
                var triangles = this.GetTriangles(UsageEnum.Picking);

                this.pickingQuadtree = QuadTree.Build(this.Game, triangles, this.Description);
            }

            if (this.Description != null && this.Description.PathFinder != null)
            {
                var triangles = this.GetTriangles(UsageEnum.PathFinding);

                this.navigationGraph = PathFinder.Build(this.Description.PathFinder.Settings, triangles);
            }
        }
        /// <summary>
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        public override bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickNearest(ref ray, facingOnly, out position, out triangle, out distance);
            }
            else
            {
                position = Vector3.Zero;
                triangle = new Triangle();
                distance = float.MaxValue;

                Vector3 p;
                Triangle t;
                float d;
                if (this.ground.PickNearest(ref ray, facingOnly, out p, out t, out d))
                {
                    Vector3 bestP = p;
                    Triangle bestT = t;
                    float bestD = d;

                    if (base.PickNearestGroundObjects(ref ray, facingOnly, out p, out t, out d))
                    {
                        if (d < bestD)
                        {
                            bestP = p;
                            bestT = t;
                            bestD = d;
                        }
                    }

                    position = bestP;
                    triangle = bestT;
                    distance = bestD;

                    return true;
                }

                return false;
            }
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        public override bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickFirst(ref ray, facingOnly, out position, out triangle, out distance);
            }
            else
            {
                position = Vector3.Zero;
                triangle = new Triangle();
                distance = float.MaxValue;

                Vector3 p;
                Triangle t;
                float d;
                if (this.ground.PickFirst(ref ray, facingOnly, out p, out t, out d))
                {
                    position = p;
                    triangle = t;
                    distance = d;

                    return true;
                }

                if (base.PickFirstGroundObjects(ref ray, facingOnly, out p, out t, out d))
                {
                    position = p;
                    triangle = t;
                    distance = d;

                    return true;
                }

                return false;
            }
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only facing triangles</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if picked positions found</returns>
        public override bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickAll(ref ray, facingOnly, out positions, out triangles, out distances);
            }
            else
            {
                positions = null;
                triangles = null;
                distances = null;

                List<Vector3> pList = new List<Vector3>();
                List<Triangle> tList = new List<Triangle>();
                List<float> dList = new List<float>();

                Vector3[] p;
                Triangle[] t;
                float[] d;
                if (this.ground.PickAll(ref ray, facingOnly, out p, out t, out d))
                {
                    pList.AddRange(p);
                    tList.AddRange(t);
                    dList.AddRange(d);

                    if (base.PickAllGroundObjects(ref ray, facingOnly, out p, out t, out d))
                    {
                        pList.AddRange(p);
                        tList.AddRange(t);
                        dList.AddRange(d);
                    }

                    positions = pList.ToArray();
                    triangles = tList.ToArray();
                    distances = dList.ToArray();

                    return true;
                }

                return false;
            }
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

                    if (curr.Model is Model)
                    {
                        BoundingSphere.Merge(sph, ((Model)curr.Model).GetBoundingSphere());
                    }

                    if (curr.Model is ModelInstanced)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr.Model).Instances.Length; m++)
                        {
                            BoundingSphere.Merge(sph, ((ModelInstanced)curr.Model).Instances[m].GetBoundingSphere());
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

                    if (curr.Model is Model)
                    {
                        BoundingBox.Merge(bbox, ((Model)curr.Model).GetBoundingBox());
                    }

                    if (curr.Model is ModelInstanced)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr.Model).Instances.Length; m++)
                        {
                            BoundingBox.Merge(bbox, ((ModelInstanced)curr.Model).Instances[m].GetBoundingBox());
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

                    if (curr.Model is Model)
                    {
                        res.Add(((Model)curr.Model).GetBoundingBox());
                    }

                    if (curr.Model is ModelInstanced)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr.Model).Instances.Length; m++)
                        {
                            res.Add(((ModelInstanced)curr.Model).Instances[m].GetBoundingBox());
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
        public Triangle[] GetTriangles(UsageEnum usage = UsageEnum.None)
        {
            List<Triangle> tris = new List<Triangle>();

            tris.AddRange(this.ground.GetTriangles());

            for (int i = 0; i < this.GroundObjects.Count; i++)
            {
                var curr = this.GroundObjects[i];

                if (usage == UsageEnum.Picking && !curr.EvaluateForPicking) continue;
                if (usage == UsageEnum.PathFinding && !curr.EvaluateForPathFinding) continue;

                if (curr.Model is Model)
                {
                    if (usage == UsageEnum.Picking && curr.UseVolumeForPicking || usage == UsageEnum.PathFinding && curr.UseVolumeForPathFinding)
                    {
                        var cylinder = BoundingCylinder.FromPoints(((Model)curr.Model).GetPoints());
                        tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                    }
                    else
                    {
                        tris.AddRange(((Model)curr.Model).GetTriangles());
                    }
                }

                if (curr.Model is ModelInstanced)
                {
                    if (usage == UsageEnum.Picking && curr.UseVolumeForPicking || usage == UsageEnum.PathFinding && curr.UseVolumeForPathFinding)
                    {
                        for (int m = 0; m < ((ModelInstanced)curr.Model).Instances.Length; m++)
                        {
                            var cylinder = BoundingCylinder.FromPoints(((ModelInstanced)curr.Model).Instances[m].GetPoints());
                            tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                        }
                    }
                    else
                    {
                        for (int m = 0; m < ((ModelInstanced)curr.Model).Instances.Length; m++)
                        {
                            tris.AddRange(((ModelInstanced)curr.Model).Instances[m].GetTriangles());
                        }
                    }
                }
            }

            return tris.ToArray();
        }
        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <param name="agent">Agent</param>
        /// <returns>Returns the path finder grid nodes</returns>
        public IGraphNode[] GetNodes(Agent agent)
        {
            IGraphNode[] nodes = null;

            if (this.navigationGraph != null)
            {
                nodes = this.navigationGraph.GetNodes(agent);
            }

            return nodes;
        }

        /// <summary>
        /// Usage enumeration for internal's update
        /// </summary>
        public enum UsageEnum
        {
            /// <summary>
            /// None
            /// </summary>
            None,
            /// <summary>
            /// For picking test
            /// </summary>
            Picking,
            /// <summary>
            /// For path finding test
            /// </summary>
            PathFinding,
        }
    }
}
