using Common;
using Common.Utils;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DirectInput;

namespace ModelDrawing
{
    public class TestScene : Scene3D
    {
        private BasicModel colorModel = null;
        private BasicModel textureModel = null;
        private BasicModel normalColorModel = null;
        private BasicModel normalTextureModel = null;
        private Material materialColor = new Material()
        {
            Ambient = Color4.White,
            Diffuse = Color4.White,
            Emission = Color4.Black,
            Specular = Color4.Black,
            Reflective = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            Transparent = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            IndexOfRefraction = 1,
        };
        private Material materialTexture = new Material()
        {
            Ambient = Color4.Black,
            Diffuse = Color4.White,
            Emission = Color4.Black,
            Specular = Color4.Black,
            Reflective = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            Transparent = new Color4(0.0f, 0.0f, 0.0f, 0.0f),
            IndexOfRefraction = 1,
            Texture = new TextureDescription()
            {
                Name = "seafloor.dds",
                TextureArray = new string[] { "resources/seafloor.dds" },
            },
        };

        private BasicModel[] models = null;
        private int selected = 0;

        private bool moveLight = false;

        public TestScene(Game game)
            : base(game)
        {
            this.colorModel = this.AddModel(this.CreatePositionColor(game));
            this.textureModel = this.AddModel(this.CreatePositionTexture(game));
            this.normalColorModel = this.AddModel(this.CreatePositionNormalColor(game));
            this.normalTextureModel = this.AddModel(this.CreatePositionNormalTexture(game));

            this.SetPositions();

            this.models = new BasicModel[] 
            {  
                this.colorModel,
                this.textureModel,
                this.normalColorModel,
                this.normalTextureModel,
            };

            this.Camera.Position = Vector3.UnitZ * -8f + Vector3.UnitY * 4f;
            this.Camera.Interest = Vector3.Zero;

            this.Lights.DirectionalLight1.Direction = Vector3.BackwardLH;
            this.Lights.DirectionalLight2.Direction = Vector3.BackwardLH;
            this.Lights.DirectionalLight3.Direction = Vector3.BackwardLH;
        }

        private void SetPositions()
        {
            this.colorModel.Transform.SetPosition(Vector3.UnitX * 1f);
            this.normalColorModel.Transform.SetPosition(Vector3.UnitX * 3f);
            this.textureModel.Transform.SetPosition(Vector3.UnitX * -3f);
            this.normalTextureModel.Transform.SetPosition(Vector3.UnitX * -1f);
        }
        private Geometry CreatePositionColor(Game game)
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

            return game.Graphics.Device.CreateGeometry(
                materialColor,
                vertices,
                PrimitiveTopology.TriangleList,
                indices);
        }
        private Geometry CreatePositionTexture(Game game)
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

            return game.Graphics.Device.CreateGeometry(
                materialTexture,
                vertices,
                PrimitiveTopology.TriangleList,
                indices);
        }
        private Geometry CreatePositionNormalColor(Game game)
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

            return game.Graphics.Device.CreateGeometry(
                materialColor,
                vertices,
                PrimitiveTopology.TriangleList,
                indices);
        }
        private Geometry CreatePositionNormalTexture(Game game)
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

            return game.Graphics.Device.CreateGeometry(
                materialTexture,
                vertices,
                PrimitiveTopology.TriangleList,
                indices);
        }
        public override void Update()
        {
            base.Update();

            if (this.Game.Input.KeyJustReleased(Key.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Key.L))
            {
                this.moveLight = !this.moveLight;
            }

            if (this.Game.Input.KeyJustReleased(Key.Tab))
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

            BasicModel selectedModel = this.models[this.selected];

            if (this.Game.Input.KeyJustReleased(Key.Home))
            {
                this.SetPositions();
            }

            if (this.Game.Input.KeyPressed(Key.A))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitX * -0.1f;
                else
                    selectedModel.Transform.MoveLeft(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.D))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitX * 0.1f;
                else
                    selectedModel.Transform.MoveRight(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.W))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitY * 0.1f;
                else
                    selectedModel.Transform.MoveUp(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.S))
            {
                if (this.moveLight)
                    this.Lights.DirectionalLight1.Direction += Vector3.UnitY * -0.1f;
                else
                    selectedModel.Transform.MoveDown(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.Z))
            {
                selectedModel.Transform.MoveForward(0.1f);
            }

            if (this.Game.Input.KeyPressed(Key.X))
            {
                selectedModel.Transform.MoveBackward(0.1f);
            }
        }
    }
}
