using SharpDX;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.UI;

    /// <summary>
    /// Game class
    /// </summary>
    public class Game : IDisposable
    {
        /// <summary>
        /// Scene list
        /// </summary>
        private List<Scene> scenes = new List<Scene>();
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
        public EngineForm Form { get; private set; }
        /// <summary>
        /// Game time
        /// </summary>
        public GameTime GameTime { get; private set; }
        /// <summary>
        /// CPU stats
        /// </summary>
        public PerformanceCounter CPUStats { get; private set; }
        /// <summary>
        /// Input helper
        /// </summary>
        public Input Input { get; private set; }
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
        /// Progress reporter
        /// </summary>
        public readonly IProgress<LoadResourceProgress> Progress;
        /// <summary>
        /// Buffer progress reporter
        /// </summary>
        public readonly IProgress<LoadResourceProgress> ProgressBuffers;
        /// <summary>
        /// Gets wheter a resource loading is running
        /// </summary>
        public bool ResourceLoadRuning { get; private set; } = false;
        /// <summary>
        /// Game status
        /// </summary>
        public readonly GameStatus GameStatus = new GameStatus();

        /// <summary>
        /// Game status collected event
        /// </summary>
        public event GameStatusCollectedHandler GameStatusCollected;
        /// <summary>
        /// Fires when a resource load starts
        /// </summary>
        public event GameLoadResourcesEventHandler ResourcesLoading;
        /// <summary>
        /// Fires when a resource load ends
        /// </summary>
        public event GameLoadResourcesEventHandler ResourcesLoaded;

        /// <summary>
        /// Gets desktop mode description
        /// </summary>
        /// <returns>Returns current desktop mode description</returns>
        private static OutputDescription1 GetDesktopMode()
        {
            using (var factory = new Factory1())
            using (var factory5 = factory.QueryInterface<Factory5>())
            {
                using (var adapter = factory5.GetAdapter1(0))
                using (var adapter4 = adapter.QueryInterface<Adapter4>())
                {
                    using (var adapterOutput = adapter4.GetOutput(0))
                    using (var adapterOutput6 = adapterOutput.QueryInterface<Output6>())
                    {
                        return adapterOutput6.Description1;
                    }
                }
            }
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

            Progress = new Progress<LoadResourceProgress>(ReportProgress);
            ProgressBuffers = new Progress<LoadResourceProgress>(ReportProgressBuffers);

            BufferManager = new BufferManager(this);

            ResourceManager = new GameResourceManager(this);

            CPUStats = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            #region Form

            bool isFullScreen = fullScreen;
            if (screenWidth == 0 || screenHeight == 0)
            {
                var mode = GetDesktopMode();

                isFullScreen = true;
                screenWidth = mode.DesktopCoordinates.Right - mode.DesktopCoordinates.Left;
                screenHeight = mode.DesktopCoordinates.Bottom - mode.DesktopCoordinates.Top;
            }

            Form = new EngineForm(name, screenWidth, screenHeight, isFullScreen);

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

            Input = new Input(Form);

            Graphics = new Graphics(Form, vsyncEnabled, refreshRate, multiSampling);

            DrawerPool.Initialize(Graphics);
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
                    scenes = null;
                }

                DrawerPool.DisposeResources();

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
            Logger.WriteInformation(this, "**************************************************************************");
            Logger.WriteInformation(this, "** Game started                                                         **");
            Logger.WriteInformation(this, "**************************************************************************");

            RenderLoop.Run(Form, Frame);
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
        /// <remarks>Current scenes will be removed from internal scene collection</remarks>
        public void SetScene<T>(SceneModes sceneMode = SceneModes.ForwardLigthning) where T : Scene
        {
            T scene = null;
            try
            {
                Logger.WriteInformation(this, "Game: Setting scene with the default constructor");

                scene = (T)Activator.CreateInstance(typeof(T), new object[] { this });
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
            List<Scene> toDispose = new List<Scene>(scenes);

            //Clear scene collection
            scenes.Clear();

            toDispose.ForEach(s => s.Dispose());
            toDispose.Clear();

            Task.WhenAll(StartScene(sceneToLoad));

            scenes.Add(sceneToLoad);
            scenes.Sort(
                delegate (Scene p1, Scene p2)
                {
                    return p2.Order.CompareTo(p1.Order);
                });
        }
        /// <summary>
        /// Adds a scene to the internal scene collection
        /// </summary>
        /// <param name="scene">New scene</param>
        private async Task StartScene(Scene scene)
        {
            // Start background thread
            await Task.Delay(1).ConfigureAwait(false);

            Logger.WriteInformation(this, "Game: Begin StartScene");

            try
            {
                scene.Active = false;

                Logger.WriteInformation(this, "Scene: Initialize start");
                await scene.Initialize();
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
        /// Report progress callback
        /// </summary>
        /// <param name="value">Progress value from 0.0f to 1.0f</param>
        public void ReportProgress(LoadResourceProgress value)
        {
            var activeScene = scenes.FirstOrDefault(s => s.Active);
            activeScene?.OnReportProgress(value);
        }
        /// <summary>
        /// Report buffer progress callback
        /// </summary>
        /// <param name="value">Progress value from 0.0f to 1.0f</param>
        public void ReportProgressBuffers(LoadResourceProgress value)
        {
            var activeScene = scenes.FirstOrDefault(s => s.Active);
            activeScene?.OnReportProgressBuffers(value);
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal Task LoadResourcesAsync<T>(Scene scene, LoadResourceGroup<T> taskGroup)
        {
            return Task.Run(async () =>
            {
                LoadResourcesResult<T> result = null;

                while (true)
                {
                    if (ResourceLoadRuning)
                    {
                        await Task.Delay(100);

                        continue;
                    }

                    ResourceLoadRuning = true;
                    try
                    {
                        result = await InternalLoadResourcesAsync(scene, taskGroup);

                        break;
                    }
                    finally
                    {
                        ResourceLoadRuning = false;
                    }
                }

                if (result != null)
                {
                    result.ThrowExceptions();
                }
            });
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal Task LoadResourcesAsync<T>(Scene scene, LoadResourceGroup<T> taskGroup, Action<LoadResourcesResult<T>> callback)
        {
            return Task.Run(async () =>
            {
                LoadResourcesResult<T> result = null;

                while (true)
                {
                    if (ResourceLoadRuning)
                    {
                        await Task.Delay(100);

                        continue;
                    }

                    ResourceLoadRuning = true;
                    try
                    {
                        result = await InternalLoadResourcesAsync(scene, taskGroup);

                        break;
                    }
                    finally
                    {
                        ResourceLoadRuning = false;
                    }
                }

                if (result != null)
                {
                    result.ThrowExceptions();

                    callback?.Invoke(result);
                }
            });
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal Task LoadResourcesAsync<T>(Scene scene, LoadResourceGroup<T> taskGroup, Func<LoadResourcesResult<T>, Task> callback)
        {
            return Task.Run(async () =>
            {
                LoadResourcesResult<T> result = null;

                while (true)
                {
                    if (ResourceLoadRuning)
                    {
                        await Task.Delay(100);

                        continue;
                    }

                    ResourceLoadRuning = true;
                    try
                    {
                        result = await InternalLoadResourcesAsync(scene, taskGroup);

                        break;
                    }
                    finally
                    {
                        ResourceLoadRuning = false;
                    }
                }

                if (result != null)
                {
                    result.ThrowExceptions();

                    await callback?.Invoke(result);
                }
            });
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal Task LoadResourcesAsync(Scene scene, LoadResourceGroup taskGroup)
        {
            return Task.Run(async () =>
            {
                LoadResourcesResult result = null;

                while (true)
                {
                    if (ResourceLoadRuning)
                    {
                        await Task.Delay(100);

                        continue;
                    }

                    ResourceLoadRuning = true;
                    try
                    {
                        result = await InternalLoadResourcesAsync(scene, taskGroup);

                        break;
                    }
                    finally
                    {
                        ResourceLoadRuning = false;
                    }
                }

                if (result != null)
                {
                    result.ThrowExceptions();
                }
            });
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal Task LoadResourcesAsync(Scene scene, LoadResourceGroup taskGroup, Action<LoadResourcesResult> callback)
        {
            return Task.Run(async () =>
            {
                LoadResourcesResult result = null;

                while (true)
                {
                    if (ResourceLoadRuning)
                    {
                        await Task.Delay(100);

                        continue;
                    }

                    ResourceLoadRuning = true;
                    try
                    {
                        result = await InternalLoadResourcesAsync(scene, taskGroup);

                        break;
                    }
                    finally
                    {
                        ResourceLoadRuning = false;
                    }
                }

                if (result != null)
                {
                    result.ThrowExceptions();

                    callback?.Invoke(result);
                }
            });
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal Task LoadResourcesAsync(Scene scene, LoadResourceGroup taskGroup, Func<LoadResourcesResult, Task> callback)
        {
            Task.Run(async () =>
            {
                LoadResourcesResult result = null;

                while (true)
                {
                    if (ResourceLoadRuning)
                    {
                        await Task.Delay(100);

                        continue;
                    }

                    ResourceLoadRuning = true;
                    try
                    {
                        result = await InternalLoadResourcesAsync(scene, taskGroup);

                        break;
                    }
                    finally
                    {
                        ResourceLoadRuning = false;
                    }
                }

                if (result != null)
                {
                    result.ThrowExceptions();

                    await callback?.Invoke(result);
                }
            });

            return Task.CompletedTask;
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <returns>Returns a load resource result.</returns>
        private async Task<LoadResourcesResult<T>> InternalLoadResourcesAsync<T>(Scene scene, LoadResourceGroup<T> taskGroup)
        {
            List<TaskResult<T>> loadResult = new List<TaskResult<T>>();

            var taskList = taskGroup.Tasks.ToList();

            int totalTasks = taskList.Count;
            int currentTask = 0;
            while (taskList.Any())
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                bool completedOk = t.Status == TaskStatus.RanToCompletion;

                TaskResult<T> res = new TaskResult<T>
                {
                    Completed = completedOk,
                    Exception = t.Exception, // Store the excetion
                    Result = completedOk ? (await t) : default, // Avoid throwing the exception now
                };

                loadResult.Add(res);

                Progress?.Report(new LoadResourceProgress { Id = taskGroup.Id, Progress = ++currentTask / (float)totalTasks });
            }

            await IntegrateResources(taskGroup.Id, scene);

            return new LoadResourcesResult<T>
            {
                Results = loadResult,
            };
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="taskGroup">Resource load tasks</param>
        /// <returns>Returns a load resource result.</returns>
        private async Task<LoadResourcesResult> InternalLoadResourcesAsync(Scene scene, LoadResourceGroup taskGroup)
        {
            List<TaskResult> loadResult = new List<TaskResult>();

            var taskList = taskGroup.Tasks.ToList();

            int totalTasks = taskList.Count;
            int currentTask = 0;
            while (taskList.Any())
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                TaskResult res = new TaskResult
                {
                    Completed = t.Status == TaskStatus.RanToCompletion,
                    Exception = t.Exception, // Store the excetion
                };

                loadResult.Add(res);

                Progress?.Report(new LoadResourceProgress { Id = taskGroup.Id, Progress = ++currentTask / (float)totalTasks });
            }

            await IntegrateResources(taskGroup.Id, scene);

            return new LoadResourcesResult
            {
                Results = loadResult,
            };
        }

        /// <summary>
        /// Integrates the requested resources into the resource manager
        /// </summary>
        /// <param name="scene">Scene</param>
        private async Task IntegrateResources(string id, Scene scene)
        {
            try
            {
                ResourcesLoading?.Invoke(this, new GameLoadResourcesEventArgs() { Id = id, Scene = scene });

                Logger.WriteInformation(this, "BufferManager: Recreating buffers");
                await BufferManager.CreateBuffersAsync(id, ProgressBuffers);
                Logger.WriteInformation(this, "BufferManager: Buffers recreated");

                Logger.WriteInformation(this, "ResourceManager: Creating new resources");
                ResourceManager.CreateResources(id, ProgressBuffers);
                Logger.WriteInformation(this, "ResourceManager: New resources created");

                ResourcesLoaded?.Invoke(this, new GameLoadResourcesEventArgs() { Id = id, Scene = scene });
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Scene: {nameof(IntegrateResources)} error: {ex.Message}", ex);

                throw;
            }
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
                Logger.WriteInformation(this, "**************************************************************************");
                Logger.WriteInformation(this, "** Game closed                                                          **");
                Logger.WriteInformation(this, "**************************************************************************");

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

            var activeScene = scenes.FirstOrDefault(s => s.Active);
            if (activeScene == null)
            {
                return;
            }

            Counters.FrameCount++;

            Logger.WriteInformation(this, $"##### Frame {Counters.FrameCount} Start ####");

            Stopwatch gSW = new Stopwatch();
            gSW.Start();

            FrameInput();

            FrameBegin(activeScene);

            FrameSceneUpdate(activeScene);

            FrameSceneDraw(activeScene);

            FrameEnd();

            gSW.Stop();
            GameStatus.Add("TOTAL", gSW);

            LogLevel level = EvaluateTime(gSW.ElapsedMilliseconds);
            Logger.Write(level, this, $"##### Frame {Counters.FrameCount} End - {gSW.ElapsedMilliseconds} milliseconds ####");

            if (ResourceManager.HasRequests)
            {
                Logger.WriteInformation(this, "ResourceManager: Creating new resources");
                ResourceManager.CreateResources($"ResourceManager.Frame{Counters.FrameCount}", null);
                Logger.WriteInformation(this, "ResourceManager: New resources created");
            }

            Counters.FramesPerSecond++;
            Counters.FrameTime += GameTime.ElapsedSeconds;

            if (Counters.FrameTime >= 1.0f)
            {
                FrameRefreshCounters();
            }

            if (CollectGameStatus)
            {
                FrameCollectGameStatus();
            }

            GameStatus.Clear();

            Counters.ClearFrame();
        }
        /// <summary>
        /// Update input
        /// </summary>
        private void FrameInput()
        {
            try
            {
                Stopwatch pSW = new Stopwatch();
                pSW.Start();
                Input.Update(GameTime);
                pSW.Stop();
                GameStatus.Add("Input", pSW);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Frame: Input Update error: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Begin frame
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameBegin(Scene scene)
        {
            try
            {
                Stopwatch pSW = new Stopwatch();
                pSW.Start();
                Graphics.Begin(scene);
                pSW.Stop();
                GameStatus.Add("Begin", pSW);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Frame: Graphics Begin error: {ex.Message}", ex);

                throw;
            }
        }
        /// <summary>
        /// Update scene state
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameSceneUpdate(Scene scene)
        {
            try
            {
                Stopwatch uSW = new Stopwatch();
                uSW.Start();
                scene.Update(GameTime);
                uSW.Stop();
                GameStatus.Add($"Scene {scene}.Update", uSW);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Scene: Update error: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Draw scene
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameSceneDraw(Scene scene)
        {
            if (!BufferManager.SetVertexBuffers())
            {
                return;
            }

            try
            {
                Stopwatch dSW = new Stopwatch();
                dSW.Start();
                scene.Draw(GameTime);
                dSW.Stop();
                GameStatus.Add($"Scene {scene}.Draw", dSW);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Scene: Draw error: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// End frame
        /// </summary>
        private void FrameEnd()
        {
            try
            {
                Stopwatch pSW = new Stopwatch();
                pSW.Start();
                Graphics.End();
                pSW.Stop();
                GameStatus.Add("End", pSW);
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Frame: Graphics End error: {ex.Message}", ex);

                throw;
            }
        }
        /// <summary>
        /// Refreshes frame counters
        /// </summary>
        private void FrameRefreshCounters()
        {
            try
            {
                RuntimeText = string.Format(
                    "{0} - {1} - Frame {2} FPS: {3:000} Draw C/D: {4:00}:{5:00} Inst: {6:00} U: {7:00} S: {8}:{9}:{10} F. Time: {11:0.0000} (secs) T. Time: {12:0000} (secs) CPU: {13:0.00}%",
                    Graphics.DeviceDescription,
                    Name,
                    Counters.FrameCount,
                    Counters.FramesPerSecond,
                    Counters.DrawCallsPerFrame,
                    Counters.InstancesPerFrame,
                    Counters.MaxInstancesPerFrame,
                    Counters.UpdatesPerFrame,
                    Counters.RasterizerStateChanges, Counters.BlendStateChanges, Counters.DepthStencilStateChanges,
                    GameTime.ElapsedSeconds,
                    GameTime.TotalSeconds,
                    CPUStats.NextValue());
#if DEBUG
                Form.Text = RuntimeText;
#endif
                Counters.FramesPerSecond = 0;
                Counters.FrameTime = 0f;
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Frame: Refresh Counters error: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Collects frame status
        /// </summary>
        private void FrameCollectGameStatus()
        {
            try
            {
                GameStatusCollectedEventArgs e = new GameStatusCollectedEventArgs()
                {
                    Trace = GameStatus.Copy(),
                };

                GameStatusCollected?.Invoke(this, e);

                CollectGameStatus = false;
            }
            catch (Exception ex)
            {
                Logger.WriteError(this, $"Frame: Collecto Game Status error: {ex.Message}", ex);
            }
        }
    }
}
