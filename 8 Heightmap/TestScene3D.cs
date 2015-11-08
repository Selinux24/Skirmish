using Engine;
using SharpDX;
using System.Diagnostics;

namespace HeightmapTest
{
    public class TestScene3D : Scene
    {
        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.50f;

        private Vector3 playerHeight = Vector3.UnitY * 5f;
        private bool playerFlying = false;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;
        private TextDrawer help2 = null;

        private Cursor cursor;
        private LensFlare lensFlare = null;
        private Skydom skydom = null;
        private Terrain2 terrain = null;
        private LineListDrawer bboxesDrawer = null;

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
            this.terrain = this.AddTerrain2(new TerrainDescription()
            {
                ContentPath = "Resources/Scenery",

                Heightmap = new TerrainDescription.HeightmapDescription()
                {
                    ContentPath = "Heightmap",
                    HeightmapFileName = "desert0hm.bmp",
                    ColormapFileName = "desert0cm.bmp",
                    CellSize = 5,
                    MaximumHeight = 50,
                },
                Quadtree = new TerrainDescription.QuadtreeDescription()
                {

                },
                Textures = new TerrainDescription.TexturesDescription()
                {
                    ContentPath = "Textures",
                    TexturesLR = new[] { "dirt0lr.dds", "dirt1lr.dds", "dirt2lr.dds" },
                    TexturesHR = new[] { "dirt0hr.dds" },
                    NormalMaps = new[] { "dirt0nm.dds" },
                    SlopeRanges = new Vector2(0.1f, 0.3f),
                },
                Vegetation = new TerrainDescription.VegetationDescription()
                {
                    ContentPath = "Foliage/Billboard",
                    VegetarionTextures = new[] { "grass.png" },
                    Saturation = 0.3f,
                    StartRadius = 0f,
                    EndRadius = 200f,
                    MinSize = new Vector2(2, 2),
                    MaxSize = new Vector2(2, 4),
                }
            });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();

            #endregion

            #region Debug

            //BoundingBox[] bboxes = this.terrain.GetBoundingBoxes(5);
            //Line[] listBoxes = GeometryUtil.CreateWiredBox(bboxes);

            //this.bboxesDrawer = this.AddLineListDrawer(listBoxes, Color.Red);
            //this.bboxesDrawer.Visible = false;
            //this.bboxesDrawer.Opaque = false;
            //this.bboxesDrawer.EnableAlphaBlending = true;

            #endregion

            this.load.Text = loadingText;

            #endregion

            Vector3 position;
            if (this.terrain.FindTopGroundPosition(0, 0, out position))
            {
                position += this.playerHeight;

                this.Camera.Goto(position);
                this.Camera.LookTo(position + Vector3.ForwardLH);
            };

            this.Camera.Goto(new Vector3(444.4133f, 43.37331f, -389.4511f));
            this.Camera.LookTo(new Vector3(443.4733f, 43.20348f, -389.1551f));
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
                if (this.terrain.FindTopGroundPosition(this.Camera.Position.X, this.Camera.Position.Z, out position))
                {
                    position += this.playerHeight;

                    this.Camera.Goto(position);
                };
            }

            this.help.Text = string.Format("Eye position {0}; Interest {1}", this.Camera.Position, this.Camera.Interest);
            this.help2.Text = this.Game.RuntimeText;
        }
    }
}
