using System.Linq;

namespace Engine.Audio
{
    using SharpDX.X3DAudio;

    /// <summary>
    /// SharpDX extensions
    /// </summary>
    static class SharpDXExtensions
    {
        /// <summary>
        /// Converts the game curve to the SharpDX curve
        /// </summary>
        /// <param name="points">Curve points</param>
        /// <returns>Returns the SharpDX curve array</returns>
        public static CurvePoint[] ConvertCurve(GameAudioCurvePoint[] points)
        {
            return points?.Select(p => new CurvePoint { Distance = p.Distance, DspSetting = p.DspSetting }).ToArray();
        }
        /// <summary>
        /// Converts the game SharpDX curve to the curve
        /// </summary>
        /// <param name="points">SharpDX curve points</param>
        /// <returns>Returns the curve array</returns>
        public static GameAudioCurvePoint[] ConvertCurve(CurvePoint[] points)
        {
            return points?.Select(p => new GameAudioCurvePoint { Distance = p.Distance, DspSetting = p.DspSetting }).ToArray();
        }
    }
}
