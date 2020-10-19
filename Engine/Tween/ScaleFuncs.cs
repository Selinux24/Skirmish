using System;

namespace Engine.Tween
{
    /// <summary>
    /// Takes in progress which is the percentage of the tween complete and returns
    /// the interpolation value that is fed into the lerp function for the tween.
    /// </summary>
    /// <remarks>
    /// Scale functions are used to define how the tween should occur. Examples would be linear,
    /// easing in quadratic, or easing out circular. You can implement your own scale function
    /// or use one of the many defined in the ScaleFuncs static class.
    /// </remarks>
    /// <param name="progress">The percentage of the tween complete in the range [0, 1].</param>
    /// <returns>The scale value used to lerp between the tween's start and end values</returns>
    public delegate float ScaleFunc(float progress);

    /// <summary>
    /// Defines a set of premade scale functions for use with tweens.
    /// </summary>
    /// <remarks>
    /// To avoid excess allocations of delegates, the public members of ScaleFuncs are already
    /// delegates that reference private methods.
    ///
    /// Implementations based on http://theinstructionlimit.com/flash-style-tweeneasing-functions-in-c
    /// which are based on http://www.robertpenner.com/easing/
    /// </remarks>
    public static class ScaleFuncs
    {
        private const float Pi = (float)Math.PI;
        private const float HalfPi = Pi / 2f;

        /// <summary>
        /// A linear progress scale function.
        /// </summary>
        public static readonly ScaleFunc Linear = LinearImpl;
        /// <summary>
        /// A quadratic (x^2) progress scale function that eases in.
        /// </summary>
        public static readonly ScaleFunc QuadraticEaseIn = QuadraticEaseInImpl;
        /// <summary>
        /// A quadratic (x^2) progress scale function that eases out.
        /// </summary>
        public static readonly ScaleFunc QuadraticEaseOut = QuadraticEaseOutImpl;
        /// <summary>
        /// A quadratic (x^2) progress scale function that eases in and out.
        /// </summary>
        public static readonly ScaleFunc QuadraticEaseInOut = QuadraticEaseInOutImpl;
        /// <summary>
        /// A cubic (x^3) progress scale function that eases in.
        /// </summary>
        public static readonly ScaleFunc CubicEaseIn = CubicEaseInImpl;
        /// <summary>
        /// A cubic (x^3) progress scale function that eases out.
        /// </summary>
        public static readonly ScaleFunc CubicEaseOut = CubicEaseOutImpl;
        /// <summary>
        /// A cubic (x^3) progress scale function that eases in and out.
        /// </summary>
        public static readonly ScaleFunc CubicEaseInOut = CubicEaseInOutImpl;
        /// <summary>
        /// A quartic (x^4) progress scale function that eases in.
        /// </summary>
        public static readonly ScaleFunc QuarticEaseIn = QuarticEaseInImpl;
        /// <summary>
        /// A quartic (x^4) progress scale function that eases out.
        /// </summary>
        public static readonly ScaleFunc QuarticEaseOut = QuarticEaseOutImpl;
        /// <summary>
        /// A quartic (x^4) progress scale function that eases in and out.
        /// </summary>
        public static readonly ScaleFunc QuarticEaseInOut = QuarticEaseInOutImpl;
        /// <summary>
        /// A quintic (x^5) progress scale function that eases in.
        /// </summary>
        public static readonly ScaleFunc QuinticEaseIn = QuinticEaseInImpl;
        /// <summary>
        /// A quintic (x^5) progress scale function that eases out.
        /// </summary>
        public static readonly ScaleFunc QuinticEaseOut = QuinticEaseOutImpl;
        /// <summary>
        /// A quintic (x^5) progress scale function that eases in and out.
        /// </summary>
        public static readonly ScaleFunc QuinticEaseInOut = QuinticEaseInOutImpl;
        /// <summary>
        /// A sinusoidal progress scale function that eases in.
        /// </summary>
        public static readonly ScaleFunc SineEaseIn = SineEaseInImpl;
        /// <summary>
        /// A sinusoidal progress scale function that eases out.
        /// </summary>
        public static readonly ScaleFunc SineEaseOut = SineEaseOutImpl;
        /// <summary>
        /// A sinusoidal progress scale function that eases in and out.
        /// </summary>
        public static readonly ScaleFunc SineEaseInOut = SineEaseInOutImpl;

        private static float LinearImpl(float progress) { return progress; }

        private static float QuadraticEaseInImpl(float progress) { return EaseInPower(progress, 2); }
        private static float QuadraticEaseOutImpl(float progress) { return EaseOutPower(progress, 2); }
        private static float QuadraticEaseInOutImpl(float progress) { return EaseInOutPower(progress, 2); }

        private static float CubicEaseInImpl(float progress) { return EaseInPower(progress, 3); }
        private static float CubicEaseOutImpl(float progress) { return EaseOutPower(progress, 3); }
        private static float CubicEaseInOutImpl(float progress) { return EaseInOutPower(progress, 3); }

        private static float QuarticEaseInImpl(float progress) { return EaseInPower(progress, 4); }
        private static float QuarticEaseOutImpl(float progress) { return EaseOutPower(progress, 4); }
        private static float QuarticEaseInOutImpl(float progress) { return EaseInOutPower(progress, 4); }
      
        private static float QuinticEaseInImpl(float progress) { return EaseInPower(progress, 5); }
        private static float QuinticEaseOutImpl(float progress) { return EaseOutPower(progress, 5); }
        private static float QuinticEaseInOutImpl(float progress) { return EaseInOutPower(progress, 5); }

        private static float EaseInPower(float progress, int power)
        {
            return (float)Math.Pow(progress, power);
        }
        private static float EaseOutPower(float progress, int power)
        {
            int sign = power % 2 == 0 ? -1 : 1;
            return (float)(sign * (Math.Pow(progress - 1, power) + sign));
        }
        private static float EaseInOutPower(float progress, int power)
        {
            progress *= 2;
            if (progress < 1)
            {
                return (float)Math.Pow(progress, power) / 2f;
            }
            else
            {
                int sign = power % 2 == 0 ? -1 : 1;
                return (float)(sign / 2.0 * (Math.Pow(progress - 2, power) + sign * 2));
            }
        }

        private static float SineEaseInImpl(float progress)
        {
            return (float)Math.Sin(progress * HalfPi - HalfPi) + 1;
        }
        private static float SineEaseOutImpl(float progress)
        {
            return (float)Math.Sin(progress * HalfPi);
        }
        private static float SineEaseInOutImpl(float progress)
        {
            return (float)(Math.Sin(progress * Pi - HalfPi) + 1) / 2;
        }
    }
}
