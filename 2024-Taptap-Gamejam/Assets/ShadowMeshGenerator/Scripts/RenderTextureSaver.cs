using UnityEngine;
using System.IO;

public class SaveRenderTextureEveryFrame : MonoBehaviour
{
    public RenderTexture renderTexture;
    public string filePath = "D:/Projects/Unity/ShaderTest/Assets/ShadowMeshGenerator/Textures/SSShadowEdge.exr";

    void LateUpdate()
    {
        SaveToEXR(renderTexture, filePath);
    }

    void SaveToEXR(RenderTexture rt, string path)
    {
        Texture2D texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);

        RenderTexture.active = rt;

        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;

        byte[] exrData = texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
        File.WriteAllBytes(path, exrData);

        Debug.Log("RenderTexture saved to: " + path);
    }
}
