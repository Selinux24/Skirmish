using System;

namespace Engine.Common
{
    public abstract class ManipulatorController
    {
        /// <summary>
        /// Following path
        /// </summary>
        protected IControllerPath path = null;
        /// <summary>
        /// Path time
        /// </summary>
        protected float pathTime = 0f;
        /// <summary>
        /// Gets if the current controller has a initialized path
        /// </summary>
        public bool HasPath
        {
            get
            {
                return this.path != null && this.path.Length > 0;
            }
        }

        /// <summary>
        /// On path ending event
        /// </summary>
        public event EventHandler PathStart;
        /// <summary>
        /// On path ending event
        /// </summary>
        public event EventHandler PathEnd;

        /// <summary>
        /// Computes current position and orientation in the curve
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="manipulator">Manipulator to update</param>
        public abstract void UpdateManipulator(GameTime gameTime, Manipulator3D manipulator);

        /// <summary>
        /// Sets the path to follow
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="time">Path initial time</param>
        public virtual void Follow(IControllerPath path, float time = 0f)
        {
            this.path = path;
            this.pathTime = time;

            this.PathStart?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Clears current path
        /// </summary>
        public virtual void Clear()
        {
            this.path = null;
            this.pathTime = 0f;

            this.PathEnd?.Invoke(this, new EventArgs());
        }
    }
}
