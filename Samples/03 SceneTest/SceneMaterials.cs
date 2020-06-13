using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace SceneTest
{
    public class SceneMaterials : Scene
    {
        private const int layerHUD = 99;

        private readonly float spaceSize = 40;
        private readonly float radius = 1;
        private readonly uint stacks = 40;

        private UITextArea title = null;
        private UITextArea runtime = null;

        private bool gameReady = false;

        public SceneMaterials(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-20, 10, -40f);
            this.Camera.LookTo(0, 0, 0);

            this.Lights.DirectionalLights[0].CastShadow = false;

            GameEnvironment.Background = Color.CornflowerBlue;

            await this.LoadResourcesAsync(
                new[]
                {
                    this.InitializeTextBoxes(),
                    this.InitializeSkyEffects(),
                    this.InitializeFloor(),
                    this.InitializeColorGroup(1, 0.1f, new Vector3(-10, 0, -10), false),
                    this.InitializeColorGroup(128, 1f, new Vector3(-10.5f, 0, -10), true)
                },
                () =>
                {
                    gameReady = true;
                });
        }

        private async Task InitializeTextBoxes()
        {
            this.title = await this.AddComponentUITextArea(new UITextAreaDescription { TextDescription = TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange) }, layerHUD);
            this.runtime = await this.AddComponentUITextArea(new UITextAreaDescription { TextDescription = TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange) }, layerHUD);

            this.title.Text = "Scene Test - Materials";
            this.runtime.Text = "";

            this.title.SetPosition(Vector2.Zero);
            this.runtime.SetPosition(new Vector2(5, this.title.Top + this.title.Height + 3));

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.runtime.Top + this.runtime.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeSkyEffects()
        {
            await this.AddComponentLensFlare(new LensFlareDescription()
            {
                ContentPath = @"Common/lensFlare",
                GlowTexture = "lfGlow.png",
                Flares = new[]
                {
                    new LensFlareDescription.Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 0.3f, 0.4f, new Color(100, 255, 200), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.2f, 1.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "lfFlare1.png"),

                    new LensFlareDescription.Flare(-0.3f, 0.7f, new Color(200,  50,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "lfFlare2.png"),

                    new LensFlareDescription.Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            });
        }
        private async Task InitializeFloor()
        {
            float l = spaceSize;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, l) },
                new VertexData{ Position = new Vector3(+l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(l, 0.0f) },
                new VertexData{ Position = new Vector3(+l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(l, l) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            MaterialContent mat = new MaterialContent()
            {
                EmissionColor = new Color4(0f, 0f, 0f, 0f),
                AmbientColor = new Color4(0.02f, 0.02f, 0.02f, 1f),

                DiffuseColor = new Color4(0.8f, 0.8f, 0.8f, 1f),
                DiffuseTexture = "SceneMaterials/floor.png",

                SpecularColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
                Shininess = 1024,
            };

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                AlphaEnabled = false,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            await this.AddComponentModel(desc);
        }
        private async Task<Model> InitializeSphere(string name, MaterialContent material)
        {
            var sphere = GeometryUtil.CreateSphere(radius, stacks, stacks);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;
            var content = ModelContent.GenerateTriangleList(vertices, indices, material);

            var desc = new ModelDescription()
            {
                Name = name,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            return await this.AddComponentModel(desc);
        }
        private async Task<MaterialContent> GenerateMaterial(Color4 diffuse, Color4 specular, float shininess, bool nmap)
        {
            var mat = new MaterialContent()
            {
                EmissionColor = new Color4(0f, 0f, 0f, 0f),
                AmbientColor = new Color4(0.02f, 0.02f, 0.02f, 1f),

                DiffuseColor = diffuse,
                DiffuseTexture = "SceneMaterials/white.png",
                NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png",

                SpecularColor = specular,
                Shininess = shininess,
            };

            return await Task.FromResult(mat);
        }
        private async Task InitializeColorGroup(float shininess, float specularFactor, Vector3 position, bool nmap)
        {
            int n = 32;

            for (int r = 0; r < 256; r += n)
            {
                for (int g = 0; g < 256; g += n)
                {
                    for (int b = 0; b < 256; b += n)
                    {
                        float f = 1f / (float)n * 4f;

                        var diffuse = new Color4(r / 256f, g / 256f, b / 256f, 1);
                        var specular = new Color4(r / 256f * specularFactor, g / 256f * specularFactor, b / 256f * specularFactor, 1);

                        var material = await this.GenerateMaterial(diffuse, specular, shininess, nmap);
                        var model = await this.InitializeSphere(string.Format("Sphere {0}.{1}.{2}", r, g, b), material);
                        model.Manipulator.SetPosition(new Vector3(r * f, (g * f) + 1f, b * f) + position);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!gameReady)
            {
                return;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            base.Update(gameTime);

            this.runtime.Text = this.Game.RuntimeText;
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                gameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }
    }
}
