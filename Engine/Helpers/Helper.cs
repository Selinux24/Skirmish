using SharpDX;

namespace Engine.Helpers
{
    /// <summary>
    /// Helper functions
    /// </summary>
    static class Helper
    {
        /// <summary>
        /// Generate an array initialized to defaultValue
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, T defaultValue)
        {
            T[] array = new T[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = defaultValue;
            }

            return array;
        }
        /// <summary>
        /// Gets angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns angle value</returns>
        public static float Angle(Vector3 one, Vector3 two)
        {
            //Get the dot product
            float dot = Vector3.Dot(one, two);

            // Divide the dot by the product of the magnitudes of the vectors
            dot = dot / (one.Length() * two.Length());

            //Get the arc cosin of the angle, you now have your angle in radians 
            return (float)System.Math.Acos(dot);
        }
    }
}
