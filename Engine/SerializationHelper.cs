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
        /// Creates a new binary formatter for serialization
        /// </summary>
        /// <remarks>Includes all the Serialization Surrogates for SharpDX native structs</remarks>
        private static IFormatter CreateBinaryFormatter()
        {
            var ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogate());
            ss.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), new Vector4SerializationSurrogate());
            ss.AddSurrogate(typeof(BoundingBox), new StreamingContext(StreamingContextStates.All), new BoundingBoxSerializationSurrogate());
            ss.AddSurrogate(typeof(Int3), new StreamingContext(StreamingContextStates.All), new Int3SerializationSurrogate());
            ss.AddSurrogate(typeof(Int4), new StreamingContext(StreamingContextStates.All), new Int4SerializationSurrogate());

            IFormatter formatter = new BinaryFormatter
            {
                SurrogateSelector = ss
            };

            return formatter;
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
                CreateBinaryFormatter().Serialize(tmp, obj);
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

                return (T)CreateBinaryFormatter().Deserialize(buffer);
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
                CreateBinaryFormatter().Serialize(tmp, obj);
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

                return (T)CreateBinaryFormatter().Deserialize(tmp);
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
    }

    /// <summary>
    /// Vector3 serialization surrogate
    /// </summary>
    sealed class Vector3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Vector3)obj;
            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
            info.AddValue("Z", v.Z);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Vector3)obj;
            v.X = info.GetSingle("X");
            v.Y = info.GetSingle("Y");
            v.Z = info.GetSingle("Z");
            return v;
        }
    }
    /// <summary>
    /// Vector4 serialization surrogate
    /// </summary>
    sealed class Vector4SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Vector4)obj;
            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
            info.AddValue("Z", v.Z);
            info.AddValue("W", v.W);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Vector4)obj;
            v.X = info.GetSingle("X");
            v.Y = info.GetSingle("Y");
            v.Z = info.GetSingle("Z");
            v.W = info.GetSingle("W");
            return v;
        }
    }
    /// <summary>
    /// Int3 serialization surrogate
    /// </summary>
    sealed class Int3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Int3)obj;
            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
            info.AddValue("Z", v.Z);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Int3)obj;
            v.X = info.GetInt32("X");
            v.Y = info.GetInt32("Y");
            v.Z = info.GetInt32("Z");
            return v;
        }
    }
    /// <summary>
    /// Int4 serialization surrogate
    /// </summary>
    sealed class Int4SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Int4)obj;
            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
            info.AddValue("Z", v.Z);
            info.AddValue("W", v.W);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Int4)obj;
            v.X = info.GetInt32("X");
            v.Y = info.GetInt32("Y");
            v.Z = info.GetInt32("Z");
            v.W = info.GetInt32("W");
            return v;
        }
    }
    /// <summary>
    /// BoundingBox serialization surrogate
    /// </summary>
    sealed class BoundingBoxSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var b = (BoundingBox)obj;
            info.AddValue("Minimum", b.Minimum);
            info.AddValue("Maximum", b.Maximum);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var b = (BoundingBox)obj;
            b.Minimum = info.GetValue<Vector3>("Minimum");
            b.Maximum = info.GetValue<Vector3>("Maximum");
            return b;
        }
    }
}
