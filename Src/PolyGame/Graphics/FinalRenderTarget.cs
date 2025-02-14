using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PolyGame.Graphics;

public class FinalRenderTarget : IDisposable
{
    public FinalRenderTarget(GameWindow window, Screen screen)
    {
        window.ClientSizeChanged += (sender, e) =>
        {
            UpdateResolutionScaler(screen);
        };
        SceneRenderTarget = new RenderTarget2D(screen.GraphicsDevice, screen.Width, screen.Height, false, screen.BackBufferFormat,
            screen.PreferredDepthStencilFormat,
            0, RenderTargetUsage.PreserveContents);
        // TODO Post processors being present would require we create the destinationRenderTarget
        // I'm probably going to get rid of this FinalRenderTarget thing and replace it with a proper render graph later on
        SetDesignResolution(screen, screen.Width, screen.Height, _defaultResolutionPolicy);
    }

    public enum ResolutionPolicy
    {
        /// <summary>
        /// Default. RenderTarget matches the sceen size
        /// </summary>
        None,

        /// <summary>
        /// The entire application is visible in the specified area without trying to preserve the original aspect ratio.
        /// Distortion can occur, and the application may appear stretched or compressed.
        /// </summary>
        ExactFit,

        /// <summary>
        /// The entire application fills the specified area, without distortion but possibly with some cropping,
        /// while maintaining the original aspect ratio of the application.
        /// </summary>
        NoBorder,

        /// <summary>
        /// Pixel perfect version of NoBorder. Scaling is limited to integer values.
        /// </summary>
        NoBorderPixelPerfect,

        /// <summary>
        /// The entire application is visible in the specified area without distortion while maintaining the original
        /// aspect ratio of the application. Borders can appear on two sides of the application.
        /// </summary>
        ShowAll,

        /// <summary>
        /// Pixel perfect version of ShowAll. Scaling is limited to integer values.
        /// </summary>
        ShowAllPixelPerfect,

        /// <summary>
        /// The application takes the height of the design resolution size and modifies the width of the internal
        /// canvas so that it fits the aspect ratio of the device.
        /// no distortion will occur however you must make sure your application works on different
        /// aspect ratios
        /// </summary>
        FixedHeight,

        /// <summary>
        /// Pixel perfect version of FixedHeight. Scaling is limited to integer values.
        /// </summary>
        FixedHeightPixelPerfect,

        /// <summary>
        /// The application takes the width of the design resolution size and modifies the height of the internal
        /// canvas so that it fits the aspect ratio of the device.
        /// no distortion will occur however you must make sure your application works on different
        /// aspect ratios
        /// </summary>
        FixedWidth,

        /// <summary>
        /// Pixel perfect version of FixedWidth. Scaling is limited to integer values.
        /// </summary>
        FixedWidthPixelPerfect,

        /// <summary>
        /// The application takes the width and height that best fits the design resolution with optional cropping inside of the "bleed area"
        /// and possible letter/pillar boxing. Works just like ShowAll except with horizontal/vertical bleed (padding). Gives you an area much
        /// like the old TitleSafeArea. Example: if design resolution is 1348x900 and bleed is 148x140 the safe area would be 1200x760 (design
        /// resolution - bleed).
        /// </summary>
        BestFit
    }

    /// <summary>
    /// if the ResolutionPolicy is pixel perfect this will be set to the scale calculated for it
    /// </summary>
    public int PixelPerfectScale = 1;


    /// <summary>
    /// default resolution policy used for all scenes
    /// </summary>
    static ResolutionPolicy _defaultResolutionPolicy = ResolutionPolicy.None;

    /// <summary>
    /// resolution policy used by the scene
    /// </summary>
    protected ResolutionPolicy _resolutionPolicy;

    /// <summary>
    /// design resolution size used by the scene
    /// </summary>
    Point _designResolutionSize;

    /// <summary>
    /// bleed size for <see cref="ResolutionPolicy.BestFit"/> resolution policy
    /// </summary>
    Point _designBleedSize;

    /// <summary>
    /// this gets setup based on the resolution policy and is used for the final blit of the RenderTarget
    /// </summary>
    public Rectangle FinalRenderDestinationRect;

    public RenderTarget2D SceneRenderTarget;
    public RenderTarget2D? DestinationRenderTarget;

    public void SetDesignResolution(
        IScreen screen,
        int width,
        int height,
        ResolutionPolicy resolutionPolicy,
        int horizontalBleed = 0,
        int verticalBleed = 0
    )
    {
        _designResolutionSize = new Point(width, height);
        _resolutionPolicy = resolutionPolicy;
        if (_resolutionPolicy == ResolutionPolicy.BestFit)
            _designBleedSize = new Point(horizontalBleed, verticalBleed);
        UpdateResolutionScaler(screen);
    }

    void UpdateResolutionScaler(IScreen screen)
    {
        var designSize = _designResolutionSize;
        var screenSize = new Point(screen.Width, screen.Height);
        var screenAspectRatio = (float)screenSize.X / (float)screenSize.Y;

        var renderTargetWidth = screenSize.X;
        var renderTargetHeight = screenSize.Y;

        var resolutionScaleX = (float)screenSize.X / (float)designSize.X;
        var resolutionScaleY = (float)screenSize.Y / (float)designSize.Y;

        var rectCalculated = false;

        // calculate the scale used by the PixelPerfect variants
        PixelPerfectScale = 1;
        if (_resolutionPolicy != ResolutionPolicy.None)
        {
            if ((float)designSize.X / (float)designSize.Y > screenAspectRatio)
                PixelPerfectScale = screenSize.X / designSize.X;
            else
                PixelPerfectScale = screenSize.Y / designSize.Y;

            if (PixelPerfectScale == 0)
                PixelPerfectScale = 1;
        }

        switch (_resolutionPolicy)
        {
            case ResolutionPolicy.None:
                FinalRenderDestinationRect.X = screen.Offset.X;
                FinalRenderDestinationRect.Y = screen.Offset.Y;
                FinalRenderDestinationRect.Width = screenSize.X;
                FinalRenderDestinationRect.Height = screenSize.Y;
                rectCalculated = true;
                break;

            case ResolutionPolicy.ExactFit:
                // exact design size render texture
                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;
                break;

            case ResolutionPolicy.NoBorder:
                // exact design size render texture
                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;

                resolutionScaleX = resolutionScaleY = Math.Max(resolutionScaleX, resolutionScaleY);
                break;

            case ResolutionPolicy.NoBorderPixelPerfect:
                // exact design size render texture
                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;

                // we are going to do some cropping so we need to use floats for the scale then round up
                PixelPerfectScale = 1;
                if ((float)designSize.X / (float)designSize.Y < screenAspectRatio)
                {
                    var floatScale = (float)screenSize.X / (float)designSize.X;
                    PixelPerfectScale = Mathf.CeilToInt(floatScale);
                }
                else
                {
                    var floatScale = (float)screenSize.Y / (float)designSize.Y;
                    PixelPerfectScale = Mathf.CeilToInt(floatScale);
                }

                if (PixelPerfectScale == 0)
                    PixelPerfectScale = 1;

                FinalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * PixelPerfectScale);
                FinalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * PixelPerfectScale);
                FinalRenderDestinationRect.X = (screenSize.X - FinalRenderDestinationRect.Width) / 2 + screen.Offset.X;
                FinalRenderDestinationRect.Y = (screenSize.Y - FinalRenderDestinationRect.Height) / 2 + screen.Offset.Y;
                rectCalculated = true;

                break;

            case ResolutionPolicy.ShowAll:
                resolutionScaleX = resolutionScaleY = Math.Min(resolutionScaleX, resolutionScaleY);

                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;
                break;

            case ResolutionPolicy.ShowAllPixelPerfect:
                // exact design size render texture
                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;

                FinalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * PixelPerfectScale);
                FinalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * PixelPerfectScale);
                FinalRenderDestinationRect.X = (screenSize.X - FinalRenderDestinationRect.Width) / 2 + screen.Offset.X;
                FinalRenderDestinationRect.Y = (screenSize.Y - FinalRenderDestinationRect.Height) / 2 + screen.Offset.Y;
                rectCalculated = true;

                break;

            case ResolutionPolicy.FixedHeight:
                resolutionScaleX = resolutionScaleY;
                designSize.X = Mathf.CeilToInt(screenSize.X / resolutionScaleX);

                // exact design size render texture for height but not width
                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;
                break;

            case ResolutionPolicy.FixedHeightPixelPerfect:
                // start with exact design size render texture height. the width may change
                renderTargetHeight = designSize.Y;

                FinalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * resolutionScaleX);
                FinalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * PixelPerfectScale);
                FinalRenderDestinationRect.X = (screenSize.X - FinalRenderDestinationRect.Width) / 2 + screen.Offset.X;
                FinalRenderDestinationRect.Y = (screenSize.Y - FinalRenderDestinationRect.Height) / 2 + screen.Offset.Y;
                rectCalculated = true;

                renderTargetWidth = (int)(designSize.X * resolutionScaleX / PixelPerfectScale);
                break;

            case ResolutionPolicy.FixedWidth:
                resolutionScaleY = resolutionScaleX;
                designSize.Y = Mathf.CeilToInt(screenSize.Y / resolutionScaleY);

                // exact design size render texture for width but not height
                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;
                break;

            case ResolutionPolicy.FixedWidthPixelPerfect:
                // start with exact design size render texture width. the height may change
                renderTargetWidth = designSize.X;

                FinalRenderDestinationRect.Width = Mathf.CeilToInt(designSize.X * PixelPerfectScale);
                FinalRenderDestinationRect.Height = Mathf.CeilToInt(designSize.Y * resolutionScaleY);
                FinalRenderDestinationRect.X = (screenSize.X - FinalRenderDestinationRect.Width) / 2 + screen.Offset.X;
                FinalRenderDestinationRect.Y = (screenSize.Y - FinalRenderDestinationRect.Height) / 2 + screen.Offset.Y;
                rectCalculated = true;

                renderTargetHeight = (int)(designSize.Y * resolutionScaleY / PixelPerfectScale);

                break;

            case ResolutionPolicy.BestFit:
                var safeScaleX = (float)screenSize.X / (designSize.X - _designBleedSize.X);
                var safeScaleY = (float)screenSize.Y / (designSize.Y - _designBleedSize.Y);

                var resolutionScale = MathHelper.Max(resolutionScaleX, resolutionScaleY);
                var safeScale = MathHelper.Min(safeScaleX, safeScaleY);

                resolutionScaleX = resolutionScaleY = MathHelper.Min(resolutionScale, safeScale);

                renderTargetWidth = designSize.X;
                renderTargetHeight = designSize.Y;

                break;
        }

        // if we didnt already calculate a rect (None and all pixel perfect variants calculate it themselves) calculate it now
        if (!rectCalculated)
        {
            // calculate the display rect of the RenderTarget
            var renderWidth = Math.Min(designSize.X * resolutionScaleX, screen.Width);
            var renderHeight = Math.Min(designSize.Y * resolutionScaleY, screen.Height);

            FinalRenderDestinationRect = new Rectangle((int)((screenSize.X - renderWidth) / 2 + screen.Offset.X),
                (int)((screenSize.Y - renderHeight) / 2 + screen.Offset.Y), (int)renderWidth, (int)renderHeight);
        }


        // set some values in the Input class to translate mouse position to our scaled resolution
        var scaleX = renderTargetWidth / (float)FinalRenderDestinationRect.Width;
        var scaleY = renderTargetHeight / (float)FinalRenderDestinationRect.Height;

        Scale = new Vector2(scaleX, scaleY);
        Offset = FinalRenderDestinationRect.Location.ToVector2();
        // TODO move this to a system
        //Input._resolutionScale = new Vector2(scaleX, scaleY);
        //Input._resolutionOffset = _finalRenderDestinationRect.Location;

        // resize our RenderTargets
        if (SceneRenderTarget != null)
            SceneRenderTarget.Dispose();
        SceneRenderTarget = new RenderTarget2D(screen.GraphicsDevice, renderTargetWidth, renderTargetHeight, false, screen.BackBufferFormat,
            screen.PreferredDepthStencilFormat,
            0, RenderTargetUsage.PreserveContents);

        // only create the destinationRenderTarget if it already exists, which would indicate we have PostProcessors
        if (DestinationRenderTarget != null)
        {
            DestinationRenderTarget.Dispose();
            DestinationRenderTarget = new RenderTarget2D(screen.GraphicsDevice, renderTargetWidth, renderTargetHeight, false, screen.BackBufferFormat,
                screen.PreferredDepthStencilFormat,
                0, RenderTargetUsage.PreserveContents);
        }

        // TODO publish event
        OnSceneBackBufferSizeChanged?.Invoke(screen, renderTargetWidth, renderTargetHeight);
        // notify the Renderers, PostProcessors and FinalRenderDelegate of the change in render texture size
        /**
        for (var i = 0; i < _renderers.Length; i++)
            _renderers.Buffer[i].OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

        for (var i = 0; i < _afterPostProcessorRenderers.Length; i++)
            _afterPostProcessorRenderers.Buffer[i]
                .OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

        for (var i = 0; i < _postProcessors.Length; i++)
            _postProcessors.Buffer[i].OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

        if (_finalRenderDelegate != null)
            _finalRenderDelegate.OnSceneBackBufferSizeChanged(renderTargetWidth, renderTargetHeight);

        Camera.OnSceneRenderTargetSizeChanged(renderTargetWidth, renderTargetHeight);
        **/
    }

    public Vector2 Scale { get; protected set; }
    public Vector2 Offset { get; protected set; }

    public delegate void BackBufferSizeChangedEventHandler(IScreen screen, int newWidth, int newHeight);
    public event BackBufferSizeChangedEventHandler OnSceneBackBufferSizeChanged;

    public void Dispose()
    {
        SceneRenderTarget?.Dispose();
        DestinationRenderTarget.Dispose();
    }
}
