
namespace Engine.Common
{
    using Engine.Animation;

    /// <summary>
    /// The instance use skinning data for render
    /// </summary>
    public interface IUseSkinningData
    {
        /// <summary>
        /// Gets the skinning data used by the current drawing data
        /// </summary>
        SkinningData SkinningData { get; }
    }
}
