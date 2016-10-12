using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
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
        private TriangleListDrawer soldierTris = null;
        private LineListDrawer soldierLines = null;
        private bool showSoldierDEBUG = false;

        private int aIdle1 = 0;
        private int aWalk = 25948 / 52;
        private int aIdle1Walk = 30056 / 52;
        private int aWalkIdle1 = 34164 / 52;
        private int aEnd = 60112 / 52;

        private int aFrom = 0;
        private int aTo = (25948 / 52) - 1;

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Lights.DirectionalLights[0].Enabled = true;
            this.Lights.DirectionalLights[1].Enabled = true;
            this.Lights.DirectionalLights[2].Enabled = true;

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

            AnimationDescription ani = new AnimationDescription();
            ani.AddClip("idle1", 0, 8);
            ani.AddClip("walk", 8, 17);
            ani.AddTransition("idle1", "walk", 0f, 0f);
            ani.AddTransition("walk", "idle1", 0f, 0f);

            this.soldier = this.AddModel(
                new ModelContentDescription()
                {
                    ContentPath = @"Resources/Soldier",
                    ModelFileName = "soldier_anim2.dae",
                    Animation = ani,
                },
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

                AnimationPath p = new AnimationPath();
                p.Add("idle1");
                p.AddRepeat("walk", 5);
                p.AddLoop("idle1");
                this.soldier.AnimationController.AddClip(p);

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
                this.soldier.AnimationController.TimeDelta = Math.Min(2, this.soldier.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                if (this.Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    this.soldier.AnimationController.Start(8.333333f);
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

            if (this.Game.Input.KeyPressed(Keys.D1))
            {
                SkinningData.DEBUGINDEX = this.aFrom = this.aIdle1;
                this.aTo = this.aWalk - 1;
            }
            if (this.Game.Input.KeyPressed(Keys.D2))
            {
                SkinningData.DEBUGINDEX = this.aFrom = this.aWalk;
                this.aTo = this.aIdle1Walk - 1;
            }
            if (this.Game.Input.KeyPressed(Keys.D3))
            {
                SkinningData.DEBUGINDEX = this.aFrom = this.aIdle1Walk;
                this.aTo = this.aWalkIdle1 - 1;
            }
            if (this.Game.Input.KeyPressed(Keys.D4))
            {
                SkinningData.DEBUGINDEX = this.aFrom = this.aWalkIdle1;
                this.aTo = this.aEnd - 1;
            }

            if (this.Game.Input.KeyPressed(Keys.D5))
            {
                //First from walk
                SkinningData.DEBUGINDEX = this.aWalk;
            }
            if (this.Game.Input.KeyPressed(Keys.D6))
            {
                //Last from idle1walf
                SkinningData.DEBUGINDEX = this.aWalkIdle1 - 1;
            }

            if (this.Game.Input.KeyPressed(Keys.Add))
            {
                SkinningData.DEBUGINDEX++;
                if (SkinningData.DEBUGINDEX > aTo) SkinningData.DEBUGINDEX = aTo;
            }
            if (this.Game.Input.KeyPressed(Keys.Subtract))
            {
                SkinningData.DEBUGINDEX--;
                if (SkinningData.DEBUGINDEX < aFrom) SkinningData.DEBUGINDEX = aFrom;
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
                    this.soldierLines = this.AddLineListDrawer(Line3.CreateWiredBox(bbox), color);
                    this.soldierLines.EnableDepthStencil = false;
                }
                else
                {
                    this.soldierLines.SetLines(color, Line3.CreateWiredBox(bbox));
                }
            }

            #endregion

            base.Update(gameTime);

            this.runtime.Text = this.Game.RuntimeText;
            this.animText.Text = string.Format(
                "Index: {0}; Delta: {1} {2}",
                this.soldier.AnimationController.CurrentIndex,
                this.soldier.AnimationController.TimeDelta,
                SkinningData.DEBUGINDEX - this.aFrom);
        }
    }
}
