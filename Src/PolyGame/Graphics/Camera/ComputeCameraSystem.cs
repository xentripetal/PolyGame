using Flecs.NET.Core;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Transform;

namespace PolyGame.Graphics.Camera;

public class ComputeCameraSystem : ClassSystem<Query, Res<Screen>>
{
    protected override (ISystemParam<Query>, ISystemParam<Res<Screen>>) CreateParams(PolyWorld world) => (
        Param.Of(world.QueryBuilder()
            .With<Camera>().In()
            .With<ComputedCamera>().InOut()
            .With<CameraInset>().In()
            .With<GlobalTransform2D>().In()
            .Build()),
        Param.OfRes<Screen>()
    );

    public override void Run(Query cameras, Res<Screen> screen)
    {
        cameras.Each((ref Camera camera, ref ComputedCamera cCam, ref CameraInset inset, ref GlobalTransform2D transform) => {
            cCam.Update(camera, transform.Value.Translation, transform.Value.RotationDegrees, screen.Get().GraphicsDevice.Viewport, inset);
        });
    }
}
