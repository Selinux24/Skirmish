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

        private SceneLightSpot spotLight = null;

        private int selected = 0;
        private float angle = 25f;
        private float radius = 25f;

        public TestScene(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
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

            this.radius = Vector3.Distance(this.Camera.Position, Vector3.Zero);

            Ray r = new Ray(this.Camera.Position, this.Camera.Direction);

            Vector3 p;
            Triangle t;
            if (this.floor.PickNearest(ref r, out p, out t))
            {
                this.radius = Vector3.Distance(this.Camera.Position, p);
            }

            this.spotLight = new SceneLightSpot()
            {
                Name = "Red Spot",
                LightColor = Color.White,
                AmbientIntensity = 0.2f,
                DiffuseIntensity = 25f,
                Angle = this.angle,
                Radius = this.radius * 10f,
                Position = this.Camera.Position,
                Direction = this.Camera.Direction,
                Enabled = true,
                CastShadow = false,
            };

            //this.Lights.Clear();
            //this.Lights.Add(SceneLightDirectional.Primary);
            this.Lights.Add(this.spotLight);

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
            this.floor.Opaque = true;
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

            this.colorModel.Opaque = true;
            this.textureModel.Opaque = true;
            this.normalColorModel.Opaque = true;
            this.normalTextureModel.Opaque = true;

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

            if (this.Game.Input.KeyJustReleased(Keys.L)) { this.spotLight.Enabled = !this.spotLight.Enabled; }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, this.Game.Input.ShiftPressed);
            }

            Manipulator3D selectedModel = this.models[this.selected].Manipulator;
            if (selectedModel != null)
            {
                if (this.Game.Input.KeyPressed(Keys.Left)) { selectedModel.MoveRight(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.Right)) { selectedModel.MoveLeft(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.Up)) { selectedModel.MoveUp(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.Down)) { selectedModel.MoveDown(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.PageUp)) { selectedModel.MoveBackward(gameTime); }
                if (this.Game.Input.KeyPressed(Keys.PageDown)) { selectedModel.MoveForward(gameTime); }

                if (this.Game.Input.KeyPressed(Keys.J)) { selectedModel.YawLeft(gameTime, MathUtil.PiOverTwo); }
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                this.angle += 0.01f;
            }
            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                this.angle -= 0.01f;
            }
            if (this.angle < 0f) this.angle = 0f;

            this.spotLight.Position = this.Camera.Position;
            this.spotLight.Direction = this.Camera.Direction;
            this.spotLight.Angle = this.angle;
            this.spotLight.Radius = this.radius;

            this.text.Text = string.Format("Angle {0:0.00}; Radius {1}; Enabled {2}", this.spotLight.Angle, this.spotLight.Radius, this.spotLight.Enabled);
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
    }
}
