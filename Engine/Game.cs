using Engine.Common;
using Engine.UI;
using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Game class
    /// </summary>
    public class Game : IDisposable
    {
        private const string LogLineString = "**************************************************************************";
        private const string NoIdString = "no-id";

        private static int loadResourceId = 0;
        private static int GetLoadResourceId()
        {
            return ++loadResourceId;
        }

        /// <summary>
        /// Images helper static instance
        /// </summary>
        private static IImages images;
        /// <summary>
        /// Images helper
        /// </summary>
        public static IImages Images
        {
            get
            {
                images ??= EngineServiceFactory.Instance<IImages>();

                return images;
            }
        }
        /// <summary>
        /// Fonts helper static instance
        /// </summary>
        private static IFonts fonts;
        /// <summary>
        /// Fonts helper
        /// </summary>
        public static IFonts Fonts
        {
            get
            {
                fonts ??= EngineServiceFactory.Instance<IFonts>();

                return fonts;
            }
        }

        /// <summary>
        /// Scene list
        /// </summary>
        private readonly List<Scene> scenes = [];
        /// <summary>
        /// Next scene to load
        /// </summary>
        private Scene nextScene = null;
        /// <summary>
        /// Application exiting flag
        /// </summary>
        private bool exiting = false;
        /// <summary>
        /// Game paused
        /// </summary>
        private bool paused = false;
        /// <summary>
        /// Resource request queue
        /// </summary>
        private readonly ConcurrentQueue<ILoadResourceGroup> resourceRequests = [];
        /// <summary>
        /// Resource integration running flag
        /// </summary>
        private bool integratingResources = false;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = null;
        /// <summary>
        /// Buffer manager
        /// </summary>
        public BufferManager BufferManager { get; private set; }
        /// <summary>
        /// Resource manager
        /// </summary>
        public GameResourceManager ResourceManager { get; private set; }
        /// <summary>
        /// Game form
        /// </summary>
        public IEngineForm Form { get; private set; }
        /// <summary>
        /// Game time
        /// </summary>
        public IGameTime GameTime { get; private set; }
        /// <summary>
        /// Input helper
        /// </summary>
        public IInput Input { get; private set; }
        /// <summary>
        /// Graphics helper
        /// </summary>
        public Graphics Graphics { get; private set; }
        /// <summary>
        /// Runtime stats
        /// </summary>
        public string RuntimeText { get; private set; }
        /// <summary>
        /// Number of scenes
        /// </summary>
        public int SceneCount
        {
            get
            {
                return scenes.Count;
            }
        }
        /// <summary>
        /// Number of active scenes
        /// </summary>
        public int ActiveScenesCount
        {
            get
            {
                return scenes.FindAll(s => s.Active).Count;
            }
        }
        /// <summary>
        /// Gets or sets if the cursor is visible
        /// </summary>
        public bool VisibleMouse
        {
            get
            {
                if (Input != null)
                {
                    return Input.VisibleMouse;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (Input != null)
                {
                    Input.VisibleMouse = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets if the cursor is locked to the screen center
        /// </summary>
        public bool LockMouse
        {
            get
            {
                if (Input != null)
                {
                    return Input.LockMouse;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (Input != null)
                {
                    Input.LockMouse = value;
                }
            }
        }
        /// <summary>
        /// Takes a shoot of the game status in the next frame
        /// </summary>
        public bool CollectGameStatus { get; set; }
        /// <summary>
        /// Game status
        /// </summary>
        public readonly GameStatus GameStatus = new();

        /// <summary>
        /// Game status collected event
        /// </summary>
        public event GameStatusCollectedHandler GameStatusCollected;

        /// <summary>
        /// Gets desktop mode description
        /// </summary>
        /// <returns>Returns current desktop mode description</returns>
        private static OutputDescription1 GetDesktopMode()
        {
            using var factory = new Factory1();
            using var factory5 = factory.QueryInterface<Factory5>();
            using var adapter = factory5.GetAdapter1(0);
            using var adapter4 = adapter.QueryInterface<Adapter4>();
            using var adapterOutput = adapter4.GetOutput(0);
            using var adapterOutput6 = adapterOutput.QueryInterface<Output6>();
            return adapterOutput6.Description1;
        }
        /// <summary>
        /// Gets the log level base on frame time
        /// </summary>
        /// <param name="frameTime">Frame time</param>
        /// <returns>Returns the log level</returns>
        private static LogLevel EvaluateTime(long frameTime)
        {
            if (frameTime > 500) return LogLevel.Warning;
            if (frameTime > 30) return LogLevel.Information;
            else return LogLevel.Debug;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="vsyncEnabled">Vertical Sync</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        public Game(string name, bool vsyncEnabled = true, int refreshRate = 0, int multiSampling = 0) :
            this(name, true, 0, 0, vsyncEnabled, refreshRate, multiSampling)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="screenWidth">Window width</param>
        /// <param name="screenHeight">Window height</param>
        /// <param name="vsyncEnabled">Vertical Sync</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        public Game(string name, int screenWidth, int screenHeight, bool vsyncEnabled = true, int refreshRate = 0, int multiSampling = 0) :
            this(name, false, screenWidth, screenHeight, vsyncEnabled, refreshRate, multiSampling)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="screenWidth">Window width</param>
        /// <param name="screenHeight">Window height</param>
        /// <param name="vsyncEnabled">Vertical Sync</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        public Game(string name, float screenWidth, float screenHeight, bool vsyncEnabled = true, int refreshRate = 0, int multiSampling = 0) :
            this(name, false, (int)screenWidth, (int)screenHeight, vsyncEnabled, refreshRate, multiSampling)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="screenSize">Window size</param>
        /// <param name="vsyncEnabled">Vertical Sync</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        public Game(string name, Vector2Int screenSize, bool vsyncEnabled = true, int refreshRate = 0, int multiSampling = 0) :
            this(name, false, screenSize.X, screenSize.Y, vsyncEnabled, refreshRate, multiSampling)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="screenSize">Window size</param>
        /// <param name="vsyncEnabled">Vertical Sync</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        public Game(string name, Vector2 screenSize, bool vsyncEnabled = true, int refreshRate = 0, int multiSampling = 0) :
            this(name, false, (int)screenSize.X, (int)screenSize.Y, vsyncEnabled, refreshRate, multiSampling)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="fullScreen">Full screen window</param>
        /// <param name="screenWidth">Window width</param>
        /// <param name="screenHeight">Window height</param>
        /// <param name="vsyncEnabled">Vertical Sync</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        private Game(string name, bool fullScreen, int screenWidth, int screenHeight, bool vsyncEnabled, int refreshRate, int multiSampling)
        {
            Name = name;

            GameTime = new GameTime();

            BufferManager = new BufferManager(this);

            ResourceManager = new GameResourceManager(this);

            #region Form

            bool isFullScreen = fullScreen;
            if (screenWidth == 0 || screenHeight == 0)
            {
                var mode = GetDesktopMode();

                isFullScreen = true;
                screenWidth = mode.DesktopCoordinates.Right - mode.DesktopCoordinates.Left;
                screenHeight = mode.DesktopCoordinates.Bottom - mode.DesktopCoordinates.Top;
            }

            Form = EngineServiceFactory.Instance<IEngineForm>();

            Form.Initialize(name, screenWidth, screenHeight, isFullScreen);

            Form.ResizeBegin += (sender, e) =>
            {
                paused = true;
                GameTime.Pause();
            };
            Form.ResizeEnd += (sender, e) =>
            {
                paused = false;
                GameTime.Resume();
                if (Form.SizeUpdated)
                {
                    OnResize();
                }
            };
            Form.Activated += (sender, e) =>
            {
                paused = false;
                GameTime.Resume();
            };
            Form.Deactivate += (sender, e) =>
            {
                paused = true;
                GameTime.Pause();
            };
            Form.Resize += (sender, e) =>
            {
                if (Form.Resizing)
                {
                    return;
                }

                if (Form.IsMinimized)
                {
                    paused = true;
                    GameTime.Pause();
                    return;
                }

                paused = false;
                GameTime.Resume();

                if (!Form.FormModeUpdated)
                {
                    return;
                }

                OnResize();
            };

            #endregion

            Input = EngineServiceFactory.Instance<IInput>();
            Input.SetForm(Form);

            Graphics = new Graphics(Form, vsyncEnabled, refreshRate, multiSampling);

            BuiltIn.BuiltInShaders.Initialize(Graphics);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Game()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove scene reference
                nextScene = null;

                if (scenes != null)
                {
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        scenes[i]?.Dispose();
                    }

                    scenes.Clear();
                }

                BuiltIn.BuiltInShaders.DisposeResources();

                FontMapCache.Clear();

                Input?.Dispose();
                Input = null;

                Form?.Dispose();
                Form = null;

                ResourceManager?.Dispose();
                ResourceManager = null;

                BufferManager?.Dispose();
                BufferManager = null;

                Graphics?.Dispose();
                Graphics = null;
            }
        }

        /// <summary>
        /// Begins render loop
        /// </summary>
        public void Run()
        {
            Logger.WriteInformation(this, LogLineString);
            Logger.WriteInformation(this, "** Game started                                                         **");
            Logger.WriteInformation(this, LogLineString);

#if DEBUG
            Form.Render(() =>
            {
                try
                {
                    Frame();
                }
                catch (System.Runtime.InteropServices.SEHException ex)
                {
                    Logger.WriteError(this, ex);

                    if (Graphics.GetRemovedDeviceStatus(out string removedReason))
                    {
                        Logger.WriteError(this, $"{nameof(Frame)}: Device Removed: {removedReason}", ex);
                    }

                    string deviceErrors = Graphics.GetDebugInfo();
                    if (!string.IsNullOrWhiteSpace(deviceErrors))
                    {
                        Logger.WriteError(this, $"{nameof(Frame)}: {deviceErrors}", ex);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(this, ex);

                    throw;
                }
            });
#else
            Form.Render(Frame);
#endif
        }

        /// <summary>
        /// On render form resize
        /// </summary>
        private void OnResize()
        {
            Graphics?.PrepareDevice(Form.RenderWidth, Form.RenderHeight, true);
        }

        /// <summary>
        /// Creates a new scene and sets it as the unique active scene
        /// </summary>
        /// <typeparam name="T">Type of scene</typeparam>
        /// <param name="sceneMode">Scene mode</param>
        /// <remarks>Current scenes will be removed from internal scene collection</remarks>
        public void SetScene<T>(SceneModes sceneMode = SceneModes.ForwardLigthning) where T : Scene
        {
            T scene = null;
            try
            {
                Logger.WriteInformation(this, "Game: Setting scene with the default constructor");

                scene = (T)Activator.CreateInstance(typeof(T), this);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Game: Error setting scene: {ex.Message}", ex);
            }

            SetScene(scene, sceneMode);
        }
        /// <summary>
        /// Creates a new scene and sets it as the unique active scene
        /// </summary>
        /// <param name="sceneType">Type of scene</param>
        /// <param name="sceneMode">Scene mode</param>
        /// <remarks>Current scenes will be removed from internal scene collection</remarks>
        public void SetScene(Type sceneType, SceneModes sceneMode = SceneModes.ForwardLigthning)
        {
            bool isScene = typeof(Scene).IsAssignableFrom(sceneType);
            if (!isScene)
            {
                throw new ArgumentException($"The {nameof(sceneType)} argument must implement {typeof(Scene)}.", nameof(sceneType));
            }

            Scene scene = null;
            try
            {
                Logger.WriteInformation(this, "Game: Setting scene with the default constructor");

                scene = (Scene)Activator.CreateInstance(sceneType, this);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Game: Error setting scene: {ex.Message}", ex);
            }

            SetScene(scene, sceneMode);
        }
        /// <summary>
        /// Sets the specified scene to next scene to load
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="sceneMode">Scene mode</param>
        public void SetScene(Scene scene, SceneModes sceneMode = SceneModes.ForwardLigthning)
        {
            if (scene == null)
            {
                return;
            }

            scene.SetRenderMode(sceneMode);
            scene.Order = 1;
            nextScene = scene;
        }
        /// <summary>
        /// Unloads the current scenes and loads the specified scene
        /// </summary>
        /// <param name="sceneToLoad">Scene to load</param>
        private void ChangeScene(Scene sceneToLoad)
        {
            //Deactivate scenes
            scenes.ForEach(s => s.Active = false);

            //Copy collection for disposing
            List<Scene> toDispose = new(scenes);

            //Clear scene collection
            scenes.Clear();

            toDispose.ForEach(s => s.Dispose());
            toDispose.Clear();

            StartScene(sceneToLoad);

            scenes.Add(sceneToLoad);
            scenes.Sort((p1, p2) => p2.Order.CompareTo(p1.Order));
        }
        /// <summary>
        /// Adds a scene to the internal scene collection
        /// </summary>
        /// <param name="scene">New scene</param>
        private void StartScene(Scene scene)
        {
            Logger.WriteInformation(this, "Game: Begin StartScene");

            try
            {
                scene.Active = false;

                Logger.WriteInformation(this, "Scene: Initialize start");
                scene.Initialize();
                Logger.WriteInformation(this, "Scene: Initialize end");

                scene.Active = true;
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Scene: Initialize error: {ex.Message}", ex);

                throw;
            }

            Logger.WriteInformation(this, "Game: End StartScene");
        }
        /// <summary>
        /// Gets the active scene
        /// </summary>
        public Scene GetActiveScene()
        {
            return scenes.Find(s => s.Active);
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="taskGroup">Resource load tasks</param>
        public void LoadResources(ILoadResourceGroup taskGroup)
        {
            if (taskGroup == null)
            {
                return;
            }

            taskGroup.Id ??= $"{GetLoadResourceId()}_{taskGroup.GetType()}";

            resourceRequests.Enqueue(taskGroup);
        }

        /// <summary>
        /// Integrates the requested resources into the resource manager
        /// </summary>
        private void IntegrateResources()
        {
            if (integratingResources)
            {
                return;
            }

            if (resourceRequests.IsEmpty && !ResourceManager.HasRequests)
            {
                return;
            }

            integratingResources = true;

            var groups = Task.Run(IntegrateResourcesAsync).ConfigureAwait(false).GetAwaiter().GetResult();
            foreach (var gr in groups)
            {
                gr.End();
            }
        }
        /// <summary>
        /// Integrates the resource groups in the resource queue
        /// </summary>
        /// <returns>Returns the integrated resource groups</returns>
        private async Task<ILoadResourceGroup[]> IntegrateResourcesAsync()
        {
            List<ILoadResourceGroup> res = [];

            try
            {
                while (resourceRequests.TryDequeue(out var loadResourceGroup))
                {
                    string logText = $"{nameof(Game)}.{nameof(IntegrateResources)}.{loadResourceGroup.Id ?? NoIdString}";

                    await loadResourceGroup.Process();

                    Logger.WriteInformation(this, $"{logText} => BufferManager: Recreating buffers");
                    BufferManager.CreateBuffers(loadResourceGroup.Id);
                    Logger.WriteInformation(this, $"{logText} => BufferManager: Buffers recreated");

                    Logger.WriteInformation(this, $"{logText} => ResourceManager: Creating new resources");
                    ResourceManager.CreateResources(loadResourceGroup.Id, null);
                    Logger.WriteInformation(this, $"{logText} => ResourceManager: New resources created");

                    res.Add(loadResourceGroup);
                }

                if (ResourceManager.HasRequests)
                {
                    Logger.WriteInformation(this, "ResourceManager: Creating new resources");
                    ResourceManager.CreateResources($"ResourceManager.Frame{FrameCounters.FrameCount}", null);
                    Logger.WriteInformation(this, "ResourceManager: New resources created");
                }
            }
            finally
            {
                integratingResources = false;
            }

            return [.. res];
        }

        /// <summary>
        /// Close game
        /// </summary>
        public void Exit()
        {
            exiting = true;
        }

        /// <summary>
        /// Per frame logic
        /// </summary>
        private void Frame()
        {
            if (exiting)
            {
                Logger.WriteInformation(this, LogLineString);
                Logger.WriteInformation(this, "** Game closed                                                          **");
                Logger.WriteInformation(this, LogLineString);

                //Exit form
                Form.Close();

                return;
            }

            if (paused)
            {
                return;
            }

            GameTime.Update();

            if (nextScene != null)
            {
                ChangeScene(nextScene);
                nextScene = null;

                return;
            }

            var activeScene = GetActiveScene();
            if (activeScene == null)
            {
                return;
            }

            FrameCounters.FrameCount++;

            Logger.WriteInformation(this, $"##### Frame {FrameCounters.FrameCount} Start ####");

            Stopwatch gSW = new();
            gSW.Start();

            FrameInput();

            FrameSceneUpdate(activeScene);

            FrameSceneDraw(activeScene);

            FramePresent();

            gSW.Stop();
            GameStatus.Add("TOTAL", gSW);

            LogLevel level = EvaluateTime(gSW.ElapsedMilliseconds);
            Logger.Write(level, this, $"##### Frame {FrameCounters.FrameCount} End - {gSW.ElapsedMilliseconds} milliseconds ####");

            Task.Run(IntegrateResources).ConfigureAwait(false);

            FrameCounters.FramesPerSecond++;
            FrameCounters.FrameTime += GameTime.ElapsedSeconds;

            if (FrameCounters.FrameTime >= 1.0f)
            {
                FrameRefreshCounters();
            }

            if (CollectGameStatus)
            {
                FrameCollectGameStatus();
            }

            GameStatus.Clear();

            FrameCounters.ClearFrame();
        }
        /// <summary>
        /// Update input
        /// </summary>
        private void FrameInput()
        {
            var iSW = Stopwatch.StartNew();
            Input.Update(GameTime);
            iSW.Stop();
            GameStatus.Add(nameof(FrameInput), iSW);
        }
        /// <summary>
        /// Update scene state
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameSceneUpdate(Scene scene)
        {
            var uSW = Stopwatch.StartNew();
            scene.Update(GameTime);
            uSW.Stop();
            GameStatus.Add(nameof(FrameSceneUpdate), uSW);
        }
        /// <summary>
        /// Draw scene
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameSceneDraw(Scene scene)
        {
            var dSW = Stopwatch.StartNew();
            scene.Draw(GameTime);
            dSW.Stop();
            GameStatus.Add(nameof(FrameSceneDraw), dSW);
        }
        /// <summary>
        /// Present frame
        /// </summary>
        private void FramePresent()
        {
            var pSW = Stopwatch.StartNew();
            Graphics.Present();
            pSW.Stop();
            GameStatus.Add(nameof(FramePresent), pSW);
        }
        /// <summary>
        /// Refreshes frame counters
        /// </summary>
        private void FrameRefreshCounters()
        {
            RuntimeText = GetRuntimeText();

#if DEBUG
            Form.Text = RuntimeText;
#endif
            FrameCounters.FramesPerSecond = 0;
            FrameCounters.FrameTime = 0f;
        }
        /// <summary>
        /// Collects frame status
        /// </summary>
        private void FrameCollectGameStatus()
        {
            GameStatusCollected?.Invoke(this, new()
            {
                Trace = GameStatus.Copy(),
            });

            CollectGameStatus = false;
        }

        /// <summary>
        /// Gets the runtime text with frame counters data
        /// </summary>
        public string GetRuntimeText()
        {
            var counters = FrameCounters.GetFrameCounters(-1);

            return string.Format(
                "{0} - {1} - Frame {2} FPS: {3:000} Updates: {4:00} {5} F. Time: {6:0.0000} (secs) T. Time: {7:0000} (secs)",
                Graphics.DeviceDescription,
                GetActiveScene()?.GetRenderMode() ?? SceneModes.Unknown,
                FrameCounters.FrameCount,
                FrameCounters.FramesPerSecond,
                FrameCounters.PickCounters.TransformUpdatesPerFrame,
                counters,
                GameTime.ElapsedSeconds,
                GameTime.TotalSeconds);
        }
    }
}
