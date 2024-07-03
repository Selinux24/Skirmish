using Engine.Content.Persistence;
using Engine.Modular.Persistence;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        /// Loads an asset configuration from the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static DungeonAssetConfiguration Load(string fileName)
        {
            return SerializationHelper.DeserializeJsonFromFile<DungeonAssetConfiguration>(fileName);
        }
        /// <summary>
        /// Saves an asset configuration to the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="configuration">Configuration to save</param>
        public static void Save(string fileName, DungeonAssetConfiguration configuration)
        {
            SerializationHelper.SerializeJsonToFile(configuration, fileName);
        }

        /// <summary>
        /// Ramdon generator
        /// </summary>
        private Random randomGenerator = null;
        /// <summary>
        /// Ramdon generator seed
        /// </summary>
        private int randomGeneratorSeed = 0;

        /// <summary>
        /// Asset definition file names
        /// </summary>
        public IEnumerable<string> AssetFiles { get; set; } = [];
        /// <summary>
        /// Asset definitions
        /// </summary>
        public IEnumerable<ContentDataFile> Assets { get; set; } = [];
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
        public IEnumerable<string> Hulls { get; set; } = ["volume", "_volume", "_volume", "_volumes"];
        /// <summary>
        /// Floor names
        /// </summary>
        public IEnumerable<DungeonProp> Floors { get; set; } = [];
        /// <summary>
        /// Wall names
        /// </summary>
        public IEnumerable<DungeonProp> Walls { get; set; } = [];
        /// <summary>
        /// Ceiling names
        /// </summary>
        public IEnumerable<DungeonProp> Ceilings { get; set; } = [];
        /// <summary>
        /// Column names
        /// </summary>
        public IEnumerable<DungeonProp> Columns { get; set; } = [];
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
        public int RandomSeed { get; set; }

        /// <summary>
        /// Gets the random generator
        /// </summary>
        private Random GetGenerator()
        {
            if (randomGenerator == null || randomGeneratorSeed != RandomSeed)
            {
                randomGenerator = Helper.NewGenerator(RandomSeed);
                randomGeneratorSeed = RandomSeed;

                return randomGenerator;
            }

            return randomGenerator;
        }
        /// <summary>
        /// Gets a ramdom value from a list
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="list">List of values</param>
        private DungeonProp GetRandom(IEnumerable<DungeonProp> list)
        {
            if (list?.Any() != true)
            {
                return default;
            }

            if (list.Count() == 1)
            {
                return list.First();
            }

            float weight = GetGenerator().NextFloat(0, 1);

            var sortedList = list.OrderBy(i => i.Weight);

            return sortedList.FirstOrDefault(i => weight <= i.Weight) ?? sortedList.Last();
        }

        /// <summary>
        /// Gets a random floor name
        /// </summary>
        public DungeonProp GetRandonFloor()
        {
            return GetRandom(Floors);
        }
        /// <summary>
        /// Gets a random wall name
        /// </summary>
        public DungeonProp GetRandonWall()
        {
            return GetRandom(Walls);
        }
        /// <summary>
        /// Gets a random ceiling name
        /// </summary>
        public DungeonProp GetRandonCeiling()
        {
            return GetRandom(Ceilings);
        }
        /// <summary>
        /// Gets a random column name
        /// </summary>
        public DungeonProp GetRandonColumn()
        {
            return GetRandom(Columns);
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
        /// Path finding
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PathFindingModes PathFinding { get; set; } = PathFindingModes.None;

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
        public IEnumerable<DungeonAsset> Assets { get; set; } = [];
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

    /// <summary>
    /// Path finding
    /// </summary>
    public enum PathFindingModes
    {
        /// <summary>
        /// Not used
        /// </summary>
        None = 0,
        /// <summary>
        /// Use the object's OBB
        /// </summary>
        Coarse = 1,
        /// <summary>
        /// Use the object's linked hull
        /// </summary>
        Hull = 2,
        /// <summary>
        /// Use the object's triangle list
        /// </summary>
        Geometry = 3,
    }
}
