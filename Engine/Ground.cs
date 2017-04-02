﻿using SharpDX;
using System;
using System.Collections.Generic;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace Engine
{
    using Engine.Collections;
    using Engine.Common;
    using Engine.PathFinding;

    /// <summary>
    /// Ground class
    /// </summary>
    /// <remarks>Used for picking tests and navigation over surfaces</remarks>
    public abstract class Ground : Drawable, IGround, IRayPickable<Triangle>
    {
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

        /// <summary>
        /// Terrain attached objects
        /// </summary>
        protected List<AttachedModel> GroundObjects = new List<AttachedModel>();
        /// <summary>
        /// Quadtree for base ground picking
        /// </summary>
        protected PickingQuadTree<Triangle> groundPickingQuadtree = null;
        /// <summary>
        /// Quadtree for static objects picking
        /// </summary>
        protected PickingQuadTree<Triangle> objectsPickingQuadtree = null;
        /// <summary>
        /// Graph used for pathfinding
        /// </summary>
        protected IGraph navigationGraph = null;

        /// <summary>
        /// Instance description used for creation
        /// </summary>
        public GroundDescription Description { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Ground description</param>
        public Ground(Game game, BufferManager bufferManager, GroundDescription description)
            : base(game, bufferManager, description)
        {
            this.Description = description;
        }

        /// <summary>
        /// Attach objects to terrain
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="use">Use</param>
        /// <param name="updateInternals">Update internal objects</param>
        protected void Attach(ModelBase model, AttachedModelUsesEnum use, bool updateInternals = true)
        {
            this.GroundObjects.Add(new AttachedModel()
            {
                Model = model,
                Use = use,
            });

            if (updateInternals)
            {
                this.UpdateInternals();
            }
        }
        /// <summary>
        /// Attach objects to terrain for full picking and full path finding
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPickingFullPathFinding(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.FullPicking | AttachedModelUsesEnum.FullPathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse picking and coarse path finding
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePickingCoarsePathFinding(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.CoarsePicking | AttachedModelUsesEnum.CoarsePathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse picking and full path finding
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePickingFullPathFinding(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.CoarsePicking | AttachedModelUsesEnum.FullPathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for full picking and coarse path finding
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPickingCoarsePathFinding(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.FullPicking | AttachedModelUsesEnum.CoarsePathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for full picking
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPicking(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.FullPicking, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse picking
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePicking(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.CoarsePicking, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for full path finding
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPathFinding(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.FullPathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse path finding
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePathFinding(ModelBase model, bool updateInternals = true)
        {
            Attach(model, AttachedModelUsesEnum.CoarsePathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="use">Use</param>
        /// <param name="updateInternals">Update internal objects</param>
        protected void Attach(ModelBase[] models, AttachedModelUsesEnum use, bool updateInternals = true)
        {
            foreach (var model in models)
            {
                this.GroundObjects.Add(new AttachedModel()
                {
                    Model = model,
                    Use = use,
                });
            }

            if (updateInternals)
            {
                this.UpdateInternals();
            }
        }
        /// <summary>
        /// Attach objects to terrain for full picking and full path finding
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPickingFullPathFinding(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.FullPicking | AttachedModelUsesEnum.FullPathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse picking and coarse path finding
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePickingCoarsePathFinding(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.CoarsePicking | AttachedModelUsesEnum.CoarsePathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse picking and full path finding
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePickingFullPathFinding(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.CoarsePicking | AttachedModelUsesEnum.FullPathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for full picking and coarse path finding
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPickingCoarsePathFinding(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.FullPicking | AttachedModelUsesEnum.CoarsePathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for full picking
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPicking(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.FullPicking, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse picking
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePicking(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.CoarsePicking, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for full path finding
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachFullPathFinding(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.FullPathFinding, updateInternals);
        }
        /// <summary>
        /// Attach objects to terrain for coarse path finding
        /// </summary>
        /// <param name="models">Model list</param>
        /// <param name="updateInternals">Update internal objects</param>
        public void AttachCoarsePathFinding(ModelBase[] models, bool updateInternals = true)
        {
            Attach(models, AttachedModelUsesEnum.CoarsePathFinding, updateInternals);
        }

        /// <summary>
        /// Updates internal objects
        /// </summary>
        public virtual void UpdateInternals()
        {
            if (this.Description != null && this.Description.Quadtree != null)
            {
                var triangles = this.GetObjectTriangles(UsageEnum.Picking);
                if (triangles != null && triangles.Length > 0)
                {
                    this.objectsPickingQuadtree = new PickingQuadTree<Triangle>(triangles, this.Description.Quadtree.MaximumDepth);
                }
            }

            if (this.Description != null && this.Description.PathFinder != null)
            {
                var gTriangles = this.GetGroundTriangles(UsageEnum.PathFinding);
                var oTriangles = this.GetObjectTriangles(UsageEnum.PathFinding);

                List<Triangle> merged = new List<Triangle>();
                merged.AddRange(gTriangles);
                merged.AddRange(oTriangles);

                this.navigationGraph = PathFinder.Build(this.Description.PathFinder.Settings, merged.ToArray());
            }
        }
        /// <summary>
        /// Gets ground triangle list
        /// </summary>
        /// <returns>Returns triangle list. Empty if the vertex type hasn't position channel</returns>
        protected Triangle[] GetGroundTriangles(UsageEnum usage = UsageEnum.None)
        {
            List<Triangle> tris = new List<Triangle>();

            var nodes = this.groundPickingQuadtree.GetTailNodes();
            foreach (var node in nodes)
            {
                if (node.Items != null)
                {
                    tris.AddRange(node.Items);
                }
            }

            return tris.ToArray();
        }
        /// <summary>
        /// Gets attached objects triangle list
        /// </summary>
        /// <returns>Returns triangle list. Empty if the vertex type hasn't position channel</returns>
        protected Triangle[] GetObjectTriangles(UsageEnum usage = UsageEnum.None)
        {
            List<Triangle> tris = new List<Triangle>();

            for (int i = 0; i < this.GroundObjects.Count; i++)
            {
                var curr = this.GroundObjects[i];

                if (curr.Model is Model)
                {
                    var model = (Model)curr.Model;

                    model.Manipulator.UpdateInternals(true);

                    if (usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePathFinding))
                    {
                        var vTris = model.GetVolume();
                        if (vTris != null && vTris.Length > 0)
                        {
                            //Use volume mesh
                            tris.AddRange(vTris);
                        }
                        else
                        {
                            //Generate cylinder
                            var cylinder = BoundingCylinder.FromPoints(model.GetPoints());
                            tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                        }
                    }
                    else if (
                        usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.FullPicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.FullPathFinding))
                    {
                        //Use full mesh
                        tris.AddRange(model.GetTriangles());
                    }
                }
                else if (curr.Model is ModelInstanced)
                {
                    var model = (ModelInstanced)curr.Model;

                    if (usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.CoarsePathFinding))
                    {
                        Array.ForEach(model.Instances, (m) =>
                        {
                            m.Manipulator.UpdateInternals(true);

                            var vTris = m.GetVolume();
                            if (vTris != null && vTris.Length > 0)
                            {
                                //Use volume mesh
                                tris.AddRange(vTris);
                            }
                            else
                            {
                                //Generate cylinder
                                var cylinder = BoundingCylinder.FromPoints(m.GetPoints());
                                tris.AddRange(Triangle.ComputeTriangleList(PrimitiveTopology.TriangleList, cylinder, 8));
                            }
                        });
                    }
                    else if (
                        usage == UsageEnum.Picking && curr.Use.HasFlag(AttachedModelUsesEnum.FullPicking) ||
                        usage == UsageEnum.PathFinding && curr.Use.HasFlag(AttachedModelUsesEnum.FullPathFinding))
                    {
                        Array.ForEach(model.Instances, (m) =>
                        {
                            m.Manipulator.UpdateInternals(true);

                            //Use full mesh
                            tris.AddRange(m.GetTriangles());
                        });
                    }
                }
            }

            return tris.ToArray();
        }

        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindTopGroundPosition(float x, float z, out Vector3 position, out Triangle triangle, out float distance)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            return this.PickNearest(ref ray, true, out position, out triangle, out distance);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindFirstGroundPosition(float x, float z, out Vector3 position, out Triangle triangle, out float distance)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.1f, z),
                Direction = Vector3.Down,
            };

            return this.PickFirst(ref ray, true, out position, out triangle, out distance);
        }
        /// <summary>
        /// Gets all ground positions giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="positions">Ground positions if exists</param>
        /// <param name="triangles">Triangles found</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if ground positions found</returns>
        public bool FindAllGroundPosition(float x, float z, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(x, bbox.Maximum.Y + 0.01f, z),
                Direction = Vector3.Down,
            };

            return this.PickAll(ref ray, true, out positions, out triangles, out distances);
        }
        /// <summary>
        /// Gets nearest ground position to "from" position
        /// </summary>
        /// <param name="from">Position from</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindNearestGroundPosition(Vector3 from, out Vector3 position, out Triangle triangle, out float distance)
        {
            BoundingBox bbox = this.GetBoundingBox();

            Ray ray = new Ray()
            {
                Position = new Vector3(from.X, bbox.Maximum.Y + 0.01f, from.Z),
                Direction = Vector3.Down,
            };

            Vector3[] pArray;
            Triangle[] tArray;
            float[] dArray;
            if (this.PickAll(ref ray, true, out pArray, out tArray, out dArray))
            {
                int index = -1;
                float dist = float.MaxValue;
                for (int i = 0; i < pArray.Length; i++)
                {
                    float d = Vector3.DistanceSquared(from, pArray[i]);
                    if (d <= dist)
                    {
                        dist = d;

                        index = i;
                    }
                }

                position = pArray[index];
                triangle = tArray[index];
                distance = dArray[index];

                return true;
            }
            else
            {
                position = Vector3.Zero;
                triangle = new Triangle();
                distance = float.MaxValue;

                return false;
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
        public virtual bool PickNearest(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            bool res = false;

            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            if (this.objectsPickingQuadtree != null)
            {
                Vector3 oP;
                Triangle oT;
                float oD;
                if (this.objectsPickingQuadtree.PickNearest(ref ray, facingOnly, out oP, out oT, out oD))
                {
                    position = oP;
                    triangle = oT;
                    distance = oD;

                    res = true;
                }
            }

            if (this.groundPickingQuadtree != null)
            {
                Vector3 gP;
                Triangle gT;
                float gD;
                if (this.groundPickingQuadtree.PickNearest(ref ray, facingOnly, out gP, out gT, out gD))
                {
                    if (distance > gD)
                    {
                        position = gP;
                        triangle = gT;
                        distance = gD;
                    }

                    res = true;
                }
            }

            return res;
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
        public virtual bool PickFirst(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            bool res = false;

            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            if (this.objectsPickingQuadtree != null)
            {
                Vector3 oP;
                Triangle oT;
                float oD;
                if (this.objectsPickingQuadtree.PickFirst(ref ray, facingOnly, out oP, out oT, out oD))
                {
                    position = oP;
                    triangle = oT;
                    distance = oD;

                    res = true;
                }
            }

            if (this.groundPickingQuadtree != null)
            {
                Vector3 gP;
                Triangle gT;
                float gD;
                if (this.groundPickingQuadtree.PickFirst(ref ray, facingOnly, out gP, out gT, out gD))
                {
                    if (distance > gD)
                    {
                        position = gP;
                        triangle = gT;
                        distance = gD;
                    }

                    res = true;
                }
            }

            return res;
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
        public virtual bool PickAll(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            bool res = false;

            List<Vector3> lPositions = new List<Vector3>();
            List<Triangle> lTriangles = new List<Triangle>();
            List<float> lDistances = new List<float>();

            if (this.objectsPickingQuadtree != null)
            {
                Vector3[] oP;
                Triangle[] oT;
                float[] oD;
                if (this.objectsPickingQuadtree.PickAll(ref ray, facingOnly, out oP, out oT, out oD))
                {
                    lPositions.AddRange(oP);
                    lTriangles.AddRange(oT);
                    lDistances.AddRange(oD);

                    res = true;
                }
            }

            if (this.groundPickingQuadtree != null)
            {
                Vector3[] gP;
                Triangle[] gT;
                float[] gD;
                if (this.groundPickingQuadtree.PickAll(ref ray, facingOnly, out gP, out gT, out gD))
                {
                    lPositions.AddRange(gP);
                    lTriangles.AddRange(gT);
                    lDistances.AddRange(gD);

                    res = true;
                }
            }

            positions = lPositions.ToArray();
            triangles = lTriangles.ToArray();
            distances = lDistances.ToArray();

            return res;
        }
        /// <summary>
        /// Pick internal ground objects nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        protected virtual bool PickNearestGroundObjects(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            var forPickingObks = this.GroundObjects.FindAll(o => o.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) || o.Use.HasFlag(AttachedModelUsesEnum.FullPicking));
            if (forPickingObks.Count > 0)
            {
                bool picked = false;
                Vector3 bestP = Vector3.Zero;
                Triangle bestT = new Triangle();
                float bestD = float.MaxValue;

                foreach (var gObj in forPickingObks)
                {
                    var model = gObj.Model as Model;
                    if (model != null)
                    {
                        Vector3 p;
                        Triangle t;
                        float d;
                        if (model.PickNearest(ref ray, facingOnly, out p, out t, out d))
                        {
                            picked = true;

                            if (d < bestD)
                            {
                                bestP = p;
                                bestT = t;
                                bestD = d;
                            }
                        }
                    }

                    var modelI = gObj.Model as ModelInstanced;
                    if (modelI != null)
                    {
                        for (int i = 0; i < modelI.Count; i++)
                        {
                            Vector3 p;
                            Triangle t;
                            float d;
                            if (modelI.Instances[i].PickNearest(ref ray, facingOnly, out p, out t, out d))
                            {
                                picked = true;

                                if (d < bestD)
                                {
                                    bestP = p;
                                    bestT = t;
                                    bestD = d;
                                }
                            }
                        }
                    }
                }

                if (picked)
                {
                    position = bestP;
                    triangle = bestT;
                    distance = bestD;

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Pick internal ground objects first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <param name="distance">Distance to position</param>
        /// <returns>Returns true if picked position found</returns>
        protected virtual bool PickFirstGroundObjects(ref Ray ray, bool facingOnly, out Vector3 position, out Triangle triangle, out float distance)
        {
            position = Vector3.Zero;
            triangle = new Triangle();
            distance = float.MaxValue;

            var forPickingObks = this.GroundObjects.FindAll(o => o.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) || o.Use.HasFlag(AttachedModelUsesEnum.FullPicking));
            if (forPickingObks.Count > 0)
            {
                foreach (var gObj in forPickingObks)
                {
                    var model = gObj.Model as Model;
                    if (model != null)
                    {
                        Vector3 p;
                        Triangle t;
                        float d;
                        if (model.PickFirst(ref ray, facingOnly, out p, out t, out d))
                        {
                            position = p;
                            triangle = t;
                            distance = d;

                            return true;
                        }
                    }

                    var modelI = gObj.Model as ModelInstanced;
                    if (modelI != null)
                    {
                        for (int i = 0; i < modelI.Count; i++)
                        {
                            Vector3 p;
                            Triangle t;
                            float d;
                            if (modelI.Instances[i].PickFirst(ref ray, facingOnly, out p, out t, out d))
                            {
                                position = p;
                                triangle = t;
                                distance = d;

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Pick internal ground objects positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="facingOnly">Select only triangles facing to ray origin</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <param name="distances">Distances to positions</param>
        /// <returns>Returns true if picked position found</returns>
        protected virtual bool PickAllGroundObjects(ref Ray ray, bool facingOnly, out Vector3[] positions, out Triangle[] triangles, out float[] distances)
        {
            positions = null;
            triangles = null;
            distances = null;

            var forPickingObks = this.GroundObjects.FindAll(o => o.Use.HasFlag(AttachedModelUsesEnum.CoarsePicking) || o.Use.HasFlag(AttachedModelUsesEnum.FullPicking));
            if (forPickingObks.Count > 0)
            {
                bool picked = false;

                List<Vector3> pList = new List<Vector3>();
                List<Triangle> tList = new List<Triangle>();
                List<float> dList = new List<float>();

                foreach (var gObj in forPickingObks)
                {
                    var model = gObj.Model as Model;
                    if (model != null)
                    {
                        Vector3[] p;
                        Triangle[] t;
                        float[] d;
                        if (model.PickAll(ref ray, facingOnly, out p, out t, out d))
                        {
                            picked = true;

                            pList.AddRange(p);
                            tList.AddRange(t);
                            dList.AddRange(d);
                        }
                    }

                    var modelI = gObj.Model as ModelInstanced;
                    if (modelI != null)
                    {
                        for (int i = 0; i < modelI.Count; i++)
                        {
                            Vector3[] p;
                            Triangle[] t;
                            float[] d;
                            if (modelI.Instances[i].PickAll(ref ray, facingOnly, out p, out t, out d))
                            {
                                picked = true;

                                pList.AddRange(p);
                                tList.AddRange(t);
                                dList.AddRange(d);
                            }
                        }
                    }
                }

                if (picked)
                {
                    positions = pList.ToArray();
                    triangles = tList.ToArray();
                    distances = dList.ToArray();

                    return true;
                }
            }

            return false;
        }

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
                    Triangle triangle;
                    float distance;
                    if (FindNearestGroundPosition(path.ReturnPath[i], out position, out triangle, out distance))
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
            if (this.navigationGraph != null)
            {
                return this.navigationGraph.IsWalkable(agent, position, out nearest);
            }

            nearest = position;

            return true;
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
            Triangle t;
            float d;
            if (this.FindNearestGroundPosition(newPosition, out walkerPos, out t, out d))
            {
                Vector3? nearest;
                if (this.IsWalkable(agent, walkerPos, out nearest))
                {
                    finalPosition = walkerPos;
                    finalPosition.Y += agent.Height;

                    var moveP = newPosition - prevPosition;
                    var moveV = finalPosition - prevPosition;
                    if (moveV.LengthSquared() > moveP.LengthSquared())
                    {
                        finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
                    }

                    return true;
                }
                else
                {
                    //Not walkable but nearest position found
                    if (nearest.HasValue)
                    {
                        //Adjust height
                        var p = nearest.Value;
                        p.Y = prevPosition.Y;

                        if (this.FindNearestGroundPosition(p, out walkerPos, out t, out d))
                        {
                            finalPosition = walkerPos;
                            finalPosition.Y += agent.Height;

                            var moveP = newPosition - prevPosition;
                            var moveV = finalPosition - prevPosition;
                            if (moveV.LengthSquared() > moveP.LengthSquared())
                            {
                                finalPosition = prevPosition + (Vector3.Normalize(moveV) * moveP.Length());
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the node list suitable for foliage planting
        /// </summary>
        /// <param name="frustum">Camera frustum</param>
        /// <param name="sph">Foliagle bounding sphere</param>
        /// <returns>Returns a node list</returns>
        public abstract PickingQuadTreeNode<Triangle>[] GetFoliageNodes(BoundingFrustum frustum, BoundingSphere sph);
    }
}
