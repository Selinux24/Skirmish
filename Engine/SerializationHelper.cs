using SharpDX;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Engine
{
    public static class SerializationHelper
    {
        private static MemoryStream Compress(Stream stream)
        {
            var mso = new MemoryStream();

            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                stream.CopyTo(gs);
            }

            return mso;
        }
        private static MemoryStream Decompress(Stream compressed)
        {
            var mso = new MemoryStream();

            using (var gs = new GZipStream(compressed, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            mso.Position = 0;

            return mso;
        }

        public static byte[] Serialize(this object obj)
        {
            using (var tmp = new MemoryStream())
            {
                new BinaryFormatter().Serialize(tmp, obj);
                tmp.Position = 0;

                return tmp.ToArray();
            }
        }
        public static T Deserialize<T>(this byte[] data)
        {
            using (var buffer = new MemoryStream(data))
            {
                buffer.Position = 0;

                return (T)new BinaryFormatter().Deserialize(buffer);
            }
        }

        public static byte[] Compress(this object obj)
        {
            using (var tmp = new MemoryStream())
            {
                new BinaryFormatter().Serialize(tmp, obj);
                tmp.Position = 0;

                using (var cmp = Compress(tmp))
                {
                    return cmp.ToArray();
                }
            }
        }
        public static T Decompress<T>(this byte[] data)
        {
            using (var buffer = new MemoryStream(data))
            using (var dec = Decompress(buffer))
            using (var tmp = new MemoryStream())
            {
                dec.CopyTo(tmp);
                tmp.Position = 0;

                return (T)new BinaryFormatter().Deserialize(tmp);
            }
        }

        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        public static void AddVector3(this SerializationInfo info, string name, Vector3 value)
        {
            info.AddValue(string.Format("{0}.X", name), value.X);
            info.AddValue(string.Format("{0}.Y", name), value.Y);
            info.AddValue(string.Format("{0}.Z", name), value.Z);
        }
        public static Vector3 GetVector3(this SerializationInfo info, string name)
        {
            var vX = info.GetSingle(string.Format("{0}.X", name));
            var vY = info.GetSingle(string.Format("{0}.Y", name));
            var vZ = info.GetSingle(string.Format("{0}.Z", name));

            return new Vector3(vX, vY, vZ);
        }

        public static void AddInt3(this SerializationInfo info, string name, Int3 value)
        {
            info.AddValue(string.Format("{0}.X", name), value.X);
            info.AddValue(string.Format("{0}.Y", name), value.Y);
            info.AddValue(string.Format("{0}.Z", name), value.Z);
        }
        public static Int3 GetInt3(this SerializationInfo info, string name)
        {
            var vX = info.GetInt32(string.Format("{0}.X", name));
            var vY = info.GetInt32(string.Format("{0}.Y", name));
            var vZ = info.GetInt32(string.Format("{0}.Z", name));

            return new Int3(vX, vY, vZ);
        }

        public static void AddInt4(this SerializationInfo info, string name, Int4 value)
        {
            info.AddValue(string.Format("{0}.X", name), value.X);
            info.AddValue(string.Format("{0}.Y", name), value.Y);
            info.AddValue(string.Format("{0}.Z", name), value.Z);
            info.AddValue(string.Format("{0}.W", name), value.W);
        }
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
