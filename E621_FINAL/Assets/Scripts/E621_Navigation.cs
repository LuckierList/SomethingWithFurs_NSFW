using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System.IO;
using System.Threading;

public class DownloadImage
{
    public ImageData data;
    public Coroutine thisCoroutine;
    public GameObject objProgress;
    public DownloadImage(string urlDownlaod, E621_NavigationButton origin, Sprite sprPrev, ImageData _data, GameObject _objProgress)
    {
        data = _data;
        objProgress = _objProgress;
        E621_Navigation.act.StartCoroutine(E621_Navigation.act.DownloadImageCo(urlDownlaod,sprPrev, objProgress, data, origin));
    }


}

public class E621_Navigation : GlobalActions
{

    #region PageLoader
    [Header("Page Loader")]
    public GameObject prefabNavButton;
    public Transform transformNavigation;
    public GameObject objLoadPageHourglass;
    Coroutine pageLoadCo;
    public Coroutine previewLoadCo;

    public InputField inputSearchField, inputGoToPage;
    public Button buttonSearch, buttonNext, buttonPrev;
    public Text textCurrentPage;
    string currentTags;
    int lastPage = 0;
    int currentPage = 0;

    Thread threadPageLoad;
    string html = "";

    Coroutine checkExistanceCo;
    [HideInInspector]
    public Queue<E621_NavigationButton> queueExistance = new Queue<E621_NavigationButton>();

    #endregion

    #region PreviewDownloader
    [Header("Preview Downloader")]
    public GameObject objPreviewViewer;
    public Image imagePreview;
    //List<string> previewURLs = new List<string>();
    #endregion

    #region DownloadHandler
    [Header("Download Handler")]
    public GameObject prefabDownloading;
    public RectTransform downloaderContent;
    public List<DownloadImage> listCoroutines = new List<DownloadImage>();

    #endregion

    #region Blacklist
    [Header("Blacklist")]
    public InputField inputNewBlacklistTag;
    public Transform transformBlackList;
    public GameObject prefabBlacklistButton;
    #endregion

    #region General
    [Header("General")]
    public Sprite imgBlank;
    public Sprite imgLoading, imgError, imgHourGlass, imgViewLocked;
    public Toggle toggleBlacklistHide, toggleBlacklistSearch;
    public Button buttonReturn;
    #endregion

    #region Configuration
    [Header("Configuration")]
    public InputField inputStraightGal;
    public InputField inputDickgirlGal, inputStraightVid, inputDickgirlVid;

    #endregion

    public new static E621_Navigation act;
    // Use this for initialization
    void Start ()
    {
        act = this;
        checkExistanceCo = StartCoroutine(CheckExistanceQueue());
        LoadBlacklist();
        threadPageLoad = new Thread(new ThreadStart(ThreadedLoadPage));
        threadPageLoad.IsBackground = true;
        buttonNext.interactable = false;
        buttonPrev.interactable = false;

        Data.act.tagSelectorFunc += SendTagToSearch;

        SetSourcesToDefault();
    }

    private void OnDestroy()
    {
        Data.act.tagSelectorFunc -= SendTagToSearch;
        if (threadPageLoad.IsAlive) threadPageLoad.Abort(threadPageLoad);
    }

    // Update is called once per frame
    void Update ()
    {
        inputGoToPage.interactable = currentPage != 0;
        textCurrentPage.text = lastPage == 0 ? "Current Page:\n0/0" : "Current Page:\n" + currentPage + "/" + lastPage;
    }

    #region PageLoad Functions

    public void StopLoadPageCo()
    {
        if (pageLoadCo != null) StopCoroutine(pageLoadCo);
        if (threadPageLoad.IsAlive) threadPageLoad.Abort(threadPageLoad);
    }

    void LoadPage()
    {
        buttonSearch.interactable = false;
        buttonReturn.interactable = false;
        ClearViewer();
        StopLoadPageCo();
        if (threadPageLoad.IsAlive) threadPageLoad.Abort();
        pageLoadCo = StartCoroutine(LoadPageCoroutine());
    }

    IEnumerator LoadPageCoroutine()
    {
        yield return null;
        inputSearchField.text = currentTags;
        string urlTags = currentTags;
        if (toggleBlacklistSearch.isOn)
        {
            foreach (string s in Data.act.e621Blacklist)
            {
                urlTags += urlTags == "" ? "~" + s : " ~" + s;
            }
        }
        urlTags = urlTags.Replace(" ", "%20");
        string url = @"https://e621.net/post/index/" + currentPage + "/" + urlTags;

        print(url);

        objLoadPageHourglass.SetActive(true);
        //Get the page
        html = "";
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            //yield return uwr.SendWebRequest();
            uwr.SendWebRequest();

            while (!uwr.isDone)
            {
                objLoadPageHourglass.transform.GetChild(0).GetComponent<Image>().fillAmount = uwr.downloadProgress;
                yield return null;
            }
            objLoadPageHourglass.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                objLoadPageHourglass.transform.GetChild(0).GetComponent<Image>().sprite = imgError;
                buttonSearch.interactable = false;
                buttonReturn.interactable = true;
                CreateAdvice(uwr.error, 2);
                Debug.Log(uwr.error);
                uwr.Dispose();
                yield break;
            }
            else
            {
                html = uwr.downloadHandler.text;
                uwr.Dispose();
            }
        }
        Resources.UnloadUnusedAssets();

        //Revisar si existen imagenes
        if (html.IndexOf("Post.register({") == -1)
        {
            buttonSearch.interactable = true;
            buttonReturn.interactable = true;
            objLoadPageHourglass.SetActive(false);
            CreateAdvice("ERROR", "There are no images with this tag(s).");
            yield break;
        }

        //Si se inicio desde el boton search
        if(currentPage == 0)
        {
            buttonNext.interactable = true;
            buttonPrev.interactable = false;
            currentPage = 1;
        }

        //obtener la ultima pagina
        if (html.IndexOf("Last Page") != -1)
        {
            string last = html.Substring(0, html.IndexOf("Last Page"));
            //print(last);
            last = last.Substring(last.LastIndexOf("href=") + 6, last.Length - (last.LastIndexOf("href=") + 6));
            //print(last);
            last = last.Substring(last.IndexOf("index") + 6, last.Length - (last.IndexOf("index") + 6));
            //print(last);
            if (urlTags != "")
                last = last.Substring(0, last.IndexOf("/"));
            else
                last = last.Substring(0, last.IndexOf("rel") - 2);

            lastPage = int.Parse(last);
        }
        else
            lastPage = 1;

        
        //substring para obtener los datos de los posts
        html = html.Substring(html.IndexOf("Post.register({"), html.Length - html.IndexOf("Post.register({"));
        html = html.Substring(0, html.IndexOf("Post.blacklist_options ="));

        

        objLoadPageHourglass.SetActive(false);
        pageLoadCo = null;
        StartThreadLoadPage();
    }

    void StartThreadLoadPage()
    {
        if (threadPageLoad.IsAlive)
        {
            threadPageLoad.Abort(threadPageLoad);
            
        }
        threadPageLoad = new Thread(new ThreadStart(ThreadedLoadPage));
        threadPageLoad.IsBackground = true;
        threadPageLoad.Start();

    }


    void ThreadedLoadPage()
    {
        try
        {
            //Ejecutar hasta que no haya mas datos
            while (html.IndexOf("Post.register({") != -1)
            {

                html = html.Substring(html.IndexOf("Post.register({"), html.Length - html.IndexOf("Post.register({"));
                //eliminar el post register actual para no ciclarse infinitamente
                html = html.Substring(15, html.Length - 15);


                int id = -1;
                List<string> tags = new List<string>();
                string urlDownload = "";
                string urlThumb = "";
                string urlPreview = "";
                string rating = "";
                string status = "";
                string md5 = "";

                string temp;
                //obtener datos del post
                //ID
                temp = html.Substring(html.IndexOf("id") + 4, html.IndexOf(",") - 5);
                id = int.Parse(temp);
                html = html.Substring(html.IndexOf(",") + 9, html.Length - html.IndexOf(",") - 9);
                //print(html);

                //tags
                temp = html.Substring(0, html.IndexOf(",") - 1);
                temp += " ";
                while (temp.IndexOf(" ") != -1)
                {
                    tags.Add(temp.Substring(0, temp.IndexOf(" ")));
                    temp = temp.Substring(temp.IndexOf(" ") + 1, temp.Length - (temp.IndexOf(" ") + 1));
                }

                html = html.Substring(html.IndexOf("md5") + 6, html.Length - (html.IndexOf("md5") + 6));
                //MD5
                md5 = html.Substring(0, html.IndexOf(",") - 1);
                //print("MD5: " + md5);
                html = html.Substring(html.IndexOf("file_url") + 11, html.Length - (html.IndexOf("file_url") + 11));

                //Image URL
                urlDownload = html.Substring(0, html.IndexOf(",") - 1);
                //print(urlDownload);
                html = html.Substring(html.IndexOf("preview_url") + 14, html.Length - (html.IndexOf("preview_url") + 14));

                //Thumb URL
                urlThumb = html.Substring(0, html.IndexOf(",") - 1);
                //print(urlThumb);
                html = html.Substring(html.IndexOf("sample_url") + 13, html.Length - (html.IndexOf("sample_url") + 13));

                //Preview URL
                urlPreview = html.Substring(0, html.IndexOf(",") - 1);
                //print(urlPreview);
                html = html.Substring(html.IndexOf("rating") + 9, html.Length - (html.IndexOf("rating") + 9));

                //Rating
                rating = html.Substring(0, html.IndexOf(",") - 1);
                //print(html);
                print("Rating: " + rating);
                html = html.Substring(html.IndexOf("rating") + 9, html.Length - (html.IndexOf("rating") + 9));

                //Status
                html = html.Substring(html.IndexOf("status") + 9, html.Length - (html.IndexOf("status") + 9));
                status = html.Substring(0, html.IndexOf(",") - 1);
                //print(status);

                //Spawn Button
                bool end = false;
                UnityThread.executeInUpdate(() =>
                {
                    GameObject button = Instantiate(prefabNavButton, transformNavigation);
                    E621_NavigationButton b = button.GetComponent<E621_NavigationButton>();
                    b.urlDownload = urlDownload;
                    b.urlThumb = urlThumb;
                    b.urlPreview = urlPreview;
                    b.rating = rating;
                    b.status = status;
                    b.md5 = md5;
                    b.urlPage = @"https://e621.net/post/index/1/id:" + id;
                    b.id = id;
                    b.tags = tags;
                    b.Initialize();
                    end = true;
                });
                while (!end) { }
            }

            UnityThread.executeInUpdate(() =>
            {
                buttonSearch.interactable = true;
                buttonReturn.interactable = true;
            });
        }
        catch
        {
            print("Thread Error: Navigation.");
            return;
        }
        
    }

    public void ClearViewer()
    {
        objLoadPageHourglass.SetActive(false);
        //print(transformNavigation.childCount);
        for (int i = transformNavigation.childCount - 1; i >= 0; i--)
        {
            Destroy(transformNavigation.GetChild(i).gameObject);
        }
    }

    public IEnumerator CheckExistanceQueue()
    {
        while (true)
        {
            yield return null;
            if (queueExistance.Count != 0)
            {
                E621_NavigationButton b = queueExistance.Dequeue();
                b.existanceChecked = false;
                b.CheckExistance();
                float waitTime = Time.time + 3f;
                int maxTries = 3, currentTries = 1;
                do
                {
                    yield return null;
                    if (waitTime <= Time.time && !b.existanceChecked && maxTries < currentTries)
                    {
                        waitTime = Time.time + 3f;
                        print("Retry");
                        currentTries++;
                    }
                    else if (!b.existanceChecked && maxTries >= currentTries)
                    {
                        b.imageExistance.color = Color.magenta;
                    }
                }
                while (waitTime > Time.time && !b.existanceChecked);
            }
        }
    }
    #endregion

    #region Blacklist Funtions
    public void ButtonBlacklist(string value)
    {
        GameObject b = GameObject.Find(value);
        Data.act.e621Blacklist.Remove(b.name);
        Destroy(b);
    }

    

    public void InputBlacklistTag(string value)
    {
        GameObject newButton = Instantiate(prefabBlacklistButton, transformBlackList);
        newButton.name = value;
        newButton.transform.GetChild(0).GetComponent<Text>().text = value;
        newButton.GetComponent<Button>().onClick.AddListener(delegate { ButtonBlacklist(value); });
        Data.act.e621Blacklist.Add(value);
        inputNewBlacklistTag.text = "";
    }

    public void LoadBlacklist()
    {
        foreach(string s in Data.act.e621Blacklist)
        {
            GameObject newButton = Instantiate(prefabBlacklistButton, transformBlackList);
            newButton.name = s;
            newButton.transform.GetChild(0).GetComponent<Text>().text = s;
            newButton.GetComponent<Button>().onClick.AddListener(delegate { ButtonBlacklist(s); });
        }
    }

    public void ButtonSaveBlacklist()
    {
        Data.act.SaveData("e621Blacklist");
    }

    #endregion

    #region Navigation Functions
    public void ButtonAction(string value)
    {
        if (threadPageLoad.IsAlive) return;
        switch(value)
        {
            case "search":
                currentTags = inputSearchField.text;
                
                currentPage = 0;
                LoadPage();
                break;
            case "next":
                currentPage++;
                buttonNext.interactable = currentPage != lastPage;
                buttonPrev.interactable = lastPage != 1;
                LoadPage();
                break;
            case "prev":
                currentPage--;
                buttonPrev.interactable = currentPage != 1;
                buttonNext.interactable = lastPage != 1;
                LoadPage();
                break;
            case "tags":
                Data.act.TagsOpenGameObject();
                break;
        }
        StopCoroutine(checkExistanceCo);
        checkExistanceCo = StartCoroutine(CheckExistanceQueue());
        queueExistance.Clear();
    }

    public void SendTagToSearch(string theTag)
    {
        if (inputSearchField.text != "") inputSearchField.text += " ";
        inputSearchField.text += theTag;
    }
    #endregion

    #region Downloading Funcitons
    public void OnPointerEnter()
    {
        Vector3 newPos = downloaderContent.anchoredPosition;
        newPos.x *= -1;
        downloaderContent.anchoredPosition = newPos;
    }

    public IEnumerator DownloadImageCo(string url, Sprite sprPrev, GameObject objProgress, ImageData downloaded, E621_NavigationButton origin)
    {
        yield return null;
        Image imgProgress = objProgress.transform.GetChild(0).GetComponent<Image>();
        Image imgPrev = objProgress.transform.GetChild(1).GetComponent<Image>();
        imgPrev.sprite = sprPrev;
        Text textProgress = objProgress.transform.GetChild(2).GetComponent<Text>();

        textProgress.text = "...Downloading...";
        
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.SendWebRequest();
            while (!uwr.isDone)
            {
                imgProgress.fillAmount = uwr.downloadProgress;
                yield return null;
            }
            //yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                textProgress.text = uwr.error;
                textProgress.color = Color.red;
                Debug.Log(uwr.error);
                yield return new WaitForSeconds(10f);
                Destroy(objProgress);
            }
            else
            {
                //https://static1.e621.net/data/fa/04/fa0477361df7971421e00f3c38950f47.jpg
                string filename = url.Substring(url.LastIndexOf("/") + 1, url.Length - (url.LastIndexOf("/") + 1));
                filename = downloaded.id + "-" +filename;
                byte[] result = uwr.downloadHandler.data;

                //string targetUrl = @"D:\HardDrive\No pls\e621\Te lo advierto\Tools\test";
                string targetUrl = !(downloaded.tags.Contains("dickgirl") || downloaded.tags.Contains("intersex") || downloaded.tags.Contains("herm"))
                    ? inputStraightGal.text : inputDickgirlGal.text;

                if (downloaded.tags.Contains("animated"))
                {
                    targetUrl = !targetUrl.Contains("Dickgirl") ? inputStraightVid.text : inputDickgirlVid.text;
                }
                File.WriteAllBytes(targetUrl + "/" + filename, result);
                print(targetUrl);

            }
        }

        Resources.UnloadUnusedAssets();
        imgProgress.fillAmount = 0f;
        textProgress.text = "Done...";
        try
        {
            queueExistance.Enqueue(origin);
        }
        catch
        {
            print("IMG download: Button didn't exist anymore");
        }
        yield return new WaitForSeconds(1f);
        listCoroutines.Remove(listCoroutines.Where(temp => temp.data.id == downloaded.id).Single());
        Destroy(objProgress);
        
    }

    #endregion

    #region Configuration Functions
    public void SetSourcesToDefault()
    {
        inputStraightGal.text = PlayerPrefs.GetString("E621_StraightMainGal");
        inputDickgirlGal.text = PlayerPrefs.GetString("E621_DickgirlMainGal");
        inputStraightVid.text = PlayerPrefs.GetString("E621_StraightMainVidGal");
        inputDickgirlVid.text = PlayerPrefs.GetString("E621_DickgirlMainVidGal");
    }

    public void ConfigApply()
    {
        if (!Directory.Exists(inputStraightGal.text)) inputStraightGal.text = ""; else PlayerPrefs.SetString("E621_StraightMainGal", inputStraightGal.text);
        if (!Directory.Exists(inputDickgirlGal.text)) inputDickgirlGal.text = ""; else PlayerPrefs.SetString("E621_DickgirlMainGal", inputDickgirlGal.text);
        if (!Directory.Exists(inputStraightVid.text)) inputStraightVid.text = ""; else PlayerPrefs.SetString("E621_StraightMainVidGal", inputStraightVid.text);
        if (!Directory.Exists(inputDickgirlVid.text)) inputDickgirlVid.text = ""; else PlayerPrefs.SetString("E621_DickgirlMainVidGal", inputDickgirlVid.text);
    }
    #endregion

    public void SaveDataPopUp()
    {
        CreateAdvice("Would you like to save the Image Data?", 0, () =>
         {
             Data.act.SaveData("imageData");
         });
    }
    

    
}
