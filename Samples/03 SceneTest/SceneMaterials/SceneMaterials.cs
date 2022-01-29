using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTest.SceneMaterials
{
    public class SceneMaterials : Scene
    {
        private readonly float spaceSize = 40;
        private readonly float radius = 1;
        private readonly uint stacks = 40;

        private Sprite backpanel = null;
        private UITextArea title = null;
        private UITextArea runtime = null;

        private ModelInstanced spheres1 = null;
        private ModelInstanced spheres2 = null;
        private Model lightEmitter1 = null;
        private Model lightEmitter2 = null;
        private SceneLightPoint movingLight1 = null;
        private SceneLightPoint movingLight2 = null;

        private uint currentAlgorithm = (uint)SpecularAlgorithms.Phong;
        private readonly uint algorithmCount = 3;

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
            Camera.Goto(20, 25, 40f);
            Camera.LookTo(0, 10, 0);

            Lights.KeyLight.CastShadow = false;

            GameEnvironment.Background = Color.CornflowerBlue;

            await LoadResourcesAsync(
                new[]
                {
                    InitializeTextBoxes(),
                    InitializeSkyEffects(),
                    InitializeFloor(),
                    InitializeEmitters(),
                    InitializeColorGroups($"Spheres {(SpecularAlgorithms)currentAlgorithm}"),
                    InitializeBuiltInList(),
                    InitializeMetallicList(),
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    UpdateLayout();
                    PrepareScene();

                    gameReady = true;
                });
        }

        private async Task InitializeTextBoxes()
        {
            var spDesc = new SpriteDescription()
            {
                BaseColor = new Color4(0, 0, 0, 0.75f),
            };
            backpanel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", spDesc, LayerUI - 1);

            var defaultFont18 = TextDrawerDescription.FromFamily("Arial", 18);
            var defaultFont10 = TextDrawerDescription.FromFamily("Arial", 10);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White, TextShadowColor = Color.Orange });
            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("Runtime", "Runtime", new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.Yellow, TextShadowColor = Color.Orange });

            title.Text = "Scene Test - Materials";
            runtime.Text = "";
        }
        private async Task InitializeSkyEffects()
        {
            await AddComponentEffect<LensFlare, LensFlareDescription>("LensFlare", "LensFlare", new LensFlareDescription()
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
            mat.Metallic = 0.9f;
            mat.Roughness = 0.1f;
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

            await AddComponent<Model, ModelDescription>("Floor", "Floor", desc);
        }
        private async Task InitializeEmitters()
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

            lightEmitter1 = await AddComponent<Model, ModelDescription>("Emitter1", "Emitter1", desc);
            lightEmitter2 = await AddComponent<Model, ModelDescription>("Emitter2", "Emitter2", desc);
        }
        private async Task InitializeColorGroups(string name)
        {
            int n = 32;
            int colorCount = 256;
            int e = colorCount / n;
            int totalSpheres = (int)Math.Pow(e, 3);
            float distance = 3f;

            var mapParams = new MaterialParams
            {
                Algorithm = (SpecularAlgorithms)currentAlgorithm,
                Shininess = 32,
                Metallic = 1f,
                Roughness = 0.05f
            };
            var materials1 = GenerateMaterials(n, colorCount, 0.1f, mapParams, true);
            var materials2 = GenerateMaterials(n, colorCount, 0.1f, mapParams, false);

            spheres1 = await InitializeSphereInstanced($"{name}1", totalSpheres, materials1);
            spheres2 = await InitializeSphereInstanced($"{name}2", totalSpheres, materials2);

            var position1 = new Vector3(-10, 0, -10);
            var position2 = new Vector3(-10.5f, 0, -10);

            SetSpheresPosition(colorCount, n, spheres1, position1, distance);
            SetSpheresPosition(colorCount, n, spheres2, position2, distance);
        }
        private async Task InitializeBuiltInList()
        {
            List<BuiltInMaterial> materials = new List<BuiltInMaterial>()
            {
                BuiltInMaterials.Emerald,
                BuiltInMaterials.Jade,
                BuiltInMaterials.Obsidian,
                BuiltInMaterials.Pearl,
                BuiltInMaterials.Ruby,
                BuiltInMaterials.Turquoise,
                BuiltInMaterials.Brass,
                BuiltInMaterials.Bronze,
                BuiltInMaterials.Chrome,
                BuiltInMaterials.Copper,
                BuiltInMaterials.Gold,
                BuiltInMaterials.Silver,
                BuiltInMaterials.BlackPlastic,
                BuiltInMaterials.CyanPlastic,
                BuiltInMaterials.GreenPlastic,
                BuiltInMaterials.RedPlastic,
                BuiltInMaterials.WhitePlastic,
                BuiltInMaterials.YellowPlastic,
                BuiltInMaterials.BlackRubber,
                BuiltInMaterials.CyanRubber,
                BuiltInMaterials.GreenRubber,
                BuiltInMaterials.RedRubber,
                BuiltInMaterials.WhiteRubber,
                BuiltInMaterials.YellowRubber,
            };

            //Cook-torrance
            var ctMaterials = materials.Select(m => (MaterialCookTorranceContent)m).OfType<IMaterialContent>();

            var spheres = await InitializeSphereInstanced("Built-in materials test", materials.Count, ctMaterials);

            float sep = 2f;
            float x = sep * spheres.InstanceCount * 0.5f;
            int index = 0;
            foreach (var item in spheres.GetInstances())
            {
                item.Manipulator.SetPosition(x - (sep * index++), 5, 20);
            }
        }
        private async Task InitializeMetallicList()
        {
            int itemsPerRow = 10;

            List<IMaterialContent> materials = new List<IMaterialContent>();

            float roughness = 0.1f;
            for (int i = 0; i < itemsPerRow; i++)
            {
                var mat = MaterialCookTorranceContent.Default;
                mat.DiffuseColor = Color.Gold;
                mat.SpecularColor = (Color.Gold * 1.25f).RGB();
                mat.Metallic = 0.9f;
                mat.Roughness = roughness * (i + 1);
                mat.DiffuseTexture = "SceneMaterials/white.png";
                mat.NormalMapTexture = "SceneMaterials/nmap2.png";

                materials.Add(mat);
            }

            float metalness = 0.1f;
            for (int i = 0; i < itemsPerRow; i++)
            {
                var mat = MaterialCookTorranceContent.Default;
                mat.DiffuseColor = Color.Gold;
                mat.SpecularColor = (Color.Gold * 1.25f).RGB();
                mat.Metallic = metalness * (i + 1);
                mat.Roughness = 0.1f;
                mat.DiffuseTexture = "SceneMaterials/white.png";
                mat.NormalMapTexture = "SceneMaterials/nmap2.png";

                materials.Add(mat);
            }

            var spheres = await InitializeSphereInstanced("Cook-Torrance Metallics", materials.Count, materials);

            float sep = 2f;
            float x = sep * itemsPerRow * 0.5f;
            float y = 8f;
            int index = 0;
            foreach (var item in spheres.GetInstances())
            {
                item.Manipulator.SetPosition(x - (sep * index++), y, 20);
                if (index >= itemsPerRow)
                {
                    index = 0;
                    y += 2.2f;
                }
            }
        }
        private IEnumerable<IMaterialContent> GenerateMaterials(int n, int colorCount, float specularFactor, MaterialParams matParams, bool nmap)
        {
            List<IMaterialContent> materials = new List<IMaterialContent>();

            for (int r = 0; r < colorCount; r += n)
            {
                for (int g = 0; g < colorCount; g += n)
                {
                    for (int b = 0; b < colorCount; b += n)
                    {
                        var diffuse = new Color3(r / (float)colorCount, g / (float)colorCount, b / (float)colorCount);
                        var specular = diffuse + new Color3(specularFactor);
                        specular = Color3.AdjustSaturation(specular, 1f);

                        materials.Add(GenerateMaterial(diffuse, specular, matParams, nmap));
                    }
                }
            }

            return materials;
        }
        private IMaterialContent GenerateMaterial(Color3 diffuse, Color3 specular, MaterialParams matParams, bool nmap)
        {
            if (matParams.Algorithm == SpecularAlgorithms.Phong)
            {
                MaterialPhongContent mat = MaterialPhongContent.Default;
                mat.DiffuseColor = new Color4(diffuse, 1f);
                mat.DiffuseTexture = "SceneMaterials/white.png";
                mat.NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png";
                mat.SpecularColor = specular;
                mat.Shininess = matParams.Shininess;
                return mat;
            }
            else if (matParams.Algorithm == SpecularAlgorithms.BlinnPhong)
            {
                MaterialBlinnPhongContent mat = MaterialBlinnPhongContent.Default;
                mat.DiffuseColor = new Color4(diffuse, 1f);
                mat.DiffuseTexture = "SceneMaterials/white.png";
                mat.NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png";
                mat.SpecularColor = specular;
                mat.Shininess = matParams.Shininess;
                return mat;
            }
            else if (matParams.Algorithm == SpecularAlgorithms.CookTorrance)
            {
                MaterialCookTorranceContent mat = MaterialCookTorranceContent.Default;
                mat.DiffuseColor = new Color4(diffuse, 1f);
                mat.DiffuseTexture = "SceneMaterials/white.png";
                mat.NormalMapTexture = nmap ? "SceneMaterials/nmap1.jpg" : "SceneMaterials/nmap2.png";
                mat.SpecularColor = specular;
                mat.Metallic = matParams.Metallic;
                mat.Roughness = matParams.Roughness;
                return mat;
            }

            return MaterialBlinnPhongContent.Default;
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

            var model = await AddComponent<ModelInstanced, ModelInstancedDescription>(name, name, desc);

            for (int i = 0; i < count; i++)
            {
                model[i].MaterialIndex = (uint)i;
            }

            return model;
        }
        private void SetSpheresPosition(int colorCount, int n, ModelInstanced spheres, Vector3 position, float distance)
        {
            int instanceIndex = 0;
            for (int r = 0; r < colorCount; r += n)
            {
                for (int g = 0; g < colorCount; g += n)
                {
                    for (int b = 0; b < colorCount; b += n)
                    {
                        float f = 1f / n * distance;

                        var instance = spheres[instanceIndex++];
                        instance.Manipulator.SetPosition(new Vector3(r * f, (g * f) + 1f, b * f) + position);
                    }
                }
            }
        }

        private void PrepareScene()
        {
            movingLight1 = new SceneLightPoint(
                "Emitter 1 light",
                false,
                Color.Yellow.RGB() * 1.25f,
                Color3.White,
                true,
                SceneLightPointDescription.Create(Vector3.Zero, 15f, 20f));

            Lights.Add(movingLight1);

            movingLight2 = new SceneLightPoint(
                "Emitter 2 light",
                false,
                Color3.White,
                Color3.White,
                true,
                SceneLightPointDescription.Create(Vector3.Zero, 10f, 5f));

            Lights.Add(movingLight2);
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
            UpdateInput();

            base.Update(gameTime);

            runtime.Text = Game.RuntimeText;
        }

        private void UpdateCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
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
            float totalSeconds = gameTime.TotalSeconds;

            float r1 = 35f;
            float d1 = 0.5f;
            float v1 = 0.8f;
            Vector3 position1 = Vector3.Zero;
            position1.X = r1 * d1 * (float)Math.Cos(v1 * totalSeconds);
            position1.Y = 5f + (2f * (1f + (float)Math.Sin(totalSeconds)));
            position1.Z = r1 * d1 * (float)Math.Sin(v1 * totalSeconds);

            lightEmitter1.Manipulator.SetPosition(position1);
            movingLight1.Position = position1;

            float r2 = 25f;
            float d2 = 0.5f;
            float v2 = 0.6f;
            Vector3 position2 = Vector3.Zero;
            position2.X = r2 * d2 * (float)Math.Cos(v2 * totalSeconds);
            position2.Y = 8f;
            position2.Z = 24;

            lightEmitter2.Manipulator.SetPosition(position2);
            movingLight2.Position = position2;
        }
        private void UpdateInput()
        {
            if (Game.Input.KeyJustReleased(Keys.Tab))
            {
                spheres1.Active = false;
                spheres2.Active = false;

                Task.Run(async () =>
                {
                    RemoveComponent(spheres1);
                    RemoveComponent(spheres2);

                    currentAlgorithm++;
                    currentAlgorithm %= algorithmCount;

                    await LoadResourcesAsync(new Task[]
                    {
                        InitializeColorGroups($"Spheres {(SpecularAlgorithms)currentAlgorithm}"),
                    });
                });
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            runtime.SetPosition(new Vector2(5, title.Top + title.Height + 3));

            backpanel.Width = Game.Form.RenderWidth;
            backpanel.Height = runtime.Top + runtime.Height + 3;
        }
    }

    struct MaterialParams
    {
        public SpecularAlgorithms Algorithm;
        public float Shininess;
        public float Metallic;
        public float Roughness;
    }
}
