using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// The collection of the <see cref="CurveKey"/> elements and a part of the <see cref="Curve"/> class.
    /// </summary>
    public class CurveKeyCollection : List<CurveKey>
    {
        /// <summary>
        /// Adds a key to this collection.
        /// </summary>
        /// <param name="item">New key for the collection.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="item"/> is null.</exception>
        /// <remarks>The new key would be added respectively to a position of that key and the position of other keys.</remarks>
        public new void Add(CurveKey item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (this.Count == 0)
            {
                base.Add(item);
                return;
            }

            for (int i = 0; i < base.Count; i++)
            {
                if (item.Position < base[i].Position)
                {
                    base.Insert(i, item);
                    return;
                }
            }

            base.Add(item);
        }
    }
}
