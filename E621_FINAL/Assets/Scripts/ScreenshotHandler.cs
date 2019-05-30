using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{

    public static ScreenshotHandler act;
    public RenderTexture sendTexture;
    Camera screenshotCam;
    bool takeScreenshotOnNextFrame;

    string filenname = "", path = "";

    private void Awake()
    {
        act = this;
    }

    private void Start()
    {
        screenshotCam = GetComponent<Camera>();
    }
    public void TakeScreenshot(string _name, string _path)
    {
        filenname = _name;
        path = _path;
        screenshotCam.targetTexture = RenderTexture.GetTemporary(720, 720);
        takeScreenshotOnNextFrame = true;
    }

    private void OnPostRender()
    {
        if (takeScreenshotOnNextFrame)
        {
            takeScreenshotOnNextFrame = false;
            RenderTexture renderTexture = screenshotCam.targetTexture;

            Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            renderResult.ReadPixels(rect, 0, 0);

            byte[] byteArray = renderResult.EncodeToPNG();
            System.IO.File.WriteAllBytes(path + "/" + filenname + ".png", byteArray);
            Debug.Log("Saved screenshot");

            RenderTexture.ReleaseTemporary(renderTexture);
            screenshotCam.targetTexture = null;
            screenshotCam.targetTexture = sendTexture;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
