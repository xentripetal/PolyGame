using Flecs.NET.Core;
using Microsoft.Xna.Framework.Graphics;
using PolyECS;
using PolyECS.Queries;
using PolyECS.Systems;
using PolyGame.Transform;

namespace PolyGame.Graphics.Camera;

public partial class ComputeCameraSystem : AutoSystem
{
    [AutoRunMethod]
    public void Run(TQuery<Components<R<Camera>, ComputedCamera, R<CameraInset>, R<GlobalTransform2D>>> cameras, Screen screen)
    {
        cameras.Query.Each((ref Camera camera, ref ComputedCamera cCam, ref CameraInset inset, ref GlobalTransform2D transform) => {
            cCam.Update(camera, transform.Value.Translation, transform.Value.RotationDegrees, screen.GraphicsDevice.Viewport, inset);
        });
    }
}
