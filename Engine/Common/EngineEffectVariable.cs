
namespace Engine.Common
{
    using SharpDX;
    using SharpDX.Direct3D11;

    class EngineEffectVariable
    {
        private EffectVariable variable = null;

        public EngineEffectVariable(EffectVariable variable)
        {
            this.variable = variable;
        }

        public T GetValue<T>() where T : struct, IBufferData
        {
            using (var ds = this.variable.GetRawValue(default(T).GetStride()))
            {
                ds.Position = 0;

                return ds.Read<T>();
            }
        }
        public T[] GetValue<T>(int length) where T : struct, IBufferData
        {
            using (var ds = this.variable.GetRawValue(default(T).GetStride() * length))
            {
                ds.Position = 0;

                return ds.ReadRange<T>(length);
            }
        }

        public void SetValue<T>(T value) where T : struct, IBufferData
        {
            using (var ds = DataStream.Create<T>(new T[] { value }, true, false))
            {
                ds.Position = 0;

                this.variable.SetRawValue(ds, default(T).GetStride());
            }
        }
        public void SetValue<T>(T[] value, int length) where T : struct, IBufferData
        {
            using (var ds = DataStream.Create<T>(value, true, false))
            {
                ds.Position = 0;

                this.variable.SetRawValue(ds, default(T).GetStride() * length);
            }
        }
    }
}
