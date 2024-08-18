using PolyGame.Assets;
using PolyGame.Graphics;
using PolyGame.Graphics.Camera;
using PolyGame.Input;
using PolyGame.Transform;

namespace PolyGame;

public class DefaultPlugins : IPluginBundle
{
    public void Apply(App app)
    {
        app.AddPlugin(new AssetsPlugin())
            .AddPlugin(new TransformPlugin())
            .AddPlugin(new RenderPlugin())
            .AddPlugin(new DebugPlugin())
            .AddPlugin(new CameraPlugin())
            .AddPlugin(new InputPlugin())
            ;
    }
}
