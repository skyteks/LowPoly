using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelationShaderHandler : MonoBehaviour
{
    public Material effectMaterial;

    private Vector2Int lastResolution;

    void OnPreRender()
    {
        if (lastResolution.x != Screen.width || lastResolution.y != Screen.height)
        {
            lastResolution = new Vector2Int(Screen.width, Screen.height);
            effectMaterial.SetFloat("_pixelsX", Screen.width);
            effectMaterial.SetFloat("_pixelsY", Screen.height);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Camera.main.depthTextureMode = DepthTextureMode.DepthNormals;
        if (enabled && effectMaterial != null)
        {
            Graphics.Blit(source, destination, effectMaterial);
        }
    }
}
