using Engine;
using Engine.Common;
using Engine.Content;
using SharpDX;
using PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology;

namespace AnimationTest
{
    public class TestScene3D : Scene
    {
        private TextDrawer title = null;
        private TextDrawer runtime = null;

        private Model floor = null;

        private Model soldier = null;
        private TriangleListDrawer soldierTris = null;
        private LineListDrawer soldierLines = null;
        private bool showSoldierDEBUG = false;

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

            this.title.Text = "Animation test";
            this.runtime.Text = "";

            this.title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, this.title.Top + this.title.Height + 3);

            #endregion

            #region Models

            #region Soldier

            this.soldier = this.AddModel(
                new ModelContentDescription()
                {
                    ContentPath = @"Resources/Soldier",
                    ModelFileName = "soldier_anim2.dae",
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
                this.soldier.AnimationController.AddClip(0, true, float.MaxValue);

                float playerHeight = this.soldier.GetBoundingBox().Maximum.Y - this.soldier.GetBoundingBox().Minimum.Y;

                this.Camera.Goto(0, playerHeight, -12f);
                this.Camera.LookTo(0, playerHeight * 0.5f, 0);
            }

            #endregion
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

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.showSoldierDEBUG = !this.showSoldierDEBUG;

                if (this.soldierTris != null) this.soldierTris.Visible = this.showSoldierDEBUG;
                if (this.soldierLines != null) this.soldierLines.Visible = this.showSoldierDEBUG;
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

            this.World *= Matrix.RotationY(MathUtil.PiOverFour * 0.1f * gameTime.ElapsedSeconds);

            base.Update(gameTime);

            this.runtime.Text = this.Game.RuntimeText;
        }
    }
}
