using Microsoft.Xna.Framework.Graphics;
using PolyECS;

namespace PolyGame.Editor;

public interface IEditorWindow
{
    bool Initialized { get; }
    bool IsShown { get; }
    string Name { get; }

    event Action<IEditorWindow>? Shown;

    event Action<IEditorWindow>? Closed;

    void Dispose();

    void DrawContent(PolyWorld world, GraphicsDevice device);

    void DrawMenu();

    void DrawWindow(PolyWorld world, GraphicsDevice device);

    void Init(PolyWorld world, GraphicsDevice device);

}
