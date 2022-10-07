using System;
using System.Linq;

namespace Engine.Common
{
    using DXBlendOperation = SharpDX.Direct3D11.BlendOperation;
    using DXBlendOption = SharpDX.Direct3D11.BlendOption;
    using DXBlendStateDescription = SharpDX.Direct3D11.BlendStateDescription1;
    using DXColorWriteMaskFlags = SharpDX.Direct3D11.ColorWriteMaskFlags;
    using DXLogicOperation = SharpDX.Direct3D11.LogicOperation;
    using DXRenderTargetBlendDescription = SharpDX.Direct3D11.RenderTargetBlendDescription1;

    /// <summary>
    /// Describes the blend state that you use in a call to CreateBlendState1 to create a blend-state object.
    /// </summary>
    /// <remarks>
    /// Here are the default values for blend state.
    /// StateDefault Value
    /// AlphaToCoverageEnable False
    /// IndependentBlendEnable False
    /// RenderTarget[0].BlendEnable False
    /// RenderTarget[0].LogicOpEnable False
    /// RenderTarget[0].SrcBlendD3D11_BLEND_ONE
    /// RenderTarget[0].DestBlendD3D11_BLEND_ZERO
    /// RenderTarget[0].BlendOpD3D11_BLEND_OP_ADD
    /// RenderTarget[0].SrcBlendAlphaD3D11_BLEND_ONE
    /// RenderTarget[0].DestBlendAlphaD3D11_BLEND_ZERO
    /// RenderTarget[0].BlendOpAlphaD3D11_BLEND_OP_ADD
    /// RenderTarget[0].LogicOpD3D11_LOGIC_OP_NOOP
    /// RenderTarget[0].RenderTargetWriteMaskD3D11_COLOR_WRITE_ENABLE_ALL
    /// If the driver type is set to D3D_DRIVER_TYPE_HARDWARE, the feature level is set to less than or equal to D3D_FEATURE_LEVEL_9_3, and the pixel format of the render target is set to DXGI_FORMAT_R8G8B8A8_UNORM_SRGB, DXGI_FORMAT_B8G8R8A8_UNORM_SRGB, or DXGI_FORMAT_B8G8R8X8_UNORM_SRGB, the display device performs the blend in standard RGB (sRGB) space and not in linear space.
    /// However, if the feature level is set to greater than D3D_FEATURE_LEVEL_9_3, the display device performs the blend in linear space, which is ideal.
    /// When you set the LogicOpEnable member of the first element of the RenderTarget array (RenderTarget[0]) to TRUE, you must also set the BlendEnable member of RenderTarget[0] to False, and the IndependentBlendEnable member of this BlendStateDescription1 to False.
    /// This reflects the limitation in hardware that you can't mix logic operations with blending across multiple render targets, and that when you use a logic operation, you must apply the same logic operation to all render targets.
    /// </remarks>
    public struct EngineBlendStateDescription
    {
        private const int MaxRenderTargetDescriptions = 8;

        public static explicit operator DXBlendStateDescription(EngineBlendStateDescription obj)
        {
            var blendState = DXBlendStateDescription.Default();

            blendState.AlphaToCoverageEnable = obj.AlphaToCoverageEnable;
            blendState.IndependentBlendEnable = obj.IndependentBlendEnable;

            if (obj.RenderTarget?.Any() == true)
            {
                int count = Math.Min(MaxRenderTargetDescriptions, obj.RenderTarget.Length);

                for (int i = 0; i < count; i++)
                {
                    blendState.RenderTarget[i] = (DXRenderTargetBlendDescription)obj.RenderTarget[i];
                }
            }

            return blendState;
        }
        public static explicit operator EngineBlendStateDescription(DXBlendStateDescription obj)
        {
            var blendState = new EngineBlendStateDescription
            {
                AlphaToCoverageEnable = obj.AlphaToCoverageEnable,
                IndependentBlendEnable = obj.IndependentBlendEnable,
                RenderTarget = new EngineRenderTargetBlendDescription[MaxRenderTargetDescriptions]
            };

            for (int i = 0; i < MaxRenderTargetDescriptions; i++)
            {
                blendState.RenderTarget[i] = (EngineRenderTargetBlendDescription)obj.RenderTarget[i];
            }

            return blendState;
        }

        /// <summary>
        /// Returns default values for BlendStateDescription1.
        /// </summary>
        public static EngineBlendStateDescription Default()
        {
            return (EngineBlendStateDescription)DXBlendStateDescription.Default();
        }

        /// <summary>
        /// Specifies whether to use alpha-to-coverage as a multisampling technique when setting a pixel to a render target. For more info about using alpha-to-coverage, see Alpha-To-Coverage.
        /// </summary>
        public bool AlphaToCoverageEnable { get; set; }
        /// <summary>
        /// Specifies whether to enable independent blending in simultaneous render targets. Set to TRUE to enable independent blending. If set to FALSE, only the RenderTarget[0] members are used; RenderTarget[1..7] are ignored.
        /// </summary>
        public bool IndependentBlendEnable { get; set; }
        /// <summary>
        /// An array of D3D11_RENDER_TARGET_BLEND_DESC structures that describe the blend states for render targets; these correspond to the eight render targets that can be bound to the output-merger stage at one time.
        /// </summary>
        public EngineRenderTargetBlendDescription[] RenderTarget { get; private set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        /// <remarks>
        /// Because this structure contains an array, it is not possible to modify it without making an explicit clone method.
        /// </remarks>
        public EngineBlendStateDescription Clone()
        {
            var blendState = Default();

            blendState.AlphaToCoverageEnable = AlphaToCoverageEnable;
            blendState.IndependentBlendEnable = IndependentBlendEnable;

            if (RenderTarget?.Any() == true)
            {
                int count = Math.Min(MaxRenderTargetDescriptions, RenderTarget.Length);

                for (int i = 0; i < count; i++)
                {
                    blendState.RenderTarget[i] = RenderTarget[i];
                }
            }

            return blendState;
        }
    }

    /// <summary>
    /// Describes the blend state for a render target.
    /// </summary>
    /// <remarks>
    /// You specify an array of RenderTargetBlendDescription1 structures in the RenderTarget member of the BlendStateDescription1 structure to describe the blend states for render targets; you can bind up to eight render targets to the output-merger stage at one time.
    /// For info about how blending is done, see the output-merger stage.Here are the default values for blend state.
    /// 
    /// StateDefault Value
    /// BlendEnableSharpDX.Result.False
    /// LogicOpEnableSharpDX.Result.False
    /// SrcBlendD3D11_BLEND_ONE
    /// DestBlendD3D11_BLEND_ZERO
    /// BlendOpD3D11_BLEND_OP_ADD
    /// SrcBlendAlphaD3D11_BLEND_ONE
    /// DestBlendAlphaD3D11_BLEND_ZERO
    /// BlendOpAlphaD3D11_BLEND_OP_ADD
    /// LogicOpD3D11_LOGIC_OP_NOOP
    /// RenderTargetWriteMaskD3D11_COLOR_WRITE_ENABLE_ALL
    /// </remarks>
    public struct EngineRenderTargetBlendDescription
    {
        public static explicit operator DXRenderTargetBlendDescription(EngineRenderTargetBlendDescription obj)
        {
            return new DXRenderTargetBlendDescription
            {
                IsBlendEnabled = obj.IsBlendEnabled,
                IsLogicOperationEnabled = obj.IsLogicOperationEnabled,
                SourceBlend = (DXBlendOption)obj.SourceBlend,
                DestinationBlend = (DXBlendOption)obj.DestinationBlend,
                BlendOperation = (DXBlendOperation)obj.BlendOperation,
                SourceAlphaBlend = (DXBlendOption)obj.SourceAlphaBlend,
                DestinationAlphaBlend = (DXBlendOption)obj.DestinationAlphaBlend,
                AlphaBlendOperation = (DXBlendOperation)obj.AlphaBlendOperation,
                LogicOperation = (DXLogicOperation)obj.LogicOperation,
                RenderTargetWriteMask = (DXColorWriteMaskFlags)obj.RenderTargetWriteMask,
            };
        }
        public static explicit operator EngineRenderTargetBlendDescription(DXRenderTargetBlendDescription obj)
        {
            return new EngineRenderTargetBlendDescription
            {
                IsBlendEnabled = obj.IsBlendEnabled,
                IsLogicOperationEnabled = obj.IsLogicOperationEnabled,
                SourceBlend = (BlendOption)obj.SourceBlend,
                DestinationBlend = (BlendOption)obj.DestinationBlend,
                BlendOperation = (BlendOperation)obj.BlendOperation,
                SourceAlphaBlend = (BlendOption)obj.SourceAlphaBlend,
                DestinationAlphaBlend = (BlendOption)obj.DestinationAlphaBlend,
                AlphaBlendOperation = (BlendOperation)obj.AlphaBlendOperation,
                LogicOperation = (LogicOperation)obj.LogicOperation,
                RenderTargetWriteMask = (ColorWriteMasks)obj.RenderTargetWriteMask,
            };
        }

        /// <summary>
        /// Enable (or disable) blending.
        /// </summary>
        public bool IsBlendEnabled { get; set; }
        /// <summary>
        /// Enable (or disable) a logical operation.
        /// </summary>
        public bool IsLogicOperationEnabled { get; set; }
        /// <summary>
        /// This blend option specifies the operation to perform on the RGB value that the pixel shader outputs.
        /// The BlendOp member defines how to combine the SrcBlend and DestBlend operations.
        /// </summary>
        public BlendOption SourceBlend { get; set; }
        /// <summary>
        /// This blend option specifies the operation to perform on the current RGB value in the render target.
        /// The BlendOp member defines how to combine the SrcBlend and DestBlend operations.
        /// </summary>
        public BlendOption DestinationBlend { get; set; }
        /// <summary>
        /// This blend operation defines how to combine the SrcBlend and DestBlend operations.
        /// </summary>
        public BlendOperation BlendOperation { get; set; }
        /// <summary>
        /// This blend option specifies the operation to perform on the alpha value that the pixel shader outputs.
        /// Blend options that end in _COLOR are not allowed. The BlendOpAlpha member defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
        /// </summary>
        public BlendOption SourceAlphaBlend { get; set; }
        /// <summary>
        /// This blend option specifies the operation to perform on the current alpha value in the render target.
        /// Blend options that end in _COLOR are not allowed.
        /// The BlendOpAlpha member defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
        /// </summary>
        public BlendOption DestinationAlphaBlend { get; set; }
        /// <summary>
        /// This blend operation defines how to combine the SrcBlendAlpha and DestBlendAlpha operations.
        /// </summary>
        public BlendOperation AlphaBlendOperation { get; set; }
        /// <summary>
        /// A LogicOperation-typed value that specifies the logical operation to configure for the render target.
        /// </summary>
        public LogicOperation LogicOperation { get; set; }
        /// <summary>
        /// A write mask.
        /// </summary>
        public ColorWriteMasks RenderTargetWriteMask { get; set; }
    }

    /// <summary>
    /// Blend factors, which modulate values for the pixel shader and render target.
    /// </summary>
    /// <remarks>
    /// Blend operations are specified in a blend description.
    /// </remarks>
    public enum BlendOption
    {
        /// <summary>
        /// The blend factor is (0, 0, 0, 0). No pre-blend operation.
        /// </summary>
        Zero = DXBlendOption.Zero,
        /// <summary>
        /// The blend factor is (1, 1, 1, 1). No pre-blend operation.
        /// </summary>
        One = DXBlendOption.One,
        /// <summary>
        /// The blend factor is (R?, G?, B?, A?), that is color data (RGB) from a pixel shader. 
        /// No pre-blend operation.
        /// </summary>
        SourceColor = DXBlendOption.SourceColor,
        /// <summary>
        /// The blend factor is (1 - R?, 1 - G?, 1 - B?, 1 - A?), that is color data (RGB) from a pixel shader.
        /// The pre-blend operation inverts the data, generating 1 - RGB.
        /// </summary>
        InverseSourceColor = DXBlendOption.InverseSourceColor,
        /// <summary>
        /// The blend factor is (A?, A?, A?, A?), that is alpha data (A) from a pixel shader.
        /// No pre-blend operation.
        /// </summary>
        SourceAlpha = DXBlendOption.SourceAlpha,
        /// <summary>
        /// The blend factor is ( 1 - A?, 1 - A?, 1 - A?, 1 - A?), that is alpha data (A) from a pixel shader.
        /// The pre-blend operation inverts the data, generating 1 - A.
        /// </summary>
        InverseSourceAlpha = DXBlendOption.InverseSourceAlpha,
        /// <summary>
        /// The blend factor is (Ad Ad Ad Ad), that is alpha data from a render target.
        /// No pre-blend operation.
        /// </summary>
        DestinationAlpha = DXBlendOption.DestinationAlpha,
        /// <summary>
        /// The blend factor is (1 - Ad 1 - Ad 1 - Ad 1 - Ad), that is alpha data from a render target.
        /// The pre-blend operation inverts the data, generating 1 - A.
        /// </summary>
        InverseDestinationAlpha = DXBlendOption.InverseDestinationAlpha,
        /// <summary>
        /// The blend factor is (Rd, Gd, Bd, Ad), that is color data from a render target.
        /// No pre-blend operation.
        /// </summary>
        DestinationColor = DXBlendOption.DestinationColor,
        /// <summary>
        /// The blend factor is (1 - Rd, 1 - Gd, 1 - Bd, 1 - Ad), that is color data from a render target.
        /// The pre-blend operation inverts the data, generating 1 - RGB.
        /// </summary>
        InverseDestinationColor = DXBlendOption.InverseDestinationColor,
        /// <summary>
        /// The blend factor is (f, f, f, 1); where f = min(A?, 1 - Ad).
        /// The pre-blend operation clamps the data to 1 or less.
        /// </summary>
        SourceAlphaSaturate = DXBlendOption.SourceAlphaSaturate,
        /// <summary>
        /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState.
        /// No pre-blend operation.
        /// </summary>
        BlendFactor = DXBlendOption.BlendFactor,
        /// <summary>
        /// The blend factor is the blend factor set with ID3D11DeviceContext::OMSetBlendState.
        /// The pre-blend operation inverts the blend factor, generating 1 - blend_factor.
        /// </summary>
        InverseBlendFactor = DXBlendOption.InverseBlendFactor,
        /// <summary>
        /// The blend factor is data sources both as color data output by a pixel shader.
        /// There is no pre-blend operation.
        /// This blend factor supports dual-source color blending.
        /// </summary>
        SecondarySourceColor = DXBlendOption.SecondarySourceColor,
        /// <summary>
        /// The blend factor is data sources both as color data output by a pixel shader.
        /// The pre-blend operation inverts the data, generating 1 - RGB.
        /// This blend factor supports dual-source color blending.
        /// </summary>
        InverseSecondarySourceColor = DXBlendOption.InverseSecondarySourceColor,
        /// <summary>
        /// The blend factor is data sources as alpha data output by a pixel shader.
        /// There is no pre-blend operation.
        /// This blend factor supports dual-source color blending.
        /// </summary>
        SecondarySourceAlpha = DXBlendOption.SecondarySourceAlpha,
        /// <summary>
        /// The blend factor is data sources as alpha data output by a pixel shader.
        /// The pre-blend operation inverts the data, generating 1 - A.
        /// This blend factor supports dual-source color blending.
        /// </summary>
        InverseSecondarySourceAlpha = DXBlendOption.InverseSecondarySourceAlpha
    }

    /// <summary>
    /// RGB or alpha blending operation.
    /// </summary>
    /// <remarks>
    /// The runtime implements RGB blending and alpha blending separately. Therefore, blend state requires separate blend operations for RGB data and alpha data.
    /// These blend operations are specified in a blend description. The two sources ?source 1 and source 2? are shown in the blending block diagram.
    /// Blend state is used by the output-merger stage to determine how to blend together two RGB pixel values and two alpha values.
    /// The two RGB pixel values and two alpha values are the RGB pixel value and alpha value that the pixel shader outputs and the RGB pixel value and alpha value already in the output render target.
    /// The blend option controls the data source that the blending stage uses to modulate values for the pixel shader, render target, or both.
    /// The blend operation controls how the blending stage mathematically combines these modulated values.
    /// </remarks>
    public enum BlendOperation
    {
        /// <summary>
        /// Add source 1 and source 2.
        /// </summary>
        Add = DXBlendOperation.Add,
        /// <summary>
        /// Subtract source 1 from source 2.
        /// </summary>
        Subtract = DXBlendOperation.Subtract,
        /// <summary>
        /// Subtract source 2 from source 1.
        /// </summary>
        ReverseSubtract = DXBlendOperation.ReverseSubtract,
        /// <summary>
        /// Find the minimum of source 1 and source 2.
        /// </summary>
        Minimum = DXBlendOperation.Minimum,
        /// <summary>
        /// Find the maximum of source 1 and source 2.
        /// </summary>
        Maximum = DXBlendOperation.Maximum
    }

    /// <summary>
    /// Note: This enumeration is supported by the Direct3D 11.1 runtime, which is available on Windows 8 and later operating systems.
    /// Specifies logical operations to configure for a render target.
    /// </summary>
    public enum LogicOperation
    {
        /// <summary>
        /// Clears the render target.
        /// </summary>
        Clear = DXLogicOperation.Clear,
        /// <summary>
        /// Sets the render target.
        /// </summary>
        Set = DXLogicOperation.Set,
        /// <summary>
        /// Copys the render target.
        /// </summary>
        Copy = DXLogicOperation.Copy,
        /// <summary>
        /// Performs an inverted-copy of the render target.
        /// </summary>
        CopyInverted = DXLogicOperation.CopyInverted,
        /// <summary>
        /// No operation is performed on the render target.
        /// </summary>
        Noop = DXLogicOperation.Noop,
        /// <summary>
        /// Inverts the render target.
        /// </summary>
        Invert = DXLogicOperation.Invert,
        /// <summary>
        /// Performs a logical AND operation on the render target.
        /// </summary>
        And = DXLogicOperation.And,
        /// <summary>
        /// Performs a logical NAND operation on the render target.
        /// </summary>
        Nand = DXLogicOperation.Nand,
        /// <summary>
        /// Performs a logical OR operation on the render target.
        /// </summary>
        Or = DXLogicOperation.Or,
        /// <summary>
        /// Performs a logical NOR operation on the render target.
        /// </summary>
        Nor = DXLogicOperation.Nor,
        /// <summary>
        /// Performs a logical XOR operation on the render target.
        /// </summary>
        Xor = DXLogicOperation.Xor,
        /// <summary>
        /// Performs a logical equal operation on the render target.
        /// </summary>
        Equiv = DXLogicOperation.Equiv,
        /// <summary>
        /// Performs a logical AND and reverse operation on the render target.
        /// </summary>
        AndReverse = DXLogicOperation.AndReverse,
        /// <summary>
        /// Performs a logical AND and invert operation on the render target.
        /// </summary>
        AndInverted = DXLogicOperation.AndInverted,
        /// <summary>
        /// Performs a logical OR and reverse operation on the render target.
        /// </summary>
        OrReverse = DXLogicOperation.OrReverse,
        /// <summary>
        /// Performs a logical OR and invert operation on the render target.
        /// </summary>
        OrInverted = DXLogicOperation.OrInverted
    }

    [Flags]
    public enum ColorWriteMasks : byte
    {
        /// <summary>
        /// Allow data to be stored in the red component.
        /// </summary>
        Red = DXColorWriteMaskFlags.Red,
        /// <summary>
        /// Allow data to be stored in the green component.
        /// </summary>
        Green = DXColorWriteMaskFlags.Green,
        /// <summary>
        /// Allow data to be stored in the blue component.
        /// </summary>
        Blue = DXColorWriteMaskFlags.Blue,
        /// <summary>
        /// Allow data to be stored in the alpha component.
        /// </summary>
        Alpha = DXColorWriteMaskFlags.Alpha,
        /// <summary>
        /// Allow data to be stored in all components.
        /// </summary>
        All = DXColorWriteMaskFlags.All
    }
}
