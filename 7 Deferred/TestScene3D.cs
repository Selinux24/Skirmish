using Engine;
using Engine.Common;
using Engine.PathFinding;
using SharpDX;
using System;
using System.Diagnostics;

namespace DeferredTest
{
    public class TestScene3D : Scene
    {
        private string titleMask = "DL test: {0} directionals, {1} points and {2} spots. Shadows {3}";

        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.10f;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;
        private TextDrawer statistics = null;

        private Model tank = null;
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Terrain terrain = null;
        private ParticleSystem fire = null;

        private SpriteTexture bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private LineListDrawer lineDrawer = null;

        private Random rnd = new Random(0);

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.DeferredLightning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;

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
                        Saturation = 0.15f,
                        Radius = 300f,
                        MinSize = Vector2.One * 5f,
                        MaxSize = Vector2.One * 10f,
                    },
                    new TerrainDescription.VegetationDescription()
                    {
                        VegetarionTextures = new[] { "grass.png" },
                        Saturation = 20f,
                        Radius = 50f,
                        MinSize = Vector2.One * 0.20f,
                        MaxSize = Vector2.One * 0.25f,
                    }
                },
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

            #region Moving fire

            this.fire = this.AddParticleSystem(ParticleSystemDescription.Fire(new[] { new Vector3(0, 10, 0) }, 0.5f, Color.Yellow, "flare2.png"));

            #endregion

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

            #region Texts

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.load = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.statistics = this.AddText("Lucida Casual", 10, Color.Red);

            this.title.Text = "Deferred Ligthning test";
            this.load.Text = loadingText;
            this.help.Text = "";
            this.statistics.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, this.title.Top + this.title.Height + 2);
            this.help.Position = new Vector2(0, this.load.Top + this.load.Height + 2);
            this.statistics.Position = new Vector2(0, this.help.Top + this.help.Height + 2);

            #endregion

            #region Object locations

            Vector3 cameraPosition = Vector3.Zero;
            int modelCount = 0;

            Vector3 tankPosition;
            Triangle tankTriangle;
            if (this.terrain.FindTopGroundPosition(0, 0, out tankPosition, out tankTriangle))
            {
                //Inclination
                this.tank.Manipulator.SetPosition(tankPosition, true);
                this.tank.Manipulator.SetNormal(tankTriangle.Normal);
                cameraPosition += tankPosition;
                modelCount++;
            }

            Vector3 helicopterPosition;
            if (this.terrain.FindTopGroundPosition(20, -20, out helicopterPosition))
            {
                helicopterPosition.Y += 10f;
                this.helicopter.Manipulator.SetPosition(helicopterPosition, true);
                cameraPosition += helicopterPosition;
                modelCount++;
            }

            for (int i = 0; i < this.helicopters.Count; i++)
            {
                Vector3 heliPos;
                if (this.terrain.FindTopGroundPosition((i * 10) - 20, 20, out heliPos))
                {
                    heliPos.Y += 10f;
                    this.helicopters.Instances[i].Manipulator.SetPosition(heliPos, true);
                    cameraPosition += heliPos;
                    modelCount++;
                }
            }

            cameraPosition /= (float)modelCount;
            this.Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            this.Camera.LookTo(cameraPosition + Vector3.Up);

            #endregion

            #region Lights

            //SceneLightDirectional primary = SceneLightDirectional.Primary;
            SceneLightDirectional primary = new SceneLightDirectional()
            {
                Name = "night has come",
                Enabled = true,
                LightColor = Color.LightBlue,
                AmbientIntensity = 0.25f,
                DiffuseIntensity = 0.25f,
                Direction = SceneLightDirectional.Primary.Direction,
                CastShadow = false,
            };

            this.Lights.ClearDirectionalLights();
            this.Lights.Add(primary);

            this.Lights.FogColor = Color.LightGray;
            this.Lights.FogStart = far * fogStart;
            this.Lights.FogRange = far * fogRange;

            #region Light Sphere Marker

            Line[] axis = GeometryUtil.CreateAxis(Matrix.Identity, 5f);

            this.lineDrawer = this.AddLineListDrawer(axis, Color.Red);
            this.lineDrawer.Opaque = false;
            this.lineDrawer.Active = false;
            this.lineDrawer.Visible = false;

            #endregion

            #endregion
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

            if (this.Game.Input.KeyJustReleased(Keys.F12))
            {
                if (this.bufferDrawer.Manipulator.Position == Vector2.Zero)
                {
                    int width = (int)(this.Game.Form.RenderWidth * 0.33f);
                    int height = (int)(this.Game.Form.RenderHeight * 0.33f);
                    int smLeft = this.Game.Form.RenderWidth - width;
                    int smTop = this.Game.Form.RenderHeight - height;

                    this.bufferDrawer.Manipulator.SetPosition(smLeft, smTop);
                    this.bufferDrawer.Resize(width, height);
                }
                else
                {
                    this.bufferDrawer.Manipulator.SetPosition(Vector2.Zero);
                    this.bufferDrawer.Resize(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
                }
            }

            if (this.DrawContext.GeometryMap != null && this.DrawContext.GeometryMap.Length > 0)
            {
                if (this.Game.Input.KeyJustReleased(Keys.F1))
                {
                    if (this.bufferDrawer.Texture == this.DrawContext.GeometryMap[0] &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Factor
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[0];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Factor";
                    }
                    else
                    {
                        //Colors
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[0];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Colors";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F2))
                {
                    if (this.bufferDrawer.Texture == this.DrawContext.GeometryMap[1] &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Power
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[1];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Power";
                    }
                    else
                    {
                        //Normals
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[1];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Normals";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F3))
                {
                    if (this.bufferDrawer.Texture == this.DrawContext.GeometryMap[2] &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Depth
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[2];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Depth";
                    }
                    else
                    {
                        //Position
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[2];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Position";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F4))
                {
                    if (this.bufferDrawer.Texture == this.DrawContext.GeometryMap[3] &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular intensity
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[3];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Intensity";
                    }
                    else
                    {
                        //Shadow positions
                        this.bufferDrawer.Texture = this.DrawContext.GeometryMap[3];
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Shadow Positions";
                    }
                    this.bufferDrawer.Visible = true;
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                if (this.DrawContext.ShadowMap != null)
                {
                    //Shadow map
                    this.bufferDrawer.Texture = this.DrawContext.ShadowMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Red;
                    this.bufferDrawer.Visible = true;
                    this.help.Text = "Shadow map";
                }
                else
                {
                    this.help.Text = "The Shadow map is empty";
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                if (this.DrawContext.LightMap != null)
                {
                    //Light map
                    this.bufferDrawer.Texture = this.DrawContext.LightMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.bufferDrawer.Visible = true;
                    this.help.Text = "Light map";
                }
                else
                {
                    this.help.Text = "The Light map is empty";
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.bufferDrawer.Visible = !this.bufferDrawer.Visible;
                this.help.Visible = this.bufferDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.terrain.Active = this.terrain.Visible = !this.terrain.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                this.tank.Active = this.tank.Visible = !this.tank.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F10))
            {
                this.helicopter.Active = this.helicopter.Visible = !this.helicopter.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F11))
            {
                this.helicopters.Active = this.helicopters.Visible = !this.helicopters.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.textIntex--;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.textIntex++;
            }

            if (this.Game.Input.KeyJustReleased(Keys.T))
            {
                this.helicopter.TextureIndex++;

                if (this.helicopter.TextureIndex > 2) this.helicopter.TextureIndex = 0;
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

            if (this.tank.Manipulator.IsFollowingPath)
            {
                Vector3 pos = this.tank.Manipulator.Position;

                Vector3 tankPosition;
                Triangle tankTriangle;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out tankPosition, out tankTriangle))
                {
                    this.tank.Manipulator.SetNormal(tankTriangle.Normal);
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

            #region Lights

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.Lights.FogStart = this.Lights.FogStart == 0f ? far * fogStart : 0f;
                this.Lights.FogRange = this.Lights.FogRange == 0f ? far * fogRange : 0f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.K))
            {
                if (this.spotLight == null)
                {
                    this.CreateSpotLights();
                }
                else
                {
                    this.ClearSpotLights();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                if (this.Lights.EnabledPointLights.Length > 0)
                {
                    this.ClearPointLigths();
                }
                else
                {
                    this.CreatePointLigths();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.animateLights = !this.animateLights;
            }

            if (this.spotLight != null)
            {
                if (this.Game.Input.KeyPressed(Keys.Left))
                {
                    this.spotLight.Position += (Vector3.Left) * 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.Right))
                {
                    this.spotLight.Position += (Vector3.Right) * 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.Up))
                {
                    this.spotLight.Position += (Vector3.ForwardLH) * 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.Down))
                {
                    this.spotLight.Position += (Vector3.BackwardLH) * 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.PageUp))
                {
                    this.spotLight.Position += (Vector3.Up) * 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.PageDown))
                {
                    this.spotLight.Position += (Vector3.Down) * 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.Add))
                {
                    //this.spotLight.Range += 0.1f;
                }

                if (this.Game.Input.KeyPressed(Keys.Subtract))
                {
                    //this.spotLight.Range -= 0.1f;
                }

                this.lineDrawer.Manipulator.SetPosition(this.spotLight.Position);
                this.lineDrawer.Manipulator.LookAt(this.spotLight.Position + this.spotLight.Direction, false);
            }
            else
            {
                this.lineDrawer.Visible = false;
            }

            if (animateLights)
            {
                if (this.Lights.EnabledPointLights.Length > 0)
                {
                    for (int i = 1; i < this.Lights.EnabledPointLights.Length; i++)
                    {
                        var l = this.Lights.EnabledPointLights[i];

                        if ((int)l.State == 1) l.Radius += (0.5f * gameTime.ElapsedSeconds * 50f);
                        if ((int)l.State == -1) l.Radius -= (2f * gameTime.ElapsedSeconds * 50f);

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

                        l.DiffuseIntensity = l.Radius * 0.1f;
                    }
                }
            }

            #endregion
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.Game.Form.IsFullscreen)
            {
                this.load.Text = this.Game.RuntimeText;
            }

            this.title.Text = string.Format(
                this.titleMask,
                this.Lights.EnabledDirectionalLights.Length,
                this.Lights.EnabledPointLights.Length,
                this.Lights.EnabledSpotLights.Length,
                this.Lights.ShadowCastingLights.Length);

            if (Counters.Statistics.Length == 0)
            {
                this.statistics.Text = "No statistics";
            }
            else if (this.textIntex < 0)
            {
                this.statistics.Text = "Press right arrow for more statistics";
                this.textIntex = -1;
            }
            else if (this.textIntex >= Counters.Statistics.Length)
            {
                this.statistics.Text = "Press left arrow for more statistics";
                this.textIntex = Counters.Statistics.Length;
            }
            else
            {
                this.statistics.Text = string.Format(
                    "{0} - {1}",
                    Counters.Statistics[this.textIntex],
                    Counters.GetStatistics(this.textIntex));
            }
        }

        private void CreateSpotLights()
        {
            this.Lights.ClearSpotLights();

            this.spotLight = new SceneLightSpot()
            {
                Enabled = true,
                LightColor = Color.Yellow,
                Attenuation = new Vector3(1.0f, 0.0f, 0.1f),
                Position = new Vector3(0, 15, 0),
                Direction = Vector3.Down,
                Spot = 20,
            };

            this.Lights.Add(this.spotLight);

            this.lineDrawer.Active = true;
            this.lineDrawer.Visible = true;
        }
        private void ClearSpotLights()
        {
            this.Lights.ClearSpotLights();

            this.lineDrawer.Active = false;
            this.lineDrawer.Visible = false;
        }

        private void CreatePointLigths()
        {
            this.Lights.ClearPointLights();

            int f = 12;
            int l = (f - 1) * 5;

            for (int i = 0; i < f; i++)
            {
                for (int x = 0; x < f; x++)
                {
                    Vector3 lightPosition;
                    if (!this.terrain.FindTopGroundPosition((i * 10) - l, (x * 10) - l, out lightPosition))
                    {
                        lightPosition = new Vector3((i * 10) - l, 1f, (x * 10) - l);
                    }
                    else
                    {
                        lightPosition.Y += 1f;
                    }

                    SceneLightPoint pointLight = new SceneLightPoint()
                    {
                        Name = string.Format("Point {0}", this.Lights.PointLights.Length),
                        Enabled = true,
                        LightColor = new Color4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1.0f),
                        AmbientIntensity = rnd.NextFloat(0, 1),
                        DiffuseIntensity = rnd.NextFloat(0, 1),
                        Position = lightPosition,
                        Radius = rnd.NextFloat(1, 25),
                        State = rnd.NextFloat(0, 1) >= 0.5f ? 1 : -1,
                    };

                    this.Lights.Add(pointLight);
                }
            }
        }
        private void ClearPointLigths()
        {
            this.Lights.ClearPointLights();
        }
    }
}
