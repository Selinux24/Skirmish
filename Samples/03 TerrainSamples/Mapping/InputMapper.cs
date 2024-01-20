using Engine;
using System.Collections.Generic;

namespace TerrainSamples.Mapping
{
    /// <summary>
    /// Input mapper
    /// </summary>
    public class InputMapper
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// input entry dictionary
        /// </summary>
        private readonly Dictionary<string, InputEntry> entryList = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public InputMapper(Game game)
        {
            this.game = game;
        }

        /// <summary>
        /// Loads a input mapping description
        /// </summary>
        /// <param name="description">Input mapping description</param>
        /// <param name="errorMessage">Resulting error message</param>
        public bool LoadMapping(InputMapperDescription description, out string errorMessage)
        {
            if (!description.IsValid(out errorMessage))
            {
                return false;
            }

            foreach (var entry in description.InputEntries)
            {
                if (entry.InputEntry is Keys key)
                {
                    Set(entry.Name, key);

                    continue;
                }

                if (entry.InputEntry is MouseButtons btn)
                {
                    Set(entry.Name, btn);

                    continue;
                }

                errorMessage = $"Input entry {entry.InputEntry} is not a valid entry. Try {nameof(Keys)}, {nameof(MouseButtons)}";

                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets or updates a key mapping
        /// </summary>
        /// <param name="name">Entry name</param>
        /// <param name="key">Key</param>
        public void Set(string name, Keys key)
        {
            if (!entryList.ContainsKey(name))
            {
                entryList.Add(name, new(game, key));

                return;
            }

            entryList[name] = new(game, key);
        }
        /// <summary>
        /// Sets or updates a mouse mapping
        /// </summary>
        /// <param name="name">Entry name</param>
        /// <param name="btn">Button</param>
        public void Set(string name, MouseButtons btn)
        {
            if (!entryList.ContainsKey(name))
            {
                entryList.Add(name, new(game, btn));

                return;
            }

            entryList[name] = new(game, btn);
        }
        /// <summary>
        /// Gets the input entry by name
        /// </summary>
        /// <param name="name">Entry name</param>
        public InputEntry Get(string name)
        {
            return entryList[name];
        }

        /// <summary>
        /// Gets whether the specified entry name is pressed
        /// </summary>
        /// <param name="name">Entry name</param>
        public bool Pressed(string name)
        {
            return entryList[name].Pressed;
        }
        /// <summary>
        /// Gets whether the specified entry name is released
        /// </summary>
        /// <param name="name">Entry name</param>
        public bool Released(string name)
        {
            return entryList[name].Released;
        }
        /// <summary>
        /// Gets whether the specified entry name is just pressed
        /// </summary>
        /// <param name="name">Entry name</param>
        public bool JustPressed(string name)
        {
            return entryList[name].JustPressed;
        }
        /// <summary>
        /// Gets whether the specified entry name is just released
        /// </summary>
        /// <param name="name">Entry name</param>
        public bool JustReleased(string name)
        {
            return entryList[name].JustReleased;
        }
    }
}
