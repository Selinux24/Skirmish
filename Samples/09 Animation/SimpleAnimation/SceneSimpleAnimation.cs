﻿using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Animation.SimpleAnimation
{
    public class SceneSimpleAnimation : Scene
    {
        private const int layerHUD = 99;

        private UITextArea title = null;
        private UITextArea runtime = null;
        private UITextArea animText = null;
        private UITextArea messages = null;
        private Sprite backPanel = null;
        private UIConsole console = null;

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

        private bool uiReady = false;
        private bool gameReady = false;

        public SceneSimpleAnimation(Game game)
            : base(game)
        {

        }

        public override async Task Initialize()
        {
            await InitializeUI();

            UpdateLayout();

            try
            {
                await LoadResourcesAsync(
                    new[]
                    {
                        InitializeLadder(),
                        InitializeLadder2(),
                        InitializeSoldier(),
                        InitializeRat(),
                        InitializeDoors(),
                        InitializeJails(),
                        InitializeFloor(),
                        InitializeDebug()
                    },
                    (res) =>
                    {
                        if (!res.Completed)
                        {
                            res.ThrowExceptions();
                        }

                        InitializeEnvironment();

                        gameReady = true;
                    });
            }
            catch (Exception ex)
            {
                messages.Text = ex.Message;
                messages.Visible = true;
            }
        }

        private async Task InitializeUI()
        {
            title = await this.AddComponentUITextArea("Title", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18), TextForeColor = Color.White }, layerHUD);
            runtime = await this.AddComponentUITextArea("Runtime", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 11), TextForeColor = Color.Yellow }, layerHUD);
            animText = await this.AddComponentUITextArea("AnimText", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 15), TextForeColor = Color.Orange }, layerHUD);
            messages = await this.AddComponentUITextArea("Messages", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 15), TextForeColor = Color.Orange }, layerHUD);

            title.Text = "Animation test";
            runtime.Text = "";
            animText.Text = "";
            messages.Text = "";

            backPanel = await this.AddComponentSprite("Backpanel", SpriteDescription.Default(new Color4(0, 0, 0, 0.75f)), SceneObjectUsages.UI, layerHUD - 1);

            var consoleDesc = UIConsoleDescription.Default(new Color4(0.35f, 0.35f, 0.35f, 1f));
            consoleDesc.LogFilterFunc = (l) => l.LogLevel > LogLevel.Trace || (l.LogLevel == LogLevel.Trace && l.CallerTypeName == nameof(AnimationController));
            console = await this.AddComponentUIConsole("Console", consoleDesc, layerHUD + 1);
            console.Visible = false;

            uiReady = true;
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
            mat.DiffuseTexture = "SimpleAnimation/resources/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SimpleAnimation/resources/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SimpleAnimation/resources/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            await this.AddComponentModel("Floor", desc);
        }
        private async Task InitializeLadder()
        {
            var ladder = await this.AddComponentModelInstanced(
                "Ladder",
                new ModelInstancedDescription()
                {
                    Instances = 2,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Ladder",
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

            animObjects.Add(ladder);
        }
        private async Task InitializeLadder2()
        {
            var ladder = await this.AddComponentModelInstanced(
                "Ladder2",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Ladder",
                        ModelContentFilename = "Dn_Anim_Ladder_2.xml",
                    }
                });

            var ladder2 = await this.AddComponentModelInstanced(
                "Ladder22",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Ladder",
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

            animObjects.Add(ladder);
            animObjects.Add(ladder2);
        }
        private async Task InitializeSoldier()
        {
            var soldier = await this.AddComponentModelInstanced(
                "Soldier",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Soldier",
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

            soldierPaths.Add("complex", new AnimationPlan(p0));
            soldierPaths.Add("idle1", new AnimationPlan(p1));
            soldierPaths.Add("idle2", new AnimationPlan(p2));
            soldierPaths.Add("stand", new AnimationPlan(p3));

            soldier[0].AnimationController.AddPath(soldierPaths["complex"]);
            soldier[1].AnimationController.AddPath(soldierPaths["complex"]);

            animObjects.Add(soldier);
        }
        private async Task InitializeRat()
        {
            var rat = await this.AddComponentModelInstanced(
                "Rat",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 2,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Rat",
                        ModelContentFilename = "rat.xml",
                    }
                });

            rat[0].Manipulator.SetPosition(2, 0, 0, true);
            rat[1].Manipulator.SetPosition(2.5f, 0, 5, true);

            AnimationPath p0 = new AnimationPath();
            p0.AddLoop("walk");

            ratPaths.Add("walk", new AnimationPlan(p0));

            rat[0].AnimationController.AddPath(ratPaths["walk"]);
            rat[1].AnimationController.AddPath(ratPaths["walk"]);

            animObjects.Add(rat);
        }
        private async Task InitializeDoors()
        {
            var doors = await this.AddComponentModelInstanced(
                "Doors",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Doors",
                        ModelContentFilename = "Dn_Doors.xml",
                    },
                });

            var walls = await this.AddComponentModelInstanced(
                "Walls",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Doors",
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

            animObjects.Add(doors);
        }
        private async Task InitializeJails()
        {
            var walls = await this.AddComponentModelInstanced(
                "Walls",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Doors",
                        ModelContentFilename = "Wall2.xml",
                    },
                });

            walls[0].Manipulator.SetPosition(10, 0, 8, true);
            walls[0].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            walls[0].Manipulator.SetScale(2.5f);

            var doors = await this.AddComponentModelInstanced(
                "Jails",
                new ModelInstancedDescription()
                {
                    CastShadow = true,
                    Instances = 1,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "SimpleAnimation/Resources/Doors",
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

            animObjects.Add(doors);
        }
        private async Task InitializeDebug()
        {
            itemTris = await this.AddComponentPrimitiveListDrawer("DebugItemTris", new PrimitiveListDrawerDescription<Triangle>() { Count = 5000, Color = itemTrisColor });
            itemLines = await this.AddComponentPrimitiveListDrawer("DebugItemLines", new PrimitiveListDrawerDescription<Line3D>() { Count = 1000, Color = itemLinesColor });
        }

        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            Lights.KeyLight.CastShadow = true;
            Lights.KeyLight.Direction = Vector3.Normalize(new Vector3(-0.1f, -1, 1));
            Lights.KeyLight.Enabled = true;
            Lights.BackLight.Enabled = false;
            Lights.FillLight.Enabled = false;
            Lights.HemisphericLigth = new SceneLightHemispheric("Ambient", Color.Gray, Color.White, true);

            BoundingBox bbox = new BoundingBox();
            animObjects.ForEach(item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    bbox = BoundingBox.Merge(bbox, item[i].GetBoundingBox());
                }
            });
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(0, playerHeight, -12f);
            Camera.LookTo(0, playerHeight * 0.6f, 0);
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<Start.SceneStart>();
            }

            if (!uiReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Oem5))
            {
                console.Toggle();
            }

            if (!gameReady)
            {
                return;
            }

            UpdateInputCamera(gameTime);
            UpdateInputAnimation();
            UpdateInputDebug();

            base.Update(gameTime);

            UpdateDebugData();

            var itemController = animObjects[itemIndex][0].AnimationController;

            runtime.Text = Game.RuntimeText;
            animText.Text = string.Format(
                "Paths: {0:00}; Delta: {1:0.0}; Index: {2}; Clip: {3}; Time: {4:0.00}; Item Time: {5:0.00}",
                itemController.PathCount,
                itemController.TimeDelta,
                itemController.CurrentIndex,
                itemController.CurrentPathItemClip,
                itemController.CurrentPathTime,
                itemController.CurrentPathItemTime);
        }
        private void UpdateInputCamera(GameTime gameTime)
        {
#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
            {
                Camera.RotateMouse(
                    gameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                gameTime,
                Game.Input.MouseXDelta,
                Game.Input.MouseYDelta);
#endif

            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInputAnimation()
        {
            if (Game.Input.KeyJustReleased(Keys.Left))
            {
                animObjects.ForEach(DecreaseDelta());
            }

            if (Game.Input.KeyJustReleased(Keys.Right))
            {
                animObjects.ForEach(IncreaseDelta());
            }

            if (Game.Input.KeyJustReleased(Keys.Up))
            {
                if (Game.Input.KeyPressed(Keys.ShiftKey))
                {
                    animObjects.ForEach(StartAnimation());
                }
                else
                {
                    animObjects.ForEach(ResumeAnimation());
                }
            }

            if (Game.Input.KeyJustReleased(Keys.Down))
            {
                animObjects.ForEach(PauseAnimation());
            }
        }
        private void UpdateInputDebug()
        {
            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (Game.Input.KeyJustReleased(Keys.C))
            {
                Lights.DirectionalLights[0].CastShadow = !Lights.DirectionalLights[0].CastShadow;
            }

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                showItemDEBUG = !showItemDEBUG;
            }

            if (Game.Input.KeyJustReleased(Keys.F2))
            {
                itemIndex--;
            }

            if (Game.Input.KeyJustReleased(Keys.F3))
            {
                itemIndex++;
            }

            itemIndex %= animObjects.Count;
            if (itemIndex < 0) itemIndex = animObjects.Count - 1;

            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                showItem = !showItem;
            }
        }
        private void UpdateDebugData()
        {
            var selectedItem = animObjects[itemIndex][0];

            animObjects.ForEach(item =>
            {
                for (int i = 0; i < item.InstanceCount; i++)
                {
                    item[i].Visible = !showItemDEBUG || showItem || (item[i] != selectedItem);
                }
            });

            if (showItemDEBUG)
            {
                var tris = selectedItem.GetTriangles();
                var bbox = selectedItem.GetBoundingBox();

                itemTris.SetPrimitives(itemTrisColor, tris);
                itemLines.SetPrimitives(itemLinesColor, Line3D.CreateWiredBox(bbox));

                itemTris.Active = itemTris.Visible = true;
                itemLines.Active = itemLines.Visible = true;
            }
            else
            {
                if (itemTris != null)
                {
                    itemTris.Active = itemTris.Visible = false;
                }

                if (itemLines != null)
                {
                    itemLines.Active = itemLines.Visible = false;
                }
            }
        }

        private void SoldierControllerPathEnding(object sender, EventArgs e)
        {
            if (sender is AnimationController controller)
            {
                var keys = soldierPaths.Keys.ToArray();

                int index = Math.Min(Helper.RandomGenerator.Next(1, 3), keys.Length - 1);

                var key = keys[index];

                controller.SetPath(soldierPaths[key]);
                controller.Start(0);
            }
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            if (!uiReady)
            {
                return;
            }

            title.SetPosition(Vector2.Zero);
            runtime.SetPosition(new Vector2(5, title.Rectangle.Bottom + 3));
            animText.SetPosition(new Vector2(5, runtime.Rectangle.Bottom + 3));
            messages.SetPosition(new Vector2(5, animText.Rectangle.Bottom + 3));

            backPanel.Width = Game.Form.RenderWidth;
            backPanel.Height = messages.Rectangle.Bottom + 3;

            console.Top = backPanel.Rectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}