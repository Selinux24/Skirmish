using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Helper functions
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// One radian
        /// </summary>
        public const float Radian = 0.0174532924f;

        /// <summary>
        /// Random number generator
        /// </summary>
        public static readonly Random RandomGenerator = new Random();

        #region Memory

        /// <summary>
        /// Swaps values
        /// </summary>
        /// <typeparam name="T">Type of values</typeparam>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }
        /// <summary>
        /// Converts the byte array to a structure
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="bytes">Bytes</param>
        /// <param name="start">Start intex</param>
        /// <param name="count">Element count</param>
        /// <returns>Gets the generated struct</returns>
        public static T ToStructure<T>(this byte[] bytes, int start, int count) where T : struct
        {
            byte[] temp = bytes.Skip(start).Take(count).ToArray();
            GCHandle handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serializes the specified object to XML file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        public static void SerializeToFile<T>(T obj, string fileName, string nameSpace = null)
        {
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                sr.Serialize(wr, obj);
            }
        }
        /// <summary>
        /// Deserializes an object from a XML file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeFromFile<T>(string fileName, string nameSpace = null)
        {
            using (StreamReader rd = new StreamReader(fileName, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                return (T)sr.Deserialize(rd);
            }
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
        /// Create the md5 sum string of the specified buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returns the md5 sum string of the specified buffer</returns>
        public static string GetMd5Sum(this byte[] buffer)
        {
            byte[] result = null;
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                result = md5.ComputeHash(buffer);
            }

            StringBuilder sb = new StringBuilder();
            Array.ForEach(result, r => sb.Append(r.ToString("X2")));
            return sb.ToString();
        }
        /// <summary>
        /// Create the md5 sum string of the specified string
        /// </summary>
        /// <param name="content">String</param>
        /// <returns>Returns the md5 sum string of the specified string</returns>
        public static string GetMd5Sum(this string content)
        {
            byte[] tmp = new byte[content.Length * 2];
            Encoding.Unicode.GetEncoder().GetBytes(content.ToCharArray(), 0, content.Length, tmp, 0, true);

            return tmp.GetMd5Sum();
        }
        /// <summary>
        /// Create the md5 sum string of the specified string list
        /// </summary>
        /// <param name="content">String list</param>
        /// <returns>Returns the md5 sum string of the specified string</returns>
        public static string GetMd5Sum(this string[] content)
        {
            string md5 = null;
            Array.ForEach(content, p => md5 += p.GetMd5Sum());

            return md5;
        }
        /// <summary>
        /// Create the md5 sum string of the specified stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Returns the md5 sum string of the specified stream</returns>
        public static string GetMd5Sum(this MemoryStream stream)
        {
            return stream.ToArray().GetMd5Sum();
        }
        /// <summary>
        /// Create the md5 sum string of the specified stream list
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the md5 sum string of the specified stream list</returns>
        public static string GetMd5Sum(this MemoryStream[] streams)
        {
            string md5 = null;
            Array.ForEach(streams, p => md5 += p.GetMd5Sum());

            return md5;
        }

        #endregion

        #region Array Utils

        /// <summary>
        /// Generate an array initialized to defaultValue
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>Returns array</returns>
        public static T[] CreateArray<T>(int length, T defaultValue) where T : struct
        {
            T[] array = new T[length];

            InitializeArray(array, defaultValue);

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

            InitializeArray(array, func);

            return array;
        }
        /// <summary>
        /// Initializes the specified array with value
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value to set to all array elements</param>
        public static void InitializeArray<T>(this T[] arr, T value) where T : struct
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }
        /// <summary>
        /// Initializes the specified array with function result
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="func">Function</param>
        public static void InitializeArray<T>(this T[] arr, Func<T> func)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = func.Invoke();
            }
        }
        /// <summary>
        /// Merge two arrays
        /// </summary>
        /// <typeparam name="T">Type of array</typeparam>
        /// <param name="array1">First array</param>
        /// <param name="array2">Second array</param>
        /// <returns>Returns an array with both array values merged</returns>
        public static T[] Merge<T>(this T[] array1, T[] array2)
        {
            T[] newArray = new T[array1.Length + array2.Length];

            array1.CopyTo(newArray, 0);
            array2.CopyTo(newArray, array1.Length);

            return newArray;
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
        /// Gets the next index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the next index</returns>
        public static int Next(int i, int length)
        {
            return i + 1 < length ? i + 1 : 0;
        }
        /// <summary>
        /// Gets the previous index value in a fixed length array
        /// </summary>
        /// <param name="i">Current index</param>
        /// <param name="length">Array length</param>
        /// <returns>Returns the previous index</returns>
        public static int Prev(int i, int length)
        {
            return i - 1 >= 0 ? i - 1 : length - 1;
        }
        /// <summary>
        /// Gets next pair of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next pair</returns>
        public static int NextPair(this int num)
        {
            return num = num * 0.5f != (int)(num * 0.5f) ? num + 1 : num;
        }
        /// <summary>
        /// Gets next odd of even number, if even
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Returns next odd</returns>
        public static int NextOdd(this int num)
        {
            return num = num * 0.5f != (int)(num * 0.5f) ? num : num + 1;
        }
        /// <summary>
        /// Calculates the next highest power of two.
        /// </summary>
        /// <remarks>
        /// This is a minimal method meant to be fast. There is a known edge case where an input of 0 will output 0
        /// instead of the mathematically correct value of 1. It will not be fixed.
        /// </remarks>
        /// <param name="v">A value.</param>
        /// <returns>The next power of two after the value.</returns>
        public static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
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
        /// Removes the last item of the list
        /// </summary>
        /// <typeparam name="T">List type</typeparam>
        /// <param name="list">The list</param>
        /// <returns>Returns the removed item</returns>
        public static T Pop<T>(this List<T> list)
        {
            int index = list.Count - 1;
            T value = list[index];
            list.RemoveAt(index);
            return value;
        }
        /// <summary>
        /// Compares two enumerable lists, element by element
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="enum1">First list</param>
        /// <param name="enum2">Second list</param>
        /// <returns>Returns true if both list are equal</returns>
        public static bool ListIsEqual<T>(this IEnumerable<T> enum1, IEnumerable<T> enum2)
        {
            if (enum1 == null && enum2 == null)
            {
                return true;
            }
            else if (enum1 != null && enum2 != null)
            {
                var list1 = enum1.ToList();
                var list2 = enum2.ToList();

                if (list1.Count == list2.Count)
                {
                    if (list1.Count > 0)
                    {
                        bool equatable = list1[0] is IEquatable<T>;

                        for (int i = 0; i < list1.Count; i++)
                        {
                            bool equal = false;

                            if (equatable)
                            {
                                equal = ((IEquatable<T>)list1[i]).Equals(list2[i]);
                            }
                            else
                            {
                                equal = list1[i].Equals(list2[i]);
                            }

                            if (!equal) return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Arithmetic

        /// <summary>
        /// Performs cross product between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns the cross product</returns>
        public static float Cross(Vector2 one, Vector2 two)
        {
            return one.X * two.Y - one.Y * two.X;
        }
        /// <summary>
        /// Gets angle between two vectors
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns angle value in radians</returns>
        public static float Angle(Vector2 one, Vector2 two)
        {
            //Get the dot product
            float dot = Vector2.Dot(one, two);

            // Divide the dot by the product of the magnitudes of the vectors
            dot = dot / (one.Length() * two.Length());

            //Get the arc cosin of the angle, you now have your angle in radians 
            return (float)Math.Acos(dot);
        }
        /// <summary>
        /// Gets angle between two quaternions
        /// </summary>
        /// <param name="one">First quaternions</param>
        /// <param name="two">Second quaternions</param>
        /// <returns>Returns angle value in radians</returns>
        public static float Angle(Quaternion one, Quaternion two)
        {
            float dot = Quaternion.Dot(one, two);

            return (float)Math.Acos(Math.Min(Math.Abs(dot), 1f)) * 2f;
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
        /// <param name="planeNormal">Plane normal</param>
        /// <returns>Returns angle value</returns>
        /// <remarks>Result signed</remarks>
        public static float Angle(Vector3 one, Vector3 two, Vector3 planeNormal)
        {
            Plane p = new Plane(planeNormal, 0);

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
        /// Gets angle between two vectors with sign
        /// </summary>
        /// <param name="one">First vector</param>
        /// <param name="two">Second vector</param>
        /// <returns>Returns angle value</returns>
        public static float AngleSigned(Vector2 one, Vector2 two)
        {
            return (float)Math.Atan2(Cross(one, two), Vector2.Dot(one, two)) * MathUtil.Pi;
        }
        /// <summary>
        /// Gets yaw and pitch values from vector
        /// </summary>
        /// <param name="vec">Vector</param>
        /// <param name="yaw">Yaw</param>
        /// <param name="pitch">Pitch</param>
        public static void GetAnglesFromVector(Vector3 vec, out float yaw, out float pitch)
        {
            yaw = (float)Math.Atan2(vec.X, vec.Y);

            if (yaw < 0.0f)
            {
                yaw += MathUtil.TwoPi;
            }

            if (Math.Abs(vec.X) > Math.Abs(vec.Y))
            {
                pitch = (float)Math.Atan2(Math.Abs(vec.Z), Math.Abs(vec.X));
            }
            else
            {
                pitch = (float)Math.Atan2(Math.Abs(vec.Z), Math.Abs(vec.Y));
            }

            if (vec.Z < 0.0f)
            {
                pitch = -pitch;
            }
        }
        /// <summary>
        /// Get vector from yaw and pitch angles
        /// </summary>
        /// <param name="yaw">Yaw angle</param>
        /// <param name="pitch">Pitch angle</param>
        /// <param name="vec">Vector</param>
        public static void GetVectorFromAngles(float yaw, float pitch, out Vector3 vec)
        {
            Quaternion rot = Quaternion.RotationYawPitchRoll(-yaw, pitch, 0.0f);
            Matrix mat = Matrix.RotationQuaternion(rot);

            vec = Vector3.TransformCoordinate(Vector3.Up, mat);
        }

        #endregion

        #region Transforms

        /// <summary>
        /// Creates a new world Matrix
        /// </summary>
        /// <param name="position">The position vector.</param>
        /// <param name="forward">The forward direction vector.</param>
        /// <param name="up">The upward direction vector. Usually <see cref="Vector3.Up"/>.</param>
        /// <returns>The world Matrix</returns>
        public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix result = new Matrix();

            Vector3.Normalize(ref forward, out Vector3 z);
            Vector3.Cross(ref forward, ref up, out Vector3 x);
            Vector3.Cross(ref x, ref forward, out Vector3 y);
            x.Normalize();
            y.Normalize();

            result.Right = x;
            result.Up = y;
            result.Forward = z;
            result.TranslationVector = position;
            result.M44 = 1f;

            return result;
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="yAxisOnly">Restricts the rotation axis to Y only</param>
        /// <returns>Returns rotation quaternion</returns>
        public static Quaternion LookAt(Vector3 eyePosition, Vector3 target, Vector3 up, bool yAxisOnly = true)
        {
            if (Vector3.Dot(Vector3.Up, Vector3.Normalize(eyePosition - target)) == 1f)
            {
                up = Vector3.Left;
            }

            Quaternion q = Quaternion.Invert(Quaternion.LookAtLH(target, eyePosition, up));

            if (yAxisOnly)
            {
                q.X = 0;
                q.Z = 0;

                q.Normalize();
            }

            return q;
        }
        /// <summary>
        /// Finds the quaternion between from and to quaternions traveling maxDelta radians
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        /// <param name="maxDelta">Maximum radians</param>
        /// <returns>Gets the quaternion between from and to quaternions traveling maxDelta radians</returns>
        public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDelta)
        {
            float angle = Helper.Angle(from, to);
            if (angle == 0f)
            {
                return to;
            }

            float delta = Math.Min(1f, maxDelta / angle);

            return Quaternion.Slerp(from, to, delta);
        }
        /// <summary>
        /// Gets the screen coordinates from the specified 3D point
        /// </summary>
        /// <param name="point">3D point</param>
        /// <param name="viewPort">View port</param>
        /// <param name="wvp">World * View * Projection</param>
        /// <param name="isInsideScreen">Returns true if the resulting point is inside the screen</param>
        /// <returns>Returns the resulting screen coordinates</returns>
        public static Vector2 UnprojectToScreen(Vector3 point, ViewportF viewPort, Matrix wvp, out bool isInsideScreen)
        {
            isInsideScreen = true;

            // Go to projection space
            Vector3.Transform(ref point, ref wvp, out Vector4 projected);

            // Clip
            // 
            //  -Wp < Xp <= Wp 
            //  -Wp < Yp <= Wp 
            //  0 < Zp <= Wp 
            // 
            if (projected.X < -projected.W)
            {
                projected.X = -projected.W;
                isInsideScreen = false;
            }
            if (projected.X > projected.W)
            {
                projected.X = projected.W;
                isInsideScreen = false;
            }
            if (projected.Y < -projected.W)
            {
                projected.Y = -projected.W;
                isInsideScreen = false;
            }
            if (projected.Y > projected.W)
            {
                projected.Y = projected.W;
                isInsideScreen = false;
            }
            if (projected.Z < 0)
            {
                projected.Z = 0;
                isInsideScreen = false;
            }
            if (projected.Z > projected.W)
            {
                projected.Z = projected.W;
                isInsideScreen = false;
            }

            // Divide by w, to move from homogeneous coordinates to 3D coordinates again 
            projected.X = projected.X / projected.W;
            projected.Y = projected.Y / projected.W;
            projected.Z = projected.Z / projected.W;

            // Perform the viewport scaling, to get the appropiate coordinates inside the viewport 
            projected.X = ((float)(((projected.X + 1.0) * 0.5) * viewPort.Width)) + viewPort.X;
            projected.Y = ((float)(((1.0 - projected.Y) * 0.5) * viewPort.Height)) + viewPort.Y;
            projected.Z = (projected.Z * (viewPort.MaxDepth - viewPort.MinDepth)) + viewPort.MinDepth;

            return projected.XY();
        }

        #endregion

        #region Debug Utils

        /// <summary>
        /// Gets matrix description
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <returns>Return matrix description</returns>
        public static string GetDescription(this Matrix matrix)
        {
            if (matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation))
            {
                return string.Format("S:{0,-30} T:{1,-30} R:{2,-30}",
                    scale.GetDescription(Vector3.One, "None"),
                    translation.GetDescription(Vector3.Zero, "Zero"),
                    rotation.GetDescription());
            }
            else
            {
                return "Bad transform matrix";
            }
        }
        /// <summary>
        /// Gets quaternion description
        /// </summary>
        /// <param name="quaternion">Quaternion</param>
        /// <returns>Return quaternion description</returns>
        public static string GetDescription(this Quaternion quaternion)
        {
            Vector3 axis = quaternion.Axis;
            float angle = quaternion.Angle;

            return string.Format("Angle: {0:0.00} in axis {1}", angle, axis.GetDescription(Vector3.One, "None"));
        }
        /// <summary>
        /// Gets vector description
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="none">Sets the vector who means wath</param>
        /// <param name="wath">Sets the string to write in description when the specified vector is near equal to none</param>
        /// <returns>Return vector description</returns>
        public static string GetDescription(this Vector3 vector, Vector3 none, string wath)
        {
            vector.X = (float)Math.Round(vector.X, 3);
            vector.Y = (float)Math.Round(vector.Y, 3);
            vector.Z = (float)Math.Round(vector.Z, 3);

            return string.Format("X:{0:0.000}; Y:{1:0.000}; Z:{2:0.000}", vector.X, vector.Y, vector.Z);
        }

        #endregion
    }
}
