using System;
using System.Collections.Generic;
using System.Threading;
using Flecs.NET.Core;
using PolyGame.Assets;

namespace PolyGame.Tests.Assets;

public class AssetServerTests
{
    [Fact]
    public void DisposesHandles()
    {
        using var world = World.Create();
        var server = new AssetServer([world]);
        server.AddLoader(new MockLoader());
        var h1 = server.Load<DisposableComponent>("file.test", false);
        var h2 = server.Load<DisposableComponent>("file.test", false);
        Assert.Equal(h1, h2);
        Assert.True(h1.Valid());
        Assert.True(server.IsLoaded(h1));
        var component = server.Get(h1);
        Assert.False(component.Disposed);
        server.Release(h2);
        Assert.True(server.IsLoaded(h1));
        Assert.False(component.Disposed);
        server.Release(h1);
        Assert.False(server.IsLoaded(h1));
        Assert.True(component.Disposed);
    }

    [Fact]
    public void DisposesComponentHandles()
    {
        using var world = World.Create();
        var server = new AssetServer([world]);
        server.AddLoader(new MockLoader());
        var e1 = world.Entity().Set(server.Load<DisposableComponent>("file.test"));
        var e2 = world.Entity().Set(server.Load<DisposableComponent>("file.test"));
        var h1 = e1.Get<Handle<DisposableComponent>>();
        // Wait for load
        while (!server.IsLoaded(h1))
        {
            Thread.Sleep(1);
        }
        e2.Destruct();
        Assert.True(server.IsLoaded(h1));
        var component = server.Get(h1);
        Assert.False(component.Disposed);
        e1.Remove<Handle<DisposableComponent>>();
        Assert.False(server.IsLoaded(h1));
        Assert.True(component.Disposed);
    }

    [Fact]
    public void MultiWorldDisposesComponentHandles()
    {
        using var world1 = World.Create();
        using var world2 = World.Create();
        var server = new AssetServer([world1, world2]);
        server.AddLoader(new MockLoader());
        var e1 = world1.Entity().Set(server.Load<DisposableComponent>("file.test"));
        var e2 = world2.Entity().Set(server.Load<DisposableComponent>("file.test"));
        var h1 = e1.Get<Handle<DisposableComponent>>();
        // Wait for load
        while (!server.IsLoaded(h1))
        {
            Thread.Sleep(1);
        }
        e2.Destruct();
        Assert.True(server.IsLoaded(h1));
        var component = server.Get(h1);
        e1.Remove<Handle<DisposableComponent>>();
        Assert.False(server.IsLoaded(h1));
        Assert.True(component.Disposed);
    }

    [Fact]
    public void UnknownFileTypeInvalid()
    {
        using var world = World.Create();
        var server = new AssetServer([world]);
        server.AddLoader(new MockLoader());
        var h1 = server.Load<DisposableComponent>("file.unknown", false);
        Assert.False(h1.Valid());

        Assert.False(server.IsLoaded(h1));
        Assert.Equal(AssetServer.LoadState.Unloaded, server.GetState(h1));
        Assert.False(server.Release(h1));
    }

    private class MockLoader : IAssetLoader
    {
        public IEnumerable<string> SupportedExtensions => new[]
        {
            "test"
        };

        public T Load<T>(AssetPath path)
        {
            if (typeof(T) == typeof(DisposableComponent))
            {
                return (T)(object)new DisposableComponent();
            }
            return default;
        }

        public void Unload(AssetPath path, object asset)
        {
            if (asset is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private class DisposableComponent : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
