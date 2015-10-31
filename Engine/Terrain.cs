using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.PathFinding;

    /// <summary>
    /// Terrain model
    /// </summary>
    public class Terrain : Drawable
    {
        /// <summary>
        /// Quadtree used for picking
        /// </summary>
        private QuadTree pickingQuadtree = null;

        /// <summary>
        /// Geometry
        /// </summary>
        private Model terrain = null;
        /// <summary>
        /// Vegetation
        /// </summary>
        private Billboard[] vegetation = null;
        /// <summary>
        /// Grid used for pathfinding
        /// </summary>
        private Grid grid = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Geometry content</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="description">Terrain description</param>
        public Terrain(Game game, ModelContent content, string contentFolder, TerrainDescription description)
            : base(game)
        {
            this.DeferredEnabled = description.DeferredEnabled;

            this.terrain = new Model(game, content);
            this.terrain.Opaque = this.Opaque = description.Opaque;
            this.terrain.DeferredEnabled = description.DeferredEnabled;

            BoundingSphere bsph = this.terrain.GetBoundingSphere();
            Triangle[] triangles = this.terrain.GetTriangles();

            if (description != null && description.Quadtree != null)
            {
                this.pickingQuadtree = QuadTree.Build(game, triangles, description);
            }
            else
            {
                if (description != null && description.PathFinder != null)
                {
                    BoundingBox bbox = this.terrain.GetBoundingBox();

                    this.grid = Grid.Build(
                        bbox,
                        triangles,
                        description.PathFinder.NodeSize,
                        description.PathFinder.NodeInclination);
                }
            }
        }
        /// <summary>
        /// Dispose of created resources
        /// </summary>
        public override void Dispose()
        {
            if (this.terrain != null)
            {
                this.terrain.Dispose();
                this.terrain = null;
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
                this.terrain.Update(context);

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
                this.terrain.Update(context);

                this.pickingQuadtree.Update(context);
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
                if (!this.terrain.Cull)
                {
                    this.terrain.Draw(context);

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
                this.terrain.Draw(context);

                this.pickingQuadtree.Draw(context);
            }
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
        /// Pick nearest position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickNearest(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickNearest(ref ray, out position, out triangle);
            }
            else
            {
                return this.terrain.PickNearest(ref ray, out position, out triangle);
            }
        }
        /// <summary>
        /// Pick first position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public bool PickFirst(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickFirst(ref ray, out position, out triangle);
            }
            else
            {
                return this.terrain.PickFirst(ref ray, out position, out triangle);
            }
        }
        /// <summary>
        /// Pick all positions
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="positions">Picked positions if exists</param>
        /// <param name="triangles">Picked triangles if exists</param>
        /// <returns>Returns true if picked positions found</returns>
        public bool PickAll(ref Ray ray, out Vector3[] positions, out Triangle[] triangles)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.PickAll(ref ray, out positions, out triangles);
            }
            else
            {
                return this.terrain.PickAll(ref ray, out positions, out triangles);
            }
        }
        /// <summary>
        /// Find path from point to point
        /// </summary>
        /// <param name="from">Start point</param>
        /// <param name="to">End point</param>
        /// <returns>Return path if exists</returns>
        public Path FindPath(Vector3 from, Vector3 to)
        {
            return PathFinding.PathFinder.FindPath(this.grid, from, to);
        }

        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere()
        {
            if (this.pickingQuadtree == null)
            {
                return this.terrain.GetBoundingSphere();
            }
            else
            {
                return this.pickingQuadtree.BoundingSphere;
            }
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox()
        {
            if (this.pickingQuadtree == null)
            {
                return this.terrain.GetBoundingBox();
            }
            else
            {
                return this.pickingQuadtree.BoundingBox;
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
                return new[] { this.terrain.GetBoundingBox() };
            }
        }
        /// <summary>
        /// Gets the path finder grid nodes
        /// </summary>
        /// <returns>Returns the path finder grid nodes</returns>
        public GridNode[] GetNodes()
        {
            if (this.grid != null)
            {
                return this.grid.Nodes;
            }

            return null;
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list. Empty if the vertex type hasn't position channel</returns>
        public Triangle[] GetTriangles()
        {
            if (this.pickingQuadtree == null)
            {
                return this.terrain.GetTriangles();
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Terrain description
    /// </summary>
    public class TerrainDescription
    {
        /// <summary>
        /// Model description
        /// </summary>
        public class ModelDescription
        {
            /// <summary>
            /// Model file name
            /// </summary>
            public string ModelFileName = null;
        }
        /// <summary>
        /// Heightmap description
        /// </summary>
        public class HeightmapDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Heightmap";
            /// <summary>
            /// Height map file name
            /// </summary>
            public string HeightmapFileName = null;
            /// <summary>
            /// Color map file name
            /// </summary>
            public string ColormapFileName = null;
            /// <summary>
            /// Cell size
            /// </summary>
            public float CellSize = 1;
            /// <summary>
            /// Maximum height
            /// </summary>
            public float MaximumHeight = 1;
        }
        /// <summary>
        /// Terrain textures
        /// </summary>
        public class TexturesDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Textures";
            /// <summary>
            /// High resolution textures
            /// </summary>
            public string[] TexturesHR = null;
            /// <summary>
            /// Low resolution textures
            /// </summary>
            public string[] TexturesLR = null;
            /// <summary>
            /// Normal maps
            /// </summary>
            public string[] NormalMaps = null;
        }
        /// <summary>
        /// Vegetation
        /// </summary>
        public class VegetationDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Vegetation";
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float StartRadius = 0f;
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float EndRadius = 0f;
            /// <summary>
            /// Seed for random position generation
            /// </summary>
            public int Seed = 0;
            /// <summary>
            /// Vegetation saturation per triangle
            /// </summary>
            public float Saturation = 0.1f;
            /// <summary>
            /// Is opaque
            /// </summary>
            public bool Opaque = true;
            /// <summary>
            /// Can be renderer by the deferred renderer
            /// </summary>
            public bool DeferredEnabled = true;

            /// <summary>
            /// Texture names array for vegetation
            /// </summary>
            public string[] VegetarionTextures = null;
            /// <summary>
            /// Vegetation sprite minimum size
            /// </summary>
            public Vector2 MinSize = Vector2.One;
            /// <summary>
            /// Vegetation sprite maximum size
            /// </summary>
            public Vector2 MaxSize = Vector2.One * 2f;
        }
        /// <summary>
        /// Quadtree description
        /// </summary>
        public class QuadtreeDescription
        {
            /// <summary>
            /// Maximum triangle count per node
            /// </summary>
            private int maxTrianglesPerNode = 0;

            /// <summary>
            /// Maximum triangle count per node
            /// </summary>
            public int MaxTrianglesPerNode
            {
                get
                {
                    return this.maxTrianglesPerNode;
                }
                set
                {
                    this.maxTrianglesPerNode = value;

                    float v = (float)Math.Pow((Math.Sqrt(value / 2) + 1), 2);
                    int vi = (int)v;

                    if (v != (float)vi) throw new ArgumentException("Bad triangles per node count.");

                    this.MaxVerticesByNode = vi;
                }
            }
            /// <summary>
            /// Maximum vertex count per node
            /// </summary>
            public int MaxVerticesByNode { get; protected set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public QuadtreeDescription()
            {
                this.MaxTrianglesPerNode = 2048;
            }
        }
        /// <summary>
        /// Path finder grid description
        /// </summary>
        public class PathFinderDescription
        {
            /// <summary>
            /// Path node side size
            /// </summary>
            public float NodeSize = 10f;
            /// <summary>
            /// Path node maximum inclination
            /// </summary>
            public float NodeInclination = MathUtil.PiOverFour;
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Model
        /// </summary>
        public ModelDescription Model = null;
        /// <summary>
        /// Heightmap
        /// </summary>
        public HeightmapDescription Heightmap = null;
        /// <summary>
        /// Textures
        /// </summary>
        public TexturesDescription Textures = null;
        /// <summary>
        /// Vegetation collection
        /// </summary>
        public VegetationDescription Vegetation = null;
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = null;
        /// <summary>
        /// Path finder
        /// </summary>
        public PathFinderDescription PathFinder = null;

        /// <summary>
        /// Is Opaque
        /// </summary>
        public bool Opaque = true;
        /// <summary>
        /// Can be renderer by the deferred renderer
        /// </summary>
        public bool DeferredEnabled = true;
    }
}
