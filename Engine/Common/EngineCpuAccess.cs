using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    [Flags]
    public enum EngineCpuAccess
    {
        /// <summary>
        /// The resource is to be mappable so that the CPU can change its contents.
        /// Resources created with this flag cannot be set as outputs of the pipeline and must be created with either dynamic or staging usage (see EngineResourceUsage).
        /// </summary>
        Write = CpuAccessFlags.Write,
        /// <summary>
        /// The resource is to be mappable so that the CPU can read its contents.
        /// Resources created with this flag cannot be set as either inputs or outputs to the pipeline and must be created with staging usage (see EngineResourceUsage).
        /// </summary>
        Read = CpuAccessFlags.Read,
        /// <summary>
        /// None
        /// </summary>
        None = CpuAccessFlags.None,
    }
}
