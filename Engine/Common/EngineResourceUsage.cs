
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Identifies expected resource use during rendering. The usage directly reflects whether a resource is accessible by the CPU and/or the graphics processing unit (GPU).
    /// </summary>
    /// <remarks>
    /// An application identifies the way a resource is intended to be used (its usage) in a resource description.
    /// There are several structures for creating resources including: Direct3D11.Texture1DDescription, Direct3D11.Texture2DDescription, Direct3D11.Texture3DDescription, and Direct3D11.BufferDescription.
    /// Differences between Direct3D 9 and Direct3D 10/11:
    ///   In Direct3D 9, you specify the type of memory a resource should be created in at resource creation time (using D3DPOOL). It was an application's job to decide what memory pool would provide the best combination of functionality and performance.
    ///   In Direct3D 10/11, an application no longer specifies what type of memory (the pool) to create a resource in. Instead, you specify the intended usage of the resource, and let the runtime (in concert with the driver and a memory manager) choose the type of memory that will achieve the best performance.
    /// </remarks>
    public enum EngineResourceUsage
    {
        /// <summary>
        /// A resource that requires read and write access by the GPU. This is likely to be the most common usage choice.
        /// </summary>
        Default = ResourceUsage.Default,
        /// <summary>
        /// A resource that can only be read by the GPU. It cannot be written by the GPU, and cannot be accessed at all by the CPU.
        /// This type of resource must be initialized when it is created, since it cannot be changed after creation.
        /// </summary>
        Immutable = ResourceUsage.Immutable,
        /// <summary>
        /// A resource that is accessible by both the GPU (read only) and the CPU (write only).
        /// A dynamic resource is a good choice for a resource that will be updated by the CPU at least once per frame.
        /// To update a dynamic resource, use a Map method. For info about how to use dynamic resources, see How to: Use dynamic resources.
        /// </summary>
        Dynamic = ResourceUsage.Dynamic,
        /// <summary>
        /// A resource that supports data transfer (copy) from the GPU to the CPU.
        /// </summary>
        Staging = ResourceUsage.Staging
    }
}
