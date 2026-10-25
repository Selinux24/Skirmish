using System.Linq;
using System.Runtime.InteropServices;

namespace Engine.TextureReader
{
    static class Helper
    {
        /// <summary>
        /// Converts the byte array to a structure
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="bytes">Bytes</param>
        /// <param name="start">Start intex</param>
        /// <param name="count">Element count</param>
        /// <returns>Gets the generated struct</returns>
        public static T ToStructure<T>(this byte[] bytes, uint start, uint count) where T : struct
        {
            byte[] temp = [.. bytes.Skip((int)start).Take((int)count)];
            GCHandle handle = GCHandle.Alloc(temp, GCHandleType.Pinned);
            T stuff = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();
            return stuff;
        }
    }
}
