using System.Diagnostics;
using System.Reflection.Metadata;
using Flecs.NET.Core;
using Microsoft.Xna.Framework.Content;
using Serilog;

namespace PolyGame;

public struct AssetPath : IEquatable<AssetPath>
{
    /// <summary>
    /// Named source of the asset, such as http:// or file://. Null will be treated as a relative path.
    /// </summary>
    public string? Source;
    /// <summary>
    /// Path to the asset. 
    /// </summary>
    public string Path;
    /// <summary>
    /// File extension of the asset.
    /// </summary>
    public string Extension;
    /// <summary>
    /// An optional label for referencing elements internal to the path asset.
    /// </summary>
    public string? Label;

    /// <summary>
    /// Creates a separate asset path from the given full path.
    /// resource.txt -> AssetPath(Path: "resource.txt")
    /// http://example.com/resource.txt -> AssetPath(Source: "http://example.com", Path: "resource.txt")
    /// Image/Sprites.Atlas#Sprite1 -> AssetPath(Path: "Image/Sprites.Atlas", Label: "Sprite1")
    /// </summary>
    /// <param name="fullPath"></param>
    public AssetPath(string fullPath)
    {
        var sourceIndex = fullPath.IndexOf("://");
        if (sourceIndex > 0)
        {
            Source = fullPath.Substring(0, sourceIndex);
            Path = fullPath.Substring(sourceIndex + 3);
        }
        else
        {
            Source = null;
            Path = fullPath;
        }

        var labelIndex = Path.LastIndexOf('#');
        if (labelIndex > 0)
        {
            Label = Path.Substring(labelIndex + 1);
            Path = Path.Substring(0, labelIndex);
        }
        else
        {
            Label = null;
        }

        (Path, Extension) = PathAndExtension(Path);
    }

    public AssetPath(string path, string source, string label)
    {
        (Path, Extension) = PathAndExtension(path);
        Source = source;
        Label = label;
    }

    public static (string, string) PathAndExtension(string path)
    {
        var extIndex = path.LastIndexOf('.');
        if (extIndex > 0)
        {
            return (path.Substring(0, extIndex), path.Substring(extIndex + 1).ToLower());
        }
        return (path, "");
    }

    public bool Equals(AssetPath other) => Source == other.Source && Path == other.Path && Extension == other.Extension && Label == other.Label;

    public override bool Equals(object? obj) => obj is AssetPath other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Source, Path, Extension, Label);
}

/// <summary>
/// Handles loading and saving assets from a given path and managing <see cref="Handle{T}"/>s to those assets.
/// </summary>
public class AssetServer
{
    public AssetServer(IEnumerable<IAssetLoader> loaders, bool watchForChanges = false)
    {
        foreach (var loader in loaders)
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
    }


    protected Dictionary<string, IAssetLoader> loadersByExtension = new ();
    protected ListPool<Asset> assets = new ();
    protected Dictionary<AssetPath, int> assignedHandles = new ();

    protected ReaderWriterLockSlim handleLock = new ();

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

    public Handle<T> Load<T>(AssetPath path, bool async = true)
    {
        if (loadersByExtension.TryGetValue(path.Extension, out var loader))
        {
            int id;
            handleLock.EnterWriteLock();
            try
            {
                // If the asset is already loaded or loading, return the handle.
                if (assignedHandles.TryGetValue(path, out id))
                {
                    var asset = assets[id];
                    asset.HandleCount++;
                    assets[id] = asset;
                    return new Handle<T>(this, path, id);
                }
                // Else claim a spot for it and trigger the load
                id = assets.Add(new Asset
                {
                    State = LoadState.Loading,
                    HandleCount = 1,
                });
                assignedHandles.Add(path, id);
            }
            finally
            {
                handleLock.ExitWriteLock();
            }
            if (async)
            {
                Task.Run(() => {
                    LoadInternal<T>(id, path, loader);
                });
            }
            else
            {
                LoadInternal<T>(id, path, loader);
            }
            return new Handle<T>(this, path, id);
        }
        return new Handle<T>(this, path, -1);
    }

    protected void LoadInternal<T>(int id, AssetPath path, IAssetLoader loader)
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
            if (asset.State == LoadState.Loading)
            {
                asset.State = valid ? LoadState.Loaded : LoadState.Failed;
                asset.Value = data;
                assets[id] = asset;
            }
            else if (asset.State == LoadState.PendingDispose)
            {
                if (valid && data is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                assets.RemoveAt(id);
            }
            else
            {
                Log.Warning("Asset {Path} was loaded but is in an unexpected state {State}", path, asset.State);
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
        var id = handle.Id();
        if (id < 0)
        {
            throw new InvalidOperationException("Invalid Handle");
        }
        handleLock.EnterReadLock();
        try
        {
            var asset = assets[id];
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
        if (handle.Id() < 0)
        {
            return LoadState.Unloaded;
        }
        handleLock.EnterReadLock();
        try
        {
            var asset = assets[handle.Id()];
            Debug.Assert(asset.Path.Equals(handle.Path()), "Handle path does not match asset path!", "Handle {handle} points to mismatched asset entry {asset}");
            return assets[handle.Id()].State;
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
        var id = handle.Id();
        if (id < 0)
        {
            return false;
        }
        handleLock.EnterWriteLock();
        try
        {
            var asset = assets[id];
            asset.HandleCount--;
            if (asset.HandleCount == 0)
            {
                UnloadInLock(id);
                return true;
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
            if (asset.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        assets.RemoveAt(id);
        assignedHandles.Remove(asset.Path);
    }

    public void Unload<T>(Handle<T> handle)
    {
        if (handle.Id() < 0)
        {
            return;
        }
        handleLock.EnterWriteLock();
        try
        {
            UnloadInLock(handle.Id());
        }
        finally
        {
            handleLock.ExitWriteLock();
        }
    }
}
