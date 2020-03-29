
namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : Updatable, IDrawable, ICullable, ISceneObject
    {
        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get { return this.Scene.Game; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager { get { return this.Game.BufferManager; } }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        public int Order { get; set; } = 0;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        public bool DeferredEnabled { get; set; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        public bool DepthEnabled { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool AlphaEnabled { get; set; }
        /// <summary>
        /// Object usage
        /// </summary>
        public SceneObjectUsages Usage { get; set; } = SceneObjectUsages.None;
        /// <summary>
        /// Gets or sets if the current object has a parent
        /// </summary>
        public bool HasParent { get; set; } = false;
        /// <summary>
        /// Maximum instance count
        /// </summary>
        public int InstanceCount { get; protected set; } = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Drawable(Scene scene, SceneObjectDescription description) : base(scene, description)
        {
            this.Name = description.Name;
            this.CastShadow = description.CastShadow;
            this.DeferredEnabled = description.DeferredEnabled;
            this.DepthEnabled = description.DepthEnabled;
            this.AlphaEnabled = description.AlphaEnabled;
        }

        /// <summary>
        /// Draw shadows
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void DrawShadows(DrawContextShadows context)
        {

        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public virtual void Draw(DrawContext context)
        {

        }

        /// <summary>
        /// Performs culling test
        /// </summary>
        /// <param name="volume">Volume</param>
        /// <param name="distance">If the object is inside the volume, returns the distance</param>
        /// <returns>Returns true if the object is outside of the frustum</returns>
        /// <remarks>By default, returns true and distance = float.MaxValue</remarks>
        public virtual bool Cull(ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            return false;
        }

        /// <summary>
        /// Gets a text representation of the instance
        /// </summary>
        /// <returns>Returns a string</returns>
        public override string ToString()
        {
            return $"{this.GetType()}. {Name}";
        }
    }
}
