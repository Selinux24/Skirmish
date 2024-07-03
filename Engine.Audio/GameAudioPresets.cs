using System.Collections.Generic;
using System.Linq;

namespace Engine.Audio
{
    using SharpDX.XAudio2.Fx;

    /// <summary>
    /// Game audio presets
    /// </summary>
    static class GameAudioPresets
    {
        /// <summary>
        /// Reverb effect preset list
        /// </summary>
        private static readonly ReverbParameters[] presetParams =
        [
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Default,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Generic,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.PaddedCell,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Room,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.BathRoom,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.LivingRoom,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.StoneRoom,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Auditorium,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.ConcertHall,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Cave,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Arena,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Hangar,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.CarpetedHallway,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Hallway,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.StoneCorridor,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Alley,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Forest,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.City,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Mountains,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Quarry,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Plain,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.ParkingLot,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.SewerPipe,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.UnderWater,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.SmallRoom,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.MediumRoom,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.LargeRoom,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.MediumHall,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.LargeHall,
            (ReverbParameters)ReverbI3DL2Parameters.Presets.Plate,
        ];

        /// <summary>
        /// Gets the number of game audio presets
        /// </summary>
        public static int NumPresets { get { return presetParams.Length; } }
        /// <summary>
        /// Gets the preset names
        /// </summary>
        /// <returns>Return the preset name list</returns>
        public static IEnumerable<string> GetPresetNames()
        {
            var propNames = typeof(ReverbI3DL2Parameters.Presets)
                .GetProperties()
                .Select(p => p.Name)
                .ToArray();

            return propNames;
        }
        /// <summary>
        /// Converts from presets enumeration to ReverbParameters type
        /// </summary>
        /// <param name="preset">Preset value</param>
        /// <param name="sampleRate">Sample rate</param>
        /// <returns>Returns the ReverbParameters type</returns>
        public static ReverbParameters Convert(GameAudioReverbPresets preset, int sampleRate)
        {
            ReverbParameters reverbSettings = presetParams[(int)preset];

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
