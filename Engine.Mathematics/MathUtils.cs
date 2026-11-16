using System;

namespace Engine.Mathematics
{
    /// <summary>
    /// Helper mathematics functions
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// One radian
        /// </summary>
        public const float Radian = 0.0174532924f;

        /// <summary>
        /// Zero tolerance value
        /// </summary>
        public const float ZeroTolerance = 1e-6f;
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static readonly Vector2 ZeroToleranceVector2 = new(ZeroTolerance);
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static readonly Vector3 ZeroToleranceVector3 = new(ZeroTolerance);
        /// <summary>
        /// Zero tolerance vector
        /// </summary>
        public static readonly Vector4 ZeroToleranceVector4 = new(ZeroTolerance);

        /// <summary>
        /// Pi number
        /// </summary>
        public const float Pi = MathF.PI;
        /// <summary>
        /// Two Pi number
        /// </summary>
        public const float TwoPi = MathF.PI * 2f;
        /// <summary>
        /// Pi over 2 number
        /// </summary>
        public const float PiOver2 = MathF.PI * 0.5f;
        /// <summary>
        /// Pi over 4 number
        /// </summary>
        public const float PiOver4 = MathF.PI * 0.25f;

        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <param name="epsilon">Epsilon value</param>
        /// <returns><c>true</c> if a almost equal to b within a float epsilon, <c>false</c> otherwise</returns>
        public static bool WithinEpsilon(float a, float b, float epsilon)
        {
            float num = a - b;
            return ((-epsilon <= num) && (num <= epsilon));
        }
        /// <summary>
        /// Gets whether the specified value is almost zero, within a float epsilon.
        /// </summary>
        /// <param name="a">The value to check.</param>
        /// <param name="epsilon">The epsilon value.</param>
        /// <returns><c>true</c> if the value is almost zero, <c>false</c> otherwise.</returns>
        public static bool IsZero(float a, float epsilon = ZeroTolerance)
        {
            return Math.Abs(a) < epsilon;
        }
        /// <summary>
        /// Gets whether the specified value is almost one, within a float epsilon.
        /// </summary>
        /// <param name="a">The value to check.</param>
        /// <param name="epsilon">The epsilon value.</param>
        /// <returns><c>true</c> if the value is almost one, <c>false</c> otherwise.</returns>
        public static bool IsOne(float a, float epsilon = ZeroTolerance)
        {
            return IsZero(a - 1.0f, epsilon);
        }
        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon"/> method). See Remarks.
        /// See remarks.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <returns><c>true</c> if a almost equal to b, <c>false</c> otherwise</returns>
        /// <remarks>
        /// The code is using the technique described by Bruce Dawson in 
        /// <a href="http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/">Comparing Floating point numbers 2012 edition</a>. 
        /// </remarks>
        public unsafe static bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if ((aInt < 0) != (bInt < 0))
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return (ulp <= maxUlp);
        }

        /// <summary>
        /// Gets the remainder of the division of the specified value by the specified divisor.
        /// </summary>
        /// <param name="value">The value to divide.</param>
        /// <param name="mod">The divisor.</param>
        /// <returns>The remainder of the division.</returns>
        public static float Mod(float value, float mod)
        {
            if (mod == 0f) return value;

            return value % mod;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degree">The degree value to convert.</param>
        /// <returns>The equivalent value in radians.</returns>
        public static float DegreesToRadians(float degree)
        {
            return degree * (Pi / 180.0f);
        }
        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="radians">The radian value to convert.</param>
        /// <returns>The equivalent value in degrees.</returns>
        public static float RadiansToDegrees(float radians)
        {
            return radians * (180.0f / Pi);
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        public static float SmoothStep(float amount)
        {
            if (amount <= 0) return 0;
            if (amount >= 1) return 1;
            return amount * amount * (3 - (2 * amount));
        }

        /// <summary>
        /// Gets the minimum and maximum values
        /// </summary>
        /// <param name="value1">Value 1</param>
        /// <param name="value2">Value 2</param>
        /// <param name="min">Returns the minimum value</param>
        /// <param name="max">Returns the maximum value</param>
        public static void MinMax(float value1, float value2, out float min, out float max)
        {
            if (value1 < value2)
            {
                min = value1;
                max = value2;
            }
            else
            {
                min = value2;
                max = value1;
            }
        }
        /// <summary>
        /// Gets the minimum and maximum values
        /// </summary>
        /// <param name="value1">Value 1</param>
        /// <param name="value2">Value 2</param>
        /// <param name="value3">Value 3</param>
        /// <param name="min">Returns the minimum value</param>
        /// <param name="max">Returns the maximum value</param>
        public static void MinMax(float value1, float value2, float value3, out float min, out float max)
        {
            min = value1;
            if (value2 < min) min = value2;
            if (value3 < min) min = value3;

            max = value1;
            if (value2 > max) max = value2;
            if (value3 > max) max = value3;
        }
    }
}
