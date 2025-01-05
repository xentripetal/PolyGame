using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;
using PolyGame.Transform;

namespace PolyGame.Graphics.Camera;

public partial class ComputeCameraSystem : AutoSystem
{
    public void Run(
        TQuery<Camera, ComputedCamera, CameraInset, GlobalTransform2D, (In<AllTerms>, InOut<Term1>)> cameras,
        Screen screen)
    {
        cameras.Each(
            (ref Camera camera, ref ComputedCamera cCam, ref CameraInset inset, ref GlobalTransform2D transform) =>
            {
                cCam.Update(camera, transform.Value.Translation, transform.Value.RotationDegrees,
                    screen.GraphicsDevice.Viewport, inset);
            });
    }
}