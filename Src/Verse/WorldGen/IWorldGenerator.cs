using Flecs.NET.Core;

namespace Verse.WorldGen;

public interface IChunkedWorldGenerator
{
    public void Seed(int seed);
    public Chunk GenerateChunk(Map map, ChunkPosition position);
}

public class StaticChunkFiller : IChunkedWorldGenerator
{
    public void Seed(int seed) { }

    public Chunk GenerateChunk(Map map, ChunkPosition position)
    {
        return null;
    }
}
