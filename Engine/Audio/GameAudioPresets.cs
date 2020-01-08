using SharpDX.XAudio2.Fx;

namespace Engine.Audio
{
    /// <summary>
    /// Game audio presets
    /// </summary>
    static class GameAudioPresets
    {
        /// <summary>
        /// Reverb effect preset list
        /// </summary>
        private static readonly ReverbI3DL2Parameters[] reverbPresetsList =
        {
            ReverbI3DL2Parameters.Presets.Default,
            ReverbI3DL2Parameters.Presets.Generic,
            ReverbI3DL2Parameters.Presets.PaddedCell,
            ReverbI3DL2Parameters.Presets.Room,
            ReverbI3DL2Parameters.Presets.BathRoom,
            ReverbI3DL2Parameters.Presets.LivingRoom,
            ReverbI3DL2Parameters.Presets.StoneRoom,
            ReverbI3DL2Parameters.Presets.Auditorium,
            ReverbI3DL2Parameters.Presets.ConcertHall,
            ReverbI3DL2Parameters.Presets.Cave,
            ReverbI3DL2Parameters.Presets.Arena,
            ReverbI3DL2Parameters.Presets.Hangar,
            ReverbI3DL2Parameters.Presets.CarpetedHallway,
            ReverbI3DL2Parameters.Presets.Hallway,
            ReverbI3DL2Parameters.Presets.StoneCorridor,
            ReverbI3DL2Parameters.Presets.Alley,
            ReverbI3DL2Parameters.Presets.Forest,
            ReverbI3DL2Parameters.Presets.City,
            ReverbI3DL2Parameters.Presets.Mountains,
            ReverbI3DL2Parameters.Presets.Quarry,
            ReverbI3DL2Parameters.Presets.Plain,
            ReverbI3DL2Parameters.Presets.ParkingLot,
            ReverbI3DL2Parameters.Presets.SewerPipe,
            ReverbI3DL2Parameters.Presets.UnderWater,
            ReverbI3DL2Parameters.Presets.SmallRoom,
            ReverbI3DL2Parameters.Presets.MediumRoom,
            ReverbI3DL2Parameters.Presets.LargeRoom,
            ReverbI3DL2Parameters.Presets.MediumHall,
            ReverbI3DL2Parameters.Presets.LargeHall,
            ReverbI3DL2Parameters.Presets.Plate,
        };

        /// <summary>
        /// Converts from presets enumeration to ReverbParameters type
        /// </summary>
        /// <param name="preset">Preset value</param>
        /// <returns>Returns the ReverbParameters type</returns>
        public static ReverbParameters Convert(ReverbPresets preset)
        {
            var reverbParams = (ReverbParameters)reverbPresetsList[(int)preset];

            //Why? I don't know - this parameter is refered to 48Khz sampling rate. Scale it? Otherwise, the effect mutes all.
            reverbParams.RoomFilterFreq = 20;

            return reverbParams;
        }
    }
}
