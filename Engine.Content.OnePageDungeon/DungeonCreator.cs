using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.OnePageDungeon
{
    using Engine.Modular;
    using Engine.Modular.Persistence;

    /// <summary>
    /// Dungeon creator helper
    /// </summary>
    /// <remarks>
    /// Creates an asset map from a one-page-dungeon file, suitable for de <see cref="ModularScenery"/> class
    /// </remarks>
    public static class DungeonCreator
    {
        /// <summary>
        /// Creates an asset map
        /// </summary>
        /// <param name="dungeon">Dungeon file</param>
        /// <param name="configuration">Dungeon asset configuration</param>
        public static AssetMap CreateAssets(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            return new AssetMap
            {
                MaintainTextureDirection = configuration.MaintainTextureDirection,
                Assets = CreateRooms(dungeon, configuration).ToArray(),
            };
        }
        /// <summary>
        /// Gets the chamber tile list
        /// </summary>
        /// <param name="rect">Chamber rectangle</param>
        private static IEnumerable<Rectangle> GetRects(Rect rect)
        {
            List<Rectangle> rectangles = new List<Rectangle>();

            for (int x = 0; x < rect.W; x++)
            {
                for (int y = 0; y < rect.H; y++)
                {
                    int vx = (int)rect.X + x;
                    int vy = (int)rect.Y + y;

                    Rectangle r = new Rectangle(vx, vy, 1, 1);

                    rectangles.Add(r);
                }
            }

            return rectangles.ToArray();
        }
        /// <summary>
        /// Gets the dungeon's wall list
        /// </summary>
        /// <param name="dungeon">Dungeon</param>
        private static IEnumerable<Wall> MarkWalls(Dungeon dungeon)
        {
            //Get all chamber block cells
            var cells = dungeon.Rects.SelectMany(GetRects);

            foreach (var cell in cells)
            {
                var inflatedCell = cell;
                inflatedCell.Inflate(1, 1);

                //Find neighbors
                var neis = cells.Where(c => c != cell && inflatedCell.Intersects(c));
                if (!neis.Any())
                {
                    continue;
                }

                var cDirs = EvaluateDirection(cell, neis);
                if (cDirs == WallDirections.None)
                {
                    continue;
                }

                yield return new Wall() { Cell = cell, Dir = cDirs };
            }
        }
        /// <summary>
        /// Evaluates the cell direction versus it's neighbors
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <param name="neis">Neighbor list</param>
        /// <returns>Returns the wall directions enumeration</returns>
        private static WallDirections EvaluateDirection(Rectangle cell, IEnumerable<Rectangle> neis)
        {
            var ctl = cell.TopLeft();
            var ctr = cell.TopRight();
            var cbl = cell.BottomLeft();
            var cbr = cell.BottomRight();

            var cDirs = WallDirections.N | WallDirections.S | WallDirections.E | WallDirections.W;

            foreach (var nei in neis)
            {
                var ntl = nei.TopLeft();
                var ntr = nei.TopRight();
                var nbl = nei.BottomLeft();
                var nbr = nei.BottomRight();

                if (ctl == nbl && ctr == nbr)
                {
                    cDirs &= ~WallDirections.N;
                }
                if (cbl == ntl && cbr == ntr)
                {
                    cDirs &= ~WallDirections.S;
                }
                if (ctr == ntl && cbr == nbl)
                {
                    cDirs &= ~WallDirections.E;
                }
                if (ctl == ntr && cbl == nbr)
                {
                    cDirs &= ~WallDirections.W;
                }
            }

            return cDirs;
        }
        /// <summary>
        /// Creates the asset list for the dungeon's rooms
        /// </summary>
        /// <param name="dungeon">Dungeon</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<Asset> CreateRooms(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            var walls = MarkWalls(dungeon);

            int rectIndex = 0;
            foreach (var rect in dungeon.Rects)
            {
                List<AssetReference> roomAssets = new List<AssetReference>();

                roomAssets.AddRange(CreateRoom(rect, walls, configuration));
                roomAssets.AddRange(CreateColumns(rect, dungeon, configuration));

                Asset room = new Asset
                {
                    Name = $"{rectIndex++}",
                    References = roomAssets.ToArray(),
                    Connections = CreateConnections(rect, dungeon, configuration),
                };

                yield return room;
            }
        }
        /// <summary>
        /// Creates a room
        /// </summary>
        /// <param name="rect">Room rectangle</param>
        /// <param name="walls">Wall list</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<AssetReference> CreateRoom(Rect rect, IEnumerable<Wall> walls, DungeonAssetConfiguration configuration)
        {
            List<AssetReference> references = new List<AssetReference>();

            for (int x = 0; x < rect.W; x++)
            {
                for (int y = 0; y < rect.H; y++)
                {
                    int ix = (int)rect.X + x;
                    int iy = (int)rect.Y + y;
                    float vx = (rect.X + x) * configuration.BlockSize;
                    float vz = (rect.Y + y) * configuration.BlockSize;

                    string floorAsset = configuration.GetRandonFloor();
                    if (!string.IsNullOrWhiteSpace(floorAsset))
                    {
                        AssetReference floor = new AssetReference()
                        {
                            AssetName = floorAsset,
                            Type = ModularSceneryAssetTypes.Floor,
                            Position = new Vector3(-vx, 0, vz),
                        };
                        references.Add(floor);
                    }

                    string ceilingAsset = configuration.GetRandonCeiling();
                    if (!string.IsNullOrWhiteSpace(ceilingAsset))
                    {
                        AssetReference ceiling = new AssetReference()
                        {
                            AssetName = ceilingAsset,
                            Type = ModularSceneryAssetTypes.Ceiling,
                            Position = new Vector3(-vx, 0, vz),
                        };
                        references.Add(ceiling);
                    }

                    var cellWalls = walls.Where(w => w.Cell.X == ix && w.Cell.Y == iy);
                    if (cellWalls.Any())
                    {
                        references.AddRange(cellWalls.SelectMany(cellWall => CreateWall(vx, vz, cellWall, configuration)));
                    }
                }
            }

            return references.ToArray();
        }
        /// <summary>
        /// Creates a wall
        /// </summary>
        /// <param name="vx">X position</param>
        /// <param name="vz">Z position</param>
        /// <param name="cellWall">Wall</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<AssetReference> CreateWall(float vx, float vz, Wall cellWall, DungeonAssetConfiguration configuration)
        {
            if (cellWall.Dir.HasFlag(WallDirections.N))
            {
                AssetReference wall = new AssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx, 0, vz - (configuration.BlockSize * 0.5f)),
                    Rotation = "Rot270",
                };

                yield return wall;
            }

            if (cellWall.Dir.HasFlag(WallDirections.S))
            {
                AssetReference wall = new AssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx, 0, vz + (configuration.BlockSize * 0.5f)),
                    Rotation = "Rot90",
                };

                yield return wall;
            }

            if (cellWall.Dir.HasFlag(WallDirections.E))
            {
                AssetReference wall = new AssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx - (configuration.BlockSize * 0.5f), 0, vz),
                };

                yield return wall;
            }

            if (cellWall.Dir.HasFlag(WallDirections.W))
            {
                AssetReference wall = new AssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx + (configuration.BlockSize * 0.5f), 0, vz),
                    Rotation = "Rot180",
                };

                yield return wall;
            }
        }
        /// <summary>
        /// Creates a room connection list
        /// </summary>
        /// <param name="rect">Room rectangle</param>
        /// <param name="dungeon">Dungeon</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<AssetConnection> CreateConnections(Rect rect, Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            var roomRect = rect.GetRectangle();
            roomRect.Inflate(1, 1);

            // Finds the room neighbors
            var neis = dungeon.Rects.Where(r =>
            {
                if (r == rect)
                {
                    return false;
                }

                var neiRect = r.GetRectangle();
                return roomRect.Intersects(neiRect);
            });

            if (!neis.Any())
            {
                return Enumerable.Empty<AssetConnection>();
            }

            // Create connection between the room and the neighbors
            return neis.SelectMany(nei => CreateConnection(rect, nei, configuration));
        }
        /// <summary>
        /// Creates a connection list between rooms
        /// </summary>
        /// <param name="one">Room rectangle one</param>
        /// <param name="two">Room rectangle two</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<AssetConnection> CreateConnection(Rect one, Rect two, DungeonAssetConfiguration configuration)
        {
            List<AssetConnection> connections = new List<AssetConnection>();

            for (int x = 0; x < one.W; x++)
            {
                for (int y = 0; y < one.H; y++)
                {
                    int ix = (int)one.X + x;
                    int iy = (int)one.Y + y;

                    connections.AddRange(CreateConnection(two, new RectangleF(ix, iy, 1, 1), configuration));
                }
            }

            return connections.ToArray();
        }
        /// <summary>
        /// Creates a connection between the room rectangle and the area rectangle
        /// </summary>
        /// <param name="rect">Room rectangle</param>
        /// <param name="two">Area</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<AssetConnection> CreateConnection(Rect rect, RectangleF two, DungeonAssetConfiguration configuration)
        {
            float tvx = two.X * configuration.BlockSize;
            float tvz = two.Y * configuration.BlockSize;
            Vector3 tposition = new Vector3(-tvx, 0, tvz);

            for (int x = 0; x < rect.W; x++)
            {
                for (int y = 0; y < rect.H; y++)
                {
                    int ix = (int)rect.X + x;
                    int iy = (int)rect.Y + y;
                    float vx = (rect.X + x) * configuration.BlockSize;
                    float vz = (rect.Y + y) * configuration.BlockSize;
                    Vector3 position = new Vector3(-vx, 0, vz);

                    RectangleF r1 = new RectangleF(ix, iy, 1, 1);
                    RectangleF r2 = new RectangleF(ix, iy, 1, 1);
                    r1.Inflate(1, 0);
                    r2.Inflate(0, 1);

                    if (two.Intersects(r1))
                    {
                        //Connection
                        yield return new AssetConnection()
                        {
                            Position = tposition,
                            Direction = -Vector3.Normalize(position - tposition),
                            Type = ModularSceneryAssetConnectionTypes.Open,
                        };
                    }

                    if (two.Intersects(r2))
                    {
                        //Connection
                        yield return new AssetConnection()
                        {
                            Position = tposition,
                            Direction = Vector3.Normalize(position - tposition),
                            Type = ModularSceneryAssetConnectionTypes.Open,
                        };
                    }
                }
            }
        }
        /// <summary>
        /// Creates a column
        /// </summary>
        /// <param name="rect">Room rectangle</param>
        /// <param name="dungeon">Dungeon</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<AssetReference> CreateColumns(Rect rect, Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            var roomRect = rect.GetRectangle();
            float half = configuration.BlockSize * 0.5f;

            foreach (var column in dungeon.Columns)
            {
                if (!roomRect.Contains(column.X, column.Y))
                {
                    continue;
                }

                float vx = column.X * configuration.BlockSize;
                float vz = column.Y * configuration.BlockSize;

                AssetReference col = new AssetReference()
                {
                    AssetName = configuration.GetRandonColumn(),
                    Type = ModularSceneryAssetTypes.None,
                    Position = new Vector3(-vx + half, 0, vz - half),
                };

                yield return col;
            }
        }
        /// <summary>
        /// Creates a level map
        /// </summary>
        /// <param name="dungeon">Dungeon</param>
        /// <param name="configuration">Asset configuration</param>
        public static LevelMap CreateLevels(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            LevelMap levels = new LevelMap
            {
                Hulls = configuration.Hulls.ToArray(),
                Levels = new[] { CreateLevel(dungeon, configuration) }
            };

            return levels;
        }
        /// <summary>
        /// Creates a level
        /// </summary>
        /// <param name="dungeon">Dungeon</param>
        /// <param name="configuration">Asset configuration</param>
        private static Level CreateLevel(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            var maps = CreateMap(dungeon);
            var objs = CreateDoors(dungeon, configuration);

            var walls = MarkWalls(dungeon);
            var startWall = walls.First(w => w.Cell.X == 0 && w.Cell.Y == 0);
            var dir = startWall.GetFirstOpenDirection();

            Level level = new Level()
            {
                Name = dungeon.Title,
                StartPosition = Vector3.Zero,
                LookingVector = dir,
                Map = maps.ToArray(),
                Objects = objs.ToArray(),
            };

            return level;
        }
        /// <summary>
        /// Creates an asset reference of the map
        /// </summary>
        /// <param name="dungeon">Dungeon</param>
        private static IEnumerable<AssetReference> CreateMap(Dungeon dungeon)
        {
            int rectIndex = 0;
            foreach (var rect in dungeon.Rects)
            {
                AssetReference assetRef = new AssetReference()
                {
                    AssetName = $"{rectIndex++}",
                };

                yield return assetRef;
            }
        }
        /// <summary>
        /// Creates the door list
        /// </summary>
        /// <param name="dungeon">Dungeon</param>
        /// <param name="configuration">Asset configuration</param>
        private static IEnumerable<ObjectReference> CreateDoors(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            int index = 0;
            foreach (var door in dungeon.Doors)
            {
                var doorType = door.GetDoorType();

                if (doorType == DoorTypes.None)
                {
                    continue;
                }

                var doorAssets = configuration.GetDoorByType(doorType);
                if (!doorAssets.Any())
                {
                    break;
                }

                float vx = door.X * configuration.BlockSize;
                float vz = door.Y * configuration.BlockSize;
                Vector3 dir = new Vector3(door.Dir.X, 0, -door.Dir.Y);
                string rot = EvaluateRotation(dir);

                ObjectReference obj = new ObjectReference
                {
                    AssetName = doorAssets.First(),
                    Id = $"door_{index++}",
                    Name = "door",
                    Type = ModularSceneryObjectTypes.Door,
                    Position = new Vector3(-vx, 0, vz) - (dir * (configuration.BlockSize * 0.5f)),
                    Rotation = rot,
                    AnimationPlans = configuration.DoorAnimationPlans?.ToArray(),
                    Actions = configuration.DoorActions?.ToArray(),
                    States = configuration.DoorStates?.ToArray(),
                };
                yield return obj;

                foreach (var asset in doorAssets.Skip(1))
                {
                    ObjectReference obj2 = new ObjectReference
                    {
                        AssetName = asset,
                        Type = ModularSceneryObjectTypes.Default,
                        Position = new Vector3(-vx, 0, vz) - (dir * (configuration.BlockSize * 0.5f)),
                        Rotation = rot,
                    };
                    yield return obj2;
                }
            }
        }
        /// <summary>
        /// Evaluates door rotation
        /// </summary>
        /// <param name="dir">Door direction</param>
        private static string EvaluateRotation(Vector3 dir)
        {
            if (dir == Vector3.ForwardLH)
            {
                return "Rot90";
            }
            else if (dir == Vector3.BackwardLH)
            {
                return "Rot270";
            }
            else if (dir == Vector3.Left)
            {
                return "";
            }
            else if (dir == Vector3.Right)
            {
                return "Rot180";
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Wall definition
        /// </summary>
        struct Wall
        {
            /// <summary>
            /// Cell
            /// </summary>
            public Rectangle Cell { get; set; }
            /// <summary>
            /// Wall direction
            /// </summary>
            public WallDirections Dir { get; set; }

            /// <summary>
            /// Get the first open direction
            /// </summary>
            public readonly Vector3 GetFirstOpenDirection()
            {
                if (!Dir.HasFlag(WallDirections.N))
                {
                    return -Vector3.ForwardLH;
                }
                else if (!Dir.HasFlag(WallDirections.S))
                {
                    return -Vector3.BackwardLH;
                }
                else if (!Dir.HasFlag(WallDirections.E))
                {
                    return Vector3.Left;
                }
                else if (!Dir.HasFlag(WallDirections.W))
                {
                    return Vector3.Right;
                }

                return -Vector3.ForwardLH;
            }
        }

        /// <summary>
        /// Wall directions flag
        /// </summary>
        [Flags]
        enum WallDirections
        {
            /// <summary>
            /// None
            /// </summary>
            None = 0,
            /// <summary>
            /// North
            /// </summary>
            N = 1,
            /// <summary>
            /// South
            /// </summary>
            S = 2,
            /// <summary>
            /// East
            /// </summary>
            E = 4,
            /// <summary>
            /// West
            /// </summary>
            W = 8,
            /// <summary>
            /// All directions
            /// </summary>
            All = 15,
        }
    }
}
