namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Drawable interface
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        void Draw(DrawContext context);
    }
}
