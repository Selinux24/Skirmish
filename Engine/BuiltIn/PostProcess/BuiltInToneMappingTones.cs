
namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Built-in tone mapping tones
    /// </summary>
    public enum BuiltInToneMappingTones : uint
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Linear
        /// </summary>
        Linear = 1,
        /// <summary>
        /// Simple Reinhard
        /// </summary>
        SimpleReinhard = 2,
        /// <summary>
        /// Luma-based Reinhard
        /// </summary>
        LumaBasedReinhard = 3,
        /// <summary>
        /// White preserving Luma-based Reinhard
        /// </summary>
        WhitePreservingLumaBasedReinhard = 4,
        /// <summary>
        /// Roman Galashov's RomBinDaHouse
        /// </summary>
        RomBinDaHouse = 5,
        /// <summary>
        /// Filmic
        /// </summary>
        Filmic = 6,
        /// <summary>
        /// Uncharted 2
        /// </summary>
        Uncharted2 = 7,
    }
}
