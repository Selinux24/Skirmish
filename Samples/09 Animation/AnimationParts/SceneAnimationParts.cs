﻿using Engine;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Threading.Tasks;

namespace Animation.AnimationParts
{
    class SceneAnimationParts : Scene
    {
        private const int layerHUD = 99;
        private const int layerModels = 10;

        private UITextArea title = null;
        private UIPanel backPanel = null;
        private UIConsole console = null;

        private Model tank;

        private bool uiReady = false;
        private bool gameReady = false;

        public SceneAnimationParts(Game game) : base(game)
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
                        InitializeTank(),
                        InitializeFloor(),
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
                Logger.WriteError(this, ex);
            }
        }

        private async Task InitializeUI()
        {
            title = await this.AddComponentUITextArea(new UITextAreaDescription { Font = TextDrawerDescription.FromFamily("Tahoma", 18, Color.White) }, layerHUD);

            title.Text = "Model Parts Test";

            backPanel = await this.AddComponentUIPanel(UIPanelDescription.Default(new Color4(0, 0, 0, 0.75f)), layerHUD - 1);

            var consoleDesc = UIConsoleDescription.Default(new Color4(0.35f, 0.35f, 0.35f, 1f));
            console = await this.AddComponentUIConsole(consoleDesc, layerHUD + 1);
            console.Visible = false;

            uiReady = true;
        }

        private async Task InitializeTank()
        {
            var tDesc = new ModelDescription()
            {
                Name = "Tanks",
                CastShadow = true,
                Optimize = false,
                Content = new ContentDescription()
                {
                    ContentFolder = "AnimationParts/Resources/Leopard",
                    ModelContentFilename = "Leopard.xml",
                },
                TransformNames = new[]
                {
                    "Hull-mesh",
                    "Turret-mesh",
                    "Barrel-mesh"
                },
                TransformDependences = new[]
                {
                    -1,
                    0,
                    1
                },
            };

            tank = await this.AddComponentModel(tDesc, SceneObjectUsages.Agent, layerModels);
            tank.Manipulator.SetScale(0.5f);
        }
        private async Task InitializeFloor()
        {
            float l = 50f;
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
            mat.DiffuseTexture = "AnimationParts/Resources/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "AnimationParts/Resources/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "AnimationParts/Resources/d_road_asphalt_stripes_specular.dds";

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

            await this.AddComponentModel(desc);
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

            BoundingBox bbox = tank.GetBoundingBox();
            float playerHeight = bbox.Maximum.Y - bbox.Minimum.Y;
            Vector3 lookDir = Vector3.Normalize(new Vector3(0, playerHeight, -playerHeight * 2f)) * 50;

            Camera.NearPlaneDistance = 0.1f;
            Camera.FarPlaneDistance = 500;
            Camera.Goto(lookDir);
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
            UpdateInputTank(gameTime);
            UpdateInputDebug();

            base.Update(gameTime);
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
        private void UpdateInputTank(GameTime gameTime)
        {
            if (Game.Input.ShiftPressed)
            {
                UpdateInputHull(gameTime);
            }
            else
            {
                UpdateInputTurret(gameTime);
            }
        }
        private void UpdateInputHull(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.J) && Game.Input.ShiftPressed)
            {
                tank.Manipulator.Rotate(-gameTime.ElapsedSeconds, 0, 0);
            }
            if (Game.Input.KeyPressed(Keys.L) && Game.Input.ShiftPressed)
            {
                tank.Manipulator.Rotate(+gameTime.ElapsedSeconds, 0, 0);
            }

            if (Game.Input.KeyPressed(Keys.I))
            {
                tank.Manipulator.MoveForward(gameTime, 10);
            }
            if (Game.Input.KeyPressed(Keys.K))
            {
                tank.Manipulator.MoveBackward(gameTime, 10);
            }
        }
        private void UpdateInputTurret(GameTime gameTime)
        {
            if (Game.Input.KeyPressed(Keys.J) && !Game.Input.ShiftPressed)
            {
                tank["Turret-mesh"].Manipulator.Rotate(-gameTime.ElapsedSeconds, 0, 0);
            }
            if (Game.Input.KeyPressed(Keys.L) && !Game.Input.ShiftPressed)
            {
                tank["Turret-mesh"].Manipulator.Rotate(+gameTime.ElapsedSeconds, 0, 0);
            }

            if (Game.Input.KeyPressed(Keys.I))
            {
                tank["Barrel-mesh"].Manipulator.Rotate(0, gameTime.ElapsedSeconds, 0);
            }
            if (Game.Input.KeyPressed(Keys.K))
            {
                tank["Barrel-mesh"].Manipulator.Rotate(0, -gameTime.ElapsedSeconds, 0);
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

            backPanel.Width = Game.Form.RenderWidth;
            backPanel.Height = title.Rectangle.Bottom + 3;

            console.Top = backPanel.Rectangle.Bottom;
            console.Width = Game.Form.RenderWidth;
        }
    }
}
