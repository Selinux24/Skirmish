using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace TerrainTest
{
    public class TestScene3D : Scene
    {
        private const int MaxPickingTest = 1000;
        private const int MaxGridDrawer = 10000;

        private bool walkMode = false;
        private float walkerVelocity = 8f;
        private bool follow = false;
        private NavigationMeshAgent walkerAgent = new NavigationMeshAgent()
        {
            Height = 1f,
            Radius = 0.5f,
            MaxClimb = 0.9f,
        };

        private bool useDebugTex = false;
        private SceneRendererResultEnum shadowResult = SceneRendererResultEnum.ShadowMapStatic;
        private SpriteTexture shadowMapDrawer = null;
        private ShaderResourceView debugTex = null;
        private int graphIndex = -1;

        private int layerHud = 99;
        private int layerObjects = 0;
        private int layerTerrain = 1;
        private int layerEffects = 2;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer stats = null;
        private TextDrawer counters1 = null;
        private TextDrawer counters2 = null;

        private Model cursor3D = null;
        private Cursor cursor2D = null;

        private Model tank = null;
        private NavigationMeshAgent tankAgent = new NavigationMeshAgent();

        private LensFlare lensFlare = null;
        private Skydom skydom = null;
        private SkyPlane clouds = null;
        private Scenery terrain = null;
        private GroundGardener gardener = null;
        private Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private List<Line3D> oks = new List<Line3D>();
        private List<Line3D> errs = new List<Line3D>();
        private LineListDrawer terrainLineDrawer = null;
        private LineListDrawer terrainPointDrawer = null;
        private TriangleListDrawer terrainGraphDrawer = null;

        private Model helipod = null;
        private Model garage = null;
        private ModelInstanced obelisk = null;
        private ModelInstanced rocks = null;
        private ModelInstanced tree1 = null;
        private ModelInstanced tree2 = null;
        private Color4 objColor = Color.Magenta;
        private LineListDrawer objLineDrawer = null;

        private Model helicopter = null;
        private AnimationPath helicopterRollPath = null;
        private LineListDrawer helicopterLineDrawer = null;
        private Vector3 helicopterHeightOffset = (Vector3.Up * 15f);
        private Color4 gridColor = new Color4(Color.LightSeaGreen.ToColor3(), 0.5f);
        private Color4 curvesColor = Color.Red;
        private Color4 pointsColor = Color.Blue;
        private Color4 segmentsColor = new Color4(Color.Cyan.ToColor3(), 0.8f);
        private Color4 hAxisColor = Color.YellowGreen;
        private Color4 wAxisColor = Color.White;
        private Color4 velocityColor = Color.Green;
        private LineListDrawer curveLineDrawer = null;

        private LineListDrawer lightsVolumeDrawer = null;
        private bool drawDrawVolumes = false;
        private bool drawCullVolumes = false;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            #region Lights

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[0].CastShadow = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = true;

            this.Lights.Add(new SceneLightPoint(
                "Blue point",
                false,
                Color.Blue,
                Color.Blue,
                true,
                Vector3.Zero,
                2f,
                5f));
            this.Lights.Add(new SceneLightPoint(
                "Red point",
                false,
                Color.Red,
                Color.Red,
                true,
                Vector3.Zero,
                2f,
                5f));

            this.lightsVolumeDrawer = this.AddLineListDrawer(new LineListDrawerDescription() { DepthEnabled = true }, 5000, this.layerEffects);

            #endregion

            #region Camera

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 5000f;

            #endregion

            #region Texts

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White), this.layerHud);
            this.load = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), this.layerHud);
            this.stats = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), this.layerHud);
            this.counters1 = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 10, Color.GreenYellow), this.layerHud);
            this.counters2 = this.AddText(TextDrawerDescription.Generate("Lucida Casual", 10, Color.GreenYellow), this.layerHud);

            this.title.Text = "Terrain collision and trajectories test";
            this.load.Text = "";
            this.stats.Text = "";
            this.counters1.Text = "";
            this.counters2.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, 24);
            this.stats.Position = new Vector2(0, 46);
            this.counters1.Position = new Vector2(0, 68);
            this.counters2.Position = new Vector2(0, 90);

            #endregion

            #region Loading models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Cursor 3D

            sw.Restart();
            var c3DDesc = new ModelDescription()
            {
                Name = "Cursor3D",
                DeferredEnabled = false,
                CastShadow = false,
                DepthEnabled = false,
            };
            this.cursor3D = this.AddModel("resources/cursor", "cursor.xml", c3DDesc, true, this.layerHud);
            sw.Stop();
            loadingText += string.Format("cursor3D: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Cursor 2D

            sw.Restart();
            var c2DDesc = new SpriteDescription()
            {
                Name = "Cursor2D",
                ContentPath = "resources/Cursor",
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };
            this.cursor2D = this.AddCursor(c2DDesc, this.layerHud);
            this.cursor2D.Color = Color.Red;
            this.cursor2D.Visible = false;
            sw.Stop();
            loadingText += string.Format("cursor2D: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Lens flare

            sw.Restart();
            var lfDesc = new LensFlareDescription()
            {
                Name = "Flares",
                ContentPath = "resources/Flare",
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
            };
            this.lensFlare = this.AddLensFlare(lfDesc, this.layerEffects);
            sw.Stop();
            loadingText += string.Format("lensFlare: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopter

            sw.Restart();
            var hDesc = new ModelDescription()
            {
                Name = "Helicopter",
                CastShadow = true,
                Static = false,
                TextureIndex = 2,
            };
            this.helicopter = this.AddModel("resources/Helicopter", "Helicopter.xml", hDesc, true, this.layerObjects);
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            this.Lights.AddRange(this.helicopter.Lights);

            this.helicopter.SetManipulator(new HeliManipulator());
            this.helicopter.Manipulator.SetScale(0.75f);

            AnimationPath p = new AnimationPath();
            p.AddLoop("default");
            this.helicopter.AnimationController.AddPath(p);
            this.helicopter.AnimationController.Start();

            #endregion

            #region Tank

            sw.Restart();
            var tDesc = new ModelDescription()
            {
                Name = "Tank",
                CastShadow = true,
                Static = false,
            };
            this.tank = this.AddModel("resources/Leopard", "Leopard.xml", tDesc, true, this.layerObjects);
            sw.Stop();
            loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);

            this.Lights.AddRange(this.tank.Lights);

            this.tank.Manipulator.SetScale(0.2f, true);

            var tankbbox = this.tank.GetBoundingBox();
            tankAgent.Height = tankbbox.GetY();
            tankAgent.Radius = tankbbox.GetX() * 0.5f;
            tankAgent.MaxClimb = tankbbox.GetY() * 0.45f;

            #endregion

            #region Helipod

            sw.Restart();
            var hpDesc = new ModelDescription()
            {
                Name = "Helipod",
                CastShadow = true,
                Static = true,
            };
            this.helipod = this.AddModel("resources/Helipod", "Helipod.xml", hpDesc, true, this.layerObjects);
            sw.Stop();
            loadingText += string.Format("helipod: {0} ", sw.Elapsed.TotalSeconds);

            this.Lights.AddRange(this.helipod.Lights);

            #endregion

            #region Garage

            sw.Restart();
            var gDesc = new ModelDescription()
            {
                Name = "Garage",
                CastShadow = true,
                Static = true,
            };
            this.garage = this.AddModel("resources/Garage", "Garage.xml", gDesc, true, this.layerObjects);
            sw.Stop();
            loadingText += string.Format("garage: {0} ", sw.Elapsed.TotalSeconds);

            this.Lights.AddRange(this.garage.Lights);

            #endregion

            #region Obelisk

            sw.Restart();
            var oDesc = new ModelInstancedDescription()
            {
                Name = "Obelisk",
                CastShadow = true,
                Static = true,
                Instances = 4,
            };
            this.obelisk = this.AddInstancingModel("resources/Obelisk", "Obelisk.xml", oDesc, true, this.layerObjects);
            sw.Stop();
            loadingText += string.Format("obelisk: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Rocks

            sw.Restart();
            var rDesc = new ModelInstancedDescription()
            {
                Name = "Rocks",
                CastShadow = true,
                Static = true,
                Instances = 250,
            };
            this.rocks = this.AddInstancingModel("resources/Rocks", "boulder.xml", rDesc, true, this.layerObjects);
            sw.Stop();
            loadingText += string.Format("rocks: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Trees

            sw.Restart();
            var t1Desc = new ModelInstancedDescription()
            {
                Name = "birch_a",
                CastShadow = true,
                Static = true,
                AlphaEnabled = true,
                Instances = 100,
            };
            var t2Desc = new ModelInstancedDescription()
            {
                Name = "birch_b",
                CastShadow = true,
                Static = true,
                AlphaEnabled = true,
                Instances = 100,
            };
            this.tree1 = this.AddInstancingModel("resources/Trees", "birch_a.xml", t1Desc, true, this.layerTerrain);
            this.tree2 = this.AddInstancingModel("resources/Trees", "birch_b.xml", t2Desc, true, this.layerTerrain);
            sw.Stop();
            loadingText += string.Format("trees: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Skydom

            sw.Restart();
            this.skydom = this.AddSkydom(new SkydomDescription()
            {
                Name = "Skydom",
                ContentPath = "resources/Skydom",
                Texture = "sunset.dds",
                Radius = this.Camera.FarPlaneDistance,
            });
            sw.Stop();
            loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Clouds

            sw.Restart();
            this.clouds = this.AddSkyPlane(new SkyPlaneDescription()
            {
                Name = "Clouds",
                ContentPath = "Resources/clouds",
                Texture1Name = "perturb001.dds",
                Texture2Name = "cloud001.dds",
                Mode = SkyPlaneMode.Perturbed,
                MaxBrightness = 0.8f,
                MinBrightness = 0.1f,
                Repeat = 5,
                Velocity = 1,
                Direction = new Vector2(1, 1),
            });
            sw.Stop();
            loadingText += string.Format("clouds: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Terrain

            sw.Restart();
            var navSettings = NavigationMeshGenerationSettings.Default;
            navSettings.Agents = new[]
            {
                walkerAgent,
                tankAgent,
            };
            var terrainDescription = new GroundDescription()
            {
                Name = "Terrain",
                Quadtree = new GroundDescription.QuadtreeDescription()
                {
                    MaximumDepth = 1,
                },
                PathFinder = new GroundDescription.PathFinderDescription()
                {
                    Settings = navSettings,
                },
                CastShadow = true,
                Static = true,
                DelayGeneration = true,
            };
            this.terrain = this.AddScenery("resources/Terrain", "two_levels.xml", terrainDescription, true, this.layerTerrain);
            sw.Stop();

            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Gardener

            sw.Restart();
            var grDesc = new GroundGardenerDescription()
            {
                ContentPath = "resources/Terrain/Foliage/Billboard",
                ChannelRed = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "grass.png" },
                    Saturation = 2f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = new Vector2(0.25f, 0.25f),
                    MaxSize = new Vector2(0.5f, 0.75f),
                },
                ChannelGreen = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "grass.png" },
                    Saturation = 2f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = new Vector2(0.25f, 0.25f),
                    MaxSize = new Vector2(0.5f, 0.75f),
                },
                ChannelBlue = new GroundGardenerDescription.Channel()
                {
                    VegetarionTextures = new[] { "grass.png" },
                    Saturation = 2f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = new Vector2(0.25f, 0.25f),
                    MaxSize = new Vector2(0.5f, 0.75f),
                }
            };
            this.gardener = this.AddGardener(grDesc, this.layerTerrain);
            sw.Stop();

            loadingText += string.Format("gardener: {0} ", sw.Elapsed.TotalSeconds);

            this.gardener.ParentGround = this.terrain;

            #endregion

            this.load.Text = loadingText;

            #endregion

            #region Model positioning over scenery

            Random posRnd = new Random(1);

            List<Line3D> lines = new List<Line3D>();

            this.gardener.SetWind(this.windDirection, this.windStrength);

            //Helipod
            Vector3 hPos;
            Triangle hTri;
            float hDist;
            if (this.terrain.FindTopGroundPosition(75, 75, out hPos, out hTri, out hDist))
            {
                this.helipod.Manipulator.SetPosition(hPos);
            }
            lines.AddRange(Line3D.CreateWiredBox(this.helipod.GetBoundingBox()));

            //Garage
            Vector3 gPos;
            Triangle gTri;
            float gDist;
            if (this.terrain.FindTopGroundPosition(-10, -40, out gPos, out gTri, out gDist))
            {
                this.garage.Manipulator.SetPosition(gPos);
                this.garage.Manipulator.SetRotation(MathUtil.PiOverFour + MathUtil.Pi, 0, 0);
            }
            lines.AddRange(Line3D.CreateWiredBox(this.garage.GetBoundingBox()));

            //Obelisk
            for (int i = 0; i < 4; i++)
            {
                int ox = i == 0 || i == 2 ? 1 : -1;
                int oy = i == 0 || i == 1 ? 1 : -1;

                Vector3 obeliskPosition;
                Triangle obeliskTri;
                float obeliskDist;
                if (this.terrain.FindTopGroundPosition(ox * 50, oy * 50, out obeliskPosition, out obeliskTri, out obeliskDist))
                {
                    this.obelisk.Instances[i].Manipulator.SetPosition(obeliskPosition);
                    this.obelisk.Instances[i].Manipulator.SetScale(1.5f);
                }
                lines.AddRange(Line3D.CreateWiredBox(this.obelisk.Instances[i].GetBoundingBox()));
            }

            //Rocks
            for (int i = 0; i < this.rocks.Instances.Length; i++)
            {
                var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                Vector3 rockPosition;
                Triangle rockTri;
                float rockDist;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out rockPosition, out rockTri, out rockDist))
                {
                    var scale = 1f;
                    if (i < 5)
                    {
                        scale = posRnd.NextFloat(2f, 5f);
                    }
                    else if (i < 30)
                    {
                        scale = posRnd.NextFloat(0.5f, 2f);
                    }
                    else
                    {
                        scale = posRnd.NextFloat(0.1f, 0.2f);
                    }

                    this.rocks.Instances[i].Manipulator.SetPosition(rockPosition);
                    this.rocks.Instances[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi));
                    this.rocks.Instances[i].Manipulator.SetScale(scale);
                }
                lines.AddRange(Line3D.CreateWiredBox(this.rocks.Instances[i].GetBoundingBox()));
            }

            //Trees
            for (int i = 0; i < this.tree1.Instances.Length; i++)
            {
                var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                Vector3 treePosition;
                Triangle treeTri;
                float treeDist;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out treePosition, out treeTri, out treeDist))
                {
                    this.tree1.Instances[i].Manipulator.SetPosition(treePosition);
                    this.tree1.Instances[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0);
                    this.tree1.Instances[i].Manipulator.SetScale(posRnd.NextFloat(0.25f, 0.75f));
                }
                lines.AddRange(Line3D.CreateWiredTriangle(this.tree1.Instances[i].GetVolume()));
            }

            for (int i = 0; i < this.tree2.Instances.Length; i++)
            {
                var pos = this.GetRandomPoint(posRnd, Vector3.Zero);

                Vector3 treePosition;
                Triangle treeTri;
                float treeDist;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out treePosition, out treeTri, out treeDist))
                {
                    this.tree2.Instances[i].Manipulator.SetPosition(treePosition);
                    this.tree2.Instances[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), 0, 0);
                    this.tree2.Instances[i].Manipulator.SetScale(posRnd.NextFloat(0.25f, 0.75f));
                }
                lines.AddRange(Line3D.CreateWiredTriangle(this.tree2.Instances[i].GetVolume()));
            }

            this.objLineDrawer = this.AddLineListDrawer(new LineListDrawerDescription(), lines.ToArray(), this.objColor);
            this.objLineDrawer.Visible = false;

            this.terrain.AttachFullPickingFullPathFinding(new ModelBase[] { this.helipod, this.garage, this.obelisk }, false);
            this.terrain.AttachCoarsePathFinding(new ModelBase[] { this.tree1, this.tree2, this.rocks }, false);
            this.terrain.UpdateInternals();

            Vector3 heliPos;
            Triangle heliTri;
            float heliDist;
            if (this.terrain.FindTopGroundPosition(this.helipod.Manipulator.Position.X, this.helipod.Manipulator.Position.Z, out heliPos, out heliTri, out heliDist))
            {
                this.helicopter.Manipulator.SetPosition(heliPos);
                this.helicopter.Manipulator.SetNormal(heliTri.Normal);
            }

            this.helicopterRollPath = new AnimationPath();
            this.helicopterRollPath.AddLoop("roll");

            Vector3 tankPosition;
            Triangle tankTriangle;
            float tankDist;
            if (this.terrain.FindTopGroundPosition(-60, -60, out tankPosition, out tankTriangle, out tankDist))
            {
                this.tank.Manipulator.SetPosition(tankPosition);
                this.tank.Manipulator.SetNormal(tankTriangle.Normal);
            }

            #endregion

            #region DEBUG Shadow Map

            int width = 300;
            int height = 300;
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;
            var stDescription = new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = SpriteTextureChannelsEnum.Red,
            };
            this.shadowMapDrawer = this.AddSpriteTexture(stDescription, this.layerHud);
            this.shadowMapDrawer.Visible = false;
            this.shadowMapDrawer.DeferredEnabled = false;

            this.debugTex = this.Game.ResourceManager.CreateResource(@"Resources\uvtest.png");

            #endregion

            #region DEBUG Path finding Graph

            this.terrainGraphDrawer = this.AddTriangleListDrawer(new TriangleListDrawerDescription(), MaxGridDrawer, this.layerEffects);
            this.terrainGraphDrawer.Visible = false;

            #endregion

            #region DEBUG Ground position test

            BoundingBox bbox = this.terrain.GetBoundingBox();

            float sep = 2.1f;
            for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += sep)
            {
                for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += sep)
                {
                    Vector3 pos;
                    Triangle tri;
                    float dist;
                    if (this.terrain.FindTopGroundPosition(x, z, out pos, out tri, out dist))
                    {
                        this.oks.Add(new Line3D(pos, pos + Vector3.Up));
                    }
                    else
                    {
                        this.errs.Add(new Line3D(x, 10, z, x, -10, z));
                    }
                }
            }

            this.terrainLineDrawer = this.AddLineListDrawer(new LineListDrawerDescription(), oks.Count + errs.Count, this.layerEffects);
            this.terrainLineDrawer.Visible = false;

            if (this.oks.Count > 0)
            {
                this.terrainLineDrawer.AddLines(Color.Green, this.oks.ToArray());
            }
            if (this.errs.Count > 0)
            {
                this.terrainLineDrawer.AddLines(Color.Red, this.errs.ToArray());
            }

            #endregion

            #region DEBUG Picking test

            this.terrainPointDrawer = this.AddLineListDrawer(new LineListDrawerDescription(), MaxPickingTest, this.layerEffects);
            this.terrainPointDrawer.Visible = false;

            #endregion

            #region DEBUG Helicopter manipulator

            this.helicopterLineDrawer = this.AddLineListDrawer(new LineListDrawerDescription(), 1000, this.layerEffects);
            this.helicopterLineDrawer.Visible = false;

            #endregion

            #region DEBUG Trajectory

            this.curveLineDrawer = this.AddLineListDrawer(new LineListDrawerDescription(), 20000, this.layerEffects);
            this.curveLineDrawer.Visible = false;
            this.curveLineDrawer.SetLines(this.wAxisColor, Line3D.CreateAxis(Matrix.Identity, 20f));

            #endregion

            this.Camera.Goto(this.helicopter.Manipulator.Position + Vector3.One * 25f);
            this.Camera.LookTo(this.helicopter.Manipulator.Position);

            this.SceneVolume = this.terrain.GetBoundingSphere();
        }
        public override void Dispose()
        {
            if (this.debugTex != null)
            {
                this.debugTex.Dispose();
                this.debugTex = null;
            }

            base.Dispose();
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Z))
            {
                this.walkMode = !this.walkMode;

                if (this.walkMode)
                {
                    this.Camera.Mode = CameraModes.FirstPerson;
                    this.Camera.MovementDelta = this.walkerVelocity;
                    this.Camera.SlowMovementDelta = this.walkerVelocity * 0.05f;
                    this.cursor3D.Visible = false;
                    this.cursor2D.Visible = true;
                }
                else
                {
                    this.Camera.Mode = CameraModes.Free;
                    this.Camera.MovementDelta = 20.5f;
                    this.Camera.SlowMovementDelta = 1f;
                    this.cursor3D.Visible = true;
                    this.cursor2D.Visible = false;
                }

                this.DEBUGUpdateGraphDrawer();
            }

            #region Cursor picking and positioning

            bool picked = false;
            Vector3 pickedPosition = Vector3.Zero;
            Triangle pickedTriangle = new Triangle();
            float pickedDistance = float.MaxValue;

            if (!this.walkMode)
            {
                Ray cursorRay = this.GetPickingRay();

                picked = this.terrain.PickNearest(ref cursorRay, true, out pickedPosition, out pickedTriangle, out pickedDistance);
                if (picked)
                {
                    this.cursor3D.Manipulator.SetPosition(pickedPosition);
                }
            }

            #endregion

            if (this.walkMode)
            {
                #region Walker

#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.Camera.RotateMouse(
                        this.Game.GameTime,
                        this.Game.Input.MouseXDelta,
                        this.Game.Input.MouseYDelta);
                }

                var prevPos = this.Camera.Position;

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

                Vector3 walkerPos;
                if (this.terrain.Walk(this.walkerAgent, prevPos, this.Camera.Position, out walkerPos))
                {
                    this.Camera.Goto(walkerPos);
                }
                else
                {
                    this.Camera.Goto(prevPos);
                }

                #endregion
            }
            else
            {
                #region Free Camera

#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.Camera.RotateMouse(
                        this.Game.GameTime,
                        this.Game.Input.MouseXDelta,
                        this.Game.Input.MouseYDelta);
                }

                if (this.Game.Input.KeyJustReleased(Keys.Space))
                {
                    this.follow = !this.follow;
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

                if (this.follow)
                {
                    var sph = this.helicopter.GetBoundingSphere();
                    this.Camera.LookTo(sph.Center);
                    this.Camera.Goto(sph.Center + (this.helicopter.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
                }

                #endregion
            }

            #region Tank

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (picked)
                {
                    var p = this.terrain.FindPath(this.tankAgent, this.tank.Manipulator.Position, pickedPosition);
                    if (p != null)
                    {
                        this.tank.Manipulator.Follow(p.ReturnPath.ToArray(), 10f, this.terrain);

                        this.DEBUGDrawTankPath(this.tank.Manipulator.Position, p);
                    }
                }
            }

            #endregion

            #region Helicopter

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                Curve3D curve = this.GenerateHelicopterPath();
                ((HeliManipulator)this.helicopter.Manipulator).Follow(curve, 10f, 0.001f);
                this.helicopter.AnimationController.SetPath(this.helicopterRollPath);
                this.DEBUGDrawHelicopterPath(curve);
            }

            this.Lights.PointLights[0].Position = (this.helicopter.Manipulator.Position + this.helicopter.Manipulator.Up + this.helicopter.Manipulator.Left);
            this.Lights.PointLights[1].Position = (this.helicopter.Manipulator.Position + this.helicopter.Manipulator.Up + this.helicopter.Manipulator.Right);

            if (this.curveLineDrawer.Visible)
            {
                Matrix rot = Matrix.RotationQuaternion(this.helicopter.Manipulator.Rotation) * Matrix.Translation(this.helicopter.Manipulator.Position);
                this.curveLineDrawer.SetLines(this.hAxisColor, Line3D.CreateAxis(rot, 5f));
            }

            if (this.helicopterLineDrawer.Visible)
            {
                BoundingSphere sph = this.helicopter.GetBoundingSphere();
                this.helicopterLineDrawer.SetLines(new Color4(Color.White.ToColor3(), 0.55f), Line3D.CreateWiredSphere(sph, 50, 20));
            }

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.terrainLineDrawer.Visible = !this.terrainLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.terrainGraphDrawer.Visible = !this.terrainGraphDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.terrainPointDrawer.Visible = !this.terrainPointDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.curveLineDrawer.Visible = !this.curveLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.helicopterLineDrawer.Visible = !this.helicopterLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.shadowMapDrawer.Visible = !this.shadowMapDrawer.Visible;
                this.shadowResult = SceneRendererResultEnum.ShadowMapStatic;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.useDebugTex = !this.useDebugTex;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                this.objLineDrawer.Visible = !this.objLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F11))
            {
                if (this.drawDrawVolumes == false && this.drawCullVolumes == false)
                {
                    this.drawDrawVolumes = true;
                    this.drawCullVolumes = false;
                }
                else if (this.drawDrawVolumes == true && this.drawCullVolumes == false)
                {
                    this.drawDrawVolumes = false;
                    this.drawCullVolumes = true;
                }
                else if (this.drawDrawVolumes == false && this.drawCullVolumes == true)
                {
                    this.drawDrawVolumes = false;
                    this.drawCullVolumes = false;
                }
            }

            if (this.drawDrawVolumes) this.DEBUGDrawLightMarks();
            if (this.drawCullVolumes) this.DEBUGDrawLightVolumes();

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.graphIndex++;
                this.DEBUGUpdateGraphDrawer();
            }
            if (this.Game.Input.KeyJustReleased(Keys.Subtract))
            {
                this.graphIndex--;
                this.DEBUGUpdateGraphDrawer();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.helicopter.TextureIndex++;
                if (this.helicopter.TextureIndex > 2) this.helicopter.TextureIndex = 2;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.helicopter.TextureIndex--;
                if (this.helicopter.TextureIndex < 0) this.helicopter.TextureIndex = 0;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                this.shadowResult = SceneRendererResultEnum.ShadowMapStatic;
            }
            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                this.shadowResult = SceneRendererResultEnum.ShadowMapDynamic;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            if (this.Game.Input.KeyJustReleased(Keys.C))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.D1))
            {
                this.walkMode = !this.walkMode;
                this.DEBUGUpdateGraphDrawer();
                this.walkMode = !this.walkMode;
            }

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (this.terrainGraphDrawer.Visible)
                {
                    this.terrainPointDrawer.Clear();

                    if (picked)
                    {
                        this.DEBUGPickingPosition(pickedPosition);
                    }
                }
            }

            #endregion
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.shadowMapDrawer.Texture = this.useDebugTex ? this.debugTex : this.Renderer.GetResource(this.shadowResult);

            #region Texts

            this.stats.Text = this.Game.RuntimeText;

            string txt1 = string.Format("Buffers active: {0} {1} Kbs, reads: {2}, writes: {3}; {4} - Result: {5}; Primitives: {6}", Counters.Buffers, Counters.BufferBytes / 1024, Counters.BufferReads, Counters.BufferWrites, this.RenderMode, this.shadowResult, Counters.PrimitivesPerFrame);
            string txt2 = string.Format("IA Input Layouts: {0}, Primitives: {1}, VB: {2}, IB: {3}, Terrain Patches: {4}", Counters.IAInputLayoutSets, Counters.IAPrimitiveTopologySets, Counters.IAVertexBuffersSets, Counters.IAIndexBufferSets, this.terrain.VisiblePatchesCount);
            this.counters1.Text = txt1;
            this.counters2.Text = txt2;

            #endregion
        }

        private Vector3 GetRandomPoint(Random rnd, Vector3 offset)
        {
            BoundingBox bbox = this.terrain.GetBoundingBox();

            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                Vector3 p;
                Triangle t;
                float d;
                if (terrain.FindTopGroundPosition(v.X, v.Z, out p, out t, out d))
                {
                    return p + offset;
                }
            }
        }
        private Curve3D GenerateHelicopterPath()
        {
            Curve3D curve = new Curve3D();
            curve.PreLoop = CurveLoopType.Constant;
            curve.PostLoop = CurveLoopType.Constant;

            Vector3[] cPoints = new Vector3[15];

            Random rnd = new Random();

            if (this.helicopter.Manipulator.IsFollowingPath)
            {
                for (int i = 0; i < cPoints.Length - 2; i++)
                {
                    cPoints[i] = this.GetRandomPoint(rnd, this.helicopterHeightOffset);
                }
            }
            else
            {
                cPoints[0] = this.helicopter.Manipulator.Position;
                cPoints[1] = this.helicopter.Manipulator.Position + (Vector3.Up * 5f) + (this.helicopter.Manipulator.Forward * 10f);

                for (int i = 2; i < cPoints.Length - 2; i++)
                {
                    cPoints[i] = this.GetRandomPoint(rnd, this.helicopterHeightOffset);
                }
            }

            var p = this.helipod.Manipulator.Position;
            Triangle t;
            float d;
            if (this.terrain.FindTopGroundPosition(p.X, p.Z, out p, out t, out d))
            {
                cPoints[cPoints.Length - 2] = p + this.helicopterHeightOffset;
                cPoints[cPoints.Length - 1] = p;
            }

            float time = 0;
            for (int i = 0; i < cPoints.Length; i++)
            {
                if (i > 0) time += Vector3.Distance(cPoints[i - 1], cPoints[i]);

                curve.AddPosition(time, cPoints[i]);
            }

            curve.SetTangents();
            return curve;
        }

        private void DEBUGPickingPosition(Vector3 position)
        {
            Vector3[] positions;
            Triangle[] triangles;
            float[] distances;
            if (this.terrain.FindAllGroundPosition(position.X, position.Z, out positions, out triangles, out distances))
            {
                this.terrainPointDrawer.SetLines(Color.Magenta, Line3D.CreateCrossList(positions, 1f));
                this.terrainPointDrawer.SetLines(Color.DarkCyan, Line3D.CreateWiredTriangle(triangles));
                if (positions.Length > 1)
                {
                    this.terrainPointDrawer.SetLines(Color.Cyan, new Line3D(positions[0], positions[positions.Length - 1]));
                }
            }
        }
        private void DEBUGDrawHelicopterPath(Curve3D curve)
        {
            List<Vector3> path = new List<Vector3>();

            float pass = curve.Length / 500f;

            for (float i = 0; i <= curve.Length; i += pass)
            {
                Vector3 pos = curve.GetPosition(i);

                path.Add(pos);
            }

            this.curveLineDrawer.SetLines(this.curvesColor, Line3D.CreatePath(path.ToArray()));
            this.curveLineDrawer.SetLines(this.pointsColor, Line3D.CreateCrossList(curve.Points, 0.5f));
            this.curveLineDrawer.SetLines(this.segmentsColor, Line3D.CreatePath(curve.Points));
        }
        private void DEBUGDrawTankPath(Vector3 from, PathFindingPath path)
        {
            int count = Math.Min(path.ReturnPath.Count, MaxPickingTest);

            Line3D[] lines = new Line3D[count + 1];

            for (int i = 0; i < count; i++)
            {
                Line3D line;
                if (i == 0)
                {
                    line = new Line3D(from, path.ReturnPath[i]);
                }
                else
                {
                    line = new Line3D(path.ReturnPath[i - 1], path.ReturnPath[i]);
                }

                lines[i] = line;
            }

            this.terrainPointDrawer.SetLines(Color.Red, lines);
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var agent = this.walkMode ? this.walkerAgent : this.tankAgent;

            var nodes = this.terrain.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                Random clrRnd = new Random(1);
                Color[] regions = new Color[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.55f);
                }

                if (this.graphIndex <= -1)
                {
                    this.graphIndex = -1;

                    this.terrainGraphDrawer.Clear();

                    for (int i = 0; i < nodes.Length; i++)
                    {
                        var node = (NavigationMeshNode)nodes[i];
                        var color = regions[node.RegionId];
                        var poly = node.Poly;
                        var tris = poly.Triangulate();

                        this.terrainGraphDrawer.AddTriangles(color, tris);
                    }
                }
                else
                {
                    if (this.graphIndex >= nodes.Length)
                    {
                        this.graphIndex = nodes.Length - 1;
                    }

                    if (this.graphIndex < nodes.Length)
                    {
                        this.terrainGraphDrawer.Clear();

                        var node = (NavigationMeshNode)nodes[this.graphIndex];
                        var color = regions[node.RegionId];
                        var poly = node.Poly;
                        var tris = poly.Triangulate();

                        this.terrainGraphDrawer.SetTriangles(color, tris);
                    }
                }
            }
            else
            {
                this.graphIndex = -1;
            }
        }
        private void DEBUGDrawLightVolumes()
        {
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = spot.GetVolume(10);

                this.lightsVolumeDrawer.AddLines(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = point.GetVolume(12, 5);

                this.lightsVolumeDrawer.AddLines(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
        private void DEBUGDrawLightMarks()
        {
            this.lightsVolumeDrawer.Clear();

            foreach (var spot in this.Lights.SpotLights)
            {
                var lines = Line3D.CreateWiredSphere(spot.BoundingSphere, 10, 10);

                this.lightsVolumeDrawer.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            foreach (var point in this.Lights.PointLights)
            {
                var lines = Line3D.CreateWiredSphere(point.BoundingSphere, 10, 10);

                this.lightsVolumeDrawer.AddLines(new Color4(Color.Red.RGB(), 0.55f), lines);
            }

            this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
        }
    }
}
