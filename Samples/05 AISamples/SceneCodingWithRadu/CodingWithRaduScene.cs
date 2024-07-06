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
        private UITextArea brain = null;
        private PrimitiveListDrawer<Line3D> lineDrawer = null;
        private PrimitiveListDrawer<Line3D> sensorDrawer = null;
        private PrimitiveListDrawer<Triangle> triangleDrawer = null;

        private const string titleText = "SELF-DRIVING CAR";
        private const string infoText = "PRESS F1 FOR HELP";
        private const string helpText = @"F1 - HELP
F - TOGGLE FOLLOW CAR
E - START SIMULATION
R - RESET SIMULATION
WASD - MOVE CAMERA
SPACE - MOVE CAMERA UP
C - MOVE CAMERA DOWN
MOUSE - ROTATE CAMERA
ESC - EXIT";
        private bool showHelp = false;

        private bool gameReady = false;

        private Car car = null;
        private Car[] traffic = [];
        private Vector2[] trafficPositions = [];

        private readonly Color4 carColor = new(0.1f, 0.1f, 0.6f, 1f);
        private readonly Color4 carTrafficColor = new(0.6f, 0.1f, 0.1f, 1f);
        private readonly Color4 carDamagedColor = new(0.5f, 0.5f, 0.5f, 1f);
        private readonly Color4 carEdgeColor = new(0.2f, 0.2f, 1f, 1f);
        private readonly Color4 carSensorColor = Color.Yellow;
        private readonly Color4 carSensorContactColor = Color.OrangeRed;

        private readonly Road road = new(0, 20, 3);
        private readonly Color4 roadColor = Color.DarkGray;
        private readonly Color4 roadEdgeColor = Color.WhiteSmoke;

        private bool followCar = true;
        private readonly CarFollower carFollower = new(100, 50);

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
                    InitializeTitle,
                    InitializeTexts,
                    InitializeTerrain,
                    InitializeLineDrawer,
                    InitializeSensorDrawer,
                    InitializeTriangleDrawer,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTitle()
        {
            var defaultFont18 = TextDrawerDescription.FromFamily("Gill Sans MT, Arial", 18);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = titleText;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeTexts()
        {
            var defaultFont11 = TextDrawerDescription.FromFamily("Gill Sans MT, Arial", 11);

            runtimeText = await AddComponentUI<UITextArea, UITextAreaDescription>("RuntimeText", "RuntimeText", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            info = await AddComponentUI<UITextArea, UITextAreaDescription>("Information", "Information", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });
            brain = await AddComponentUI<UITextArea, UITextAreaDescription>("Brain", "Brain", new UITextAreaDescription { Font = defaultFont11, TextForeColor = Color.Yellow, MaxTextLength = 256 });

            runtimeText.Text = "";
            info.Text = infoText;
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
        private async Task InitializeSensorDrawer()
        {
            var desc = new PrimitiveListDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = false,
            };
            sensorDrawer = await AddComponentEffect<PrimitiveListDrawer<Line3D>, PrimitiveListDrawerDescription<Line3D>>(
                "SensorDrawer",
                "SensorDrawer",
                desc,
                LayerEffects + 2);
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

            UpdateLayout();

            Camera.Goto(new Vector3(0, 120, -175));
            Camera.LookTo(Vector3.Zero);
            Camera.FarPlaneDistance = spaceSize * 1.5f;
            Camera.MovementDelta = spaceSize * 0.2f;
            Camera.SlowMovementDelta = Camera.MovementDelta / 20f;

            DrawRoad();

            StartSimulation();

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

            if (Game.Input.KeyJustReleased(Keys.F1))
            {
                ToggleHelp();
            }
            if (Game.Input.KeyJustReleased(Keys.F))
            {
                followCar = !followCar;
            }

            if (Game.Input.KeyJustReleased(Keys.E))
            {
                StartSimulation();
            }
            if (Game.Input.KeyJustReleased(Keys.R))
            {
                ResetSimulation();
            }

            UpdateInputCamera(gameTime);
            UpdateInputCar();

            BeginDrawCar();
            BeginDrawSensor();
            UpdateCar(gameTime, car, traffic, carColor);
            UpdateTraffic(gameTime);
        }

        private void ToggleHelp()
        {
            showHelp = !showHelp;

            if (showHelp)
            {
                info.Text = helpText;
            }
            else
            {
                info.Text = infoText;
            }
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
                Camera.Following = carFollower;
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
            if (car.ControlType != CarControlTypes.Player)
            {
                return;
            }

            car.Controls.Forward = Game.Input.KeyPressed(Keys.Up);
            car.Controls.Reverse = Game.Input.KeyPressed(Keys.Down);
            car.Controls.Left = Game.Input.KeyPressed(Keys.Left);
            car.Controls.Right = Game.Input.KeyPressed(Keys.Right);
        }

        private void UpdateCar(IGameTime gameTime, Car c, Car[] tr, Color4 color)
        {
            c.Update(gameTime, road, tr);

            DrawCar(c, color);

            if (c.Sensor != null)
            {
                DrawSensor(c);
            }

            if (c.Brain != null)
            {
                DrawBrain(c);
            }
        }
        private void UpdateTraffic(IGameTime gameTime)
        {
            foreach (var c in traffic)
            {
                UpdateCar(gameTime, c, [], carTrafficColor);
            }
        }

        private void BeginDrawCar()
        {
            triangleDrawer.Clear(carColor);
            triangleDrawer.Clear(carTrafficColor);
            triangleDrawer.Clear(carDamagedColor);
            lineDrawer.Clear(carEdgeColor);
        }
        private void DrawCar(Car c, Color4 color)
        {
            var box = c.GetBox();
            var tris = Triangle.ComputeTriangleList(box);
            var lines = Line3D.CreateBox(box);

            triangleDrawer.AddPrimitives(c.Damaged ? carDamagedColor : color, tris);
            lineDrawer.AddPrimitives(carEdgeColor, lines);
        }

        private void BeginDrawSensor()
        {
            sensorDrawer.Clear(carSensorColor);
            sensorDrawer.Clear(carSensorContactColor);
        }
        private void DrawSensor(Car c)
        {
            var readings = c.Sensor?.GetReadings() ?? [];

            var rayList = c.Sensor?.GetRays().SelectMany(r => DrawSensorRay(r, readings))
                .GroupBy(r => r.Item1)
                .ToDictionary(
                    keySelector => keySelector.Key,
                    elementSelector => elementSelector.Select(r => r.Item2));

            sensorDrawer.AddPrimitives(rayList ?? []);
        }
        private IEnumerable<(Color4, Line3D)> DrawSensorRay(PickingRay r, SensorReading[] readings)
        {
            var p0 = r.Start;
            p0.Y += 0.5f;
            var p1 = p0 + (r.Direction * r.MaxDistance);

            //Find readings for the ray
            var rayReading = Array.Find(readings, rd => rd?.Ray == r);
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

        private void DrawBrain(Car c)
        {
            var b = c.Brain;
            if (b == null)
            {
                return;
            }

            brain.Text = c.Controls.ToString();
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

            float panelBottom = runtimeText.Top + runtimeText.Height;
            panel.Width = Game.Form.RenderWidth;
            panel.Height = panelBottom;

            info.SetPosition(new Vector2(5, panelBottom + 3));

            brain.TextVerticalAlign = TextVerticalAlign.Top;
            brain.TextHorizontalAlign = TextHorizontalAlign.Right;
            brain.SetPosition(0, panelBottom + 3);
            brain.SetRectangle(new(0, 0, Game.Form.RenderWidth, Game.Form.RenderHeight - (panelBottom + 3)));
        }

        private void StartSimulation()
        {
            const float carWidth = 10;
            const float carHeight = 7;
            const float carDepth = 20;

            car = new(carWidth, carHeight, carDepth, CarControlTypes.AI, 1, 0.5f);
            carFollower.Car = car;

            const int trafficCars = 3;
            traffic = new Car[trafficCars];
            trafficPositions = new Vector2[trafficCars];
            for (int i = 0; i < traffic.Length; i++)
            {
                traffic[i] = new(carWidth, carHeight, carDepth, CarControlTypes.Dummy, 0.5f, 0.25f);

                var lanePos = road.GetLaneCenter(i % traffic.Length);
                lanePos.Y = Helper.RandomGenerator.NextFloat(0, 3) * -(carDepth * 3);

                trafficPositions[i] = lanePos;
            };

            RelocateSimulationObjects();
        }
        private void ResetSimulation()
        {
            car.Reset();
            for (int i = 0; i < traffic.Length; i++)
            {
                traffic[i].Reset();
            }

            RelocateSimulationObjects();
        }
        private void RelocateSimulationObjects()
        {
            var cLanePos = road.GetLaneCenter(road.LaneCount / 2);
            cLanePos.Y = -200;
            car.SetPosition(cLanePos);

            for (int i = 0; i < traffic.Length; i++)
            {
                traffic[i].SetPosition(trafficPositions[i]);
            };
        }
    }
}
