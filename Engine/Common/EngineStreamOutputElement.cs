
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Description of a vertex element in a vertex buffer in an output slot.
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public struct EngineStreamOutputElement(string semanticName, int semanticIndex, byte startComponent, byte componentCount, byte outputSlot)
    {
        public static explicit operator StreamOutputElement(EngineStreamOutputElement obj)
        {
            return new StreamOutputElement
            {
                Stream = obj.Stream,
                SemanticName = obj.SemanticName,
                SemanticIndex = obj.SemanticIndex,
                StartComponent = obj.StartComponent,
                ComponentCount = obj.ComponentCount,
                OutputSlot = obj.OutputSlot,
            };
        }
        public static explicit operator EngineStreamOutputElement(StreamOutputElement obj)
        {
            return new EngineStreamOutputElement
            {
                Stream = obj.Stream,
                SemanticName = obj.SemanticName,
                SemanticIndex = obj.SemanticIndex,
                StartComponent = obj.StartComponent,
                ComponentCount = obj.ComponentCount,
                OutputSlot = obj.OutputSlot,
            };
        }

        /// <summary>
        /// Zero-based, stream number.
        /// </summary>
        public int Stream { get; set; } = 0;
        /// <summary>
        /// Type of output element; possible values include: "POSITION", "NORMAL", or "TEXCOORD0".
        /// Note that if SemanticName is null then ComponentCount can be greater than 4 and the described entry will be a gap in the stream out where no data will be written.
        /// </summary>
        public string SemanticName { get; set; } = semanticName;
        /// <summary>
        /// Output element's zero-based index.
        /// Should be used if, for example, you have more than one texture coordinate stored in each vertex.
        /// </summary>
        public int SemanticIndex { get; set; } = semanticIndex;
        /// <summary>
        /// Which component of the entry to begin writing out to. Valid values are 0 to 3.
        /// For example, if you only wish to output to the y and z components of a position, then StartComponent should be 1 and ComponentCount should be 2.
        /// </summary>
        public byte StartComponent { get; set; } = startComponent;
        /// <summary>
        /// The number of components of the entry to write out to. Valid values are 1 to 4.
        /// For example, if you only wish to output to the y and z components of a position, then StartComponent should be 1 and ComponentCount should be 2.
        /// Note that if SemanticName is null then ComponentCount can be greater than 4 and the described entry will be a gap in the stream out where no data will be written.
        /// </summary>
        public byte ComponentCount { get; set; } = componentCount;
        /// <summary>
        /// The associated stream output buffer that is bound to the pipeline (see ID3D11DeviceContext::SOSetTargets).
        /// The valid range for OutputSlot is 0 to 3.
        /// </summary>
        public byte OutputSlot { get; set; } = outputSlot;
    }
}
