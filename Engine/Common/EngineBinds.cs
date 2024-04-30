using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    [Flags]
    public enum EngineBinds
    {
        /// <summary>
        /// Bind a buffer as a vertex buffer to the input-assembler stage.
        /// </summary>
        VertexBuffer = BindFlags.VertexBuffer,
        /// <summary>
        /// Bind a buffer as an index buffer to the input-assembler stage.
        /// </summary>
        IndexBuffer = BindFlags.IndexBuffer,
        /// <summary>
        /// Bind a buffer as a constant buffer to a shader stage; this flag may NOT be combined with any other bind flag.
        /// </summary>
        ConstantBuffer = BindFlags.ConstantBuffer,
        /// <summary>
        /// Bind a buffer or texture to a shader stage; this flag cannot be used with the D3D11_MAP_WRITE_NO_OVERWRITE flag.
        /// Note: The Direct3D 11.1 runtime, which is available starting with Windows 8, enables mapping dynamic constant buffers and shader resource views (SRVs) of dynamic buffers with D3D11_MAP_WRITE_NO_OVERWRITE.
        /// The Direct3D 11 and earlier runtimes limited mapping to vertex or index buffers.
        /// To determine if a Direct3D device supports these features, call ID3D11Device::CheckFeatureSupport with D3D11_FEATURE_D3D11_OPTIONS.
        /// CheckFeatureSupport fills members of a SharpDX.Direct3D11.FeatureDataD3D11Options structure with the device's features.
        /// The relevant members here are MapNoOverwriteOnDynamicConstantBuffer and MapNoOverwriteOnDynamicBufferSRV.
        /// </summary>
        ShaderResource = BindFlags.ShaderResource,
        /// <summary>
        /// Bind an output buffer for the stream-output stage.
        /// </summary>
        StreamOutput = BindFlags.StreamOutput,
        /// <summary>
        /// Bind a texture as a render target for the output-merger stage.
        /// </summary>
        RenderTarget = BindFlags.RenderTarget,
        /// <summary>
        /// Bind a texture as a depth-stencil target for the output-merger stage.
        /// </summary>
        DepthStencil = BindFlags.DepthStencil,
        /// <summary>
        /// Bind an unordered access resource.
        /// </summary>
        UnorderedAccess = BindFlags.UnorderedAccess,
        /// <summary>
        /// Set this flag to indicate that a 2D texture is used to receive output from the decoder API.
        /// The common way to create resources for a decoder output is by calling the ID3D11Device::CreateTexture2D method to create an array of 2D textures.
        /// However, you cannot use texture arrays that are created with this flag in calls to ID3D11Device::CreateShaderResourceView.
        /// Direct3D 11: This value is not supported until Direct3D 11.1.
        /// </summary>
        Decoder = BindFlags.Decoder,
        /// <summary>
        /// Set this flag to indicate that a 2D texture is used to receive input from the video encoder API.
        /// The common way to create resources for a video encoder is by calling the ID3D11Device::CreateTexture2D method to create an array of 2D textures.
        /// However, you cannot use texture arrays that are created with this flag in calls to ID3D11Device::CreateShaderResourceView.
        /// Direct3D 11: This value is not supported until Direct3D 11.1.
        /// </summary>
        VideoEncoder = BindFlags.VideoEncoder,
        /// <summary>
        /// None
        /// </summary>
        None = BindFlags.None
    }
}
