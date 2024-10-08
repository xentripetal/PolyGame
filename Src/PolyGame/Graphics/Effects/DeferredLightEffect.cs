using PolyGame.Graphics.Camera;
using PolyGame.Graphics.Lights;
using PolyGame.Transform;

namespace PolyGame.Graphics.Effects;

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

public class DeferredLightEffect : Effect
{
    EffectPass clearGBufferPass;
    EffectPass pointLightPass;
    EffectPass spotLightPass;
    EffectPass areaLightPass;
    EffectPass directionalLightPass;
    EffectPass finalCombinePass;

    #region EffectParameter caches

    // gBuffer
    EffectParameter _clearColorParam;

    // matrices
    EffectParameter _objectToWorldParam;
    EffectParameter _worldToViewParam;
    EffectParameter _projectionParam;
    EffectParameter _screenToWorldParam;

    // common
    EffectParameter _normalMapParam;
    EffectParameter _lightPositionParam;
    EffectParameter _colorParam;
    EffectParameter _lightRadiusParam;
    EffectParameter _lightIntensityParam;

    // spot
    EffectParameter _lightDirectionParam;
    EffectParameter _coneAngleParam;

    // directional
    EffectParameter _specularIntensityParam;
    EffectParameter _specularPowerParam;
    EffectParameter _dirAreaLightDirectionParam; // shared with area light

    // final combine
    EffectParameter _ambientColorParam;
    EffectParameter _colorMapParam;
    EffectParameter _lightMapParam;

    #endregion


    public DeferredLightEffect(GraphicsDevice device) : base(device, EffectResource.DeferredLightBytes)
    {
        clearGBufferPass = Techniques["ClearGBuffer"].Passes[0];
        pointLightPass = Techniques["DeferredPointLight"].Passes[0];
        spotLightPass = Techniques["DeferredSpotLight"].Passes[0];
        areaLightPass = Techniques["DeferredAreaLight"].Passes[0];
        directionalLightPass = Techniques["DeferredDirectionalLight"].Passes[0];
        finalCombinePass = Techniques["FinalCombine"].Passes[0];

        CacheEffectParameters();
    }


    void CacheEffectParameters()
    {
        // gBuffer
        _clearColorParam = Parameters["_clearColor"];

        // matrices
        _objectToWorldParam = Parameters["_objectToWorld"];
        _worldToViewParam = Parameters["_worldToView"];
        _projectionParam = Parameters["_projection"];
        _screenToWorldParam = Parameters["_screenToWorld"];

        // common
        _normalMapParam = Parameters["_normalMap"];
        _lightPositionParam = Parameters["_lightPosition"];
        _colorParam = Parameters["_color"];
        _lightRadiusParam = Parameters["_lightRadius"];
        _lightIntensityParam = Parameters["_lightIntensity"];

        // spot
        _lightDirectionParam = Parameters["_lightDirection"];
        _coneAngleParam = Parameters["_coneAngle"];

        // directional
        _specularIntensityParam = Parameters["_specularIntensity"];
        _specularPowerParam = Parameters["_specularPower"];
        _dirAreaLightDirectionParam = Parameters["_dirAreaLightDirection"];

        // final combine
        _ambientColorParam = Parameters["_ambientColor"];
        _colorMapParam = Parameters["_colorMap"];
        _lightMapParam = Parameters["_lightMap"];
    }


    public void PrepareClearGBuffer()
    {
        CurrentTechnique = Techniques["ClearGBuffer"];
        clearGBufferPass.Apply();
    }


    /// <summary>
    /// updates the camera matrixes in the Effect
    /// </summary>
    /// <param name="camera">Camera.</param>
    public void UpdateForCamera(ComputedCamera camera)
    {
        SetWorldToViewMatrix(camera.TransformMatrix);
        SetProjectionMatrix(camera.ProjectionMatrix);
        SetScreenToWorld(Matrix.Invert(camera.ViewProjectionMatrix));
    }


    /// <summary>
    /// updates the shader values for the light and sets the appropriate CurrentTechnique
    /// </summary>
    /// <param name="light">Light.</param>
    public void UpdateForLight(Affine2 transform, PointLight light)
    {
        SetLightPosition(new Vector3(transform.Translation, light.Height));
        SetColor(light.Color);
        var scale = transform.Scale;
        SetLightRadius(light.Radius * scale.X);
        SetLightIntensity(light.Intensity);

        var objToWorld = Matrix.CreateScale(light.Radius * scale.X) *
                         Matrix.CreateTranslation(transform.Translation.X, transform.Translation.Y, 0);
        SetObjectToWorldMatrix(objToWorld);

        CurrentTechnique = Techniques["DeferredPointLight"];
        pointLightPass.Apply();
    }


    /// <summary>
    /// updates the shader values for the light and sets the appropriate CurrentTechnique
    /// </summary>
    /// <param name="light">Light.</param>
    public void UpdateForLight(Affine2 transform, SpotLight light)
    {
        SetLightPosition(new Vector3(transform.Translation, light.Height));
        SetColor(light.Color);
        var scale = transform.Scale;
        SetLightRadius(light.Radius * scale.X);
        SetLightIntensity(light.Intensity);

        var objToWorld = Matrix.CreateScale(light.Radius * scale.X) *
                         Matrix.CreateTranslation(transform.Translation.X, transform.Translation.Y, 0);
        SetObjectToWorldMatrix(objToWorld);

        CurrentTechnique = Techniques["DeferredPointLight"];
        pointLightPass.Apply();

        SetSpotLightDirection(transform.Matrix2.XAxis);
        SetSpotConeAngle(light.ConeAngle);

        CurrentTechnique = Techniques["DeferredSpotLight"];
        spotLightPass.Apply();
    }


    /// <summary>
    /// updates the shader values for the light and sets the appropriate CurrentTechnique
    /// </summary>
    /// <param name="light">Light.</param>
    public void UpdateForLight(Affine2 transform, AreaLight light)
    {
        SetColor(light.Color);
        SetAreaDirectionalLightDirection(light.Direction);
        SetLightIntensity(light.Intensity);

        var scale = transform.Scale;
        var objToWorld =
            Matrix.CreateScale(light.Width * scale.X, light.Height * scale.Y, 1f) *
            Matrix.CreateTranslation(transform.Translation.X, transform.Translation.Y, 0);
        SetObjectToWorldMatrix(objToWorld);

        areaLightPass.Apply();
    }


    /// <summary>
    /// updates the shader values for the light and sets the appropriate CurrentTechnique
    /// </summary>
    /// <param name="light">Light.</param>
    public void UpdateForLight(DirLight light)
    {
        SetColor(light.Color);
        SetAreaDirectionalLightDirection(light.Direction);
        SetSpecularPower(light.SpecularPower);
        SetSpecularIntensity(light.SpecularIntensity);

        CurrentTechnique = Techniques["DeferredDirectionalLight"];
        directionalLightPass.Apply();
    }


    #region Matrix properties

    public void SetClearColor(Color color)
    {
        _clearColorParam.SetValue(color.ToVector3());
    }


    public void SetObjectToWorldMatrix(Matrix objToWorld)
    {
        _objectToWorldParam.SetValue(objToWorld);
    }


    public void SetWorldToViewMatrix(Matrix worldToView)
    {
        _worldToViewParam.SetValue(worldToView);
    }


    public void SetProjectionMatrix(Matrix projection)
    {
        _projectionParam.SetValue(projection);
    }


    /// <summary>
    /// inverse of Camera.getViewProjectionMatrix
    /// </summary>
    /// <param name="screenToWorld">screenToWorld.</param>
    public void SetScreenToWorld(Matrix screenToWorld)
    {
        _screenToWorldParam.SetValue(screenToWorld);
    }

    #endregion


    #region Point/Spot common properties

    public void SetNormalMap(Texture2D normalMap)
    {
        _normalMapParam.SetValue(normalMap);
    }


    public void SetLightPosition(Vector3 lightPosition)
    {
        _lightPositionParam.SetValue(lightPosition);
    }


    public void SetColor(Color color)
    {
        _colorParam.SetValue(color.ToVector3());
    }


    public void SetLightRadius(float radius)
    {
        _lightRadiusParam.SetValue(radius);
    }


    public void SetLightIntensity(float intensity)
    {
        _lightIntensityParam.SetValue(intensity);
    }

    #endregion


    #region Spot properties

    /// <summary>
    /// directly sets the light direction
    /// </summary>
    /// <param name="lightDirection">Light direction.</param>
    public void SetSpotLightDirection(Vector2 lightDirection)
    {
        _lightDirectionParam.SetValue(lightDirection);
    }


    /// <summary>
    /// sets the light direction using just an angle in degrees. 0 degrees points to theright, 90 degrees would be straight down, etc
    /// </summary>
    /// <param name="degrees">Degrees.</param>
    public void SetSpotLightDirection(float degrees)
    {
        var radians = MathHelper.ToRadians(degrees);
        var dir = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        SetSpotLightDirection(dir);
    }


    public void SetSpotConeAngle(float coneAngle)
    {
        _coneAngleParam.SetValue(coneAngle);
    }

    #endregion


    #region Directional light properties

    public void SetSpecularIntensity(float specIntensity)
    {
        _specularIntensityParam.SetValue(specIntensity);
    }


    public void SetSpecularPower(float specPower)
    {
        _specularPowerParam.SetValue(specPower);
    }


    public void SetAreaDirectionalLightDirection(Vector3 lightDir)
    {
        _dirAreaLightDirectionParam.SetValue(lightDir);
    }

    #endregion


    #region Final combine properties

    public void SetAmbientColor(Color color)
    {
        _ambientColorParam.SetValue(color.ToVector3());
    }


    /// <summary>
    /// sets the two textures required for the final combine and applies the pass
    /// </summary>
    /// <param name="diffuse">Diffuse.</param>
    /// <param name="lightMap">Light map.</param>
    public void PrepareForFinalCombine(Texture2D diffuse, Texture2D lightMap, Texture2D normalMap)
    {
        _colorMapParam.SetValue(diffuse);
        _lightMapParam.SetValue(lightMap);
        _normalMapParam.SetValue(normalMap);

        CurrentTechnique = Techniques["FinalCombine"];
        finalCombinePass.Apply();
    }

    #endregion
}
