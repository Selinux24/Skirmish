using System;
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
        /// Joins two arrays
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array1">First array</param>
        /// <param name="array2">Second array</param>
        /// <returns>Returns an array with both array values</returns>
        public static T[] Join<T>(this T[] array1, T[] array2)
        {
            T[] newArray = new T[array1.Length + array2.Length];

            array1.CopyTo(newArray, 0);
            array2.CopyTo(newArray, array1.Length);

            return newArray;
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
        /// <summary>
        /// Dispose disposable object
        /// </summary>
        /// <param name="obj">Disposable object</param>
        public static void Dispose(IDisposable obj)
        {
            if (obj != null)
            {
                obj.Dispose();
                obj = null;
            }
        }
    }
}
