﻿using Engine;
using Engine.BuiltIn.Components.Decals;
using Engine.BuiltIn.Components.Ground;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Particles;
using Engine.BuiltIn.Components.Primitives;
using Engine.BuiltIn.Components.Skies;
using Engine.BuiltIn.Components.Water;
using Engine.BuiltIn.Drawers.PostProcess;
using Engine.Common;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using Engine.UI;
using SharpDX;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrainSamples.SceneSkybox
{
    public class SkyboxScene : WalkableScene
    {
        private const string resourceAudioFolder = "Common/Audio/Effects/";
        private const string resourceCursorFile = "Common/UI/Cursor/target.png";

        private const string resourceParticlesFolder = "Common/Effects/Particles/";
        private const string resourceParticlesFireFile = "fire.png";
        private const string resourceParticlesSmokeFile = "smoke.png";

        private const string resourceSkyboxFile = "Common/Skyboxes/Daylight Box UV.png";
        private const string resourceBulletFile = "Common/Effects/Bullets/bullet-hole.png";

        private const string resourceLakeBottomFolder = "Common/Surfaces/Dirt/";
        private const string resourceLakeBottomDiffuseFile = "Diffuse.jpg";
        private const string resourceLakeBottomNormalFile = "Normal.jpg";

        private const string resourceRuinsFolder = "Common/Buildings/Ruins/";
        private const string resourceRuinsFile = "ruins.json";
        private const string resourceFountainFolder = "Common/Buildings/Fountain/";
        private const string resourceFountainFile = "Fountain.json";
        private const string resourceObeliskFolder = "Common/Buildings/Obelisk2/";
        private const string resourceObeliskFile = "obelisk.json";
        private const string resourceTorchFolder = "Common/Props/Torch/";
        private const string resourceTorchFile = "torch.json";

        private const float alpha = 0.25f;

        private readonly Color4 ruinsVolumeColor = new(Color.Green.RGB(), alpha);
        private readonly Color4 torchVolumeColor = new(Color.GreenYellow.RGB(), alpha);
        private readonly Color4 obeliskVolumeColor = new(Color.DarkGreen.RGB(), alpha);
        private readonly Color4 fountainVolumeColor = new(Color.DarkSeaGreen.RGB(), alpha);
        private readonly int bsphSlices = 20;
        private readonly int bsphStacks = 10;
        private readonly Vector2[] firePositions =
        [
            new Vector2(+5, +5),
            new Vector2(-5, +5),
            new Vector2(+5, -5),
            new Vector2(-5, -5),
        ];
        private readonly Vector3[] obeliskPositions =
        [
            new Vector3(+100, -0.2f, +100),
            new Vector3(-100, -10, +100),
            new Vector3(+100, -1, -100),
            new Vector3(-100, -1, -100),
        ];
        private readonly Quaternion[] obeliskRotations =
        [
            Quaternion.RotationYawPitchRoll(-MathUtil.PiOverTwo * 0.75f, MathUtil.PiOverTwo*1.03f, 0.45f),
            Quaternion.RotationYawPitchRoll(0, -0.15f, 0),
            Quaternion.Identity,
            Quaternion.Identity,
        ];

        private readonly GraphAgentType walker = new()
        {
            Name = "Walker",
            Height = 1.7f,
            Radius = 0.4f,
            MaxClimb = 1.2f,
            MaxSlope = 45,
        };

        private SoundEffectsManager soundEffectsManager;

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea help = null;
        private UITextArea fps = null;

        private Skydom skydom = null;
        private Scenery lakeBottom = null;
        private Scenery ruins = null;
        private Water water = null;
        private PrimitiveListDrawer<Line3D> volumesDrawer = null;
        private PrimitiveListDrawer<Triangle> graphDrawer = null;

        private readonly int mapSize = 256;
        private readonly float terrainSize = 512;
        private readonly float terrainHeight = 20;

        private ModelInstanced torchs = null;
        private ModelInstanced obelisks = null;
        private Model fountain = null;

        private Model movingFire = null;
        private ParticleEmitter movingFireEmitter = null;
        private SceneLightPoint movingFireLight = null;

        private ParticleManager pManager = null;
        private readonly ParticleSystemDescription pBigFire = ParticleSystemDescription.InitializeFire(resourceParticlesFolder, resourceParticlesFireFile, 0.5f);
        private readonly ParticleSystemDescription pFire = ParticleSystemDescription.InitializeFire(resourceParticlesFolder, resourceParticlesFireFile, 0.1f);
        private readonly ParticleSystemDescription pPlume = ParticleSystemDescription.InitializeSmokePlume(resourceParticlesFolder, resourceParticlesSmokeFile, 0.1f);

        private DecalDrawer decalEmitter = null;

        private int directionalLightCount = 0;

        private bool loadingReady = false;
        private bool gameReady = false;

        private readonly BuiltInPostProcessState postProcessingState = BuiltInPostProcessState.Empty;

        public SkyboxScene(Game game)
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

            InitializeUI();
        }

        private void InitializeUI()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeCursor,
                    InitializeTextComponents,
                    InitializeDebug,
                ],
                InitializeUICompleted);

            LoadResources(group);
        }
        private async Task InitializeCursor()
        {
            var cursorDesc = UICursorDescription.Default(resourceCursorFile, 16, 16, true, Color.Purple);
            cursorDesc.StartsVisible = false;

            await AddComponentCursor<UICursor, UICursorDescription>("Cursor", "Cursor", cursorDesc);
        }
        private async Task InitializeTextComponents()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            help = await AddComponentUI<UITextArea, UITextAreaDescription>("Help", "Help", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            fps = await AddComponentUI<UITextArea, UITextAreaDescription>("FPS", "FPS", new UITextAreaDescription { Font = defaultFont12, TextForeColor = Color.Yellow, MaxTextLength = 256 });

            title.Text = "Skybox scene";
            help.Text = "Loading...";
            fps.Text = "";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Backpanel", "Backpanel", spDesc, LayerUI - 1);
        }
        private async Task InitializeDebug()
        {
            volumesDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>("DebugVolumesDrawer", "DebugVolumesDrawer", new PrimitiveListDrawerDescription<Line3D>() { Count = 10000 });
            volumesDrawer.Visible = false;

            graphDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>("DebugGraphDrawer", "DebugGraphDrawer", new PrimitiveListDrawerDescription<Triangle>() { Count = 10000 });
            graphDrawer.Visible = false;
        }
        private void InitializeUICompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            StartCamera();

            InitializeResources();
        }
        private void StartCamera()
        {
            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 5000.0f;
            Camera.Goto(new Vector3(-6, walker.Height, 5));
            Camera.LookTo(Vector3.UnitY + Vector3.UnitZ);
            Camera.MovementDelta = 4f;
            Camera.SlowMovementDelta = 2f;
        }

        private void InitializeResources()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeAudio,
                    InitializeSkydom,
                    InitializeLakeBottom,
                    InitializeTorchs,
                    InitializeObelisks,
                    InitializeFountain,
                    InitializeRuins,
                    InitializeWater,
                    InitializeParticles,
                    InitializeEmitter,
                    InitializeDecalEmitter,
                ],
                InitializeResourcesCompleted);

            LoadResources(group);
        }
        private async Task InitializeAudio()
        {
            soundEffectsManager = await AddComponent<SoundEffectsManager>("audioManager", "audioManager");
            soundEffectsManager.InitializeAudio(resourceAudioFolder);
        }
        private async Task InitializeSkydom()
        {
            string fileName = resourceSkyboxFile;
            int faceSize = 512;
            var skydomDesc = SkydomDescription.Sphere(fileName, faceSize, Camera.FarPlaneDistance);
            skydomDesc.StartsVisible = false;

            skydom = await AddComponentSky<Skydom, SkydomDescription>("Skydom", "Skydom", skydomDesc);
        }
        private async Task InitializeLakeBottom()
        {
            // Generates a random terrain using perlin noise
            var nmDesc = new NoiseMapDescriptor
            {
                MapWidth = mapSize,
                MapHeight = mapSize,
                Scale = 0.5f,
                Lacunarity = 2f,
                Persistance = 0.5f,
                Octaves = 4,
                Offset = Vector2.One,
                Seed = 5,
            };
            var noiseMap = NoiseMap.CreateNoiseMap(nmDesc);

            var heightCurve = new Curve();
            heightCurve.Keys.Add(0, 0);
            heightCurve.Keys.Add(0.4f, 0f);
            heightCurve.Keys.Add(1f, 1f);

            float cellSize = terrainSize / mapSize;

            var textures = new HeightmapTexturesDescription
            {
                ContentPath = resourceLakeBottomFolder,
                TexturesLR = [resourceLakeBottomDiffuseFile],
                NormalMaps = [resourceLakeBottomNormalFile],
                Scale = 0.0333f,
            };
            var groundDesc = SceneryDescription.FromHeightmap(noiseMap, cellSize, terrainHeight, heightCurve, textures, 2);
            groundDesc.Heightmap.UseFalloff = true;
            groundDesc.Heightmap.Transform = Matrix.Translation(0, -terrainHeight * 0.33f, 0);
            groundDesc.StartsVisible = false;

            lakeBottom = await AddComponentEffect<Scenery, SceneryDescription>("Lake Bottom", "Lake Bottom", groundDesc);
        }
        private async Task InitializeTorchs()
        {
            var torchDesc = new ModelInstancedDescription()
            {
                Instances = firePositions.Length,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(resourceTorchFolder, resourceTorchFile),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Default,
            };

            torchs = await AddComponentGround<ModelInstanced, ModelInstancedDescription>("Torchs", "Torchs", torchDesc);
        }
        private async Task InitializeObelisks()
        {
            var obeliskDesc = new ModelInstancedDescription()
            {
                Instances = firePositions.Length,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(resourceObeliskFolder, resourceObeliskFile),
                StartsVisible = false,
            };

            obelisks = await AddComponentEffect<ModelInstanced, ModelInstancedDescription>("Obelisks", "Obelisks", obeliskDesc);
        }
        private async Task InitializeFountain()
        {
            var fountainDesc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromFile(resourceFountainFolder, resourceFountainFile),
                StartsVisible = false,
                PathFindingHull = PickingHullTypes.Perfect,
            };

            fountain = await AddComponentGround<Model, ModelDescription>("Fountain", "Fountain", fountainDesc);
        }
        private async Task InitializeRuins()
        {
            var ruinsDesc = SceneryDescription.FromFile(resourceRuinsFolder, resourceRuinsFile);
            ruinsDesc.Quadtree.MaximumDepth = 1;
            ruinsDesc.StartsVisible = false;

            ruins = await AddComponentGround<Scenery, SceneryDescription>("Ruins", "Ruins", ruinsDesc);
        }
        private async Task InitializeWater()
        {
            var waterDesc = WaterDescription.CreateCalm(5000f, -2f);
            waterDesc.BaseColor = new Color3(0.067f, 0.065f, 0.003f);
            waterDesc.WaterColor = new Color4(0.003f, 0.267f, 0.096f, 0.98f);
            waterDesc.StartsVisible = false;

            water = await AddComponentEffect<Water, WaterDescription>("Water", "Water", waterDesc);
        }
        private async Task InitializeEmitter()
        {
            var mat = MaterialCookTorranceContent.Default;
            mat.EmissiveColor = Color.Yellow.RGB();

            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 0.05f, 32, 32);

            var mFireDesc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
                StartsVisible = false,
            };

            movingFire = await AddComponentEffect<Model, ModelDescription>("Emitter", "Emitter", mFireDesc);
        }
        private async Task InitializeParticles()
        {
            var pManagerDesc = ParticleManagerDescription.Default();
            pManagerDesc.StartsVisible = false;

            pManager = await AddComponentEffect<ParticleManager, ParticleManagerDescription>("ParticleManager", "ParticleManager", pManagerDesc);

            movingFireEmitter = new ParticleEmitter()
            {
                EmissionRate = 0.1f,
                InfiniteDuration = true,
                MaximumDistance = GameEnvironment.LODDistanceLow,
            };

            pManager.AddParticleSystem(ParticleSystemTypes.CPU, pBigFire, movingFireEmitter);
        }
        private async Task InitializeDecalEmitter()
        {
            var desc = new DecalDrawerDescription
            {
                TextureName = resourceBulletFile,
                MaxDecalCount = 1000,
                RotateDecals = true,
            };
            decalEmitter = await AddComponentEffect<DecalDrawer, DecalDrawerDescription>("Bullets", "Bullets", desc);
        }
        private void InitializeResourcesCompleted(LoadResourcesResult res)
        {
            loadingReady = true;

            if (!res.Completed)
            {
                help.Text = res.GetErrorMessage();

                return;
            }

            //Put ground objects in position
            StartGroundObjects();

            StartPathFinding();
        }
        private void StartGroundObjects()
        {
            Parallel.For(0, firePositions.Length, (i, loopState) =>
            {
                if (loopState.IsExceptional)
                {
                    return;
                }

                var color = Color.Yellow.RGB();
                if (i == 1) color = Color.Red.RGB();
                if (i == 2) color = Color.Green.RGB();
                if (i == 3) color = Color.LightBlue.RGB();

                FindTopGroundPosition(firePositions[i].X, firePositions[i].Y, out PickingResult<Triangle> result);
                var firePositions3D = result.Position;

                torchs[i].Manipulator.SetTransform(firePositions3D, Quaternion.Identity, 0.2f);

                var bbox = torchs[i].GetBoundingBox(true);
                firePositions3D.Y += (bbox.Maximum.Y - bbox.Minimum.Y) * 0.95f;

                var torchLights = new SceneLightPoint(
                    string.Format("Torch {0}", i),
                    false,
                    color,
                    color,
                    true,
                    SceneLightPointDescription.Create(firePositions3D, 4f, 20f));

                Lights.Add(torchLights);

                _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pFire, new ParticleEmitter() { Position = firePositions3D, InfiniteDuration = true, EmissionRate = 0.1f, MaximumDistance = GameEnvironment.LODDistanceLow });
                _ = pManager.AddParticleSystem(ParticleSystemTypes.CPU, pPlume, new ParticleEmitter() { Position = firePositions3D, InfiniteDuration = true, EmissionRate = 0.2f, MaximumDistance = GameEnvironment.LODDistanceLow });
            });

            Parallel.For(0, obeliskPositions.Length, (i, loopState) =>
            {
                if (loopState.IsExceptional)
                {
                    return;
                }

                obelisks[i].Manipulator.SetTransform(obeliskPositions[i], obeliskRotations[i], 10f);
            });

            fountain.Manipulator.SetScaling(2.3f);
        }
        private void StartPathFinding()
        {
            //Configure the input geometry
            var nvInput = new InputGeometry(GetTrianglesForNavigationGraph);

            //Configure de navigation mesh build settings
            var nvSettings = BuildSettings.Default;
            nvSettings.TileSize = 16;
            nvSettings.CellSize = 0.05f;
            nvSettings.CellHeight = 0.02f;
            nvSettings.PartitionType = SamplePartitionTypes.Monotone;

            //Generate the path finder description
            PathFinderDescription = new(nvSettings, nvInput, [walker]);

            EnqueueNavigationGraphUpdate(
                NavigationGraphLoaded,
                (progress) => { help.Text = $"Loading navigation mesh {progress:0.0%}..."; });
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            UpdateInput();

            UpdateState(gameTime);
        }
        private void UpdateInput()
        {
            if (!loadingReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (Game.Input.KeyJustReleased(Keys.F7))
            {
                var m1 = fountain.GetMaterial("World_Expansion03_doodads_gilneas_fountains_gilneas_fountainbro");
                var bronze1 = MeshMaterial.FromMaterial(MaterialCookTorrance.FromBuiltIn(BuiltInMaterials.Bronze));
                bronze1.DiffuseTexture = m1.DiffuseTexture;

                var m2 = fountain.GetMaterial("World_Expansion03_doodads_gilneas_fountains_gilneas_fountai.000");
                var bronze2 = MeshMaterial.FromMaterial(MaterialCookTorrance.FromBuiltIn(BuiltInMaterials.Bronze));
                bronze2.DiffuseTexture = m2.DiffuseTexture;

                fountain.ReplaceMaterial("World_Expansion03_doodads_gilneas_fountains_gilneas_fountainbro", bronze1);
                fountain.ReplaceMaterial("World_Expansion03_doodads_gilneas_fountains_gilneas_fountai.000", bronze2);
            }

            UpdateInputDebug();
            UpdateInputCamera();
            UpdateInputPlayer();
            UpdateInputLights();
        }
        private void UpdateInputCamera()
        {
            if (Game.Input.MouseButtonPressed(MouseButtons.Left))
            {
                var pRay = GetPickingRay();

                if (ruins.PickNearest(pRay, out PickingResult<Triangle> r))
                {
                    var tri = Line3D.CreateTriangle(r.Primitive);
                    var cross = Line3D.CreateCross(r.Position, 0.1f);

                    volumesDrawer.SetPrimitives(Color.White, tri);
                    volumesDrawer.SetPrimitives(Color.YellowGreen, cross);
                }
            }

            if (Game.Input.MouseButtonJustReleased(MouseButtons.Left))
            {
                var pRay = GetPickingRay();

                if (ruins.PickNearest(pRay, out PickingResult<Triangle> r))
                {
                    decalEmitter.AddDecal(
                        r.Position,
                        r.Primitive.Normal,
                        Vector2.One * 0.1f,
                        60);
                }
            }

#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                Game.GameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInputPlayer()
        {
            if (Walk(walker, Camera.Position, Camera.GetNextPosition(), true, out var walkerPos))
            {
                Camera.Goto(walkerPos);
            }
            else
            {
                Camera.Goto(Camera.Position);
            }
        }
        private void UpdateInputLights()
        {
            if (Game.Input.KeyJustReleased(Keys.Add))
            {
                directionalLightCount++;
                if (directionalLightCount > 3)
                {
                    directionalLightCount = 0;
                }

                UpdateInputEnabledLights();
            }

            if (Game.Input.KeyJustReleased(Keys.Subtract))
            {
                directionalLightCount--;
                if (directionalLightCount < 0)
                {
                    directionalLightCount = 3;
                }

                UpdateInputEnabledLights();
            }
        }
        private void UpdateInputEnabledLights()
        {
            Lights.DirectionalLights[0].Enabled = directionalLightCount > 0;
            Lights.DirectionalLights[1].Enabled = directionalLightCount > 1;
            Lights.DirectionalLights[2].Enabled = directionalLightCount > 2;
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.Home))
            {
                StartCamera();
            }

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                volumesDrawer.Visible = !volumesDrawer.Visible;

                if (volumesDrawer.Visible)
                {
                    DEBUGUpdateVolumesDrawer();
                }
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                graphDrawer.Visible = !graphDrawer.Visible;

                DEBUGUpdateGraphDrawer();
            }
        }
        private void UpdateState(IGameTime gameTime)
        {
            if (!gameReady)
            {
                return;
            }

            UpdateMovingLights(gameTime);
            UpdateTexts();

            DEBUGUpdateMovingVolumesDrawer();
        }
        private void UpdateMovingLights(IGameTime gameTime)
        {
            float r = 5.5f;
            float h = 1.25f;
            float d = 0.5f;
            float v = 0.8f;

            float totalSeconds = gameTime.TotalSeconds;
            Vector3 position = Vector3.Zero;
            position.X = r * d * MathF.Cos(v * totalSeconds);
            position.Y = h + (0.25f * (1f + MathF.Sin(totalSeconds)));
            position.Z = r * d * MathF.Sin(v * totalSeconds);

            movingFire.Manipulator.SetPosition(position);
            movingFireEmitter.Position = position;
            movingFireLight.Position = position;
        }
        private void UpdateTexts()
        {
            var m = soundEffectsManager.GetOutputMatrix();
            var ep = movingFire.Manipulator.Position.GetDescription();
            var ev = movingFire.Manipulator.Velocity.GetDescription();
            var lp = Camera.Position.GetDescription();
            var lv = Camera.Velocity.GetDescription();
            var d = Vector3.Distance(movingFire.Manipulator.Position, Camera.Position);
            var sb = new StringBuilder();
            sb.AppendLine($"Mouse (X:{Game.Input.MouseXDelta}; Y:{Game.Input.MouseYDelta}, Wheel: {Game.Input.MouseWheelDelta}) Absolute (X:{Game.Input.MouseX}; Y:{Game.Input.MouseY})");
            sb.AppendLine($"L {m[0]:0.000} R {m[1]:0.000} Distance {d}");
            sb.AppendLine($"Emitter  pos: {ep} Emitter  vel: {ev}");
            sb.AppendLine($"Listener pos: {lp} Listener vel: {lv}");
            fps.Text = sb.ToString();
        }

        private void DEBUGUpdateVolumesDrawer()
        {
            volumesDrawer.SetPrimitives(ruinsVolumeColor, Line3D.CreateBox(ruins.GetBoundingBox()));

            var volumesTorchs = torchs.GetInstances().Select(i => i.GetBoundingBox());
            volumesDrawer.SetPrimitives(torchVolumeColor, Line3D.CreateBoxes(volumesTorchs));

            var volumesObelisks = obelisks.GetInstances().Select(i => i.GetBoundingBox());
            volumesDrawer.SetPrimitives(obeliskVolumeColor, Line3D.CreateBoxes(volumesObelisks));

            var volumeFountain = fountain.GetBoundingBox();
            volumesDrawer.SetPrimitives(fountainVolumeColor, Line3D.CreateBox(volumeFountain));

            var lights = Lights.PointLights.Select(l => new { Color = l.DiffuseColor, Sphere = l.BoundingSphere });
            foreach (var light in lights)
            {
                volumesDrawer.SetPrimitives(new Color4(light.Color, alpha), Line3D.CreateSphere(light.Sphere, bsphSlices, bsphStacks));
            }
        }
        private void DEBUGUpdateMovingVolumesDrawer()
        {
            var light = Lights.PointLights[0];

            volumesDrawer.SetPrimitives(new Color4(light.DiffuseColor, alpha), Line3D.CreateSphere(light.BoundingSphere, bsphSlices, bsphStacks));
        }
        private void DEBUGUpdateGraphDrawer()
        {
            if (!graphDrawer.Visible)
            {
                return;
            }

            var nodes = GetNodes(walker).OfType<GraphNode>();
            if (!nodes.Any())
            {
                return;
            }

            graphDrawer.Clear();

            foreach (var node in nodes)
            {
                var color = Helper.IntToCol(node.Id, 128);
                graphDrawer.AddPrimitives(color, node.Triangles);
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
            help.SetPosition(new Vector2(0, 24));
            fps.SetPosition(new Vector2(0, 40));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = 120;
        }

        public void NavigationGraphLoaded(bool loaded)
        {
            if (!loaded)
            {
                return;
            }

            StartScene();

            StartSounds();

            gameReady = true;
        }
        private void StartScene()
        {
            Lights.DirectionalLights[0].Enabled = true;
            Lights.DirectionalLights[0].CastShadow = true;

            Lights.DirectionalLights[1].Enabled = true;

            Lights.DirectionalLights[2].Enabled = false;

            directionalLightCount = Lights.DirectionalLights.Length;

            movingFireLight = new SceneLightPoint(
                "Moving fire light",
                false,
                Color.Orange.RGB(),
                Color.Orange.RGB(),
                true,
                SceneLightPointDescription.Create(Vector3.Zero, 15f, 20f));

            Lights.Add(movingFireLight);

            skydom.Visible = true;
            lakeBottom.Visible = true;
            torchs.Visible = true;
            obelisks.Visible = true;
            fountain.Visible = true;
            ruins.Visible = true;
            water.Visible = true;
            movingFire.Visible = true;
            pManager.Visible = true;

            postProcessingState.AddToneMapping(BuiltInToneMappingTones.SimpleReinhard);
            Renderer.ClearPostProcessingEffects();
            Renderer.PostProcessingObjectsEffects = postProcessingState;

#if DEBUG
            help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Right Mouse: Drag view | Left Mouse: Pick";
#else
            help.Text = "Escape: Exit | Home: Reset camera | AWSD: Move camera | Move Mouse: View | Left Mouse: Pick";
#endif
        }
        private void StartSounds()
        {
            soundEffectsManager.Start(0.5f);
            soundEffectsManager.PlaySphereEffect(movingFire);
        }
    }
}
