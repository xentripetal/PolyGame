using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Graphics;

/// <summary>
/// Sets the graphics render target to our final render before updating so that all viewport based logic is based on the correct scale.
/// </summary>
public class SetViewport : ClassSystem<ResMut<GraphicsDevice>, Res<FinalRenderTarget>>
{
    protected override (ISystemParam<ResMut<GraphicsDevice>>, ISystemParam<Res<FinalRenderTarget>>) CreateParams(PolyWorld world)
        => (Param.OfResMut<GraphicsDevice>(), Param.OfRes<FinalRenderTarget>());

    public override void Run(ResMut<GraphicsDevice> devRes, Res<FinalRenderTarget> targetRes)
    {
        if (targetRes.IsEmpty || devRes.IsEmpty)
        {
            return;
        }
        var dev = devRes.Get();
        var target = targetRes.Get();
        dev.SetRenderTarget(target.SceneRenderTarget);
    }
}
