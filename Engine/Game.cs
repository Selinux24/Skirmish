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
                return this.scenes.Count;
            }
        }
        /// <summary>
        /// Number of active scenes
        /// </summary>
        public int ActiveScenesCount
        {
            get
            {
                return this.scenes.FindAll(s => s.Active).Count;
            }
        }
        /// <summary>
        /// Gets or sets if the cursor is visible
        /// </summary>
        public bool VisibleMouse
        {
            get
            {
                if (this.Input != null)
                {
                    return this.Input.VisibleMouse;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (this.Input != null)
                {
                    this.Input.VisibleMouse = value;
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
                if (this.Input != null)
                {
                    return this.Input.LockMouse;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (this.Input != null)
                {
                    this.Input.LockMouse = value;
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
        public readonly IProgress<float> Progress;
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
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="screenWidth">Window width</param>
        /// <param name="screenHeight">Window height</param>
        /// <param name="fullScreen">Full screen window</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampling">Enable multi-sampling</param>
        public Game(string name, bool fullScreen = true, int screenWidth = 0, int screenHeight = 0, bool vsyncEnabled = true, int refreshRate = 0, int multiSampling = 0)
        {
            this.Name = name;

            this.GameTime = new GameTime();

            this.Progress = new Progress<float>(ReportProgress);

            this.BufferManager = new BufferManager(this);

            this.ResourceManager = new GameResourceManager(this);

            this.CPUStats = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            #region Form

            if (screenWidth == 0 || screenHeight == 0)
            {
                var mode = GetDesktopMode();

                screenWidth = mode.DesktopCoordinates.Right - mode.DesktopCoordinates.Left;
                screenHeight = mode.DesktopCoordinates.Bottom - mode.DesktopCoordinates.Top;
            }

            this.Form = new EngineForm(name, screenWidth, screenHeight, fullScreen);

            this.Form.UserResized += (sender, eventArgs) =>
            {
                if (this.Graphics != null)
                {
                    this.Graphics.PrepareDevice(this.Form.RenderWidth, this.Form.RenderHeight, true);
                }
            };

            #endregion

            this.Input = new Input(this.Form);

            this.Graphics = new Graphics(this.Form, vsyncEnabled, refreshRate, multiSampling);

            DrawerPool.Initialize(this.Graphics);
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
                this.nextScene = null;

                if (this.scenes != null)
                {
                    for (int i = 0; i < this.scenes.Count; i++)
                    {
                        this.scenes[i]?.Dispose();
                    }

                    this.scenes.Clear();
                    this.scenes = null;
                }

                DrawerPool.DisposeResources();

                FontMap.ClearCache();

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
            RenderLoop.Run(this.Form, this.Frame);
        }

        /// <summary>
        /// Creates a new scene and sets it as the unique active scene
        /// </summary>
        /// <typeparam name="T">Type of scene</typeparam>
        /// <remarks>Current scenes will be removed from internal scene collection</remarks>
        public void SetScene<T>(SceneModes sceneMode = SceneModes.ForwardLigthning) where T : Scene
        {
            T scene;

            try
            {
                scene = (T)Activator.CreateInstance(typeof(T), new object[] { this, sceneMode });
            }
            catch
            {
                scene = (T)Activator.CreateInstance(typeof(T), new object[] { this });
            }

            scene.Order = 1;

            this.nextScene = scene;
        }
        /// <summary>
        /// Unloads the current scenes and loads the specified scene
        /// </summary>
        /// <param name="sceneToLoad">Scene to load</param>
        private void ChangeScene(Scene sceneToLoad)
        {
            //Deactivate scenes
            this.scenes.ForEach(s => s.Active = false);

            //Copy collection for disposing
            List<Scene> toDispose = new List<Scene>(this.scenes);

            //Clear scene collection
            this.scenes.Clear();

            toDispose.ForEach(s => s.Dispose());
            toDispose.Clear();

            Task.WhenAll(this.StartScene(sceneToLoad));

            this.scenes.Add(sceneToLoad);
            this.scenes.Sort(
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
            Console.WriteLine("Game: Begin StartScene");
            scene.Active = false;

            Console.WriteLine("Scene: Initialize start");
            await scene.Initialize();
            Console.WriteLine("Scene: Initialize end");

            scene.Active = true;
            Console.WriteLine("Game: End StartScene");
        }

        /// <summary>
        /// Report progress callback
        /// </summary>
        /// <param name="value">Progress value from 0.0f to 1.0f</param>
        public void ReportProgress(float value)
        {
            var activeScene = scenes.FirstOrDefault(s => s.Active);
            activeScene?.OnReportProgress(value);
        }

        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="tasks">Resource load tasks</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal bool LoadResources(Scene scene, params Task[] tasks)
        {
            if (ResourceLoadRuning)
            {
                return false;
            }

            Task.WhenAll(LoadResourcesAsync(scene, tasks));

            return true;
        }

        /// <summary>
        /// Executes a resource load task
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="task">Resource load task</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal async Task<bool> LoadResourcesAsync<T>(Scene scene, Task<T> task, Action<T> callback = null)
        {
            return await LoadResourcesAsync(scene, new[] { task }, (results) => { callback?.Invoke(results.FirstOrDefault()); });
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="tasks">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal async Task<bool> LoadResourcesAsync<T>(Scene scene, IEnumerable<Task<T>> tasks, Action<IEnumerable<T>> callback = null)
        {
            if (ResourceLoadRuning)
            {
                return false;
            }

            ResourceLoadRuning = true;

            List<T> taskResults = new List<T>();

            try
            {
                var taskList = tasks.ToList();

                int totalTasks = taskList.Count;
                int currentTask = 0;
                while (taskList.Any())
                {
                    var t = await Task.WhenAny(taskList);

                    taskList.Remove(t);

                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"Faulted task {t.Exception}");
                    }

                    taskResults.Add(await t);

                    Progress?.Report(++currentTask / (float)totalTasks);
                }

                ResourcesLoading?.Invoke(this, new GameLoadResourcesEventArgs() { Scene = scene });

                Console.WriteLine("BufferManager: Recreating buffers");
                this.BufferManager.CreateBuffers(Progress);
                Console.WriteLine("BufferManager: Buffers recreated");

                Console.WriteLine("ResourceManager: Creating new resources");
                this.ResourceManager.CreateResources(Progress);
                Console.WriteLine("ResourceManager: New resources created");

                ResourcesLoaded?.Invoke(this, new GameLoadResourcesEventArgs() { Scene = scene });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ResourceLoadRuning = false;

                callback?.Invoke(taskResults.ToArray());
            }

            return true;
        }
        /// <summary>
        /// Executes a resource load task
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="task">Resource load task</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal async Task<bool> LoadResourcesAsync(Scene scene, Task task, Action callback = null)
        {
            return await LoadResourcesAsync(scene, new[] { task }, callback);
        }
        /// <summary>
        /// Executes a list of resource load tasks
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="tasks">Resource load tasks</param>
        /// <param name="callback">Callback</param>
        /// <returns>Returns true when the load executes. When another load task is running, returns false.</returns>
        internal async Task<bool> LoadResourcesAsync(Scene scene, IEnumerable<Task> tasks, Action callback = null)
        {
            if (ResourceLoadRuning)
            {
                return false;
            }

            ResourceLoadRuning = true;

            try
            {
                var taskList = tasks.ToList();

                int totalTasks = taskList.Count;
                int currentTask = 0;
                while (taskList.Any())
                {
                    var t = await Task.WhenAny(taskList);

                    taskList.Remove(t);

                    Progress?.Report(++currentTask / (float)totalTasks);
                }

                ResourcesLoading?.Invoke(this, new GameLoadResourcesEventArgs() { Scene = scene });

                Console.WriteLine("BufferManager: Recreating buffers");
                this.BufferManager.CreateBuffers(Progress);
                Console.WriteLine("BufferManager: Buffers recreated");

                Console.WriteLine("ResourceManager: Creating new resources");
                this.ResourceManager.CreateResources(Progress);
                Console.WriteLine("ResourceManager: New resources created");

                ResourcesLoaded?.Invoke(this, new GameLoadResourcesEventArgs() { Scene = scene });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                ResourceLoadRuning = false;

                callback?.Invoke();
            }

            return true;
        }

        /// <summary>
        /// Close game
        /// </summary>
        public void Exit()
        {
            this.exiting = true;
        }

        /// <summary>
        /// Per frame logic
        /// </summary>
        private void Frame()
        {
            if (this.exiting)
            {
                return;
            }

            this.GameTime.Update();

            if (this.nextScene != null)
            {
                this.ChangeScene(this.nextScene);
                this.nextScene = null;

                return;
            }

            var activeScene = scenes.FirstOrDefault(s => s.Active);
            if (activeScene == null)
            {
                return;
            }

            Stopwatch gSW = new Stopwatch();
            gSW.Start();

            FrameInput();

            FrameBegin();

            FrameSceneUpdate(activeScene);

            FrameSceneDraw(activeScene);

            FrameEnd();

            gSW.Stop();
            GameStatus.Add("TOTAL", gSW);

            if (this.ResourceManager.HasRequests)
            {
                Console.WriteLine("ResourceManager: Creating new resources");
                this.ResourceManager.CreateResources(null);
                Console.WriteLine("ResourceManager: New resources created");
            }

            Counters.FrameCount++;
            Counters.FrameTime += this.GameTime.ElapsedSeconds;

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

            if (this.exiting)
            {
                //Exit form
                this.Form.Close();
            }
        }
        /// <summary>
        /// Update input
        /// </summary>
        private void FrameInput()
        {
            Stopwatch pSW = new Stopwatch();
            pSW.Start();
            this.Input.Update(this.GameTime);
            pSW.Stop();
            GameStatus.Add("Input", pSW);
        }
        /// <summary>
        /// Begin frame
        /// </summary>
        private void FrameBegin()
        {
            Stopwatch pSW = new Stopwatch();
            pSW.Start();
            this.Graphics.Begin();
            pSW.Stop();
            GameStatus.Add("Begin", pSW);
        }
        /// <summary>
        /// Update scene state
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameSceneUpdate(Scene scene)
        {
            Stopwatch uSW = new Stopwatch();
            uSW.Start();
            scene.Update(this.GameTime);
            uSW.Stop();
            GameStatus.Add($"Scene {scene}.Update", uSW);
        }
        /// <summary>
        /// Draw scene
        /// </summary>
        /// <param name="scene">Scene</param>
        private void FrameSceneDraw(Scene scene)
        {
            if (this.BufferManager.SetVertexBuffers())
            {
                Stopwatch dSW = new Stopwatch();
                dSW.Start();
                scene.Draw(this.GameTime);
                dSW.Stop();
                GameStatus.Add($"Scene {scene}.Draw", dSW);
            }
        }
        /// <summary>
        /// End frame
        /// </summary>
        private void FrameEnd()
        {
            Stopwatch pSW = new Stopwatch();
            pSW.Start();
            this.Graphics.End();
            pSW.Stop();
            GameStatus.Add("End", pSW);
        }
        /// <summary>
        /// Refreshes frame counters
        /// </summary>
        private void FrameRefreshCounters()
        {
            this.RuntimeText = string.Format(
                "{0} - {1} - FPS: {2:000} Draw C/D: {3:00}:{4:00} Inst: {5:00} U: {6:00} S: {7}:{8}:{9} F. Time: {10:0.0000} (secs) T. Time: {11:0000} (secs) CPU: {12:0.00}%",
                this.Graphics.DeviceDescription,
                this.Name,
                Counters.FrameCount,
                Counters.DrawCallsPerFrame,
                Counters.InstancesPerFrame,
                Counters.MaxInstancesPerFrame,
                Counters.UpdatesPerFrame,
                Counters.RasterizerStateChanges, Counters.BlendStateChanges, Counters.DepthStencilStateChanges,
                this.GameTime.ElapsedSeconds,
                this.GameTime.TotalSeconds,
                this.CPUStats.NextValue());
#if DEBUG
            this.Form.Text = this.RuntimeText;
#endif
            Counters.FrameCount = 0;
            Counters.FrameTime = 0f;
        }
        /// <summary>
        /// Collects frame status
        /// </summary>
        private void FrameCollectGameStatus()
        {
            GameStatusCollectedEventArgs e = new GameStatusCollectedEventArgs()
            {
                Trace = GameStatus.Copy(),
            };

            GameStatusCollected?.Invoke(this, e);

            CollectGameStatus = false;
        }
    }
}
