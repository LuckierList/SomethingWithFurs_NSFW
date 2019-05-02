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
    public int id;
    public Coroutine thisCoroutine;
    public GameObject objHourglass;
    public DownloadImage(int _id, GameObject _objHourglass)
    {
        id = _id;
        objHourglass = _objHourglass;
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

    public InputField inputSearchField, inputGoToPage;
    public Button buttonSearch, buttonNext, buttonPrev;
    public Text textCurrentPage;
    string currentTags;
    int currentPage = 1;

    Thread threadPageLoad;
    string html = "";
    #endregion

    #region PreviewDownloader
    [Header("Preview Downloader")]
    public GameObject objPreviewViewer;
    List<string> previewURLs = new List<string>();
    #endregion

    #region DownloadHandler
    [Header("Download Handler")]
    public GameObject prefabDownloading;
    public Transform downloaderContent;
    List<DownloadImage> listCoroutines = new List<DownloadImage>();

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
    public Toggle toggleBlacklistHide;
    #endregion

    public new static E621_Navigation act;
    // Use this for initialization
    void Start ()
    {
        act = this;
        LoadBlacklist();
        threadPageLoad = new Thread(new ThreadStart(ThreadedLoadPage));
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    #region PageLoad Functions

    public void StopLoadPageCo()
    {
        if (pageLoadCo != null) StopCoroutine(pageLoadCo);
    }

    void LoadPage()
    {
        StopLoadPageCo();
        if (threadPageLoad.IsAlive) threadPageLoad.Abort();
        pageLoadCo = StartCoroutine(LoadPageCoroutine());
    }

    IEnumerator LoadPageCoroutine()
    {
        yield return null;
        ClearViewer();

        string url = @"https://e621.net/post/index/" + currentPage + "/" +  currentTags;

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
        
        //substring para obtener los datos de los posts
        html = html.Substring(html.IndexOf("Post.register({"), html.Length - html.IndexOf("Post.register({"));
        html = html.Substring(0, html.IndexOf("Post.blacklist_options ="));

        

        objLoadPageHourglass.SetActive(false);
        pageLoadCo = null;
        StartThreadLoadPage();
    }

    void StartThreadLoadPage()
    {
        if (threadPageLoad.IsAlive) threadPageLoad.Abort();
        threadPageLoad = new Thread(new ThreadStart(ThreadedLoadPage));
        print("oof");
        threadPageLoad.Start();
    }


    void ThreadedLoadPage()
    {
        //Ejecutar hasta que no haya mas datos
        while (html.IndexOf("Post.register({") != -1)
        {
            UnityThread.executeInUpdate(() => { print("LEL"); });
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
            temp = html.Substring(0, html.IndexOf(",") - 2);
            while (temp.IndexOf(" ") != -1)
            {
                tags.Add(temp.Substring(0, temp.IndexOf(" ")));
                temp = temp.Substring(temp.IndexOf(" ") + 1, temp.Length - (temp.IndexOf(" ") + 1));
                
            }

            html = html.Substring(html.IndexOf("md5") + 6, html.Length - (html.IndexOf("md5") + 6));
            //MD5
            md5 = html.Substring(0, html.IndexOf(",") - 1);
            print("MD5: " + md5);
            html = html.Substring(html.IndexOf("file_url") + 11, html.Length - (html.IndexOf("file_url") + 11));

            //Image URL
            urlDownload = html.Substring(0, html.IndexOf(",") - 1);
            print(urlDownload);
            html = html.Substring(html.IndexOf("preview_url") + 14, html.Length - (html.IndexOf("preview_url") + 14));

            //Thumb URL
            urlThumb = html.Substring(0, html.IndexOf(",") - 1);
            print(urlThumb);
            html = html.Substring(html.IndexOf("sample_url") + 13, html.Length - (html.IndexOf("sample_url") + 13));

            //Thumb URL
            urlPreview = html.Substring(0, html.IndexOf(",") - 1);
            print(urlPreview);
            html = html.Substring(html.IndexOf("rating") + 9, html.Length - (html.IndexOf("rating") + 9));

            //Rating
            rating = html.Substring(0, html.IndexOf(",") - 1);
            print(rating);
            html = html.Substring(html.IndexOf("rating") + 9, html.Length - (html.IndexOf("rating") + 9));

            //Rating
            rating = html.Substring(0, html.IndexOf(",") - 1);
            print(rating);
            html = html.Substring(html.IndexOf("status") + 9, html.Length - (html.IndexOf("status") + 9));

            //Status
            status = html.Substring(0, html.IndexOf(",") - 1);
            print(status);

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
    }

    public void ClearViewer()
    {
        objLoadPageHourglass.SetActive(false);
        for (int i = 0; i < transformNavigation.childCount; i++)
        {
            Destroy(transformNavigation.GetChild(0).gameObject);
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

    public void ButtonAction(string value)
    {
        switch(value)
        {
            case "search":
                currentTags = inputSearchField.text;
                currentPage = 1;
                LoadPage();
                break;
            
        }
    }

    private void OnDestroy()
    {
        if (threadPageLoad.IsAlive) threadPageLoad.Abort();
        Resources.UnloadUnusedAssets();
    }
}
