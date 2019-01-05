using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Threading;


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
    public InputField inputSource, inputStraightGal, inputDickgirlGal, inputFilteredFolder;
    bool canFilterChars;
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
    public Button[] buttonsFilter;

    [Header("Viewer")]
    public GameObject objViewerChars;
    public Button buttonShowTheirImages;
    public Toggle toggleErasePrevious;
    public Text textViewerName, textViewerData, textViewerAppearances;
    public Image imageViewerPortrait, imageViewerSex;
    public Color colorGreen, colorRed, colorOrange, colorBlack;
    [SerializeField]
    List<string> urlsCharacterSaved = new List<string>();
    [SerializeField]
    List<string> urlsCharacterToShow = new List<string>();
    string currentChar;
    List<string> currentSelectedChars = new List<string>();
    // Use this for initialization
    public override void Awake()
    {
        base.Awake();
        inputSource.text = PlayerPrefs.GetString("E621_CharacterSource");
        inputStraightGal.text = PlayerPrefs.GetString("E621_StraightMainGal");
        inputDickgirlGal.text = PlayerPrefs.GetString("E621_DickgirlMainGal");
        inputFilteredFolder.text = PlayerPrefs.GetString("E621_CharsFiltered");

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
                string messages = "";
                canFilterChars = true;
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
                    messages += "The source URL is not valid, no characters will be shown.\n";
                }

                if(Directory.Exists(inputStraightGal.text) && Directory.Exists(inputDickgirlGal.text) && Directory.Exists(inputFilteredFolder.text))
                {
                    PlayerPrefs.SetString("E621_StraightMainGal", inputStraightGal.text);
                    PlayerPrefs.SetString("E621_DickgirlMainGal", inputDickgirlGal.text);
                    PlayerPrefs.SetString("E621_CharsFiltered", inputFilteredFolder.text);
                }
                else
                {
                    canFilterChars = false;
                    messages += "The Straight/Dickgirl gallery or the filtered folder URLs don't exist, te three of them need to be correct!";
                }

                foreach(Button b in buttonsFilter)
                {
                    b.interactable = canFilterChars;
                }
                if (messages != "") CreateAdvice("Warning!", messages, 1);

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
                objViewerChars.SetActive(false);
                objViewer.SetActive(true);
                ShowPage(currentPage);
                break;
            case "openInPage":
                Application.OpenURL("https://e621.net/post/index/1/"+loadedData.tagName);
                break;
            case "openSource":
                Application.OpenURL(inputSource.text);
                break;
            //Viewer
            case "showImages":
                if (currentSelectedChars.Count == 0)
                {
                    CreateAdvice("No characters have been choosen.");
                    break;
                }
                string currentChars = "The selected character(s) are:\n\n";
                for(int i = 0; i < currentSelectedChars.Count; i++)
                {
                    currentChars += currentSelectedChars[i] + ((i != currentSelectedChars.Count - 1) ? " | " : "");
                }
                currentChars += "\n\nTotal Images: " + urlsCharacterToShow.Count + "\nContinue?";
                CreateAdvice(currentChars, 2,
                    () =>
                    {
                        LoadingReset("Creating folder with images of the desired character(s).");
                        Thread t = new Thread(new ThreadStart(ShowImagesInFolderThread));
                        t.Start();
                    }
                );
                break;
            case "addImages":
                if (toggleErasePrevious.isOn)
                {
                    currentSelectedChars.Clear();
                    urlsCharacterToShow.Clear();
                }

                if (!currentSelectedChars.Contains(currentChar)) currentSelectedChars.Add(currentChar);

                foreach(string s in urlsCharacterSaved)
                {
                    if (!urlsCharacterToShow.Contains(s))
                    {
                        urlsCharacterToShow.Add(s);
                    }
                    else print("Existing image skiped");
                }
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
            //b.url = data.urlSmall;
            b.url = showFiles[correctID];
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

    //Viewer of Character
    public void OpenOnViewer(E621CharacterData data, int id, Sprite sprImage)
    {
        objViewer.SetActive(false);
        loadedData = data;
        imageViewerPortrait.sprite = sprImage;
        ClearGridChilds();
        textViewerName.text = loadedData.tagName;
        const string dataFormat = "Categories:\n{0}\n\nFavorite:\n{1}\n\nAnimated Appearances:\n{2}\n\nFetish Worth:\n{3}\n\nGood & Constant Quality:\n{4}";
        string categories = "";
        

        if (data.booleans[1]) categories += "Anthro, ";
        if (data.booleans[2]) categories += "Mid-Beast, ";
        if (data.booleans[3]) categories += "Angel, ";
        if (data.booleans[4]) categories += "Demon, ";
        if (data.booleans[5]) categories += "Marine, ";
        if (data.booleans[7]) categories += "Dragon, ";
        if (data.booleans[8]) categories += "Robot, ";
        if (data.booleans[9]) categories += "Human, ";
        if (data.booleans[10]) categories += "Egyptian, ";
        if (data.booleans[11]) categories += "Other, ";
        if (categories != "")
            categories = categories.Substring(0, categories.Length - 2) + ".";
        else
            categories = "Not Specified";
        if (!data.booleans[0] && !data.booleans[6])
            imageViewerSex.color = colorBlack;
        else if (data.booleans[0] && !data.booleans[6])
            imageViewerSex.color = colorGreen;
        else if (!data.booleans[0] && data.booleans[6])
            imageViewerSex.color = colorRed;
        else
            imageViewerSex.color = colorOrange;

        string animated = "";
        string fetish = "";
        string quality = "";
        for (int i = 0; i < 4; i++)
        {
            int theInt = 0;
            if (i == 0) theInt = data.animated;
            if (i == 1) theInt = data.fetish;
            if (i == 3) theInt = data.quality;
            string say = "";
            switch (theInt)
            {
                case 0:
                    say = "Doesn't Apply";
                    break;
                case 1:
                    say = "A bit of it";
                    break;
                case 2:
                    say = "A 50/50, maybe";
                    break;
                case 3:
                    say = "Worth it";
                    break;
                case 4:
                    say = "ABSOLUTELY";
                    break;
            }
            if (i == 0) animated = say;
            if (i == 1) fetish = say;
            if (i == 3) quality = say;
        }

        textViewerData.text = string.Format(dataFormat, categories, data.booleans[12] ? "Yes." : "No.", animated, fetish, quality);
        int imageTotal = 0;
        int imageStraigth = 0, imageDickgirl = 0;

        urlsCharacterSaved.Clear();
        foreach (ImageData a in Data.act.imageData)
        {
            if (a.tags.Contains(Path.GetFileNameWithoutExtension(showFiles[id])))
            {
                urlsCharacterSaved.Add(a.filename);
                imageTotal++;

                if(a.tags.Contains("dickgirl") || a.tags.Contains("intersex") || a.tags.Contains("herm"))
                {
                    imageDickgirl++;
                }
                else
                {
                    imageStraigth++;
                }
            }
        }

        currentChar = Path.GetFileNameWithoutExtension(showFiles[id]) + " (" + imageTotal +")";

        textViewerAppearances.text = "Appeared '" + imageTotal + "' times.\n" + imageStraigth + " - Straight\n" + imageDickgirl + " - Dickgirl";
        objViewerChars.SetActive(true);
    }

    void ShowImagesInFolderThread()
    {
        bool done = false;
        int cont = 0;
        int added = 0;
        float contImages = 0;
        string[] filesOnFolder = null;
        List<string> filesToShow = null;
        while (cont < 2)
        {
            UnityThread.executeInUpdate(() =>
            {
                if (filesToShow == null) filesToShow = new List<string>();
                LoadingReset("Getting the files from a folder. First is straight, second is dickgirl. Might take a while...");
                StartLoadingWait();
                
                done = true;
            });
            while (!done) { }
            done = false;
            if (cont == 0)
                filesOnFolder = Directory.GetFiles(inputStraightGal.text);
            if (cont == 1)
                filesOnFolder = Directory.GetFiles(inputDickgirlGal.text);
            UnityThread.executeInUpdate(() =>
            {
                loadingWait = false;
                LoadingReset("Searching in the folder images of the desired character(s).");
                done = true;
            });
            while (!done) { }
            done = false;
            contImages = 0;
            for(int i = 0; i < filesOnFolder.Length; i++)
            {
                UnityThread.executeInUpdate(() =>
                {
                    UpdateLoadingValue(contImages / filesOnFolder.Length, "Searching in the folder images of the desired character(s).\nAdded: " + added);
                });
                for(int j = 0; j < urlsCharacterToShow.Count; j++)
                {
                    if (filesOnFolder[i].Contains(urlsCharacterToShow[j]))
                    {
                        filesToShow.Add(filesOnFolder[i]);
                        added += 1;
                    }
                }
            }
            cont += 1;
        }
        UnityThread.executeInUpdate(() =>
        {
            LoadingReset("Loading the images from the filter folder...");
            StartLoadingWait();
        });
        string[] filesInFilterFolder = Directory.GetFiles(inputFilteredFolder.text);
        
        //Delete unfitting images
        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Deleting unfitting images from the filtered folder.");
        });
        contImages = 0;
        foreach (string s in filesInFilterFolder)
        {
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(contImages / filesToShow.Count);
            });
            bool delete = true;
            for (int i = 0; i < filesToShow.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(filesToShow[i]) == Path.GetFileNameWithoutExtension(s))
                {
                    delete = false;
                }
            }
            if(delete) File.Delete(s);
            contImages++;
        }

        //Add new images
        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Copying new images to the filter folder.");
        });
        contImages = 0;
        foreach (string s in filesToShow)
        {
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(contImages/filesToShow.Count);
                
            });
            if (!File.Exists(inputFilteredFolder.text + @"\" + Path.GetFileName(s)))
            {
                File.Copy(s, inputFilteredFolder.text + @"\" + Path.GetFileName(s), true);
            }
            contImages++;
        }

        UnityThread.executeInUpdate(() =>
        {
            loadingComp.obj.SetActive(false);
            CreateAdvice("Finished!");
            Application.OpenURL(inputFilteredFolder.text);
            done = true;
        });
        while (!done) { }


    }

}
