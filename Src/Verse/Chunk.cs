using Flecs.NET.Core;

namespace Verse;

public class MapManager
{
    public MapManager(World world) { }
}

public class Map
{
    public readonly ushort ChunkWidth;
    public readonly ushort ChunkHeight;
    public readonly byte ChunkLayers;
}

/// <summary>
/// Sparse chunk is a chunk optimized for sparsely filled tiles and layers. This should be used for things like space maps.
/// It is recommended to use very large chunks with this type.
/// </summary>
public class SparseChunk : Chunk { }

/// <summary>
/// Dense chunk is a chunk optimized for highly filled tiles and layers. This should be used for normal maps and levels.
/// </summary>
public class DenseChunk : Chunk { }

public abstract class Chunk : IDisposable
{
    public Chunk(Entity entity, ushort width, ushort height)
    {
        Width = width;
        Height = height;

        Tiles = new Tile?[NumLayers][];
        TileEntities = new Entity?[NumLayers][];
        for (int i = 0; i < NumLayers; i++)
        {
            Tiles[i] = new Tile[width * height];
            TileEntities[i] = new Entity[width * height];
        }

        var observer = entity.CsWorld().Observer().With<Tile>().With<TilePosition>().With<TileLayer>().With(Ecs.ChildOf, entity).Event(Ecs.OnSet)
            .Each(OnTileChange);
    }
    
    protected abstract void OnTileChange(Iter it, int i, ref Tile tile, ref TilePosition pos, ref TileLayer layer);

    public void register() { }

    public readonly Entity Entity;
    public readonly ushort Width;
    public readonly ushort Height;
    public readonly byte NumLayers;

    protected Tile[][] Tiles;
    protected Entity[][] TileEntities;
    protected Observer tileObserver;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Entity? GetObjectAt(ChunkPosition position)
    {
        return null;
    }

    public void Dispose()
    {
        tileObserver.Dispose();
    }
}

public struct ChunkPosition(int X, int Y) { }
public struct TileChunkPosition(byte X, byte Y) { }
public struct TilePosition(int x, int Y) { }
public struct TileLayer(ushort Value);

public struct Tile
{
    ushort AtlasIndex;
}
