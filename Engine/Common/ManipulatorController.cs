using SharpDX;
using System;

namespace Engine.Common
{
    /// <summary>
    /// Manipulator controller base class
    /// </summary>
    public abstract class ManipulatorController : IControllable
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
        /// Current velocity
        /// </summary>
        protected Vector3 Velocity = Vector3.Zero;

        /// <summary>
        /// Current speed
        /// </summary>
        public float Speed
        {
            get
            {
                return this.Velocity.Length();
            }
        }
        /// <summary>
        /// Maximum speed
        /// </summary>
        public float MaximumSpeed { get; set; } = 1f;
        /// <summary>
        /// Maximum force
        /// </summary>
        public float MaximumForce { get; set; } = 1f;
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
        /// First point
        /// </summary>
        public Vector3 First
        {
            get
            {
                return path.First;
            }
        }
        /// <summary>
        /// Last point
        /// </summary>
        public Vector3 Last
        {
            get
            {
                return path.Last;
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

            if (this.PathStart != null)
            {
                this.PathStart.Invoke(this, new EventArgs());
            }
        }
        /// <summary>
        /// Clears current path
        /// </summary>
        public virtual void Clear()
        {
            this.path = null;
            this.pathTime = 0f;

            if (this.PathEnd != null)
            {
                this.PathEnd.Invoke(this, new EventArgs());
            }
        }
    }
}
