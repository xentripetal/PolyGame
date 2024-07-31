using PolyECS.Systems;

namespace PolyECS;

public static class Sets
{
    public static SystemSet Named(string name) => new NamedSet(name);
    public static SystemSet Default = Named("Default");
}
