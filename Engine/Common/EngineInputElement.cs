using System;

namespace Engine.Common
{
    using SharpDX.DXGI;

    /// <summary>
    /// Initializes a new instance of the Direct3D11 InputElement struct.
    /// </summary>
    /// <param name="name">The HLSL semantic associated with this element in a shader input-signature.</param>
    /// <param name="index">The semantic index for the element.</param>
    /// <param name="format">The data type of the element data.</param>
    /// <param name="offset">Offset (in bytes) between each element.</param>
    /// <param name="slot">An integer value that identifies the input-assembler.</param>
    /// <param name="slotClass">Identifies the input data class for a single input slot.</param>
    /// <param name="stepRate">The number of instances to draw using the same per-instance data before advancing in the buffer by one element.</param>
    public struct EngineInputElement(string name, int index, Format format, int offset, int slot, EngineInputClassification slotClass, int stepRate) : IEquatable<EngineInputElement>
    {
        /// <summary>
        /// The HLSL semantic associated with this element in a shader input-signature.
        /// </summary>
        public string SemanticName { get; set; } = name;
        /// <summary>
        /// The semantic index for the element. A semantic index modifies a semantic, with
        /// an integer index number. A semantic index is only needed in a case where there
        /// is more than one element with the same semantic. For example, a 4x4 matrix would
        /// have four components each with the semantic name matrix , however each of the
        /// four component would have different semantic indices (0, 1, 2, and 3).
        /// </summary>
        public int SemanticIndex { get; set; } = index;
        /// <summary>
        /// The data type of the element data. See SharpDX.DXGI.Format.
        /// </summary>
        public Format Format { get; set; } = format;
        /// <summary>
        /// An integer value that identifies the input-assembler (see input slot). Valid
        /// values are between 0 and 15, defined in D3D11.h.
        /// </summary>
        public int Slot { get; set; } = slot;
        /// <summary>
        /// Optional. Offset (in bytes) between each element. Use D3D11_APPEND_ALIGNED_ELEMENT
        /// for convenience to define the current element directly after the previous one,
        /// including any packing if necessary.
        /// </summary>
        public int AlignedByteOffset { get; set; } = offset;
        /// <summary>
        /// Identifies the input data class for a single input slot (see SharpDX.Direct3D11.InputClassification).
        /// </summary>
        public EngineInputClassification Classification { get; set; } = slotClass;
        /// <summary>
        /// The number of instances to draw using the same per-instance data before advancing
        /// in the buffer by one element. This value must be 0 for an element that contains
        /// per-vertex data (the slot class is set to D3D11_INPUT_PER_VERTEX_DATA).
        /// </summary>
        public int InstanceDataStepRate { get; set; } = stepRate;

        /// <inheritdoc/>
        public readonly bool Equals(EngineInputElement other)
        {
            if (Equals(other.SemanticName, SemanticName) &&
                other.SemanticIndex == SemanticIndex &&
                Equals(other.Format, Format) && other.Slot == Slot &&
                other.AlignedByteOffset == AlignedByteOffset &&
                Equals(other.Classification, Classification))
            {
                return other.InstanceDataStepRate == InstanceDataStepRate;
            }

            return false;
        }
        /// <inheritdoc/>
        public override readonly bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if ((object)obj.GetType() != typeof(EngineInputElement))
            {
                return false;
            }

            return Equals((EngineInputElement)obj);
        }
        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(SemanticName, SemanticIndex, Format, Slot, AlignedByteOffset, Classification, InstanceDataStepRate);
        }

        /// <inheritdoc/>
        public static bool operator ==(EngineInputElement left, EngineInputElement right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(EngineInputElement left, EngineInputElement right)
        {
            return !left.Equals(right);
        }
    }
}
