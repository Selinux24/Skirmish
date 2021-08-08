using System.Collections.Generic;
using System.Linq;

namespace Engine.UI
{
    /// <summary>
    /// Font map keycode generator
    /// </summary>
    class FontMapKeycodeGenerator
    {
        /// <summary>
        /// Key codes
        /// </summary>
        public const uint KeyCodes = 512;
        /// <summary>
        /// Gets the default key list
        /// </summary>
        public static char[] DefaultKeys
        {
            get
            {
                List<char> cList = new List<char>((int)KeyCodes);

                for (uint i = 1; i < KeyCodes; i++)
                {
                    char c = (char)i;

                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }

                    if (char.IsControl(c))
                    {
                        continue;
                    }

                    cList.Add(c);
                }

                return cList.ToArray();
            }
        }

        /// <summary>
        /// Creates a keycode generator
        /// </summary>
        /// <returns>Returns a new FontMapKeycodeGenerator</returns>
        public static FontMapKeycodeGenerator Default()
        {
            return new FontMapKeycodeGenerator
            {
                Keys = DefaultKeys,
            };
        }
        /// <summary>
        /// Creates a keycode generator adding the specified collection
        /// </summary>
        /// <param name="customKeys">Custom character collection</param>
        /// <returns>Returns a new FontMapKeycodeGenerator</returns>
        public static FontMapKeycodeGenerator DefaultWithCustom(IEnumerable<char> customKeys)
        {
            var keys = DefaultKeys.ToList();

            if (customKeys?.Any() == true)
            {
                keys.AddRange(customKeys);
            }

            return new FontMapKeycodeGenerator
            {
                Keys = keys,
            };
        }
        /// <summary>
        /// Creates a keycode generator using the specified collection
        /// </summary>
        /// <param name="customKeys">Custom character collection</param>
        /// <returns>Returns a new FontMapKeycodeGenerator</returns>
        public static FontMapKeycodeGenerator Custom(IEnumerable<char> customKeys)
        {
            return new FontMapKeycodeGenerator
            {
                Keys = customKeys ?? Enumerable.Empty<char>(),
            };
        }

        /// <summary>
        /// Keys
        /// </summary>
        public IEnumerable<char> Keys { get; private set; } = Enumerable.Empty<char>();
    }
}
