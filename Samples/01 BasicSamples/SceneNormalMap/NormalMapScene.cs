﻿using BasicSamples.SceneStart;
using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System.Threading.Tasks;

namespace BasicSamples.SceneNormalMap
{
    /// <summary>
    /// Normal map test scene
    /// </summary>
    public class NormalMapScene : Scene
    {
        private readonly string resourcesFolder = "SceneNormalMap/resources";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtime = null;

        private Model lightEmitter = null;
        private SceneLightPoint pointLight = null;

        private bool gameReady = false;

        public NormalMapScene(Game game)
            : base(game)
        {
#if DEBUG
            Game.VisibleMouse = false;
            Game.LockMouse = false;
#else
            Game.VisibleMouse = false;
            Game.LockMouse = true;
#endif
        }

        public override void Initialize()
        {
            base.Initialize();

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var group = LoadResourceGroup.FromTasks(
                [
                    InitializeUI,
                    InitializeDungeonWall,
                    InitializeEmitter,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeUI()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Tahoma", 18);
            var defaultFont12 = TextDrawerDescription.FromFamily("Tahoma", 12);
            defaultFont18.LineAdjust = true;
            defaultFont12.LineAdjust = true;

            var titleDesc = new UITextAreaDescription()
            {
                Font = defaultFont18,
                Text = "Tiled Wall Test Scene",
                TextForeColor = Color.White,
            };

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", titleDesc);

            var runtimeDesc = new UITextAreaDescription()
            {
                Font = defaultFont12,
                MaxTextLength = 256,
                TextForeColor = Color.Yellow,
            };

            runtime = await AddComponentUI<UITextArea, UITextAreaDescription>("FPS", "FPS", runtimeDesc);

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.75f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("BackPanel", "BackPanel", spDesc, LayerUI - 1);
        }
        private async Task InitializeDungeonWall()
        {
            var desc = new ModelInstancedDescription()
            {
                Instances = 7,
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromFile(resourcesFolder, "wall.json"),
            };

            var wall = await AddComponent<ModelInstanced, ModelInstancedDescription>("Wall", "Wall", desc);

            var bbox = wall[0].GetBoundingBox();

            float x = bbox.Width * (10f / 11f);
            float z = bbox.Depth;

            wall[0].Manipulator.SetPosition(new Vector3(+3 * x, 0, +0 * z));
            wall[1].Manipulator.SetPosition(new Vector3(+2 * x, 0, +0 * z));
            wall[2].Manipulator.SetPosition(new Vector3(+1 * x, 0, +0 * z));
            wall[3].Manipulator.SetPosition(new Vector3(+0 * x, 0, +0 * z));
            wall[4].Manipulator.SetPosition(new Vector3(-1 * x, 0, +0 * z));
            wall[5].Manipulator.SetPosition(new Vector3(-2 * x, 0, +0 * z));
            wall[6].Manipulator.SetPosition(new Vector3(-3 * x, 0, +0 * z));
        }
        private async Task InitializeEmitter()
        {
            var mat = MaterialPhongContent.Default;
            mat.EmissiveColor = Color.White.RGB();

            var sphere = GeometryUtil.CreateSphere(Topology.TriangleList, 0.05f, 32, 15);

            var desc = new ModelDescription()
            {
                Content = ContentDescription.FromContentData(sphere, mat),
            };

            lightEmitter = await AddComponent<Model, ModelDescription>("Emitter", "Emitter", desc);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                res.ThrowExceptions();
            }

            UpdateLayout();

            StartCamera();
            StartEnvironment();

            gameReady = true;
        }
        private void StartCamera()
        {
            Camera.NearPlaneDistance = 0.5f;
            Camera.FarPlaneDistance = 500;
            Camera.Mode = CameraModes.Free;
            Camera.SetPosition(-5, 3, -5);
            Camera.SetInterest(0, 0, 0);
        }
        private void StartEnvironment()
        {
            GameEnvironment.Background = Color.Black;

            var desc = SceneLightPointDescription.Create(new Vector3(0, 1, -1), 10f, 10f);

            pointLight = new SceneLightPoint("light", false, Color3.White, Color3.White, true, desc);

            Lights.Add(pointLight);
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<StartScene>();
            }

            if (Game.Input.KeyJustReleased(Keys.R))
            {
                SetRenderMode(GetRenderMode() == SceneModes.ForwardLigthning ?
                    SceneModes.DeferredLightning :
                    SceneModes.ForwardLigthning);
            }

            UpdateCamera();

            UpdateLight(gameTime);

            runtime.Text = Game.RuntimeText;
        }
        private void UpdateCamera()
        {
            if (Game.Input.KeyPressed(Keys.A))
            {
                Camera.MoveLeft(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.D))
            {
                Camera.MoveRight(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.W))
            {
                Camera.MoveForward(Game.GameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Camera.MoveBackward(Game.GameTime, Game.Input.ShiftPressed);
            }

#if DEBUG
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
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
        }
        private void UpdateLight(IGameTime gameTime)
        {
            var pos = pointLight.Position;

            if (Game.Input.KeyPressed(Keys.Left))
            {
                pos.X -= gameTime.ElapsedSeconds * 5f;
            }

            if (Game.Input.KeyPressed(Keys.Right))
            {
                pos.X += gameTime.ElapsedSeconds * 5f;
            }

            if (Game.Input.KeyPressed(Keys.Up))
            {
                pos.Z += gameTime.ElapsedSeconds * 5f;
            }

            if (Game.Input.KeyPressed(Keys.Down))
            {
                pos.Z -= gameTime.ElapsedSeconds * 5f;
            }

            lightEmitter.Manipulator.SetPosition(pos);
            pointLight.Position = pos;
        }

        public override void GameGraphicsResized()
        {
            base.GameGraphicsResized();

            UpdateLayout();
        }
        private void UpdateLayout()
        {
            title.SetPosition(Vector2.Zero);
            runtime.SetPosition(new Vector2(0, 24));
            panel.Width = Game.Form.RenderWidth;
            panel.Height = runtime.Top + runtime.Height + 3;
        }
    }
}
