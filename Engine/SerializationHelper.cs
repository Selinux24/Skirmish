using SharpDX;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Engine
{
    /// <summary>
    /// Serialization helper
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// Compress
        /// </summary>
        /// <param name="stream">Stream to compress</param>
        /// <returns>Returns a compressed memory stream</returns>
        private static MemoryStream CompressStream(Stream stream)
        {
            var mso = new MemoryStream();

            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                stream.CopyTo(gs);
            }

            return mso;
        }
        /// <summary>
        /// Decompress
        /// </summary>
        /// <param name="compressed">Compressed stream</param>
        /// <returns>Returns a decompressed memory stream</returns>
        private static MemoryStream DecompressStream(Stream compressed)
        {
            var mso = new MemoryStream();

            using (var gs = new GZipStream(compressed, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            mso.Position = 0;

            return mso;
        }

        /// <summary>
        /// Serialize into bytes
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Returns a byte array</returns>
        public static byte[] Serialize<T>(this T obj)
        {
            using (var tmp = new MemoryStream())
            {
                new BinaryFormatter().Serialize(tmp, obj);
                tmp.Position = 0;

                return tmp.ToArray();
            }
        }
        /// <summary>
        /// Deserialize from a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <returns>Returns the deserialized object</returns>
        public static T Deserialize<T>(this byte[] data)
        {
            using (var buffer = new MemoryStream(data))
            {
                buffer.Position = 0;

                return (T)new BinaryFormatter().Deserialize(buffer);
            }
        }

        /// <summary>
        /// Compress the object to a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Returns a compressed byte array</returns>
        public static byte[] Compress<T>(this T obj)
        {
            using (var tmp = new MemoryStream())
            {
                new BinaryFormatter().Serialize(tmp, obj);
                tmp.Position = 0;

                using (var cmp = CompressStream(tmp))
                {
                    return cmp.ToArray();
                }
            }
        }
        /// <summary>
        /// Decompress from a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <returns>Returns the decompressed object</returns>
        public static T Decompress<T>(this byte[] data)
        {
            using (var buffer = new MemoryStream(data))
            using (var dec = DecompressStream(buffer))
            using (var tmp = new MemoryStream())
            {
                dec.CopyTo(tmp);
                tmp.Position = 0;

                return (T)new BinaryFormatter().Deserialize(tmp);
            }
        }

        /// <summary>
        /// Gets a value from a serialization info object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Object name</param>
        /// <returns>Returns the object</returns>
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        /// <summary>
        /// Adds a Vector3 instance to a serialization info object
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public static void AddVector3(this SerializationInfo info, string name, Vector3 value)
        {
            info.AddValue(string.Format("{0}.X", name), value.X);
            info.AddValue(string.Format("{0}.Y", name), value.Y);
            info.AddValue(string.Format("{0}.Z", name), value.Z);
        }
        /// <summary>
        /// Gets a Vector3 instance from a serialization info object
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Name</param>
        /// <returns>Returns a Vector3 instance</returns>
        public static Vector3 GetVector3(this SerializationInfo info, string name)
        {
            var vX = info.GetSingle(string.Format("{0}.X", name));
            var vY = info.GetSingle(string.Format("{0}.Y", name));
            var vZ = info.GetSingle(string.Format("{0}.Z", name));

            return new Vector3(vX, vY, vZ);
        }

        /// <summary>
        /// Adds a Int3 instance to a serialization info object
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public static void AddInt3(this SerializationInfo info, string name, Int3 value)
        {
            info.AddValue(string.Format("{0}.X", name), value.X);
            info.AddValue(string.Format("{0}.Y", name), value.Y);
            info.AddValue(string.Format("{0}.Z", name), value.Z);
        }
        /// <summary>
        /// Gets a Int3 instance from a serialization info object
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Name</param>
        /// <returns>Returns a Int3 instance</returns>
        public static Int3 GetInt3(this SerializationInfo info, string name)
        {
            var vX = info.GetInt32(string.Format("{0}.X", name));
            var vY = info.GetInt32(string.Format("{0}.Y", name));
            var vZ = info.GetInt32(string.Format("{0}.Z", name));

            return new Int3(vX, vY, vZ);
        }

        /// <summary>
        /// Adds a Int4 instance to a serialization info object
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public static void AddInt4(this SerializationInfo info, string name, Int4 value)
        {
            info.AddValue(string.Format("{0}.X", name), value.X);
            info.AddValue(string.Format("{0}.Y", name), value.Y);
            info.AddValue(string.Format("{0}.Z", name), value.Z);
            info.AddValue(string.Format("{0}.W", name), value.W);
        }
        /// <summary>
        /// Gets a Int4 instance from a serialization info object
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="name">Name</param>
        /// <returns>Returns a Int4 instance</returns>
        public static Int4 GetInt4(this SerializationInfo info, string name)
        {
            var vX = info.GetInt32(string.Format("{0}.X", name));
            var vY = info.GetInt32(string.Format("{0}.Y", name));
            var vZ = info.GetInt32(string.Format("{0}.Z", name));
            var vW = info.GetInt32(string.Format("{0}.W", name));

            return new Int4(vX, vY, vZ, vW);
        }
    }
}
