# Bundles 

## Bundle Query Views
`07-26-2024`

I wonder if theres a way to treat bundles as a query view. Such that instead of doing 
```csharp
var query = world.Query().With<Transform>.With<Sprite>.With<Handle<Texture2D>>.With<Visibility>();
query.Each((Entity en, ref Transform t, ref Sprite s, ref Handle<Texture2D> h, ref Visibility v) => {
    // Do something
});
```

you could do
```csharp
var query = world.Query().WithBundle<SpriteBundle>();
query.Each(ref SpriteBundle bundle) => {
    // Do something
});
```

I'd definitely need to implement my own Query type wrapping a flecs query. However, that was already something I was
considering for doing typed query generation (e.g. `Query<(T1, T2, T3), Changed<T1>>`)