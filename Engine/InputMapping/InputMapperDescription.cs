using System;
using System.Linq;

namespace Engine.InputMapping
{
    /// <summary>
    /// Input mapper description
    /// </summary>
    public struct InputMapperDescription
    {
        /// <summary>
        /// Input entry list
        /// </summary>
        public InputEntryDescription[] InputEntries { get; set; }

        /// <summary>
        /// Validates the entry list
        /// </summary>
        /// <param name="errorMessage">Resulting error message</param>
        public readonly bool IsValid(out string errorMessage)
        {
            if (InputEntries.Length != InputEntries.Select(e => e.Name).Distinct().Count())
            {
                errorMessage = "Entry names must be unique.";

                return false;
            }

            errorMessage = null;

            return true;
        }
    }
}
