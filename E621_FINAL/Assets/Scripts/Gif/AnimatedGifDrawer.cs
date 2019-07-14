using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;
using System.Threading;

public class AnimatedGifDrawer : MonoBehaviour
{
    public UnityEngine.UI.RawImage rawImage;
    public string loadingGifPath;
    public float speed = 1;
    public int pixelIncrement = 1;
    public bool loadOnce;
    //public Vector2 drawPosition;

    List<Texture2D> gifFrames = new List<Texture2D>();

    Coroutine gifPlay, gifLoad;

    private void Awake()
    {
        UnityThread.initUnityThread();
    }

    private void Start()
    {
        if (loadingGifPath != "") DrawGif();
    }
    public void DrawGif()
    {
        #region Original
        /*
        var gifImage = Image.FromFile(loadingGifPath);
        var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);
        for (int i = 0; i < frameCount; i++)
        {
            gifImage.SelectActiveFrame(dimension, i);
            var frame = new Bitmap(gifImage.Width, gifImage.Height);
            System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
            var frameTexture = new Texture2D(frame.Width, frame.Height);
            for (int x = 0; x < frame.Width; x++)
                for (int y = 0; y < frame.Height; y++)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                    frameTexture.SetPixel(frame.Width - 1 - x, y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped
                }
            

            frameTexture.Apply();
            gifFrames.Add(frameTexture);
        }
        gifPlay = StartCoroutine(PlayGif());
        */
        #endregion

        if (gifPlay != null)
        {
            StopCoroutine(gifPlay);
            gifPlay = null;
        }
        if(gifLoad != null)
        {
            StopCoroutine(gifLoad);
            gifLoad = null;
        }
        ResetGifPlayback();
        if (!loadOnce) gifLoad = StartCoroutine(LoadGif()); else GetFirstFrame();
        
    }

    void GetFirstFrame()
    {
        Image gifImage = Image.FromFile(loadingGifPath);
        FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        gifImage.SelectActiveFrame(dimension, 0);
        Bitmap frame = new Bitmap(gifImage.Width, gifImage.Height);
        System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
        Texture2D frameTexture = new Texture2D(frame.Width, frame.Height);
        //for (int x = 0; x < frame.Width; x++)
        for (int x = 0; x < frame.Width; x += pixelIncrement)
        {
            //for (int y = 0; y < frame.Height; y++)
            for (int y = 0; y < frame.Height; y += pixelIncrement)
            {
                System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                //frameTexture.SetPixel(frame.Width - 1 - x, y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped
                UnityEngine.Color newColor = new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A);
                UnityEngine.Color[] newColorA = new UnityEngine.Color[pixelIncrement * pixelIncrement];
                for (int z = 0; z < newColorA.Length; z++)
                {
                    newColorA[z] = newColor;
                }
                frameTexture.SetPixels(frame.Width - 1 - x, y, pixelIncrement, pixelIncrement, newColorA);
            }
        }
        frameTexture.Apply();
        gifFrames.Add(frameTexture);
        gifPlay = StartCoroutine(PlayGif());
    }

    IEnumerator LoadGif()
    {
        Image gifImage = Image.FromFile(loadingGifPath);
        FrameDimension dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);
        for (int i = 0; i < frameCount; i++)
        {
            yield return null;
            gifImage.SelectActiveFrame(dimension, i);

            Bitmap frame = new Bitmap(gifImage.Width, gifImage.Height);

            System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
            Texture2D frameTexture = new Texture2D(frame.Width, frame.Height);
            //for (int x = 0; x < frame.Width; x++)
            for (int x = 0; x < frame.Width; x += pixelIncrement)
            {
                //for (int y = 0; y < frame.Height; y++)
                for (int y = 0; y < frame.Height; y += pixelIncrement)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                    //frameTexture.SetPixel(frame.Width - 1 - x, y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped
                    UnityEngine.Color newColor = new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A);
                    UnityEngine.Color[] newColorA = new UnityEngine.Color[pixelIncrement * pixelIncrement];
                    for (int z = 0; z < newColorA.Length; z++)
                    {
                        newColorA[z] = newColor;
                    }
                    frameTexture.SetPixels(frame.Width - 1 - x, y, pixelIncrement, pixelIncrement, newColorA);
                }
            }

            frameTexture.Apply();
            gifFrames.Add(frameTexture);
        }
        gifPlay = StartCoroutine(PlayGif());
        yield return null;
    }
    

    public void StopGif()
    {
        if (gifPlay != null)
        {
            StopCoroutine(gifPlay);
            gifPlay = null;
        }
    }

    void ResetGifPlayback()
    {
        gifFrames.Clear();
    }

    IEnumerator PlayGif()
    {
        while (true)
        {
            Debug.Log("Play");
            rawImage.texture = gifFrames[(int)(Time.frameCount * speed) % gifFrames.Count];
            yield return null;
        }
    }
}