using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Graphics;

public static class GraphicsHelper
{
    /// <summary>
    /// helper method that generates a single color texture of the given dimensions
    /// </summary>
    /// <returns>The single color texture.</returns>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="color">Color.</param>
    public static Texture2D CreateSingleColorTexture(GraphicsDevice device, int width, int height, Color color)
    {
        var texture = new Texture2D(device, width, height);
        var data = new Color[width * height];
        for (var i = 0; i < data.Length; i++)
            data[i] = color;

        texture.SetData(data);
        return texture;
    }
}
