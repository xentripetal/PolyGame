using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Editor;

public partial class InitEditor : AutoSystem
{
    public void Run(PolyWorld world, GraphicsDevice device)
    {
        Designer.Init(world, device);
    }
}

public partial class DrawEditor : AutoSystem
{
    public void Run(PolyWorld world, GraphicsDevice device)
    {
        Designer.Draw(world, device);
    }
}
