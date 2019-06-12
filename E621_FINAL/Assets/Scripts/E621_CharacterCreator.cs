using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class E621_CharacterCreator : GlobalActions
{

    public new static E621_CharacterCreator act;

    public enum Mode { Character, Artist }
    public Mode mode = Mode.Character;

    [Header("Global")]
    public InputField inputPortraits;
    public InputField inputSources, inputStraightGal, inputDickgirlGal;
    public Button buttonSave, buttonCreate, buttonPrev, buttonNext, buttonFirst, buttonLast;
    public Sprite imgError, imgMissing, imgLoading, imgBlank;
    public Sprite sprFavorite, sprFetish, sprMature, sprBeast, sprDickgirl, sprEgyptian, sprSizeDiff, sprDemon, sprMuscular;
    public GameObject prefabButtonChar, objPreview;
    public int indexPreviewFull, indexPreviewIco;
    public RectTransform transformViewer;
    public GameObject objMainUI, objCreatorEditor;
    public InputField inputFilter;
    public Dropdown dropFilter;
    public Toggle toggleFilter;

    [SerializeField]
    List<E621CharacterData> listShowableChars = new List<E621CharacterData>();
    public int imagesPerPage = 10;
    [SerializeField]
    int currentPage = 0;

    [Header("Creator")]
    public Slider sliderScale;
    public Slider sliderOffsetX, sliderOffsetY;
    public InputField inputMaxOffsetX, inputMaxOffsetY, inputMaxScale;
    public Image imageCreatorPortrait;
    public Text textCreatorName;
    public Dropdown dropCreatorImageSource;
    public InputField inputCreatorTag, inputCreatorName, inputCreatorSourceFile, inputCreatorPortraitFile, inputCreatorTagHighlights, inputCreatorSpecial;
    public Button buttonDelete;

    string[] filesOnSource;
    string[] filesOnPortrait;
    bool editing = false;
    E621CharacterData newCharData;
    int loadedDataID = -1;

    bool canUse = false;
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        act = this;
    }

    private void Start()
    {
        SetSourcesToDefault();
        ButtonApplyConfig();
        FilterInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        buttonDelete.interactable = editing;

        inputFilter.interactable = canUse && !toggleFilter.isOn;
        dropFilter.interactable = canUse;
        toggleFilter.interactable = canUse;
    }

    public void SetSourcesToDefault()
    {
        inputStraightGal.text = PlayerPrefs.GetString("E621_StraightMainGal");
        inputDickgirlGal.text = PlayerPrefs.GetString("E621_DickgirlMainGal");
        if (mode == Mode.Character)
        {
            inputSources.text = PlayerPrefs.GetString("E621_CharUnedited");
            inputPortraits.text = PlayerPrefs.GetString("E621_CharPortraits");
        }
        else if(mode == Mode.Artist)
        {
            inputSources.text = PlayerPrefs.GetString("E621_ArtistUnedited");
            inputPortraits.text = PlayerPrefs.GetString("E621_ArtistPortraits");
        }
        
    }

    #region Button Handlers
    public void ButtonApplyConfig()
    {
        string configApplyMessage = "";
        if (!Directory.Exists(inputStraightGal.text))
        {
            configApplyMessage += "\n Straight Gallery URL";
        }
        if (!Directory.Exists(inputDickgirlGal.text))
        {
            configApplyMessage += "\n Dickgirl Gallery URL";
        }
        if (!Directory.Exists(inputPortraits.text))
        {
            configApplyMessage += "\n Portraits URL";
        }
        if (!Directory.Exists(inputSources.text))
        {
            configApplyMessage += "\n Images (Unedited) URL";
        }

        if (configApplyMessage != "")
        {
            CreateAdvice("Error in the Urls:" + configApplyMessage + "\n Everything must be Correct to use this section.", 1);
            inputStraightGal.text = "";
            inputDickgirlGal.text = "";
            inputSources.text = "";
            inputPortraits.text = "";
        }
        else
        {
            PlayerPrefs.SetString("E621_StraightMainGal", inputStraightGal.text);
            PlayerPrefs.SetString("E621_DickgirlMainGal", inputDickgirlGal.text);

            if (mode == Mode.Character)
            {
                PlayerPrefs.SetString("E621_CharPortraits", inputPortraits.text);
                PlayerPrefs.SetString("E621_CharUnedited", inputSources.text);
            }
            else if (mode == Mode.Artist)
            {
                PlayerPrefs.SetString("E621_ArtistPortraits", inputPortraits.text);
                PlayerPrefs.SetString("E621_ArtistUnedited", inputSources.text);
            }
            

            filesOnSource = Directory.GetFiles(inputSources.text);
            filesOnPortrait = Directory.GetFiles(inputPortraits.text);
            
            canUse = true;
            GeneratePages();
        }
    }

    public void ButtonSave()
    {
        string message = "null";

        if (mode == Mode.Character)
            message = "Character";
        else if (mode == Mode.Artist)
            message = "Artist";

        CreateAdvice("Would you like to Overwrite the " + message +  ".DATA?", 0, () =>
        {
            if(mode == Mode.Character)
                Data.act.SaveData("e621CharacterData");
            else if(mode == Mode.Artist)
                Data.act.SaveData("e621ArtistData");
        });
    }

    public void ButtonAction(string value)
    {
        switch (value)
        {
            //Navigation
            case "next":
                currentPage++;
                if ((currentPage + 1) * imagesPerPage >= listShowableChars.Count)
                {
                    buttonNext.interactable = false;
                    buttonPrev.interactable = true;
                    buttonLast.interactable = false;
                    buttonFirst.interactable = true;
                }
                else
                {
                    buttonFirst.interactable = true;
                    buttonPrev.interactable = true;
                    buttonNext.interactable = true;
                    buttonLast.interactable = true;
                }
                ShowPage();
                break;
            case "prev":
                currentPage--;
                if (currentPage - 1 == -1)
                {
                    buttonNext.interactable = true;
                    buttonPrev.interactable = false;
                    buttonLast.interactable = true;
                    buttonFirst.interactable = false;
                }
                else
                {
                    buttonNext.interactable = true;
                    buttonPrev.interactable = true;
                    buttonLast.interactable = true;
                    buttonFirst.interactable = true;
                }
                ShowPage();
                break;
            case "first":
                currentPage = 0;
                buttonNext.interactable = true;
                buttonPrev.interactable = false;
                buttonLast.interactable = true;
                buttonFirst.interactable = false;
                ShowPage();
                break;
            case "last":
                currentPage = listShowableChars.Count / imagesPerPage;
                buttonNext.interactable = false;
                buttonPrev.interactable = true;
                buttonLast.interactable = false;
                buttonFirst.interactable = true;
                ShowPage();
                break;
        }
    }

    #endregion

    #region Page Handlers
    void GeneratePages()
    {
        if (!canUse) return;
        print("gen");
        listShowableChars.Clear();

        if (mode == Mode.Character)
        {
            foreach (E621CharacterData d in Data.act.e621CharacterData)
            {
                if (dropFilter.value == 0)
                {
                    listShowableChars.Add(d);
                    continue;
                }

                string validated = (d.tagHighlights + " " + d.special).ToLower();
                validated = validated.Replace("_", " ");

                if (validated.Contains(dropFilter.options[dropFilter.value].text.ToLower()))
                {
                    listShowableChars.Add(d);
                }

            }
        }
        else if (mode == Mode.Artist)
        {
            foreach (E621CharacterData d in Data.act.e621ArtistData)
            {
                if (dropFilter.value == 0)
                {
                    listShowableChars.Add(d);
                    continue;
                }

                string validated = (d.tagHighlights + " " + d.special).ToLower();
                validated = validated.Replace("_", " ");

                if (validated.Contains(dropFilter.options[dropFilter.value].text.ToLower()))
                {
                    listShowableChars.Add(d);
                }

            }
        }


        ButtonAction("first");
    }

    void ShowPage()
    {
        if (!canUse) return;
        ClearGridChilds();
        Resources.UnloadUnusedAssets();
        
        float contDelay = 0f;
        for (int i = 0; i < imagesPerPage; i++)
        {
            int correctID = i + (imagesPerPage * currentPage);
            if (correctID == listShowableChars.Count) break;
            E621_CharacterCreatorButton b = Instantiate(prefabButtonChar, transformViewer).GetComponent<E621_CharacterCreatorButton>();
            b.data = listShowableChars[correctID];
            b.delay = contDelay;
            b.LoadImageFunc();
            contDelay += 0.001f;
        }
    }

    void ClearGridChilds()
    {
        int childCount = transformViewer.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(transformViewer.transform.GetChild(i).gameObject);
        }
    }

    #endregion
    
    #region Creator Functions

    public void CreatorNew()
    {
        newCharData = new E621CharacterData();
        loadedDataID = -1;

        CreatorDropImageSourceReset();
        CreatorResetPortraitToDefault();

        dropCreatorImageSource.interactable = true;
        inputCreatorSourceFile.interactable = false;
        inputCreatorPortraitFile.interactable = true;

        editing = false;
    }

    public void CreatorExistent(E621CharacterData data)
    {
        objMainUI.SetActive(false);
        objCreatorEditor.SetActive(true);

        newCharData = data;
        if (mode == Mode.Character)
            loadedDataID = Data.act.e621CharacterData.IndexOf(data);
        else if(mode == Mode.Artist)
            loadedDataID = Data.act.e621ArtistData.IndexOf(data);

        if (loadedDataID == -1) CreateAdvice("Error getting the char ID, ABORT!");

        dropCreatorImageSource.ClearOptions();
        CreatorResetPortraitToDefault();
        inputCreatorTag.text = data.tag;
        inputCreatorName.text = data.name;
        inputCreatorSourceFile.text = data.sourceFile;
        inputCreatorPortraitFile.text = data.portraitFile;
        inputCreatorTagHighlights.text = data.tagHighlights;
        inputCreatorSpecial.text = data.special;
        string url = inputSources.text + @"\" + data.sourceFile;
        LoadImage(imgLoading, imgError, imageCreatorPortrait, url);

        dropCreatorImageSource.interactable = false;
        inputCreatorSourceFile.interactable = false;
        inputCreatorPortraitFile.interactable = false;

        editing = true;
    }

    public void CreatorDropImageSource(int val)
    {
        string url = inputSources.text + @"\" + dropCreatorImageSource.options[val].text;
        LoadImage(imgLoading, imgError, imageCreatorPortrait, url);
        inputCreatorTag.text = dropCreatorImageSource.options[val].text;
        inputCreatorTag.text = inputCreatorTag.text.Replace(".png", "");
        inputCreatorTag.text = inputCreatorTag.text.Replace(".jpg", "");
        inputCreatorTag.text = inputCreatorTag.text.Replace(" ", "_");
        inputCreatorName.text = inputCreatorTag.text.Replace("_", " ");
        inputCreatorSourceFile.text = dropCreatorImageSource.options[val].text;
        inputCreatorPortraitFile.text = inputCreatorTag.text;
        inputCreatorTagHighlights.text = "";
        inputCreatorSpecial.text = "";
    }

    void CreatorDropImageSourceReset()
    {
        dropCreatorImageSource.ClearOptions();
        List<string> newOp = new List<string>();
        foreach(string s in filesOnSource)
        {
            string file = Path.GetFileName(s);
            E621CharacterData dat = null;
            if(mode == Mode.Character)
                dat = Data.act.e621CharacterData.Where(temp => temp.sourceFile == file).SingleOrDefault();
            else if(mode == Mode.Artist)
                dat = Data.act.e621ArtistData.Where(temp => temp.sourceFile == file).SingleOrDefault();
            if (dat == null)
            {
                newOp.Add(file);
            }
        }
        dropCreatorImageSource.AddOptions(newOp);
        CreatorDropImageSource(0);
    }

    public void CreatorButtonOpenInPage()
    {
        Application.OpenURL("https://e621.net/post/index/1/" + inputCreatorTag.text);
    }

    public void CreatorButtonConfirm()
    {
        newCharData.SetData(inputCreatorTag.text, inputCreatorName.text, inputCreatorSourceFile.text, inputCreatorPortraitFile.text, inputCreatorTagHighlights.text, inputCreatorSpecial.text);
        newCharData.SetPortraitData(float.Parse(inputMaxScale.text), sliderScale.value, float.Parse(inputMaxOffsetX.text), sliderOffsetX.value, float.Parse(inputMaxOffsetY.text), sliderOffsetY.value);
        ScreenshotHandler.act.TakeScreenshot(newCharData.portraitFile, inputPortraits.text);
        if(mode == Mode.Character)
        {
            if (!editing)
            {
                Data.act.e621CharacterData.Add(newCharData);

                //---<
                if (Data.act.e621CharacterData.Count > 1)
                    Data.act.e621CharacterData.Sort((v1, v2) => v1.sourceFile.CompareTo(v2.sourceFile));
                //---<

                OpenSceneAsync("character");
                //ButtonApplyConfig();
                print("Added new char data");
            }
            else
            {
                Data.act.e621CharacterData[loadedDataID] = newCharData;
                print("Edited char data");
            }
        }
        else if(mode == Mode.Artist)
        {
            if (!editing)
            {
                Data.act.e621ArtistData.Add(newCharData);

                //---<
                if (Data.act.e621ArtistData.Count > 1)
                    Data.act.e621ArtistData.Sort((v1, v2) => v1.sourceFile.CompareTo(v2.sourceFile));
                //---<

                OpenSceneAsync("artist");
                //ButtonApplyConfig();
                print("Added new artist data");
            }
            else
            {
                Data.act.e621ArtistData[loadedDataID] = newCharData;
                print("Edited artist data");
            }
        }
        
    }

    public void CreatorButtonDelete()
    {
        CreateAdvice("Warning!", "Deleting character data also deletes the image in the 'Source' Images folder.\n\nContinue?", 0, () =>
        {
            if (mode == Mode.Character)
            {
                File.Delete(inputSources.text + @"\" + Data.act.e621CharacterData[loadedDataID].sourceFile);
                File.Delete(inputPortraits.text + @"\" + Data.act.e621CharacterData[loadedDataID].portraitFile + ".png");

                Data.act.e621CharacterData.RemoveAt(loadedDataID);
                OpenSceneAsync("character");
            }
            else if(mode == Mode.Artist)
            {
                File.Delete(inputSources.text + @"\" + Data.act.e621ArtistData[loadedDataID].sourceFile);
                File.Delete(inputPortraits.text + @"\" + Data.act.e621ArtistData[loadedDataID].portraitFile + ".png");

                Data.act.e621ArtistData.RemoveAt(loadedDataID);
                OpenSceneAsync("artist");
            }
            
        });
    }

    //-------------------------------------------------
    //Portrait Edit
    public void CreatorPortraitName(string s)
    {
        textCreatorName.text = s;
    }

    public void CreatorResetPortraitToDefault()
    {
        inputMaxScale.text = newCharData.pMaxScale.ToString();
        CreatorInputMaxScale(inputMaxScale.text);
        sliderScale.value = newCharData.pScale;
        
        inputMaxOffsetX.text = newCharData.pMaxOffX.ToString();
        CreatorInputMaxOffsetX(inputMaxOffsetX.text);
        sliderOffsetX.value = newCharData.pOffX;

        inputMaxOffsetY.text = newCharData.pMaxOffX.ToString();
        CreatorInputMaxOffsetY(inputMaxOffsetY.text);
        sliderOffsetY.value = newCharData.pOffY;
    }

    public void CreatorInputMaxScale(string value)
    {
        float val = float.Parse(value);
        sliderScale.minValue = 0.1f;
        sliderScale.maxValue = val;
    }

    public void CreatorInputMaxOffsetX(string value)
    {
        float val = float.Parse(value);
        sliderOffsetX.minValue = -val;
        sliderOffsetX.maxValue = val;
    }

    public void CreatorInputMaxOffsetY(string value)
    {
        float val = float.Parse(value);
        sliderOffsetY.minValue = -val;
        sliderOffsetY.maxValue = val;
    }

    public void CreatorSliderScale(float value)
    {
        imageCreatorPortrait.rectTransform.localScale = new Vector3(value, value, value);
    }

    public void CreatorSliderOffsetX(float value)
    {
        Vector3 newPos = imageCreatorPortrait.rectTransform.localPosition;
        newPos.x = value;
        imageCreatorPortrait.rectTransform.localPosition = newPos;
    }

    public void CreatorSliderOffsetY(float value)
    {
        Vector3 newPos = imageCreatorPortrait.rectTransform.localPosition;
        newPos.y = value;
        imageCreatorPortrait.rectTransform.localPosition = newPos;
    }

    //-------------------------------------------------
    public void Screenshot()
    {
        ScreenshotHandler.act.TakeScreenshot("jeje", @"D:\HardDrive\No pls\e621");
    }

    #endregion

    #region Filterers
    public void FilterInitialize()
    {
        dropFilter.ClearOptions();
        if(Data.act.e621CharacterFilterers.Count > 1) Data.act.e621CharacterFilterers.Sort();
        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();
        newOptions.Add(new Dropdown.OptionData("No Filter"));
        foreach(string s in Data.act.e621CharacterFilterers)
        {
            //Icon Compatibility
            Sprite sp = null;
            switch (s.ToLower())
            {
                case "favorite":
                    sp = sprFavorite;
                    break;
                case "bestiality":
                    sp = sprBeast;
                    break;
                case "dickgirl":
                    sp = sprDickgirl;
                    break;
                case "mature":
                    sp = sprMature;
                    break;
                case "muscular":
                    sp = sprMuscular;
                    break;
                case "difference":
                    sp = sprSizeDiff;
                    break;
                case "fetish":
                    sp = sprFetish;
                    break;
                case "egyptian":
                    sp = sprEgyptian;
                    break;
                case "demon":
                    sp = sprDemon;
                    break;
            }
            newOptions.Add(new Dropdown.OptionData(s, sp));
        }
        dropFilter.AddOptions(newOptions);
        
        dropFilter.value = 0;
        dropFilter.RefreshShownValue();
    }

    public void DropFilter(int val)
    {
        if (toggleFilter.isOn && val != 0)
        {
            CreateAdvice("Are you sure you want to delete these filter?\n\n" + dropFilter.options[val].text, 0, () =>
            {
                Data.act.e621CharacterFilterers.Remove(dropFilter.options[val].text);
                Data.act.SaveData("e621CharacterFilter");
                FilterInitialize();
                GeneratePages();
            }, () =>
            {
                dropFilter.value = 0;
                dropFilter.RefreshShownValue();
                GeneratePages();
            });
            return;
        }
        GeneratePages();
    }

    public void ToggleFilter(bool val)
    {
        if (dropFilter.value == 0) return;
        inputFilter.interactable = !val;
    }

    public void InputFilter(string s)
    {
        inputFilter.text = "";
        if (s == "" || s == " ") return;
        if (Data.act.e621CharacterFilterers.Contains(s))
        {
            CreateAdvice("Filter already exists.");
            return;
        }
        Data.act.e621CharacterFilterers.Add(s);
        Data.act.SaveData("e621CharacterFilter");

        FilterInitialize();
    }

    #endregion

}
