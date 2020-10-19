using SharpDX.X3DAudio;
using System.Linq;

namespace Engine.Audio
{
    /// <summary>
    /// Defines a DSP setting at a given normalized distance.
    /// </summary>
    public struct GameAudioCurvePoint
    {
        /// <summary>
        /// Default linear curve
        /// </summary>
        public static readonly GameAudioCurvePoint[] DefaultLinearCurve = new GameAudioCurvePoint[]
        {
            new GameAudioCurvePoint(){ Distance = 0.0f, DspSetting = 1.0f, },
            new GameAudioCurvePoint(){ Distance = 1.0f, DspSetting = 0.0f, },
        };
        /// <summary>
        /// Default emitter lfe curve
        /// </summary>
        public static readonly GameAudioCurvePoint[] DefaultLfeCurve = new GameAudioCurvePoint[]
        {
            new GameAudioCurvePoint(){ Distance = 0.0f, DspSetting = 1.0f, },
            new GameAudioCurvePoint(){ Distance = 0.25f, DspSetting = 0.0f, },
            new GameAudioCurvePoint(){ Distance = 1.0f, DspSetting = 0.0f, },
        };
        /// <summary>
        /// Default emitter reverb curve
        /// </summary>
        public static readonly GameAudioCurvePoint[] DefaultReverbCurve = new GameAudioCurvePoint[]
        {
            new GameAudioCurvePoint(){ Distance = 0.0f, DspSetting = 0.5f, },
            new GameAudioCurvePoint(){ Distance = 0.75f, DspSetting = 1.0f, },
            new GameAudioCurvePoint(){ Distance = 1.0f, DspSetting = 0.0f, },
        };

        /// <summary>
        /// Converts the game curve to the SharpDX curve
        /// </summary>
        /// <param name="points">Curve points</param>
        /// <returns>Returns the SharpDX curve array</returns>
        internal static CurvePoint[] ConvertCurve(GameAudioCurvePoint[] points)
        {
            return points?.Select(p => (CurvePoint)p).ToArray();
        }
        /// <summary>
        /// Converts the game SharpDX curve to the curve
        /// </summary>
        /// <param name="points">SharpDX curve points</param>
        /// <returns>Returns the curve array</returns>
        internal static GameAudioCurvePoint[] ConvertCurve(CurvePoint[] points)
        {
            return points?.Select(p => (GameAudioCurvePoint)p).ToArray();
        }

        /// <summary>
        /// Normalized distance. This must be within 0.0f to 1.0f.
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// DSP control setting.
        /// </summary>
        public float DspSetting { get; set; }

        /// <summary>
        /// Defines an explicit conversion of a SharpDX.X3DAudio.CurvePoint to GameAudioCurvePoint
        /// </summary>
        /// <param name="curvePoint">Curve point</param>
        public static explicit operator GameAudioCurvePoint(CurvePoint curvePoint)
        {
            return new GameAudioCurvePoint()
            {
                Distance = curvePoint.Distance,
                DspSetting = curvePoint.DspSetting,
            };
        }
        /// <summary>
        /// Defines an explicit conversion of a GameAudioCurvePoint to SharpDX.X3DAudio.CurvePoint
        /// </summary>
        /// <param name="curvePoint">Curve point</param>
        public static explicit operator CurvePoint(GameAudioCurvePoint curvePoint)
        {
            return new CurvePoint()
            {
                Distance = curvePoint.Distance,
                DspSetting = curvePoint.DspSetting,
            };
        }
    }
}
