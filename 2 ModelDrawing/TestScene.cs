using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;

namespace ModelDrawing
{
    public class TestScene : Scene
    {
        private TextDrawer text = null;

        private Model floor = null;

        private Model colorModel = null;
        private Model textureModel = null;
        private Model normalColorModel = null;
        private Model normalTextureModel = null;
        private Model[] models = null;

        private int selected = 0;
        private float spot = 0f;
        private Vector3 attenuation = new Vector3(1, 0, 0);
        private int selectedAttenuation = 0;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.text = this.AddText("Arial", 24, Color.Yellow);
            this.text.Position = Vector2.One;

            this.InitializeFloor();

            this.InitializeModels();

            this.Camera.Goto(Vector3.ForwardLH * -15f + Vector3.UnitY * 10f);
            this.Camera.LookTo(Vector3.Zero);

            float range = Vector3.Distance(this.Camera.Position, Vector3.Zero);

            Ray r = new Ray(this.Camera.Position, this.Camera.Direction);

            Vector3 p;
            Triangle t;
            if (this.floor.PickNearest(ref r, out p, out t))
            {
                range = Vector3.Distance(this.Camera.Position, p);
            }

            SceneLightSpot spotLight = new SceneLightSpot()
            {
                LightColor = Color.Yellow,
                Spot = 0f,
                Attenuation = new Vector3(1f, 0f, 0f),
                Position = this.Camera.Position,
                Direction = this.Camera.Direction,
                Enabled = true,
            };

            this.Lights.SpotLights = new[]
            {
                spotLight,
            };

            this.InitializePositions();
        }
        private void InitializeFloor()
        {
            float l = 20f;
            float h = 4f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 0.0f) },
                new VertexData{ Position = new Vector3(+l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            this.floor = this.AddModel(ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, this.CreateMaterialFloor()));
        }
        private void InitializeModels()
        {
            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-1f, +0f, +0f), Normal = Vector3.BackwardLH, Color = Color.Red, Texture = new Vector2(0.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+0f, +2f, +0f), Normal = Vector3.BackwardLH, Color = Color.Red, Texture = new Vector2(0.5f, 0.0f) },
                new VertexData{ Position = new Vector3(+1f, +0f, +0f), Normal = Vector3.BackwardLH, Color = Color.Red, Texture = new Vector2(1.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+0f, -2f, +0f), Normal = Vector3.BackwardLH, Color = Color.Red, Texture = new Vector2(0.5f, 0.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            this.colorModel = this.AddModel(ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionColor, vertices, indices, this.CreateMaterialColor()));
            this.textureModel = this.AddModel(ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionTexture, vertices, indices, this.CreateMaterialTexture()));
            this.normalColorModel = this.AddModel(ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalColor, vertices, indices, this.CreateMaterialColor()));
            this.normalTextureModel = this.AddModel(ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, this.CreateMaterialTexture()));

            this.models = new[]
            {
                this.colorModel,
                this.textureModel,
                this.normalColorModel,
                this.normalTextureModel,
            };
        }
        private void InitializePositions()
        {
            this.colorModel.Manipulator.SetPosition(Vector3.UnitX * -3f);
            this.textureModel.Manipulator.SetPosition(Vector3.UnitX * -1f);
            this.normalColorModel.Manipulator.SetPosition(Vector3.UnitX * 1f);
            this.normalTextureModel.Manipulator.SetPosition(Vector3.UnitX * 3f);
        }
        private MaterialContent CreateMaterialFloor()
        {
            MaterialContent mat = MaterialContent.Default;

            mat.DiffuseTexture = "resources/floor.png";

            return mat;
        }
        private MaterialContent CreateMaterialColor()
        {
            return MaterialContent.Default;
        }
        private MaterialContent CreateMaterialTexture()
        {
            MaterialContent mat = MaterialContent.Default;

            mat.DiffuseTexture = "resources/seafloor.dds";

            return mat;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape)) { this.Game.Exit(); }
            if (this.Game.Input.KeyJustReleased(Keys.Home)) { this.InitializePositions(); }
            if (this.Game.Input.KeyJustReleased(Keys.Tab)) { this.NextModel(); }
            if (this.Game.Input.KeyJustReleased(Keys.Space)) { this.NextAttenuation(); }

            if (this.Game.Input.KeyJustReleased(Keys.L)) { this.Lights.SpotLights[0].Enabled = !this.Lights.SpotLights[0].Enabled; }

            Manipulator3D selectedModel = this.models[this.selected].Manipulator;
            if (selectedModel != null)
            {
                if (this.Game.Input.KeyPressed(Keys.A)) { selectedModel.MoveRight(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.D)) { selectedModel.MoveLeft(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.W)) { selectedModel.MoveUp(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.S)) { selectedModel.MoveDown(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.Z)) { selectedModel.MoveBackward(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.X)) { selectedModel.MoveForward(gameTime); }

                if (this.Game.Input.KeyPressed(Keys.J)) { selectedModel.YawLeft(gameTime, MathUtil.PiOverTwo); }
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                spot += 0.01f;
            }
            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                spot -= 0.01f;
            }
            if (spot < 0f) spot = 0f;

            this.Lights.SpotLights[0].Spot = spot;
            this.Lights.SpotLights[0].Attenuation = this.attenuation;

            this.text.Text = string.Format("Spot {0:0.00}; Attenuation {1}", spot, this.attenuation);
        }
        private void NextModel()
        {
            if (this.selected >= this.models.Length - 1)
            {
                this.selected = 0;
            }
            else
            {
                this.selected++;
            }
        }
        private void NextAttenuation()
        {
            if (this.selectedAttenuation >= 3 - 1)
            {
                this.selectedAttenuation = 0;
            }
            else
            {
                this.selectedAttenuation++;
            }

            if (this.selectedAttenuation == 0) this.attenuation = new Vector3(1, 0, 0);
            else if (this.selectedAttenuation == 1) this.attenuation = new Vector3(0, 1, 0);
            else if (this.selectedAttenuation == 2) this.attenuation = new Vector3(0, 0, 1);
        }
    }
}
