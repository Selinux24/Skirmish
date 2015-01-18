using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;

namespace ModelDrawing
{
    public class TestScene : Scene3D
    {
        private Model colorModel = null;
        private Model textureModel = null;
        private Model normalColorModel = null;
        private Model normalTextureModel = null;
        private Material materialColor = new Material()
        {
            AmbientColor = Color4.White,
            DiffuseColor = Color4.White,
            EmissionColor = Color4.Black,
            SpecularColor = Color4.Black,
            ReflectiveColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            Transparent = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            IndexOfRefraction = 1,
        };
        private Material materialTexture = new Material()
        {
            AmbientColor = Color4.Black,
            DiffuseColor = Color4.White,
            EmissionColor = Color4.Black,
            SpecularColor = Color4.Black,
            ReflectiveColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            Transparent = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            IndexOfRefraction = 1,
            //Texture = new TextureDescription()
            //{
            //    Name = "seafloor.dds",
            //    TextureArray = new string[] { "resources/seafloor.dds" },
            //},
        };

        private Model[] models = null;
        private int selected = 0;

        private bool moveLight = false;

        public TestScene(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.colorModel = this.AddModel(this.InitializePositionColor(this.Game));
            this.textureModel = this.AddModel(this.InitializePositionTexture(this.Game));
            this.normalColorModel = this.AddModel(this.InitializePositionNormalColor(this.Game));
            this.normalTextureModel = this.AddModel(this.InitializePositionNormalTexture(this.Game));

            this.InitializePositions();

            this.models = new Model[] 
            {  
                this.colorModel,
                this.textureModel,
                this.normalColorModel,
                this.normalTextureModel,
            };

            this.Camera.Goto(Vector3.UnitZ * -8f + Vector3.UnitY * 4f);
            this.Camera.LookTo(Vector3.Zero);

            this.Lights.DirectionalLight1.Direction = Vector3.BackwardLH;
            this.Lights.DirectionalLight2.Direction = Vector3.BackwardLH;
            this.Lights.DirectionalLight3.Direction = Vector3.BackwardLH;
        }
        private void InitializePositions()
        {
            this.colorModel.Manipulator.SetPosition(Vector3.UnitX * 1f);
            this.normalColorModel.Manipulator.SetPosition(Vector3.UnitX * 3f);
            this.textureModel.Manipulator.SetPosition(Vector3.UnitX * -3f);
            this.normalTextureModel.Manipulator.SetPosition(Vector3.UnitX * -1f);
        }
        private ModelContent InitializePositionColor(Game game)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[]
            {
                new VertexPositionColor{ Position = new Vector3(-1.0f, 0.0f, 0.0f), Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
                new VertexPositionColor{ Position = new Vector3(0.0f, 2.0f, 0.0f), Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
                new VertexPositionColor{ Position = new Vector3(1.0f, 0.0f, 0.0f), Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
                new VertexPositionColor{ Position = new Vector3(0.0f, -2.0f, 0.0f), Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            ModelContent modelInfo = null;
            //modelInfo = ModelInfo.CreateModel(vertices, indices);

            return modelInfo;
        }
        private ModelContent InitializePositionTexture(Game game)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[]
            {
                new VertexPositionTexture{ Position = new Vector3(-1.0f, 0.0f, 0.0f), Texture = new Vector2(0.0f, 1.0f) },
                new VertexPositionTexture{ Position = new Vector3(0.0f, 2.0f, 0.0f), Texture = new Vector2(0.5f, 0.0f) },
                new VertexPositionTexture{ Position = new Vector3(1.0f, 0.0f, 0.0f), Texture = new Vector2(1.0f, 1.0f) },
                new VertexPositionTexture{ Position = new Vector3(0.0f, -2.0f, 0.0f), Texture = new Vector2(0.5f, 0.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            ModelContent modelInfo = null;
            //modelInfo = ModelInfo.CreateModel(vertices, indices);

            return modelInfo;
        }
        private ModelContent InitializePositionNormalColor(Game game)
        {
            VertexPositionNormalColor[] vertices = new VertexPositionNormalColor[]
            {
                new VertexPositionNormalColor{ Position = new Vector3(-1.0f, 0.0f, 0.0f), Normal = Vector3.UnitZ, Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
                new VertexPositionNormalColor{ Position = new Vector3(0.0f, 2.0f, 0.0f), Normal = Vector3.UnitZ, Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
                new VertexPositionNormalColor{ Position = new Vector3(1.0f, 0.0f, 0.0f), Normal = Vector3.UnitZ, Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
                new VertexPositionNormalColor{ Position = new Vector3(0.0f, -2.0f, 0.0f), Normal = Vector3.UnitZ, Color = new Color4(1.0f, 0.0f,0.0f,1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            ModelContent modelInfo = null;
            //modelInfo = ModelInfo.CreateModel(vertices, indices);

            return modelInfo;
        }
        private ModelContent InitializePositionNormalTexture(Game game)
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[]
            {
                new VertexPositionNormalTexture{ Position = new Vector3(-1.0f, 0.0f, 0.0f), Normal = Vector3.UnitZ, Texture = new Vector2(0.0f, 1.0f) },
                new VertexPositionNormalTexture{ Position = new Vector3(0.0f, 2.0f, 0.0f), Normal = Vector3.UnitZ, Texture = new Vector2(0.5f, 0.0f) },
                new VertexPositionNormalTexture{ Position = new Vector3(1.0f, 0.0f, 0.0f), Normal = Vector3.UnitZ, Texture = new Vector2(1.0f, 1.0f) },
                new VertexPositionNormalTexture{ Position = new Vector3(0.0f, -2.0f, 0.0f), Normal = Vector3.UnitZ, Texture = new Vector2(0.5f, 0.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            ModelContent modelInfo = null;
            //modelInfo = ModelInfo.CreateModel(vertices, indices);

            return modelInfo;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                this.moveLight = !this.moveLight;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Tab))
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

            Model selectedModel = this.models[this.selected];

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.InitializePositions();
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitX * -0.1f;
                else
                    selectedModel.Manipulator.MoveLeft(gameTime, 0.1f);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitX * 0.1f;
                else
                    selectedModel.Manipulator.MoveRight(gameTime, 0.1f);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitY * 0.1f;
                else
                    selectedModel.Manipulator.MoveUp(gameTime, 0.1f);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitY * -0.1f;
                else
                    selectedModel.Manipulator.MoveDown(gameTime, 0.1f);
            }

            if (this.Game.Input.KeyPressed(Keys.Z))
            {
                selectedModel.Manipulator.MoveForward(gameTime, 0.1f);
            }

            if (this.Game.Input.KeyPressed(Keys.X))
            {
                selectedModel.Manipulator.MoveBackward(gameTime, 0.1f);
            }
        }
    }
}
