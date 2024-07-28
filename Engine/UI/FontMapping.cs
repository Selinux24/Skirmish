
namespace Engine.UI
{
    /// <summary>
    /// Font mapping description
    /// </summary>
    public struct FontMapping
    {
        /// <summary>
        /// Image filename
        /// </summary>
        public string ImageFile { get; set; }
        /// <summary>
        /// Map filename
        /// </summary>
        public string MapFile { get; set; }

        /// <summary>
        /// Map is empty
        /// </summary>
        public readonly bool IsEmpty
        {
            get
            {
                //Needs to be both informed
                return string.IsNullOrEmpty(ImageFile) || string.IsNullOrEmpty(MapFile);
            }
        }
    }
}
