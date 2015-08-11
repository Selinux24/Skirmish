
namespace Engine.Common
{
    /// <summary>
    /// 2D components interface
    /// </summary>
    public interface IScreenFitted
    {
        /// <summary>
        /// Resizes width and height to adapt to screen size changes
        /// </summary>
        void Resize();
    }
}
