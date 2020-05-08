
namespace Engine.Tween
{
    /// <summary>
    /// Standard linear interpolation function: "start + (end - start) * progress"
    /// </summary>
    /// <remarks>
    /// In a language like C++ we wouldn't need this delegate at all. Templates in C++ would allow us
    /// to simply write "start + (end - start) * progress" in the tween class and the compiler would
    /// take care of enforcing that the type supported those operators. Unfortunately C#'s generics
    /// are not so powerful so instead we must have the user provide the interpolation function.
    ///
    /// Thankfully frameworks like XNA and Unity provide lerp functions on their primitive math types
    /// which means that for most users there is nothing specific to do here. Additionally this file
    /// provides concrete implementations of tweens for vectors, colors, and more for XNA and Unity
    /// users, lessening the burden even more.
    /// </remarks>
    /// <typeparam name="T">The type to interpolate.</typeparam>
    /// <param name="start">The starting value.</param>
    /// <param name="end">The ending value.</param>
    /// <param name="progress">The interpolation progress.</param>
    /// <returns>The interpolated value, generally using "start + (end - start) * progress"</returns>
    public delegate T LerpFunc<T>(T start, T end, float progress);

    /// <summary>
    /// Object used to tween float values.
    /// </summary>
    public class FloatTween : Tween<float>
    {
        private static float LerpFloat(float start, float end, float progress)
        {
            return start + (end - start) * progress;
        }

        /// <summary>
        /// Static readonly delegate to avoid multiple delegate allocations
        /// </summary>
        private static readonly LerpFunc<float> LerpFunc = LerpFloat;

        /// <summary>
        /// Initializes a new FloatTween instance.
        /// </summary>
        public FloatTween() : base(LerpFunc)
        {

        }
    }
}
