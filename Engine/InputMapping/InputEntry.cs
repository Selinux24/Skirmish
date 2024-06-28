using System;

namespace Engine.InputMapping
{
    /// <summary>
    /// Input entry
    /// </summary>
    public readonly struct InputEntry
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// Entry
        /// </summary>
        private readonly object entry;
        /// <summary>
        /// Pressed function
        /// </summary>
        private readonly Func<IInput, bool> pressedFnc;
        /// <summary>
        /// Just pressed function
        /// </summary>
        private readonly Func<IInput, bool> justPressedFnc;
        /// <summary>
        /// Just released function
        /// </summary>
        private readonly Func<IInput, bool> justReleasedFnc;

        /// <summary>
        /// Gets whether the entry is pressed
        /// </summary>
        public readonly bool Pressed
        {
            get
            {
                return pressedFnc?.Invoke(game?.Input) ?? false;
            }
        }
        /// <summary>
        /// Gets whether the entry is released
        /// </summary>
        public readonly bool Released
        {
            get
            {
                return !Pressed;
            }
        }
        /// <summary>
        /// Gets whether the entry is just pressed
        /// </summary>
        public readonly bool JustPressed
        {
            get
            {
                return justPressedFnc?.Invoke(game?.Input) ?? false;
            }
        }
        /// <summary>
        /// Gets whether the entry is just released
        /// </summary>
        public readonly bool JustReleased
        {
            get
            {
                return justReleasedFnc?.Invoke(game?.Input) ?? false;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="key">Key</param>
        public InputEntry(Game game, Keys key)
        {
            this.game = game;
            entry = key;
            pressedFnc = (input) => input?.KeyPressed(key) ?? false;
            justPressedFnc = (input) => input?.KeyJustPressed(key) ?? false;
            justReleasedFnc = (input) => input?.KeyJustReleased(key) ?? false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="btn">Mouse button</param>
        public InputEntry(Game game, MouseButtons btn)
        {
            this.game = game;
            entry = btn;
            pressedFnc = (input) => input?.MouseButtonPressed(btn) ?? false;
            justPressedFnc = (input) => input?.MouseButtonJustPressed(btn) ?? false;
            justReleasedFnc = (input) => input?.MouseButtonJustReleased(btn) ?? false;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (entry is MouseButtons)
            {
                return $"Mouse {entry}";
            }

            return $"{entry ?? "Unspecified"}";
        }
    }
}
