using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace AnimationTest
{
    public class TestScene3D : Scene
    {
        private TextDrawer title = null;
        private TextDrawer runtime = null;
        private TextDrawer animText = null;

        private Model floor = null;

        private Model soldier = null;
        private List<AnimationPath> soldierPaths = new List<AnimationPath>();
        private TriangleListDrawer soldierTris = null;
        private LineListDrawer soldierLines = null;
        private bool showSoldierDEBUG = false;

        private Random rnd = new Random();

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = false;
            this.Lights.DirectionalLights[2].Enabled = false;

            this.Camera.NearPlaneDistance = 1;
            this.Camera.FarPlaneDistance = 500;

            GameEnvironment.Background = Color.CornflowerBlue;

            #region Texts

            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White));
            this.runtime = this.AddText(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow));
            this.animText = this.AddText(TextDrawerDescription.Generate("Tahoma", 15, Color.Orange));

            this.title.Text = "Animation test";
            this.runtime.Text = "";
            this.animText.Text = "";

            this.title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, this.title.Top + this.title.Height + 3);
            this.animText.Position = new Vector2(5, this.runtime.Top + this.runtime.Height + 3);

            #endregion

            #region Models

            #region Soldier

            this.soldier = this.AddModel(
                @"Resources/Soldier",
                @"soldier_anim2.xml",
                new ModelDescription()
                {
                    TextureIndex = 1,
                });

            #endregion

            #region Floor

            {
                float l = 20f;
                float h = 0f;

                VertexData[] vertices = new VertexData[]
                {
                    new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 1.0f) },
                    new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 1.0f) },
                };

                uint[] indices = new uint[]
                {
                    0, 1, 2,
                    1, 3, 2,
                };

                MaterialContent mat = MaterialContent.Default;
                mat.DiffuseTexture = "resources/d_road_asphalt_stripes_diffuse.dds";
                mat.NormalMapTexture = "resources/d_road_asphalt_stripes_normal.dds";
                mat.SpecularTexture = "resources/d_road_asphalt_stripes_specular.dds";

                var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, mat);

                var desc = new ModelDescription()
                {
                    Static = true,
                    AlwaysVisible = false,
                    CastShadow = false,
                    DeferredEnabled = true,
                    EnableDepthStencil = true,
                    EnableAlphaBlending = false,
                };

                this.floor = this.AddModel(content, desc);
            }

            #endregion

            {
                this.soldier.Manipulator.SetPosition(0, 0, 0, true);

                this.soldier.AnimationController.PathEnding += AnimationController_PathEnding;

                AnimationPath p0 = new AnimationPath();
                p0.Add("idle1");
                p0.AddRepeat("stand", 5);
                p0.Add("idle2");
                p0.Add("stand");
                p0.AddRepeat("walk", 5);
                p0.AddRepeat("run", 5);
                p0.Add("stand");
                p0.Add("idle1");

                AnimationPath p1 = new AnimationPath();
                p1.AddRepeat("idle1", 2);

                AnimationPath p2 = new AnimationPath();
                p2.Add("idle2");

                this.soldierPaths.Add(p0);
                this.soldierPaths.Add(p1);
                this.soldierPaths.Add(p2);

                this.soldier.AnimationController.AddPath(p0);

                float playerHeight = this.soldier.GetBoundingBox().Maximum.Y - this.soldier.GetBoundingBox().Minimum.Y;

                this.Camera.Goto(0, playerHeight, -12f);
                this.Camera.LookTo(0, playerHeight * 0.6f, 0);
            }

            #endregion
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            #region World

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                //Rotates the scene
                this.World *= Matrix.RotationY(MathUtil.PiOverFour * gameTime.ElapsedSeconds);
            }
            if (this.Game.Input.KeyPressed(Keys.D))
            {
                //Rotates the scene
                this.World *= Matrix.RotationY(-MathUtil.PiOverFour * gameTime.ElapsedSeconds);
            }
            if (this.Game.Input.KeyPressed(Keys.W))
            {
                //Rotates the scene
                this.Camera.MoveForward(gameTime, false);
            }
            if (this.Game.Input.KeyPressed(Keys.S))
            {
                //Rotates the scene
                this.Camera.MoveBackward(gameTime, false);
            }

            #endregion

            #region Animation control

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.soldier.AnimationController.TimeDelta -= 0.1f;
                this.soldier.AnimationController.TimeDelta = Math.Max(0, this.soldier.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.soldier.AnimationController.TimeDelta += 0.1f;
                this.soldier.AnimationController.TimeDelta = Math.Min(5, this.soldier.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                if (this.Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    this.soldier.AnimationController.Start(0);
                }
                else
                {
                    this.soldier.AnimationController.Resume();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                this.soldier.AnimationController.Pause();
            }

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.showSoldierDEBUG = !this.showSoldierDEBUG;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.soldier.Visible = !this.soldier.Visible;
            }

            if (this.showSoldierDEBUG)
            {
                Color color = new Color(Color.Red.ToColor3(), 0.6f);
                Triangle[] tris = this.soldier.GetTriangles(true);
                BoundingBox bbox = this.soldier.GetBoundingBox(true);

                if (this.soldierTris == null)
                {
                    this.soldierTris = this.AddTriangleListDrawer(tris, color);
                    this.soldierTris.EnableDepthStencil = false;
                }
                else
                {
                    this.soldierTris.SetTriangles(color, tris);
                }

                if (this.soldierLines == null)
                {
                    this.soldierLines = this.AddLineListDrawer(Line3D.CreateWiredBox(bbox), color);
                    this.soldierLines.EnableDepthStencil = false;
                }
                else
                {
                    this.soldierLines.SetLines(color, Line3D.CreateWiredBox(bbox));
                }
            }

            #endregion

            base.Update(gameTime);

            this.runtime.Text = this.Game.RuntimeText;
            this.animText.Text = string.Format(
                "Delta: {0:0.0}; Index: {1}; Clip: {2}; Time: {3:0.00}; Item Time: {4:0.00}",
                this.soldier.AnimationController.TimeDelta,
                this.soldier.AnimationController.CurrentIndex,
                this.soldier.AnimationController.CurrentPathItemClip,
                this.soldier.AnimationController.CurrentPathTime,
                this.soldier.AnimationController.CurrentPathItemTime);
        }

        private void AnimationController_PathEnding(object sender, EventArgs e)
        {
            int index = Math.Min(this.rnd.Next(1, 3), this.soldierPaths.Count - 1);

            ((AnimationController)sender).SetPath(this.soldierPaths[index]);
            ((AnimationController)sender).Start(0);
        }
    }
}
