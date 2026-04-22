using System.Collections.Generic;
using System.Diagnostics;

namespace Engine
{
    /// <summary>
    /// Game status
    /// </summary>
    public class GameStatus
    {
        /// <summary>
        /// Status buffer
        /// </summary>
        private readonly List<string> readBuffer = new(32);

        /// <summary>
        /// Internal status dictionary
        /// </summary>
        internal readonly Dictionary<string, double> status = [];

        /// <summary>
        /// Constructor
        /// </summary>
        public GameStatus()
        {

        }
        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="otherStatus">Game status dictionary</param>
        private GameStatus(GameStatus otherStatus) : this()
        {
            status = new(otherStatus.status);
        }

        /// <summary>
        /// Adds a line to the status
        /// </summary>
        /// <param name="key">Line key</param>
        /// <param name="value">Line value</param>
        public void Add(string key, double value)
        {
            if (!status.TryAdd(key, value))
            {
                status[key] += value;
            }
        }
        /// <summary>
        /// Adds a line to the status
        /// </summary>
        /// <param name="key">Line key</param>
        /// <param name="stopwatch">Line value</param>
        /// <remarks>Gets the elapsed milliseconds from the Stopwatch</remarks>
        public void Add(string key, Stopwatch stopwatch)
        {
            Add(key, stopwatch.Elapsed.TotalMilliseconds);
        }
        /// <summary>
        /// Adds the dictionary to the status
        /// </summary>
        /// <param name="dictionary">Dictionary</param>
        public void Add(IDictionary<string, double> dictionary)
        {
            foreach (var item in dictionary)
            {
                Add(item.Key, item.Value);
            }
        }
        /// <summary>
        /// Adds the status to the current game status
        /// </summary>
        /// <param name="otherStatus">Other status</param>
        public void Add(GameStatus otherStatus)
        {
            Add(otherStatus.status);
        }

        /// <summary>
        /// Reads the complete status into a string collection
        /// </summary>
        /// <returns>Returns a string collection with the complete status</returns>
        public IReadOnlyList<string> ReadStatus()
        {
            readBuffer.Clear();
            foreach (var kvp in status)
            {
                readBuffer.Add($"{kvp.Key}: {kvp.Value:0.00}");
            }
            return readBuffer;
        }
        /// <summary>
        /// Copies the current game status
        /// </summary>
        /// <returns>Returns a new instance with the current game status</returns>
        public GameStatus Copy()
        {
            return new GameStatus(this);
        }
        /// <summary>
        /// Clears the status
        /// </summary>
        public void Clear()
        {
            status.Clear();
        }
    }
}
