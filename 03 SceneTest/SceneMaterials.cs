using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;

namespace SceneTest
{
    public class SceneMaterials : Scene
    {
        private const int layerHUD = 99;

        private float spaceSize = 40;
        private float radius = 1;
        private uint stacks = 40;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> runtime = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<LensFlare> lensFlare = null;

        private SceneObject<Model> floor = null;

        public SceneMaterials(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-20, 10, -40f);
            this.Camera.LookTo(0, 0, 0);

            this.Lights.DirectionalLights[0].CastShadow = false;

            GameEnvironment.Background = Color.CornflowerBlue;

            this.InitializeTextBoxes();
            this.InitializeSkyEffects();
            this.InitializeFloor();
            this.InitializeColorGroup(1, 0.1f, new Vector3(-10, 0, -10), false);
            this.InitializeColorGroup(128, 1f, new Vector3(-10.5f, 0, -10), true);
        }

        private void InitializeTextBoxes()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange), SceneObjectUsageEnum.UI, layerHUD);
            this.runtime = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange), SceneObjectUsageEnum.UI, layerHUD);

            this.title.Instance.Text = "Scene Test - Materials";
            this.runtime.Instance.Text = "";

            this.title.Instance.Position = Vector2.Zero;
            this.runtime.Instance.Position = new Vector2(5, this.title.Instance.Top + this.title.Instance.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.runtime.Instance.Top + this.runtime.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
        }
        private void InitializeSkyEffects()
        {
            this.lensFlare = this.AddComponent<LensFlare>(new LensFlareDescription()
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
        private void InitializeFloor()
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

                AmbientColor = new Color4(0.8f, 0.8f, 0.8f, 1f),

                DiffuseColor = new Color4(0.8f, 0.8f, 0.8f, 1f),
                DiffuseTexture = "SceneMaterials/floor.png",

                SpecularColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
                Shininess = 1024,
            };

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Static = true,
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

            this.floor = this.AddComponent<Model>(desc);
        }
        private SceneObject<Model> InitializeSphere(string name, MaterialContent material)
        {
            Vector3[] v = null;
            Vector3[] n = null;
            Vector2[] uv = null;
            uint[] ix = null;
            GeometryUtil.CreateSphere(radius, (uint)stacks, (uint)stacks, out v, out n, out uv, out ix);

            VertexData[] vertices = new VertexData[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                vertices[i] = new VertexData()
                {
                    Position = v[i],
                    Normal = n[i],
                    Texture = uv[i],
                };
            }

            var content = ModelContent.GenerateTriangleList(vertices, ix, material);

            var desc = new ModelDescription()
            {
                Name = name,
                Static = true,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            return this.AddComponent<Model>(desc);
        }
        private MaterialContent GenerateMaterial(Color4 diffuse, Color4 specular, float shininess, bool nmap)
        {
            return new MaterialContent()
            {
                EmissionColor = new Color4(0f, 0f, 0f, 0f),
                AmbientColor = new Color4(0.8f, 0.8f, 0.8f, 1f),

                DiffuseColor = diffuse,
                DiffuseTexture = "SceneMaterials/white.png",
                NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png",

                SpecularColor = specular,
                Shininess = shininess,
            };
        }
        private void InitializeColorGroup(float shininess, float specularFactor, Vector3 position, bool nmap)
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

                        var material = this.GenerateMaterial(diffuse, specular, shininess, nmap);
                        var model = this.InitializeSphere(string.Format("Sphere {0}.{1}.{2}", r, g, b), material);
                        model.Transform.SetPosition(new Vector3(r * f, (g * f) + 1f, b * f) + position);
                    }
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            if (this.Game.Input.KeyJustReleased(Keys.Tab))
            {
                this.Game.SetScene<SceneStencilPass>();
            }

            base.Update(gameTime);

            this.runtime.Instance.Text = this.Game.RuntimeText;
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

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
