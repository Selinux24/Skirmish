using SharpDX.D3DCompiler;
using System.IO;

namespace Engine
{
    using Engine.Common;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Graphics effects management
    /// </summary>
    public sealed partial class Graphics
    {
        /// <summary>
        /// Loads an effect from byte code
        /// </summary>
        /// <param name="bytes">Byte code</param>
        /// <param name="profile">Compilation profile</param>
        /// <returns>Returns loaded effect</returns>
        public EngineEffect CompileEffect(byte[] bytes, string profile)
        {
            using (var includeManager = new ShaderIncludeManager())
            using (var cmpResult = ShaderBytecode.Compile(
                bytes,
                null,
                profile,
                ShaderFlags.EnableStrictness,
                EffectFlags.None,
                null,
                includeManager))
            {
                var effect = new Effect(
                    device,
                    cmpResult.Bytecode.Data,
                    EffectFlags.None);

                return new EngineEffect(effect);
            }
        }
        /// <summary>
        /// Loads an effect from pre-compiled file
        /// </summary>
        /// <param name="bytes">Pre-compiled byte code</param>
        /// <returns>Returns loaded effect</returns>
        public EngineEffect LoadEffect(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;

                using (var effectCode = ShaderBytecode.FromStream(ms))
                {
                    var effect = new Effect(
                        device,
                        effectCode.Data,
                        EffectFlags.None);

                    return new EngineEffect(effect);
                }
            }
        }
        /// <summary>
        /// Apply effect pass
        /// </summary>
        /// <param name="technique"></param>
        /// <param name="index"></param>
        /// <param name="flags"></param>
        public void EffectPassApply(EngineEffectTechnique technique, int index, int flags)
        {
            technique.GetPass(index).Apply(deviceContext, flags);
        }
    }
}
