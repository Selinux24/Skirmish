using System;

namespace Engine.Geometry
{
    /// <summary>
    /// A <see cref="RegionId"/> is an identifier with flags marking borders.
    /// </summary>
    [Serializable]
    public struct RegionId : IEquatable<RegionId>, IEquatable<int>
    {
        /// <summary>
        /// A bitmask 
        /// </summary>
        public const int MaskId = 0x1fffffff;

        /// <summary>
        /// A null region is one with an ID of 0.
        /// </summary>
        public static readonly RegionId Null = new RegionId(0, 0);

        /// <summary>
        /// Creates a new <see cref="RegionId"/> from a value that contains both the region ID and the flags.
        /// </summary>
        /// <param name="bits">The int containing <see cref="RegionId"/> data.</param>
        /// <returns>A new instance of the <see cref="RegionId"/> struct with the specified data.</returns>
        public static RegionId FromRawBits(int bits)
        {
            return new RegionId()
            {
                bits = bits
            };
        }
        /// <summary>
        /// Creates a new <see cref="RegionId"/> with extra flags.
        /// </summary>
        /// <param name="region">The region to add flags to.</param>
        /// <param name="flags">The flags to add.</param>
        /// <returns>A new instance of the <see cref="RegionId"/> struct with extra flags.</returns>
        public static RegionId WithFlags(RegionId region, RegionFlags flags)
        {
            if ((RegionFlags)((int)flags & ~MaskId) != flags)
            {
                throw new ArgumentException("flags", "The provide region flags are invalid.");
            }

            RegionFlags newFlags = region.Flags | flags;
            return RegionId.FromRawBits((region.bits & MaskId) | (int)newFlags);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="RegionId"/> class without any flags set.
        /// </summary>
        /// <param name="region">The region to use.</param>
        /// <returns>A new instance of the <see cref="RegionId"/> struct without any flags set.</returns>
        public static RegionId WithoutFlags(RegionId region)
        {
            return new RegionId(region.Id);
        }
        /// <summary>
        /// Creates a new instance of the <see cref="RegionId"/> class without certain flags set.
        /// </summary>
        /// <param name="region">The region to use.</param>
        /// <param name="flags">The flags to unset.</param>
        /// <returns>A new instnace of the <see cref="RegionId"/> struct without certain flags set.</returns>
        public static RegionId WithoutFlags(RegionId region, RegionFlags flags)
        {
            if ((RegionFlags)((int)flags & ~MaskId) != flags)
            {
                throw new ArgumentException("flags", "The provide region flags are invalid.");
            }

            RegionFlags newFlags = region.Flags & ~flags;
            return RegionId.FromRawBits((region.bits & MaskId) | (int)newFlags);
        }
        /// <summary>
        /// Checks if a region has certain flags.
        /// </summary>
        /// <param name="region">The region to check.</param>
        /// <param name="flags">The flags to check.</param>
        /// <returns>A value indicating whether the region has all of the specified flags.</returns>
        public static bool HasFlags(RegionId region, RegionFlags flags)
        {
            return (region.Flags & flags) != 0;
        }

        /// <summary>
        /// Compares an instance of <see cref="RegionId"/> with an integer for equality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the region. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// int right = ...;
        /// if (left.Id == right)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">An integer.</param>
        /// <returns>A value indicating whether the two values are equal.</returns>
        public static bool operator ==(RegionId left, int right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares an instance of <see cref="RegionId"/> with an integer for inequality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the region. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// int right = ...;
        /// if (left.Id != right)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">An integer.</param>
        /// <returns>A value indicating whether the two values are unequal.</returns>
        public static bool operator !=(RegionId left, int right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Compares two instances of <see cref="RegionId"/> for equality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the regions. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// RegionId right = ...;
        /// if (left.Id == right.Id)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">Another instance of <see cref="RegionId"/>.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public static bool operator ==(RegionId left, RegionId right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two instances of <see cref="RegionId"/> for inequality.
        /// </summary>
        /// <remarks>
        /// This checks for both the ID and flags set on the regions. If you want to only compare the IDs, use the
        /// following code:
        /// <code>
        /// RegionId left = ...;
        /// RegionId right = ...;
        /// if (left.Id != right.Id)
        /// {
        ///    // ...
        /// }
        /// </code>
        /// </remarks>
        /// <param name="left">An instance of <see cref="RegionId"/>.</param>
        /// <param name="right">Another instance of <see cref="RegionId"/>.</param>
        /// <returns>A value indicating whether the two instances are unequal.</returns>
        public static bool operator !=(RegionId left, RegionId right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Converts an instance of <see cref="RegionId"/> to an integer containing both the ID and the flags.
        /// </summary>
        /// <param name="id">An instance of <see cref="RegionId"/>.</param>
        /// <returns>An integer.</returns>
        public static explicit operator int(RegionId id)
        {
            return id.bits;
        }

        /// <summary>
        /// The internal storage of a <see cref="RegionId"/>. The <see cref="RegionFlags"/> portion are the most
        /// significant bits, the integer identifier are the least significant bits, marked by <see cref="MaskId"/>.
        /// </summary>
        private int bits;
        /// <summary>
        /// Gets the ID of the region without any flags.
        /// </summary>
        public int Id
        {
            get
            {
                return bits & MaskId;
            }
        }
        /// <summary>
        /// Gets the flags set for this region.
        /// </summary>
        public RegionFlags Flags
        {
            get
            {
                return (RegionFlags)(bits & ~MaskId);
            }
        }
        /// <summary>
        /// Gets a value indicating whether the region is the null region (ID == 0).
        /// </summary>
        public bool IsNull
        {
            get
            {
                return (bits & MaskId) == 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegionId"/> struct without any flags.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public RegionId(int id)
            : this(id, 0)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionId"/> struct.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flags"></param>
        public RegionId(int id, RegionFlags flags)
        {
            int masked = id & MaskId;

            if (masked != id)
            {
                throw new ArgumentOutOfRangeException("id", "The provided id is outside of the valid range. The 3 most significant bits must be 0. Maybe you wanted RegionId.FromRawBits()?");
            }

            if ((RegionFlags)((int)flags & ~MaskId) != flags)
            {
                throw new ArgumentException("flags", "The provide region flags are invalid.");
            }

            bits = masked | (int)flags;
        }

        /// <summary>
        /// Compares this instance with another instance of <see cref="RegionId"/> for equality, including flags.
        /// </summary>
        /// <param name="other">An instance of <see cref="RegionId"/>.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals(RegionId other)
        {
            bool thisNull = this.IsNull;
            bool otherNull = other.IsNull;

            if (thisNull && otherNull)
            {
                return true;
            }
            else if (thisNull ^ otherNull)
            {
                return false;
            }
            else
            {
                return this.bits == other.bits;
            }
        }
        /// <summary>
        /// Compares this instance with another an intenger for equality, including flags.
        /// </summary>
        /// <param name="other">An integer.</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public bool Equals(int other)
        {
            RegionId otherId = new RegionId()
            {
                bits = other
            };

            return this.Equals(otherId);
        }
        /// <summary>
        /// Compares this instance with an object for equality.
        /// </summary>
        /// <param name="obj">An object</param>
        /// <returns>A value indicating whether the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            var regObj = obj as RegionId?;
            var intObj = obj as int?;

            if (regObj.HasValue)
            {
                return this.Equals(regObj.Value);
            }
            else if (intObj.HasValue)
            {
                return this.Equals(intObj.Value);
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Gets a unique hash code for this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            if (IsNull) return 0;

            return bits.GetHashCode();
        }
        /// <summary>
        /// Gets a human-readable version of this instance.
        /// </summary>
        /// <returns>A string representing this instance.</returns>
        public override string ToString()
        {
            return string.Format("Id: {0}; Flags: {0}", this.Id, this.Flags);
        }
    }
}
