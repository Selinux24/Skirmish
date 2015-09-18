﻿using Engine;
using Engine.Common;
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

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;

        private Terrain terrain = null;
        private LineListDrawer bboxesDrawer = null;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;

            #region Texts

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.load = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);

            this.title.Text = "Heightmap Terrain test";
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
                Heightmap = new TerrainDescription.HeightmapDescription()
                {
                    HeightmapFileName = "heightmap0.bmp",
                    Texture = "dirt0.dds",
                    CellSize = 25,
                    MaximumHeight = 250,
                },
                Skydom = new TerrainDescription.SkydomDescription()
                {
                    Texture = "sunset.dds",
                },
                Quadtree = new TerrainDescription.QuadtreeDescription()
                {

                },
            });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();

            #endregion

            #region Debug

            BoundingBox[] bboxes = this.terrain.GetBoundingBoxes(5);
            Line[] listBoxes = GeometryUtil.CreateWiredBox(bboxes);

            this.bboxesDrawer = this.AddLineListDrawer(listBoxes, Color.Red);
            this.bboxesDrawer.Visible = false;
            this.bboxesDrawer.Opaque = false;
            this.bboxesDrawer.EnableAlphaBlending = true;

            #endregion

            this.load.Text = loadingText;

            #endregion

            this.Camera.Goto(0, 1, 0);
            this.Camera.LookTo(1, 1, 1);

            this.Lights.FogColor = Color.WhiteSmoke;
            this.Lights.FogStart = far * fogStart;
            this.Lights.FogRange = far * fogRange;

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = false;
            this.Lights.DirectionalLights[2].Enabled = false;
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

            #region Walk

            Vector3 v = this.Camera.Position;

            Vector3 p;
            Triangle tri;
            if (this.terrain.FindTopGroundPosition(v.X, v.Z, out p, out tri))
            {
                this.Camera.Goto(p + Vector3.UnitY);
            }

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.bboxesDrawer.Visible = !this.bboxesDrawer.Visible;
            }

            #endregion

            base.Update(gameTime);

            var frustum = this.Camera.Frustum;
            var nodes = this.terrain.Contained(ref frustum);
            var nodeCount = nodes != null ? nodes.Length : 0;

            this.help.Text = string.Format("Visible quad-tree nodes: {0}", nodeCount);
        }
    }
}