using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PolyECS.Queries;

namespace PolyGame.Graphics.Lights;

/// <summary>
/// PointLights radiate light in a circle. Note that PointLights are affected by Transform.scale. The Transform.scale.X value is multiplied
/// by the lights radius when sent to the GPU. It is expected that scale will be linear.
/// </summary>
public struct PointLight
{
    public float Radius;
    public float Intensity;
    public Color Color;
    /// <summary>
    /// Virtual height of the light. 
    /// </summary>
    public float Height;

    public PointLight(float radius = 400f, float intensity = 3f, Color? color = null, float Height = 140f)
    {
        Radius = radius;
        Intensity = intensity;
        Color = color ?? Color.White;
        this.Height = Height;
    }
}

public struct SpotLight
{
    public float Radius = 400f;
    public float Intensity = 3f;
    public Color Color = Color.White;
    public float ConeAngle = 90f;

    /// <summary>
    /// Virtual height of the light. 
    /// </summary>
    public float Height = 140f;
    
    public SpotLight(float radius = 1f, float intensity = 3f, Color? color = null, float coneAngle = 90f, float height = 140f)
    {
        Radius = radius;
        Intensity = intensity;
        Color = color ?? Color.White;
        ConeAngle = coneAngle;
        Height = height;
    }
    
    public SpotLight()
    {}
}

public struct AreaLight
{
    public float Width = 200;
    public float Height = 200;
    public Vector3 Direction = new Vector3(500, 500, 50);
    public float Intensity = 12f;
    public Color Color = Color.White;

    public AreaLight(float width = 200, float height = 200, Vector3? direction = null, float intensity = 12f, Color? color = null)
    {
        Width = width;
        Height = height;
        Direction = direction ?? new Vector3(500, 500, 50);
        Intensity = intensity;
        Color = color ?? Color.White;
    }

    public AreaLight()
    {}
}

public struct DirLight
{
    public Color Color = Color.White;
    public Vector3 Direction = new Vector3(50, 20, 100);
    public float SpecularIntensity = 0.5f;
    public float SpecularPower = 2;

    public DirLight() { }

    public DirLight(Color color)
    {
        Color = color;
    }
}
