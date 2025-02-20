using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyGame.Editor.Attributes;
using PolyGame.Editor.Widgets;

namespace PolyGame.Editor;

using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Widgets;
using Hexa.NET.ImGui.Widgets.Dialogs;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

public static class WindowManager
{
    private static GraphicsDevice? device;
    private static PolyWorld world;
    private static readonly List<IEditorWindow> windows = new();
    private static readonly List<EditorWindowCategory> categories = new();
    private static readonly EditorWindowCategory Default = new(string.Empty);

    static WindowManager()
    {
        categories.Add(Default);
        Register<HierarchyWidget>();
        Register<FPSTracker>();
        /**

        Register<OutputWidget>();

        Register<PreferencesWidget>();
        Register<PipelineWidget>();
        Register<AssetBrowser>();
        Register<GitWidget>();
        Register<PropertiesWidget>();

        Register<MixerWidget>();
        Register<PublishProjectWindow>();
        Register<SceneVariablesWindow>();
        Register<DebugWindow>();
        Register<ProfilerWindow>();
        Register<DeepProfilerWindow>();
        Register<PoseEditorWindow>();
        Register<MaterialEditorWindow>();
        Register<PostProcessWindow>();
        Register<InputWindow>();
        Register<TextEditorWindow>();
        Register<ImagePainterWindow>();
        Register<WeatherWidget>();
        Register<RenderGraphWidget>();
        Register<RendererWidget>();
        Register<MemoryWidget>();
        Register<NativeMemoryWidget>();
        Register<InputManagerWindow>();
        Register<PackageManagerWidget>();
        Register<ErrorListWidget>();

        Register<GraphicsDebugger>();

        Register<BakeWindow>();
        **/
    }

    public static IReadOnlyList<IEditorWindow> Windows => windows;

    public static IReadOnlyList<EditorWindowCategory> Categories => categories;

    public static bool BlockInput { get; internal set; }

    public static EditorWindowCategory? GetCategory(string categoryName)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            if (category.Name == categoryName)
            {
                return category;
            }
        }
        return null;
    }

    public static bool TryGetCategory(string categoryName, [NotNullWhen(true)] out EditorWindowCategory? category)
    {
        category = GetCategory(categoryName);
        return category is not null;
    }

    public static EditorWindowCategory GetOrAddCategory(string categoryName)
    {
        return GetCategory(categoryName) ?? AddCategory(categoryName);
    }

    public static EditorWindowCategory AddCategory(string categoryName)
    {
        EditorWindowCategory category = new(categoryName);
        categories.Add(category);
        return category;
    }

    public static bool HasCategory(string categoryName)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].Name == categoryName)
            {
                return true;
            }
        }
        return false;
    }

    public static T? GetWindow<T>() where T : class, IEditorWindow
    {
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (window is T t)
            {
                return t;
            }
        }
        return null;
    }

    public static IEditorWindow? GetWindow(string name)
    {
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (window.Name == name)
            {
                return window;
            }
        }
        return null;
    }

    public static bool TryGetWindow<T>([NotNullWhen(true)] out T? editorWindow) where T : class, IEditorWindow
    {
        editorWindow = GetWindow<T>();
        return editorWindow != null;
    }

    public static bool TryGetWindow(string name, [NotNullWhen(true)] out IEditorWindow? editorWindow)
    {
        editorWindow = GetWindow(name);
        return editorWindow != null;
    }

    public static bool HasWindow<T>() where T : class, IEditorWindow
    {
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (window is T)
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasWindow(string name)
    {
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (window.Name == name)
            {
                return true;
            }
        }
        return false;
    }

    public static void Register<T>() where T : IEditorWindow, new()
    {
        IEditorWindow window = new T();
        window.Shown += Shown;
        window.Closed += Closed;
        windows.Add(window);
        var type = typeof(T);
        var categoryAttr = type.GetCustomAttribute<EditorWindowCategoryAttribute>();
        if (categoryAttr == null)
        {
            Default.Add(window);
            return;
        }

        var category = GetOrAddCategory(categoryAttr.Category);
        category.Add(window);
    }

    public static void Register(IEditorWindow window)
    {
        window.Shown += Shown;
        window.Closed += Closed;
        windows.Add(window);
        var type = window.GetType();
        var categoryAttr = type.GetCustomAttribute<EditorWindowCategoryAttribute>();
        if (categoryAttr == null)
        {
            Default.Add(window);
            return;
        }

        var category = GetOrAddCategory(categoryAttr.Category);
        category.Add(window);
    }

    public static void Unregister<T>() where T : IEditorWindow, new()
    {
        IEditorWindow? window = windows.FirstOrDefault(x => x is T);
        if (window != null)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                category.Remove(window);
                if (category.Count == 0)
                {
                    categories.RemoveAt(i);
                    i--;
                }
            }

            window.Shown -= Shown;
            window.Closed -= Closed;
            if (window.Initialized)
            {
                window.Dispose();
            }

            windows.Remove(window);
        }
    }

    public static void Unregister(IEditorWindow window)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            var category = categories[i];
            category.Remove(window);
            if (category.Count == 0)
            {
                categories.RemoveAt(i);
                i--;
            }
        }

        window.Shown -= Shown;
        window.Closed -= Closed;
        if (window.Initialized)
        {
            window.Dispose();
        }

        windows.Remove(window);
    }

    private static void Closed(IEditorWindow window) { }

    private static void Shown(IEditorWindow window)
    {
        if (!window.Initialized && device != null)
        {
            window.Init(world, device);
        }
    }

    public static void Init(PolyWorld w, GraphicsDevice device)
    {
        WindowManager.device = device;
        world = w;
        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            if (window.IsShown)
            {
                window.Init(world, device);
            }
        }
    }

    //[Profiling.Profile]
    public static void Draw(PolyWorld world, GraphicsDevice device)
    {
        ImGui.BeginDisabled(BlockInput);

        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];

            if (!window.IsShown)
            {
                continue;
            }

            //CPUProfiler.Global.Begin(window.Name);
            window.DrawWindow(world, device);
            //CPUProfiler.Global.End(window.Name);
        }

        ImGui.EndDisabled();

        DialogManager.Draw();
        MessageBoxes.Draw();
        AnimationManager.Tick();
    }

    public static unsafe void DrawMenu()
    {
        for (int i = 0; i < categories.Count; i++)
        {
            categories[i].DrawMenu();
        }
    }

    public static void Dispose()
    {
        for (int i = 0; i < categories.Count; i++)
        {
            categories[i].Clear();
        }
        categories.Clear();

        for (int i = 0; i < windows.Count; i++)
        {
            var window = windows[i];
            window.Shown -= Shown;
            window.Closed -= Closed;
            if (window.Initialized)
            {
                window.Dispose();
            }
        }

        windows.Clear();
    }
}
