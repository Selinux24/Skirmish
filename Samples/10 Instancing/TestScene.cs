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
        private const int layerObjects = 0;
        private const int layerTerrain = 1;
        private const int layerHUD = 99;

        private UITextArea runtimeText = null;

        private ModelInstanced troops = null;

        private bool gameReady = false;

        public TestScene(Game game) : base(game, SceneModes.ForwardLigthning)
        {

        }

        public override async Task Initialize()
        {
            GameEnvironment.Background = Color.CornflowerBlue;

            await this.LoadResourcesAsync(
                new[]
                {
                    InitializeTexts(),
                    InitializeFloor(),
                    InitializeTrees(),
                    InitializeTroops(),
                    InitializeWall()
                },
                () =>
                {
                    this.Camera.Goto(new Vector3(-45, 17, -30));
                    this.Camera.LookTo(Vector3.Zero);
                    this.Camera.FarPlaneDistance = 250;

                    gameReady = true;
                });
        }

        private async Task InitializeTexts()
        {
            var title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.Default("Tahoma", 18, Color.White) }, layerHUD);
            runtimeText = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.Default("Tahoma", 11, Color.Yellow) }, layerHUD);

            title.Text = "Instancing test";
            runtimeText.Text = "";

            title.SetPosition(Vector2.Zero);
            runtimeText.SetPosition(new Vector2(5, title.Top + title.Height + 3));

            var spDesc = new SpriteDescription()
            {
                Width = this.Game.Form.RenderWidth,
                Height = this.runtimeText.Top + this.runtimeText.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            await this.AddComponentSprite(spDesc, SceneObjectUsages.UI, layerHUD - 1);
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

            MaterialContent mat = MaterialContent.Default;
            mat.DiffuseTexture = "resources/ground/gravel_01_diffuse.jpg";
            mat.NormalMapTexture = "resources/ground/gravel_01_normal.jpg";

            var content = ModelContent.GenerateTriangleList(vertices, indices, mat);

            var desc = new ModelInstancedDescription()
            {
                Name = "Floor",
                CastShadow = true,
                DeferredEnabled = true,
                DepthEnabled = true,
                UseAnisotropicFiltering = true,
                Instances = side * side,
                Content = new ContentDescription()
                {
                    ModelContent = content,
                }
            };

            var floor = await this.AddComponentModelInstanced(desc);

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
                Name = "Trees",
                CastShadow = true,
                Instances = instances,
                BlendMode = BlendModes.DefaultTransparent,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Trees",
                    ModelContentFilename = @"tree.xml",
                }
            };
            var trees = await this.AddComponentModelInstanced(treeDesc, SceneObjectUsages.None, layerTerrain);

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
                Name = "Troops",
                Instances = 100,
                CastShadow = true,
                UseAnisotropicFiltering = true,
                Content = new ContentDescription()
                {
                    ContentFolder = @"Resources/Soldier",
                    ModelContentFilename = @"soldier_anim2.xml",
                }
            };
            this.troops = await this.AddComponentModelInstanced(tDesc, SceneObjectUsages.Agent, layerObjects);
            this.troops.MaximumCount = -1;

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
            for (int i = 0; i < this.troops.InstanceCount; i++)
            {
                var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta + rnd.NextVector3(vMin, vMax);

                this.troops[i].Manipulator.SetPosition(iPos, true);
                this.troops[i].Manipulator.SetRotation(iPos.Z, 0, 0, true);
                this.troops[i].TextureIndex = (uint)(i % 3);

                this.troops[i].AnimationController.TimeDelta = 0.4f + (0.1f * (i % 2));
                this.troops[i].AnimationController.AddPath(animations[anim[i % anim.Length]]);
                this.troops[i].AnimationController.Start(rnd.NextFloat(0f, 8f));

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
                new ModelInstancedDescription()
                {
                    Name = "wall",
                    Instances = 40,
                    CastShadow = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/Wall",
                        ModelContentFilename = "wall.xml",
                    }
                });

            BoundingBox bbox = wall[0].GetBoundingBox();

            float x = bbox.GetX() * (10f / 11f);

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
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            if (!gameReady)
            {
                return;
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                this.Game.GameTime,
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

            int increment = shift ? 10 : 1;

            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.troops.MaximumCount = Math.Max(-1, this.troops.MaximumCount - increment);
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.troops.MaximumCount = Math.Min(this.troops.InstanceCount, this.troops.MaximumCount + increment);
            }

            base.Update(gameTime);

            this.runtimeText.Text = this.Game.RuntimeText;
        }
    }
}
