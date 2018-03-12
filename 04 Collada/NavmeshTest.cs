using Engine;
using Engine.PathFinding;
using Engine.PathFinding.NavMesh2;
using SharpDX;
using System;

namespace Collada
{
    /// <summary>
    /// Navigation mesh test scene
    /// </summary>
    class NavmeshTest : Scene
    {
        private Player2 agent = null;

        private SceneObject<TriangleListDrawer> dungeonDrawer = null;
        private SceneObject<LineListDrawer> dungeonTriDrawer = null;
        private SceneObject<TriangleListDrawer> graphDrawer = null;
        private Color4 color = new Color4(0.5f, 0.5f, 0.5f, 1f);
        private Color4 colorTri = new Color4(0.1f, 0.1f, 0.1f, 0.15f);
        private Color4 colorNodeTri = new Color4(1.0f, 0.0f, 0.0f, 0.85f);
        private Color4 colorNodeBox = new Color4(0.0f, 1.0f, 1.0f, 0.50f);

        private InputGeometry inputGeometry = null;
        private int inputGeometryIndex = -1;

        private BuildSettings nmsettings = BuildSettings.Default;

        public NavmeshTest(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.FarPlaneDistance *= 2;

            this.agent = new Player2()
            {
                Name = "Player",
                Height = 2f,
                MaxClimb = 0.8f,
                MaxSlope = 45f,
                Radius = 0.5f,
                Velocity = 4f,
                VelocitySlow = 1f,
            };

            nmsettings.Agents = new[] { this.agent };
            nmsettings.TileSize = 32;
            nmsettings.BuildTiles = false;

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = nmsettings,
            };

            var dungeonDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "Dungeon",
                AlphaEnabled = true,
                DepthEnabled = true,
                Count = 20000,
            };
            this.dungeonDrawer = this.AddComponent<TriangleListDrawer>(dungeonDrawerDesc);
            var dungeonTriDrawerDesc = new LineListDrawerDescription()
            {
                Name = "DEBUG++ Triangles",
                AlphaEnabled = true,
                DepthEnabled = false,
                Count = 100000,
            };
            this.dungeonTriDrawer = this.AddComponent<LineListDrawer>(dungeonTriDrawerDesc);

            var triangles = InputGeometry.DebugTris();

            this.dungeonDrawer.Instance.SetTriangles(color, triangles);
            this.dungeonTriDrawer.Instance.SetTriangles(colorTri, triangles);

            var graphDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 20000,
            };
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(graphDrawerDesc);

            this.inputGeometry = new InputGeometry(triangles);
        }
        protected override Triangle[] GetTrianglesForNavigationGraph()
        {
            return this.inputGeometry.GetChunkyMesh().triangles;
        }
        public override void Initialized()
        {
            base.Initialized();

            this.UpdateGraphNodes(this.agent);

            var bbox = inputGeometry.BoundingBox;
            var center = bbox.GetCenter();
            float maxD = Math.Max(Math.Max(bbox.GetX(), bbox.GetY()), bbox.GetZ());

            this.Camera.Interest = center;
            this.Camera.Position = center + new Vector3(1, 0.8f, -1) * maxD * 0.8f;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                if (shift)
                {
                    this.UpdateInputGeometryNodes(--inputGeometryIndex);
                }
                else
                {
                    this.UpdateInputGeometryNodes(++inputGeometryIndex);
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Add))
            {
                int ppType = (int)nmsettings.PartitionType;
                ppType++;
                nmsettings.PartitionType = (SamplePartitionTypeEnum)(ppType % 3);

                this.UpdateNavigationGraph();
                this.UpdateGraphNodes(this.agent);
            }
        }
        private void UpdateCamera(GameTime gameTime)
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, slow);
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
        }
        private void UpdateInputGeometryNodes(int index)
        {
            this.dungeonTriDrawer.Instance.Clear(colorNodeBox);
            this.dungeonTriDrawer.Instance.Clear(colorNodeTri);

            Random rnd = new Random(1);

            var chunkyMesh = this.inputGeometry.GetChunkyMesh();

            if (index >= 0)
            {
                if (index == 0)
                {
                    for (int i = 0; i < chunkyMesh.nnodes; i++)
                    {
                        var node = chunkyMesh.nodes[i];
                        if (node.i >= 0)
                        {
                            var color = rnd.NextColor().ToColor4();
                            color.Alpha = colorNodeTri.Alpha;
                            this.dungeonTriDrawer.Instance.Clear(color);
                        }
                    }
                }

                var curNode = chunkyMesh.nodes[index];

                var bbox = new BoundingBox(
                    new Vector3(curNode.bmin.X, inputGeometry.BoundingBox.Minimum.Y, curNode.bmin.Y),
                    new Vector3(curNode.bmax.X, inputGeometry.BoundingBox.Maximum.Y, curNode.bmax.Y));

                this.dungeonTriDrawer.Instance.SetLines(colorNodeBox, Line3D.CreateWiredBox(bbox));

                if (curNode.i >= 0)
                {
                    var triangles = chunkyMesh.GetTriangles(curNode);

                    this.dungeonTriDrawer.Instance.SetTriangles(colorNodeTri, triangles);
                }
            }
            else
            {
                for (int i = 0; i < chunkyMesh.nnodes; i++)
                {
                    var node = chunkyMesh.nodes[i];

                    var bbox = new BoundingBox(
                        new Vector3(node.bmin.X, inputGeometry.BoundingBox.Minimum.Y, node.bmin.Y),
                        new Vector3(node.bmax.X, inputGeometry.BoundingBox.Maximum.Y, node.bmax.Y));

                    this.dungeonTriDrawer.Instance.AddLines(colorNodeBox, Line3D.CreateWiredBox(bbox));

                    if (node.i >= 0)
                    {
                        var triangles = chunkyMesh.GetTriangles(node);

                        var color = rnd.NextColor().ToColor4();
                        color.Alpha = colorNodeTri.Alpha;
                        this.dungeonTriDrawer.Instance.SetTriangles(color, triangles);
                    }
                }
            }
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                Random clrRnd = new Random(24);
                Color[] regions = new Color[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    regions[i] = new Color(clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), clrRnd.NextFloat(0, 1), 0.75f);
                }

                this.graphDrawer.Instance.Clear();

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = (GraphNode)nodes[i];
                    var color = regions[node.RegionId];
                    var poly = node.Polygon;
                    var tris = poly.Triangulate();

                    this.graphDrawer.Instance.AddTriangles(color, tris);
                }
            }
        }
    }
}
