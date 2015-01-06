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
        /// Position cache
        /// </summary>
        private Triangle[] terrainCache = null;

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
            this.terrainCache = content.ComputeTriangleList();

            if (description != null && description.AddVegetation)
            {
                ModelContent vegetationContent = ModelContent.GenerateVegetationBillboard(
                    contentFolder,
                    description.VegetarionTextures,
                    this.terrainCache,
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
        /// Gets ground position giving x, z coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns>Returns ground position if exists</returns>
        public Vector3? FindGroundPosition(float x, float z)
        {
            Vector3? position = null;

            Ray ray = new Ray()
            {
                Position = new Vector3(x, 1000f, z),
                Direction = Vector3.Down,
            };

            for (int i = 0; i < this.terrainCache.Length; i++)
            {
                Triangle tri = Triangle.Transform(this.terrainCache[i], this.Manipulator.LocalTransform);

                Vector3 pos;
                if (tri.Intersects(ref ray, out pos))
                {
                    position = pos;

                    break;
                }
            }

            return position;
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
