namespace Engine.Helpers.DDS
{
    /// <summary>
    /// DDS Caps
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/es-es/library/windows/desktop/bb943982(v=vs.85).aspx</remarks>
    public enum DDSCapsEnum : int
    {
        /// <summary>
        /// DDSCAPS_COMPLEX: Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
        /// </summary>
        Complex = 0x8,
        /// <summary>
        /// DDSCAPS_MIPMAP: Optional; should be used for a mipmap.
        /// </summary>
        Mipmap = 0x400000,
        /// <summary>
        /// DDSCAPS_TEXTURE: Required
        /// </summary>
        Texture = 0x1000,
    }
}
