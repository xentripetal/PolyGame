using System.Diagnostics;
using System.Runtime.InteropServices;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Serilog;

namespace PolyGame;

/// <summary>
/// Handles loading and saving assets from a given path and managing <see cref="Handle{T}"/>s to those assets.
/// </summary>
public class AssetServer
{
    public AssetServer(World[] worlds)
    {
        this.worlds = worlds;
    }

    // TODO - Implement watch for changes
    public bool WatchForChanges { get; set; }

    public void AddLoader(IAssetLoader loader)
    {
        foreach (var extension in loader.SupportedExtensions)
        {
            if (!loadersByExtension.TryAdd(extension, loader))
            {
                Log.Warning("Asset Loader {Loader} will be ignored for extension {Extension} as {MainLoader} already claimed it", loader, extension,
                    loadersByExtension[extension]);
            }
        }
    }


    protected Dictionary<string, IAssetLoader> loadersByExtension = new ();
    protected ListPool<Asset> assets = new ();
    protected Dictionary<AssetPath, (int, ushort)> assignedHandles = new ();
    protected ReaderWriterLockSlim handleLock = new ();
    protected World[] worlds;

    public struct Asset
    {
        public LoadState State;
        public AssetPath Path;
        public object? Value;
        public int HandleCount;
    }

    public enum LoadState
    {
        Unloaded,
        Loading,
        PendingDispose,
        Loaded,
        Failed
    }

    internal unsafe void HandleDtor<T>(void* data, int count, flecs.ecs_type_info_t* typeInfoHandle)
    {
        Handle<T>* handles = (Handle<T>*)data;
        for (int i = 0; i < count; i++)
        {
            Release(handles[i]);
        }
    }

    protected Handle<T> CreateHandle<T>(int id, ushort generation)
    {
        unsafe
        {
            lock (this)
            {
                // TODO - I really hate this and it seems dangerous in multithreading scenarios.
                // Maybe we implement our own Register component with a custom hook?
                foreach (var world in worlds)
                {
                    if (!Type<Handle<T>>.IsRegistered(world.Handle))
                    {
                        var component = Type<Handle<T>>.RegisterComponent(world, true, true, 0, "");
                        flecs.ecs_type_hooks_t hooksDesc = default;
                        hooksDesc.dtor = Marshal.GetFunctionPointerForDelegate(HandleDtor<T>);
                        flecs.ecs_set_hooks_id(world, component, &hooksDesc);
                    }
                }
            }
        }
        return new Handle<T>(id, generation);
    }

    public Handle<T> Clone<T>(Handle<T> handle)
    {
        if (!handle.Valid())
        {
            return Handle<T>.Invalid;
        }
        handleLock.EnterWriteLock();
        try
        {
            if (handle.Generation() != assets.GetGeneration(handle.Index()))
            {
                return Handle<T>.Invalid;
            }
            var asset = assets[handle.Index()];
            asset.HandleCount++;
            assets[handle.Index()] = asset;
            return CreateHandle<T>(handle.Index(), handle.Generation());
        }
        finally
        {
            handleLock.ExitWriteLock();
        }
    }

    public Handle<T> Load<T>(AssetPath path, bool async = true)
    {
        if (!loadersByExtension.TryGetValue(path.Extension, out var loader))
            return Handle<T>.Invalid;

        handleLock.EnterWriteLock();
        Handle<T> handle;
        try
        {
            // If the asset is already loaded or loading, return the handle.
            if (assignedHandles.TryGetValue(path, out var idx))
            {
                var asset = assets[idx.Item1];
                asset.HandleCount++;
                assets[idx.Item1] = asset;
                return CreateHandle<T>(idx.Item1, idx.Item2);
            }
            // Else claim a spot for it and trigger the load
            var (id, gen) = assets.Add(new Asset
            {
                State = LoadState.Loading,
                HandleCount = 1,
            });
            assignedHandles.Add(path, (id, gen));
            handle = CreateHandle<T>(id, gen);
        }
        finally
        {
            handleLock.ExitWriteLock();
        }
        if (async)
        {
            Task.Run(() => {
                LoadInternal<T>(handle.Index(), handle.Generation(), path, loader);
            });
        }
        else
        {
            LoadInternal<T>(handle.Index(), handle.Generation(), path, loader);
        }
        return handle;
    }

    protected void LoadInternal<T>(int id, ushort gen, AssetPath path, IAssetLoader loader)
    {
        object? data = null;
        bool valid = false;
        try
        {
            data = loader.Load<T>(path);
            valid = true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load asset {Path}", path);
            valid = false;
        }

        handleLock.EnterWriteLock();
        try
        {
            var asset = assets[id];
            var validGen = gen == assets.GetGeneration(id);
            if (asset.State == LoadState.Loading && validGen)
            {
                asset.State = valid ? LoadState.Loaded : LoadState.Failed;
                asset.Value = data;
                assets[id] = asset;
            }
            else if (asset.State == LoadState.PendingDispose && validGen)
            {
                if (valid && data != null)
                {
                    loader.Unload(path, data);
                }
                asset.State = LoadState.Unloaded;
                assets[id] = asset;
                assets.Free(id);
                assignedHandles.Remove(path);
            }
            else
            {
                Log.Warning("Asset {Path} was loaded but is in an unexpected state {State} or Generation", path, asset.State);
                if (valid && data is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
        finally
        {
            handleLock.ExitWriteLock();
        }
    }

    public Handle<T> Load<T>(string path, bool async = true)
    {
        return Load<T>(new AssetPath(path), async);
    }

    public T? Get<T>(Handle<T> handle)
    {
        if (!handle.Valid())
        {
            return default;
        }
        handleLock.EnterReadLock();
        try
        {
            if (handle.Generation() != assets.GetGeneration(handle.Index()))
            {
                return default;
            }

            var asset = assets[handle.Index()];
            if (asset.State == LoadState.Loaded)
            {
                Debug.Assert(asset.Value != null, "Asset.Value is null when state is Loaded");
                Debug.Assert(asset.Value is T, "Asset.Value is not of type T when state is Loaded");
                return (T)asset.Value;
            }
        }
        finally
        {
            handleLock.ExitReadLock();
        }
        return default;
    }

    public bool IsLoaded<T>(Handle<T> handle)
    {
        return GetState(handle) == LoadState.Loaded;
    }

    public LoadState GetState<T>(Handle<T> handle)
    {
        if (!handle.Valid())
        {
            return LoadState.Unloaded;
        }
        handleLock.EnterReadLock();
        try
        {
            if (handle.Generation() != assets.GetGeneration(handle.Index()))
            {
                return LoadState.Unloaded;
            }
            return assets[handle.Index()].State;
        }
        finally
        {
            handleLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Releases the given handle. If there are no more references to the asset, it will be unloaded.
    /// </summary>
    /// <param name="handle"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns>True if the asset was fully disposed after this release. </returns>
    public bool Release<T>(Handle<T> handle)
    {
        if (!handle.Valid())
        {
            return false;
        }

        handleLock.EnterWriteLock();
        try
        {
            var idx = handle.Index();
            if (idx >= assets.Capacity)
            {
                return false;
            }
            if (handle.Generation() != assets.GetGeneration(handle.Index()))
            {
                // The handle is invalid/expired and we can assume it was already released.
                return true;
            }
            var asset = assets[idx];
            asset.HandleCount--;
            if (asset.HandleCount == 0)
            {
                UnloadInLock(idx);
                return true;
            }
            else
            {
                assets[idx] = asset;
            }
        }
        finally
        {
            handleLock.ExitWriteLock();
        }
        return false;
    }

    protected void UnloadInLock(int id)
    {
        var asset = assets[id];
        if (asset.State == LoadState.Loaded)
        {
            var loader = loadersByExtension[asset.Path.Extension];
            loader.Unload(asset.Path, asset.Value);
            asset.State = LoadState.Unloaded;
            assets[id] = asset;
            assets.Free(id);
            assignedHandles.Remove(asset.Path);
        }
        else if (asset.State == LoadState.Loading)
        {
            asset.State = LoadState.PendingDispose;
            assets[id] = asset;
        }
        else
        {
            throw new InvalidOperationException("Asset is in an invalid state to be unloaded");
        }
    }

    public void Unload<T>(Handle<T> handle)
    {
        if (!handle.Valid())
        {
            return;
        }
        handleLock.EnterWriteLock();
        try
        {
            if (handle.Generation() != assets.GetGeneration(handle.Index()))
            {
                return;
            }
            UnloadInLock(handle.Index());
        }
        finally
        {
            handleLock.ExitWriteLock();
        }
    }
}
