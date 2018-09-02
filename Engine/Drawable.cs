namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable object
    /// </summary>
    public abstract class Drawable : Updatable, IDrawable, ICullable
    {
        /// <summary>
        /// Game class
        /// </summary>
        public Game Game { get { return this.Scene.Game; } }
        /// <summary>
        /// Buffer manager
        /// </summary>
        public BufferManager BufferManager { get { return this.Scene.BufferManager; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public Drawable(Scene scene, SceneObjectDescription description) : base(scene, description)
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
        public virtual bool Cull(ICullingVolume volume, out float distance)
        {
            distance = float.MaxValue;

            return false;
        }
    }
}
