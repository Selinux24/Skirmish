using System;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Effect variable
    /// </summary>
    public class EngineEffectVariable
    {
        /// <summary>
        /// Effect variable
        /// </summary>
        private readonly EffectVariable variable = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variable">Internal effect variable</param>
        public EngineEffectVariable(EffectVariable variable)
        {
            this.variable = variable;
        }

        /// <summary>
        /// Gets a value of the specified type from the variable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Returns a value of the specified type from the variable</returns>
        public T GetValue<T>() where T : struct, IBufferData
        {
            using (var ds = variable.GetRawValue(default(T).GetStride()))
            {
                ds.Position = 0;

                return ds.Read<T>();
            }
        }
        /// <summary>
        /// Gets a array of values of the specified type from the variable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="length">Array length</param>
        /// <returns>Returns a array of values of the specified type from the variable</returns>
        public T[] GetValue<T>(int length) where T : struct, IBufferData
        {
            using (var ds = variable.GetRawValue(default(T).GetStride() * length))
            {
                ds.Position = 0;

                return ds.ReadRange<T>(length);
            }
        }

        /// <summary>
        /// Sets a value of the specified type from the variable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value</param>
        public void SetValue<T>(T value) where T : struct, IBufferData
        {
            int sizeInBytes = default(T).GetStride();

            IntPtr ptr = Marshal.AllocHGlobal(sizeInBytes);
            Utilities.Write(ptr, ref value);
            variable.SetRawValue(ptr, 0, sizeInBytes);
            Marshal.FreeHGlobal(ptr);
        }
        /// <summary>
        /// Sets a array of values of the specified type from the variable
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="length">Length</param>
        public void SetValue<T>(T[] value, int length) where T : struct, IBufferData
        {
            int sizeInBytes = default(T).GetStride() * length;

            IntPtr ptr = Marshal.AllocHGlobal(sizeInBytes);
            Utilities.Write(ptr, value, 0, length);
            variable.SetRawValue(ptr, 0, sizeInBytes);
            Marshal.FreeHGlobal(ptr);
        }
    }
}
