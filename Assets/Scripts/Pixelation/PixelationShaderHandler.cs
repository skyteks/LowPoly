using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PixelationShaderHandler : MonoBehaviour
{
    public Material effectMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (enabled && effectMaterial != null)
        {
            Graphics.Blit(source, destination, effectMaterial);
        }
    }
}
