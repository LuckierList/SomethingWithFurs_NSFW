using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

public class GlobalActions : MonoBehaviour
{
    //GameObject allObjectsPrefab;

    //Advice CTRL!!!
    AdviceComponents adviceComp;
    public delegate void ButtonConfirm();
    ButtonConfirm functionConfirm;
    public delegate void ButtonDeny();
    ButtonDeny functionDeny;

    //Loading CTRL!!!!
    [HideInInspector]
    public LoadingComponents loadingComp;
    [HideInInspector]
    public bool loadingWait;

    //Load Image CTRL!!!
    Coroutine loadImageCO;
    Texture2D newTexture;
    Sprite newSprite;

    // Use this for initialization
    public virtual void Awake()
    {
        loadingWait = false;
        adviceComp = GameObject.FindGameObjectWithTag("Advice").GetComponent<AdviceComponents>();
        loadingComp = GameObject.FindGameObjectWithTag("Loading").GetComponent<LoadingComponents>();
        UnityThread.initUnityThread();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //-----------------------------------------------------------
    //Advice Controls

    public void ButtonDoConfirm()
    {
        foreach (AdviceBox box in adviceComp.adviceBoxList)
        {
            box.obj.SetActive(false);
        }
        adviceComp.panel.gameObject.SetActive(false);

        functionConfirm();
    }

    public void ButtonDoDeny()
    {
        foreach (AdviceBox box in adviceComp.adviceBoxList)
        {
            box.obj.SetActive(false);
        }
        adviceComp.panel.gameObject.SetActive(false);

        functionDeny();
    }

    public void CreateAdvice(string text, int size = 0, ButtonConfirm actionConfirm = null, ButtonDeny actionDeny = null)
    {
        CreateAdvice("Advice", text, size, actionConfirm, actionDeny);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title">Advice Title</param>
    /// <param name="text">The message of the advice.</param>
    /// <param name="size">(1-3) Defaults to 0.</param>
    /// <param name="actionConfrim">Action when clicking Ok/Yes.</param>
    /// <param name="actionDeny">Action when clicking No. Will force a double button to appear instead of only the button Ok.</param>
    /// 
    public void CreateAdvice(string title, string text, int size = 0, ButtonConfirm actionConfirm = null, ButtonDeny actionDeny = null)
    {
        //Determinar el tamaño de la caja de mensaje
        adviceComp.panel.gameObject.SetActive(true);
        AdviceBox usedBox = adviceComp.adviceBoxList[size];
        usedBox.txtTitle.text = title;
        usedBox.txtAdvice.text = text;
        functionConfirm = actionConfirm;
        functionDeny = actionDeny;

        if (actionConfirm == null)
        {
            usedBox.buttonYes.gameObject.SetActive(false);
            usedBox.buttonNo.gameObject.SetActive(false);
            usedBox.buttonOk.gameObject.SetActive(true);
        }
        else
        {
            usedBox.buttonYes.gameObject.SetActive(true);
            usedBox.buttonNo.gameObject.SetActive(true);
            usedBox.buttonOk.gameObject.SetActive(false);
        }

        usedBox.buttonOk.onClick.RemoveAllListeners();
        usedBox.buttonYes.onClick.RemoveAllListeners();
        usedBox.buttonNo.onClick.RemoveAllListeners();

        if (actionConfirm == null)
            functionConfirm = () => { };
        if (actionDeny == null)
            functionDeny = () => { };

        usedBox.buttonOk.onClick.AddListener(ButtonDoConfirm);
        usedBox.buttonYes.onClick.AddListener(ButtonDoConfirm);
        usedBox.buttonNo.onClick.AddListener(ButtonDoDeny);

        usedBox.obj.SetActive(true);
    }
    //-----------------------------------------------------------

    //-----------------------------------------------------------
    //Loading Controls
    public void LoadingReset(string newTitle)
    {
        loadingComp.slider.value = 0f;
        loadingComp.message.text = newTitle;
        loadingComp.obj.SetActive(true);
    }

    public void UpdateLoadingValue(float value)
    {
        loadingComp.slider.value = value;
        //if (value == 1f && !loadingWait) loadingComp.obj.SetActive(false);
    }

    public void StartLoadingWait()
    {
        loadingWait = true;
        StartCoroutine(LoadingWait());
    }

    IEnumerator LoadingWait()
    {
        while (loadingWait)
        {
            loadingComp.slider.value += Time.deltaTime;
            if (loadingComp.slider.value >= 1f) loadingComp.slider.value = 0f;
            yield return null;
        }
        yield return null;
    }
    //-----------------------------------------------------------

    //-----------------------------------------------------------
    //Load images

    public void LoadImage(Sprite imgLoading, Sprite imgError, Image image, string imageURL, bool clear = false)
    {
        LoadImageCancel();
        loadImageCO = StartCoroutine(ShowImageCorroutine(imgLoading, imgError, image, imageURL, clear));
    }
    public void LoadImageCancel()
    {
        if(loadImageCO != null)
        {
            StopCoroutine(loadImageCO);
            loadImageCO = null;
        }
    }

    /// <summary>
    /// Load the image from a URL in the disk.
    /// </summary>
    /// <param name="imgLoading"></param>
    /// <param name="imgError"></param>
    /// <param name="image"></param>
    /// <param name="imageURL"></param>
    /// <param name="clear">When the load is complete, delete the unused Assets (Crashes if you are loading another image and do the cleanse)</param>
    /// <returns></returns>
    IEnumerator ShowImageCorroutine(Sprite imgLoading, Sprite imgError, Image image, string imageURL, bool clear)
    {
        
        yield return null;
        image.sprite = imgLoading;
        if (!File.Exists(imageURL))
        {
            GetComponent<Button>().interactable = false;
            image.sprite = imgError;
        }
        else
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + imageURL))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    newTexture = DownloadHandlerTexture.GetContent(uwr);
                    newSprite = Sprite.Create(newTexture, new Rect(0f, 0f, newTexture.width, newTexture.height), new Vector2(.5f, .5f), 100f);
                    image.sprite = newSprite;
                    if(clear) Resources.UnloadUnusedAssets();
                }
            }
        }
        loadImageCO = null;
    }

    //-----------------------------------------------------------

    //-----------------------------------------------------------
    //Misc.
    public void OpenInPage(string url)
    {
        string filenameNoExtension = Path.GetFileNameWithoutExtension(url);

        //Prevenir signo "-"; antes de este signo, viene la ID de la imagen, se lo quito para el código MD5, la ID se obtendrá
        //desde el HTML descargado.
        if (filenameNoExtension.Contains("-"))
        {
            filenameNoExtension = filenameNoExtension.Substring((filenameNoExtension.IndexOf("-") + 1), filenameNoExtension.Length - (filenameNoExtension.IndexOf("-") + 1));
        }
        Application.OpenURL("https://e621.net/post/index/1/md5:" + filenameNoExtension);
    }

    //-----------------------------------------------------------
}
