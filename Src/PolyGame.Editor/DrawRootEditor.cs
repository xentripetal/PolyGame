using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Scheduling.Configs;
using PolyECS.Systems;
using PolyGame.ImGuiNet;

namespace PolyGame.Editor;

[After<ImGuiSets>(ImGuiSets.Init)]
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
