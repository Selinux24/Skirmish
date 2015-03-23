using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Common;
using Engine.Helpers;
using Engine.PathFinding;
using SharpDX;

namespace TerrainTest
{
    public class TestScene3D : Scene
    {
        private Random rnd = new Random();

        private bool follow = false;

        private bool useDebugTex = false;
        private SpriteTexture shadowMapDrawer = null;
        private SharpDX.Direct3D11.ShaderResourceView debugTex = null;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;

        private Model cursor3D = null;
        private Model tank = null;

        private Terrain terrain = null;
        private List<Line> oks = new List<Line>();
        private List<Line> errs = new List<Line>();
        private LineListDrawer terrainLineDrawer = null;
        private LineListDrawer terrainGridDrawer = null;
        private LineListDrawer terrainPointDrawer = null;

        private ModelInstanced obelisk = null;

        private Model helicopter = null;
        private float v = 0f;
        private BezierPath curve = null;
        private float curveTime = 0;
        private LineListDrawer helicopterLineDrawer = null;
        private Vector3 heightOffset = (Vector3.Up * 10f);

        private Color4 gridColor = new Color4(Color.LightSeaGreen.ToColor3(), 0.5f);
        private Color4 curvesColor = Color.Red;
        private Color4 pointsColor = Color.Blue;
        private Color4 segmentsColor = new Color4(Color.Cyan.ToColor3(), 0.8f);
        private Color4 hAxisColor = Color.YellowGreen;
        private Color4 wAxisColor = Color.White;
        private Color4 velocityColor = Color.Green;
        private LineListDrawer curveLineDrawer = null;

        public TestScene3D(Game game)
            : base(game)
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

            this.title.Text = "Terrain collision and trajectories test";
            this.load.Text = "";
            this.help.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, 24);
            this.help.Position = new Vector2(0, 48);

            #endregion

            #region Models

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Cursor

            sw.Restart();
            this.cursor3D = this.AddModel(new ModelDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "cursor.dae",
            });
            sw.Stop();
            loadingText += string.Format("cursor3D: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Terrain

            sw.Restart();
            this.terrain = this.AddTerrain(new TerrainDescription()
            {
                ModelFileName = "two_levels.dae",
                UseQuadtree = true,
                UsePathFinding = true,
                PathNodeSize = 2f,
                PathNodeInclination = MathUtil.DegreesToRadians(35),
                AddSkydom = true,
                SkydomTexture = "sunset.dds",
                DropShadow = true,
            });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();

            #endregion

            #region Helicopter

            sw.Restart();
            this.helicopter = this.AddModel(new ModelDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "helicopter.dae",
                DropShadow = true,
                TextureIndex = 1,
            });
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            Vector3 gPos;
            if (this.terrain.FindTopGroundPosition(20, 20, out gPos))
            {
                this.helicopter.Manipulator.SetPosition(gPos, true);
            }

            this.curve = new BezierPath();

            Vector3[] cPoints = new[]
            {
                this.helicopter.Manipulator.Position,
                this.helicopter.Manipulator.Position + (Vector3.Up * 5f) + (this.helicopter.Manipulator.Forward * 10f),
                this.GetRandomPoint(this.heightOffset),
                this.GetRandomPoint(this.heightOffset),
                this.GetRandomPoint(this.heightOffset),
                this.GetRandomPoint(this.heightOffset),
                this.GetRandomPoint(this.heightOffset),
                this.GetRandomPoint(this.heightOffset),
            };

            this.curve.SetControlPoints(cPoints, 1, 10, 0.33f);

            #endregion

            #region Tank

            sw.Restart();
            this.tank = this.AddModel(new ModelDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "tank.dae",
                DropShadow = true,
            });
            sw.Stop();
            loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);

            this.tank.Manipulator.SetScale(3);

            Vector3 tankPosition;
            if (this.terrain.FindTopGroundPosition(0, 0, out tankPosition))
            {
                this.tank.Manipulator.SetPosition(tankPosition, true);
            }

            #endregion

            #region Obelisk

            sw.Restart();
            this.obelisk = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = "Resources",
                ModelFileName = "obelisk.dae",
                DropShadow = true,
                Instances = 4,
            });
            sw.Stop();
            loadingText += string.Format("obelisk: {0} ", sw.Elapsed.TotalSeconds);

            for (int i = 0; i < 4; i++)
            {
                int ox = i == 0 || i == 2 ? 1 : -1;
                int oy = i == 0 || i == 1 ? 1 : -1;

                Vector3 obeliskPosition;
                if (this.terrain.FindTopGroundPosition(ox * 50, oy * 50, out obeliskPosition))
                {
                    this.obelisk.Instances[i].Manipulator.SetPosition(obeliskPosition, true);
                }
            }

            #endregion

            this.load.Text = loadingText;

            #endregion

            #region Shadow Map

            int width = 300;
            int height = 300;
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.shadowMapDrawer = this.AddSpriteTexture(new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
            });

            this.debugTex = this.Device.LoadTexture(@"Resources\uvtest.png");

            #endregion

            #region DEBUG Path finding Grid

            if (this.terrain.grid != null && this.terrain.grid.Nodes.Length > 0)
            {
                this.terrainGridDrawer = this.AddLineListDrawer(this.terrain.grid.Nodes.Length * 4);
                this.terrainGridDrawer.UseZBuffer = true;
                this.terrainGridDrawer.EnableAlphaBlending = true;
                this.terrainGridDrawer.Visible = false;

                Matrix m = Matrix.Translation(Vector3.Up * 0.5f);

                for (int i = 0; i < this.terrain.grid.Nodes.Length; i++)
                {
                    float c = (this.terrain.grid.Nodes[i].Cost / MathUtil.PiOverFour);

                    Color4 color = Color.Transparent;

                    if (c > 0.66f) { color = new Color4(Color.Red.ToColor3(), 0.5f); }
                    else if (c > 0.33f) { color = new Color4(Color.Yellow.ToColor3(), 0.5f); }
                    else { color = new Color4(Color.Green.ToColor3(), 0.5f); }

                    Vector3[] corners = this.terrain.grid.Nodes[i].GetCorners();

                    this.terrainGridDrawer.AddLines(color, Line.Transform(GeometryUtil.CreateWiredSquare(corners), m));
                }
            }

            #endregion

            #region DEBUG Ground position test

            BoundingBox bbox = this.terrain.GetBoundingBox();

            float sep = 2.1f;
            for (float x = bbox.Minimum.X + 1; x < bbox.Maximum.X - 1; x += sep)
            {
                for (float z = bbox.Minimum.Z + 1; z < bbox.Maximum.Z - 1; z += sep)
                {
                    Vector3 pos;
                    if (this.terrain.FindTopGroundPosition(x, z, out pos))
                    {
                        this.oks.Add(new Line(pos, pos + Vector3.Up));
                    }
                    else
                    {
                        this.errs.Add(new Line(x, 10, z, x, -10, z));
                    }
                }
            }

            this.terrainLineDrawer = this.AddLineListDrawer(oks.Count + errs.Count);
            this.terrainLineDrawer.UseZBuffer = true;
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

            this.terrainPointDrawer = this.AddLineListDrawer(1000);
            this.terrainPointDrawer.Visible = false;
            this.terrainPointDrawer.UseZBuffer = false;

            #endregion

            #region DEBUG Helicopter manipulator

            this.helicopterLineDrawer = this.AddLineListDrawer(1000);
            this.helicopterLineDrawer.Visible = false;

            #endregion

            #region DEBUG Trajectory

            this.curveLineDrawer = this.AddLineListDrawer(20000);
            this.curveLineDrawer.UseZBuffer = false;
            this.curveLineDrawer.Visible = false;
            this.curveLineDrawer.SetLines(this.wAxisColor, GeometryUtil.CreateAxis(Matrix.Identity, 20f));

            this.DEBUGComputePath();

            #endregion

            this.Camera.Goto(this.helicopter.Manipulator.Position + Vector3.One * 25f);
            this.Camera.LookTo(this.helicopter.Manipulator.Position);

            this.Lights.EnableShadows = true;
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

            Ray cursorRay = this.GetPickingRay();

            #region Cursor picking and positioning

            Vector3 position;
            Triangle triangle;
            bool picked = this.terrain.PickNearest(ref cursorRay, out position, out triangle);
            if (picked)
            {
                this.cursor3D.Manipulator.SetPosition(position);
            }

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.terrainLineDrawer.Visible = !this.terrainLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.terrainPointDrawer.Visible = this.terrainGridDrawer.Visible = !this.terrainGridDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.curveLineDrawer.Visible = !this.curveLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F4))
            {
                this.helicopterLineDrawer.Visible = !this.helicopterLineDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.Lights.EnableShadows = !this.Lights.EnableShadows;

                this.shadowMapDrawer.Visible = this.Lights.EnableShadows;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.shadowMapDrawer.Visible = !this.shadowMapDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.useDebugTex = !this.useDebugTex;
            }

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (this.terrainGridDrawer.Visible)
                {
                    this.terrainPointDrawer.ClearLines();

                    if (picked)
                    {
                        Vector3[] positions;
                        Triangle[] triangles;
                        if (this.terrain.FindAllGroundPosition(position.X, position.Z, out positions, out triangles))
                        {
                            this.terrainPointDrawer.SetLines(Color.Magenta, GeometryUtil.CreateCrossList(positions, 1f));
                            this.terrainPointDrawer.SetLines(Color.DarkCyan, GeometryUtil.CreateWiredTriangle(triangles));
                            if (positions.Length > 1)
                            {
                                this.terrainPointDrawer.SetLines(Color.Cyan, new Line(positions[0], positions[positions.Length - 1]));
                            }
                        }
                    }
                }
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

            #region Helicopter

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                this.curveTime = 0f;
                this.v = 0.01f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                this.curve.AddPoint(this.GetRandomPoint(this.heightOffset));

                this.DEBUGComputePath();
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.curve != null && this.v != 0f)
            {
                if (this.v < 10f) this.v += 0.01f;

                float time = gameTime.ElapsedSeconds * this.v;

                if (this.Game.Input.KeyPressed(Keys.LShiftKey))
                {
                    time *= 0.01f;
                }

                int segment;
                float segmentDistance;
                this.curve.FindCurve(this.curveTime, out segment, out segmentDistance);

                if (segment < this.curve.Count - 3)
                {
                    int segment2;
                    float segmentDistance2;
                    this.curve.FindCurve(this.curveTime + time, out segment2, out segmentDistance2);

                    float segmentDelta = segmentDistance2 - segmentDistance;

                    Vector3 p0 = this.curve.GetPosition(this.curveTime);
                    Vector3 p1 = this.curve.GetPosition(this.curveTime + time);

                    Vector3 cfw = this.helicopter.Manipulator.Forward;
                    Vector3 nfw = Vector3.Normalize(p1 - p0);
                    cfw.Y = 0f;
                    nfw.Y = 0f;

                    float pitch = Vector3.DistanceSquared(p0, p1) * 10f;
                    float roll = Helper.Angle(Vector3.Normalize(nfw), Vector3.Normalize(cfw), Vector3.Up) * 50f;

                    pitch = MathUtil.Clamp(pitch, -MathUtil.PiOverFour, MathUtil.PiOverFour);
                    roll = MathUtil.Clamp(roll, -MathUtil.PiOverFour, MathUtil.PiOverFour);

                    roll *= pitch / MathUtil.PiOverFour;

                    Quaternion r =
                        Helper.LookAt(p1, p0) *
                        Quaternion.RotationYawPitchRoll(0, -pitch, roll);

                    r = Quaternion.Slerp(this.helicopter.Manipulator.Rotation, r, 0.1f);

                    this.helicopter.Manipulator.SetPosition(p0);
                    this.helicopter.Manipulator.SetRotation(r);

                    this.curveTime += time;

                    this.curveLineDrawer.SetLines(this.velocityColor, new[] { new Line(p0, p1) });

                    this.help.Text = string.Format(
                        "Pitch {0:+00.00;-00.00}; Roll {1:+00.00;-00.00}; Delta {2:00.0000}; Segment {3} of {4}/{5:00.0000}/{6:00.0000}",
                        MathUtil.RadiansToDegrees(pitch),
                        MathUtil.RadiansToDegrees(roll),
                        pitch / MathUtil.PiOverFour,
                        segment + 1,
                        this.curve.Count,
                        segmentDistance,
                        segmentDelta);
                }
                else
                {
                    this.curve.AddPoint(this.GetRandomPoint(this.heightOffset));

                    this.DEBUGComputePath();
                }
            }

            Matrix m = Matrix.RotationQuaternion(this.helicopter.Manipulator.Rotation) * Matrix.Translation(this.helicopter.Manipulator.Position);
            BoundingSphere sph = this.helicopter.GetBoundingSphere();

            this.curveLineDrawer.SetLines(this.hAxisColor, GeometryUtil.CreateAxis(m, 5f));

            this.helicopterLineDrawer.SetLines(new Color4(Color.White.ToColor3(), 0.20f), GeometryUtil.CreateWiredSphere(sph, 50, 20));

            #endregion

            #region Camera

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
                this.Camera.LookTo(sph.Center);
                this.Camera.Goto(sph.Center + (this.helicopter.Manipulator.Backward * 15f) + (Vector3.UnitY * 5f), CameraTranslations.UseDelta);
            }

            #endregion
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            this.shadowMapDrawer.Texture = this.useDebugTex ? this.debugTex : this.DrawContext.ShadowMap;
        }

        private void DEBUGComputePath()
        {
            List<Vector3> path = new List<Vector3>();

            float pass = this.curve.Length / 500f;

            for (float i = 0; i <= this.curve.Length; i += pass)
            {
                Vector3 pos = this.curve.GetPosition(i);

                path.Add(pos);
            }

            this.curveLineDrawer.SetLines(this.curvesColor, GeometryUtil.CreatePath(path.ToArray()));
            this.curveLineDrawer.SetLines(this.pointsColor, GeometryUtil.CreateCrossList(this.curve.Points, 0.5f));
            this.curveLineDrawer.SetLines(this.segmentsColor, GeometryUtil.CreatePath(this.curve.Points));
        }

        private Vector3 GetRandomPoint(Vector3 offset)
        {
            BoundingBox bbox = this.terrain.GetBoundingBox();

            while (true)
            {
                Vector3 v = rnd.NextVector3(bbox.Minimum * 0.9f, bbox.Maximum * 0.9f);

                Vector3 p;
                if (terrain.FindTopGroundPosition(v.X, v.Z, out p))
                {
                    return p + offset;
                }
            }
        }
    }
}
