using AISamples.Common;
using AISamples.Common.Agents;
using Engine;
using Engine.BuiltIn.Components.Models;
using Engine.BuiltIn.Components.Primitives;
using Engine.BuiltIn.UI;
using Engine.Common;
using Engine.Content;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AISamples.SceneCWRSelfDrivingCar
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
    class SelfDrivingCarScene : Scene
    {
        private const float spaceSize = 1000f;
        private const string resourceTerrainDiffuse = "SceneCWRSelfDrivingCar/resources/dirt002.dds";
        private const string resourceTerrainNormal = "SceneCWRSelfDrivingCar/resources/normal001.dds";
        private const string bestCarFileName = "bestCar.json";

        private Sprite panel = null;
        private UITextArea title = null;
        private UITextArea runtimeText = null;
        private UITextArea info = null;
        private UITextArea brain = null;
        private GeometryColorDrawer<Line3D> sensorDrawer = null;
        private GeometryColorDrawer<Triangle> roadDrawer = null;
        private GeometryColorDrawer<Line3D> roadLaneDrawer = null;
        private Model terrain = null;
        private ModelInstanced carModels = null;
        private ModelInstanced trafficModels = null;

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

        private Car bestCar = null;
        private Car[] cars = [];
        private Car[] traffic = [];
        private Vector2[] trafficPositions = [];
        private Visualizer visualizer = null;

        private const float carWidth = 10;
        private const float carHeight = 7;
        private const float carDepth = 20;
        private const int maxCarInstances = 10;
        private const int maxTrafficInstances = 3;

        private readonly Color4 carColor = new(0.1f, 0.1f, 0.6f, 0.2f);
        private readonly Color4 carTrafficColor = new(0.6f, 0.1f, 0.1f, 1f);
        private readonly Color4 carDamagedColor = new(0.5f, 0.5f, 0.5f, 1f);
        private readonly Color4 carSensorColor = Color.Yellow;
        private readonly Color4 carSensorContactColor = Color.OrangeRed;

        private readonly Road road = new(0, 20, 3, spaceSize * 0.5f);
        private readonly Color4 roadColor = Color.DarkGray;
        private readonly Color4 roadLaneColor = Color.WhiteSmoke;

        private bool followCar = true;
        private readonly AgentFollower carFollower = new(100, 50);

        public SelfDrivingCarScene(Game game) : base(game)
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
                    InitializeVisualizerDrawer,
                    InitializeCars,
                ],
                InitializeComponentsCompleted);

            LoadResources(group);
        }
        private async Task InitializeTitle()
        {
            var defaultFont18 = FontDescription.FromFamily("Gill Sans MT, Arial", 18);

            title = await AddComponentUI<UITextArea, UITextAreaDescription>("Title", "Title", new UITextAreaDescription { Font = defaultFont18, TextForeColor = Color.White });
            title.Text = titleText;

            var spDesc = SpriteDescription.Default(new Color4(0, 0, 0, 0.66f));
            panel = await AddComponentUI<Sprite, SpriteDescription>("Panel", "Panel", spDesc, LayerUI - 1);
        }
        private async Task InitializeTexts()
        {
            var defaultFont11 = FontDescription.FromFamily("Gill Sans MT, Arial", 11);

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

            terrain = await AddComponentGround<Model, ModelDescription>("Terrain", "Terrain", desc);
        }
        private async Task InitializeLineDrawer()
        {
            var desc = new GeometryColorDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            roadLaneDrawer = await AddComponentEffect<GeometryColorDrawer<Line3D>, GeometryColorDrawerDescription<Line3D>>(
                "roadLaneDrawer",
                "roadLaneDrawer",
                desc,
                LayerEffects - 1);
        }
        private async Task InitializeSensorDrawer()
        {
            var desc = new GeometryColorDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = false,
            };
            sensorDrawer = await AddComponentEffect<GeometryColorDrawer<Line3D>, GeometryColorDrawerDescription<Line3D>>(
                "SensorDrawer",
                "SensorDrawer",
                desc,
                LayerEffects + 2);
        }
        private async Task InitializeTriangleDrawer()
        {
            var desc = new GeometryColorDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = true,
            };
            roadDrawer = await AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                "roadDrawer",
                "roadDrawer",
                desc,
                LayerEffects + 1);
        }
        private async Task InitializeVisualizerDrawer()
        {
            var descL = new GeometryColorDrawerDescription<Line3D>()
            {
                Count = 20000,
                DepthEnabled = true,
                BlendMode = BlendModes.Alpha,
            };
            var visualizerLineDrawer = await AddComponentEffect<GeometryColorDrawer<Line3D>, GeometryColorDrawerDescription<Line3D>>(
                "visualizerLineDrawer",
                "visualizerLineDrawer",
                descL);

            var descT = new GeometryColorDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = true,
                BlendMode = BlendModes.Alpha,
            };
            var visualizerTriangleDrawer = await AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                "visualizerTriangleDrawer",
                "visualizerTriangleDrawer",
                descT);

            var descO = new GeometryColorDrawerDescription<Triangle>()
            {
                Count = 20000,
                DepthEnabled = true,
                BlendMode = BlendModes.Opaque,
            };
            var visualizerOpaqueDrawer = await AddComponentEffect<GeometryColorDrawer<Triangle>, GeometryColorDrawerDescription<Triangle>>(
                "visualizerOpaqueDrawer",
                "visualizerOpaqueDrawer",
                descO,
                LayerEffects - 5);

            visualizer = new(visualizerOpaqueDrawer, visualizerTriangleDrawer, visualizerLineDrawer);
        }
        private async Task InitializeCars()
        {
            var geo = GeometryUtil.CreateBox(Topology.TriangleList, carWidth, carHeight, carDepth);
            var mat = MaterialPhongContent.Default;
            mat.IsTransparent = true;

            var cDesc = new ModelInstancedDescription()
            {
                Instances = maxCarInstances,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromContentData(ContentData.GenerateTriangleList(geo, mat)),
                BlendMode = BlendModes.Alpha,
                StartsVisible = false,
            };
            carModels = await AddComponentAgent<ModelInstanced, ModelInstancedDescription>("Cars", "Cars", cDesc);

            var tDesc = new ModelInstancedDescription()
            {
                Instances = maxTrafficInstances,
                CastShadow = ShadowCastingAlgorihtms.All,
                Content = ContentDescription.FromContentData(ContentData.GenerateTriangleList(geo, mat)),
                BlendMode = BlendModes.Alpha,
                StartsVisible = false,
            };
            trafficModels = await AddComponentAgent<ModelInstanced, ModelInstancedDescription>("Traffic", "Traffic", tDesc);

            cars = new Car[maxCarInstances];
            traffic = new Car[maxTrafficInstances];
            trafficPositions = new Vector2[maxTrafficInstances];
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

            carModels.Visible = true;
            trafficModels.Visible = true;

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
            if (Game.Input.KeyJustReleased(Keys.F5))
            {
                SaveBestCar();
            }
            if (Game.Input.KeyJustReleased(Keys.F6))
            {
                DeleteBestCar();
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
            UpdateInputCar(bestCar);

            BeginDrawSensor();
            UpdateCars(gameTime);
            UpdateTraffic(gameTime);
            UpdateRoad();

            SelectBestCar();

            DrawNeuralNetwork(bestCar);
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
        private void SaveBestCar()
        {
            bestCar.Brain.Save(bestCarFileName);
        }
        private static void DeleteBestCar()
        {
            if (File.Exists(bestCarFileName))
            {
                File.Delete(bestCarFileName);
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
        private void UpdateInputCar(Car car)
        {
            if (car == null)
            {
                return;
            }

            if (car.ControlType != AgentControlTypes.Player)
            {
                return;
            }

            car.Controls.Forward = Game.Input.KeyPressed(Keys.Up);
            car.Controls.Reverse = Game.Input.KeyPressed(Keys.Down);
            car.Controls.Left = Game.Input.KeyPressed(Keys.Left);
            car.Controls.Right = Game.Input.KeyPressed(Keys.Right);
        }

        private void UpdateCars(IGameTime gameTime)
        {
            for (int i = 0; i < cars.Length; i++)
            {
                UpdateCar(gameTime, cars[i], carModels[i], traffic, false, carColor);
            }
        }
        private void UpdateCar(IGameTime gameTime, Car car, ModelInstance carModel, Car[] tr, bool damageTraffic, Color4 color)
        {
            if (car.Damaged)
            {
                return;
            }

            car.Update(gameTime, road.GetBorders(), tr, damageTraffic);

            carModel.Manipulator.SetTransform(car.GetTransform());

            bool isBestCar = car == bestCar;
            var betColor = isBestCar ? new Color4(color.ToVector3(), 1f) : color;
            carModel.TintColor = car.Damaged ? carDamagedColor : betColor;

            if (!isBestCar)
            {
                return;
            }

            if (car.Sensor != null)
            {
                DrawSensor(car);
            }

            if (car.Brain != null)
            {
                DrawBrain(car);
            }
        }
        private void UpdateTraffic(IGameTime gameTime)
        {
            for (int i = 0; i < traffic.Length; i++)
            {
                UpdateCar(gameTime, traffic[i], trafficModels[i], [], false, carTrafficColor);
            }
        }

        private void BeginDrawSensor()
        {
            sensorDrawer.Clear(carSensorColor);
            sensorDrawer.Clear(carSensorContactColor);
        }
        private void DrawSensor(Car car)
        {
            var readings = car.Sensor?.GetReadings() ?? [];

            var rayList = car.Sensor?.GetRays().SelectMany(r => DrawSensorRay(r, readings))
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

        private void DrawBrain(Car car)
        {
            var b = car.Brain;
            if (b == null)
            {
                return;
            }

            brain.Text = car.Controls.ToString();
        }

        private void UpdateRoad()
        {
            if (bestCar == null)
            {
                return;
            }

            if (road == null)
            {
                return;
            }

            float depth = bestCar.GetPosition().Y;

            road.Update(depth);

            roadDrawer.Manipulator.SetPosition(0, 0, depth);
            roadLaneDrawer.Manipulator.SetPosition(0, 0, depth);
            terrain.Manipulator.SetPosition(0, 0, depth);
        }
        private void DrawRoad()
        {
            roadDrawer.Clear(roadColor);
            roadLaneDrawer.Clear(roadLaneColor);

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

                roadDrawer.AddPrimitives(roadColor, tris);
                roadLaneDrawer.AddPrimitives(roadLaneColor, lines);
            }
        }

        private void DrawNeuralNetwork(Car car)
        {
            var network = car?.Brain;
            if (network == null)
            {
                return;
            }

            float depth = car.GetPosition().Y;

            visualizer.DrawNetwork(network, new(-100, 100, depth), 25, 90, 120, 2);
        }

        private void SelectBestCar()
        {
            var c = cars
                .Where(c => !c.Damaged)
                .OrderByDescending(c => c.GetPosition().Y)
                .FirstOrDefault();

            if (c != null)
            {
                bestCar = c;
                carFollower.Car = bestCar;
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
            bool mutate = File.Exists(bestCarFileName);

            for (int i = 0; i < cars.Length; i++)
            {
                cars[i] = new(carWidth, carHeight, carDepth, AgentControlTypes.AI, 1, 0.5f);

                if (!mutate)
                {
                    continue;
                }

                cars[i].Brain.Load(bestCarFileName);
                if (i == 0)
                {
                    continue;
                }

                cars[i].Brain.Mutate(0.1f);
            }
            carFollower.Car = bestCar = cars[0];

            for (int i = 0; i < traffic.Length; i++)
            {
                traffic[i] = new(carWidth, carHeight, carDepth, AgentControlTypes.Dummy, 0.5f, 0.25f);

                var lanePos = road.GetLaneCenter(i % traffic.Length);
                lanePos.Y = Helper.RandomGenerator.NextFloat(0, 3) * -(carDepth * 3);

                trafficPositions[i] = lanePos;
            }

            RelocateSimulationObjects();
        }
        private void ResetSimulation()
        {
            for (int i = 0; i < cars.Length; i++)
            {
                cars[i].Reset();
            }

            for (int i = 0; i < traffic.Length; i++)
            {
                traffic[i].Reset();
            }

            RelocateSimulationObjects();
        }
        private void RelocateSimulationObjects()
        {
            var cLanePos = road.GetLaneCenter(road.LaneCount / 2);
            cLanePos.Y = -(carDepth * 10);

            for (int i = 0; i < cars.Length; i++)
            {
                cars[i].SetPosition(cLanePos);
            }

            for (int i = 0; i < traffic.Length; i++)
            {
                traffic[i].SetPosition(trafficPositions[i]);
            }
        }
    }
}
