using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.PathFinding;
using SharpDX;

namespace DeferredTest
{
    public class TestScene3D : Scene
    {
        private Random rnd = new Random();

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;

        private Model tank = null;
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Terrain terrain = null;

        private SpriteTexture bufferDrawer = null;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.DeferredLightning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.5f;
            this.Camera.FarPlaneDistance = 5000f;

            List<SceneLightPoint> lights = new List<SceneLightPoint>();

            for (int i = 0; i < 5; i++)
            {
                for (int x = 0; x < 5; x++)
                {
                    SceneLightPoint pointLight = new SceneLightPoint()
                    {
                        Position = new Vector3((i * 10) - 20, 5, (x * 10) - 20),
                        Range = 8f,
                        Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f),
                        Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f),
                        Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f),
                        Attenuation = new Vector3(1.0f, 0.0f, 0.1f),
                        Enabled = true,
                    };

                    lights.Add(pointLight);
                }
            }

            this.Lights.PointLights = lights.ToArray();

            //Vector3 att = this.Lights.PointLight.Attenuation;

            //System.Collections.Generic.List<float> attList1 = new System.Collections.Generic.List<float>();
            //System.Collections.Generic.List<float> attList2 = new System.Collections.Generic.List<float>();
            //for (int x = 0; x < this.Lights.PointLight.Range + 1; x++)
            //{
            //    Vector3 dist = new Vector3(1, x, x * x);
            //    float attenuation1 = 1.0f / Vector3.Dot(att, dist);
            //    float attenuation2 = 1.0f - x / this.Lights.PointLight.Range;
            //    attList1.Add(attenuation1);
            //    attList2.Add(attenuation2);
            //}

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = false;
            this.Lights.DirectionalLights[2].Enabled = false;

            this.Lights.EnableShadows = false;

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

            #region Debug Buffer Drawer

            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.bufferDrawer = this.AddSpriteTexture(new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = SpriteTextureChannelsEnum.NoAlpha,
            });

            this.bufferDrawer.Visible = false;

            #endregion

            #region Object locations

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

            #endregion
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

            if (this.DrawContext.GeometryMap != null && this.DrawContext.GeometryMap.Length > 0)
            {
                if (this.Game.Input.KeyJustReleased(Keys.F1))
                {
                    this.bufferDrawer.Texture = this.DrawContext.GeometryMap[0];
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F2))
                {
                    this.bufferDrawer.Texture = this.DrawContext.GeometryMap[1];
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F3))
                {
                    this.bufferDrawer.Texture = this.DrawContext.GeometryMap[2];
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                    this.bufferDrawer.Visible = true;
                }
            }

            if (this.DrawContext.LightMap != null)
            {
                if (this.Game.Input.KeyJustReleased(Keys.F4))
                {
                    this.bufferDrawer.Texture = this.DrawContext.LightMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.All;
                    this.bufferDrawer.Visible = true;
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.bufferDrawer.Visible = !this.bufferDrawer.Visible;
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

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                this.helicopters.Visible = !this.helicopters.Visible;
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

            #region Light

            if (this.Game.Input.KeyPressed(Keys.Left))
            {
                this.Lights.PointLights[0].Position += (Vector3.Left) * 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.Right))
            {
                this.Lights.PointLights[0].Position += (Vector3.Right) * 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.Up))
            {
                this.Lights.PointLights[0].Position += (Vector3.ForwardLH) * 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.Down))
            {
                this.Lights.PointLights[0].Position += (Vector3.BackwardLH) * 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.PageUp))
            {
                this.Lights.PointLights[0].Position += (Vector3.Up) * 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.PageDown))
            {
                this.Lights.PointLights[0].Position += (Vector3.Down) * 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                this.Lights.PointLights[0].Range += 0.1f;
            }

            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                this.Lights.PointLights[0].Range -= 0.1f;
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
        }
    }
}
