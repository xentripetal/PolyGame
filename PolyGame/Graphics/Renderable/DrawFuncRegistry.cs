using Microsoft.Xna.Framework.Graphics;
using PolyGame.Components.Render;

namespace PolyGame.Graphics.Renderable;

public class DrawFuncRegistry
{
    public delegate void DrawFunc(RenderableReference renderable, SpriteBatch batch, ref ComputedCamera camera, Material material);
    protected FastList<DrawFunc> drawFuncs = new ();
    protected Dictionary<DrawFunc, int> drawFuncIndices = new ();

    /// <summary>
    /// Registers a draw function to be called when rendering a renderable.
    /// </summary>
    /// <param name="drawFunc">Index of registered draw func. This should be used when adding a renderable you want to use this draw func</param>
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