using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Graphics;

/// <summary>
/// Sets the graphics render target to our final render before updating so that all viewport based logic is based on the correct scale.
/// </summary>
public partial class SetViewport : AutoSystem
{
    public void Run(GraphicsDevice device, FinalRenderTarget target)
    {
        device.SetRenderTarget(target.SceneRenderTarget);
    }
}
