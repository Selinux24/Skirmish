using Engine;
using Engine.Content;
using Engine.PathFinding;
using Engine.PathFinding.RecastNavigation;
using SharpDX;
using System;

namespace Collada
{
    /// <summary>
    /// Navigation mesh test scene
    /// </summary>
    class NavmeshTest : Scene
    {
        private const int layerHUD = 99;

        private Player2 agent = null;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> help = null;
        private SceneObject<TextDrawer> debug = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<TriangleListDrawer> dungeonDrawer = null;
        private SceneObject<LineListDrawer> dungeonTriDrawer = null;
        private SceneObject<TriangleListDrawer> graphDrawer = null;
        private Color4 color = new Color4(0.5f, 0.5f, 0.5f, 1f);
        private Color4 colorTri = new Color4(0.1f, 0.1f, 0.1f, 0.15f);
        private Color4 colorNodeTri = new Color4(1.0f, 0.0f, 0.0f, 0.85f);
        private Color4 colorNodeBox = new Color4(0.0f, 1.0f, 1.0f, 0.50f);

        private SceneObject<Model> inputGeometry = null;
        private int inputGeometryIndex = -1;
        private BuildSettings nmsettings = BuildSettings.Default;

        public NavmeshTest(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.FarPlaneDistance *= 2;

            this.InitializeText();
            this.InitializeNavmesh();
            this.InitializeDebug();
        }
        private void InitializeText()
        {
            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.title.Instance.Text = "Navigation Mesh Test Scene";
            this.title.Instance.Position = Vector2.Zero;

            this.help = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.help.Instance.Text = "Camera: WASD+Mouse. B: Change Build Mode. P: Change Partition Type. (SHIFT reverse). F5: Save. F6: Load.";
            this.help.Instance.Position = new Vector2(0, 24);

            this.debug = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.debug.Instance.Text = null;
            this.debug.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.debug.Instance.Top + this.debug.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);
        }
        private void InitializeNavmesh()
        {
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
            nmsettings.BuildMode = BuildModesEnum.Tiled;
            nmsettings.PartitionType = SamplePartitionTypeEnum.Layers;

            this.PathFinderDescription = new PathFinderDescription()
            {
                Settings = nmsettings,
            };

            this.inputGeometry = this.AddComponent<Model>(
                new ModelDescription()
                {
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/NavmeshTest",
                        ModelContentFilename = "dungeon.xml",
                    }
                }, 
                SceneObjectUsageEnum.Ground);
        }
        private void InitializeDebug()
        {
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

            //this.dungeonDrawer.Instance.SetTriangles(color, triangles);
            //this.dungeonTriDrawer.Instance.SetTriangles(colorTri, triangles);

            var graphDrawerDesc = new TriangleListDrawerDescription()
            {
                Name = "DEBUG++ Graph",
                AlphaEnabled = true,
                Count = 20000,
            };
            this.graphDrawer = this.AddComponent<TriangleListDrawer>(graphDrawerDesc);
        }

        public override void Initialized()
        {
            base.Initialized();

            this.UpdateGraphNodes(this.agent);

            var bbox = inputGeometry.Instance.GetBoundingBox();
            var center = bbox.GetCenter();
            float maxD = Math.Max(Math.Max(bbox.GetX(), bbox.GetY()), bbox.GetZ());

            //this.Camera.Interest = center;
            //this.Camera.Position = center + new Vector3(1, 0.8f, -1) * maxD * 0.8f;

            var pos = new Vector3(19.3437824f, 19.3090019f, -80.3498535f);
            if (this.FindNearestGroundPosition(pos, out Vector3 p, out Triangle t, out float d))
            {
                p += agent.Height;

                this.Camera.Position = p;
                this.Camera.Interest = p + Vector3.ForwardLH;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            this.UpdateCamera(gameTime);

            this.UpdateGraph(gameTime);
        }
        private void UpdateCamera(GameTime gameTime)
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            var prevPos = this.Camera.Position;

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

            if (this.Walk(this.agent, prevPos, this.Camera.Position, out Vector3 walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }
        }
        private void UpdateGraph(GameTime gameTime)
        {
            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

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

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.navigationGraph.Save(@"test.grf");
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                this.navigationGraph.Load(@"test.grf");
                this.UpdateGraphNodes(this.agent);
            }

            bool updateGraph = false;

            if (this.Game.Input.KeyJustReleased(Keys.B))
            {
                if (!shift)
                {
                    nmsettings.BuildMode = (BuildModesEnum)Helper.Next((int)nmsettings.BuildMode, 3);
                }
                else
                {
                    nmsettings.BuildMode = (BuildModesEnum)Helper.Prev((int)nmsettings.BuildMode, 3);
                }
                updateGraph = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                if (!shift)
                {
                    nmsettings.PartitionType = (SamplePartitionTypeEnum)Helper.Next((int)nmsettings.PartitionType, 3);
                }
                else
                {
                    nmsettings.PartitionType = (SamplePartitionTypeEnum)Helper.Prev((int)nmsettings.PartitionType, 3);
                }
                updateGraph = true;
            }

            if (updateGraph)
            {
                this.UpdateNavigationGraph();
                this.UpdateGraphNodes(this.agent);
            }

            this.debug.Instance.Text = string.Format("Build Mode: {0}; Partition Type: {1};", nmsettings.BuildMode, nmsettings.PartitionType);
        }
        private void UpdateGraphNodes(AgentType agent)
        {
            var nodes = this.GetNodes(agent);
            if (nodes != null && nodes.Length > 0)
            {
                this.graphDrawer.Instance.Clear();

                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = (GraphNode)nodes[i];
                    var color = node.Color;
                    var tris = node.Triangles;

                    this.graphDrawer.Instance.AddTriangles(color, tris);
                }
            }
        }
        private void UpdateInputGeometryNodes(int index)
        {
            //this.dungeonTriDrawer.Instance.Clear(colorNodeBox);
            //this.dungeonTriDrawer.Instance.Clear(colorNodeTri);

            //Random rnd = new Random(1);

            //var chunkyMesh = this.inputGeometry.GetChunkyMesh();

            //if (index >= 0)
            //{
            //    if (index == 0)
            //    {
            //        for (int i = 0; i < chunkyMesh.nnodes; i++)
            //        {
            //            var node = chunkyMesh.nodes[i];
            //            if (node.i >= 0)
            //            {
            //                var color = rnd.NextColor().ToColor4();
            //                color.Alpha = colorNodeTri.Alpha;
            //                this.dungeonTriDrawer.Instance.Clear(color);
            //            }
            //        }
            //    }

            //    var curNode = chunkyMesh.nodes[index];

            //    var bbox = new BoundingBox(
            //        new Vector3(curNode.bmin.X, inputGeometry.BoundingBox.Minimum.Y, curNode.bmin.Y),
            //        new Vector3(curNode.bmax.X, inputGeometry.BoundingBox.Maximum.Y, curNode.bmax.Y));

            //    this.dungeonTriDrawer.Instance.SetLines(colorNodeBox, Line3D.CreateWiredBox(bbox));

            //    if (curNode.i >= 0)
            //    {
            //        var triangles = chunkyMesh.GetTriangles(curNode);

            //        this.dungeonTriDrawer.Instance.SetTriangles(colorNodeTri, triangles);
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < chunkyMesh.nnodes; i++)
            //    {
            //        var node = chunkyMesh.nodes[i];

            //        var bbox = new BoundingBox(
            //            new Vector3(node.bmin.X, inputGeometry.BoundingBox.Minimum.Y, node.bmin.Y),
            //            new Vector3(node.bmax.X, inputGeometry.BoundingBox.Maximum.Y, node.bmax.Y));

            //        this.dungeonTriDrawer.Instance.AddLines(colorNodeBox, Line3D.CreateWiredBox(bbox));

            //        if (node.i >= 0)
            //        {
            //            var triangles = chunkyMesh.GetTriangles(node);

            //            var color = rnd.NextColor().ToColor4();
            //            color.Alpha = colorNodeTri.Alpha;
            //            this.dungeonTriDrawer.Instance.SetTriangles(color, triangles);
            //        }
            //    }
            //}
        }
    }
}
