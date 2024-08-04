// We can't do void as generics so use nullable object to represent void.
global using RunSystem = PolyECS.Systems.BaseSystem<PolyECS.Empty, PolyECS.Empty>;
