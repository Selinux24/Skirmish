using Engine;
using Engine.Animation;
using Engine.BuiltIn.Components.Foliage;
using Engine.BuiltIn.Components.Ground;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.BuiltIn.Components.Skies;
using Engine.BuiltIn.Drawers.PostProcess;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntermediateSamples.SceneDeferredLights
{
    /// <summary>
    /// Deferred lights scene test
    /// </summary>
    public class DeferredLightsScene : Scene
    {
        private const string titleMask = "{0}: {1} directionals, {2} points and {3} spots. Shadows {4}";

        private const string resourcesLocalFolder = "SceneDeferredLights/Resources/";
        private const string resourcesCursor = resourcesLocalFolder + "target.png";
        private const string resourcesSkydom = resourcesLocalFolder + "sunset.dds";

        private const string resourcesM24Folder = "Common/M24/";
        private const string resourcesM24File = "m24.json";

        private const string resourcesTerrainFolder = "Common/Terrain/";
        private const string resourcesTerrainFile = "terrain.json";

        private const string resourcesTreeFolder = "Common/trees/";
        private const string resourcesBirchAFile = "birch_a.json";
        private const string resourcesBirchBFile = "birch_b.json";

        private const string resourcesVegetationFolder = resourcesLocalFolder + "Vegetation/";
        private const string resourcesGrassFile = "grass.png";

        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.10f;
        private const string titleText = "Deferred Ligthning test";
        private bool showHelp = false;
        private const string helpText1 = "Press H for Help";
        private const string helpText2 = @"H: Hide this help.
WASD Space/C: Move the camera.
F: Toggle fog.
G: Toggle shadows.
L: Toggle lights.
P: Toggle lights animation.
Up/Down/Left/Right/PageUp/PageDown: Move the spotlight.
Add/Subtract: Change the spotlight intensity.
Tab: Show frustum.
F1 to F5: View G-Buffer textures. 
F6: Maximize/minimize G-Buffer view. 
F7 to F10: Toggle object visibility.";

        private UITextArea title = null;
        private UITextArea help = null;
        private UITextArea bufferText = null;
        private Sprite upperPanel = null;

        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Scenery terrain = null;

        private Model tree = null;
        private ModelInstanced trees = null;

        private UITextureRenderer bufferDrawer = null;
        private bool bufferDrawerFullscreen = false;
        private SceneRendererResults bufferType = SceneRendererResults.None;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private PrimitiveListDrawer<Line3D> lineDrawer = null;

        private bool onlyModels = true;

        private readonly Dictionary<string, AnimationPlan> animations = [];

        private bool uiReady = false;
        private bool gameReady = false;

        private readonly BuiltInPostProcessState postProcessingState = BuiltInPostProcessState.Empty;

        public DeferredLightsScene(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif
        }

        public override void Initialize()
        {
            base.Initialize();

            LoadingTaskUI();
        }

        private void LoadingTaskUI()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeCursor,
                    InitializeUIComponents
                ],
                LoadingTaskUICompleted);

            LoadResources(group);
        }
        private async Task InitializeCursor()
        {
            var desc = UICursorDescription.Default(resourcesCursor, 15, 15, true);

            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", desc);
        }
        private async Task InitializeUIComponents()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            var defaultFont10 = TextDrawerDescription.FromFamily("Tahoma", 10);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;
            defaultFont10.LineAdjust = true;

            var dTitle = new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White, StartsVisible = false, Text = titleText };
            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", dTitle);

            var dBufferText = new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow, StartsVisible = false };
            bufferText = await AddComponentUI<UITextArea, UITextAreaDescription>("BufferText", "BufferText", dBufferText);

            var dHelpText = new UITextAreaDescription { Font = defaultFont10, TextForeColor = Color.Yellow, StartsVisible = false, Text = helpText1, MaxTextLength = 512 };
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", dHelpText);

            var upperPanelDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            upperPanel = await AddComponentUI<Sprite, SpriteDescription>("Upperpanel", "Upperpanel", upperPanelDesc, LayerUI - 1);

            var bufferDwDesc = UITextureRendererDescription.Default();
            bufferDwDesc.StartsVisible = false;
            bufferDrawer = await AddComponentUI<UITextureRenderer, UITextureRendererDescription>("DebugBuferDrawer", "DebugBuferDrawer", bufferDwDesc, LayerUI - 1);
        }
        private void LoadingTaskUICompleted(LoadResourcesResult res)
        {
            res.ThrowExceptions();

            title.Visible = true;
            help.Visible = true;
            bufferText.Visible = true;

            UpdateLayout();

            LoadingTaskObjects();

            uiReady = true;
        }

        private void LoadingTaskObjects()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeSkydom,
                    InitializeHelicopters,
                    InitializeTerrain,
                    InitializeGardener,
                    InitializeTrees,
                    InitializeDebug,
                ],
                LoadingTaskObjectsCompleted);

            LoadResources(group);
        }
        private async Task InitializeSkydom()
        {
            var desc = SkydomDescription.Sphere(resourcesSkydom, far);

            await AddComponentSky<Skydom, SkydomDescription>("Sky", "Sky", desc);
        }
        private async Task InitializeHelicopters()
        {
            var desc1 = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                TextureIndex = 2,
                Content = ContentDescription.FromFile(resourcesM24Folder, resourcesM24File),
                StartsVisible = false,
            };
            helicopter = await AddComponent<Model, ModelDescription>("Helicopter", "Helicopter", desc1);
            Lights.AddRange(helicopter.Lights);

            var desc2 = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Instances = 2,
                Content = ContentDescription.FromFile(resourcesM24Folder, resourcesM24File),
                StartsVisible = false,
            };
            helicopters = await AddComponent<ModelInstanced, ModelInstancedDescription>("Bunch of Helicopters", "Bunch of Helicopters", desc2);
            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                Lights.AddRange(helicopters[i].Lights);
            }
        }
        private async Task InitializeTerrain()
        {
            var desc = SceneryDescription.FromFile(resourcesTerrainFolder, resourcesTerrainFile, 2);

            terrain = await AddComponentGround<Scenery, SceneryDescription>("Terrain", "Terrain", desc);
        }
        private async Task InitializeTrees()
        {
            var desc1 = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.OpaqueTransparent,
                Content = ContentDescription.FromFile(resourcesTreeFolder, resourcesBirchAFile),
                StartsVisible = false,
            };
            tree = await AddComponentGround<Model, ModelDescription>("Lonely tree", "Lonely tree", desc1);

            var desc2 = new ModelInstancedDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                BlendMode = BlendModes.OpaqueTransparent,
                Instances = 12,
                Content = ContentDescription.FromFile(resourcesTreeFolder, resourcesBirchBFile),
                StartsVisible = false,
            };
            trees = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Bunch of trees", "Bunch of trees", desc2);
        }
        private async Task InitializeGardener()
        {
            var desc = new FoliageDescription()
            {
                ContentPath = resourcesVegetationFolder,
                ChannelRed = new()
                {
                    VegetationTextures = [resourcesGrassFile],
                    Density = 20f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = Vector2.One * 0.20f,
                    MaxSize = Vector2.One * 0.25f,
                },
                StartsVisible = false,
            };
            await AddComponentEffect<Foliage, FoliageDescription>("Vegetation", "Vegetation", desc);
        }
        private async Task InitializeDebug()
        {
            var lineDrawerDesc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 1000,
                StartsVisible = false,
            };
            lineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DEBUG++ Lines", "DEBUG++ Lines", lineDrawerDesc);
        }
        private void LoadingTaskObjectsCompleted(LoadResourcesResult res)
        {
            res.ThrowExceptions();

            postProcessingState.AddToneMapping(BuiltInToneMappingTones.Uncharted2);
            Renderer.ClearPostProcessingEffects();
            Renderer.PostProcessingObjectsEffects = postProcessingState;

            Lights.KeyLight.Enabled = false;
            Lights.KeyLight.CastShadow = false;

            Lights.BackLight.Enabled = false;
            Lights.BackLight.CastShadow = false;

            Lights.FillLight.Enabled = true;
            Lights.FillLight.CastShadow = true;
            Lights.FillLight.DiffuseColor = new Color3(0.8f, 0.9f, 1);
            Lights.FillLight.SpecularColor = new Color3(0.4f, 0.5f, 1);
            Lights.FillLight.Brightness = 0.2f;

            StartAnimations();

            StartTerrain();

            StartItems(out Vector3 cameraPosition, out int modelCount);

            cameraPosition /= modelCount;
            Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            Camera.LookTo(cameraPosition + Vector3.Up);
            Camera.NearPlaneDistance = near;
            Camera.FarPlaneDistance = far;

            gameReady = true;
        }
        private void StartAnimations()
        {
            var ap = new AnimationPath();
            ap.AddLoop("roll");
            animations.Add("default", new AnimationPlan(ap));
        }
        private void StartTerrain()
        {
            if (FindTopGroundPosition<Triangle>(20, -20, out var treePos))
            {
                tree.Manipulator.SetTransform(treePos.Position, Quaternion.Identity, 0.5f);
            }

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                float x = (i * 10) - 55;
                float z = 17 + (i % 2 == 0 ? i : -i);

                if (FindTopGroundPosition<Triangle>(x, z, out var pos))
                {
                    trees[i].Manipulator.SetTransform(pos.Position, i, 0, 0, 0.5f);
                }
            }

            tree.Visible = true;
            trees.Visible = true;
        }
        private void StartItems(out Vector3 cameraPosition, out int modelCount)
        {
            cameraPosition = Vector3.Zero;
            modelCount = 0;

            if (FindTopGroundPosition<Triangle>(20, -20, out var hPos))
            {
                var p = hPos.Position;
                p.Y += 10f;
                helicopter.Manipulator.SetPosition(p);
                helicopter.Manipulator.SetScaling(0.15f);
                cameraPosition += p;
                modelCount++;
            }

            helicopter.AnimationController.Start(animations["default"]);
            helicopter.AnimationController.TimeDelta = 3f;

            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                if (FindTopGroundPosition<Triangle>((i * 10) - 20, 20, out var r))
                {
                    var p = r.Position;
                    p.Y += 10f;
                    helicopters[i].Manipulator.SetPosition(p);
                    helicopters[i].Manipulator.SetScaling(0.15f);
                    cameraPosition += p;
                    modelCount++;
                }

                helicopters[i].AnimationController.Start(animations["default"]);
                helicopters[i].AnimationController.TimeDelta = 3f;
            }

            helicopter.Visible = true;
            helicopters.Visible = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
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

            if (Game.Input.KeyJustReleased(Keys.H))
            {
                showHelp = !showHelp;

                help.Text = showHelp ? helpText2 : helpText1;
            }

            UpdateInput(gameTime);

            UpdateState(gameTime);
        }
        private void UpdateInput(IGameTime gameTime)
        {
            UpdateInputCamera(gameTime);
            UpdayeInputLights();
            UpdateInputObjectsVisibility();
            UpdateInputSpotlight(gameTime);
            UpdateInputDeferredMap();
        }
        private void UpdateInputCamera(IGameTime gameTime)
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

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Tab))
            {
                lineDrawer.SetPrimitives(Color.Yellow, Line3D.CreateFrustum(Camera.Frustum));
                lineDrawer.Visible = true;
            }
        }
        private void UpdayeInputLights()
        {
            if (Game.Input.KeyJustReleased(Keys.F))
            {
                Lights.BaseFogColor = new Color((byte)54, (byte)56, (byte)68);
                Lights.FogStart = MathUtil.IsZero(Lights.FogStart) ? far * fogStart : 0f;
                Lights.FogRange = MathUtil.IsZero(Lights.FogRange) ? far * fogRange : 0f;
            }

            if (Game.Input.KeyJustReleased(Keys.G))
            {
                Lights.DirectionalLights[0].CastShadow = !Lights.DirectionalLights[0].CastShadow;
            }

            if (Game.Input.KeyJustReleased(Keys.L))
            {
                onlyModels = !onlyModels;

                CreateLights(onlyModels, !Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyJustReleased(Keys.P))
            {
                animateLights = !animateLights;
            }
        }
        private void UpdateInputDeferredMap()
        {
            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                UpdateDebugColorMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                UpdateDebugNormalMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                UpdateDebugDepthMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F4))
            {
                UpdateDebugShadowMap(Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                UpdateDebugLightMap();
            }

            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                UpdateDebugBufferDrawer();
            }
        }
        private void UpdateInputObjectsVisibility()
        {
            if (Game.Input.KeyJustReleased(Keys.F7))
            {
                bufferDrawer.Visible = !bufferDrawer.Visible;
                bufferText.Visible = bufferDrawer.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F8))
            {
                terrain.Active = terrain.Visible = !terrain.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F9))
            {
                helicopter.Active = helicopter.Visible = !helicopter.Visible;
            }

            if (Game.Input.KeyJustReleased(Keys.F10))
            {
                helicopters.Active = helicopters.Visible = !helicopters.Visible;
            }
        }
        private void UpdateInputSpotlight(IGameTime gameTime)
        {
            if (spotLight == null)
            {
                return;
            }

            if (Game.Input.KeyPressed(Keys.Left))
            {
                var v = Camera.Left;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += v * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                var v = Camera.Right;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += v * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Up))
            {
                var v = Camera.Forward;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += v * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Down))
            {
                var v = Camera.Backward;
                v.Y = 0;
                v.Normalize();
                spotLight.Position += v * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.PageUp))
            {
                spotLight.Position += Vector3.Up * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.PageDown))
            {
                spotLight.Position += Vector3.Down * gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Add))
            {
                spotLight.Intensity += gameTime.ElapsedSeconds * 10f;
            }

            if (Game.Input.KeyPressed(Keys.Subtract))
            {
                spotLight.Intensity -= gameTime.ElapsedSeconds * 10f;

                spotLight.Intensity = MathF.Max(0f, spotLight.Intensity);
            }
        }

        private void UpdateState(IGameTime gameTime)
        {
            UpdateLights(gameTime);
        }
        private void UpdateLights(IGameTime gameTime)
        {
            if (spotLight != null)
            {
                lineDrawer.SetPrimitives(Color.White, spotLight.GetVolume(10));
            }
            else
            {
                lineDrawer.Clear(Color.White);
            }

            if (animateLights && Lights.PointLights.Length > 0)
            {
                UpdatePointLightsAnimation(gameTime);
            }
        }
        private void UpdatePointLightsAnimation(IGameTime gameTime)
        {
            for (int i = 1; i < Lights.PointLights.Length; i++)
            {
                var l = Lights.PointLights[i];

                if ((int?)l.State == 1) l.Radius += 0.5f * gameTime.ElapsedSeconds * 50f;
                if ((int?)l.State == -1) l.Radius -= 2f * gameTime.ElapsedSeconds * 50f;

                if (l.Radius <= 0)
                {
                    l.Radius = 0;
                    l.State = 1;
                }

                if (l.Radius >= 50)
                {
                    l.Radius = 50;
                    l.State = -1;
                }

                l.Intensity = l.Radius * 0.1f;
            }
        }

        private void UpdateDebugMap()
        {
            if (bufferType == SceneRendererResults.None)
            {
                bufferDrawer.Texture = null;
            }
            else
            {
                bufferDrawer.Texture = Renderer.GetResource(bufferType);
            }
        }
        private void UpdateDebugColorMap()
        {
            bufferType = SceneRendererResults.ColorMap;

            var colorMap = Renderer.GetResource(bufferType);

            //Colors
            bufferDrawer.Texture = colorMap;
            bufferDrawer.Channel = ColorChannels.NoAlpha;
            bufferText.Text = "Colors";

            bufferDrawer.Visible = true;
        }
        private void UpdateDebugNormalMap()
        {
            bufferType = SceneRendererResults.NormalMap;

            var normalMap = Renderer.GetResource(bufferType);

            if (bufferDrawer.Texture == normalMap &&
                bufferDrawer.Channel == ColorChannels.NoAlpha)
            {
                //Specular Power
                bufferDrawer.Texture = normalMap;
                bufferDrawer.Channel = ColorChannels.Alpha;
                bufferText.Text = "Specular Power";
            }
            else
            {
                //Normals
                bufferDrawer.Texture = normalMap;
                bufferDrawer.Channel = ColorChannels.NoAlpha;
                bufferText.Text = "Normals";
            }
            bufferDrawer.Visible = true;
        }
        private void UpdateDebugDepthMap()
        {
            bufferType = SceneRendererResults.DepthMap;

            var depthMap = Renderer.GetResource(bufferType);

            if (bufferDrawer.Texture == depthMap &&
                bufferDrawer.Channel == ColorChannels.NoAlpha)
            {
                //Specular Factor
                bufferDrawer.Texture = depthMap;
                bufferDrawer.Channel = ColorChannels.Alpha;
                bufferText.Text = "Specular Intensity";
            }
            else
            {
                //Position
                bufferDrawer.Texture = depthMap;
                bufferDrawer.Channel = ColorChannels.NoAlpha;
                bufferText.Text = "Position";
            }
            bufferDrawer.Visible = true;
        }
        private void UpdateDebugShadowMap(bool shift)
        {
            bufferType = SceneRendererResults.ShadowMapDirectional;

            var shadowMap = Renderer.GetResource(bufferType);

            if (shadowMap != null)
            {
                //Shadow map
                if (!bufferText.Text.StartsWith("Shadow map"))
                {
                    bufferDrawer.Texture = shadowMap;
                    bufferDrawer.TextureIndex = 0;
                    bufferDrawer.Channel = ColorChannels.Red;
                    bufferDrawer.Visible = true;
                }
                else
                {
                    uint tIndex = bufferDrawer.TextureIndex;
                    if (!shift)
                    {
                        tIndex++;
                        tIndex %= 6;
                    }
                    else
                    {
                        tIndex--;
                        if (tIndex < 0)
                        {
                            tIndex = 5;
                        }
                    }

                    bufferDrawer.TextureIndex = tIndex;
                }

                bufferText.Text = string.Format("Shadow map {0}", bufferDrawer.TextureIndex);
            }
            else
            {
                bufferText.Text = "The Shadow map is empty";
            }
        }
        private void UpdateDebugLightMap()
        {
            bufferType = SceneRendererResults.LightMap;

            var lightMap = Renderer.GetResource(bufferType);

            if (lightMap != null)
            {
                //Light map
                bufferDrawer.Texture = lightMap;
                bufferDrawer.Channel = ColorChannels.NoAlpha;
                bufferDrawer.Visible = true;
                bufferText.Text = "Light map";
            }
            else
            {
                bufferText.Text = "The Light map is empty";
            }
        }
        private void UpdateDebugBufferDrawer()
        {
            bufferDrawerFullscreen = !bufferDrawerFullscreen;

            UpdateLayout();
        }

        public override void Draw(IGameTime gameTime)
        {
            base.Draw(gameTime);

            if (!uiReady)
            {
                return;
            }

            title.Text = string.Format(
                titleMask,
                GetRenderMode(),
                Lights.DirectionalLights.Length,
                Lights.PointLights.Length,
                Lights.SpotLights.Length,
                Lights.GetDirectionalShadowCastingLights(GameEnvironment).Count());
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();

            UpdateDebugMap();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            bufferText.SetPosition(new Vector2(0, title.Top + title.Height + 2));
            help.SetPosition(new Vector2(0, bufferText.Top + bufferText.Height + 2));

            upperPanel.Width = Game.Form.RenderWidth;
            upperPanel.Height = bufferText.Top + bufferText.Height + 3;

            if (bufferDrawerFullscreen)
            {
                bufferDrawer.Width = Game.Form.RenderWidth;
                bufferDrawer.Height = Game.Form.RenderHeight;
                bufferDrawer.Left = 0;
                bufferDrawer.Top = 0;
            }
            else
            {
                bufferDrawer.Width = (int)(Game.Form.RenderWidth * 0.33f);
                bufferDrawer.Height = (int)(Game.Form.RenderHeight * 0.33f);
                bufferDrawer.Left = Game.Form.RenderWidth - bufferDrawer.Width;
                bufferDrawer.Top = Game.Form.RenderHeight - bufferDrawer.Height;
            }
        }

        private void CreateLights(bool modelsOnly, bool castShadows)
        {
            Lights.ClearPointLights();
            Lights.ClearSpotLights();
            spotLight = null;

            Lights.AddRange(helicopter.Lights);
            for (int i = 0; i < helicopters.InstanceCount; i++)
            {
                Lights.AddRange(helicopters[i].Lights);
            }

            if (modelsOnly)
            {
                return;
            }

            SetLightsPosition(castShadows);

            int sep = 10;
            int f = 12;
            int l = (f - 1) * sep;
            l -= l / 2;

            for (int y = 0; y < f; y++)
            {
                for (int x = 0; x < f; x++)
                {
                    var lightPosition = new Vector3((y * sep) - l, 1f, (x * sep) - l);

                    if (FindTopGroundPosition((y * sep) - l, (x * sep) - l, out PickingResult<Triangle> r))
                    {
                        lightPosition = r.Position;
                        lightPosition.Y += 1f;
                    }

                    var color = new Color3(Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1), Helper.RandomGenerator.NextFloat(0, 1));

                    var pointLight = new SceneLightPoint(
                        $"Point {Lights.PointLights.Length}",
                        castShadows,
                        color,
                        color,
                        true,
                        SceneLightPointDescription.Create(lightPosition, 5f, 10f))
                    {
                        State = Helper.RandomGenerator.NextFloat(0, 1) >= 0.5f ? 1 : -1
                    };

                    Lights.Add(pointLight);
                }
            }
        }
        private void SetLightsPosition(bool castShadows)
        {
            if (!FindTopGroundPosition(0, 1, out PickingResult<Triangle> r))
            {
                return;
            }

            var lightPosition = r.Position;
            lightPosition.Y += 10f;

            spotLight = new SceneLightSpot(
                "Spot the dog",
                castShadows,
                Color.Yellow.RGB(),
                Color.Yellow.RGB(),
                true,
                SceneLightSpotDescription.Create(lightPosition, Vector3.Down, 25, 25, 25f));

            Lights.Add(spotLight);
        }
    }
}
