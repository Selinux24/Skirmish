using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public string Name = null;
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
                return this.scenes.FindAll(s => s.Active == true).Count;
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
        /// Takes a shoot of the frame
        /// </summary>
        public bool TakeFrameShoot { get; set; }

        internal Dictionary<string, double> FrameShoot = new Dictionary<string, double>();

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
                if (this.scenes != null)
                {
                    for (int i = 0; i < this.scenes.Count; i++)
                    {
                        this.scenes[i]?.Dispose();
                        this.scenes[i] = null;
                    }

                    this.scenes.Clear();
                    this.scenes = null;
                }

                DrawerPool.DisposeResources();

                FontMap.ClearCache();

                if (Input != null)
                {
                    Input.Dispose();
                    Input = null;
                }
                if (Form != null)
                {
                    Form.Dispose();
                    Form = null;
                }
                if (ResourceManager != null)
                {
                    ResourceManager.Dispose();
                    ResourceManager = null;
                }
                if (Graphics != null)
                {
                    Graphics.Dispose();
                    Graphics = null;
                }
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
        /// Adds a new scene to collection
        /// </summary>
        /// <typeparam name="T">Type of scene</typeparam>
        /// <param name="active">Sets the new scene as active</param>
        /// <param name="order">The processing order of the scene</param>
        public void AddScene<T>(bool active = true, int order = 1) where T : Scene
        {
            Scene scene = (T)Activator.CreateInstance(typeof(T), new[] { this });
            scene.Active = active;
            scene.Order = order;

            this.AddScene(scene);
        }
        /// <summary>
        /// Creates a new scene and sets it as the unique active scene
        /// </summary>
        /// <typeparam name="T">Type of scene</typeparam>
        /// <remarks>Current scenes will be removed from internal scene collection</remarks>
        public void SetScene<T>() where T : Scene
        {
            this.nextScene = (T)Activator.CreateInstance(typeof(T), new[] { this });
        }
        /// <summary>
        /// Adds a scene to the internal scene collection
        /// </summary>
        /// <param name="scene">New scene</param>
        private void AddScene(Scene scene)
        {
            this.scenes.Add(scene);
            this.scenes.Sort(
                delegate (Scene p1, Scene p2)
                {
                    return p2.Order.CompareTo(p1.Order);
                });

            scene.Initialize();

            scene.Initialized();

            scene.SetResources();
        }
        /// <summary>
        /// Remove scene from collection
        /// </summary>
        /// <param name="scene">Scene</param>
        public void RemoveScene(Scene scene)
        {
            if (this.scenes.Contains(scene))
            {
                this.scenes.Remove(scene);

                scene.Dispose();
                scene = null;
            }
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
            this.GameTime.Update();

            Stopwatch gSW = new Stopwatch();
            gSW.Start();

            Stopwatch pSW = new Stopwatch();
            pSW.Start();
            this.Input.Update(this.GameTime);
            pSW.Stop();
            FrameShoot.Add("Input", pSW.Elapsed.TotalMilliseconds);

            pSW.Restart();
            this.Graphics.Begin();
            pSW.Stop();
            FrameShoot.Add("Begin", pSW.Elapsed.TotalMilliseconds);

            for (int i = 0; i < this.scenes.Count; i++)
            {
                if (this.scenes[i].Active)
                {
                    pSW.Restart();
                    this.scenes[i].Update(this.GameTime);
                    pSW.Stop();
                    FrameShoot.Add($"Scene {i} Update", pSW.Elapsed.TotalMilliseconds);

                    pSW.Restart();
                    this.scenes[i].Draw(this.GameTime);
                    pSW.Stop();
                    FrameShoot.Add($"Scene {i} Draw", pSW.Elapsed.TotalMilliseconds);
                }
            }

            pSW.Restart();
            this.Graphics.End();
            pSW.Stop();
            FrameShoot.Add("End", pSW.Elapsed.TotalMilliseconds);

            gSW.Stop();
            FrameShoot.Add("TOTAL", gSW.Elapsed.TotalMilliseconds);

            Counters.FrameCount++;
            Counters.FrameTime += this.GameTime.ElapsedSeconds;

            if (Counters.FrameTime >= 1.0f)
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

            if (TakeFrameShoot)
            {
                ShootTakenEventArgs e = new ShootTakenEventArgs()
                {
                    Trace = new Dictionary<string, double>(FrameShoot),
                };

                ShootTaken?.Invoke(this, e);

                TakeFrameShoot = false;
            }

            FrameShoot.Clear();

            Counters.ClearFrame();

            if (this.exiting)
            {
                //Exit form
                this.Form.Close();
            }
            else if (this.nextScene != null)
            {
                this.ChangeScene(this.nextScene);
                this.nextScene = null;
            }
        }
        /// <summary>
        /// Unloads the current scenes and loads the specified scene
        /// </summary>
        /// <param name="sceneToLoad">Scene to load</param>
        private void ChangeScene(Scene sceneToLoad)
        {
            foreach (var s in this.scenes)
            {
                s.Active = false;
            }

            List<Scene> toDispose = new List<Scene>(this.scenes);
            foreach (var s in toDispose)
            {
                s.Dispose();
            }
            toDispose.Clear();
            toDispose = null;

            this.scenes.Clear();
            this.AddScene(sceneToLoad);
            sceneToLoad.Active = true;
        }

        public event ShootTakenEventHandler ShootTaken;
    }

    public class ShootTakenEventArgs : EventArgs
    {
        public Dictionary<string, double> Trace = new Dictionary<string, double>();
    }

    public delegate void ShootTakenEventHandler(object sender, ShootTakenEventArgs e);
}
