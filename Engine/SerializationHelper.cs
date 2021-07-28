using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SharpDX;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

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
        /// Serialize into file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        public static void SerializeToFile<T>(this T obj, string fileName, string nameSpace = null)
        {
            string extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".xml":
                    SerializeXmlToFile(obj, fileName, nameSpace);
                    break;
                case ".json":
                    SerializeJsonToFile(obj, fileName);
                    break;
                default:
                    SerializeBinaryToFile(obj, fileName);
                    break;
            }
        }
        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeFromFile<T>(string fileName, string nameSpace = null)
        {
            T result;

            string extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".xml":
                    result = DeserializeXmlFromFile<T>(fileName, nameSpace);
                    break;
                case ".json":
                    result = DeserializeJsonFromFile<T>(fileName);
                    break;
                default:
                    result = DeserializeBinaryFromFile<T>(fileName);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Creates a new binary formatter for serialization
        /// </summary>
        /// <remarks>Includes all the Serialization Surrogates for SharpDX native structs</remarks>
        private static IFormatter CreateBinaryFormatter()
        {
            var ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All), new Vector2SerializationSurrogate());
            ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), new Vector3SerializationSurrogate());
            ss.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), new Vector4SerializationSurrogate());
            ss.AddSurrogate(typeof(BoundingBox), new StreamingContext(StreamingContextStates.All), new BoundingBoxSerializationSurrogate());
            ss.AddSurrogate(typeof(Int3), new StreamingContext(StreamingContextStates.All), new Int3SerializationSurrogate());
            ss.AddSurrogate(typeof(Int4), new StreamingContext(StreamingContextStates.All), new Int4SerializationSurrogate());
            ss.AddSurrogate(typeof(Color3), new StreamingContext(StreamingContextStates.All), new Color3SerializationSurrogate());
            ss.AddSurrogate(typeof(Color4), new StreamingContext(StreamingContextStates.All), new Color4SerializationSurrogate());

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
        public static byte[] SerializeBinary<T>(this T obj)
        {
            using (var tmp = new MemoryStream())
            {
                CreateBinaryFormatter().Serialize(tmp, obj);
                tmp.Position = 0;

                return tmp.ToArray();
            }
        }
        /// <summary>
        /// Serialize into file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fileName">File name</param>
        public static void SerializeBinaryToFile<T>(this T obj, string fileName)
        {
            var data = SerializeBinary(obj);

            File.WriteAllBytes(fileName, data);
        }
        /// <summary>
        /// Deserialize from a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeBinary<T>(this byte[] data)
        {
            using (var buffer = new MemoryStream(data))
            {
                buffer.Position = 0;

                return (T)CreateBinaryFormatter().Deserialize(buffer);
            }
        }
        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeBinaryFromFile<T>(string fileName)
        {
            return DeserializeBinary<T>(File.ReadAllBytes(fileName));
        }

        /// <summary>
        /// Serialize into bytes
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns a byte array</returns>
        public static byte[] SerializeXml<T>(this T obj, string nameSpace = null)
        {
            byte[] data = null;

            MemoryStream mso = new MemoryStream();
            using (StreamWriter wr = new StreamWriter(mso, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                sr.Serialize(wr, obj);

                mso.Position = 0;

                data = mso.ToArray();
            }

            return data;
        }
        /// <summary>
        /// Serialize into file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        public static void SerializeXmlToFile<T>(this T obj, string fileName, string nameSpace = null)
        {
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                sr.Serialize(wr, obj);
            }
        }
        /// <summary>
        /// Deserialize from a byte array 
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeXml<T>(this byte[] data, string nameSpace = null)
        {
            using (MemoryStream mso = new MemoryStream(data))
            using (StreamReader rd = new StreamReader(mso, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                return (T)sr.Deserialize(rd);
            }
        }
        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeXmlFromFile<T>(string fileName, string nameSpace = null)
        {
            using (StreamReader rd = new StreamReader(fileName, Encoding.Default))
            {
                XmlSerializer sr = new XmlSerializer(typeof(T), nameSpace);

                return (T)sr.Deserialize(rd);
            }
        }

        /// <summary>
        /// Serialize into bytes
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Returns a byte array</returns>
        public static byte[] SerializeJson<T>(this T obj)
        {
            byte[] data = null;

            MemoryStream mso = new MemoryStream();
            using (StreamWriter wr = new StreamWriter(mso, Encoding.Default))
            {
                JsonSerializer sr = new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                sr.Serialize(wr, obj, typeof(T));

                mso.Position = 0;

                data = mso.ToArray();
            }

            return data;
        }
        /// <summary>
        /// Serialize into file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fileName">File name</param>
        public static void SerializeJsonToFile<T>(this T obj, string fileName)
        {
            using (StreamWriter wr = new StreamWriter(fileName, false, Encoding.Default))
            {
                JsonSerializer sr = new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                sr.Serialize(wr, obj, typeof(T));
            }
        }
        /// <summary>
        /// Deserialize from a byte array 
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeJson<T>(this byte[] data)
        {
            using (MemoryStream mso = new MemoryStream(data))
            using (StreamReader rd = new StreamReader(mso, Encoding.Default))
            {
                JsonSerializer sr = new JsonSerializer();

                return (T)sr.Deserialize(rd, typeof(T));
            }
        }
        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeJsonFromFile<T>(string fileName)
        {
            using (StreamReader rd = new StreamReader(fileName, Encoding.Default))
            {
                JsonSerializer sr = new JsonSerializer();

                return (T)sr.Deserialize(rd, typeof(T));
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
    /// Vector2 serialization surrogate
    /// </summary>
    sealed class Vector2SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Vector2)obj;
            info.AddValue("X", v.X);
            info.AddValue("Y", v.Y);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Vector2)obj;
            v.X = info.GetSingle("X");
            v.Y = info.GetSingle("Y");
            return v;
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
    /// <summary>
    /// Color3 serialization surrogate
    /// </summary>
    sealed class Color3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Color3)obj;
            info.AddValue("R", v.Red);
            info.AddValue("G", v.Green);
            info.AddValue("B", v.Blue);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Color3)obj;
            v.Red = info.GetSingle("R");
            v.Green = info.GetSingle("G");
            v.Blue = info.GetSingle("B");
            return v;
        }
    }
    /// <summary>
    /// Color4 serialization surrogate
    /// </summary>
    sealed class Color4SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            var v = (Color4)obj;
            info.AddValue("R", v.Red);
            info.AddValue("G", v.Green);
            info.AddValue("B", v.Blue);
            info.AddValue("A", v.Alpha);
        }
        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            var v = (Color4)obj;
            v.Red = info.GetSingle("R");
            v.Green = info.GetSingle("G");
            v.Blue = info.GetSingle("B");
            v.Alpha = info.GetSingle("A");
            return v;
        }
    }
}
