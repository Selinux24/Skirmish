using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.OnePageDungeon
{
    public static class DungeonCreator
    {
        public static ModularSceneryAssetConfiguration CreateAssets(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            ModularSceneryAssetConfiguration assets = new ModularSceneryAssetConfiguration
            {
                MaintainTextureDirection = true
            };

            List<ModularSceneryAssetDescription> assetList = new List<ModularSceneryAssetDescription>();

            assetList.AddRange(CreateRooms(dungeon, configuration));

            assets.Assets = assetList.ToArray();

            return assets;
        }

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

        private static IEnumerable<Wall> MarkWalls(Dungeon dungeon)
        {
            List<Wall> walls = new List<Wall>();

            List<Rectangle> cells = new List<Rectangle>();

            foreach (var room in dungeon.Rects)
            {
                cells.AddRange(GetRects(room));
            }

            foreach (var cell in cells)
            {
                var inflatedCell = cell;
                inflatedCell.Inflate(1, 1);

                //Find neighborus
                var neis = cells.Where(c => c != cell && inflatedCell.Intersects(c));
                if (neis.Any())
                {
                    var cDirs = EvaluateDirection(cell, neis);
                    if (cDirs != WallDirections.None)
                    {
                        walls.Add(new Wall() { Cell = cell, Dir = cDirs });
                    }
                }
            }

            return walls.ToArray();
        }

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

        private static IEnumerable<ModularSceneryAssetDescription> CreateRooms(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetDescription> assetList = new List<ModularSceneryAssetDescription>();

            var walls = MarkWalls(dungeon);

            int rectIndex = 0;
            foreach (var rect in dungeon.Rects)
            {
                List<ModularSceneryAssetReference> roomAssets = new List<ModularSceneryAssetReference>();

                roomAssets.AddRange(CreateRoom(rect, walls, configuration));
                roomAssets.AddRange(CreateColumns(rect, dungeon, configuration));

                ModularSceneryAssetDescription room = new ModularSceneryAssetDescription()
                {
                    Name = $"{rectIndex++}",
                };

                room.Assets = roomAssets.ToArray();
                room.Connections = CreateConnections(rect, dungeon, configuration).ToArray();

                assetList.Add(room);
            }

            return assetList.ToArray();
        }

        private static IEnumerable<ModularSceneryAssetReference> CreateRoom(Rect rect, IEnumerable<Wall> walls, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetReference> references = new List<ModularSceneryAssetReference>();

            for (int x = 0; x < rect.W; x++)
            {
                for (int y = 0; y < rect.H; y++)
                {
                    int ix = (int)rect.X + x;
                    int iy = (int)rect.Y + y;
                    float vx = (rect.X + x) * configuration.PositionDelta;
                    float vz = (rect.Y + y) * configuration.PositionDelta;

                    ModularSceneryAssetReference floor = new ModularSceneryAssetReference()
                    {
                        AssetName = configuration.GetRandonFloor(),
                        Type = ModularSceneryAssetTypes.Floor,
                        Position = new Vector3(-vx, 0, vz),
                    };

                    references.Add(floor);

                    ModularSceneryAssetReference ceiling = new ModularSceneryAssetReference()
                    {
                        AssetName = configuration.GetRandonCeiling(),
                        Type = ModularSceneryAssetTypes.Ceiling,
                        Position = new Vector3(-vx, 0, vz),
                    };

                    references.Add(ceiling);

                    var cellWalls = walls.Where(w => w.Cell.X == ix && w.Cell.Y == iy);
                    if (cellWalls.Any())
                    {
                        foreach (var cellWall in cellWalls)
                        {
                            references.AddRange(CreateWall(vx, vz, cellWall, configuration));
                        }
                    }
                }
            }

            return references.ToArray();
        }

        private static IEnumerable<ModularSceneryAssetReference> CreateWall(float vx, float vz, Wall cellWall, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetReference> references = new List<ModularSceneryAssetReference>();

            if (cellWall.Dir.HasFlag(WallDirections.N))
            {
                ModularSceneryAssetReference wall = new ModularSceneryAssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx, 0, vz - (configuration.PositionDelta * 0.5f)),
                    RotationText = "Rot270",
                };

                references.Add(wall);
            }

            if (cellWall.Dir.HasFlag(WallDirections.S))
            {
                ModularSceneryAssetReference wall = new ModularSceneryAssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx, 0, vz + (configuration.PositionDelta * 0.5f)),
                    RotationText = "Rot90",
                };

                references.Add(wall);
            }

            if (cellWall.Dir.HasFlag(WallDirections.E))
            {
                ModularSceneryAssetReference wall = new ModularSceneryAssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx - (configuration.PositionDelta * 0.5f), 0, vz),
                };

                references.Add(wall);
            }

            if (cellWall.Dir.HasFlag(WallDirections.W))
            {
                ModularSceneryAssetReference wall = new ModularSceneryAssetReference()
                {
                    AssetName = configuration.GetRandonWall(),
                    Type = ModularSceneryAssetTypes.Wall,
                    Position = new Vector3(-vx + (configuration.PositionDelta * 0.5f), 0, vz),
                    RotationText = "Rot180",
                };

                references.Add(wall);
            }

            return references.ToArray();
        }

        private static IEnumerable<ModularSceneryAssetDescriptionConnection> CreateConnections(Rect rect, Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetDescriptionConnection> res = new List<ModularSceneryAssetDescriptionConnection>();

            var roomRect = rect.GetRectangle();
            roomRect.Inflate(1, 1);

            var neis = dungeon.Rects.Where(r =>
            {
                if (r == rect)
                {
                    return false;
                }

                var neiRect = r.GetRectangle();
                return roomRect.Intersects(neiRect);
            });

            if (neis.Any())
            {
                foreach (var nei in neis)
                {
                    res.AddRange(CreateConnection(rect, nei, configuration));
                }
            }

            return res.ToArray();
        }

        private static IEnumerable<ModularSceneryAssetDescriptionConnection> CreateConnection(Rect one, Rect two, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetDescriptionConnection> connections = new List<ModularSceneryAssetDescriptionConnection>();

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

        private static IEnumerable<ModularSceneryAssetDescriptionConnection> CreateConnection(Rect rect, RectangleF two, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetDescriptionConnection> connections = new List<ModularSceneryAssetDescriptionConnection>();

            float tvx = two.X * configuration.PositionDelta;
            float tvz = two.Y * configuration.PositionDelta;
            Vector3 tposition = new Vector3(-tvx, 0, tvz);

            for (int x = 0; x < rect.W; x++)
            {
                for (int y = 0; y < rect.H; y++)
                {
                    int ix = (int)rect.X + x;
                    int iy = (int)rect.Y + y;
                    float vx = (rect.X + x) * configuration.PositionDelta;
                    float vz = (rect.Y + y) * configuration.PositionDelta;
                    Vector3 position = new Vector3(-vx, 0, vz);

                    RectangleF r1 = new RectangleF(ix, iy, 1, 1);
                    RectangleF r2 = new RectangleF(ix, iy, 1, 1);
                    r1.Inflate(1, 0);
                    r2.Inflate(0, 1);

                    if (two.Intersects(r1))
                    {
                        //Connection
                        connections.Add(new ModularSceneryAssetDescriptionConnection()
                        {
                            Position = tposition,
                            Direction = -Vector3.Normalize(position - tposition),
                            Type = ModularSceneryAssetDescriptionConnectionTypes.Open,
                        });
                    }

                    if (two.Intersects(r2))
                    {
                        //Connection
                        connections.Add(new ModularSceneryAssetDescriptionConnection()
                        {
                            Position = tposition,
                            Direction = Vector3.Normalize(position - tposition),
                            Type = ModularSceneryAssetDescriptionConnectionTypes.Open,
                        });
                    }
                }
            }

            return connections.ToArray();
        }

        private static IEnumerable<ModularSceneryAssetReference> CreateColumns(Rect rect, Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryAssetReference> references = new List<ModularSceneryAssetReference>();

            var roomRect = rect.GetRectangle();
            float half = configuration.PositionDelta * 0.5f;

            foreach (var column in dungeon.Columns)
            {
                if (!roomRect.Contains(column.X, column.Y))
                {
                    continue;
                }

                float vx = column.X * configuration.PositionDelta;
                float vz = column.Y * configuration.PositionDelta;

                ModularSceneryAssetReference col = new ModularSceneryAssetReference()
                {
                    AssetName = configuration.GetRandonColumn(),
                    Type = ModularSceneryAssetTypes.None,
                    Position = new Vector3(-vx + half, 0, vz - half),
                };

                references.Add(col);
            }

            return references.ToArray();
        }

        public static ModularSceneryLevels CreateLevels(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            ModularSceneryLevels levels = new ModularSceneryLevels()
            {
                Volumes = configuration.Volumes.ToArray(),
            };

            levels.Levels = new[] { CreateLevel(dungeon, configuration) };

            return levels;
        }

        private static ModularSceneryLevel CreateLevel(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            var maps = CreateMap(dungeon);
            var objs = CreateDoors(dungeon, configuration);

            var walls = MarkWalls(dungeon);
            var startWall = walls.First(w => w.Cell.X == 0 && w.Cell.Y == 0);
            var dir = startWall.GetFirstOpenDirection();

            ModularSceneryLevel level = new ModularSceneryLevel()
            {
                Name = dungeon.Title,
                StartPosition = Vector3.Zero,
                LookingVector = dir,
                Map = maps.ToArray(),
                Objects = objs.ToArray(),
            };

            return level;
        }

        private static IEnumerable<ModularSceneryAssetReference> CreateMap(Dungeon dungeon)
        {
            List<ModularSceneryAssetReference> mapList = new List<ModularSceneryAssetReference>();

            int rectIndex = 0;
            foreach (var rect in dungeon.Rects)
            {
                ModularSceneryAssetReference assetRef = new ModularSceneryAssetReference()
                {
                    AssetName = $"{rectIndex++}",
                };

                mapList.Add(assetRef);
            }

            return mapList.ToArray();
        }

        private static IEnumerable<ModularSceneryObjectReference> CreateDoors(Dungeon dungeon, DungeonAssetConfiguration configuration)
        {
            List<ModularSceneryObjectReference> objs = new List<ModularSceneryObjectReference>();

            int index = 0;
            foreach (var door in dungeon.Doors)
            {
                if (door.Type == (int)DoorTypes.None)
                {
                    continue;
                }

                var doorAssets = configuration.GetDoor((DoorTypes)door.Type);
                if (!doorAssets.Any())
                {
                    break;
                }

                float vx = door.X * configuration.PositionDelta;
                float vz = door.Y * configuration.PositionDelta;
                Vector3 dir = new Vector3(door.Dir.X, 0, -door.Dir.Y);
                string rot = EvaluateRotation(dir);

                ModularSceneryObjectReference obj = new ModularSceneryObjectReference
                {
                    AssetName = doorAssets.First(),
                    Id = $"door_{index++}",
                    Name = "door",
                    Type = ModularSceneryObjectTypes.Door,
                    Position = new Vector3(-vx, 0, vz) - (dir * (configuration.PositionDelta * 0.5f)),
                    RotationText = rot,
                    AnimationPlans = configuration.DoorAnimationPlans?.ToArray(),
                    Actions = configuration.DoorActions?.ToArray(),
                    States = configuration.DoorStates?.ToArray(),
                };
                objs.Add(obj);

                foreach (var asset in doorAssets.Skip(1))
                {
                    ModularSceneryObjectReference obj2 = new ModularSceneryObjectReference
                    {
                        AssetName = asset,
                        Type = ModularSceneryObjectTypes.Default,
                        Position = new Vector3(-vx, 0, vz) - (dir * (configuration.PositionDelta * 0.5f)),
                        RotationText = rot,
                    };
                    objs.Add(obj2);
                }
            }

            return objs.ToArray();
        }

        private static string EvaluateRotation(Vector3 dir)
        {
            if (dir == Vector3.ForwardLH)
            {
                return "Rot90";
            }
            if (dir == Vector3.BackwardLH)
            {
                return "Rot270";
            }
            if (dir == Vector3.Left)
            {
                return "";
            }
            if (dir == Vector3.Right)
            {
                return "Rot180";
            }

            return null;
        }

        struct Wall
        {
            public Rectangle Cell { get; set; }
            public WallDirections Dir { get; set; }

            public Vector3 GetFirstOpenDirection()
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

        [Flags]
        enum WallDirections
        {
            None = 0,
            N = 1,
            S = 2,
            E = 4,
            W = 8,
            All = 15,
        }
    }

    public enum DoorTypes
    {
        None = 0,
        Normal = 1,
        Archway = 2,
        Stairs = 3,
        Portcullis = 4,
        Special = 5,
        Secret = 6,
        Barred = 7,
    }
}
