using System;
using System.Collections.Generic;
using SharpDX.Windows;

namespace Common
{
    public class Game : IDisposable
    {
        private List<Scene> scenes = new List<Scene>();
        private string name = null;
#if DEBUG
        private int frameCount = 0;
        private double frameTime = 0;
#endif

        public RenderForm Form { get; private set; }
        public GameTime GameTime { get; private set; }
        public Input Input { get; private set; }
        public Graphics Graphics { get; private set; }

        public Game(string name, int screenWidth, int screenHeight, bool vSync, bool fullScreen, int refreshRate = 0, int multiSampleCount = 8)
        {
            this.name = name;

            #region Form

            this.Form = new RenderForm(name)
            {
                ClientSize = new System.Drawing.Size()
                {
                    Width = screenWidth,
                    Height = screenHeight,
                },
            };

            this.Form.UserResized += (sender, eventArgs) =>
            {
                this.Graphics.PrepareDevice((RenderForm)sender, true);

                if (this.scenes.Count > 0)
                {
                    for (int i = 0; i < this.scenes.Count; i++)
                    {
                        this.scenes[i].Camera.SetLens(
                            this.Graphics.Width, 
                            this.Graphics.Height);
                    }
                }
            };

            #endregion

            this.GameTime = new GameTime();

            this.Input = new Input(this.Form);

            this.Graphics = new Graphics(this.Form, vSync, fullScreen, refreshRate, multiSampleCount);
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

            this.Input.Update();

            this.Graphics.Begin();

            if (this.scenes.Count > 0)
            {
                for (int i = 0; i < this.scenes.Count; i++)
                {
                    if (this.scenes[i].Active)
                    {
                        this.Graphics.SetDefaultRasterizer();

                        this.scenes[i].Update();

                        if (this.scenes[i].UseZBuffer)
                        {
                            this.Graphics.EnableZBuffer();
                        }
                        else
                        {
                            this.Graphics.DisableZBuffer();
                        }

                        this.scenes[i].Draw();
                    }
                }
            }

            this.Graphics.End();

#if DEBUG
            this.frameCount++;
            this.frameTime += this.GameTime.ElapsedTime.TotalMilliseconds;
            if (this.frameTime >= 1000f)
            {
                this.Form.Text = string.Format(
                    "{0} - FPS: {1} Frame Time: {2:0.0000} (ms)",
                    this.name,
                    this.frameCount,
                    this.frameTime / this.frameCount);

                this.frameCount = 0;
                this.frameTime = 0f;
            }
#endif
        }
    }
}
