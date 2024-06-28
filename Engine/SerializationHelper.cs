using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
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
            MemoryStream mso = new();

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
            MemoryStream mso = new();

            using (var gs = new GZipStream(compressed, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            mso.Seek(0, SeekOrigin.Begin);

            return mso;
        }
        /// <summary>
        /// Compress the object to a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns a compressed byte array</returns>
        public static byte[] Compress<T>(this T obj, bool handleTypeNames = true)
        {
            using var tmp = obj.SerializeJsonToStream(handleTypeNames);
            using var cmp = CompressStream(tmp);

            return cmp.ToArray();
        }
        /// <summary>
        /// Decompress from a byte array
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns the decompressed object</returns>
        public static T Decompress<T>(this byte[] data, bool handleTypeNames = true)
        {
            using MemoryStream buffer = new(data);
            using var dec = DecompressStream(buffer);
            using MemoryStream tmp = new();

            dec.CopyTo(tmp);
            tmp.Seek(0, SeekOrigin.Begin);

            return DeserializeJsonFromStream<T>(tmp, handleTypeNames);
        }

        /// <summary>
        /// Serialize into file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        public static void SerializeToFile<T>(this T obj, string fileName, string nameSpace = null, bool handleTypeNames = true)
        {
            string extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".xml":
                    SerializeXmlToFile(obj, fileName, nameSpace);
                    break;
                default:
                    SerializeJsonToFile(obj, fileName, handleTypeNames);
                    break;
            }
        }
        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="nameSpace">Name space</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeFromFile<T>(string fileName, string nameSpace = null, bool handleTypeNames = true)
        {
            string extension = Path.GetExtension(fileName);
            var result = extension switch
            {
                ".xml" => DeserializeXmlFromFile<T>(fileName, nameSpace),
                _ => DeserializeJsonFromFile<T>(fileName, handleTypeNames),
            };
            return result;
        }

        /// <summary>
        /// Creates a new xml serializer
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="nameSpace">Default namespace</param>
        /// <returns>Returns a new xml serializer</returns>
        private static XmlSerializer CreateXmlFormatter(Type objectType, string nameSpace = null)
        {
            return new XmlSerializer(objectType, nameSpace);
        }
        /// <summary>
        /// Serialize into bytes
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Returns a byte array</returns>
        public static byte[] SerializeXml<T>(this T obj)
        {
            using var mso = SerializeXmlToStream(obj);

            return mso.ToArray();
        }
        /// <summary>
        /// Serialize into MemoryStream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns a MemoryStream</returns>
        public static MemoryStream SerializeXmlToStream<T>(this T obj, string nameSpace = null)
        {
            MemoryStream mso = new();

            using StreamWriter wr = new(mso, Encoding.Default, 4096, true);
            var sr = CreateXmlFormatter(typeof(T), nameSpace);
            sr.Serialize(wr, obj);
            wr.Flush();
            mso.Seek(0, SeekOrigin.Begin);

            return mso;
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
            using StreamWriter wr = new(fileName, false, Encoding.Default);

            var sr = CreateXmlFormatter(typeof(T), nameSpace);

            sr.Serialize(wr, obj);
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
            using MemoryStream mso = new(data);
            using StreamReader rd = new(mso, Encoding.Default);

            var sr = CreateXmlFormatter(typeof(T), nameSpace);

            return (T)sr.Deserialize(rd);
        }
        /// <summary>
        /// Deserialize from a Stream 
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="mso">Stream</param>
        /// <param name="nameSpace">Name space</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeXmlFromStream<T>(this Stream mso, string nameSpace = null)
        {
            using StreamReader rd = new(mso, Encoding.Default);

            var sr = CreateXmlFormatter(typeof(T), nameSpace);

            return (T)sr.Deserialize(rd);
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
            using StreamReader rd = new(fileName, Encoding.Default);

            var sr = CreateXmlFormatter(typeof(T), nameSpace);

            return (T)sr.Deserialize(rd);
        }

        /// <summary>
        /// Creates a new Json serializer for readers
        /// </summary>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns a Json serializer</returns>
        private static JsonSerializer CreateJsonFormatter(bool handleTypeNames)
        {
            return new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = handleTypeNames ? TypeNameHandling.Auto : TypeNameHandling.None,
            };
        }
        /// <summary>
        /// Serialize into bytes
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns a byte array</returns>
        public static byte[] SerializeJson<T>(this T obj, bool handleTypeNames = true)
        {
            using var mso = SerializeJsonToStream(obj, handleTypeNames);

            return mso.ToArray();
        }
        /// <summary>
        /// Serialize into MemoryStream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns a MemoryStream</returns>
        public static MemoryStream SerializeJsonToStream<T>(this T obj, bool handleTypeNames = true)
        {
            MemoryStream mso = new();

            using StreamWriter wr = new(mso, Encoding.Default, 4096, true);
            var sr = CreateJsonFormatter(handleTypeNames);
            sr.Serialize(wr, obj);
            wr.Flush();
            mso.Seek(0, SeekOrigin.Begin);

            return mso;
        }
        /// <summary>
        /// Serialize into file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="fileName">File name</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        public static void SerializeJsonToFile<T>(this T obj, string fileName, bool handleTypeNames = true)
        {
            using StreamWriter wr = new(fileName, false, Encoding.Default);

            var sr = CreateJsonFormatter(handleTypeNames);

            sr.Serialize(wr, obj, typeof(T));
        }
        /// <summary>
        /// Deserialize from a byte array 
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="data">Byte array</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeJson<T>(this byte[] data, bool handleTypeNames = true)
        {
            using MemoryStream mso = new(data);

            return DeserializeJsonFromStream<T>(mso, handleTypeNames);
        }
        /// <summary>
        /// Deserialize from a Stream 
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="mso">Stream</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeJsonFromStream<T>(this Stream mso, bool handleTypeNames = true)
        {
            using StreamReader rd = new(mso, Encoding.Default);

            var sr = CreateJsonFormatter(handleTypeNames);

            return (T)sr.Deserialize(rd, typeof(T));
        }
        /// <summary>
        /// Deserialize from a file
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="fileName">File name</param>
        /// <param name="handleTypeNames">Handle type names inheritance</param>
        /// <returns>Returns the deserialized object</returns>
        public static T DeserializeJsonFromFile<T>(string fileName, bool handleTypeNames = true)
        {
            using StreamReader rd = new(fileName, Encoding.Default);

            var sr = CreateJsonFormatter(handleTypeNames);

            return (T)sr.Deserialize(rd, typeof(T));
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
}
