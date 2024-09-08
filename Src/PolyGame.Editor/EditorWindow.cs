using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;

namespace PolyGame.Editor;

public abstract class EditorWindow : IEditorWindow
{
    protected bool IsDocked;
    protected ImGuiWindowFlags Flags;
    protected bool windowEnded;
    protected bool wasShown;
    protected bool initialized;
    protected bool isShown;

    protected abstract string Name { get; }

    string IEditorWindow.Name => Name;

    public bool IsShown
    {
        get => isShown;
        protected set => isShown = value;
    }

    public bool Initialized => initialized;

    public event Action<IEditorWindow>? Shown;

    public event Action<IEditorWindow>? Closed;

    public void Init(PolyWorld world, GraphicsDevice device)
    {
        InitWindow(world, device);
        initialized = true;
    }

    protected virtual void InitWindow(PolyWorld world, GraphicsDevice device) { }

    public virtual void DrawWindow(PolyWorld world, GraphicsDevice device)
    {
        if (!IsShown)
        {
            return;
        }

        if (!ImGui.Begin(Name, ref isShown, Flags))
        {
            if (wasShown)
            {
                OnClosed();
            }
            wasShown = false;
            ImGui.End();
            return;
        }

        if (!wasShown)
        {
            OnShown();
        }
        wasShown = true;

        windowEnded = false;

        DrawContent(world, device);

        if (!windowEnded)
        {
            ImGui.End();
        }
    }

    protected virtual void OnShown()
    {
        Shown?.Invoke(this);
    }

    protected virtual void OnClosed()
    {
        Closed?.Invoke(this);
    }

    public abstract void DrawContent(PolyWorld world, GraphicsDevice device);

    protected void EndWindow()
    {
        if (!IsShown)
        {
            return;
        }

        ImGui.End();
        windowEnded = true;
    }

    public virtual void DrawMenu()
    {
        if (ImGui.MenuItem(Name))
        {
            IsShown = true;
        }
    }

    public virtual void Show()
    {
        IsShown = true;
    }

    public virtual void Close()
    {
        IsShown = false;
    }

    public void Dispose()
    {
        DisposeCore();
        initialized = false;
    }

    protected virtual void DisposeCore() { }

    public void Focus()
    {
        ImGuiWindowPtr window = ImGui.FindWindowByName(Name);
        ImGui.FocusWindow(window, ImGuiFocusRequestFlags.UnlessBelowModal);
    }
}
