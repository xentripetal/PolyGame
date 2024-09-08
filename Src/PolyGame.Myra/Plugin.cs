using Flecs.NET.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Myra;

public class MyraPlugin : IPlugin
{
    public void Apply(App app)
    {
        // Todo should ditch the statics if we can
        MyraEnvironment.Game = app;
        
        var desktop = new Desktop();
        app.SetResource(desktop);
        app.AddSystem<DrawMyraSystem>(Schedules.PostRender);
    }

    /// <summary>
    /// Renders all Myra Desktops. Myra is using statics and direct App/Game references, so we claim mutable access to the world for safety
    /// </summary>
    public class DrawMyraSystem : ClassSystem<ResMut<Desktop>, PolyWorld>
    {
        protected override (ISystemParam<ResMut<Desktop>>, ISystemParam<PolyWorld>) CreateParams(PolyWorld world)
            => (Param.OfResMut<Desktop>(), Param.OfWorld());

        public override void Run(ResMut<Desktop> desktop, PolyWorld world)
        {
            if (desktop.HasValue)
            {
                desktop.Get().Render();
            }
        }
    }
}
