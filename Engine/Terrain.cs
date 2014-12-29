using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    public class Terrain : Drawable
    {
        private Model terrain = null;
        private Billboard vegetation = null;
        private Cubemap skydom = null;
        private Triangle[] terrainCache = null;

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
        public Manipulator Manipulator { get; private set; }

        public Terrain(Game game, Scene3D scene, ModelContent terrainContent, string contentFolder, TerrainDescription description, bool debugMode = false)
            : base(game, scene)
        {
            this.Manipulator = new Manipulator();

            this.terrain = new Model(game, scene, terrainContent, debugMode);
            this.terrainCache = terrainContent.ComputeTriangleList();

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
        public override void HandleResizing()
        {
            
        }

        public Vector3? SetToGround(float x, float z)
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

    public class TerrainDescription
    {
        public bool AddVegetation = false;
        public string[] VegetarionTextures = null;
        public float Saturation = 0f;
        public Vector2 MinSize = Vector2.Zero;
        public Vector2 MaxSize = Vector2.Zero;
        public int Seed = 0;

        public bool AddSkydom = false;
        public string SkydomTexture = null;
    }
}
