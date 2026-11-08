
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
        public string[] Controllers { get; set; } = [];
        /// <summary>
        /// Skeleton information
        /// </summary>
        public Skeleton Skeleton { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Controllers == null)
            {
                return $"{nameof(SkinningContent)}. Empty;";
            }

            return $"{nameof(SkinningContent)}. {string.Join(", ", Controllers)}";
        }
    }
}
