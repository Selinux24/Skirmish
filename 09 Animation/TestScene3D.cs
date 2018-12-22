using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Animation
{
    public class TestScene3D : Scene
    {
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> runtime = null;
        private SceneObject<TextDrawer> animText = null;

        private SceneObject<Model> soldier = null;
        private readonly Dictionary<string, AnimationPlan> soldierPaths = new Dictionary<string, AnimationPlan>();
        private SceneObject<PrimitiveListDrawer<Triangle>> soldierTris = null;
        private SceneObject<PrimitiveListDrawer<Line3D>> soldierLines = null;
        private readonly Color soldierTrisColor = new Color(Color.Yellow.ToColor3(), 0.6f);
        private readonly Color soldierLinesColor = new Color(Color.Red.ToColor3(), 1f);
        private bool showSoldierDEBUG = false;

        private SceneObject<Model> rat = null;
        private readonly Dictionary<string, AnimationPlan> ratPaths = new Dictionary<string, AnimationPlan>();

        private SceneObject<Model> ladder = null;
        private readonly Dictionary<string, AnimationPlan> ladderPaths = new Dictionary<string, AnimationPlan>();

        private readonly Random rnd = new Random();

        public TestScene3D(Game game)
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.InitializeSoldier();
            this.InitializeRat();
            this.InitializeLadder();
            this.InitializeFloor();

            this.InitializeUI();
            this.InitializeEnvironment();

            this.InitializeDebug();
        }
        private void InitializeUI()
        {
            var title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            this.runtime = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.animText = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 15, Color.Orange), SceneObjectUsages.UI, layerHUD);

            title.Instance.Text = "Animation test";
            this.runtime.Instance.Text = "";
            this.animText.Instance.Text = "";

            title.Instance.Position = Vector2.Zero;
            this.runtime.Instance.Position = new Vector2(5, title.Instance.Top + title.Instance.Height + 3);
            this.animText.Instance.Position = new Vector2(5, this.runtime.Instance.Top + this.runtime.Instance.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.animText.Instance.Top + this.animText.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.AddComponent<Sprite>(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private void InitializeFloor()
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

            this.AddComponent<Model>(desc);
        }
        private void InitializeRat()
        {
            this.rat = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Rat",
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Rat",
                        ModelContentFilename = "rat.xml",
                    }
                });

            this.rat.Transform.SetPosition(2, 0, 0, true);

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");

            this.ratPaths.Add("walk", new AnimationPlan(p0));

            this.rat.Instance.AnimationController.AddPath(this.ratPaths["walk"]);
        }
        private void InitializeLadder()
        {
            this.ladder = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Ladder",
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Ladder",
                        ModelContentFilename = "Dn_Anim_Ladder.xml",
                    }
                });

            this.ladder.Transform.SetPosition(-2, 1, 0, true);

            AnimationPath def = new AnimationPath();
            def.Add("default");
            AnimationPath pull = new AnimationPath();
            pull.Add("pull");
            AnimationPath push = new AnimationPath();
            push.Add("push");

            this.ladderPaths.Add("default", new AnimationPlan(def));
            this.ladderPaths.Add("pull", new AnimationPlan(pull));
            this.ladderPaths.Add("push", new AnimationPlan(push));

            this.ladder.Instance.AnimationController.AddPath(this.ladderPaths["pull"]);
        }
        private void InitializeSoldier()
        {
            this.soldier = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Soldier",
                    TextureIndex = 1,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            this.soldier.Transform.SetPosition(0, 0, 0, true);

            this.soldier.Instance.AnimationController.PathEnding += AnimationController_PathEnding;

            AnimationPath p0 = new AnimationPath();
            p0.Add("idle1");
            p0.AddRepeat("idle2", 5);
            p0.Add("idle1");
            p0.Add("stand");
            p0.Add("idle1");
            p0.AddRepeat("walk", 5);
            p0.AddRepeat("run", 10);
            p0.AddRepeat("walk", 1);
            p0.AddRepeat("idle2", 5);
            p0.Add("idle1");

            AnimationPath p1 = new AnimationPath();
            p1.Add("idle1");

            AnimationPath p2 = new AnimationPath();
            p2.AddRepeat("idle2", 2);

            AnimationPath p3 = new AnimationPath();
            p3.AddRepeat("stand", 5);

            this.soldierPaths.Add("complex", new AnimationPlan(p0));
            this.soldierPaths.Add("idle1", new AnimationPlan(p1));
            this.soldierPaths.Add("idle2", new AnimationPlan(p2));
            this.soldierPaths.Add("stand", new AnimationPlan(p3));

            this.soldier.Instance.AnimationController.AddPath(this.soldierPaths["complex"]);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            this.Lights.KeyLight.CastShadow = true;
            this.Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-0.1f, -1, 1));
            this.Lights.KeyLight.Enabled = true;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = false;
            this.Lights.HemisphericLigth = new SceneLightHemispheric("Ambient", Color.Gray, Color.White, true);

            var bbox = this.soldier.Instance.GetBoundingBox();
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(0, playerHeight, -12f);
            this.Camera.LookTo(0, playerHeight * 0.6f, 0);
        }
        private void InitializeDebug()
        {
            this.soldierTris = this.AddComponent<PrimitiveListDrawer<Triangle>>(new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, Color = soldierTrisColor });
            this.soldierLines = this.AddComponent<PrimitiveListDrawer<Line3D>>(new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, Color = soldierLinesColor });
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            #region Camera

            this.UpdateCamera(gameTime, shift);

            #endregion

            #region Animation control

            this.UpdateAnimation();

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
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

                this.soldierTris.Instance.SetPrimitives(soldierTrisColor, tris);
                this.soldierLines.Instance.SetPrimitives(soldierLinesColor, Line3D.CreateWiredBox(bbox));

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
        private void UpdateCamera(GameTime gameTime, bool shift)
        {
#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
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

                this.rat.Instance.AnimationController.TimeDelta -= 0.1f;
                this.rat.Instance.AnimationController.TimeDelta = Math.Max(0, this.rat.Instance.AnimationController.TimeDelta);

                this.ladder.Instance.AnimationController.TimeDelta -= 0.1f;
                this.ladder.Instance.AnimationController.TimeDelta = Math.Max(0, this.ladder.Instance.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.soldier.Instance.AnimationController.TimeDelta += 0.1f;
                this.soldier.Instance.AnimationController.TimeDelta = Math.Min(5, this.soldier.Instance.AnimationController.TimeDelta);

                this.rat.Instance.AnimationController.TimeDelta += 0.1f;
                this.rat.Instance.AnimationController.TimeDelta = Math.Min(5, this.rat.Instance.AnimationController.TimeDelta);

                this.ladder.Instance.AnimationController.TimeDelta += 0.1f;
                this.ladder.Instance.AnimationController.TimeDelta = Math.Min(5, this.ladder.Instance.AnimationController.TimeDelta);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                if (this.Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    this.soldier.Instance.AnimationController.Start(0);
                    this.rat.Instance.AnimationController.Start(0);
                    this.ladder.Instance.AnimationController.Start(0);
                }
                else
                {
                    this.soldier.Instance.AnimationController.Resume();
                    this.rat.Instance.AnimationController.Resume();
                    this.ladder.Instance.AnimationController.Resume();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                this.soldier.Instance.AnimationController.Pause();
                this.rat.Instance.AnimationController.Pause();
                this.ladder.Instance.AnimationController.Pause();
            }

            if (this.Game.Input.KeyJustReleased(Keys.Space))
            {
                this.soldier.Instance.AnimationController.ContinuePath(this.soldierPaths["stand"]);
                this.rat.Instance.AnimationController.ContinuePath(this.ratPaths["walk"]);
                this.ladder.Instance.AnimationController.ContinuePath(this.ladderPaths["pull"]);
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
