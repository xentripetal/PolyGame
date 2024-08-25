using PolyGame.Assets;
using PolyGame.Graphics.Renderers;
using Serilog;

namespace PolyGame.Graphics.Renderable;

public class DrawFuncRegistry
{
    public delegate void DrawFunc(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch);
    protected Dictionary<DrawFunc, int> drawFuncIndices = new ();
    protected FastList<DrawFunc> drawFuncs = new ();

    public DrawFuncRegistry()
    {
        RegisterDrawFunc(NoopDraw);
    }

    /// <summary>
    /// A no-op draw function that does nothing. This is always registered as index 0.
    /// </summary>
    /// <param name="assets"></param>
    /// <param name="renderable"></param>
    /// <param name="batch"></param>
    public void NoopDraw(Renderer renderer, AssetServer assets, RenderableReference renderable, Batcher batch)
    {
        Log.Debug("Noop draw function called for renderable {Renderable}", renderable.Entity);
    }

    /// <summary>
    ///     Registers a draw function to be called when rendering a renderable.
    /// </summary>
    /// <param name="drawFunc">
    ///     Index of registered draw func. This should be used when adding a renderable you want to use this
    ///     draw func
    /// </param>
    /// <returns></returns>
    public int RegisterDrawFunc(DrawFunc drawFunc)
    {
        if (drawFuncIndices.TryGetValue(drawFunc, out var index))
        {
            return index;
        }
        drawFuncs.Add(drawFunc);
        index = drawFuncs.Length - 1;
        drawFuncIndices[drawFunc] = index;
        return index;
    }

    public DrawFunc GetDrawFunc(int index) => drawFuncs.Buffer[index];
}
