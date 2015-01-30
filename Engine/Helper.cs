using System;
using SharpDX;

namespace Engine
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
        public static T[] CreateArray<T>(int length, T defaultValue) where T : struct
        {
            T[] array = new T[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = defaultValue;
            }

            return array;
        }
        /// <summary>
        /// Generate an array initialized to function result
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="func">Function</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, Func<T> func)
        {
            T[] array = new T[length];

            for (int i = 0; i < length; i++)
            {
                array[i] = func.Invoke();
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
        /// Look at target
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="target">Target</param>
        /// <param name="yAxisOnly">Restricts the rotation axis to Y only</param>
        /// <returns>Returns rotation quaternion</returns>
        public static Quaternion LookAt(Vector3 eyePosition, Vector3 target, bool yAxisOnly = true)
        {
            Vector3 forwardVector = Vector3.Normalize(eyePosition - target);

            float dot = Vector3.Dot(Vector3.ForwardLH, forwardVector);

            if (Math.Abs(dot - (-1.0f)) < 0.000001f)
            {
                return new Quaternion(Vector3.Up, MathUtil.Pi);
            }
            else if (Math.Abs(dot - (1.0f)) < 0.000001f)
            {
                return Quaternion.Identity;
            }
            else
            {
                Vector3 rotAxis = Vector3.Normalize(Vector3.Cross(Vector3.ForwardLH, forwardVector));
                float rotAngle = Angle(Vector3.ForwardLH, forwardVector);

                Quaternion q = Quaternion.RotationAxis(rotAxis, rotAngle);

                if (yAxisOnly)
                {
                    q.X = 0;
                    q.Z = 0;

                    q.Normalize();
                }

                return q;
            }
        }
        /// <summary>
        /// Gets matrix description
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Return matrix description</returns>
        public static string GetDescription(this Matrix matrix)
        {
            if (matrix.IsIdentity)
            {
                return "Identity";
            }
            else
            {
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                if (matrix.Decompose(out scale, out rotation, out translation))
                {
                    string text = "";

                    scale.X = (float)Math.Round(scale.X, 0);
                    scale.Y = (float)Math.Round(scale.Y, 0);
                    scale.Z = (float)Math.Round(scale.Z, 0);

                    rotation.X = (float)Math.Round(rotation.X, 0);
                    rotation.Y = (float)Math.Round(rotation.Y, 0);
                    rotation.Z = (float)Math.Round(rotation.Z, 0);
                    //rotation.W = (float)Math.Round(rotation.W, 0);

                    translation.X = (float)Math.Round(translation.X, 0);
                    translation.Y = (float)Math.Round(translation.Y, 0);
                    translation.Z = (float)Math.Round(translation.Z, 0);

                    if (scale != Vector3.One) text += string.Format("Scale: {0}; ", scale);
                    if (!rotation.IsIdentity && rotation.Angle != 0f) text += string.Format("Axis: {0}; Angle: {1}; ", rotation.Axis, MathUtil.RadiansToDegrees(rotation.Angle));
                    if (translation != Vector3.Zero) text += string.Format("Translation: {0}; ", translation);

                    return text;
                }
                else
                {
                    return "Bad transform matrix";
                }
            }
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
