using Microsoft.Xna.Framework;

namespace PolyGame.Graphics.Camera;

public class CameraPlugin : IPlugin
{
    public void Apply(App app)
    {
        app.RegisterType<Matrix2D>().Member<float>("M11").Member<float>("M12").Member<float>("M21").Member<float>("M22").Member<float>("M31")
            .Member<float>("M32");
        app.RegisterType<Matrix>().Member<float>("M11").Member<float>("M12").Member<float>("M13").Member<float>("M14").Member<float>("M21")
            .Member<float>("M22").Member<float>("M23").Member<float>("M24").Member<float>("M31").Member<float>("M32").Member<float>("M33").Member<float>("M34")
            .Member<float>("M41").Member<float>("M42").Member<float>("M43").Member<float>("M44");

        app.RegisterComponent<RectangleF>()
            .RegisterComponent<Range<float>>()
            .RegisterComponent<ComputedCamera>()
            .RegisterComponent<Camera>()
            .RegisterComponent<CameraInset>()
            .AddSystem<ComputeCameraSystem>(Schedules.PostUpdate);
    }
}
