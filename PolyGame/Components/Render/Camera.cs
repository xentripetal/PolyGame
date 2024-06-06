using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyGame.Math;

namespace PolyGame.Components.Render;

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

    public Camera() { }
}

/// <summary>
/// Render viewport configuration for the <see cref="Camera"/> component.
///
/// The viewport defines the area on the render target to which the camera renders its image.
/// You can overlay multiple cameras in a single window using viewports to create effects like
/// split screen, minimaps, and character viewers./// </summary>
public struct ViewPort
{
    /// <summary>
    /// The physical position to render this viewport to within the <see cref="RenderTarget"/> of this <see cref="Camera"/>
    /// (0,0) corresponds to the top-left corner
    /// </summary>
    public UPoint PhysicalPosition;
    /// <summary>
    /// The physical size of the viewport rectangle to render to within the <see cref="RenderTarget"/> of this <see cref="Camera"/>
    /// The origin of the rectangle is in the top-left corner.
    /// </summary>
    public UPoint PhysicalSize;

    /// <summary>
    /// The minimum and maximum depth to render (on a scale from 0.0 to 1.0).
    /// TODO - Verify this is the correct range for Monogame
    /// </summary>
    public Range<float> Depth;
}
