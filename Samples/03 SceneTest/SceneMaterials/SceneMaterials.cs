using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SceneTest.SceneMaterials
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
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(-20, 10, -40f);
            Camera.LookTo(0, 0, 0);

            Lights.DirectionalLights[0].CastShadow = false;

            GameEnvironment.Background = Color.CornflowerBlue;

            await LoadResourcesAsync(
                new[]
                {
                    InitializeTextBoxes(),
                    InitializeSkyEffects(),
                    InitializeFloor(),
                    InitializeColorGroup("Spheres soft", 1, 0.1f, new Vector3(-10, 0, -10), false),
                    InitializeColorGroup("Spheres rought", 128, 1f, new Vector3(-10.5f, 0, -10), true)
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    gameReady = true;
                });
        }

        private async Task InitializeTextBoxes()
        {
            title = await this.AddComponentUITextArea("Title", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18), TextForeColor = Color.White, TextShadowColor = Color.Orange }, layerHUD);
            runtime = await this.AddComponentUITextArea("Runtime", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 10), TextForeColor = Color.Yellow, TextShadowColor = Color.Orange }, layerHUD);

            title.Text = "Scene Test - Materials";
            runtime.Text = "";

            title.SetPosition(Vector2.Zero);
            runtime.SetPosition(new Vector2(5, title.Top + title.Height + 3));

            var spDesc = new SpriteDescription()
            {
                Width = Game.Form.RenderWidth,
                Height = runtime.Top + runtime.Height + 3,
                BaseColor = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite("Backpanel", spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeSkyEffects()
        {
            await this.AddComponentLensFlare("LensFlare", new LensFlareDescription()
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
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            await this.AddComponentModel("Floor", desc);
        }
        private async Task<ModelInstanced> InitializeSphereInstanced(string name, int count, IEnumerable<MaterialContent> materials)
        {
            var sphere = GeometryUtil.CreateSphere(radius, stacks, stacks);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;
            var content = ModelContent.GenerateTriangleList(vertices, indices, materials);

            var desc = new ModelInstancedDescription()
            {
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Instances = count,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            var model = await this.AddComponentModelInstanced(name, desc);

            for (int i = 0; i < count; i++)
            {
                model[i].MaterialIndex = (uint)i;
            }

            return model;
        }
        private async Task InitializeColorGroup(string name, float shininess, float specularFactor, Vector3 position, bool nmap)
        {
            int n = 32;
            int colorCount = 256;
            int totalSpheres = (int)Math.Pow(colorCount / n, 3);

            List<MaterialContent> materials = new List<MaterialContent>();
            for (int r = 0; r < colorCount; r += n)
            {
                for (int g = 0; g < colorCount; g += n)
                {
                    for (int b = 0; b < colorCount; b += n)
                    {
                        var diffuse = new Color4(r / (float)colorCount, g / (float)colorCount, b / (float)colorCount, 1);
                        var specular = new Color4(r / (float)colorCount * specularFactor, g / (float)colorCount * specularFactor, b / (float)colorCount * specularFactor, 1f);

                        materials.Add(GenerateMaterial(diffuse, specular, shininess, nmap));
                    }
                }
            }

            var modelInstanced = await InitializeSphereInstanced(name, totalSpheres, materials);

            int instanceIndex = 0;
            for (int r = 0; r < colorCount; r += n)
            {
                for (int g = 0; g < colorCount; g += n)
                {
                    for (int b = 0; b < colorCount; b += n)
                    {
                        float f = 1f / n * 4f;

                        var instance = modelInstanced[instanceIndex++];
                        instance.Manipulator.SetPosition(new Vector3(r * f, (g * f) + 1f, b * f) + position);
                    }
                }
            }
        }
        private MaterialContent GenerateMaterial(Color4 diffuse, Color4 specular, float shininess, bool nmap)
        {
            return new MaterialContent()
            {
                EmissionColor = new Color4(0f, 0f, 0f, 0f),
                AmbientColor = new Color4(0.02f, 0.02f, 0.02f, 1f),

                DiffuseColor = diffuse,
                DiffuseTexture = "SceneMaterials/white.png",
                NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png",

                SpecularColor = specular,
                Shininess = shininess,
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.SceneStart>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!gameReady)
            {
                return;
            }

            UpdateCamera(gameTime);

            base.Update(gameTime);

            runtime.Text = Game.RuntimeText;
        }

        private void UpdateCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }
        }
    }
}
