using SharpDX;
using System.Collections.Generic;
using System;

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
        /// Skydom
        /// </summary>
        private Cubemap skydom = null;
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

                if (description != null && description.Vegetation != null && description.Vegetation.Length > 0)
                {
                    List<Billboard> vegetationList = new List<Billboard>();

                    BoundingBox bbox = this.terrain.GetBoundingBox();

                    for (int i = 0; i < description.Vegetation.Length; i++)
                    {
                        TerrainDescription.VegetationDescriptionBillboard vegetationDesc = description.Vegetation[i] as TerrainDescription.VegetationDescriptionBillboard;
                        if (vegetationDesc != null)
                        {
                            ModelContent vegetationContent = ModelContent.GenerateVegetationBillboard(
                                contentFolder,
                                bbox,
                                triangles,
                                vegetationDesc.VegetarionTextures,
                                vegetationDesc.Saturation,
                                vegetationDesc.MinSize,
                                vegetationDesc.MaxSize,
                                vegetationDesc.Seed);

                            if (vegetationContent != null)
                            {
                                var billboard = new Billboard(game, vegetationContent, 0)
                                {
                                    StartRadius = vegetationDesc.StartRadius,
                                    EndRadius = vegetationDesc.EndRadius,
                                    Opaque = vegetationDesc.Opaque,
                                    DeferredEnabled = vegetationDesc.DeferredEnabled,
                                };

                                vegetationList.Add(billboard);
                            }
                        }
                    }

                    this.vegetation = vegetationList.ToArray();
                }
            }

            if (description != null && description.Skydom != null)
            {
                ModelContent skydomContent = ModelContent.GenerateSkydom(
                    contentFolder,
                    description.Skydom.Texture,
                    bsph.Radius * 100f);

                this.skydom = new Cubemap(game, skydomContent)
                {
                    Opaque = description.Opaque,
                    DeferredEnabled = description.DeferredEnabled,
                };
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

            if (this.skydom != null)
            {
                this.skydom.Dispose();
                this.skydom = null;
            }
        }
        /// <summary>
        /// Objects updating
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
        {
            if (this.pickingQuadtree == null)
            {
                this.terrain.Update(gameTime);

                if (this.vegetation != null && this.vegetation.Length > 0)
                {
                    for (int i = 0; i < this.vegetation.Length; i++)
                    {
                        this.vegetation[i].Update(gameTime);
                    }
                }
            }
            else
            {
                this.terrain.Update(gameTime);

                this.pickingQuadtree.Update(gameTime);
            }

            if (this.skydom != null) this.skydom.Update(gameTime);
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            if (this.skydom != null)
            {
                this.skydom.Draw(gameTime, context);
            }

            if (this.pickingQuadtree == null)
            {
                if (!this.terrain.Cull)
                {
                    this.terrain.Draw(gameTime, context);

                    if (this.vegetation != null && this.vegetation.Length > 0)
                    {
                        for (int i = 0; i < this.vegetation.Length; i++)
                        {
                            this.vegetation[i].Draw(gameTime, context);
                        }
                    }
                }
            }
            else
            {
                this.terrain.Draw(gameTime, context);

                this.pickingQuadtree.Draw(gameTime, context);
            }
        }
        /// <summary>
        /// Performs frustum culling test
        /// </summary>
        /// <param name="frustum">Camera frustum</param>
        public override void FrustumCulling(BoundingFrustum frustum)
        {
            if (this.pickingQuadtree == null)
            {
                this.terrain.FrustumCulling(frustum);
            }
            else
            {
                this.pickingQuadtree.FrustumCulling(frustum);
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
        /// Gets the quad-tree nodes contained into the specified frustum
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <returns>Returns the quad-tree nodes contained into the specified frustum</returns>
        public QuadTreeNode[] Contained(ref BoundingFrustum frustum)
        {
            if (this.pickingQuadtree != null)
            {
                return this.pickingQuadtree.Contained(ref frustum);
            }

            return null;
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
            /// Height map file name
            /// </summary>
            public string HeightmapFileName = null;
            /// <summary>
            /// Textures for heightmap
            /// </summary>
            public string Texture = null;
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
        /// Skydom description
        /// </summary>
        public class SkydomDescription
        {
            /// <summary>
            /// Skydom cube texture
            /// </summary>
            public string Texture = null;
        }

        /// <summary>
        /// Vegetation
        /// </summary>
        public class VegetationDescription
        {
            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Resources";
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
        }
        /// <summary>
        /// Vegetation billboards
        /// </summary>
        public class VegetationDescriptionBillboard : VegetationDescription
        {
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
        /// Vegetation models
        /// </summary>
        public class VegetationDescriptionModel : VegetationDescription
        {
            /// <summary>
            /// Model file name
            /// </summary>
            public string Model;
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
        /// Vegetation collection
        /// </summary>
        public VegetationDescription[] Vegetation = null;
        /// <summary>
        /// Skydom
        /// </summary>
        public SkydomDescription Skydom = null;
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
