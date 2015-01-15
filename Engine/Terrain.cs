using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

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
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator { get; private set; }
        /// <summary>
        /// Gets static bounding box
        /// </summary>
        public BoundingBox BoundingBox { get { return this.terrain.BoundingBox; } }
        /// <summary>
        /// Gets static bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get { return this.terrain.BoundingSphere; } }
        /// <summary>
        /// Gets static oriented bounding box
        /// </summary>
        public OrientedBoundingBox OrientedBoundingBox { get { return this.terrain.OrientedBoundingBox; } }

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
            this.Manipulator = new Manipulator3D();

            this.terrain = new Model(game, scene, content);
            this.terrain.ComputeVolumes(Matrix.Identity);

            if (description != null && description.AddVegetation)
            {
                ModelContent vegetationContent = ModelContent.GenerateVegetationBillboard(
                    contentFolder,
                    description.VegetarionTextures,
                    this.terrain.Triangles,
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
            this.Manipulator.Update(gameTime);

            this.terrain.Manipulator.SetPosition(this.Manipulator.Position);
            this.terrain.Update(gameTime);

            if (this.vegetation != null)
            {
                this.vegetation.Manipulator.SetPosition(this.Manipulator.Position);
                this.vegetation.Update(gameTime);
            }

            if (this.skydom != null)
            {
                this.skydom.Manipulator.SetPosition(this.Scene.Camera.Position);
                this.skydom.Update(gameTime);
            }
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
        /// Updates model static volumes using per vertex transform
        /// </summary>
        /// <param name="transform">Per vertex transform</param>
        public void ComputeVolumes(Matrix transform)
        {
            this.terrain.ComputeVolumes(transform);
        }
        /// <summary>
        /// Get bounding boxes collection
        /// </summary>
        /// <returns>Returns bounding boxes list</returns>
        public BoundingBox[] GetBoundingBoxes()
        {
            return this.terrain.GetBoundingBoxes();
        }
        /// <summary>
        /// Get bounding spheres collection
        /// </summary>
        /// <returns>Returns bounding spheres list</returns>
        public BoundingSphere[] GetBoundingSpheres()
        {
            return this.terrain.GetBoundingSpheres();
        }
        /// <summary>
        /// Get oriented bounding boxes collection
        /// </summary>
        /// <returns>Returns oriented bounding boxes list</returns>
        public OrientedBoundingBox[] GetOrientedBoundingBoxes()
        {
            return this.terrain.GetOrientedBoundingBoxes();
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindGroundPosition(float x, float z, out Vector3 position)
        {
            Triangle tri;
            return FindGroundPosition(x, z, out position, out tri);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindGroundPosition(float x, float z, out Vector3 position, out Triangle triangle)
        {
            Ray ray = new Ray()
            {
                Position = new Vector3(x, 1000f, z),
                Direction = Vector3.Down,
            };

            return this.terrain.Pick(ray, out position, out triangle);
        }
        /// <summary>
        /// Pick position
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="position">Picked position if exists</param>
        /// <param name="triangle">Picked triangle if exists</param>
        /// <returns>Returns true if picked position found</returns>
        public bool Pick(Ray ray, out Vector3 position, out Triangle triangle)
        {
            return this.terrain.Pick(ray, out position, out triangle);
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
    }
}
