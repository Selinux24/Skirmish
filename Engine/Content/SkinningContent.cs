
namespace Engine.Content
{
    using Engine.Animation;

    /// <summary>
    /// Skinning content
    /// </summary>
    public class SkinningContent
    {
        /// <summary>
        /// Controller names
        /// </summary>
        public string[] Controllers { get; set; } = new string[] { };
        /// <summary>
        /// Skeleton information
        /// </summary>
        public Skeleton Skeleton { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (this.Controllers?.Length == 1)
            {
                return string.Format("{0}", this.Controllers[0]);
            }
            else if (this.Controllers?.Length > 1)
            {
                return string.Format("{0}", string.Join(", ", this.Controllers));
            }
            else
            {
                return "Empty Controller;";
            }
        }
    }
}
