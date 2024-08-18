using Microsoft.Xna.Framework.Graphics;
using PolyGame.Graphics.Effects;

namespace PolyGame.Graphics.Materials;

public class DeferredSpriteMaterial : Material<DeferredSpriteEffect>
{
    /// <summary>
    ///     DeferredSpriteEffects require a normal map. If you want to forego the normal map and have just diffuse light use
    ///     the
    ///     DeferredLightingRenderer.nullNormalMapTexture.
    /// </summary>
    /// <param name="normalMap">Normal map.</param>
    public DeferredSpriteMaterial(Texture2D normalMap)
    {
        BlendState = BlendState.Opaque;
        Effect = new DeferredSpriteEffect(normalMap.GraphicsDevice).SetNormalMap(normalMap);
    }
}
