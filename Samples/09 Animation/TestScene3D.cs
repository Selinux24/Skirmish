using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Animation
{
    public class TestScene3D : Scene
    {
        private const int layerHUD = 99;

        private TextDrawer runtime = null;
        private TextDrawer animText = null;
        private TextDrawer messages = null;

        private PrimitiveListDrawer<Triangle> itemTris = null;
        private PrimitiveListDrawer<Line3D> itemLines = null;
        private readonly Color itemTrisColor = new Color(Color.Yellow.ToColor3(), 0.6f);
        private readonly Color itemLinesColor = new Color(Color.Red.ToColor3(), 1f);
        private bool showItemDEBUG = false;
        private bool showItem = true;
        private int itemIndex = 0;

        private static Action<ModelInstanced> StartAnimation()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].AnimationController.Start(0);
                }
            };
        }
        private static Action<ModelInstanced> PauseAnimation()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].AnimationController.Pause();
                }
            };
        }
        private static Action<ModelInstanced> ResumeAnimation()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].AnimationController.Resume();
                }
            };
        }
        private static Action<ModelInstanced> IncreaseDelta()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    var controller = item[i].AnimationController;

                    controller.TimeDelta += 0.1f;
                    controller.TimeDelta = Math.Min(5, controller.TimeDelta);
                }
            };
        }
        private static Action<ModelInstanced> DecreaseDelta()
        {
            return item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    var controller = item[i].AnimationController;

                    controller.TimeDelta -= 0.1f;
                    controller.TimeDelta = Math.Max(0, controller.TimeDelta);
                }
            };
        }

        private readonly List<ModelInstanced> animObjects = new List<ModelInstanced>();

        private readonly Dictionary<string, AnimationPlan> soldierPaths = new Dictionary<string, AnimationPlan>();
        private readonly Dictionary<string, AnimationPlan> ratPaths = new Dictionary<string, AnimationPlan>();

        private Guid assetsId = Guid.NewGuid();
        private bool gameReady = false;

        public TestScene3D(Game game)
            : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override async Task Initialize()
        {
            await this.InitializeUI();

            try
            {
                await this.LoadResourcesAsync(assetsId,
                    this.InitializeLadder(),
                    this.InitializeLadder2(),
                    this.InitializeSoldier(),
                    this.InitializeRat(),
                    this.InitializeDoors(),
                    this.InitializeJails(),
                    this.InitializeFloor(),
                    this.InitializeDebug());
            }
            catch (Exception ex)
            {
                messages.Text = ex.Message;
                messages.Visible = true;
            }
        }
        public override void GameResourcesLoaded(Guid id)
        {
            if (id == assetsId)
            {
                InitializeEnvironment();

                gameReady = true;
            }
        }

        private async Task InitializeUI()
        {
            var title = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            this.runtime = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.animText = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 15, Color.Orange), SceneObjectUsages.UI, layerHUD);
            this.messages = await this.AddComponentTextDrawer(TextDrawerDescription.Generate("Tahoma", 15, Color.Orange), SceneObjectUsages.UI, layerHUD);

            title.Text = "Animation test";
            this.runtime.Text = "";
            this.animText.Text = "";
            this.messages.Text = "";

            title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, title.Top + title.Height + 3);
            this.animText.Position = new Vector2(5, this.runtime.Top + this.runtime.Height + 3);
            this.messages.Position = new Vector2(5, this.animText.Top + this.animText.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.messages.Top + this.messages.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
        }
        private async Task InitializeFloor()
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

            await this.AddComponentModel(desc);
        }
        private async Task InitializeLadder()
        {
            var ladder = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Ladder",
                    Instances = 2,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Ladder",
                        ModelContentFilename = "Dn_Anim_Ladder.xml",
                    }
                });

            ladder[0].Manipulator.SetPosition(-4f, 1, 0, true);
            ladder[1].Manipulator.SetPosition(-4.5f, 1, 5, true);

            ladder[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            ladder[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            AnimationPath def = new AnimationPath();
            def.Add("default");
            AnimationPath pull = new AnimationPath();
            pull.Add("pull");
            AnimationPath push = new AnimationPath();
            push.Add("push");

            Dictionary<string, AnimationPlan> ladderPaths = new Dictionary<string, AnimationPlan>
            {
                { "default", new AnimationPlan(def) },
                { "pull", new AnimationPlan(pull) },
                { "push", new AnimationPlan(push) }
            };

            ladder[0].AnimationController.AddPath(ladderPaths["pull"]);
            ladder[1].AnimationController.AddPath(ladderPaths["pull"]);

            this.animObjects.Add(ladder);
        }
        private async Task InitializeLadder2()
        {
            var ladder = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Ladder2",
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Ladder",
                        ModelContentFilename = "Dn_Anim_Ladder_2.xml",
                    }
                });

            var ladder2 = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Ladder22",
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Ladder",
                        ModelContentFilename = "Dn_Anim_Ladder_22.xml",
                    }
                });

            ladder[0].Manipulator.SetPosition(-3f, 1, 0, true);
            ladder[1].Manipulator.SetPosition(-3.5f, 1, 5, true);

            ladder[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            ladder[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            ladder2[0].Manipulator.SetPosition(-2f, 1, 0, true);
            ladder2[1].Manipulator.SetPosition(-2.5f, 1, 5, true);

            ladder2[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            ladder2[1].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);

            AnimationPath def = new AnimationPath();
            def.Add("default");
            AnimationPath pull = new AnimationPath();
            pull.Add("pull");
            AnimationPath push = new AnimationPath();
            push.Add("push");

            Dictionary<string, AnimationPlan> ladder2Paths = new Dictionary<string, AnimationPlan>
            {
                { "default", new AnimationPlan(def) },
                { "pull", new AnimationPlan(pull) },
                { "push", new AnimationPlan(push) }
            };

            ladder[0].AnimationController.AddPath(ladder2Paths["pull"]);
            ladder[1].AnimationController.AddPath(ladder2Paths["pull"]);

            ladder2[0].AnimationController.AddPath(ladder2Paths["push"]);
            ladder2[1].AnimationController.AddPath(ladder2Paths["push"]);

            this.animObjects.Add(ladder);
            this.animObjects.Add(ladder2);
        }
        private async Task InitializeSoldier()
        {
            var soldier = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Soldier",
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Soldier",
                        ModelContentFilename = "soldier_anim2.xml",
                    }
                });

            soldier[0].Manipulator.SetPosition(0, 0, 0, true);
            soldier[1].Manipulator.SetPosition(0.5f, 0, 5, true);

            soldier[0].AnimationController.PathEnding += SoldierControllerPathEnding;
            soldier[1].AnimationController.PathEnding += SoldierControllerPathEnding;

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

            soldier[0].AnimationController.AddPath(this.soldierPaths["complex"]);
            soldier[1].AnimationController.AddPath(this.soldierPaths["complex"]);

            this.animObjects.Add(soldier);
        }
        private async Task InitializeRat()
        {
            var rat = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Rat",
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Rat",
                        ModelContentFilename = "rat.xml",
                    }
                });

            rat[0].Manipulator.SetPosition(2, 0, 0, true);
            rat[1].Manipulator.SetPosition(2.5f, 0, 5, true);

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");

            this.ratPaths.Add("walk", new AnimationPlan(p0));

            rat[0].AnimationController.AddPath(this.ratPaths["walk"]);
            rat[1].AnimationController.AddPath(this.ratPaths["walk"]);

            this.animObjects.Add(rat);
        }
        private async Task InitializeDoors()
        {
            var doors = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Doors",
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Doors",
                        ModelContentFilename = "Dn_Doors.xml",
                    },
                });

            var walls = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Walls",
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Doors",
                        ModelContentFilename = "Wall1.xml",
                    },
                });

            doors[0].Manipulator.SetPosition(-10, 0, 8, true);
            doors[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            doors[0].Manipulator.SetScale(2.5f);

            walls[0].Manipulator.SetPosition(-10, 0, 8, true);
            walls[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            walls[0].Manipulator.SetScale(2.5f);

            AnimationPath def = new AnimationPath();
            def.Add("default");
            AnimationPath open = new AnimationPath();
            open.Add("open");
            AnimationPath close = new AnimationPath();
            close.Add("close");
            AnimationPath rep = new AnimationPath();
            rep.Add("open");
            rep.Add("close");

            Dictionary<string, AnimationPlan> doorsPaths = new Dictionary<string, AnimationPlan>
            {
                { "default", new AnimationPlan(def) },
                { "open", new AnimationPlan(open) },
                { "close", new AnimationPlan(close) },
                { "rep", new AnimationPlan(rep) }
            };

            doors[0].AnimationController.AddPath(doorsPaths["rep"]);

            this.animObjects.Add(doors);
        }
        private async Task InitializeJails()
        {
            var walls = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Walls",
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Doors",
                        ModelContentFilename = "Wall2.xml",
                    },
                });

            walls[0].Manipulator.SetPosition(10, 0, 8, true);
            walls[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            walls[0].Manipulator.SetScale(2.5f);

            var doors = await this.AddComponentModelInstanced(
                new ModelInstancedDescription()
                {
                    Name = "Jails",
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Doors",
                        ModelContentFilename = "Dn_Jails.xml",
                    }
                });

            doors[0].Manipulator.SetPosition(10, 0, 8, true);
            doors[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            doors[0].Manipulator.SetScale(2.5f);

            AnimationPath def = new AnimationPath();
            def.Add("default");
            AnimationPath open = new AnimationPath();
            open.Add("open");
            AnimationPath close = new AnimationPath();
            close.Add("close");
            AnimationPath rep = new AnimationPath();
            rep.Add("open");
            rep.Add("close");

            Dictionary<string, AnimationPlan> jailsPaths = new Dictionary<string, AnimationPlan>
            {
                { "default", new AnimationPlan(def) },
                { "open", new AnimationPlan(open) },
                { "close", new AnimationPlan(close) },
                { "rep", new AnimationPlan(rep) }
            };

            doors[0].AnimationController.AddPath(jailsPaths["rep"]);

            this.animObjects.Add(doors);
        }
        private async Task InitializeDebug()
        {
            this.itemTris = await this.AddComponentPrimitiveListDrawer<Triangle>(new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, Color = itemTrisColor });
            this.itemLines = await this.AddComponentPrimitiveListDrawer<Line3D>(new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, Color = itemLinesColor });
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
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    bbox = BoundingBox.Merge(bbox, item[i].GetBoundingBox());
                }
            });
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(0, playerHeight, -12f);
            this.Camera.LookTo(0, playerHeight * 0.6f, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (!gameReady)
            {
                return;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

            this.UpdateInputCamera(gameTime, shift);
            this.UpdateInputAnimation();
            this.UpdateInputDebug();

            base.Update(gameTime);

            this.UpdateDebugData();

            var itemController = animObjects[itemIndex][0].AnimationController;

            this.runtime.Text = this.Game.RuntimeText;
            this.animText.Text = string.Format(
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
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                gameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

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
                animObjects.ForEach(DecreaseDelta());
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                animObjects.ForEach(IncreaseDelta());
            }

            if (this.Game.Input.KeyJustReleased(Keys.Up))
            {
                if (this.Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    animObjects.ForEach(StartAnimation());
                }
                else
                {
                    animObjects.ForEach(ResumeAnimation());
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.Down))
            {
                animObjects.ForEach(PauseAnimation());
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
            var selectedItem = animObjects[itemIndex][0];

            animObjects.ForEach(item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].Visible = !this.showItemDEBUG || this.showItem || (item[i] != selectedItem);
                }
            });

            if (this.showItemDEBUG)
            {
                var tris = selectedItem.GetTriangles(true);
                var bbox = selectedItem.GetBoundingBox(true);

                this.itemTris.SetPrimitives(itemTrisColor, tris);
                this.itemLines.SetPrimitives(itemLinesColor, Line3D.CreateWiredBox(bbox));

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

            int index = Math.Min(Helper.RandomGenerator.Next(1, 3), keys.Length - 1);

            var key = keys[index];

            ((AnimationController)sender).SetPath(this.soldierPaths[key]);
            ((AnimationController)sender).Start(0);
        }
    }
}
