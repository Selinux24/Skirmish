using Engine;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Diagnostics;

namespace HeightmapTest
{
    public class TestScene3D : Scene
    {
        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.50f;

        private Random rnd = new Random();

        private Vector3 playerHeight = Vector3.UnitY * 5f;
        private bool playerFlying = true;

        private Vector3 windDirection = Vector3.UnitX;
        private float windStrength = 1f;
        private float windNextStrength = 1f;
        private float windStep = 0.001f;
        private float windDuration = 0;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;
        private TextDrawer help2 = null;

        private Cursor cursor;
        private LensFlare lensFlare = null;
        private Skydom skydom = null;
        private Terrain terrain = null;
        private LineListDrawer bboxesDrawer = null;

        private ModelInstanced torchs = null;
        private SceneLightPoint[] torchLights = null;
        private SceneLightSpot spotLight1 = null;
        private SceneLightSpot spotLight2 = null;

        private Model soldier = null;
        //private TriangleListDrawer soldierTris = null;
        private LineListDrawer soldierLines = null;

        private Model helicopter = null;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Lights.FogColor = Color.WhiteSmoke;
            this.Lights.FogStart = 0;
            this.Lights.FogRange = 0;

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = false;
            this.Lights.DirectionalLights[2].Enabled = false;

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;

            #region Cursor

            SpriteDescription cursorDesc = new SpriteDescription()
            {
                Textures = new[] { "target.png" },
                Width = 16,
                Height = 16,
            };

            this.cursor = this.AddCursor(cursorDesc);

            #endregion

            #region Texts

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.load = this.AddText("Tahoma", 11, Color.Yellow);
            this.help = this.AddText("Tahoma", 11, Color.Yellow);
            this.help2 = this.AddText("Tahoma", 11, Color.Orange);

            this.title.Text = "Heightmap Terrain test";
            this.load.Text = "";
            this.help.Text = "";
            this.help2.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(5, this.title.Top + this.title.Height + 3);
            this.help.Position = new Vector2(5, this.load.Top + this.load.Height + 3);
            this.help2.Position = new Vector2(5, this.help.Top + this.help.Height + 3);

            #endregion

            #region Models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Lens flare

            this.lensFlare = this.AddLensFlare(new LensFlareDescription()
            {
                ContentPath = @"Resources/Scenery/Flare",
                GlowTexture = "lfGlow.png",
                Flares = new FlareDescription[]
                {
                    new FlareDescription(-0.5f, 0.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new FlareDescription( 0.3f, 0.4f, new Color(100, 255, 200), "lfFlare1.png"),
                    new FlareDescription( 1.2f, 1.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new FlareDescription( 1.5f, 1.5f, new Color( 50, 100,  50), "lfFlare1.png"),

                    new FlareDescription(-0.3f, 0.7f, new Color(200,  50,  50), "lfFlare2.png"),
                    new FlareDescription( 0.6f, 0.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new FlareDescription( 0.7f, 0.4f, new Color( 50, 200, 200), "lfFlare2.png"),

                    new FlareDescription(-0.7f, 0.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new FlareDescription( 0.0f, 0.6f, new Color( 25,  25,  25), "lfFlare3.png"),
                    new FlareDescription( 2.0f, 1.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            });

            this.lensFlare.Light = this.Lights.DirectionalLights[0];

            #endregion

            #region Skydom

            sw.Restart();
            this.skydom = this.AddSkydom(new SkydomDescription()
            {
                ContentPath = @"Resources/Scenery/Skydom",
                Radius = far,
                Texture = "sunset.dds",
            });
            sw.Stop();
            loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Terrain

            sw.Restart();

            var pfSettings = NavigationMeshGenerationSettings.Default;
            pfSettings.CellHeight = 20f;
            pfSettings.CellSize = 20f;

            this.terrain = this.AddTerrain(new GroundDescription()
            {
                ContentPath = "Resources/Scenery",

                Heightmap = new GroundDescription.HeightmapDescription()
                {
                    ContentPath = "Heightmap",
                    HeightmapFileName = "desert0hm.bmp",
                    ColormapFileName = "desert0cm.bmp",
                    CellSize = 5,
                    MaximumHeight = 50,
                },
                Quadtree = new GroundDescription.QuadtreeDescription()
                {
                    MaximumDepth = 3,
                },
                //PathFinder = new GroundDescription.PathFinderDescription()
                //{
                //    Settings = pfSettings,
                //},
                Textures = new GroundDescription.TexturesDescription()
                {
                    ContentPath = "Textures",
                    NormalMaps = new[] { "normal001.dds", "normal002.dds" },

                    UseAlphaMapping = false,
                    AlphaMap = "alpha001.dds",
                    ColorTextures = new[] { "dirt001.dds", "dirt002.dds", "dirt004.dds", "stone001.dds" },

                    UseSlopes = true,
                    SlopeRanges = new Vector2(0.005f, 0.25f),
                    //TexturesLR = new[] { "dirt0lr.dds", "dirt1lr.dds", "dirt2lr.dds" },
                    TexturesLR = new[] { "am01.jpg", "am02.jpg", "am04.jpg" },
                    TexturesHR = new[] { "dirt0hr.dds" },

                    Proportion = 0.25f,
                },
                //Vegetation = new GroundDescription.VegetationDescription()
                //{
                //    ContentPath = "Foliage/Billboard",
                //    VegetarionTextures = new[] { "grass.png" },
                //    Saturation = 0.3f,
                //    StartRadius = 0f,
                //    EndRadius = 200f,
                //    MinSize = new Vector2(2, 2),
                //    MaxSize = new Vector2(2, 4),
                //}
            });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();
            this.terrain.SetWind(this.windDirection, this.windStrength);

            #endregion

            #region Soldier

            this.soldier = this.AddModel(new ModelDescription()
            {
                ContentPath = @"Resources/Soldier",
                ModelFileName = "soldier.dae",
            });

            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(0, 0, out position, out triangle, out distance))
                {
                    this.soldier.Manipulator.SetPosition(position, true);
                }
            }

            this.playerHeight.Y = this.soldier.GetBoundingBox().Maximum.Y - this.soldier.GetBoundingBox().Minimum.Y;

            var bbox2 = this.soldier.GetBoundingBox();
            this.soldierLines = this.AddLineListDrawer(Line3.CreateWiredBox(bbox2), Color.White);

            //Matrix baseTrn = Matrix.Translation(this.soldier.Manipulator.Position + (Vector3.Left * 5));

            //Triangle[] tris = this.soldier.GetPoseAtTime(0, baseTrn);
            //this.soldierTris = this.AddTriangleListDrawer(tris, new Color(Color.Red.ToColor3(), 0.6f));
            //this.soldierTris.EnableDepthStencil = false;

            //Line3[] lines = this.soldier.GetSkeletonAtTime(0, baseTrn);
            //this.soldierLines = this.AddLineListDrawer(lines, Color.White);
            //this.soldierLines.EnableDepthStencil = false;

            #endregion

            #region Helicopter

            this.helicopter = this.AddModel(new ModelDescription()
            {
                ContentPath = @"Resources/Helicopter",
                ModelFileName = "Helicopter.dae",
            });

            this.helicopter.Manipulator.SetScale(10, true);

            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(100, 100, out position, out triangle, out distance))
                {
                    this.helicopter.Manipulator.SetPosition(position, true);
                }
            }

            #endregion

            #region Torchs

            int torchCount = 50;
            Random rnd = new Random(1);

            var bbox = this.terrain.GetBoundingBox();

            this.torchs = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = "Resources/Scenery/Objects",
                ModelFileName = "torch.dae",
                Instances = torchCount,
                Opaque = true,
            });

            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(5, 5, out position, out triangle, out distance))
                {
                    this.torchs.Instances[0].Manipulator.SetScale(1f, 1f, 1f, true);
                    this.torchs.Instances[0].Manipulator.SetPosition(position, true);
                    BoundingBox tbbox = this.torchs.Instances[0].GetBoundingBox();

                    position.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                    this.spotLight1 = new SceneLightSpot(position, Vector3.Normalize(Vector3.One * -1f), 25, 25)
                    {
                        Name = "Spot",
                        LightColor = Color.Red,
                        AmbientIntensity = 0.2f,
                        DiffuseIntensity = 10f,
                        Enabled = true,
                        CastShadow = false,
                    };

                    this.spotLight2 = new SceneLightSpot(position, Vector3.Normalize(Vector3.One * -1f), 25, 25)
                    {
                        Name = "Spot",
                        LightColor = Color.Blue,
                        AmbientIntensity = 0.2f,
                        DiffuseIntensity = 10f,
                        Enabled = true,
                        CastShadow = false,
                    };

                    this.Lights.Add(this.spotLight1);
                    this.Lights.Add(this.spotLight2);
                };
            }

            this.torchLights = new SceneLightPoint[torchCount - 1];
            for (int i = 1; i < torchCount; i++)
            {
                Color color = new Color(
                    rnd.NextFloat(0, 1),
                    rnd.NextFloat(0, 1),
                    rnd.NextFloat(0, 1),
                    1);

                Vector3 pos = new Vector3(
                    rnd.NextFloat(bbox.Minimum.X, bbox.Maximum.X),
                    0f,
                    rnd.NextFloat(bbox.Minimum.Z, bbox.Maximum.Z));

                Triangle t;
                float d;
                this.terrain.FindTopGroundPosition(pos.X, pos.Z, out pos, out t, out d);

                this.torchs.Instances[i].Manipulator.SetScale(0.20f, true);
                this.torchs.Instances[i].Manipulator.SetPosition(pos, true);
                BoundingBox tbbox = this.torchs.Instances[i].GetBoundingBox();

                pos.Y += (tbbox.Maximum.Y - tbbox.Minimum.Y) * 0.95f;

                this.torchLights[i - 1] = new SceneLightPoint()
                {
                    Name = string.Format("Torch {0}", i),
                    LightColor = color,
                    AmbientIntensity = 0.1f,
                    DiffuseIntensity = 5f,
                    Position = pos,
                    Radius = 4f,
                    Enabled = true,
                    CastShadow = false,
                };

                this.Lights.Add(this.torchLights[i - 1]);
            }

            #endregion

            this.load.Text = loadingText;

            #endregion

            {
                this.Camera.Goto(this.soldier.Manipulator.Position + this.playerHeight + new Vector3(8, 5, -16));
                this.Camera.LookTo(this.soldier.Manipulator.Position + this.playerHeight + new Vector3(0, 2f, 0));
            }

            #region Debug

            var bboxes = this.terrain.GetBoundingBoxes(5);
            var listBoxes = Line3.CreateWiredBox(bboxes);

            this.bboxesDrawer = this.AddLineListDrawer(listBoxes, Color.Red);
            this.bboxesDrawer.Visible = false;
            this.bboxesDrawer.Opaque = false;
            this.bboxesDrawer.EnableAlphaBlending = true;

            #endregion
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.Lights.FogStart = this.Lights.FogStart == 0f ? far * fogStart : 0f;
                this.Lights.FogRange = this.Lights.FogRange == 0f ? far * fogRange : 0f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            Ray cursorRay = this.GetPickingRay();

            #region Camera

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
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

            #endregion

            #region Walk / Fly

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.playerFlying = !this.playerFlying;
            }

            #endregion

            #region Wind

            this.windDuration += gameTime.ElapsedSeconds;
            if (this.windDuration > 10)
            {
                this.windDuration = 0;

                this.windNextStrength = this.windStrength + this.rnd.NextFloat(-0.5f, +0.5f);
                if (this.windNextStrength > 100f) this.windNextStrength = 100f;
                if (this.windNextStrength < 0f) this.windNextStrength = 0f;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                this.windStrength += this.windStep;
                if (this.windStrength > 100f) this.windStrength = 100f;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                this.windStrength -= this.windStep;
                if (this.windStrength < 0f) this.windStrength = 0f;
            }

            if (this.windNextStrength < this.windStrength)
            {
                this.windStrength -= this.windStep;
                if (this.windNextStrength > this.windStrength) this.windStrength = this.windNextStrength;
            }
            if (this.windNextStrength > this.windStrength)
            {
                this.windStrength += this.windStep;
                if (this.windNextStrength < this.windStrength) this.windStrength = this.windNextStrength;
            }

            this.terrain.SetWind(this.windDirection, this.windStrength);

            #endregion

            {
                float d = 1f;
                float v = 5f;

                var x = d * (float)Math.Cos(v * this.Game.GameTime.TotalSeconds);
                var z = d * (float)Math.Sin(v * this.Game.GameTime.TotalSeconds);

                this.spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
                this.spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));
            }

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            #endregion

            base.Update(gameTime);

            if (!this.playerFlying)
            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(this.Camera.Position.X, this.Camera.Position.Z, out position, out triangle, out distance))
                {
                    position += this.playerHeight;

                    this.Camera.Goto(position);
                };
            }

            this.help.Text = string.Format(
                "{0}. Wind {1} {2} - Next {3}",
                this.Renderer,
                this.windDirection, this.windStrength, this.windNextStrength);

            this.help2.Text = this.Game.RuntimeText;
        }
    }
}
