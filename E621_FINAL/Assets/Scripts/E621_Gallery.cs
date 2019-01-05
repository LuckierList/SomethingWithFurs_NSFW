using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Threading;
using System.IO;

public class E621_Gallery : GlobalActions
{

    public int[] buttonSizesValue, buttonSizesQuantity;
    [HideInInspector]
    public string[] files1;
    [HideInInspector]
    public string[] files2;
    [HideInInspector]
    public List<string> showFiles;
    [HideInInspector]
    public List<string> showFilesFilter;
    public int currentPage;
    public int imagesPerPage;
    public GameObject objGridParent, prefabButtonGal;
    public GridLayoutGroup gridButtons;

    public Button buttonNext, buttonPrev, buttonLast, buttonFirst;

    public InputField inputFilter;
    public Dropdown dropFilter;
    public Toggle toggleFilter, toggleIncludeStraight, toggleIncludeDickgirl;
    public Button buttonFilter, buttonOpenFilterFolder;
    bool useFilter = false, canUseFilterFolder = false;
    public InputField inputStraightGal, inputDickgirlGal, inputFilterFolder;


    // Use this for initialization
    public override void Awake()
    {
        base.Awake();
        SetSourcesToDefault();
    }

    private void Start()
    {
        ButtonSizeManager(0, false);
        ButtonAction("configApply");
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void SetSourcesToDefault()
    {
        inputStraightGal.text = PlayerPrefs.GetString("E621_StraightMainGal");
        inputDickgirlGal.text = PlayerPrefs.GetString("E621_DickgirlMainGal");
        inputFilterFolder.text = PlayerPrefs.GetString("E621_GalleryFilterFolder");
    }

    //----------------------------------------------------
    //Main UI 
    void SetNavigationButtons(bool next, bool prev, bool last, bool first)
    {
        buttonNext.interactable = next;
        buttonPrev.interactable = prev;
        buttonLast.interactable = last;
        buttonFirst.interactable = first;
    }

    void ClearGridChilds()
    {
        int childCount = objGridParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            objGridParent.transform.GetChild(i).GetComponent<E621_GalleryButton>().StopThisCoroutine();
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
        if (show)
        {
            SetNavigationButtons(true, false, true, false);
            currentPage = 0;
            ShowPage(0);
        }
    }
    //----------------------------------------------------
    //Filterers
    public void InputFilterAdd(string value)
    {
        value = value.ToLower();
        value.Replace(' ', '_');
        if (value == "" || value == "none") return;
        
        List<string> newOptions = new List<string>();

        foreach(Dropdown.OptionData dat in dropFilter.options)
        {
            //if (dat.text == "None") continue;
            //if (dat.text == value) continue;
            newOptions.Add(dat.text);
        }
        newOptions.Add(value);
        dropFilter.ClearOptions();
        dropFilter.AddOptions(newOptions);
        inputFilter.text = "";
    }

    public void DropFilterDelete(int value)
    {
        if (dropFilter.options[value].text == "None")
        {
            return;
        }
        dropFilter.options.RemoveAt(value);
        dropFilter.value = 0;
        dropFilter.RefreshShownValue();
    }


    //----------------------------------------------------

    public void ButtonAction(string value)
    {
        int showFilesValue = 0;
        if (useFilter)
            showFilesValue = showFilesFilter.Count;
        else
            showFilesValue = showFiles.Count;
        switch (value)
        {
            case "filter":
                FilterPages();
                break;
            case "openFiltered":
                CreateAdvice("You are about to move '" + (showFilesFilter.Count - 1) + "' images. Remember this may take a while if you copy/delete a lot of images.\n Continue}?", 0, () =>
                {
                    Thread t = new Thread(new ThreadStart(ShowImagesInFolderThread));
                    t.Start();
                });
                break;
            //Navigation
            case "mainMenu":
                ClearGridChilds();
                OpenSceneAsync("mainMenu");
                break;
            case "next":
                currentPage++;
                if ((currentPage + 1) * imagesPerPage >= showFilesValue)
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
                    SetNavigationButtons(true, false, true, false);
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
                currentPage = showFilesValue / imagesPerPage;
                SetNavigationButtons(false, true, false, true);
                ShowPage(currentPage);
                break;
            case "configApply":
                string configApplyMessage = "";
                if(!Directory.Exists(inputStraightGal.text))
                {
                    configApplyMessage += "\n Straight Gallery URL";
                }
                if (!Directory.Exists(inputDickgirlGal.text))
                {
                    configApplyMessage += "\n Dickgirl Gallery URL";
                }
                if(configApplyMessage != "")
                {
                    CreateAdvice("Error in the Urls: " + "\n Both URLs need to be correct.");
                    inputStraightGal.text = "";
                    inputDickgirlGal.text = "";
                    ClearGridChilds();
                }
                else
                {
                    PlayerPrefs.SetString("E621_StraightMainGal", inputStraightGal.text);
                    PlayerPrefs.SetString("E621_DickgirlMainGal", inputDickgirlGal.text);
                    
                    LoadingReset("Getting the files list from Straight Gallery. May take a while... a bit too much I'd say.");
                    StartLoadingWait();
                    Thread t2 = new Thread(new ThreadStart(GetTheImageFilesThread));
                    t2.Start();
                }

                if(Directory.Exists(inputFilterFolder.text))
                {
                    PlayerPrefs.SetString("E621_GalleryFilterFolder", inputFilterFolder.text);
                    canUseFilterFolder = true;
                }
                else
                {
                   
                    canUseFilterFolder = false;
                    inputFilterFolder.text = "";
                }
                if (useFilter)
                    buttonOpenFilterFolder.interactable = canUseFilterFolder;
                else
                    buttonOpenFilterFolder.interactable = false;
                break;
        }
    }

    void GetTheImageFilesThread()
    {
        files1 = Directory.GetFiles(inputStraightGal.text);
        
        UnityThread.executeInUpdate(() =>
        {
            LoadingReset("Getting the files list from Dickgirl Gallery. May take a while... a bit too much I'd say.");
            StartLoadingWait();
        });

        files2 = Directory.GetFiles(inputDickgirlGal.text);

        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Combining the files from Straight and Dickgirl.");
            
        });
        showFiles.Clear();
        int cont = 0;
        foreach (string s in files1)
        {
            showFiles.Add(s);
            cont++;
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(cont/(files1.Length + files2.Length));
            });
        }

        foreach (string s in files2)
        {
            showFiles.Add(s);
            cont++;
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(cont / (files1.Length + files2.Length));
            });
        }

        UnityThread.executeInUpdate(() =>
        {
            loadingComp.obj.SetActive(false);
            currentPage = 0;
            ShowPage(0);
        });
    }

    public void ShowPage(int page)
    {
        ClearGridChilds();
        Resources.UnloadUnusedAssets();
        //print("SHow page");
        /*
        E621_CharacterButton b = prefabCharButton.GetComponent<E621_CharacterButton>();
        */
        float contDelay = 0f;
        E621_GalleryButton b = prefabButtonGal.GetComponent<E621_GalleryButton>();

        for (int i = 0; i < imagesPerPage; i++)
        {
            int correctID = i + (imagesPerPage * currentPage);
            //print("for");

            if(!useFilter)
            {
                if (correctID >= showFiles.Count) break;

                b.textName.text = Path.GetFileNameWithoutExtension(showFiles[correctID]);


                b.url = showFiles[correctID];
                b.delay = contDelay;
            }
            else
            {
                if (correctID >= showFilesFilter.Count) break;

                b.textName.text = Path.GetFileNameWithoutExtension(showFilesFilter[correctID]);

                b.url = showFilesFilter[correctID];
                b.delay = contDelay;
            }
            
            Instantiate(prefabButtonGal, objGridParent.transform);
            
            contDelay += 0.001f;
        }
    }

    void FilterPages()
    {
        showFilesFilter.Clear();
        foreach (ImageData data in Data.act.imageData)
        {
            bool canAdd = false;

            if (!toggleIncludeDickgirl.isOn && (data.tags.Contains("dickgirl") || data.tags.Contains("intersex") || data.tags.Contains("herm")))
                continue;
            if (!toggleIncludeStraight.isOn && !(data.tags.Contains("dickgirl") || data.tags.Contains("intersex") || data.tags.Contains("herm")))
                continue;

            int contador = 0;
            for (int i = 0; i < dropFilter.options.Count; i++)
            {
                if (data.tags.Contains(dropFilter.options[i].text))
                {
                    if (!toggleFilter.isOn)
                    {
                        canAdd = true;
                        break;
                    }
                    else
                    {
                        contador++;
                        if(contador == dropFilter.options.Count - 1)
                        {
                            canAdd = true;
                        }
                    }
                }
            }

            if (canAdd)
            {
                string url = "";

                if(File.Exists(inputStraightGal.text + @"\" + data.filename))
                {
                    url = inputStraightGal.text + @"\" + data.filename;
                }
                else if(File.Exists(inputDickgirlGal.text + @"\" + data.filename))
                {
                    url = inputDickgirlGal.text + @"\" + data.filename;
                }
                if(url != "")
                showFilesFilter.Add(url);
            }

        }

        useFilter = true;
        if (showFilesFilter.Count == 0)
        {
            CreateAdvice("No images exist, filter skipped!");
            useFilter = false;
        }

        if(canUseFilterFolder && useFilter)
        {
            buttonOpenFilterFolder.interactable = true;
        }
        else
        {
            buttonOpenFilterFolder.interactable = false;
        }
        currentPage = 0;
        ShowPage(0);
    }

    void ShowImagesInFolderThread()
    {
        bool done = false;
        float contImages;
        UnityThread.executeInUpdate(() =>
        {
            LoadingReset("Loading the images from the filter folder...");
            StartLoadingWait();
        });
        string[] filesInFilterFolder = Directory.GetFiles(inputFilterFolder.text);

        //Delete unfitting images
        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Deleting unfitting images from the filtered folder.");
            done = true;
        });
        while (!done) { }
        done = false;

        contImages = 0;
        foreach (string s in filesInFilterFolder)
        {
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(contImages / showFilesFilter.Count);
            });
            bool delete = true;
            for (int i = 0; i < showFilesFilter.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(showFilesFilter[i]) == Path.GetFileNameWithoutExtension(s))
                {
                    delete = false;
                }
            }
            if (delete) File.Delete(s);
            contImages++;
        }

        UnityThread.executeInUpdate(() =>
        {
            contImages = 0;
            LoadingReset("Copying new images to the filter folder.");
            done = true;
        });
        while (!done) { }
        done = false;
        foreach (string s in showFilesFilter)
        {
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(contImages / showFilesFilter.Count);
            });
            if (!File.Exists(inputFilterFolder.text + @"\" + Path.GetFileName(s)))
            {
                File.Copy(s, inputFilterFolder.text + @"\" + Path.GetFileName(s), true);
            }
            contImages++;
        }

        UnityThread.executeInUpdate(() =>
        {
            loadingComp.obj.SetActive(false);
            CreateAdvice("Finished! Remember that you need to press the filter button again to repeat this!");
            buttonOpenFilterFolder.interactable = false;
            Application.OpenURL(inputFilterFolder.text);
            done = true;
        });
        while (!done) { }


    }


}
