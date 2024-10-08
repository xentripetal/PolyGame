using PolyECS;
using PolyECS.Systems;
using PolyGame;
using PolyGame.Graphics.Camera;
using PolyGame.Transform;

namespace Verse;

public class SpawnCamera : ClassSystem<PolyWorld>
{
    protected override ITSystemParam<PolyWorld> CreateParam(PolyWorld world) => Param.OfWorld();

    public override void Run(PolyWorld world)
    {
        var cb = new CameraBundle
        {
            Camera = new Camera
            {
                PositionZ3D = 0,
                NearClipPlane3D = 0,
                FarClipPlane3D = 0,
                RawZoom = 1,
                Origin = default,
                Zoom = 0,
                ZoomBounds = new Range<float>(1, 4)
            },
            Inset = new CameraInset(),
            Transform = new TransformBundle()
        };
        cb.Apply(world.Entity("MainCamera"));
    }
}
