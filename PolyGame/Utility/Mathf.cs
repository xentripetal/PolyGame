using System.Runtime.CompilerServices;

namespace PolyGame;

/// <summary>
/// Math utilities not in the standard <see cref="Math"/> and <see cref="MathF"/> classes. 
/// </summary>
/// <remarks>Based on Nez.Mathf</remarks>
public static class Mathf
{
    /// <summary>
    /// maps value (which is in the range leftMin - leftMax) to a value in the range rightMin - rightMax
    /// </summary>
    /// <param name="value">Value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Map(float value, float sourceMin, float sourceMax, float targetMin, float targetMax)
    {
        return targetMin + (value - sourceMin) * (targetMax - targetMin) / (sourceMax - sourceMin);
    }
    
    /// <summary>
    /// returns the minimum of the passed in values
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MinOf(float a, float b, float c, float d)
    {
        return Math.Min(a, Math.Min(b, Math.Min(c, d)));
    }
    
    /// <summary>
    /// returns the maximum of the passed in values
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MaxOf(float a, float b, float c, float d)
    {
        return Math.Max(a, Math.Max(b, Math.Max(c, d)));
    }
}
