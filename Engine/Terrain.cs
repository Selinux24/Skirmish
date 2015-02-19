using SharpDX;

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
        /// Geometry
        /// </summary>
        private Model terrain = null;
        /// <summary>
        /// Vegetation
        /// </summary>
        private Billboard vegetation = null;
        /// <summary>
        /// Skydom
        /// </summary>
        private Cubemap skydom = null;
        /// <summary>
        /// Quadtree used for picking
        /// </summary>
        public QuadTree pickingQuadtree = null;
        /// <summary>
        /// Grid used for pathfinding
        /// </summary>
        public Grid grid = null;

        /// <summary>
        /// Current scene
        /// </summary>
        public new Scene3D Scene
        {
            get
            {
                return base.Scene as Scene3D;
            }
            set
            {
                base.Scene = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="scene">Scene</param>
        /// <param name="content">Geometry content</param>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="description">Terrain description</param>
        public Terrain(Game game, Scene3D scene, ModelContent content, string contentFolder, TerrainDescription description)
            : base(game, scene)
        {
            this.terrain = new Model(game, scene, content);

            BoundingBox bbox = this.terrain.GetBoundingBox();
            Triangle[] triangles = this.terrain.GetTriangles();

            if (description != null && description.AddVegetation)
            {
                ModelContent vegetationContent = ModelContent.GenerateVegetationBillboard(
                    contentFolder,
                    triangles,
                    description.VegetarionTextures,
                    description.Saturation,
                    description.MinSize,
                    description.MaxSize,
                    description.Seed);

                this.vegetation = new Billboard(game, scene, vegetationContent);
            }

            if (description != null && description.AddSkydom)
            {
                ModelContent skydomContent = ModelContent.GenerateSkydom(
                    contentFolder,
                    description.SkydomTexture,
                    1000f);

                this.skydom = new Cubemap(game, scene, skydomContent);
            }

            if (description != null && description.UseQuadtree)
            {
                this.pickingQuadtree = QuadTree.Build(triangles);
            }

            if (description != null && description.UsePathFinding)
            {
                this.grid = Grid.Build(this, description.PathNodeSize);
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

            if (this.vegetation != null)
            {
                this.vegetation.Dispose();
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
            this.terrain.Update(gameTime);

            if (this.vegetation != null) this.vegetation.Update(gameTime);

            if (this.skydom != null) this.skydom.Update(gameTime);
        }
        /// <summary>
        /// Objects drawing
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Draw(GameTime gameTime)
        {
            this.terrain.Draw(gameTime);

            if (this.vegetation != null)
            {
                this.vegetation.Draw(gameTime);
            }

            if (this.skydom != null)
            {
                this.skydom.Draw(gameTime);
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
            Ray ray = new Ray()
            {
                Position = new Vector3(x, 1000f, z),
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
            Ray ray = new Ray()
            {
                Position = new Vector3(x, 1000f, z),
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
            return this.terrain.GetBoundingSphere();
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox()
        {
            return this.terrain.GetBoundingBox();
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list. Empty if the vertex type hasn't position channel</returns>
        public Triangle[] GetTriangles()
        {
            return this.terrain.GetTriangles();
        }
    }

    /// <summary>
    /// Terrain description
    /// </summary>
    public class TerrainDescription
    {
        /// <summary>
        /// Indicates whether the new terrain has vegetation
        /// </summary>
        public bool AddVegetation = false;
        /// <summary>
        /// Texture names array for vegetation
        /// </summary>
        public string[] VegetarionTextures = null;
        /// <summary>
        /// Vegetation saturation per triangle
        /// </summary>
        public float Saturation = 0f;
        /// <summary>
        /// Vegetation sprite minimum size
        /// </summary>
        public Vector2 MinSize = Vector2.Zero;
        /// <summary>
        /// Vegetation sprite maximum size
        /// </summary>
        public Vector2 MaxSize = Vector2.Zero;
        /// <summary>
        /// Seed for random position generation
        /// </summary>
        public int Seed = 0;

        /// <summary>
        /// Indicates whether the new terrain has skydom
        /// </summary>
        public bool AddSkydom = false;
        /// <summary>
        /// Skydom cube texture
        /// </summary>
        public string SkydomTexture = null;

        /// <summary>
        /// Use quadtree for picking
        /// </summary>
        public bool UseQuadtree = true;

        /// <summary>
        /// Generate grid for path finding
        /// </summary>
        public bool UsePathFinding = false;
        /// <summary>
        /// Path node side size
        /// </summary>
        public float PathNodeSize = 10f;
    }
}
