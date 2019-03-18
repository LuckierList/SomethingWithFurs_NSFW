using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System.IO;


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
    public GameObject objLoadPageHourglass;
    Coroutine pageLoadCo;

    public InputField inputSearchField, inputGoToPage;
    public Button buttonSearch, buttonNext, buttonPrev;
    public Text textCurrentPage;
    string currentTags;
    int currentPage = 1;
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

    #region General
    [Header("General")]
    public Sprite imgBlank;
    public Sprite imgLoading, imgError, imgHourGlass, imgViewLocked;
    #endregion

    // Use this for initialization
    void Start ()
    {
		
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
        pageLoadCo = StartCoroutine(LoadPageCoroutine());
    }

    IEnumerator LoadPageCoroutine()
    {
        yield return null;
        string url = @"https://e621.net/post/index/" + currentPage + "/" +  currentTags;

        objLoadPageHourglass.SetActive(true);
        //Get the page
        string html = "";
        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            yield return uwr.SendWebRequest();
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

        //Ejecutar hasta que no haya mas datos
        while (html.IndexOf("Post.register({") != -1)
        {
            yield return null;
            //eliminar el post register actual para no ciclarse infinitamente
            html = html.Substring(15, html.Length - 15);


            int id = -1;
            List<string> tags = new List<string>();
            string urlPage = "";
            string urlThumb = "";
            string urlPreview = "";
            string urlDownload = "";
            string rating = "";
            string status = "";

            //obtener datos del post
            //ID
            {
                string temp = html.Substring(html.IndexOf("id") + 4, html.IndexOf(",") - 5);
                print(temp);
                id = int.Parse(temp);
            }
            html = html.Substring(html.IndexOf(",") + 9, html.Length - html.IndexOf(",") - 9);
            print(html);
            //tags
            {
                string temp = html.Substring(0, html.IndexOf(",") - 2);
                print(temp);
                while(temp.IndexOf(" ") != -1)
                {
                    yield return null;
                    tags.Add(temp.Substring(0, temp.IndexOf(" ")));
                    temp.Substring(temp.IndexOf(" ") + 2, temp.Length - (temp.IndexOf(" ") + 2));
                }
                for(int i = 0; i < tags.Count; i++)
                {
                    yield return null;
                    print(tags[i]);
                }
            }
            //
        }
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
}
