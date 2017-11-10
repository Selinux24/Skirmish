using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Animation
{
    public class TestScene3D : Scene
    {
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> runtime = null;
        private SceneObject<TextDrawer> animText = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<Model> floor = null;

        private SceneObject<Model> soldier = null;
        private Dictionary<string, AnimationPlan> soldierPaths = new Dictionary<string, AnimationPlan>();
        private SceneObject<TriangleListDrawer> soldierTris = null;
        private SceneObject<LineListDrawer> soldierLines = null;
        private Color soldierTrisColor = new Color(Color.Yellow.ToColor3(), 0.6f);
        private Color soldierLinesColor = new Color(Color.Red.ToColor3(), 1f);
        private bool showSoldierDEBUG = false;

        private Random rnd = new Random();

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;

            GameEnvironment.Background = Color.CornflowerBlue;

            this.Lights.KeyLight.CastShadow = true;

            #region Texts

            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.runtime = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);
            this.animText = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 15, Color.Orange), SceneObjectUsageEnum.UI, layerHUD);

            this.title.Instance.Text = "Animation test";
            this.runtime.Instance.Text = "";
            this.animText.Instance.Text = "";

            this.title.Instance.Position = Vector2.Zero;
            this.runtime.Instance.Position = new Vector2(5, this.title.Instance.Top + this.title.Instance.Height + 3);
            this.animText.Instance.Position = new Vector2(5, this.runtime.Instance.Top + this.runtime.Instance.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.animText.Instance.Top + this.animText.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);

            #endregion

            #region Models

            #region Soldier

            this.soldier = this.AddComponent<Model>(
                new ModelDescription()
                {
                    TextureIndex = 1,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            #endregion

            #region Floor

            {
                float l = 15f;
                float h = 0f;

                VertexData[] vertices = new VertexData[]
                {
                    new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture = new Vector2(0.0f, 1.0f) },
                    new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 0.0f) },
                    new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture = new Vector2(1.0f, 1.0f) },
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

                var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

                var desc = new ModelDescription()
                {
                    Static = true,
                    CastShadow = true,
                    DeferredEnabled = true,
                    DepthEnabled = true,
                    AlphaEnabled = false,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ModelContent = content,
                    }
                };

                this.floor = this.AddComponent<Model>(desc);
            }

            #endregion

            #region Debug

            this.soldierTris = this.AddComponent<TriangleListDrawer>(new TriangleListDrawerDescription() { Count = 5000, Color = soldierTrisColor });
            this.soldierLines = this.AddComponent<LineListDrawer>(new LineListDrawerDescription() { Count = 1000, Color = soldierLinesColor });

            #endregion

            {
                this.soldier.Transform.SetPosition(0, 0, 0, true);

                this.soldier.Instance.AnimationController.PathEnding += AnimationController_PathEnding;

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

                AnimationPath p3 = new AnimationPath();
                p3.AddLoop("stand");

                this.soldierPaths.Add("complex", new AnimationPlan(p0));
                this.soldierPaths.Add("idle1", new AnimationPlan(p1));
                this.soldierPaths.Add("idle2", new AnimationPlan(p2));
                this.soldierPaths.Add("stand", new AnimationPlan(p3));

                this.soldier.Instance.AnimationController.AddPath(this.soldierPaths["complex"]);

                var bbox = this.soldier.Instance.GetBoundingBox();

                float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;

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

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            #region Animation control

            this.UpdateAnimation();

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            if (this.Game.Input.KeyJustReleased(Keys.C))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
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
                Triangle[] tris = this.soldier.Instance.GetTriangles(true);
                BoundingBox bbox = this.soldier.Instance.GetBoundingBox(true);

                this.soldierTris.Instance.SetTriangles(soldierTrisColor, tris);
                this.soldierLines.Instance.SetLines(soldierLinesColor, Line3D.CreateWiredBox(bbox));

                this.soldierTris.Active = this.soldierTris.Visible = true;
                this.soldierLines.Active = this.soldierLines.Visible = true;
            }
            else
            {
                if (this.soldierTris != null)
                {
                    this.soldierTris.Active = this.soldierTris.Visible = false;
                }

                if (this.soldierLines != null)
                {
                    this.soldierLines.Active = this.soldierLines.Visible = false;
                }
            }

            #endregion

            base.Update(gameTime);

            this.runtime.Instance.Text = this.Game.RuntimeText;
            this.animText.Instance.Text = string.Format(
                "Paths: {0:00}; Delta: {1:0.0}; Index: {2}; Clip: {3}; Time: {4:0.00}; Item Time: {5:0.00}",
                this.soldier.Instance.AnimationController.PathCount,
                this.soldier.Instance.AnimationController.TimeDelta,
                this.soldier.Instance.AnimationController.CurrentIndex,
                this.soldier.Instance.AnimationController.CurrentPathItemClip,
                this.soldier.Instance.AnimationController.CurrentPathTime,
                this.soldier.Instance.AnimationController.CurrentPathItemTime);
        }
        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }
        private void UpdateAnimation()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.soldier.Instance.AnimationController.TimeDelta -= 0.1f;
                this.soldier.Instance.AnimationController.TimeDelta = Math.Max(0, this.soldier.Instance.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.soldier.Instance.AnimationController.TimeDelta += 0.1f;
                this.soldier.Instance.AnimationController.TimeDelta = Math.Min(5, this.soldier.Instance.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                if (this.Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    this.soldier.Instance.AnimationController.Start(0);
                }
                else
                {
                    this.soldier.Instance.AnimationController.Resume();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                this.soldier.Instance.AnimationController.Pause();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.soldier.Instance.AnimationController.ContinuePath(this.soldierPaths["stand"]);
            }
        }

        private void AnimationController_PathEnding(object sender, EventArgs e)
        {
            var keys = this.soldierPaths.Keys.ToArray();

            int index = Math.Min(this.rnd.Next(1, 3), keys.Length - 1);

            var key = keys[index];

            ((AnimationController)sender).SetPath(this.soldierPaths[key]);
            ((AnimationController)sender).Start(0);
        }
    }
}
