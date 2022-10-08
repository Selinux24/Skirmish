using SharpDX;

namespace Engine.Common
{
    using DXComparison = SharpDX.Direct3D11.Comparison;
    using DXFilter = SharpDX.Direct3D11.Filter;
    using DXSampleStateDescription = SharpDX.Direct3D11.SamplerStateDescription;
    using DXTextureAddressMode = SharpDX.Direct3D11.TextureAddressMode;

    /// <summary>
    /// Sampler state description
    /// </summary>
    public struct EngineSamplerStateDescription
    {
        public static explicit operator DXSampleStateDescription(EngineSamplerStateDescription obj)
        {
            return new DXSampleStateDescription
            {
                Filter = (DXFilter)obj.Filter,
                AddressU = (DXTextureAddressMode)obj.AddressU,
                AddressV = (DXTextureAddressMode)obj.AddressV,
                AddressW = (DXTextureAddressMode)obj.AddressW,
                MipLodBias = obj.MipLodBias,
                MaximumAnisotropy = obj.MaximumAnisotropy,
                ComparisonFunction = (DXComparison)obj.ComparisonFunction,
                BorderColor = obj.BorderColor,
                MinimumLod = obj.MinimumLod,
                MaximumLod = obj.MaximumLod,
            };
        }
        public static explicit operator EngineSamplerStateDescription(DXSampleStateDescription obj)
        {
            return new EngineSamplerStateDescription
            {
                Filter = (Filter)obj.Filter,
                AddressU = (TextureAddressMode)obj.AddressU,
                AddressV = (TextureAddressMode)obj.AddressV,
                AddressW = (TextureAddressMode)obj.AddressW,
                MipLodBias = obj.MipLodBias,
                MaximumAnisotropy = obj.MaximumAnisotropy,
                ComparisonFunction = (Comparison)obj.ComparisonFunction,
                BorderColor = obj.BorderColor,
                MinimumLod = obj.MinimumLod,
                MaximumLod = obj.MaximumLod,
            };
        }

        /// <summary>
        /// Returns default values for EngineSamplerStateDescription.
        /// </summary>
        /// <returns></returns>
        public static EngineSamplerStateDescription Default()
        {
            return (EngineSamplerStateDescription)DXSampleStateDescription.Default();
        }

        /// <summary>
        /// Filtering method to use when sampling a texture (see Filter).
        /// </summary>
        public Filter Filter { get; set; }
        /// <summary>
        /// Method to use for resolving a u texture coordinate that is outside the 0 to 1 range (see TextureAddressMode).
        /// </summary>
        public TextureAddressMode AddressU { get; set; }
        /// <summary>
        /// Method to use for resolving a v texture coordinate that is outside the 0 to 1 range.
        /// </summary>
        public TextureAddressMode AddressV { get; set; }
        /// <summary>
        /// Method to use for resolving a w texture coordinate that is outside the 0 to 1 range.
        /// </summary>
        public TextureAddressMode AddressW { get; set; }
        /// <summary>
        /// Offset from the calculated mipmap level. For example, if Direct3D calculates
        /// that a texture should be sampled at mipmap level 3 and MipLODBias is 2, then
        /// the texture will be sampled at mipmap level 5.
        /// </summary>
        public float MipLodBias { get; set; }
        /// <summary>
        /// Clamping value used if D3D11_FILTER_ANISOTROPIC or D3D11_FILTER_COMPARISON_ANISOTROPIC
        /// is specified in Filter. Valid values are between 1 and 16.
        /// </summary>
        public int MaximumAnisotropy { get; set; }
        /// <summary>
        /// A function that compares sampled data against existing sampled data. The function
        /// options are listed in SharpDX.Direct3D11.Comparison.
        /// </summary>
        public Comparison ComparisonFunction { get; set; }
        /// <summary>
        /// Border color to use if D3D11_TEXTURE_ADDRESS_BORDER is specified for AddressU,
        /// AddressV, or AddressW. Range must be between 0.0 and 1.0 inclusive.
        /// </summary>
        public Color4 BorderColor { get; set; }
        /// <summary>
        /// Lower end of the mipmap range to clamp access to, where 0 is the largest and
        /// most detailed mipmap level and any level higher than that is less detailed.
        /// </summary>
        public float MinimumLod { get; set; }
        /// <summary>
        /// Upper end of the mipmap range to clamp access to, where 0 is the largest and
        /// most detailed mipmap level and any level higher than that is less detailed. This
        /// value must be greater than or equal to MinLOD. To have no upper limit on LOD
        /// set this to a large value such as D3D11_FLOAT32_MAX.
        /// </summary>
        public float MaximumLod { get; set; }
    }

    /// <summary>
    /// Filtering options during texture sampling.
    ///
    /// Note??If you use different filter types for min versus mag filter, undefined
    /// behavior occurs in certain cases where the choice between whether magnification
    /// or minification happens is ambiguous. To prevent this undefined behavior, use
    /// filter modes that use similar filter operations for both min and mag (or use
    /// anisotropic filtering, which avoids the issue as well).?During texture sampling,
    /// one or more texels are read and combined (this is calling filtering) to produce
    /// a single value. Point sampling reads a single texel while linear sampling reads
    /// two texels (endpoints) and linearly interpolates a third value between the endpoints.HLSL
    /// texture-sampling functions also support comparison filtering during texture sampling.
    /// Comparison filtering compares each sampled texel against a comparison value.
    /// The boolean result is blended the same way that normal texture filtering is blended.You
    /// can use HLSL intrinsic texture-sampling functions that implement texture filtering
    /// only or companion functions that use texture filtering with comparison filtering.
    /// Texture Sampling FunctionTexture Sampling Function with Comparison Filtering
    /// samplesamplecmp or samplecmplevelzero ?Comparison filters only work with textures
    /// that have the following DXGI formats: R32_FLOAT_X8X24_TYPELESS, R32_FLOAT, R24_UNORM_X8_TYPELESS,
    /// R16_UNORM.
    /// </summary>
    public enum Filter
    {
        /// <summary>
        /// Use point sampling for minification, magnification, and mip-level sampling.
        /// </summary>
        MinMagMipPoint = DXFilter.MinMagMipPoint,
        /// <summary>
        /// Use point sampling for minification and magnification; 
        /// use linear interpolation for mip-level sampling.
        /// </summary>
        MinMagPointMipLinear = DXFilter.MinMagPointMipLinear,
        /// <summary>
        /// Use point sampling for minification; 
        /// use linear interpolation for magnification; 
        /// use point sampling for mip-level sampling.
        /// </summary>
        MinPointMagLinearMipPoint = DXFilter.MinPointMagLinearMipPoint,
        /// <summary>
        /// Use point sampling for minification; 
        /// use linear interpolation for magnification and mip-level sampling.
        /// </summary>
        MinPointMagMipLinear = DXFilter.MinPointMagMipLinear,
        /// <summary>
        /// Use linear interpolation for minification; 
        /// use point sampling for magnification and mip-level sampling.
        /// </summary>
        MinLinearMagMipPoint = DXFilter.MinLinearMagMipPoint,
        /// <summary>
        /// Use linear interpolation for minification; 
        /// use point sampling for magnification; 
        /// use linear interpolation for mip-level sampling.
        /// </summary>
        MinLinearMagPointMipLinear = DXFilter.MinLinearMagPointMipLinear,
        /// <summary>
        /// Use linear interpolation for minification and magnification; 
        /// use point sampling for mip-level sampling.
        /// </summary>
        MinMagLinearMipPoint = DXFilter.MinMagLinearMipPoint,
        /// <summary>
        /// Use linear interpolation for minification, magnification, and mip-level sampling.
        /// </summary>
        MinMagMipLinear = DXFilter.MinMagMipLinear,
        /// <summary>
        /// Use anisotropic interpolation for minification, magnification, and mip-level sampling.
        /// </summary>
        Anisotropic = DXFilter.Anisotropic,
        /// <summary>
        /// Use point sampling for minification, magnification, and mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagMipPoint = DXFilter.ComparisonMinMagMipPoint,
        /// <summary>
        /// Use point sampling for minification and magnification; 
        /// use linear interpolation for mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagPointMipLinear = DXFilter.ComparisonMinMagPointMipLinear,
        /// <summary>
        /// Use point sampling for minification; 
        /// use linear interpolation for magnification; 
        /// use point sampling for mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinPointMagLinearMipPoint = DXFilter.ComparisonMinPointMagLinearMipPoint,
        /// <summary>
        /// Use point sampling for minification; 
        /// use linear interpolation for magnification and mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinPointMagMipLinear = DXFilter.ComparisonMinPointMagMipLinear,
        /// <summary>
        /// Use linear interpolation for minification; 
        /// use point sampling for magnification and mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinLinearMagMipPoint = DXFilter.ComparisonMinLinearMagMipPoint,
        /// <summary>
        /// Use linear interpolation for minification; 
        /// use point sampling for magnification; 
        /// use linear interpolation for mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinLinearMagPointMipLinear = DXFilter.ComparisonMinLinearMagPointMipLinear,
        /// <summary>
        /// Use linear interpolation for minification and magnification; 
        /// use point sampling for mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagLinearMipPoint = DXFilter.ComparisonMinMagLinearMipPoint,
        /// <summary>
        /// Use linear interpolation for minification, magnification, and mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonMinMagMipLinear = DXFilter.ComparisonMinMagMipLinear,
        /// <summary>
        /// Use anisotropic interpolation for minification, magnification, and mip-level sampling. 
        /// Compare the result to the comparison value.
        /// </summary>
        ComparisonAnisotropic = DXFilter.ComparisonAnisotropic,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_MIP_POINT and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum. 
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinMagMipPoint = DXFilter.MinimumMinMagMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_POINT_MIP_LINEAR and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinMagPointMipLinear = DXFilter.MinimumMinMagPointMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_POINT_MAG_LINEAR_MIP_POINT and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinPointMagLinearMipPoint = DXFilter.MinimumMinPointMagLinearMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_POINT_MAG_MIP_LINEAR and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinPointMagMipLinear = DXFilter.MinimumMinPointMagMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_LINEAR_MAG_MIP_POINT and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinLinearMagMipPoint = DXFilter.MinimumMinLinearMagMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_LINEAR_MAG_POINT_MIP_LINEAR and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinLinearMagPointMipLinear = DXFilter.MinimumMinLinearMagPointMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinMagLinearMipPoint = DXFilter.MinimumMinMagLinearMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_MIP_LINEAR and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumMinMagMipLinear = DXFilter.MinimumMinMagMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_ANISOTROPIC and instead of filtering them return the minimum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the minimum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MinimumAnisotropic = DXFilter.MinimumAnisotropic,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_MIP_POINT and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinMagMipPoint = DXFilter.MaximumMinMagMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_POINT_MIP_LINEAR and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinMagPointMipLinear = DXFilter.MaximumMinMagPointMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_POINT_MAG_LINEAR_MIP_POINT and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinPointMagLinearMipPoint = DXFilter.MaximumMinPointMagLinearMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_POINT_MAG_MIP_LINEAR and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinPointMagMipLinear = DXFilter.MaximumMinPointMagMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_LINEAR_MAG_MIP_POINT and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinLinearMagMipPoint = DXFilter.MaximumMinLinearMagMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_LINEAR_MAG_POINT_MIP_LINEAR and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinLinearMagPointMipLinear = DXFilter.MaximumMinLinearMagPointMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinMagLinearMipPoint = DXFilter.MaximumMinMagLinearMipPoint,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_MIN_MAG_MIP_LINEAR and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumMinMagMipLinear = DXFilter.MaximumMinMagMipLinear,
        /// <summary>
        /// Fetch the same set of texels as D3D11_FILTER_ANISOTROPIC and instead of filtering them return the maximum of the texels.
        /// Texels that are weighted 0 during filtering aren't counted towards the maximum.
        /// You can query support for this filter type from the MinMaxFiltering member in the D3D11_FEATURE_D3D11_OPTIONS1 structure.
        /// </summary>
        MaximumAnisotropic = DXFilter.MaximumAnisotropic
    }

    /// <summary>
    /// Identify a technique for resolving texture coordinates that are outside of the boundaries of a texture.
    /// </summary>
    public enum TextureAddressMode
    {
        /// <summary>
        /// Tile the texture at every (u,v) integer junction. For example, for u values between 0 and 3, the texture is repeated three times.
        /// </summary>
        Wrap = DXTextureAddressMode.Wrap,
        /// <summary>
        /// Flip the texture at every (u,v) integer junction. For u values between 0 and
        /// 1, for example, the texture is addressed normally; between 1 and 2, the texture
        /// is flipped (mirrored); between 2 and 3, the texture is normal again; and so on.
        /// </summary>
        Mirror = DXTextureAddressMode.Mirror,
        /// <summary>
        /// Texture coordinates outside the range [0.0, 1.0] are set to the texture color
        /// at 0.0 or 1.0, respectively.
        /// </summary>
        Clamp = DXTextureAddressMode.Clamp,
        /// <summary>
        /// Texture coordinates outside the range [0.0, 1.0] are set to the border color
        /// specified in EngineSampleStateDescription or HLSL code.
        /// </summary>
        Border = DXTextureAddressMode.Border,
        /// <summary>
        /// Similar to D3D11_TEXTURE_ADDRESS_MIRROR and D3D11_TEXTURE_ADDRESS_CLAMP. Takes
        /// the absolute value of the texture coordinate (thus, mirroring around 0), and
        /// then clamps to the maximum value.
        /// </summary>
        MirrorOnce = DXTextureAddressMode.MirrorOnce
    }

    /// <summary>
    /// Comparison options.
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// Never pass the comparison.
        /// </summary>
        Never = DXComparison.Never,
        /// <summary>
        /// If the source data is less than the destination data, the comparison passes.
        /// </summary>
        Less = DXComparison.Less,
        /// <summary>
        /// If the source data is equal to the destination data, the comparison passes.
        /// </summary>
        Equal = DXComparison.Equal,
        /// <summary>
        /// If the source data is less than or equal to the destination data, the comparison passes.
        /// </summary>
        LessEqual = DXComparison.LessEqual,
        /// <summary>
        /// If the source data is greater than the destination data, the comparison passes.
        /// </summary>
        Greater = DXComparison.Greater,
        /// <summary>
        /// If the source data is not equal to the destination data, the comparison passes.
        /// </summary>
        NotEqual = DXComparison.NotEqual,
        /// <summary>
        /// If the source data is greater than or equal to the destination data, the comparison passes.
        /// </summary>
        GreaterEqual = DXComparison.GreaterEqual,
        /// <summary>
        /// Always pass the comparison.
        /// </summary>
        Always = DXComparison.Always
    }
}
