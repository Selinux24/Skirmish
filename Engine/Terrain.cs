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
        public Manipulator3D Manipulator { get { return this.terrain.Manipulator; } }

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

            if (description != null && description.AddVegetation)
            {
                Triangle[] triangles = this.terrain.GetTriangles();

                ModelContent vegetationContent = ModelContent.GenerateVegetationBillboard(
                    contentFolder,
                    triangles,
                    description.VegetarionTextures,
                    description.Saturation,
                    description.MinSize,
                    description.MaxSize,
                    description.Seed);

                this.vegetation = new Billboard(game, scene, vegetationContent);
                this.vegetation.Manipulator = this.Manipulator;
            }

            if (description != null && description.AddSkydom)
            {
                ModelContent skydomContent = ModelContent.GenerateSkydom(
                    contentFolder,
                    description.SkydomTexture,
                    1000f);

                this.skydom = new Cubemap(game, scene, skydomContent);
                this.skydom.Manipulator = this.Manipulator;
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
        /// <param name="point">Plane position</param>
        /// <param name="position">Ground position if exists</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindGroundPosition(Vector2 point, out Vector3 position)
        {
            return FindGroundPosition(point.X, point.Y, out position);
        }
        /// <summary>
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="point">Plane position</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public bool FindGroundPosition(Vector2 point, out Vector3 position, out Triangle triangle)
        {
            return FindGroundPosition(point.X, point.Y, out position, out triangle);
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
