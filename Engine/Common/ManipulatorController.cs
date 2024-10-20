﻿using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Common
{
    /// <summary>
    /// Manipulator controller base class
    /// </summary>
    public abstract class ManipulatorController : IControllable, IHasGameState
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
                return Velocity.Length();
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
                return (path?.Length ?? 0) > 0;
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

        /// <inheritdoc/>
        public abstract void UpdateManipulator(IGameTime gameTime, IManipulator3D manipulator);

        /// <inheritdoc/>
        public virtual void Follow(IControllerPath newPath, float time = 0f)
        {
            path = newPath;
            pathTime = time;

            PathStart?.Invoke(this, new EventArgs());
        }
        /// <inheritdoc/>
        public virtual void Clear()
        {
            path = null;
            pathTime = 0f;

            PathEnd?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the current controller path
        /// </summary>
        /// <returns>Returns the current controller path</returns>
        public IEnumerable<Vector3> SamplePath(float sampleTime = 0.1f)
        {
            return path?.SamplePath(sampleTime) ?? Array.Empty<Vector3>();
        }

        /// <inheritdoc/>
        public abstract IGameState GetState();
        /// <inheritdoc/>
        public abstract void SetState(IGameState state);
    }
}
