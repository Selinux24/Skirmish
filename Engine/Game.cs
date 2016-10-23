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
        /// Constructor
        /// </summary>
        /// <param name="name">Name, for the game form</param>
        /// <param name="screenWidth">Window width</param>
        /// <param name="screenHeight">Window height</param>
        /// <param name="fullScreen">Full screen window</param>
        /// <param name="refreshRate">Refresh rate</param>
        /// <param name="multiSampleCount">Multi-sample count</param>
        public Game(string name, bool fullScreen = true, int screenWidth = 0, int screenHeight = 0, bool vsyncEnabled = true, int refreshRate = 0, int multiSampleCount = 0)
        {
            this.Name = name;

            this.GameTime = new GameTime();

            this.ResourceManager = new GameResourceManager(this);

            this.CPUStats = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            #region Form

            if (screenWidth == 0 || screenHeight == 0)
            {
                OutputDescription mode = Graphics.GetDesktopMode();

                screenWidth = mode.DesktopBounds.Width;
                screenHeight = mode.DesktopBounds.Height;
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

            this.Graphics = new Graphics(this.Form, vsyncEnabled, refreshRate, multiSampleCount);

            DrawerPool.Initialize(this.Graphics.Device);
        }
        /// <summary>
        /// Begins render loop
        /// </summary>
        public void Run()
        {
            RenderLoop.Run(this.Form, this.Frame);
        }
        /// <summary>
        /// Adds scene to collection
        /// </summary>
        /// <param name="scene">Scene</param>
        public void AddScene(Scene scene)
        {
            this.scenes.Add(scene);
            this.scenes.Sort(
                delegate(Scene p1, Scene p2)
                {
                    return p2.Order.CompareTo(p1.Order);
                });

            scene.Initialize();
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
        /// Dispose opened resources
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.scenes);

            DrawerPool.Dispose();

            FontMap.ClearCache();

            Helper.Dispose(this.Graphics);
            Helper.Dispose(this.Input);
            Helper.Dispose(this.Form);
            Helper.Dispose(this.ResourceManager);
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

            this.Input.Update(this.GameTime);

            this.Graphics.Begin();

            for (int i = 0; i < this.scenes.Count; i++)
            {
                if (this.scenes[i].Active)
                {
                    this.scenes[i].Update(this.GameTime);

                    this.scenes[i].Draw(this.GameTime);
                }
            }

            this.Graphics.End();

            Counters.FrameCount++;
            Counters.FrameTime += this.GameTime.ElapsedSeconds;

            if (Counters.FrameTime >= 1.0f)
            {
                this.RuntimeText = string.Format(
                    "{0} - {1} - FPS: {2:000} Draw Calls: {3:00}/{4:00} Updates: {5:00} Frame Time: {6:0.0000} (secs) Total Time: {7:0000} (secs) CPU: {8:0.00}%",
                    this.Graphics.DeviceDescription,
                    this.Name,
                    Counters.FrameCount,
                    Counters.DrawCallsPerFrame,
                    Counters.InstancesPerFrame,
                    Counters.UpdatesPerFrame,
                    this.GameTime.ElapsedSeconds,
                    this.GameTime.TotalSeconds,
                    this.CPUStats.NextValue());
#if DEBUG
                this.Form.Text = this.RuntimeText;
#endif
                Counters.FrameCount = 0;
                Counters.FrameTime = 0f;
            }

            Counters.ClearFrame();

            if (this.exiting)
            {
                //Exit form
                this.Form.Close();
            }
        }
    }
}
