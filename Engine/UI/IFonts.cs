
namespace Engine.UI
{
    /// <summary>
    /// Font helpers interface
    /// </summary>
    public interface IFonts
    {
        /// <summary>
        /// Creates a font map of the specified font file and size
        /// </summary>
        /// <param name="generator">Keycode generator</param>
        /// <param name="mapParams">Font map parameters</param>
        /// <param name="fileName">Font file</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        FontMapDescription FromFile(FontMapKeycodeGenerator generator, FontMapProcessParameters mapParams, string fileName, float size, FontMapStyles style);
        /// <summary>
        /// Creates a font map of the specified font and size
        /// </summary>
        /// <param name="generator">Keycode generator</param>
        /// <param name="mapParams">Font map parameters</param>
        /// <param name="familyName">Font family</param>
        /// <param name="size">Size</param>
        /// <param name="style">Style</param>
        /// <returns>Returns the created font map</returns>
        FontMapDescription FromFamilyName(FontMapKeycodeGenerator generator, FontMapProcessParameters mapParams, string familyName, float size, FontMapStyles style);
        /// <summary>
        /// Find the first valid font of a comma separated font family string
        /// </summary>
        /// <param name="fontFamily">Comma separated font family string</param>
        /// <returns>Returns the first valid font of a comma separated font family string</returns>
        string FindFonts(string fontFamily);
        /// <summary>
        /// Gets the font name from a font file
        /// </summary>
        /// <param name="fileName">Font file</param>
        /// <returns>Returns the font name</returns>
        string GetFromFileFontName(string fileName);
    }
}
