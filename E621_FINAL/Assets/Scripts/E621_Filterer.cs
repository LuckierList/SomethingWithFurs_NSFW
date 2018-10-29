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

    Button[] navigationButtons;

    public InputField inputStraight, inputDickgirl, inputIntersex, inputHerm;
    public Dropdown dropSource, dropFormat;
    public Button buttonKeep, buttonFilter, buttonNext, buttonPrev, buttonOpenInPage, buttonMoveToFolder;

    public Sprite imgBlank;
    public Image imageShow;

    int currentIndex = 0;
    [HideInInspector]
    public List<string> files;
    string sourceURL;

    bool[] canUseMode;
    bool navigation;
    bool video;

    private void Start()
    {
        canUseMode = new bool[4];
        files = new List<string>();
        DefaultURLs();
        DropSource(0);
    }

    private void Update()
    {
        NavigationButtonsSet();
    }

    void NavigationButtonsSet()
    {
        foreach(Button b in navigationButtons)
        {
            b.interactable = navigation;
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
    
    public void DropSource(int index)
    {
        currentIndex = 0;
        sourceURL = "";
        switch (index)
        {
            case 0:
                navigation = false;
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

            loadingComp.obj.SetActive(false);
            done = true;

        });
        while (!done) { }
    }

    public void OnButtonClick(string action)
    {
        switch (action)
        {
            case "configApply":
                ValidateURLs(true);
                break;
            case "configCancel":
                DefaultURLs();
                break;
            case "return":
                SceneManager.LoadSceneAsync("E621_MainMenu");
                break;
        }
    }
}
