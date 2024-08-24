using PolyGame.Assets;
using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Effects;
using PolyGame.Graphics.Lights;
using PolyGame.Graphics.Materials;
using PolyGame.Graphics.Renderable;
using PolyGame.Transform;
using Serilog;

namespace PolyGame.Graphics.Renderers;

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

/// <summary>
/// handles deferred lighting. This Renderer should be ordered after any of your Renderers that render to a RenderTexture. Any renderLayers
/// rendered by this Renderer should have Renderables with DeferredSpriteMaterials (or null Material to use the default, diffuse only Material).
/// </summary>
public class DeferredLightingRenderer : Renderer
{
    /// <summary>
    /// we do not want to render into the Scene render texture
    /// </summary>
    /// <value>true</value>
    /// <c>false</c>
    public override bool WantsToRenderToGraphRenderTarget => false;

    /// <summary>
    /// the renderLayers this Renderer will render
    /// </summary>
    public HashSet<int> RenderLayers;

    /// <summary>
    /// ambient lighting color. Alpha is ignored
    /// </summary>
    /// <value>The color of the ambient.</value>
    public Color AmbientColor
    {
        get => _ambientColor;
        set => SetAmbientColor(value);
    }

    /// <summary>
    /// clear color for the diffuse portion of the gbuffer
    /// </summary>
    /// <value>The color of the clear.</value>
    public Color ClearColor
    {
        get => _clearColor;
        set => SetClearColor(value);
    }

    /// <summary>
    /// single pixel texture of a neutral normal map. This will effectively make the object have only diffuse lighting if applied as the normal map.
    /// </summary>
    /// <value>The null normal map texture.</value>
    public Texture2D NullNormalMapTexture
    {
        get
        {
            if (_nullNormalMapTexture == null)
                _nullNormalMapTexture = GraphicsHelper.CreateSingleColorTexture(GraphicsDevice, 1, 1, new Color(0.5f, 0.5f, 1f, 0f));
            return _nullNormalMapTexture;
        }
    }

    public GraphicsDevice GraphicsDevice { get; protected set; }

    /// <summary>
    /// if true, all stages of the deferred pipeline are rendered after the final combine
    /// </summary>
    public bool EnableDebugBufferRender;


    int _lightLayer;
    Color _ambientColor;
    Color _clearColor;
    Texture2D _nullNormalMapTexture;

    public RenderTexture DiffuseRT;
    public RenderTexture NormalRT;
    public RenderTexture LightRT;
    public RenderTexture CombinedRT;

    DeferredLightEffect _lightEffect;

    // light volumes. quad for directional/area and polygon for others
    QuadMesh _quadMesh;
    PolygonMesh _polygonMesh;
    PolygonMesh _quadPolygonMesh;


    public DeferredLightingRenderer(Screen screen, int renderOrder, int lightLayer, params int[] renderLayers) : base(renderOrder)
    {
        // make sure we have a workable Material for our lighting system
        GraphicsDevice = screen.GraphicsDevice;
        Material = new DeferredSpriteMaterial(NullNormalMapTexture);

        _lightLayer = lightLayer;
        Array.Sort(renderLayers);
        Array.Reverse(renderLayers);
        RenderLayers = new HashSet<int>(renderLayers);

        _lightEffect = new DeferredLightEffect(GraphicsDevice);

        // meshes used for light volumes
        _quadMesh = new QuadMesh(GraphicsDevice);
        _polygonMesh = PolygonMesh.CreateSymmetricalPolygon(GraphicsDevice, 10);
        _quadPolygonMesh = PolygonMesh.CreateRectangle(GraphicsDevice);

        // set some sensible defaults
        SetAmbientColor(new Color(0.2f, 0.2f, 0.2f))
            .SetClearColor(Color.CornflowerBlue);

        DiffuseRT = new RenderTexture(screen, screen.Width, screen.Height, SurfaceFormat.Color, DepthFormat.None);
        NormalRT = new RenderTexture(screen, screen.Width, screen.Height, SurfaceFormat.Color, DepthFormat.None);
        LightRT = new RenderTexture(screen, screen.Width, screen.Height, SurfaceFormat.Color, DepthFormat.None);
        CombinedRT = new RenderTexture(screen, screen.Width, screen.Height, SurfaceFormat.Color, DepthFormat.None);
    }

    public override void Render(
        AssetServer assets,
        DrawFuncRegistry registry,
        ref ComputedCamera cam,
        GraphicsDevice device,
        Batcher batch,
        RenderableList renderables,
        RenderTarget2D cameraTarget
    )
    {
        ClearRenderTargets();
        RenderSprites(assets, registry, ref cam, device, batch, renderables);
        RenderLights(assets, registry, ref cam, device, batch, renderables);
        RenderFinalCombine(cameraTarget);

        if (EnableDebugBufferRender)
            RenderAllBuffers(cameraTarget, batch);
    }



    #region Configuration

    /// <summary>
    /// ambient lighting color. Alpha is ignored
    /// </summary>
    /// <returns>The ambient color.</returns>
    /// <param name="color">Color.</param>
    public DeferredLightingRenderer SetAmbientColor(Color color)
    {
        if (_ambientColor != color)
        {
            _ambientColor = color;
            _lightEffect.SetAmbientColor(color);
        }

        return this;
    }

    /// <summary>
    /// clear color for the diffuse portion of the gbuffer
    /// </summary>
    /// <returns>The clear color.</returns>
    /// <param name="color">Color.</param>
    public DeferredLightingRenderer SetClearColor(Color color)
    {
        if (_clearColor != color)
        {
            _clearColor = color;
            _lightEffect.SetClearColor(color);
        }

        return this;
    }

    #endregion


    #region Rendering

    void ClearRenderTargets()
    {
        GraphicsDevice.SetRenderTargets(DiffuseRT.RenderTarget, NormalRT.RenderTarget);
        _lightEffect.PrepareClearGBuffer();
        _quadMesh.Render(GraphicsDevice);
    }

    void RenderSprites(
        AssetServer assets,
        DrawFuncRegistry registry,
        ref ComputedCamera cam,
        GraphicsDevice device,
        Batcher batch,
        RenderableList renderables
    )
    {
        BeginRender(ref cam, device, batch);

        foreach (var layer in renderables.GetLayers())
        {
            if (!RenderLayers.Contains(layer))
                continue;
            foreach (var renderable in renderables.GetRenderables(layer))
            {
                RenderAfterStateCheck(registry, cam, batch, assets, renderable);
            }
        }
        EndRender(batch);
    }

    void RenderLights(
        AssetServer assets,
        DrawFuncRegistry registry,
        ref ComputedCamera cam,
        GraphicsDevice device,
        Batcher batch,
        RenderableList renderables
    )
    {
        // bind the normalMap and update the Effect with our camera
        _lightEffect.SetNormalMap(NormalRT);
        _lightEffect.UpdateForCamera(cam);

        GraphicsDevice.SetRenderTarget(LightRT);
        GraphicsDevice.Clear(Color.Transparent);
        GraphicsDevice.BlendState = BlendState.Additive;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;

        var layer = renderables.GetRenderables(_lightLayer);

        foreach (var renderable in layer)
        {
            // TODO this is pretty bad. This should be handled by the draw funcs.
            if (renderable.Entity.Has<SpotLight>())
            {
                RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<SpotLight>());
            }
            else if (renderable.Entity.Has<DirLight>())
            {
                RenderLight(renderable.Entity.Get<DirLight>());
            }
            else if (renderable.Entity.Has<PointLight>())
            {
                RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<PointLight>());
            }
            else if (renderable.Entity.Has<AreaLight>())
            {
                RenderLight(renderable.Entity.Get<GlobalTransform2D>().Value, renderable.Entity.Get<AreaLight>());
            }
            else
            {
                Log.Warning("DeferredLightingRenderer found a Renderable with no Light component");
            }
        }
    }

    void RenderFinalCombine(RenderTarget2D cameraTarget)
    {
        GraphicsDevice.SetRenderTarget(cameraTarget);
        GraphicsDevice.BlendState = BlendState.Opaque;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;

        // combine everything. ambient color is set in the shader when the property is set so no need to reset it
        _lightEffect.PrepareForFinalCombine(DiffuseRT, LightRT, NormalRT);
        _quadMesh.Render(GraphicsDevice);
    }

    void RenderAllBuffers(RenderTarget2D cameraTarget, Batcher batcher)
    {
        GraphicsDevice.SetRenderTarget(CombinedRT);

        var halfWidth = CombinedRT.RenderTarget.Width / 2;
        var halfHeight = CombinedRT.RenderTarget.Height / 2;

        batcher.Begin(BlendState.Opaque);
        batcher.Draw(LightRT, new Rectangle(0, 0, halfWidth, halfHeight));
        batcher.Draw(DiffuseRT, new Rectangle(halfWidth, 0, halfWidth, halfHeight));
        batcher.Draw(NormalRT, new Rectangle(0, halfHeight, halfWidth, halfHeight));
        batcher.Draw(cameraTarget, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight));
        batcher.End();

        GraphicsDevice.SetRenderTarget(cameraTarget);
        batcher.Begin(BlendState.Opaque);
        batcher.Draw(CombinedRT, Vector2.Zero);
        batcher.End();

    }

    #endregion


    #region Light rendering

    void RenderLight(DirLight light)
    {
        _lightEffect.UpdateForLight(light);
        _quadMesh.Render(GraphicsDevice);
    }

    void RenderLight(Affine2 transform, PointLight light)
    {
        _lightEffect.UpdateForLight(transform, light);
        _polygonMesh.Render(GraphicsDevice);
    }

    void RenderLight(Affine2 transform, SpotLight light)
    {
        _lightEffect.UpdateForLight(transform, light);
        _polygonMesh.Render(GraphicsDevice);
    }

    void RenderLight(Affine2 transform, AreaLight light)
    {
        _lightEffect.UpdateForLight(transform, light);
        _quadPolygonMesh.Render(GraphicsDevice);
    }

    #endregion


    public override void OnSceneBackBufferSizeChanged(Screen screen, int newWidth, int newHeight)
    {
        // create our RenderTextures if we havent and resize them if we have
        if (DiffuseRT == null)
        {
            DiffuseRT = new RenderTexture(screen, newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None);
            NormalRT = new RenderTexture(screen, newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None);
            LightRT = new RenderTexture(screen, newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None);
        }
        else
        {
            DiffuseRT.OnSceneBackBufferSizeChanged(screen, newWidth, newHeight);
            NormalRT.OnSceneBackBufferSizeChanged(screen, newWidth, newHeight);
            LightRT.OnSceneBackBufferSizeChanged(screen, newWidth, newHeight);
        }
    }
    
    public override void Unload()
    {
        _lightEffect.Dispose();

        DiffuseRT.Dispose();
        NormalRT.Dispose();
        LightRT.Dispose();

        if (_nullNormalMapTexture != null)
            _nullNormalMapTexture.Dispose();

        base.Unload();
    }
}
