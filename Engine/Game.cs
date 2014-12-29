using System;
using System.Collections.Generic;
using SharpDX.Windows;

namespace Engine
{
    using Engine.Common;

    public class Game : IDisposable
    {
        private string name = null;
        private List<Scene> scenes = new List<Scene>();

        public EngineForm Form { get; private set; }
        public GameTime GameTime { get; private set; }
        public Input Input { get; private set; }
        public Graphics Graphics { get; private set; }
        public string RuntimeText { get; private set; }
        public int SceneCount
        {
            get
            {
                return this.scenes.Count;
            }
        }

        public Game(string name, int screenWidth, int screenHeight, bool fullScreen, int refreshRate = 0, int multiSampleCount = 0)
        {
            this.name = name;

            this.GameTime = new GameTime();

            #region Form

            this.Form = new EngineForm(name, screenWidth, screenHeight, fullScreen);

            this.Form.UserResized += (sender, eventArgs) =>
            {
                if (this.Graphics != null)
                {
                    this.Graphics.PrepareDevice(this.Form.RenderWidth, this.Form.RenderHeight, true);

                    this.scenes.ForEach(s => s.HandleResizing());
                }
            };

            this.Form.GotFocus += (sender, eventArgs) =>
            {
                this.HandleFocus(true);
            };

            this.Form.LostFocus += (sender, eventArgs) =>
            {
                this.HandleFocus(false);
            };

            #endregion

            this.Input = new Input(this.Form);

            this.Graphics = new Graphics(this.Form, fullScreen, refreshRate, multiSampleCount);
        }
        public void Run()
        {
            RenderLoop.Run(this.Form, this.Frame);
        }
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
        public void RemoveScene(Scene scene)
        {
            if (this.scenes.Contains(scene))
            {
                this.scenes.Remove(scene);

                scene.Dispose();
                scene = null;
            }
        }
        public void Dispose()
        {
            if (this.scenes.Count > 0)
            {
                foreach (Scene scene in this.scenes)
                {
                    scene.Dispose();
                }

                this.scenes.Clear();
                this.scenes = null;
            }

            FontMap.ClearCache();

            if (this.Input != null)
            {
                this.Input.Dispose();
                this.Input = null;
            }

            if (this.Graphics != null)
            {
                this.Graphics.Dispose();
                this.Graphics = null;
            }

            if (this.Form != null)
            {
                this.Form.Dispose();
                this.Form = null;
            }
        }
        public void Exit()
        {
            this.Form.Close();
        }

        private void Frame()
        {
            this.GameTime.Update();

            if (this.Form.Active)
            {
                this.Input.Update();
            }
            else
            {
                this.Input.Clear();
            }

            this.Graphics.Begin();

            for (int i = 0; i < this.scenes.Count; i++)
            {
                if (this.scenes[i].Active)
                {
                    this.scenes[i].Update(this.GameTime);

                    this.Graphics.SetDefaultRasterizer();

                    if (this.scenes[i].UseZBuffer)
                    {
                        this.Graphics.EnableZBuffer();
                    }
                    else
                    {
                        this.Graphics.DisableZBuffer();
                    }

                    this.scenes[i].Draw(this.GameTime);
                }
            }

            this.Graphics.End();

            Counters.FrameCount++;
            Counters.FrameTime += this.GameTime.ElapsedSeconds;
            if (Counters.FrameTime >= 1.0f)
            {
                this.RuntimeText = string.Format(
                    "{0} - FPS: {1} Frame Time: {2:0.0000} (ms) Draw Calls: {3}",
                    this.name,
                    Counters.FrameCount,
                    Counters.FrameTime / Counters.FrameCount,
                    Counters.DrawCallsPerFrame);
#if DEBUG
                this.Form.Text = this.RuntimeText;
#endif
                Counters.FrameCount = 0;
                Counters.FrameTime = 0f;
            }

            Counters.ClearFrame();
        }
        private void HandleFocus(bool hasFocus)
        {
            if (this.Input != null)
            {
                if (hasFocus)
                {
                    if (this.Form.ShowMouse)
                    {
                        this.Input.LockToCenter = false;
                        this.Input.ShowMouse();
                    }
                    else
                    {
                        this.Input.LockToCenter = true;
                        this.Input.HideMouse();
                    }
                }
                else
                {
                    this.Input.LockToCenter = false;
                    this.Input.ShowMouse();
                }
            }
        }
    }
}
