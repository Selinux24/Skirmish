using System;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Game object
    /// </summary>
    public abstract class GameObject : IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        public virtual Game Game { get; private set; }
        /// <summary>
        /// Graphics device
        /// </summary>
        public virtual Graphics Graphics { get { return this.Game.Graphics; } }
        /// <summary>
        /// Graphics context
        /// </summary>
        public virtual DeviceContext DeviceContext { get { return this.Game.Graphics.DeviceContext; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        public virtual BufferManager BufferManager { get; protected set; }
        /// <summary>
        /// Object Description
        /// </summary>
        public virtual GameObjectDescription Description { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game"></param>
        /// <param name="bufferManager"></param>
        /// <param name="description"></param>
        public GameObject(Game game, BufferManager bufferManager, GameObjectDescription description)
        {
            this.Game = game;
            this.BufferManager = bufferManager;
            this.Description = description;
        }

        /// <summary>
        /// Resource disposal
        /// </summary>
        public abstract void Dispose();
    }

    /// <summary>
    /// Scene object description
    /// </summary>
    public class GameObjectDescription
    {

    }
}
