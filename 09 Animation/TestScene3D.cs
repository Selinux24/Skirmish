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

        private readonly Random rnd = new Random();

        private SceneObject<TextDrawer> runtime = null;
        private SceneObject<TextDrawer> animText = null;

        private SceneObject<PrimitiveListDrawer<Triangle>> itemTris = null;
        private SceneObject<PrimitiveListDrawer<Line3D>> itemLines = null;
        private readonly Color itemTrisColor = new Color(Color.Yellow.ToColor3(), 0.6f);
        private readonly Color itemLinesColor = new Color(Color.Red.ToColor3(), 1f);
        private bool showItemDEBUG = false;
        private bool showItem = true;
        private int itemIndex = 0;

        private readonly List<SceneObject<Model>> animObjects = new List<SceneObject<Model>>();

        private readonly Dictionary<string, AnimationPlan> soldierPaths = new Dictionary<string, AnimationPlan>();
        private readonly Dictionary<string, AnimationPlan> ratPaths = new Dictionary<string, AnimationPlan>();
        private readonly Dictionary<string, AnimationPlan> ladderPaths = new Dictionary<string, AnimationPlan>();

        public TestScene3D(Game game)
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.InitializeLadder();
            this.InitializeSoldier();
            this.InitializeRat();
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
        private void InitializeLadder()
        {
            var ladder = this.AddComponent<Model>(
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

            var ladder2 = this.AddComponent<Model>(
                new ModelDescription()
                {
                    Name = "Ladder",
                    TextureIndex = 0,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Ladder",
                        ModelContentFilename = "Dn_Anim_Ladder2.xml",
                    }
                });

            ladder.Transform.SetPosition(-3f, 1, 0, true);
            ladder2.Transform.SetPosition(-2, 1, 0, true);

            AnimationPath def = new AnimationPath();
            def.Add("default");
            AnimationPath pull = new AnimationPath();
            pull.Add("pull");
            AnimationPath push = new AnimationPath();
            push.Add("push");

            this.ladderPaths.Add("default", new AnimationPlan(def));
            this.ladderPaths.Add("pull", new AnimationPlan(pull));
            this.ladderPaths.Add("push", new AnimationPlan(push));

            ladder.Instance.AnimationController.AddPath(this.ladderPaths["pull"]);
            ladder2.Instance.AnimationController.AddPath(this.ladderPaths["push"]);

            this.animObjects.Add(ladder);
            this.animObjects.Add(ladder2);
        }
        private void InitializeSoldier()
        {
            var soldier = this.AddComponent<Model>(
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

            soldier.Transform.SetPosition(0, 0, 0, true);

            soldier.Instance.AnimationController.PathEnding += SoldierControllerPathEnding;

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

            soldier.Instance.AnimationController.AddPath(this.soldierPaths["complex"]);

            this.animObjects.Add(soldier);
        }
        private void InitializeRat()
        {
            var rat = this.AddComponent<Model>(
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

            rat.Transform.SetPosition(2, 0, 0, true);

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");

            this.ratPaths.Add("walk", new AnimationPlan(p0));

            rat.Instance.AnimationController.AddPath(this.ratPaths["walk"]);

            this.animObjects.Add(rat);
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

            BoundingBox bbox = new BoundingBox();
            animObjects.ForEach(item =>
            {
                bbox = BoundingBox.Merge(bbox, item.Instance.GetBoundingBox());
            });
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(0, playerHeight, -12f);
            this.Camera.LookTo(0, playerHeight * 0.6f, 0);
        }
        private void InitializeDebug()
        {
            this.itemTris = this.AddComponent<PrimitiveListDrawer<Triangle>>(new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, Color = itemTrisColor });
            this.itemLines = this.AddComponent<PrimitiveListDrawer<Line3D>>(new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, Color = itemLinesColor });
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            this.UpdateInputCamera(gameTime, shift);
            this.UpdateInputAnimation();
            this.UpdateInputDebug();

            base.Update(gameTime);

            this.UpdateDebugData();

            var itemController = animObjects[itemIndex].Instance.AnimationController;

            this.runtime.Instance.Text = this.Game.RuntimeText;
            this.animText.Instance.Text = string.Format(
                "Paths: {0:00}; Delta: {1:0.0}; Index: {2}; Clip: {3}; Time: {4:0.00}; Item Time: {5:0.00}",
                itemController.PathCount,
                itemController.TimeDelta,
                itemController.CurrentIndex,
                itemController.CurrentPathItemClip,
                itemController.CurrentPathTime,
                itemController.CurrentPathItemTime);
        }
        private void UpdateInputCamera(GameTime gameTime, bool shift)
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
        private void UpdateInputAnimation()
        {
            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                animObjects.ForEach(item =>
                {
                    item.Instance.AnimationController.TimeDelta -= 0.1f;
                    item.Instance.AnimationController.TimeDelta = Math.Max(0, item.Instance.AnimationController.TimeDelta);
                });
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                animObjects.ForEach(item =>
                {
                    item.Instance.AnimationController.TimeDelta += 0.1f;
                    item.Instance.AnimationController.TimeDelta = Math.Min(5, item.Instance.AnimationController.TimeDelta);
                });
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                if (this.Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    animObjects.ForEach(item =>
                    {
                        item.Instance.AnimationController.Start(0);
                    });
                }
                else
                {
                    animObjects.ForEach(item =>
                    {
                        item.Instance.AnimationController.Resume();
                    });
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                animObjects.ForEach(item =>
                {
                    item.Instance.AnimationController.Pause();
                });
            }
        }
        private void UpdateInputDebug()
        {
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
                this.showItemDEBUG = !this.showItemDEBUG;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.itemIndex--;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.itemIndex++;
            }

            this.itemIndex %= animObjects.Count;
            if (this.itemIndex < 0) this.itemIndex = animObjects.Count - 1;

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                this.showItem = !this.showItem;
            }
        }
        private void UpdateDebugData()
        {
            var selectedItem = animObjects[itemIndex];

            animObjects.ForEach(item =>
            {
                item.Visible = !this.showItemDEBUG || this.showItem || (item != selectedItem);
            });

            if (this.showItemDEBUG)
            {
                var tris = selectedItem.Instance.GetTriangles(true);
                var bbox = selectedItem.Instance.GetBoundingBox(true);

                this.itemTris.Instance.SetPrimitives(itemTrisColor, tris);
                this.itemLines.Instance.SetPrimitives(itemLinesColor, Line3D.CreateWiredBox(bbox));

                this.itemTris.Active = this.itemTris.Visible = true;
                this.itemLines.Active = this.itemLines.Visible = true;
            }
            else
            {
                if (this.itemTris != null)
                {
                    this.itemTris.Active = this.itemTris.Visible = false;
                }

                if (this.itemLines != null)
                {
                    this.itemLines.Active = this.itemLines.Visible = false;
                }
            }
        }

        private void SoldierControllerPathEnding(object sender, EventArgs e)
        {
            var keys = this.soldierPaths.Keys.ToArray();

            int index = Math.Min(this.rnd.Next(1, 3), keys.Length - 1);

            var key = keys[index];

            ((AnimationController)sender).SetPath(this.soldierPaths[key]);
            ((AnimationController)sender).Start(0);
        }
    }
}
