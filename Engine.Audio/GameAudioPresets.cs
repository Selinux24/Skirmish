using System.Collections.Generic;
using System.Linq;

namespace Engine.Audio
{
    using Vortice.XAudio2.Fx;

    /// <summary>
    /// Game audio presets
    /// </summary>
    static class GameAudioPresets
    {
        /// <summary>
        /// Reverb effect preset list
        /// </summary>
        private static readonly ReverbI3DL2Parameters[] presetParams =
        [
            Presets.Default,
            Presets.Generic,
            Presets.PaddedCell,
            Presets.Room,
            Presets.BathRoom,
            Presets.LivingRoom,
            Presets.StoneRoom,
            Presets.Auditorium,
            Presets.ConcertHall,
            Presets.Cave,
            Presets.Arena,
            Presets.Hangar,
            Presets.CarpetedHallway,
            Presets.Hallway,
            Presets.StoneCorridor,
            Presets.Alley,
            Presets.Forest,
            Presets.City,
            Presets.Mountains,
            Presets.Quarry,
            Presets.Plain,
            Presets.ParkingLot,
            Presets.SewerPipe,
            Presets.UnderWater,
            Presets.SmallRoom,
            Presets.MediumRoom,
            Presets.LargeRoom,
            Presets.MediumHall,
            Presets.LargeHall,
            Presets.Plate,
        ];

        /// <summary>
        /// Gets the number of game audio presets
        /// </summary>
        public static int NumPresets => presetParams.Length;
        /// <summary>
        /// Gets the preset names
        /// </summary>
        /// <returns>Return the preset name list</returns>
        public static IEnumerable<string> GetPresetNames()
        {
            var propNames = typeof(Presets)
                .GetProperties()
                .Select(p => p.Name)
                .ToArray();

            return propNames;
        }
        /// <summary>
        /// Converts from presets enumeration to ReverbParameters type
        /// </summary>
        /// <param name="preset">Preset value</param>
        /// <returns>Returns the ReverbParameters type</returns>
        public static ReverbI3DL2Parameters Convert(GameAudioReverbPresets preset)
        {
            return presetParams[(int)preset];
        }
    }
}
