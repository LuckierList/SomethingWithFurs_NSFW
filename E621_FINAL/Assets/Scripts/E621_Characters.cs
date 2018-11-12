using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;


public class E621_Characters : GlobalActions
{
    public static E621_Characters act;
    public int[] buttonSizesValue;
    public int[] buttonSizesQuantity;
    [HideInInspector]
    public string[] files;
    [HideInInspector]
    public string[] filesUnedited;
    [HideInInspector]
    public List<string> showFiles;
    [HideInInspector]
    public List<string> showFilesUnedited;
    public int currentPage;
    public int imagesPerPage;
    public int selectedChar;
    //private static string extraInfo = "Images:\n{0}";//\nFiltered:\n{1}\nRatio:\n{2}";

    [Header("Main")]
    public GameObject prefabCharButton;
    public Dropdown dropFilter, dropViewerSize;
    public Toggle toggleFilter;
    public InputField inputSource;
    public GameObject objViewer, objEditor, objGridParent;
    public Button buttonNext, buttonPrev, buttonFirst, buttonLast;
    public GridLayoutGroup gridButtons;
    [Header("Editor")]
    public Sprite imgLoading, imgError;
    public Text textCharName;
    public Text textExtraInfo;
    public Image imageThumbSmall, imageThumbBig;
    public Toggle[] toggles;
    public Dropdown dropAnimated, dropFetish, dropQuality;
    E621CharacterData loadedData;

    // Use this for initialization
    public override void Awake()
    {
        base.Awake();
        inputSource.text = PlayerPrefs.GetString("E621_CharacterSource");
        ButtonSizeManager(1, false);
        ButtonAction("configApply");
        act = this;
    }

    private void Start()
    {
        ButtonSizeManager(dropViewerSize.value);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //General

    void CreateFileList()
    {
        showFiles.Clear();
        showFilesUnedited.Clear();
        currentPage = 0;
        /*
        string deleteThis = showFiles.Where(temp => temp.Contains("desktop.ini")).SingleOrDefault();
        if(deleteThis != null) showFiles.Remove(deleteThis);
        */
        //Hacer los filtros-------
        //showFiles = files.ToList();
        int filterIndex = dropFilter.value -1;
        foreach (string s in files)
        {
            if (s.Contains("desktop")) continue;
            if(filterIndex == -1)
                showFiles.Add(s);
            else
            {
                E621CharacterData loaded = Data.act.e621CharacterData.Where(
                    temp => temp.tagName == Path.GetFileNameWithoutExtension(s)).SingleOrDefault();
                if (loaded != null)
                {
                    if(loaded.booleans[filterIndex])
                        showFiles.Add(s);
                    
                }
                else if (loaded == null && toggleFilter.isOn)
                    showFiles.Add(s);
            }
        }
        showFilesUnedited = filesUnedited.ToList();
        //------------------------
        currentPage = 0;
        ShowPage(currentPage);
        SetNavigationButtons(true, false, true, false);
    }

    void SetNavigationButtons(bool next, bool prev, bool last, bool first)
    {
        buttonNext.interactable = next;
        buttonPrev.interactable = prev;
        buttonLast.interactable = last;
        buttonFirst.interactable = first;
    }

    public void ButtonAction(string value)
    {
        switch (value)
        {
            //Navigation
            case "mainMenu":
                ClearGridChilds();
                OpenSceneAsync("mainMenu");
                break;
            case "next":
                currentPage++;
                if((currentPage + 1) * imagesPerPage >= showFiles.Count)
                {
                    SetNavigationButtons(false, true, false, true);
                }
                else
                {
                    SetNavigationButtons(true, true, true, true);
                }
                ShowPage(currentPage);
                break;
            case "prev":
                currentPage--;
                if (currentPage - 1 == -1)
                {
                    SetNavigationButtons(true,false,true,false);
                }
                else
                {
                    SetNavigationButtons(true, true, true, true);
                }
                ShowPage(currentPage);
                break;
            case "first":
                currentPage = 0;
                SetNavigationButtons(true, false, true, false);
                ShowPage(currentPage);
                break;
            case "last":
                currentPage = showFiles.Count / imagesPerPage;
                SetNavigationButtons(false, true, false, true);
                ShowPage(currentPage);
                break;
            case "configApply":
                if (Directory.Exists(inputSource.text))
                {
                    PlayerPrefs.SetString("E621_CharacterSource", inputSource.text);
                    files = Directory.GetFiles(inputSource.text);
                    filesUnedited = Directory.GetFiles(inputSource.text + @"\Unedited");
                    CreateFileList();
                    currentPage = 0;
                }
                else
                {
                    CreateAdvice("Warning!", "The source URL is not valid, no characters will be shown.");
                }
                break;
            case "save":
                CreateAdvice("Are you sure you want to overwrite 'Character.DATA'?",0,() =>
                {
                    Data.act.SaveData("e621CharacterData");
                });
                break;
            //Editor
            case "apply":
                objEditor.SetActive(false);
                loadedData.edited = true;
                E621CharacterData oldData = Data.act.e621CharacterData.Where(temp => temp.tagName == loadedData.tagName).SingleOrDefault();
                if(oldData != null)
                {
                    Data.act.e621CharacterData.Remove(oldData);
                    print("Detected old data and removed it.");
                }

                for (int i = 0; i < toggles.Length; i++) loadedData.booleans[i] = toggles[i].isOn;
                loadedData.animated = dropAnimated.value;
                loadedData.fetish = dropFetish.value;
                loadedData.quality = dropQuality.value;

                Data.act.e621CharacterData.Add(loadedData);
                objViewer.SetActive(true);
                objEditor.SetActive(false);
                ShowPage(currentPage);
                break;
            case "cancel":
                objEditor.SetActive(false);
                objViewer.SetActive(true);
                ShowPage(currentPage);
                break;
            case "openInPage":
                Application.OpenURL("https://e621.net/post/index/1/"+loadedData.tagName);
                break;
            case "openSource":
                Application.OpenURL(inputSource.text);
                break;
        }
    }
    //Viewer
    public void ShowPage(int page)
    {
        ClearGridChilds();
        Resources.UnloadUnusedAssets();
        //print("SHow page");
        E621_CharacterButton b = prefabCharButton.GetComponent<E621_CharacterButton>();
        float contDelay = 0f;
        for (int i = 0 ; i < imagesPerPage; i++)
        {
            int correctID = i + (imagesPerPage * currentPage);
            //print("for");
            if (correctID == showFiles.Count) break;
            b.id = correctID;
            E621CharacterData data = Data.act.e621CharacterData.Where
                (
                    temp => temp.tagName == Path.GetFileNameWithoutExtension(showFiles[correctID])
                ).SingleOrDefault();

            if (data == null)
                data = new E621CharacterData(Path.GetFileNameWithoutExtension(showFiles[correctID]), showFiles[correctID]);
            b.url = data.urlSmall;
            b.edited = data.edited;
            b.delay = contDelay;
            b.data = data;
            Instantiate(prefabCharButton, objGridParent.transform);
            contDelay += 0.001f;
        }
    }

    public void FilterHandler()
    {
        CreateFileList();
    }

    void ClearGridChilds()
    {
        int childCount = objGridParent.transform.childCount;
        for(int i = childCount - 1; i >= 0; i--)
        {
            objGridParent.transform.GetChild(i).GetComponent<E621_CharacterButton>().StopThisCoroutine();
            Destroy(objGridParent.transform.GetChild(i).gameObject);
        }
    }
    public void ButtonSizeManager(int value)
    {
        ButtonSizeManager(value, true);
    }

    public void ButtonSizeManager(int value, bool show)
    {
        int size = 0, newImagesPerPage = 0;
        size = buttonSizesValue[value];
        newImagesPerPage = buttonSizesQuantity[value];
        gridButtons.cellSize = new Vector2(size, size);
        imagesPerPage = newImagesPerPage;
        if(show)
        {
            SetNavigationButtons(true, false, true, false);
            currentPage = 0;
            ShowPage(0);
        }
    }

    //Editor
    public void OpenInEditor(E621CharacterData data, int id, Sprite sprImage)
    {
        print("oof");
        loadedData = data;
        ClearGridChilds();
        textCharName.text = loadedData.tagName;
        for(int i = 0; i < toggles.Length; i++)
        {
            toggles[i].isOn = loadedData.booleans[i];
        }
        dropAnimated.value = loadedData.animated;
        dropAnimated.RefreshShownValue();
        dropFetish.value = loadedData.fetish;
        dropFetish.RefreshShownValue();
        dropQuality.value = loadedData.quality;
        dropQuality.RefreshShownValue();
        //LoadImage(imgLoading, imgError, imageThumbSmall, loadedData.urlSmall);
        imageThumbSmall.sprite = sprImage;
        print("Files: " + showFiles.Count + "\nUnedited: " + showFilesUnedited.Count);
        if (loadedData.urlBig == "" && (showFiles.Count == showFilesUnedited.Count))
        {
            loadedData.urlBig = showFilesUnedited[id];
            print(loadedData.urlBig);
        }
        else if(loadedData.urlBig == "")
        {
            CreateAdvice("WARNING!", "Character images in the directory DO NOT match the Unedited character images, you should check that, else you're not getting a big preview.", 1);
        }
        //print(loadedData.urlBig);
        LoadImage(imgLoading, imgError, imageThumbBig, loadedData.urlBig);

        int imageTotal = 0;
        foreach (ImageData a in Data.act.imageData)
        {
            if (a.tags.Contains(Path.GetFileNameWithoutExtension(showFiles[id])))
            {
                imageTotal++;
            }
        }
        //string.Format(extraInfo, imageTotal);
        textExtraInfo.text = "Images:\n" + imageTotal;
        objViewer.SetActive(false);
        objEditor.SetActive(true);
    }
}
