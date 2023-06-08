using Engine.Modular.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.OnePageDungeon
{
    /// <summary>
    /// Assets configuration
    /// </summary>
    /// <remarks>
    /// Used for referencing the one-page-dungeon file with a collection of assets, representing walls, ceilings, doors...
    /// </remarks>
    public class DungeonAssetConfiguration
    {
        /// <summary>
        /// Ramdon generator
        /// </summary>
        private Random randomGenerator = Helper.RandomGenerator;
        /// <summary>
        /// Random seed value
        /// </summary>
        /// <remarks>
        /// Used for positioning props in the dungeon
        /// </remarks>
        private int randomSeed = 0;

        /// <summary>
        /// Asset definition file names
        /// </summary>
        public IEnumerable<string> Assets { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Asset block size
        /// </summary>
        public float BlockSize { get; set; } = 1;
        /// <summary>
        /// Maintain texture direction for ceilings and floors, avoiding asset map rotations
        /// </summary>
        public bool MaintainTextureDirection { get; set; } = true;
        /// <summary>
        /// Hull names - geometry used for navigation mapping and coarse collision detection
        /// </summary>
        public IEnumerable<string> Hulls { get; set; } = new[] { "volume", "_volume", "_volume", "_volumes" };
        /// <summary>
        /// Floor names
        /// </summary>
        public IEnumerable<string> Floors { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Wall names
        /// </summary>
        public IEnumerable<string> Walls { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Ceiling names
        /// </summary>
        public IEnumerable<string> Ceilings { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Column names
        /// </summary>
        public IEnumerable<string> Columns { get; set; } = Array.Empty<string>();
        /// <summary>
        /// Door names dictionary by door type
        /// </summary>
        public IDictionary<DoorTypes, string[]> Doors { get; set; } = new Dictionary<DoorTypes, string[]> { };
        /// <summary>
        /// Door animations
        /// </summary>
        public IEnumerable<ObjectAnimationPlan> DoorAnimationPlans { get; set; }
        /// <summary>
        /// Door actions
        /// </summary>
        public IEnumerable<ObjectAction> DoorActions { get; set; }
        /// <summary>
        /// Door states
        /// </summary>
        public IEnumerable<ObjectState> DoorStates { get; set; }

        /// <summary>
        /// Random seed
        /// </summary>
        public int RandomSeed
        {
            get
            {
                return randomSeed;
            }
            set
            {
                if (randomSeed != value)
                {
                    return;
                }

                randomSeed = value;

                randomGenerator = Helper.SetRandomGeneratorSeed(randomSeed);
            }
        }

        /// <summary>
        /// Gets a random floor name
        /// </summary>
        public string GetRandonFloor()
        {
            return GetRandon(Floors);
        }
        /// <summary>
        /// Gets a random wall name
        /// </summary>
        public string GetRandonWall()
        {
            return GetRandon(Walls);
        }
        /// <summary>
        /// Gets a random ceiling name
        /// </summary>
        public string GetRandonCeiling()
        {
            return GetRandon(Ceilings);
        }
        /// <summary>
        /// Gets a random column name
        /// </summary>
        public string GetRandonColumn()
        {
            return GetRandon(Columns);
        }
        /// <summary>
        /// Gets a random door name by type
        /// </summary>
        public IEnumerable<string> GetDoorByType(DoorTypes doorType)
        {
            if (Doors.TryGetValue(doorType, out var res))
            {
                return res;
            }

            return Array.Empty<string>();
        }
        /// <summary>
        /// Gets a ramdom value from a list
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list">List of values</param>
        private T GetRandon<T>(IEnumerable<T> list)
        {
            if (list?.Any() != true)
            {
                return default;
            }

            int index = randomGenerator.Next(0, list.Count());

            return list.ElementAt(index);
        }
    }
}
