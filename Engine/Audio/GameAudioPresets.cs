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
        /// <param name="sampleRate">Sample rate</param>
        /// <returns>Returns the ReverbParameters type</returns>
        public static ReverbParameters Convert(ReverbPresets preset, int sampleRate)
        {
            ReverbParameters reverbSettings = (ReverbParameters)reverbPresetsList[(int)preset];

            // All parameters related to sampling rate or time are relative to a 48kHz voice and must be scaled for use with other sampling rates.
            var timeScale = sampleRate / 48000f;

            var result = new ReverbParameters
            {
                ReflectionsGain = reverbSettings.ReflectionsGain,
                ReverbGain = reverbSettings.ReverbGain,
                DecayTime = reverbSettings.DecayTime,
                ReflectionsDelay = (int)(reverbSettings.ReflectionsDelay * timeScale),
                ReverbDelay = (byte)(reverbSettings.ReverbDelay * timeScale),
                RearDelay = (byte)(reverbSettings.RearDelay * timeScale),
                SideDelay = (byte)(reverbSettings.SideDelay * timeScale),
                RoomSize = reverbSettings.RoomSize,
                Density = reverbSettings.Density,
                LowEQGain = reverbSettings.LowEQGain,
                LowEQCutoff = reverbSettings.LowEQCutoff,
                HighEQGain = reverbSettings.HighEQGain,
                HighEQCutoff = reverbSettings.HighEQCutoff,
                PositionLeft = reverbSettings.PositionLeft,
                PositionRight = reverbSettings.PositionRight,
                PositionMatrixLeft = reverbSettings.PositionMatrixLeft,
                PositionMatrixRight = reverbSettings.PositionMatrixRight,
                EarlyDiffusion = reverbSettings.EarlyDiffusion,
                LateDiffusion = reverbSettings.LateDiffusion,
                RoomFilterMain = reverbSettings.RoomFilterMain,
                RoomFilterFreq = reverbSettings.RoomFilterFreq * timeScale / 100f,
                RoomFilterHF = reverbSettings.RoomFilterHF,
                WetDryMix = reverbSettings.WetDryMix,
                DisableLateField = reverbSettings.DisableLateField,
            };

            return result;
        }
    }
}
