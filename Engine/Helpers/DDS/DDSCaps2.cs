using System;

namespace Engine.Helpers.DDS
{
    /// <summary>
    /// DDS Caps2
    /// </summary>
    /// <remarks>https://msdn.microsoft.com/es-es/library/windows/desktop/bb943982(v=vs.85).aspx</remarks>
    [Flags]
    enum DdsCaps2
    {
        /// <summary>
        /// DDSCAPS2_CUBEMAP
        /// </summary>
        Cubemap = 0x200,
        /// <summary>
        /// DDSCAPS2_CUBEMAP_POSITIVEX
        /// </summary>
        CubemapPositiveX = 0x400,
        /// <summary>
        /// DDSCAPS2_CUBEMAP_NEGATIVEX
        /// </summary>
        CubemapNegativeX = 0x800,
        /// <summary>
        /// DDSCAPS2_CUBEMAP_POSITIVEY
        /// </summary>
        CubemapPositiveY = 0x1000,
        /// <summary>
        /// DDSCAPS2_CUBEMAP_NEGATIVEY
        /// </summary>
        CubemapNegativeY = 0x2000,
        /// <summary>
        /// DDSCAPS2_CUBEMAP_POSITIVEZ
        /// </summary>
        CubemapPositiveZ = 0x4000,
        /// <summary>
        /// DDSCAPS2_CUBEMAP_NEGATIVEZ
        /// </summary>
        CubemapNegativeZ = 0x8000,
        /// <summary>
        /// DDSCAPS2_VOLUME
        /// </summary>
        Volume = 0x200000,
        /// <summary>
        /// DDSCAPS2_ALLFACES
        /// </summary>
        AllFaces = (Cubemap | CubemapPositiveX | CubemapNegativeX | CubemapPositiveY | CubemapNegativeY | CubemapPositiveZ | CubemapNegativeZ),
    }
}
