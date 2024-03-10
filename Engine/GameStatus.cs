using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Game status
    /// </summary>
    public class GameStatus
    {
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
            status = new Dictionary<string, double>(otherStatus.status);
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
        public IEnumerable<string> ReadStatus()
        {
            return status.Select((i) => $"{i.Key}: {i.Value:0.00}");
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
