using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Helpers;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh;
using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace TerrainTest
{
    public class TestScene3D : Scene
    {
        private const int MaxPickingTest = 1000;
        private const int MaxGridDrawer = 10000;

        private bool follow = false;

        private bool useDebugTex = false;
        private SpriteTexture shadowMapDrawer = null;
        private ShaderResourceView debugTex = null;
        private int graphIndex = -1;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;

        private Model cursor3D = null;
        private Model tank = null;

        private Skydom skydom = null;
        private Scenery terrain = null;
        private List<Line3> oks = new List<Line3>();
        private List<Line3> errs = new List<Line3>();
        private LineListDrawer terrainLineDrawer = null;
        private LineListDrawer terrainPointDrawer = null;
        private TriangleListDrawer terrainGraphDrawer = null;

        private ModelInstanced obelisk = null;
        private ModelInstanced rocks = null;
        private ModelInstanced trees = null;

        private Model helicopter = null;
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

            this.title.Text = "Terrain collision and trajectories test";
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

            #region Cursor

            sw.Restart();
            this.cursor3D = this.AddModel(new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "cursor.dae",
                DeferredEnabled = false,
            });
            this.cursor3D.EnableDepthStencil = false;
            sw.Stop();
            loadingText += string.Format("cursor3D: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Skydom

            sw.Restart();
            this.skydom = this.AddSkydom(new SkydomDescription()
            {
                ContentPath = resources,
                Radius = this.Camera.FarPlaneDistance,
                Texture = "sunset.dds",
            });
            sw.Stop();
            loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopter

            sw.Restart();
            var heliDesc = new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "helicopter.dae",
                Opaque = true,
                TextureIndex = 2,
            };
            this.helicopter = this.AddModel(heliDesc, Matrix.RotationY(0));
            this.helicopter.SetManipulator(new HeliManipulator());
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            this.helicopter.Manipulator.SetScale(0.75f);

            #endregion

            #region Tank

            sw.Restart();
            this.tank = this.AddModel(new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "Leopard.dae",
                Opaque = true,
            });
            sw.Stop();
            loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);

            this.tank.Manipulator.SetScale(2);

            #endregion

            #region Obelisk

            sw.Restart();
            this.obelisk = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = resources,
                ModelFileName = "obelisk.dae",
                Opaque = true,
                Instances = 4,
            });
            sw.Stop();
            loadingText += string.Format("obelisk: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Roks

            sw.Restart();
            this.rocks = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = resources,
                ModelFileName = "rocks.dae",
                Opaque = true,
                Instances = 15,
            });
            sw.Stop();
            loadingText += string.Format("rocks: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Roks

            sw.Restart();
            this.trees = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = resources + "\\tree",
                ModelFileName = "tree1.dae",
                Opaque = true,
                Instances = 30,
            });
            sw.Stop();
            loadingText += string.Format("trees: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Terrain

            sw.Restart();

            var navSettings = NavigationMeshGenerationSettings.Default;
            var tankbbox = this.tank.GetBoundingBox();
            navSettings.AgentHeight = tankbbox.GetY();
            navSettings.AgentRadius = tankbbox.GetZ() * 0.5f;
            navSettings.MaxClimb = tankbbox.GetY() * 0.45f;

            var terrainDescription = new GroundDescription()
            {
                ContentPath = resources,
                Model = new GroundDescription.ModelDescription()
                {
                    ModelFileName = "two_levels.dae",
                },
                Quadtree = new GroundDescription.QuadtreeDescription()
                {
                    MaxTrianglesPerNode = 2048,
                },
                PathFinder = new GroundDescription.PathFinderDescription()
                {
                    Settings = navSettings,
                },
                Vegetation = new GroundDescription.VegetationDescription()
                {
                    VegetarionTextures = new[] { "tree0.dds", "tree1.dds", "tree2.dds", "tree3.dds", "tree4.png", "tree5.png" },
                    Saturation = 0.5f,
                    Opaque = true,
                    StartRadius = 0f,
                    EndRadius = 300f,
                    MinSize = Vector2.One * 2.50f,
                    MaxSize = Vector2.One * 3.50f,
                },
                Opaque = true,
                DelayGeneration = true,
            };
            this.terrain = this.AddTerrain(terrainDescription);
            sw.Stop();

            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            this.load.Text = loadingText;

            #region Positioning

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

            Random posRnd = new Random(1);

            for (int i = 0; i < this.rocks.Instances.Length; i++)
            {
                var pos = this.DEBUGGetRandomPoint(posRnd, Vector3.Zero);

                Vector3 rockPosition;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out rockPosition))
                {
                    this.rocks.Instances[i].Manipulator.SetPosition(rockPosition, true);
                    this.rocks.Instances[i].Manipulator.SetRotation(posRnd.NextFloat(0, 3), 0, 0, true);
                    this.rocks.Instances[i].Manipulator.SetScale(posRnd.NextFloat(0.5f, 2f), true);
                }
            }

            for (int i = 0; i < this.trees.Instances.Length; i++)
            {
                var pos = this.DEBUGGetRandomPoint(posRnd, Vector3.Zero);

                Vector3 treePosition;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out treePosition))
                {
                    this.trees.Instances[i].Manipulator.SetPosition(treePosition, true);
                    this.trees.Instances[i].Manipulator.SetRotation(posRnd.NextFloat(0, 3), 0, 0, true);
                    this.trees.Instances[i].Manipulator.SetScale(posRnd.NextFloat(0.5f, 1.5f), true);
                }
            }

            this.terrain.AttachObject(new GroundAttachedObject() { Model = this.obelisk, EvaluateForPicking = false }, false);
            this.terrain.AttachObject(new GroundAttachedObject() { Model = this.rocks, EvaluateForPicking = false }, false);
            this.terrain.AttachObject(new GroundAttachedObject() { Model = this.trees, EvaluateForPicking = false, UseVolumeForPicking = true, EvaluateForPathFinding = true, UseVolumeForPathFinding = true }, false);
            this.terrain.UpdateInternals();

            this.SceneVolume = this.terrain.GetBoundingSphere();

            Vector3 gPos;
            Triangle gTri;
            if (this.terrain.FindTopGroundPosition(20, 20, out gPos, out gTri))
            {
                this.helicopter.Manipulator.SetPosition(gPos, true);
                this.helicopter.Manipulator.SetNormal(gTri.Normal);
            }

            Vector3 tankPosition;
            Triangle tankTriangle;
            if (this.terrain.FindTopGroundPosition(0, 0, out tankPosition, out tankTriangle))
            {
                this.tank.Manipulator.SetPosition(tankPosition, true);
                this.tank.Manipulator.SetNormal(tankTriangle.Normal);
            }

            #endregion

            #endregion

            #region Shadow Map

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
            this.shadowMapDrawer = this.AddSpriteTexture(stDescription, 99);
            this.shadowMapDrawer.Visible = false;
            this.shadowMapDrawer.DeferredEnabled = false;

            this.debugTex = this.Device.LoadTexture(@"Resources\uvtest.png");

            #endregion

            #region DEBUG Path finding Graph

            this.terrainGraphDrawer = this.AddTriangleListDrawer(MaxGridDrawer);
            this.terrainGraphDrawer.EnableDepthStencil = false;
            this.terrainGraphDrawer.EnableAlphaBlending = true;
            this.terrainGraphDrawer.Visible = false;
            this.terrainGraphDrawer.DeferredEnabled = false;

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
                        this.oks.Add(new Line3(pos, pos + Vector3.Up));
                    }
                    else
                    {
                        this.errs.Add(new Line3(x, 10, z, x, -10, z));
                    }
                }
            }

            this.terrainLineDrawer = this.AddLineListDrawer(oks.Count + errs.Count);
            this.terrainLineDrawer.Visible = false;
            this.terrainLineDrawer.DeferredEnabled = false;
            this.terrainLineDrawer.EnableAlphaBlending = true;
            this.terrainLineDrawer.EnableDepthStencil = false;

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

            this.terrainPointDrawer = this.AddLineListDrawer(MaxPickingTest);
            this.terrainPointDrawer.Visible = false;
            this.terrainPointDrawer.DeferredEnabled = false;
            this.terrainPointDrawer.EnableAlphaBlending = true;
            this.terrainPointDrawer.EnableDepthStencil = false;

            #endregion

            #region DEBUG Helicopter manipulator

            this.helicopterLineDrawer = this.AddLineListDrawer(1000);
            this.helicopterLineDrawer.Visible = false;
            this.helicopterLineDrawer.DeferredEnabled = false;
            this.helicopterLineDrawer.EnableAlphaBlending = true;
            this.helicopterLineDrawer.EnableDepthStencil = false;

            #endregion

            #region DEBUG Trajectory

            this.curveLineDrawer = this.AddLineListDrawer(20000);
            this.curveLineDrawer.Visible = false;
            this.curveLineDrawer.DeferredEnabled = false;
            this.curveLineDrawer.EnableAlphaBlending = true;
            this.curveLineDrawer.EnableDepthStencil = false;
            this.curveLineDrawer.SetLines(this.wAxisColor, Line3.CreateAxis(Matrix.Identity, 20f));

            #endregion

            this.Camera.Goto(this.helicopter.Manipulator.Position + Vector3.One * 25f);
            this.Camera.LookTo(this.helicopter.Manipulator.Position);

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = false;
            this.Lights.DirectionalLights[2].Enabled = false;
            this.Lights.Add(new SceneLightPoint()
            {
                Name = "One point",
                Enabled = true,
                LightColor = Color.Blue,
                AmbientIntensity = 1,
                DiffuseIntensity = 1,
                Position = Vector3.Zero,
                Radius = 1f,
            });
            this.Lights.Add(new SceneLightPoint()
            {
                Name = "Another point",
                Enabled = true,
                LightColor = Color.Red,
                AmbientIntensity = 1,
                DiffuseIntensity = 1,
                Position = Vector3.Zero,
                Radius = 1f,
            });
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
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.useDebugTex = !this.useDebugTex;
            }

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

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (this.terrainGraphDrawer.Visible)
                {
                    this.terrainPointDrawer.Clear();

                    if (picked)
                    {
                        this.DEBUGPickingPosition(position);
                    }
                }
            }

            #endregion

            #region Tank

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (picked)
                {
                    var p = this.terrain.FindPath(this.tank.Manipulator.Position, position);
                    if (p != null)
                    {
                        this.tank.Manipulator.Follow(p.ReturnPath.ToArray(), 0.1f, this.terrain);

                        this.DEBUGDrawTankPath(this.tank.Manipulator.Position, p);
                    }
                }
            }

            #endregion

            #region Helicopter

            if (this.Game.Input.KeyJustReleased(Keys.Home))
            {
                Curve3D curve = this.DEBUGGenerateHelicopterPath();
                ((HeliManipulator)this.helicopter.Manipulator).Follow(curve, 10f, 0.001f);
                this.DEBUGDrawHelicopterPath(curve);
            }

            this.Lights.PointLights[0].Position = (this.helicopter.Manipulator.Position + this.helicopter.Manipulator.Up + this.helicopter.Manipulator.Left);
            this.Lights.PointLights[1].Position = (this.helicopter.Manipulator.Position + this.helicopter.Manipulator.Up + this.helicopter.Manipulator.Right);

            Matrix rot = Matrix.RotationQuaternion(this.helicopter.Manipulator.Rotation) * Matrix.Translation(this.helicopter.Manipulator.Position);
            this.curveLineDrawer.SetLines(this.hAxisColor, Line3.CreateAxis(rot, 5f));

            BoundingSphere sph = this.helicopter.GetBoundingSphere();
            this.helicopterLineDrawer.SetLines(new Color4(Color.White.ToColor3(), 0.55f), Line3.CreateWiredSphere(sph, 50, 20));

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

            this.shadowMapDrawer.Texture = this.useDebugTex ? this.debugTex : this.Renderer.GetResource(SceneRendererResultEnum.ShadowMap);
        }

        private void DEBUGPickingPosition(Vector3 position)
        {
            Vector3[] positions;
            Triangle[] triangles;
            if (this.terrain.FindAllGroundPosition(position.X, position.Z, out positions, out triangles))
            {
                this.terrainPointDrawer.SetLines(Color.Magenta, Line3.CreateCrossList(positions, 1f));
                this.terrainPointDrawer.SetLines(Color.DarkCyan, Line3.CreateWiredTriangle(triangles));
                if (positions.Length > 1)
                {
                    this.terrainPointDrawer.SetLines(Color.Cyan, new Line3(positions[0], positions[positions.Length - 1]));
                }
            }
        }
        private Curve3D DEBUGGenerateHelicopterPath()
        {
            Curve3D curve = new Curve3D();
            curve.PreLoop = CurveLoopType.Constant;
            curve.PostLoop = CurveLoopType.Constant;

            Vector3[] cPoints = new Vector3[10];

            Random rnd = new Random();

            if (this.helicopter.Manipulator.IsFollowingPath)
            {
                for (int i = 0; i < cPoints.Length; i++)
                {
                    cPoints[i] = this.DEBUGGetRandomPoint(rnd, this.heightOffset);
                }
            }
            else
            {
                cPoints[0] = this.helicopter.Manipulator.Position;
                cPoints[1] = this.helicopter.Manipulator.Position + (Vector3.Up * 5f) + (this.helicopter.Manipulator.Forward * 10f);

                for (int i = 2; i < cPoints.Length; i++)
                {
                    cPoints[i] = this.DEBUGGetRandomPoint(rnd, this.heightOffset);
                }
            }

            float t = 0;
            for (int i = 0; i < cPoints.Length; i++)
            {
                if (i > 0) t += Vector3.Distance(cPoints[i - 1], cPoints[i]);

                curve.AddPosition(t, cPoints[i]);
            }

            curve.SetTangents();
            return curve;
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

            this.curveLineDrawer.SetLines(this.curvesColor, Line3.CreatePath(path.ToArray()));
            this.curveLineDrawer.SetLines(this.pointsColor, Line3.CreateCrossList(curve.Points, 0.5f));
            this.curveLineDrawer.SetLines(this.segmentsColor, Line3.CreatePath(curve.Points));
        }
        private void DEBUGDrawTankPath(Vector3 from, PathFindingPath path)
        {
            int count = Math.Min(path.ReturnPath.Count, MaxPickingTest);

            Line3[] lines = new Line3[count + 1];

            for (int i = 0; i < count; i++)
            {
                Line3 line;
                if (i == 0)
                {
                    line = new Line3(from, path.ReturnPath[i]);
                }
                else
                {
                    line = new Line3(path.ReturnPath[i - 1], path.ReturnPath[i]);
                }

                lines[i] = line;
            }

            this.terrainPointDrawer.SetLines(Color.Red, lines);
        }
        private void DEBUGUpdateGraphDrawer()
        {
            var nodes = this.terrain.GetNodes();
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
        private Vector3 DEBUGGetRandomPoint(Random rnd, Vector3 offset)
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
