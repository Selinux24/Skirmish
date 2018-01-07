using System;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Tile Cache
    /// </summary>
    public class TileCache
    {
        public const int MaxLayers = 32;

        private TileCacheObstacle[] m_obstacles = null;
        private TileCacheObstacle m_nextFreeObstacle = null;
        private int m_tileLutSize;
        private int m_tileLutMask;
        private CompressedTile[] m_tiles = null;
        private CompressedTile[] m_posLookup = null;
        private CompressedTile m_nextFreeTile = null;
        private uint m_tileBits;
        private uint m_saltBits;

        /// <summary>
        /// Constructor
        /// </summary>
        public TileCache()
        {

        }

        public void Init(TileCacheParams tcparams)
        {
            // Alloc space for obstacles.
            m_obstacles = new TileCacheObstacle[tcparams.MaxObstacles];
            m_nextFreeObstacle = null;
            for (int i = tcparams.MaxObstacles - 1; i >= 0; i--)
            {
                m_obstacles[i] = new TileCacheObstacle();
                m_obstacles[i].Salt = 1;
                m_obstacles[i].Next = m_nextFreeObstacle;
                m_nextFreeObstacle = m_obstacles[i];
            }

            // Init tiles
            m_tileLutSize = Helper.NextPowerOfTwo(tcparams.MaxTiles / 4);
            if (m_tileLutSize == 0) m_tileLutSize = 1;
            m_tileLutMask = m_tileLutSize - 1;

            m_tiles = new CompressedTile[tcparams.MaxTiles];
            m_posLookup = new CompressedTile[m_tileLutSize];

            for (int i = tcparams.MaxTiles - 1; i >= 0; i--)
            {
                m_tiles[i] = new CompressedTile();
                m_tiles[i].Salt = 1;
                m_tiles[i].Next = m_nextFreeTile;
                m_nextFreeTile = m_tiles[i];
            }

            // Init ID generator values.
            m_tileBits = (uint)Math.Log(Helper.NextPowerOfTwo(tcparams.MaxTiles), 2);

            // Only allow 31 salt bits, since the salt mask is calculated using 32bit uint and it will overflow.
            m_saltBits = Math.Min(31, 32 - m_tileBits);
            if (m_saltBits < 10)
            {
                throw new EngineException("NavMesh DT_INVALID_PARAM");
            }
        }

        public CompressedTile AddTile(TileCacheData data, uint flags)
        {
            // Make sure the data is in right format.
            TileCacheLayerHeader header = data.Header;
            if (header.magic != TileCacheLayerHeader.TileCacheMagic)
            {
                throw new EngineException("DT_WRONG_MAGIC");
            }
            if (header.version != TileCacheLayerHeader.TileCacheVersion)
            {
                throw new EngineException("DT_WRONG_VERSION");
            }

            // Make sure the location is free.
            if (GetTileAt(header.tx, header.ty, header.tlayer))
            {
                throw new EngineException("DT_FAILURE");
            }

            // Allocate a tile.
            CompressedTile tile = null;
            if (m_nextFreeTile != null)
            {
                tile = m_nextFreeTile;
                m_nextFreeTile = tile.Next;
                tile.Next = null;
            }

            // Insert tile into the position lut.
            int h = ComputeTileHash(header.tx, header.ty, m_tileLutMask);
            tile.Next = m_posLookup[h];
            m_posLookup[h] = tile;

            // Init tile.
            int headerSize = Helper.Align4(TileCacheLayerHeader.Size);
            tile.Header = data.Header;
            tile.Data = data.Data;
            tile.DataSize = data.DataSize;
            tile.Data.CopyTo(tile.Compressed, headerSize);
            tile.CompressedSize = tile.DataSize - headerSize;
            tile.Flags = flags;

            return GetTileRef(tile);
        }

        private CompressedTile GetTileRef(CompressedTile tile)
        {
            throw new NotImplementedException();
        }

        private int ComputeTileHash(int tx, int ty, int m_tileLutMask)
        {
            throw new NotImplementedException();
        }

        private bool GetTileAt(int tx, int ty, int tlayer)
        {
            throw new NotImplementedException();
        }

        public void BuildNavMeshTilesAt(int x, int y, NavMesh nm)
        {

        }
    }
}
