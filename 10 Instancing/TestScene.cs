using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using System;
using System.Collections.Generic;

namespace Instancing
{
    class TestScene : Scene
    {
        private const int layerObjects = 0;
        private const int layerTerrain = 1;
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> title = null;
        private SceneObject<TextDrawer> runtime = null;
        private SceneObject<Sprite> backPannel = null;

        private SceneObject<ModelInstanced> floor = null;

        private SceneObject<ModelInstanced> trees = null;

        private SceneObject<ModelInstanced> troops = null;
        private Dictionary<string, AnimationPlan> animations = new Dictionary<string, AnimationPlan>();

        public TestScene(Game game) : base(game, SceneModesEnum.ForwardLigthning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            GameEnvironment.Background = Color.CornflowerBlue;

            #region Texts

            this.title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsageEnum.UI, layerHUD);
            this.runtime = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 11, Color.Yellow), SceneObjectUsageEnum.UI, layerHUD);

            this.title.Instance.Text = "Instancing test";
            this.runtime.Instance.Text = "";

            this.title.Instance.Position = Vector2.Zero;
            this.runtime.Instance.Position = new Vector2(5, this.title.Instance.Top + this.title.Instance.Height + 3);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = this.runtime.Instance.Top + this.runtime.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.backPannel = this.AddComponent<Sprite>(spDesc, SceneObjectUsageEnum.UI, layerHUD - 1);

            #endregion

            #region Floor

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
                    Static = true,
                    CastShadow = true,
                    DeferredEnabled = true,
                    DepthEnabled = true,
                    AlphaEnabled = false,
                    UseAnisotropicFiltering = true,
                    Instances = side * side,
                    Content = new ContentDescription()
                    {
                        ModelContent = content,
                    }
                };

                this.floor = this.AddComponent<ModelInstanced>(desc);

                Vector3 delta = new Vector3(l * side, 0, l * side) - new Vector3(l, 0, l);
                int x = 0;
                int y = 0;
                for (int i = 0; i < this.floor.Count; i++)
                {
                    var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta;

                    this.floor.Instance[i].Manipulator.SetPosition(iPos, true);

                    x++;
                    if (x >= side)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            #endregion

            #region Trees

            {
                int instances = 40;

                var treeDesc = new ModelInstancedDescription()
                {
                    Name = "Trees",
                    CastShadow = true,
                    Static = true,
                    Instances = instances,
                    AlphaEnabled = true,
                    UseAnisotropicFiltering = true,
                    Content = new ContentDescription()
                    {
                        ContentFolder = @"Resources/Trees",
                        ModelContentFilename = @"tree.xml",
                    }
                };
                this.trees = this.AddComponent<ModelInstanced>(treeDesc, SceneObjectUsageEnum.None, layerTerrain);

                int side = instances / 4;
                float groundSide = 55f;

                for (int i = 0; i < this.trees.Count; i++)
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

                    this.trees.Instance[i].Manipulator.SetPosition(iPos, true);
                    this.trees.Instance[i].Manipulator.SetRotation(iPos.Z + iPos.X, 0, 0, true);
                    this.trees.Instance[i].Manipulator.SetScale(2 + (i % 3 * 0.2f), true);
                    this.trees.Instance[i].TextureIndex = (uint)(i % 2);
                }
            }

            #endregion

            #region Troops

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
                this.troops = this.AddComponent<ModelInstanced>(tDesc, SceneObjectUsageEnum.Agent, layerObjects);
                this.troops.Instance.MaximumCount = -1;

                {
                    var sp = new AnimationPath();
                    sp.AddLoop("stand");
                    this.animations.Add("soldier_stand", new AnimationPlan(sp));
                }

                {
                    var sp = new AnimationPath();
                    sp.AddLoop("idle1");
                    this.animations.Add("soldier_idle", new AnimationPlan(sp));
                }

                {
                    var sp = new AnimationPath();
                    sp.AddLoop("idle2");
                    this.animations.Add("soldier_idle2", new AnimationPlan(sp));
                }

                string[] anim = new[] { "soldier_stand", "soldier_idle", "soldier_idle2" };

                Random rnd = new Random(1);
                float l = 5;
                var vMax = new Vector3(l - 1, 0, l - 1);
                var vMin = -vMax;
                int side = 10;
                Vector3 delta = new Vector3(l * side, 0, l * side) - new Vector3(l, 0, l);

                int x = 0;
                int y = 0;
                for (int i = 0; i < this.troops.Count; i++)
                {
                    var iPos = new Vector3(x * l * 2, 0, y * l * 2) - delta + rnd.NextVector3(vMin, vMax);

                    this.troops.Instance[i].Manipulator.SetPosition(iPos, true);
                    this.troops.Instance[i].Manipulator.SetRotation(iPos.Z, 0, 0, true);
                    this.troops.Instance[i].TextureIndex = (uint)(i % 2);

                    this.troops.Instance[i].AnimationController.TimeDelta = 0.4f + (0.1f * (i % 2));
                    this.troops.Instance[i].AnimationController.AddPath(this.animations[anim[i % 3]]);
                    this.troops.Instance[i].AnimationController.Start(rnd.NextFloat(0f, 8f));

                    x++;
                    if (x >= side)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            #endregion

            this.Camera.Goto(new Vector3(-45, 17, -30));
            this.Camera.LookTo(Vector3.Zero);
            this.Camera.FarPlaneDistance = 250;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.SetRenderMode(this.GetRenderMode() == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning);
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
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


            if (this.Game.Input.KeyJustReleased(Keys.Left))
            {
                this.troops.Instance.MaximumCount -= 1;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Right))
            {
                this.troops.Instance.MaximumCount += 1;
            }

            base.Update(gameTime);

            this.runtime.Instance.Text = this.Game.RuntimeText;
        }
    }
}
