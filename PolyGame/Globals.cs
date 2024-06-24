using Microsoft.Xna.Framework.Graphics;

namespace PolyGame;

/// <summary>
/// Globals contains statics that are used throughout the game from Nez that I'm too lazy to port over into carrying over every constructor. I'm sorry.
/// </summary>
public static class Globals
{
    public static SamplerState DefaultSamplerState = new SamplerState
    {
        Filter = TextureFilter.Point
    };
    public static bool DebugRenderEnabled = false;
}
