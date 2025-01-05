using Flecs.NET.Core;

namespace PolyECS;

public static class FlecsUtils
{
    public static int GetTermCount(this QueryBuilder builder)
    {
        int i = 0;
        for (; i < 32; i++)
        {
            if (builder.Desc.terms[i] == default)
                break;
        }
        return i;
    }
}