using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Transform NDC space [-1,+1]^2 to texture space [0,1]^2
        /// </summary>
        private static readonly Matrix ndcTransform = new Matrix(
            0.5f, 0.0f, 0.0f, 0.0f,
            0.0f, -0.5f, 0.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.0f, 1.0f);

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
            return (float)Math.Acos(dot);
        }
        /// <summary>
        /// Gets angle between two vectors in the same plane
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <param name="pn">Plane normal</param>
        /// <returns>Returns angle value</returns>
        /// <remarks>Result signed</remarks>
        public static float Angle(Vector3 one, Vector3 two, Vector3 pn)
        {
            Plane p = new Plane(pn, 0);

            float dot = MathUtil.Clamp(Vector3.Dot(Vector3.Normalize(one), Vector3.Normalize(two)), 0, 1);

            float angle = (float)Math.Acos(dot);

            Vector3 cross = Vector3.Cross(one, two);

            if (Vector3.Dot(p.Normal, cross) > 0)
            {
                angle = -angle;
            }

            return angle;
        }
        /// <summary>
        /// Gets total distance between point list
        /// </summary>
        /// <param name="points">Point list</param>
        /// <returns>Returns total distance between point list</returns>
        public static float Distance(params Vector3[] points)
        {
            float length = 0;

            Vector3 p0 = points[0];

            for (int i = 1; i < points.Length; i++)
            {
                Vector3 p1 = points[i];

                length += Vector3.Distance(p0, p1);

                p0 = p1;
            }

            return length;
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
            Vector3 up = Vector3.Up;

            if (Vector3.Dot(Vector3.Up, Vector3.Normalize(eyePosition - target)) == 1f)
            {
                up = Vector3.Left;
            }

            Quaternion q = Quaternion.Invert(Quaternion.LookAtLH(eyePosition, target, up));

            if (yAxisOnly)
            {
                q.X = 0;
                q.Z = 0;

                q.Normalize();
            }

            return q;
        }
        /// <summary>
        /// Set transform to normal device coordinates
        /// </summary>
        /// <param name="view">View matrix</param>
        /// <param name="projection">Projection matrix</param>
        /// <returns>Returns NDC matrix</returns>
        public static Matrix NormalDeviceCoordinatesTransform(Matrix view, Matrix projection)
        {
            return view * projection * ndcTransform;
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

                    translation.X = (float)Math.Round(translation.X, 0);
                    translation.Y = (float)Math.Round(translation.Y, 0);
                    translation.Z = (float)Math.Round(translation.Z, 0);

                    if (scale != Vector3.One) text += string.Format("Scale: {0}; ", scale);
                    if (!rotation.IsIdentity && rotation.Angle != 0f) text += string.Format("Axis: {0}; Angle: {1}; ", rotation.Axis, MathUtil.RadiansToDegrees(rotation.Angle));
                    if (translation != Vector3.Zero) text += string.Format("Translation: {0}; ", translation);

                    return text == "" ? "Near Identity" : text;
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
            }
        }
        /// <summary>
        /// Dispose disposable objects array
        /// </summary>
        /// <param name="array">Disposable objects array</param>
        public static void Dispose(IEnumerable<IDisposable> array)
        {
            if (array != null && array.Count() > 0)
            {
                foreach (var item in array)
                {
                    Helper.Dispose(item);
                }
            }
        }
        /// <summary>
        /// Dispose disposable objects dictionary
        /// </summary>
        /// <param name="dictionary">Disposable objects dictionary</param>
        public static void Dispose(IDictionary dictionary)
        {
            if (dictionary != null && dictionary.Count > 0)
            {
                foreach (var item in dictionary.Values)
                {
                    if (item is IDisposable)
                    {
                        Helper.Dispose((IDisposable)item);
                    }
                    else if (item is IEnumerable<IDisposable>)
                    {
                        Helper.Dispose((IEnumerable<IDisposable>)item);
                    }
                    else if (item is IDictionary)
                    {
                        Helper.Dispose((IDictionary)item);
                    }
                }

                dictionary.Clear();
            }
        }
        /// <summary>
        /// Gets the maximum value of the collection 
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="array">Array of parameters</param>
        /// <returns>Return the maximum value of the collection</returns>
        public static T Max<T>(params T[] array) where T : IComparable<T>
        {
            T res = default(T);

            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0)
                {
                    res = array[i];
                }
                else if (res.CompareTo(array[i]) < 0)
                {
                    res = array[i];
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the minimum value of the collection 
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="array">Array of parameters</param>
        /// <returns>Return the minimum value of the collection</returns>
        public static T Min<T>(params T[] array) where T : IComparable<T>
        {
            T res = default(T);

            for (int i = 0; i < array.Length; i++)
            {
                if (i == 0)
                {
                    res = array[i];
                }
                else if (res.CompareTo(array[i]) > 0)
                {
                    res = array[i];
                }
            }

            return res;
        }
        /// <summary>
        /// Concatenates the members of a collection of type T, using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T">Collection type</typeparam>
        /// <param name="list">Collection</param>
        /// <param name="separator">The string to use as a separator</param>
        /// <returns>A string that consists of the members of values delimited by the separator string</returns>
        public static string Join<T>(this ICollection<T> list, string separator = "")
        {
            List<string> res = new List<string>();

            list.ToList().ForEach(a => res.Add(a.ToString()));

            return string.Join(separator, res);
        }
        /// <summary>
        /// Performs distinc selection over the result of the provided function
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TKey">Function result type</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="getKey">Selection function</param>
        /// <returns>Returns a collection of distinct function results</returns>
        public static IEnumerable<TKey> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey)
        {
            Dictionary<TKey, TSource> dictionary = new Dictionary<TKey, TSource>();

            foreach (TSource item in source)
            {
                TKey key = getKey(item);

                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, item);
                }
            }

            return dictionary.Select(item => item.Key);
        }
        /// <summary>
        /// Gets next pair of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next pair</returns>
        public static int Pair(this int num)
        {
            return num = num * 0.5f != (int)(num * 0.5f) ? num + 1 : num;
        }
        /// <summary>
        /// Writes stream to memory
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns a memory stream</returns>
        public static MemoryStream WriteToMemory(this Stream stream)
        {
            MemoryStream ms = new MemoryStream();

            stream.CopyTo(ms);

            ms.Position = 0;

            return ms;
        }
        /// <summary>
        /// Writes file to memory
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns a memory stream</returns>
        public static MemoryStream WriteToMemory(this string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return stream.WriteToMemory();
            }
        }
        /// <summary>
        /// Generates a bounding box from a triangle list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the minimum bounding box that contains all the specified triangles</returns>
        public static BoundingBox CreateBoundingBox(Triangle[] triangles)
        {
            BoundingBox bbox = new BoundingBox();

            for (int i = 0; i < triangles.Length; i++)
            {
                BoundingBox tbox = BoundingBox.FromPoints(triangles[i].GetCorners());

                if (i == 0)
                {
                    bbox = tbox;
                }
                else
                {
                    bbox = BoundingBox.Merge(bbox, tbox);
                }
            }

            return bbox;
        }
        /// <summary>
        /// Generates a bounding sphere from a triangle list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the minimum bounding sphere that contains all the specified triangles</returns>
        public static BoundingSphere CreateBoundingSphere(Triangle[] triangles)
        {
            BoundingSphere bsph = new BoundingSphere();

            for (int i = 0; i < triangles.Length; i++)
            {
                BoundingSphere tsph = BoundingSphere.FromPoints(triangles[i].GetCorners());

                if (i == 0)
                {
                    bsph = tsph;
                }
                else
                {
                    bsph = BoundingSphere.Merge(bsph, tsph);
                }
            }

            return bsph;
        }
        /// <summary>
        /// Toggle coordinates from left-handed to right-handed and vice versa
        /// </summary>
        /// <typeparam name="T">Index type</typeparam>
        /// <param name="indices">Indices in a triangle list topology</param>
        /// <returns>Returns a new array</returns>
        public static T[] ChangeCoordinate<T>(T[] indices)
        {
            T[] res = new T[indices.Length];

            for (int i = 0; i < indices.Length; i += 3)
            {
                res[i + 0] = indices[i + 0];
                res[i + 1] = indices[i + 2];
                res[i + 2] = indices[i + 1];
            }

            return res;
        }
        /// <summary>
        /// Cast object as specified type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Object to cast as type</param>
        /// <returns>Return object casted as type</returns>
        public static T Cast<T>(this object input)
        {
            return (T)input;
        }
        /// <summary>
        /// Convert object to specified type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Object to convert to type</param>
        /// <returns>Return object converted to type</returns>
        public static T Convert<T>(this object input)
        {
            return (T)System.Convert.ChangeType(input, typeof(T));
        }
    }
}
