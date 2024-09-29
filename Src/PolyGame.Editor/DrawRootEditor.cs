using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Editor;

public class InitEditor : ClassSystem<PolyWorld, ResMut<GraphicsDevice>>
{
    protected override (ITSystemParam<PolyWorld>, ITSystemParam<ResMut<GraphicsDevice>>) CreateParams(PolyWorld world)
        => (Param.OfWorld(), Param.OfResMut<GraphicsDevice>());

    public override void Run(PolyWorld world, ResMut<GraphicsDevice> deviceRes)
    {
        Designer.Init(world, deviceRes.Get());
    }
}

public class DrawEditor : ClassSystem<PolyWorld, ResMut<GraphicsDevice>>
{
    protected override (ITSystemParam<PolyWorld>, ITSystemParam<ResMut<GraphicsDevice>>) CreateParams(PolyWorld world)
        => (Param.OfWorld(), Param.OfResMut<GraphicsDevice>());

    public override void Run(PolyWorld world, ResMut<GraphicsDevice> deviceRes)
    {
        Designer.Draw(world, deviceRes.Get());
    }
}
