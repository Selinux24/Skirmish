
namespace Engine.Common
{
    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : Updatable, IDrawable, ICullable, ISceneObject
    {
        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get { return Scene.Game; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager { get { return Game.BufferManager; } }

        /// <summary>
        /// Visible
        /// </summary>
        public virtual bool Visible { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        public virtual bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        public virtual bool DeferredEnabled { get; set; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        public virtual bool DepthEnabled { get; set; }
        /// <summary>
        /// Blend mode
        /// </summary>
        public virtual BlendModes BlendMode { get; set; }
        /// <summary>
        /// Object usage
        /// </summary>
        public virtual SceneObjectUsages Usage { get; set; } = SceneObjectUsages.None;
        /// <summary>
        /// Processing layer
        /// </summary>
        public virtual int Layer { get; set; } = 0;
        /// <summary>
        /// Gets or sets if the current object has a parent
        /// </summary>
        public virtual bool HasParent { get; set; } = false;
        /// <summary>
        /// Maximum instance count
        /// </summary>
        public virtual int InstanceCount { get; protected set; } = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        protected Drawable(string name, Scene scene, SceneObjectDescription description) :
            base(name, scene, description)
        {
            CastShadow = description.CastShadow;
            DeferredEnabled = description.DeferredEnabled;
            DepthEnabled = description.DepthEnabled;
            BlendMode = description.BlendMode;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {

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
        public virtual bool Cull(IIntersectionVolume volume, out float distance)
        {
            distance = float.MaxValue;

            return false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{GetType()}.{Name ?? "NoName"}";
        }
    }
}
