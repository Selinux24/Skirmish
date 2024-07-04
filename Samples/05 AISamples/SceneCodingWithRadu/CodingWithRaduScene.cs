using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AISamples.SceneCodingWithRadu
{
    /// <summary>
    /// Coding with Radu scene
    /// </summary>
    /// <remarks>
    /// It's a engine capacity test scene, trying to simulate a self-driving car, using the Radu's course as reference:
    /// https://www.youtube.com/playlist?list=PLB0Tybl0UNfYoJE7ZwsBQoDIG4YN9ptyY
    /// https://github.com/gniziemazity/Self-driving-car
    /// https://radufromfinland.com/
    /// </remarks>
    class CodingWithRaduScene : Scene
    {
        private const float spaceSize = 500f;
        private const string resourceTerrainDiffuse = "SceneCodingWithRadu/resources/dirt002.dds";
        private const string resourceTerrainNormal = "SceneCodingWithRadu/resources/normal001.dds";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;
        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Triangle> triangleDrawer = null;

        private bool gameReady = false;

        private readonly Car car = new(0, 0, 10, 7, 20);
        private readonly Color4 carColor = new(0.1f, 0.1f, 0.6f, 1f);
        private readonly Color4 carDamagedColor = new(0.5f, 0.5f, 0.5f, 1f);
        private readonly Color4 carEdgeColor = new(0.2f, 0.2f, 1f, 1f);
        private readonly Color4 carSensorColor = Color.LightYellow;
        private readonly Color4 carSensorContactColor = Color.OrangeRed;

        private readonly Road road = new(0, 15, 4);
        private readonly Color4 roadColor = Color.DarkGray;
        private readonly Color4 roadEdgeColor = Color.WhiteSmoke;

        private bool followCar = false;
        private readonly float followCarDistance = 100;
        private readonly float followCarHeight = 50;

        public CodingWithRaduScene(Game game) : base(game)
        {
            Game.VisibleMouse = true;
            Game.LockMouse = false;

            GameEnvironment.Background = Color.Black;
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
                    InitializeTexts,
                    InitializeTerrain,
                    InitializeLineDrawer,
                    InitializeTriangleDrawer,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTexts()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Gill Sans MT, Arial", 18);
            var defaultFont11 = TextDrawerDescription.FromFamily("Gill Sans MT, Arial", 11);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });

            title.Text = "SELF-DRIVING CAR";
            runtimeText.Text = "";
            info.Text = "Press F1 for Help.";

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeTerrain()
        {
            float l = spaceSize;
            float h = 0f;

            var geo = GeometryUtil.CreatePlane(l, h, Vector3.Up);
            geo.Uvs = geo.Uvs.Select(uv => uv * 5f);

            var mat = MaterialBlinnPhongContent.Default;
            mat.DiffuseTexture = resourceTerrainDiffuse;
            mat.NormalMapTexture = resourceTerrainNormal;

            var desc = new ModelDescription()
            {
                CastShadow = ShadowCastingAlgorihtms.All,
                UseAnisotropicFiltering = true,
                Content = ContentDescription.FromContentData(geo, mat),
            };

            await AddComponentGround<Model, ModelDescription>("Terrain", "Terrain", desc);
        }
        private async Task InitializeLineDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            lineDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "EdgeDrawer",
                "EdgeDrawer",
                desc,
                LayerEffects - 1);
        }
        private async Task InitializeTriangleDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            triangleDrawer = await AddComponentEffect<PrimitiveListDrawer<Triangle>, PrimitiveListDrawerDescription<Triangle>>(
                "TriangleDrawer",
                "TriangleDrawer",
                desc,
                LayerEffects + 1);
        }
        private void InitializeComponentsCompleted(LoadResourcesResult res)
        {
            if (!res.Completed)
            {
                var exList = res.GetExceptions();
                foreach (var ex in exList)
                {
                    Logger.WriteError(this, ex);
                }

                Game.Exit();
            }

            car.SetPosition(road.GetLaneCenter(road.LaneCount - 1));

            UpdateLayout();

            Camera.Goto(new Vector3(0, 120, -175));
            Camera.LookTo(Vector3.Zero);
            Camera.FarPlaneDistance = spaceSize * 1.5f;
            Camera.MovementDelta = spaceSize * 0.2f;
            Camera.SlowMovementDelta = Camera.MovementDelta / 20f;

            DrawRoad();

            gameReady = true;
        }

        public override void Update(IGameTime gameTime)
        {
            base.Update(gameTime);

            if (Game.Input.KeyJustReleased(Keys.Escape))
            {
                Game.SetScene<SceneStart.StartScene>();
            }

            if (!gameReady)
            {
                return;
            }

            if (Game.Input.KeyJustReleased(Keys.F))
            {
                followCar = !followCar;
            }

            UpdateInputCamera(gameTime);
            UpdateInputCar();

            UpdateCar(gameTime);
        }
        private void UpdateInputCamera(IGameTime gameTime)
        {
            if (!followCar)
            {
                Camera.Following = null;

                UpdateInputCameraManual(gameTime);

                return;
            }

            if (Camera.Following == null)
            {
                Camera.Following = new CarFollower(car, followCarDistance, followCarHeight);
            }
        }
        private void UpdateInputCameraManual(IGameTime gameTime)
        {
            if (Game.Input.MouseButtonPressed(MouseButtons.Right))
            {
                Camera.RotateMouse(
                    Game.GameTime,
                    Game.Input.MouseXDelta,
                    Game.Input.MouseYDelta);
            }

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
                Vector3 fwd = new(Camera.Forward.X, 0, Camera.Forward.Z);
                fwd.Normalize();
                Camera.Move(gameTime, fwd, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.S))
            {
                Vector3 bwd = new(Camera.Backward.X, 0, Camera.Backward.Z);
                bwd.Normalize();
                Camera.Move(gameTime, bwd, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.C))
            {
                Camera.MoveDown(gameTime, Game.Input.ShiftPressed);
            }

            if (Game.Input.KeyPressed(Keys.Space))
            {
                Camera.MoveUp(gameTime, Game.Input.ShiftPressed);
            }
        }
        private void UpdateInputCar()
        {
            car.Controls.Forward = Game.Input.KeyPressed(Keys.Up);
            car.Controls.Reverse = Game.Input.KeyPressed(Keys.Down);
            car.Controls.Left = Game.Input.KeyPressed(Keys.Left);
            car.Controls.Right = Game.Input.KeyPressed(Keys.Right);
        }

        private void UpdateCar(IGameTime gameTime)
        {
            car.Update(gameTime, road);

            DrawCar();
            DrawSensor();
        }

        private void DrawCar()
        {
            triangleDrawer.Clear(carColor);
            triangleDrawer.Clear(carDamagedColor);
            lineDrawer.Clear(carEdgeColor);

            var box = car.GetBox();
            var tris = Triangle.ComputeTriangleList(box);
            var lines = Line3D.CreateBox(box);

            triangleDrawer.AddPrimitives(car.Damaged ? carDamagedColor : carColor, tris);
            lineDrawer.AddPrimitives(carEdgeColor, lines);
        }
        private void DrawSensor()
        {
            lineDrawer.Clear(carSensorColor);
            lineDrawer.Clear(carSensorContactColor);

            var readings = car.Sensor.GetReadings();

            var rayList = car.Sensor.GetRays().SelectMany(r => DrawSensorRay(r, readings))
                .GroupBy(r => r.Item1)
                .ToDictionary(
                    keySelector => keySelector.Key,
                    elementSelector => elementSelector.Select(r => r.Item2));

            lineDrawer.AddPrimitives(rayList);
        }
        private IEnumerable<(Color4, Line3D)> DrawSensorRay(PickingRay r, SensorReading[] readings)
        {
            var p0 = r.Start;
            p0.Y += 0.5f;
            var p1 = p0 + (r.Direction * r.MaxDistance);

            //Find readings for the ray
            var rayReading = Array.Find(readings, rd => rd.Ray == r);
            if (rayReading == null)
            {
                //No reading, draw the ray
                yield return (carSensorColor, new Line3D(p0, p1));

                yield break;
            }

            var pi = rayReading.Position;
            pi.Y += 0.5f;

            yield return (carSensorColor, new Line3D(p0, pi));
            yield return (carSensorContactColor, new Line3D(pi, p1));
        }
        private void DrawRoad()
        {
            triangleDrawer.Clear(roadColor);
            lineDrawer.Clear(roadEdgeColor);

            const float h = 0.2f;
            var rectPoints = road.GetLanes().Select(r =>
                new Vector3[] {
                    new (r.Left, h, r.Top),
                    new (r.Right, h, r.Top),
                    new (r.Right, h, r.Bottom),
                    new (r.Left, h, r.Bottom),
                });

            foreach (var rect in rectPoints)
            {
                var gTris = GeometryUtil.CreatePolygonTriangleList(rect, false);
                var gLines = GeometryUtil.CreatePolygonLineList(rect);

                var tris = Triangle.ComputeTriangleList(gTris);
                var lines = Line3D.CreateFromVertices(gLines);

                triangleDrawer.AddPrimitives(roadColor, tris);
                lineDrawer.AddPrimitives(roadEdgeColor, lines);
            }
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
            info.SetPosition(new Vector2(5, runtimeText.Top + runtimeText.Height + 3));

            panel.Width = Game.Form.RenderWidth;
            panel.Height = info.Top + info.Height + 3;
        }
    }
}
