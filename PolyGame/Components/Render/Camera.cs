using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Components.Render;

public struct CameraInset
{
    public float Left;
    public float Right;
    public float Top;
    public float Bottom;
}

public struct ComputedCamera
{
    /// <summary>
    /// Used to convert from world coordinates to screen
    /// </summary>
    public Matrix2D TransformMatrix;
    /// <summary>
    /// Used to convert from screen coordinates to world
    /// </summary>
    public Matrix2D InverseTransformMatrix;
    /// <summary>
    /// the 2D Cameras projection matrix
    /// </summary>
    public Matrix ProjectionMatrix;
    /// <summary>
    /// The view-projection matrix which is the transformMatrix * the projection matrix
    /// </summary>
    public Matrix ViewProjectionMatrix;
    /// <summary>
    /// A perspective projection for this camera for use when rendering 3D objects
    /// </summary>
    public Matrix ProjectionMatrix3D;
    public RectangleF Bounds;

    /// <summary>
    /// converts a point from screen coordinates to world
    /// </summary>
    /// <returns>The to world point.</returns>
    /// <param name="screenPosition">Screen position.</param>
    public Vector2 ScreenToWorldPoint(Vector2 screenPosition)
    {
        Vector2Ext.Transform(ref screenPosition, ref InverseTransformMatrix, out screenPosition);
        return screenPosition;
    }

    public ComputedCamera(Camera cam, Vector2 pos, float rot, Viewport viewport, CameraInset inset)
    {
        Update(cam, pos, rot, viewport, inset);
    }

    public void Update(Camera cam, Vector2 pos, float rot, Viewport viewport, CameraInset inset)
    {
        var origin = new Vector2(viewport.X / 2f, viewport.Y / 2f);
        Update(cam, pos, rot, viewport, origin, inset);
    }

    public void Update(Camera cam, Vector2 translation, float radians, Viewport viewport, Vector2 origin, CameraInset inset)
    {
        TransformMatrix = cam.TransformMatrix(translation, origin, radians);
        Matrix2D.Invert(ref TransformMatrix, out InverseTransformMatrix);
        ProjectionMatrix = cam.ProjectionMatrix(viewport.X, viewport.Y);
        ViewProjectionMatrix = TransformMatrix * ProjectionMatrix;
        ComputeBounds(translation, radians, viewport, inset);
    }

    private void ComputeBounds(Vector2 translation, float radians, Viewport viewport, CameraInset inset)
    {
        // top-left and bottom-right are needed by either rotated or non-rotated bounds
        var topLeft = ScreenToWorldPoint(new Vector2(viewport.X + inset.Left, viewport.Y + inset.Top));
        var bottomRight = ScreenToWorldPoint(new Vector2(viewport.X + viewport.Width - inset.Right, viewport.Y + viewport.Height - inset.Bottom));

        if (radians != 0)
        {
            // special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
            var topRight = ScreenToWorldPoint(new Vector2(viewport.X + viewport.Width - inset.Right, viewport.Y + inset.Top));
            var bottomLeft = ScreenToWorldPoint(new Vector2(viewport.X + inset.Left, viewport.Y + viewport.Height - inset.Bottom));

            var minX = Mathf.MinOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
            var maxX = Mathf.MaxOf(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
            var minY = Mathf.MinOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);
            var maxY = Mathf.MaxOf(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);

            Bounds.Location = new Vector2(minX, minY);
            Bounds.Width = maxX - minX;
            Bounds.Height = maxY - minY;
        }
        else
        {
            Bounds.Location = topLeft;
            Bounds.Width = bottomRight.X - topLeft.X;
            Bounds.Height = bottomRight.Y - topLeft.Y;
        }
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
