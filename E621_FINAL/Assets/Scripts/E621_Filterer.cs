using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.IO;
using System.Threading;

public class E621_Filterer : GlobalActions
{
    List<Button> navigationButtons;
    List<Button> filteringButtons;

    public InputField inputStraight, inputDickgirl, inputIntersex, inputHerm;
    public Dropdown dropSource, dropFormat;
    public Button buttonKeep, buttonFilter, buttonDeleteData, buttonNext, buttonPrev, buttonOpenInPage, buttonMoveToFolder, buttonSave;

    public Sprite imgBlank, imgLoading, imgError;
    public Image imageShow;

    public Text textCurrentlyShowing, textFilteredComparation, textFilteredPercent;

    int currentIndex = 0;
    [HideInInspector]
    public List<string> files;
    string sourceURL;

    bool[] canUseMode;
    bool navigation;
    bool video;

    private void Start()
    {
        navigationButtons = new List<Button>();
        //navigationButtons.Add();
        navigationButtons.Add(buttonNext);
        navigationButtons.Add(buttonPrev);
        navigationButtons.Add(buttonOpenInPage);
        navigationButtons.Add(buttonMoveToFolder);
        //navigationButtons.Add(buttonSave);

        filteringButtons = new List<Button>();
        //filteringButtons.Add();
        filteringButtons.Add(buttonFilter);
        filteringButtons.Add(buttonKeep);
        filteringButtons.Add(buttonDeleteData);

        canUseMode = new bool[4];
        files = new List<string>();
        DefaultURLs();
        DropSource(0);
    }

    private void Update()
    {
        StatusTextUpdater();
        NavigationButtonsSet();
        KeyboardMovement();
    }
    //---------------------------------------------------------------------------
    //Update Functions
    void NavigationButtonsSet()
    {
        foreach(Button b in navigationButtons)
        {
            b.interactable = navigation;
        }
    }

    void KeyboardMovement()
    {
        if (!navigation) return;
        if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.R))
        {
            OnButtonClick("next");
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.E))
        {
            OnButtonClick("prev");
        }
        if (Input.GetKeyDown(KeyCode.Q) && buttonKeep.interactable)
        {
            OnButtonClick("keep");
        }
        if (Input.GetKeyDown(KeyCode.W) && buttonFilter.interactable)
        {
            OnButtonClick("filter");
        }

    }

    void StatusTextUpdater()
    {
        if (files.Count == 0) textCurrentlyShowing.text = "0 / 0";
        else textCurrentlyShowing.text = "" + (currentIndex) + " / " + (files.Count - 1);
        float filtered = 0f;
        foreach(ImageData img in Data.act.imageData)
        {
            if (img.filtered) filtered++;
        }
        float percentage = Mathf.Round((filtered / Data.act.imageData.Count) * 10000f) / 100f;
        textFilteredComparation.text = "" + filtered + " / " + Data.act.imageData.Count;
        textFilteredPercent.text = "" + percentage + "%";

    }
    //---------------------------------------------------------------------------
    void FilteringButtonsSet(bool value)
    {
        foreach(Button b in filteringButtons)
        {
            b.interactable = value;
        }
    }



    public void DefaultURLs()
    {
        inputStraight.text = PlayerPrefs.GetString("FilterStraigth");
        inputDickgirl.text = PlayerPrefs.GetString("FilterDickgirl");
        inputIntersex.text = PlayerPrefs.GetString("FilterIntersex");
        inputHerm.text = PlayerPrefs.GetString("FilterHerm");
        ValidateURLs(false);
    }

    public void ValidateURLs(bool advice)
    {
        canUseMode[0] = (Directory.Exists(inputStraight.text)) ? true : false;
        canUseMode[1] = (Directory.Exists(inputDickgirl.text)) ? true : false;
        canUseMode[2] = (Directory.Exists(inputIntersex.text)) ? true : false;
        canUseMode[3] = (Directory.Exists(inputHerm.text)) ? true : false;

        if (advice)
        {
            string log = "Available Options:\n";
            int logInt = 0;
            if (canUseMode[0])
            {
                PlayerPrefs.SetString("FilterStraigth", inputStraight.text);
                log += "Straight\n";
                logInt++;
            }
            if (canUseMode[1])
            {
                PlayerPrefs.SetString("FilterDickgirl", inputDickgirl.text);
                log += "Dickgirl\n";
                logInt++;
            }
            if (canUseMode[2])
            {
                PlayerPrefs.SetString("FilterIntersex", inputIntersex.text);
                log += "Intersex\n";
                logInt++;
            }
            if (canUseMode[3])
            {
                PlayerPrefs.SetString("FilterHerm", inputHerm.text);
                log += "Herm\n";
                logInt++;
            }
            log += "Total: " + logInt + "/4 URLs exist and are ready to be used.";
            CreateAdvice(log,1);
        }
    }

    void DropSourceInvalid(string name)
    {
        CreateAdvice("'"+ name + "' url hasn't been assigned or it stopped existing. Open the configuration and fix the URL.");
        dropSource.value = 0;
        dropSource.RefreshShownValue();
    }
    
    //Drop Source Handler
    public void DropSource(int index)
    {
        currentIndex = 0;
        sourceURL = "";
        switch (index)
        {
            case 0:
                imageShow.sprite = imgBlank;
                LoadImageCancel();
                navigation = false;
                FilteringButtonsSet(false);
                imageShow.sprite = imgBlank;
                break;
            case 1:
                if (!canUseMode[0]) DropSourceInvalid("Straight");
                else sourceURL = inputStraight.text;
                break;
            case 2:
                if (!canUseMode[1]) DropSourceInvalid("Dickgirl");
                else sourceURL = inputDickgirl.text;
                break;
            case 3:
                if (!canUseMode[2]) DropSourceInvalid("Intersex");
                else sourceURL = inputIntersex.text;
                break;
            case 4:
                if (!canUseMode[3]) DropSourceInvalid("Herm");
                else sourceURL = inputHerm.text;
                break;
        }
        if (sourceURL == "") return;

        files.Clear();
        LoadingReset("Loading the file list!");
        StartLoadingWait();

        Thread t = new Thread(new ThreadStart(DropSourceThread));
        t.Start();
    }

    //DropSource Handler Thread
    void DropSourceThread()
    {
        bool done = false;
        float cont = 0f;
        string[] filesThread = Directory.GetFiles(sourceURL);
        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Creating the file List with the correct extensions!");
            done = true;
        });
        while (!done) { }
        done = false;
        foreach(string s in filesThread)
        {
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(cont / filesThread.Length);
                //done = true;
            });
            //while (!done) { }
            //done = false;
            bool canAdd = true;
            string extension = Path.GetExtension(s);

            switch (dropFormat.value)
            {
                case 0://IMG
                    if (extension == ".gif" || extension == ".webm" || extension == ".swf")
                    {
                        canAdd = false;
                    }
                    break;
                case 1://Webm
                    if (extension != ".webm")
                    {
                        canAdd = false;
                    }
                    break;
            }

            if (canAdd) files.Add(s);
            cont++;
        }
        UnityThread.executeInUpdate(() =>
        {
            LoadingReset("Just wait...");
            UpdateLoadingValue(1f);
            navigation = true;

            //AQUI NECESITA ACTUALIZARZE
            switch (dropFormat.value)
            {
                case 0://IMG
                    ShowImage();
                    break;
                case 1://Webm

                    break;
            }
            loadingComp.obj.SetActive(false);
            done = true;

        });
        while (!done) { }
    }

    //Show image at the preview Thumb....
    void ShowImage()
    {
        LoadImageCancel();
        DetermineFilterStatus();
        LoadImage(imgLoading, imgError, imageShow, files[currentIndex], true);
    }

    void DetermineFilterStatus()
    {
        ImageData temp = Data.act.imageData.Where(tempo => tempo.filename == Path.GetFileName(files[currentIndex])).SingleOrDefault();
        if (temp != null)
        {
            if (temp.filtered)
            {
                buttonKeep.interactable = true;
                buttonFilter.interactable = false;
            }
            else
            {
                buttonKeep.interactable = false;
                buttonFilter.interactable = true;
            }
            buttonDeleteData.interactable = true;
        }
        else
        {
            buttonKeep.interactable = true;
            buttonFilter.interactable = true;
            buttonDeleteData.interactable = false;
        }
    }

    //All button interactions
    public void OnButtonClick(string action)
    {
        //variables that filter/keep/deleteData handle
        string filename = "";
        ImageData oldData = null;
        ImageData newData = null;
        int newID = 0;
        string _newID = "";
        //----------------------------

        switch (action)
        {
            //Navigation
            case "next":
                currentIndex++;
                if (currentIndex >= files.Count) currentIndex = 0;
                switch (dropFormat.value)
                {
                    case 0://img
                        ShowImage();
                        break;
                    case 1://webm

                        break;
                }
                break;
            case "prev":
                currentIndex--;
                if (currentIndex < 0) currentIndex = files.Count-1;
                switch (dropFormat.value)
                {
                    case 0://img
                        ShowImage();
                        break;
                    case 1://webm

                        break;
                }
                break;
            case "openInPage":
                OpenInPage(files[currentIndex]);
                break;
            //Filtering
            case "filter":
                filename = Path.GetFileName(files[currentIndex]);
                oldData = Data.act.imageData.Where(data => data.filename == filename).SingleOrDefault();
                if(oldData != null)
                {
                    Data.act.imageData.Remove(oldData);
                    print("Detected old data and removed it");
                }
                newID = 0;
                if (filename.Contains("-"))
                {
                    _newID = filename.Substring(0, filename.IndexOf("-"));
                    newID = int.Parse(_newID);
                }

                newData = new ImageData("E621", newID, filename, true);
                Data.act.imageData.Add(newData);

                DetermineFilterStatus();
                break;
            case "keep":
                filename = Path.GetFileName(files[currentIndex]);
                oldData = Data.act.imageData.Where(data => data.filename == filename).SingleOrDefault();
                if (oldData != null)
                {
                    Data.act.imageData.Remove(oldData);
                    print("Detected old data and removed it");
                }
                newID = 0;
                if (filename.Contains("-"))
                {
                    _newID = filename.Substring(0, filename.IndexOf("-"));
                    newID = int.Parse(_newID);
                }

                newData = new ImageData("E621", newID, filename, false);
                Data.act.imageData.Add(newData);

                DetermineFilterStatus();
                break;
            case "deleteData":
                filename = Path.GetFileName(files[currentIndex]);
                oldData = Data.act.imageData.Where(data => data.filename == filename).SingleOrDefault();
                if (oldData != null)
                {
                    Data.act.imageData.Remove(oldData);
                    print("Detected old data and removed it");
                }
                DetermineFilterStatus();
                break;
            //Others
            case "save":
                CreateAdvice("Are you sure you want to Override ImageData.DATA?",0,
                    () =>
                    {
                        Data.act.SaveData("imageData");
                        CreateAdvice("'ImageData.DATA' succesfully Overwritten!");
                    });
                break;
            case "configApply":
                ValidateURLs(true);
                dropSource.value = 0;
                dropSource.RefreshShownValue();
                break;
            case "configCancel":
                DefaultURLs();
                break;
            case "return":
                LoadImageCancel();
                imageShow.sprite = imgBlank;
                Resources.UnloadUnusedAssets();
                SceneManager.LoadSceneAsync("E621_MainMenu");
                break;
            default:
                CreateAdvice("Warning!", "The selected button is not labeled right!");
                break;
        }
    }
}
