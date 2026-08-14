using UnityEngine;

public class LoadingScreenInfo
{
    public string NextScene { get; private set; }
    public string TextureName { get; private set; }

    public void SetNextScene(string scene, string screenTexture)
    {
        Debug.Log("[DD] [SCENE] LoadingInfo.SetNextScene: " + scene + " / " + screenTexture);
        NextScene = scene;
        TextureName = screenTexture;
    }
}