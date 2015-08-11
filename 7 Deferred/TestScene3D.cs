using System;
using System.Diagnostics;
using Engine;
using Engine.PathFinding;
using SharpDX;

namespace DeferredTest
{
    public class TestScene3D : Scene
    {
        private Random rnd = new Random();

        private SpriteTexture gBufferDrawer = null;
        private int bufferIndex = 0;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;

        private Model tank = null;
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Terrain terrain = null;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.DeferredLightning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 5000f;

            #region Texts

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.load = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.title.Text = "Deferred Ligthning test";
            this.load.Text = "";
            this.help.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, 24);
            this.help.Position = new Vector2(0, 48);

            #endregion

            #region Models

            string resources = @"Resources";

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Terrain

            sw.Restart();
            this.terrain = this.AddTerrain(new TerrainDescription()
            {
                ContentPath = resources,
                ModelFileName = "terrain.dae",
                UseQuadtree = true,
                UsePathFinding = true,
                PathNodeSize = 2f,
                PathNodeInclination = MathUtil.DegreesToRadians(35),
                AddSkydom = true,
                SkydomTexture = "sunset.dds",
                AddVegetation = true,
                Vegetation = new[]
                {
                    new TerrainDescription.VegetationDescription()
                    {
                        VegetarionTextures = new[] { "tree0.dds", "tree1.dds" },
                        Saturation = 0.5f,
                        Opaque = true,
                        Radius = 300f,
                        MinSize = Vector2.One * 2.50f,
                        MaxSize = Vector2.One * 3.50f,
                    },
                    new TerrainDescription.VegetationDescription()
                    {
                        VegetarionTextures = new[] { "grass.png" },
                        Saturation = 10f,
                        Opaque = false,
                        Radius = 50f,
                        MinSize = Vector2.One * 0.20f,
                        MaxSize = Vector2.One * 0.25f,
                    }
                },
                Opaque = true,
            });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();

            #endregion

            #region Helicopter

            sw.Restart();
            this.helicopter = this.AddModel(new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "helicopter.dae",
                Opaque = true,
                TextureIndex = 2,
            });
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopters

            sw.Restart();
            this.helicopters = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = resources,
                ModelFileName = "helicopter.dae",
                Opaque = true,
                Instances = 2,
            });
            sw.Stop();
            loadingText += string.Format("helicopters: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Tank

            sw.Restart();
            this.tank = this.AddModel(new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "tank.dae",
                Opaque = true,
            });
            sw.Stop();
            loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);

            this.tank.Manipulator.SetScale(3);

            #endregion

            this.load.Text = loadingText;

            #endregion

            #region G Buffer

            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.gBufferDrawer = this.AddSpriteTexture(new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = SpriteTextureChannelsEnum.NoAlpha,
            });

            #endregion

            Vector3 cameraPosition = Vector3.Zero;
            int modelCount = 0;

            Vector3 tankPosition;
            if (this.terrain.FindTopGroundPosition(0, 0, out tankPosition))
            {
                this.tank.Manipulator.SetPosition(tankPosition, true);
                cameraPosition += tankPosition;
                modelCount++;
            }

            Vector3 helicopterPosition;
            if (this.terrain.FindTopGroundPosition(20, -20, out helicopterPosition))
            {
                this.helicopter.Manipulator.SetPosition(helicopterPosition, true);
                cameraPosition += helicopterPosition;
                modelCount++;
            }

            for (int i = 0; i < this.helicopters.Count; i++)
            {
                Vector3 heliPos;
                if (this.terrain.FindTopGroundPosition((i * 10) - 20, 20, out heliPos))
                {
                    this.helicopters.Instances[i].Manipulator.SetPosition(heliPos, true);
                    cameraPosition += heliPos;
                    modelCount++;
                }
            }

            cameraPosition /= (float)modelCount;
            this.Camera.Goto(cameraPosition + (Vector3.One * 30f));
            this.Camera.LookTo(cameraPosition + Vector3.Up);

            this.Lights.PointLight.Position = Vector3.Zero;
            this.Lights.PointLight.Range = 15f;
            this.Lights.PointLight.Enabled = true;

            this.Lights.DirectionalLight1.Enabled = false;
            this.Lights.DirectionalLight2.Enabled = false;
            this.Lights.DirectionalLight3.Enabled = false;

            this.Lights.EnableShadows = false;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            Ray cursorRay = this.GetPickingRay();

            #region Cursor picking and positioning

            Vector3 position;
            Triangle triangle;
            bool picked = this.terrain.PickNearest(ref cursorRay, out position, out triangle);

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bufferIndex = 0;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.bufferIndex = 1;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.bufferIndex = 2;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.bufferIndex = -1;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.gBufferDrawer.Visible = !this.gBufferDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.terrain.Visible = !this.terrain.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.tank.Visible = !this.tank.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.helicopter.Visible = !this.helicopter.Visible;
            }

            #endregion

            #region Tank

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (picked)
                {
                    Path p = this.terrain.FindPath(this.tank.Manipulator.Position, position);
                    if (p != null)
                    {
                        this.tank.Manipulator.Follow(p.GenerateBezierPath(), 0.2f);
                    }
                }
            }

            #endregion

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

            if (this.Game.Form.IsFullscreen)
            {
                this.load.Text = this.Game.RuntimeText;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.bufferIndex >= 0 && this.DrawContext.GBuffer != null && this.DrawContext.GBuffer.Length > 0)
            {
                this.gBufferDrawer.Texture = this.bufferIndex >= 0 ? this.DrawContext.GBuffer[this.bufferIndex] : null;
            }
            else
            {
                this.gBufferDrawer.Texture = null;
            }
        }
    }
}
