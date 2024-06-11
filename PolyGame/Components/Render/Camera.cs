using Microsoft.Xna.Framework;

namespace PolyGame.Components.Render;

public struct ComputedCamera
{
    public Matrix2D TransformMatrix;
    public Matrix2D InverseTransformMatrix;
    public Matrix ProjectionMatrix;

    public ComputedCamera(Camera cam, Transform.Transform transform, Point viewport)
    {
        Update(cam, transform, viewport);
    }

    public void Update(Camera cam, Transform.Transform transform, Point viewport)
    {
        var origin = new Vector2(viewport.X / 2f, viewport.Y / 2f);
        Update(cam, transform, viewport, origin);
    }

    public void Update(Camera cam, Vector2 translation, float radians, Point viewport)
    {
        var origin = new Vector2(viewport.X / 2f, viewport.Y / 2f);
        Update(cam, translation, radians, viewport, origin);
    }

    public void Update(Camera cam, Transform.Transform transform, Point viewport, Vector2 origin)
    {
        var euler = transform.Quat.ToEuler();
        Update(cam, transform.Translation.XY(), euler.Z, viewport, origin);
    }

    public void Update(Camera cam, Vector2 translation, float radians, Point viewport, Vector2 origin)
    {
        TransformMatrix = cam.TransformMatrix(translation, origin, radians);
        Matrix2D.Invert(ref TransformMatrix, out InverseTransformMatrix);
        ProjectionMatrix = cam.ProjectionMatrix(viewport.X, viewport.Y);
    }
}

public struct Camera
{
    public float PositionZ3D = 2000f;

    /// <summary>
    /// near clip plane of the 3D camera projection
    /// </summary>
    public float NearClipPlane3D = 0.0001f;

    /// <summary>
    /// far clip plane of the 3D camera projection
    /// </summary>
    public float FarClipPlane3D = 5000f;

    /// <summary>
    /// Raw zoom value for the camera. 
    /// </summary>
    public float RawZoom = 1f;

    public Vector2 Origin;

    /// <summary>
    /// the zoom value should be between -1 and 1. This value is then translated to be from minimumZoom to maximumZoom. This lets you set
    /// appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
    /// </summary>
    /// <value>The zoom.</value>
    public float Zoom
    {
        get
        {
            if (RawZoom == 0)
                return 1f;
            if (RawZoom < 1)
                return Mathf.Map(RawZoom, ZoomBounds.Start, 1, -1, 0);
            return Mathf.Map(RawZoom, 1, ZoomBounds.End, 0, 1);
        }
        set => SetZoom(value);
    }

    /// <summary>
    /// Minimum and Maximum bounds for the zoom level. Default is 0.3 to 3. This is only enforced if you use <see cref="SetZoom"/>.
    /// </summary>
    public Range<float> ZoomBounds = new (0.3f, 3f);


    /// <summary>
    /// sets the the zoom value which should be between -1 and 1. This value is then translated to be from minimumZoom to maximumZoom.
    /// This lets you set appropriate minimum/maximum values then use a more intuitive -1 to 1 mapping to change the zoom.
    /// </summary>
    /// <param name="zoom">Zoom.</param>
    public void SetZoom(float zoom)
    {
        var newZoom = Math.Clamp(zoom, -1, 1);
        if (newZoom == 0)
        {
            RawZoom = 1f;
        }
        else if (newZoom < 0)
        {
            RawZoom = Mathf.Map(newZoom, -1, 0, ZoomBounds.Start, 1);
        }
        else
        {
            RawZoom = Mathf.Map(newZoom, 0, 1, 1, ZoomBounds.End);
        }
    }


    /// <summary>
    /// Gets the 2D cameras projection matrix
    /// </summary>
    /// <param name="width">Viewport width</param>
    /// <param name="height">Viewport height</param>
    /// <returns>The projection matrix</returns>
    public Matrix ProjectionMatrix(int width, int height)
    {
        Matrix.CreateOrthographicOffCenter(0, width, height, 0, 0, -1, out var projectionMatrix);
        return projectionMatrix;
    }

    public Matrix2D TransformMatrix(Vector2 translation, Vector2 origin, float radians)
    {
        Matrix2D tempMat;
        var mat = Matrix2D.CreateTranslation(-translation.X, -translation.Y); // position

        if (RawZoom != 1f)
        {
            Matrix2D.CreateScale(RawZoom, RawZoom, out tempMat); // scale ->
            Matrix2D.Multiply(ref mat, ref tempMat, out mat);
        }

        if (radians != 0f)
        {
            Matrix2D.CreateRotation(radians, out tempMat); // rotation
            Matrix2D.Multiply(ref mat, ref tempMat, out mat);
        }

        Matrix2D.CreateTranslation((int)origin.X, (int)origin.Y, out tempMat); // translate -origin
        Matrix2D.Multiply(ref mat, ref tempMat, out mat);
        return mat;
    }

    public Camera() { }
}
