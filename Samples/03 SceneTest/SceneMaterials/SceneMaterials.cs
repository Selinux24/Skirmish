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

        private Model lightEmitter = null;
        private SceneLightPoint movingLight = null;

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
            Camera.Goto(-20, 25, -40f);
            Camera.LookTo(0, 10, 0);

            Lights.DirectionalLights[0].CastShadow = false;

            GameEnvironment.Background = Color.CornflowerBlue;

            await LoadResourcesAsync(
                new[]
                {
                    InitializeTextBoxes(),
                    InitializeSkyEffects(),
                    InitializeFloor(),
                    InitializeEmitter(),
                    InitializeColorGroup("Spheres soft", 1, 0.1f, new Vector3(-10, 0, -10), false),
                    InitializeColorGroup("Spheres rought", 128, 1f, new Vector3(-10.5f, 0, -10), true)
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    PrepareScene();

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
            float t = spaceSize / 5f;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, t) },
                new VertexData{ Position = new Vector3(+l, -h, -l), Normal = Vector3.Up, Texture = new Vector2(t, 0.0f) },
                new VertexData{ Position = new Vector3(+l, -h, +l), Normal = Vector3.Up, Texture = new Vector2(t, t) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            MaterialCookTorranceContent mat = MaterialCookTorranceContent.Default;
            mat.F0 = 0.9f;
            mat.Roughness = 0.2f;
            mat.K = 0.1f;
            mat.DiffuseTexture = "SceneMaterials/corrugated_d.jpg";
            mat.NormalMapTexture = "SceneMaterials/corrugated_n.jpg";

            var desc = new ModelDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            await this.AddComponentModel("Floor", desc);
        }
        private async Task InitializeEmitter()
        {
            MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
            mat.EmissiveColor = Color3.White;

            var sphere = GeometryUtil.CreateSphere(0.25f, 32, 32);

            var desc = new ModelDescription()
            {
                CastShadow = false,
                DeferredEnabled = true,
                DepthEnabled = true,
                Content = ContentDescription.FromContentData(sphere, mat),
            };

            lightEmitter = await this.AddComponentModel("Emitter", desc);
        }
        private async Task<ModelInstanced> InitializeSphereInstanced(string name, int count, IEnumerable<IMaterialContent> materials)
        {
            var sphere = GeometryUtil.CreateSphere(radius, stacks, stacks);
            var vertices = VertexData.FromDescriptor(sphere);
            var indices = sphere.Indices;

            var desc = new ModelInstancedDescription()
            {
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Instances = count,
                Content = ContentDescription.FromContentData(vertices, indices, materials),
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
            int e = colorCount / n;
            int totalSpheres = (int)Math.Pow(e, 3);
            float distance = 3f;

            List<IMaterialContent> materials = new List<IMaterialContent>();
            for (int r = 0; r < colorCount; r += n)
            {
                for (int g = 0; g < colorCount; g += n)
                {
                    for (int b = 0; b < colorCount; b += n)
                    {
                        var diffuse = new Color3(r / (float)colorCount, g / (float)colorCount, b / (float)colorCount);
                        var specular = new Color3(r / (float)colorCount * specularFactor, g / (float)colorCount * specularFactor, b / (float)colorCount * specularFactor);

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
                        float f = 1f / n * distance;

                        var instance = modelInstanced[instanceIndex++];
                        instance.Manipulator.SetPosition(new Vector3(r * f, (g * f) + 1f, b * f) + position);
                    }
                }
            }
        }
        private IMaterialContent GenerateMaterial(Color3 diffuse, Color3 specular, float shininess, bool nmap)
        {
            MaterialCookTorranceContent mat = MaterialCookTorranceContent.Default;
            mat.DiffuseColor = new Color4(diffuse, 1f);
            mat.DiffuseTexture = "SceneMaterials/white.png";
            mat.NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png";
            mat.SpecularColor = specular;
            mat.F0 = 0.8f;
            mat.Roughness = 0.1f;
            mat.K = 0.2f;

            return mat;
        }

        private void PrepareScene()
        {
            movingLight = new SceneLightPoint(
                "Moving fire light",
                false,
                Color.Yellow * 1.25f,
                Color.White,
                true,
                SceneLightPointDescription.Create(Vector3.Zero, 15f, 20f));

            Lights.Add(movingLight);
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
            UpdateLight(gameTime);

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
        private void UpdateLight(GameTime gameTime)
        {
            float r = 35f;
            float d = 0.5f;
            float v = 0.8f;
            float totalSeconds = gameTime.TotalSeconds;

            Vector3 position = Vector3.Zero;
            position.X = r * d * (float)Math.Cos(v * totalSeconds);
            position.Y = 5f + (2f * (1f + (float)Math.Sin(totalSeconds)));
            position.Z = r * d * (float)Math.Sin(v * totalSeconds);

            lightEmitter.Manipulator.SetPosition(position);
            movingLight.Position = position;
        }
    }
}
