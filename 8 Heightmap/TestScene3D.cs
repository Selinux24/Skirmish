using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.PathFinding.NavMesh;
using SharpDX;
using System;
using System.Diagnostics;

namespace HeightmapTest
{
    public class TestScene3D : Scene
    {
        private const float near = 1f;
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

        private ModelInstanced rocks = null;

        private Model soldier = null;
        private TriangleListDrawer soldierTris = null;
        private LineListDrawer soldierLines = null;
        private bool showSoldierDEBUG = false;

        private ModelInstanced troops = null;

        private Model helicopter = null;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            Random rnd = new Random(1);

            this.Lights.FogColor = new Color((byte)54, (byte)56, (byte)68);
            this.Lights.FogStart = 0;
            this.Lights.FogRange = 0;

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = true;

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

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White));
            this.load = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow));
            this.help = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow));
            this.help2 = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Orange));

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

            #region Rocks

            sw.Restart();
            this.rocks = this.AddInstancingModel(
                new ModelContentDescription()
                {
                    ContentPath = "Resources/Rocks",
                    ModelFileName = "boulder.dae",
                },
                new ModelInstancedDescription()
                {
                    Name = "DEBUG_CUBE_INSTANCED",
                    CastShadow = true,
                    Static = true,
                    Instances = 250,
                });
            sw.Stop();
            loadingText += string.Format("rocks: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Soldier model content (for soldier and troops)

            AnimationDescription ani = new AnimationDescription();
            ani.AddClip("idle1", 0, 7);
            ani.AddClip("idle2", 7, 17);
            ani.AddClip("stand", 18, 20);
            ani.AddClip("walk", 21, 29);
            ani.AddClip("run", 30, 42);

            ani.AddTransition("stand", "idle1", 0f, 0f);
            ani.AddTransition("idle1", "stand", 0f, 0f);
            ani.AddTransition("stand", "idle2", 0f, 0f);
            ani.AddTransition("idle2", "stand", 0f, 0f);

            ani.AddTransition("stand", "walk", 0f, 0f);
            ani.AddTransition("walk", "stand", 0f, 0f);

            ani.AddTransition("stand", "run", 0f, 0f);
            ani.AddTransition("run", "stand", 0f, 0f);

            ani.AddTransition("walk", "run", 0f, 0f);
            ani.AddTransition("run", "walk", 0f, 0f);

            ModelContentDescription soldierModel = new ModelContentDescription()
            {
                ContentPath = @"Resources/Soldier",
                ModelFileName = "soldier_anim2.dae",
                Animation = ani,
            };

            #endregion

            #region Soldier

            this.soldier = this.AddModel(
                soldierModel,
                new ModelDescription()
                {
                    TextureIndex = 0,
                });

            this.playerHeight.Y = this.soldier.GetBoundingBox().Maximum.Y - this.soldier.GetBoundingBox().Minimum.Y;

            #endregion

            #region Troops

            this.troops = this.AddInstancingModel(
                soldierModel,
                new ModelInstancedDescription()
                {
                    Instances = 4,
                });

            #endregion

            #region Helicopter

            this.helicopter = this.AddModel(
                new ModelContentDescription()
                {
                    ContentPath = @"Resources/m24",
                    ModelFileName = "m24.dae",
                },
                new ModelDescription() { });

            #endregion

            #region Torchs

            this.torchs = this.AddInstancingModel(
                new ModelContentDescription()
                {
                    ContentPath = "Resources/Scenery/Objects",
                    ModelFileName = "torch.dae",
                },
                new ModelInstancedDescription()
                {
                    Instances = 50,
                    CastShadow = true,
                });

            #endregion

            #region Terrain

            sw.Restart();

            var pfSettings = NavigationMeshGenerationSettings.Default;
            pfSettings.CellHeight = 20f;
            pfSettings.CellSize = 20f;

            this.terrain = this.AddTerrain(
                new HeightmapDescription()
                {
                    ContentPath = "Resources/Scenery/Heightmap",
                    HeightmapFileName = "desert0hm.bmp",
                    ColormapFileName = "desert0cm.bmp",
                    CellSize = 5,
                    MaximumHeight = 50,
                    Textures = new HeightmapDescription.TexturesDescription()
                    {
                        ContentPath = "Textures",
                        NormalMaps = new[] { "normal001.dds", "normal002.dds" },

                        UseAlphaMapping = true,
                        AlphaMap = "alpha001.dds",
                        ColorTextures = new[] { "dirt001.dds", "dirt002.dds", "dirt004.dds", "stone001.dds" },

                        UseSlopes = false,
                        SlopeRanges = new Vector2(0.005f, 0.25f),
                        //TexturesLR = new[] { "dirt0lr.dds", "dirt1lr.dds", "dirt2lr.dds" },
                        TexturesLR = new[] { "am01.jpg", "am02.jpg", "am04.jpg" },
                        TexturesHR = new[] { "dirt0hr.dds" },

                        Proportion = 0.25f,
                    },
                },
                new GroundDescription()
                {
                    Quadtree = new GroundDescription.QuadtreeDescription()
                    {
                        MaximumDepth = 3,
                    },
                    //PathFinder = new GroundDescription.PathFinderDescription()
                    //{
                    //    Settings = pfSettings,
                    //},
                    //Vegetation = new GroundDescription.VegetationDescription()
                    //{
                    //    ContentPath = "Resources/Scenery/Foliage/Billboard",
                    //    VegetarionTextures = new[] { "grass.png" },
                    //    Saturation = 0.3f,
                    //    StartRadius = 0f,
                    //    EndRadius = 200f,
                    //    MinSize = new Vector2(1f, 1f),
                    //    MaxSize = new Vector2(1.5f, 2f),
                    //}
                });
            this.terrain.SetWind(this.windDirection, this.windStrength);
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();

            #endregion

            this.load.Text = loadingText;

            #endregion

            #region Positioning

            //Helicopter
            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(100, 100, out position, out triangle, out distance))
                {
                    this.helicopter.Manipulator.SetPosition(position, true);
                }
            }

            //Rocks
            {
                Random posRnd = new Random(1);

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
                            scale = posRnd.NextFloat(10f, 30f);
                        }
                        else if (i < 30)
                        {
                            scale = posRnd.NextFloat(2f, 5f);
                        }
                        else
                        {
                            scale = posRnd.NextFloat(0.1f, 1f);
                        }

                        this.rocks.Instances[i].Manipulator.SetPosition(rockPosition, true);
                        this.rocks.Instances[i].Manipulator.SetRotation(posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), posRnd.NextFloat(0, MathUtil.TwoPi), true);
                        this.rocks.Instances[i].Manipulator.SetScale(scale, true);
                    }
                }
            }

            //Torchs
            {
                var bbox = this.terrain.GetBoundingBox();

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

                this.torchLights = new SceneLightPoint[this.torchs.Count - 1];
                for (int i = 1; i < this.torchs.Count; i++)
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
            }

            this.terrain.AttachFullPickingFullPathFinding(new ModelBase[] { this.helicopter, this.rocks }, false);
            this.terrain.AttachCoarsePathFinding(new ModelBase[] { this.torchs }, false);
            this.terrain.UpdateInternals();

            //Player soldier
            {
                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(0, 0, out position, out triangle, out distance))
                {
                    this.soldier.Manipulator.SetPosition(position, true);
                }

                AnimationPath p = new AnimationPath();
                p.AddLoop("stand");
                this.soldier.AnimationController.AddPath(p);
                this.soldier.AnimationController.Start();
            }

            //Instanced soldiers
            {
                Vector3[] iPos = new Vector3[]
                {
                    new Vector3(4, -2, MathUtil.PiOverFour),
                    new Vector3(5, -5, MathUtil.PiOverTwo),
                    new Vector3(-4, -2, -MathUtil.PiOverFour),
                    new Vector3(-5, -5, -MathUtil.PiOverTwo),
                };

                for (int i = 0; i < 4; i++)
                {
                    Vector3 position;
                    Triangle triangle;
                    float distance;
                    if (this.terrain.FindTopGroundPosition(iPos[i].X, iPos[i].Y, out position, out triangle, out distance))
                    {
                        this.troops.Instances[i].Manipulator.SetPosition(position, true);
                        this.troops.Instances[i].Manipulator.SetRotation(iPos[i].Z, 0, 0, true);
                        this.troops.Instances[i].TextureIndex = 1;

                        AnimationPath p = new AnimationPath();
                        p.AddLoop("idle1");
                        this.troops.Instances[i].AnimationController.AddPath(p);
                        this.troops.Instances[i].AnimationController.Start(rnd.NextFloat(0f, 8f));
                    }
                }
            }

            #endregion

            this.Camera.Position = new Vector3(-10, 10, -20);
            this.Camera.Interest = new Vector3(0, 4, 0);

            if (this.playerFlying)
            {
                this.Fly();
            }
            else
            {
                this.Walk();
            }

            #region Debug

            var bboxes = this.terrain.GetBoundingBoxes(5);
            var listBoxes = Line3.CreateWiredBox(bboxes);

            this.bboxesDrawer = this.AddLineListDrawer(listBoxes, Color.Red);
            this.bboxesDrawer.Visible = false;
            this.bboxesDrawer.CastShadow = false;
            this.bboxesDrawer.EnableAlphaBlending = true;

            #endregion
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            Ray cursorRay = this.GetPickingRay();

            #region Walk / Fly

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.playerFlying = !this.playerFlying;

                if (this.playerFlying)
                {
                    this.Fly();
                }
                else
                {
                    this.Walk();
                }
            }

            #endregion

            #region Camera

            if (this.playerFlying)
            {
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
                    this.Camera.MoveLeft(gameTime, false);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.Camera.MoveRight(gameTime, false);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.Camera.MoveForward(gameTime, false);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.Camera.MoveBackward(gameTime, false);
                }
            }
            else
            {
#if DEBUG
                if (this.Game.Input.RightMouseButtonPressed)
#endif
                {
                    this.soldier.Manipulator.Rotate(
                        this.Game.Input.MouseXDelta * 0.001f,
                        0, 0);
                }

                if (this.Game.Input.KeyPressed(Keys.A))
                {
                    this.soldier.Manipulator.MoveLeft(gameTime, 4);
                }

                if (this.Game.Input.KeyPressed(Keys.D))
                {
                    this.soldier.Manipulator.MoveRight(gameTime, 4);
                }

                if (this.Game.Input.KeyPressed(Keys.W))
                {
                    this.soldier.Manipulator.MoveForward(gameTime, 4);
                }

                if (this.Game.Input.KeyPressed(Keys.S))
                {
                    this.soldier.Manipulator.MoveBackward(gameTime, 4);
                }

                Vector3 position;
                Triangle triangle;
                float distance;
                if (this.terrain.FindTopGroundPosition(this.soldier.Manipulator.Position.X, this.soldier.Manipulator.Position.Z, out position, out triangle, out distance))
                {
                    this.soldier.Manipulator.SetPosition(position);
                };
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

            #region Lights

            {
                float d = 1f;
                float v = 5f;

                var x = d * (float)Math.Cos(v * this.Game.GameTime.TotalSeconds);
                var z = d * (float)Math.Sin(v * this.Game.GameTime.TotalSeconds);

                this.spotLight1.Direction = Vector3.Normalize(new Vector3(x, -1, z));
                this.spotLight2.Direction = Vector3.Normalize(new Vector3(-x, -1, -z));
            }

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.showSoldierDEBUG = !this.showSoldierDEBUG;

                if (this.soldierTris != null) this.soldierTris.Visible = this.showSoldierDEBUG;
                if (this.soldierLines != null) this.soldierLines.Visible = this.showSoldierDEBUG;
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

            if (this.showSoldierDEBUG)
            {
                Color color = new Color(Color.Red.ToColor3(), 0.6f);

                Triangle[] tris = this.soldier.GetTriangles(true);
                if (this.soldierTris == null)
                {
                    this.soldierTris = this.AddTriangleListDrawer(tris, color);
                    this.soldierTris.EnableDepthStencil = false;
                }
                else
                {
                    this.soldierTris.SetTriangles(color, tris);
                }

                BoundingBox[] bboxes = new BoundingBox[]
                {
                    this.soldier.GetBoundingBox(true),
                    this.troops.Instances[0].GetBoundingBox(true),
                    this.troops.Instances[1].GetBoundingBox(true),
                    this.troops.Instances[2].GetBoundingBox(true),
                    this.troops.Instances[3].GetBoundingBox(true),
                };
                if (this.soldierLines == null)
                {
                    this.soldierLines = this.AddLineListDrawer(Line3.CreateWiredBox(bboxes), color);
                    this.soldierLines.EnableDepthStencil = false;
                }
                else
                {
                    this.soldierLines.SetLines(color, Line3.CreateWiredBox(bboxes));
                }
            }

            #endregion

            base.Update(gameTime);

            this.help.Text = string.Format(
                "{0}. Wind {1} {2} - Next {3}",
                this.Renderer,
                this.windDirection, this.windStrength, this.windNextStrength);

            this.help2.Text = this.Game.RuntimeText;
        }

        private void Fly()
        {
            this.Camera.Following = null;
        }
        private void Walk()
        {
            var offset = (this.playerHeight * 1.2f) + (Vector3.ForwardLH * 10f) + (Vector3.Left * 3f);
            var view = (Vector3.BackwardLH * 4f) + Vector3.Down;
            this.Camera.Following = new CameraFollower(this.soldier.Manipulator, offset, view);
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
    }
}
