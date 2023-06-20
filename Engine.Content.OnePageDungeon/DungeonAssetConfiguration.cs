using Engine.Content.Persistence;
using Engine.Modular.Persistence;
using SharpDX;
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
    public record DungeonAssetConfiguration
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
        public IEnumerable<string> AssetFiles { get; set; } = Enumerable.Empty<string>();
        /// <summary>
        /// Asset definitions
        /// </summary>
        public IEnumerable<ContentDataFile> Assets { get; set; } = Enumerable.Empty<ContentDataFile>();
        /// <summary>
        /// Asset block size
        /// </summary>
        public float BlockSize { get; set; } = 1;
        /// <summary>
        /// Maintain texture direction for ceilings and floors, avoiding asset map rotations
        /// </summary>
        public bool MaintainTextureDirection { get; set; } = true;
        /// <summary>
        /// Rotation delta
        /// </summary>
        /// <remarks>
        /// 0 - No delta
        /// 1 - 90 degrees delta
        /// 2 - 180 degrees delta
        /// 3 - 270 degrees delta
        /// </remarks>
        public int RotationDelta { get; set; } = 0;
        /// <summary>
        /// Hull names - geometry used for navigation mapping and coarse collision detection
        /// </summary>
        public IEnumerable<string> Hulls { get; set; } = new[] { "volume", "_volume", "_volume", "_volumes" };
        /// <summary>
        /// Floor names
        /// </summary>
        public IEnumerable<DungeonProp> Floors { get; set; } = Enumerable.Empty<DungeonProp>();
        /// <summary>
        /// Wall names
        /// </summary>
        public IEnumerable<DungeonProp> Walls { get; set; } = Enumerable.Empty<DungeonProp>();
        /// <summary>
        /// Ceiling names
        /// </summary>
        public IEnumerable<DungeonProp> Ceilings { get; set; } = Enumerable.Empty<DungeonProp>();
        /// <summary>
        /// Column names
        /// </summary>
        public IEnumerable<DungeonProp> Columns { get; set; } = Enumerable.Empty<DungeonProp>();
        /// <summary>
        /// Door names dictionary by door type
        /// </summary>
        public IDictionary<DoorTypes, DungeonDoor> Doors { get; set; } = new Dictionary<DoorTypes, DungeonDoor> { };
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
        public DungeonProp GetRandonFloor()
        {
            return GetRandon(Floors);
        }
        /// <summary>
        /// Gets a random wall name
        /// </summary>
        public DungeonProp GetRandonWall()
        {
            return GetRandon(Walls);
        }
        /// <summary>
        /// Gets a random ceiling name
        /// </summary>
        public DungeonProp GetRandonCeiling()
        {
            return GetRandon(Ceilings);
        }
        /// <summary>
        /// Gets a random column name
        /// </summary>
        public DungeonProp GetRandonColumn()
        {
            return GetRandon(Columns);
        }
        /// <summary>
        /// Gets a random door name by type
        /// </summary>
        public DungeonDoor GetDoorByType(DoorTypes doorType)
        {
            if (Doors.TryGetValue(doorType, out var res))
            {
                return res;
            }

            return null;
        }
        /// <summary>
        /// Gets a ramdom value from a list
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list">List of values</param>
        private DungeonProp GetRandon(IEnumerable<DungeonProp> list)
        {
            if (list?.Any() != true)
            {
                return default;
            }

            if (list.Count() == 1)
            {
                return list.First();
            }

            float weight = randomGenerator.NextFloat(0, 1);

            var sortedList = list.OrderBy(i => i.Weight);

            return sortedList.FirstOrDefault(i => weight <= i.Weight) ?? sortedList.Last();
        }
    }

    /// <summary>
    /// Dungeon asset description
    /// </summary>
    public record DungeonAsset
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Scale
        /// </summary>
        public Scale3 Scale { get; set; } = Scale3.One;
        /// <summary>
        /// Position
        /// </summary>
        public Position3 Position { get; set; } = Position3.Zero;
        /// <summary>
        /// Rotation
        /// </summary>
        public RotationQ Rotation { get; set; } = RotationQ.Identity;

        /// <summary>
        /// Gets the initial transform matrix
        /// </summary>
        public Matrix4X4 GetTransform()
        {
            return
                Matrix4X4.Scaling(Scale) *
                Matrix4X4.Rotation(Rotation) *
                Matrix4X4.Translation(Position);
        }
    }

    /// <summary>
    /// Dungeon prop
    /// </summary>
    public record DungeonProp
    {
        /// <summary>
        /// Asset list
        /// </summary>
        public IEnumerable<DungeonAsset> Assets { get; set; } = Enumerable.Empty<DungeonAsset>();
        /// <summary>
        /// Prop weight in the collection
        /// </summary>
        public float Weight { get; set; } = 1;
    }

    /// <summary>
    /// Dungeon door
    /// </summary>
    public record DungeonDoor
    {
        /// <summary>
        /// Door asset
        /// </summary>
        public DungeonAsset Door { get; set; }
        /// <summary>
        /// Prop
        /// </summary>
        public DungeonProp Prop { get; set; }
    }
}
