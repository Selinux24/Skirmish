using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Instancing
{
    class TestScene : Scene
    {
        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;

        private ModelInstanced troops = null;

        private bool gameReady = false;

        public TestScene(Game game) : base(game)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            await LoadResourcesAsync(
                new[]
                {
                    InitializeTexts(),
                    InitializeFloor(),
                    InitializeTrees(),
                    InitializeTroops(),
                    InitializeWall()
                },
                (res) =>
                {
                    if (!res.Completed)
                    {
                        res.ThrowExceptions();
                    }

                    Camera.Goto(new Vector3(-45, 17, -30));
                    Camera.LookTo(Vector3.Zero);
                    Camera.FarPlaneDistance = 250;

                    gameReady = true;
                });
        }

        private async Task InitializeTexts()
        {
            title = await this.AddComponentUITextArea("Title", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18), TextForeColor = Color.White });
            runtimeText = await this.AddComponentUITextArea("RuntimeText", new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 11), TextForeColor = Color.Yellow });

            title.Text = "Instancing test";
            runtimeText.Text = "";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await this.AddComponentSprite("Backpanel", spDesc, SceneObjectUsages.UI, LayerUI - 1);
        }
        private async Task InitializeFloor()
        {
            float l = 12f;
            float h = 0f;
            int side = 5;

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

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = "resources/ground/gravel_01_diffuse.jpg";
            mat.NormalMapTexture = "resources/ground/gravel_01_normal.jpg";

            var desc = new ModelInstancedDescription()
            {
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Instances = side * side,
                Content = ContentDescription.FromContentData(vertices, indices, mat),
            };

            var floor = await this.AddComponentModelInstanced("Floor", desc);

            Vector3 delta = new Vector3(l * side, 0, l * side) - new Vector3(l, 0, l);
            int x = 0;
            int y = 0;
            for (int i = 0; i < floor.InstanceCount; i++)
            {
                var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta;

                floor[i].Manipulator.SetPosition(iPos, true);

                x++;
                if (x >= side)
                {
                    x = 0;
                    y++;
                }
            }
        }
        private async Task InitializeTrees()
        {
            int instances = 40;

            var treeDesc = new ModelInstancedDescription()
            {
                CastShadow = true,
                Instances = instances,
                BlendMode = BlendModes.DefaultTransparent,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(@"Resources/Trees", @"tree.xml"),
            };
            var trees = await this.AddComponentModelInstanced("Trees", treeDesc);

            int side = instances / 4;
            float groundSide = 55f;

            for (int i = 0; i < trees.InstanceCount; i++)
            {
                var iPos = Vector3.Zero;

                if (i < side)
                {
                    iPos = new Vector3((i - ((side * 0) + (side * 0.5f))) * side, 0, +groundSide);
                }
                else if (i < side * 2)
                {
                    iPos = new Vector3(+groundSide, 0, (i - ((side * 1) + (side * 0.5f))) * side);
                }
                else if (i < side * 3)
                {
                    iPos = new Vector3((i - ((side * 2) + (side * 0.5f))) * side, 0, -groundSide);
                }
                else if (i < side * 4)
                {
                    iPos = new Vector3(-groundSide, 0, (i - ((side * 3) + (side * 0.5f))) * side);
                }

                trees[i].Manipulator.SetPosition(iPos, true);
                trees[i].Manipulator.SetRotation(iPos.Z + iPos.X, 0, 0, true);
                trees[i].Manipulator.SetScale(2 + (i % 3 * 0.2f), true);
                trees[i].TextureIndex = (uint)(i % 2);
            }
        }
        private async Task InitializeTroops()
        {
            var tDesc = new ModelInstancedDescription()
            {
                Instances = 100,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(@"Resources/Soldier", @"soldier_anim2.xml"),
            };
            troops = await this.AddComponentModelInstanced("Troops", tDesc, SceneObjectUsages.Agent);
            troops.MaximumCount = -1;

            Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

            var sp1 = new AnimationPath();
            sp1.AddLoop("idle1");
            animations.Add("soldier_idle1", new AnimationPlan(sp1));

            var sp2 = new AnimationPath();
            sp2.AddLoop("idle2");
            animations.Add("soldier_idle2", new AnimationPlan(sp2));

            string[] anim = new[] { "soldier_idle1", "soldier_idle2" };

            Random rnd = new Random(1);
            float l = 5;
            var vMax = new Vector3(l - 1, 0, l - 1);
            var vMin = -vMax;
            int side = 10;
            Vector3 delta = new Vector3(l * side, 0, l * side) - new Vector3(l, 0, l);

            int x = 0;
            int y = 0;
            for (int i = 0; i < troops.InstanceCount; i++)
            {
                var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta + rnd.NextVector3(vMin, vMax);

                troops[i].Manipulator.SetPosition(iPos, true);
                troops[i].Manipulator.SetRotation(iPos.Z, 0, 0, true);
                troops[i].TextureIndex = (uint)(i % 3);

                troops[i].AnimationController.TimeDelta = 0.4f + (0.1f * (i % 2));
                troops[i].AnimationController.AddPath(animations[anim[i % anim.Length]]);
                troops[i].AnimationController.Start(rnd.NextFloat(0f, 8f));

                x++;
                if (x >= side)
                {
                    x = 0;
                    y++;
                }
            }
        }
        private async Task InitializeWall()
        {
            var wall = await this.AddComponentModelInstanced(
                 "Wall",
                new ModelInstancedDescription()
                {
                    Instances = 40,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = ContentDescription.FromFile("Resources/Wall", "wall.xml"),
                });

            BoundingBox bbox = wall[0].GetBoundingBox();

            float x = bbox.Width * (10f / 11f);

            for (int i = 0; i < 10; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3((i - 5) * x, 0.01f, 60));
            }

            for (int i = 10; i < 20; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3(60, 0, (i - 9 - 5) * x));
                wall[i].Manipulator.SetRotation(MathUtil.PiOverTwo, 0, 0);
            }

            for (int i = 20; i < 30; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3((i - 19 - 5) * x, 0.01f, -60));
                wall[i].Manipulator.SetRotation(MathUtil.Pi, 0, 0);
            }

            for (int i = 30; i < 40; i++)
            {
                wall[i].Manipulator.SetPosition(new Vector3(-60, 0, (i - 30 - 5) * x));
                wall[i].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.Exit();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!gameReady)
            {
                return;
            }

#if DEBUG
            if (Game.Input.RightMouseButtonPressed)
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }
#else
            Camera.RotateMouse(
                Game.GameTime,
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

            int increment = Game.Input.ShiftPressed ? 10 : 1;

            if (Game.Input.KeyJustReleased(Keys.Left))
            {
                troops.MaximumCount = Math.Max(-1, troops.MaximumCount - increment);
            }

            if (Game.Input.KeyJustReleased(Keys.Right))
            {
                troops.MaximumCount = Math.Min(troops.InstanceCount, troops.MaximumCount + increment);
            }

            base.Update(gameTime);

            runtimeText.Text = Game.RuntimeText;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            runtimeText.SetPosition(new Vector2(5, title.Top + title.Height + 3));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = runtimeText.Top + runtimeText.Height + 3;


        }
    }
}
