using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.OnePageDungeon
{
    using Engine.Modular;
    using Engine.Modular.Persistence;

    public class DungeonAssetConfiguration
    {
        private Random randomGenerator = Helper.RandomGenerator;
        private int randomSeed = 0;

        public float PositionDelta { get; set; } = 1;

        public IEnumerable<string> Volumes { get; set; } = new[] { "volume", "_volume", "_volume", "_volumes" };

        public IEnumerable<string> Floors { get; set; } = new string[] { };
        public IEnumerable<string> Walls { get; set; } = new string[] { };
        public IEnumerable<string> Ceilings { get; set; } = new string[] { };
        public IEnumerable<string> Columns { get; set; } = new string[] { };

        public IDictionary<DoorTypes, string[]> Doors { get; set; } = new Dictionary<DoorTypes, string[]> { };
        public IEnumerable<ObjectAnimationPlan> DoorAnimationPlans { get; set; }
        public IEnumerable<ObjectAction> DoorActions { get; set; }
        public IEnumerable<ObjectState> DoorStates { get; set; }

        public int RandomSeed
        {
            get
            {
                return randomSeed;
            }
            set
            {
                randomSeed = value;

                randomGenerator = Helper.SetRandomGeneratorSeed(randomSeed);
            }
        }


        public string GetRandonFloor()
        {
            return GetRandon(Floors);
        }

        public string GetRandonWall()
        {
            return GetRandon(Walls);
        }

        public string GetRandonCeiling()
        {
            return GetRandon(Ceilings);
        }

        public string GetRandonColumn()
        {
            return GetRandon(Columns);
        }

        public IEnumerable<string> GetDoor(DoorTypes doorType)
        {
            if (Doors.TryGetValue(doorType, out var res))
            {
                return res;
            }

            return new string[] { };
        }

        public T GetRandon<T>(IEnumerable<T> list)
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
