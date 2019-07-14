
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using System.Threading;

public class Data : MonoBehaviour
{

    #region New Data
    public List<FileData> fileData;
    public TagData tagData;
    #endregion

    public static Data act;
    [HideInInspector]
    public List<ImageData> imageData;
    [HideInInspector]
    public List<E621CharacterData> e621CharacterData;
    [HideInInspector]
    public List<E621CharacterData> e621ArtistData; //Used the same format because these two are practically identical in terms of properties;
    [HideInInspector]
    public List<string> e621SpecificTags;
    [HideInInspector]
    public List<string> e621Blacklist;
    [HideInInspector]
    public List<string> e621CharacterFilterers;

    Queue loadQueue = new Queue();
    Queue saveQueue = new Queue();
    Thread loadThread;
    string loadName = "";
    [HideInInspector]
    public string persistentDataPath;

    [Header("Visual")]
    public GameObject objLoad;
    public Text textLoad;
    public Animation animLoad;
    public Sprite sprLoading, sprError, sprBlank;
    public GameObject objLoadingScene;
    public AsyncOperation asyncLoadingScene;

    [Header("Tag Handler")]
    public GameObject objTagHandler;

    public Dropdown dropTagsChar;
    public Text textTagsCharName;
    public Text textTagsCharStats;
    public Image imageTagsChars;
    public Button buttonTagsCharAdd;

    public Dropdown dropTagsArtist;
    public Text textTagsArtistName;
    public Text textTagsArtistStats;
    public Image imageTagsArtist;
    public Button buttonTagsArtistAdd;

    public Dropdown dropTagsSpecific;
    public InputField inputTagsSpecific;
    public Toggle toggleTagsDelete;

    public delegate void TagSelectorFunc(string theTag);
    public TagSelectorFunc tagSelectorFunc;

    private void Awake()
    {
        if (act == null)
        {
            Screen.SetResolution(1920, 1080, true);
            UnityThread.initUnityThread();
            act = this;
            StartCoroutine(DataLoader());
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        persistentDataPath = Application.persistentDataPath;
        ReloadAllData();
        //InitialLoadData();
    }

    private void Update()
    {
        dropTagsChar.interactable = e621CharacterData.Count != 0;
        dropTagsArtist.interactable = e621ArtistData.Count != 0;
        dropTagsSpecific.interactable = e621SpecificTags.Count != 0;

        objLoadingScene.SetActive(asyncLoadingScene != null && !asyncLoadingScene.isDone);
    }

    #region TagData Functions
    public int TagData_GenerateID(string _tag)
    {
        int tagID = tagData.all.IndexOf(_tag);
        if (tagID == -1)
        {
            tagData.all.Add(_tag);
            tagID = tagData.all.IndexOf(_tag);
        }
        if (tagID == -1) Debug.LogError("ALERT! Invalid tag. It somehow was not generated!");
        return tagID;
    }

    public int TagData_GetIdByString(string _tag)
    {
        return tagData.all.IndexOf(_tag);
    }

    public string TagData_GetStringByID(int _tag)
    {
        string theTag = "?¿?";
        if (_tag >= 0 && _tag < tagData.all.Count)
        {
            theTag = tagData.all[_tag];
        }
        if (theTag == "?¿?") Debug.LogError("ALERT! Invalid tag. It somehow was not returned!");
        return theTag;
    }
    #endregion

    #region Main Data Functions
    IEnumerator DataLoader()
    {
        while (true)
        {
            yield return null;

            if(loadQueue.Count > 0 && loadThread == null)
            {
                loadName = (string)loadQueue.Dequeue();
                //print("Load:" + loadName);
                textLoad.text = "Loading:\n" + loadName;
                objLoad.SetActive(true);
                //animLoad.Play("LoadProgressHourGlass_Rotate");
                loadThread = new Thread(new ThreadStart(LoadDataThreaded));
                loadThread.Start();
            }

            if(loadThread == null && objLoad.activeInHierarchy)
            {
                objLoad.SetActive(false);
            }

            if (loadThread != null) continue;

            if (saveQueue.Count > 0 && loadThread == null)
            {
                loadName = (string)saveQueue.Dequeue();
                //print("Save:" + loadName);
                textLoad.text = "Saving:\n" + loadName;
                objLoad.SetActive(true);
                //animLoad.Play("LoadProgressHourGlass_Rotate");
                loadThread = new Thread(new ThreadStart(SaveDataThreaded));
                loadThread.Start();
            }

        }
    }

    public void ReloadAllData()
    {
        LoadData("fileData");
        LoadData("tagData");

        LoadData("imageData");
        LoadData("e621CharacterData");
        LoadData("e621ArtistData");
        LoadData("e621SpecificTags");
        LoadData("e621Blacklist");
        LoadData("e621CharacterFilter");
        print("Reloaded All Data");
    }

    public void InitialLoadData()
    {
        ReloadAllData();
    }

    /// <summary>
    /// Load Data
    /// </summary>
    /// <param name="type">fileData, tagData, imageData, e621CharacterData, e621ArtistData, e621SpecificTags, e621Blacklist, e621CharacterFilter</param>
    public void LoadData(string type)
    {
        if (!loadQueue.Contains(type)) loadQueue.Enqueue(type);
    }


    public void LoadDataThreaded()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = null;
        switch (loadName)
        {
            case "fileData":
                if (File.Exists(persistentDataPath + "/FileData.DATA"))
                {
                    file = File.Open(persistentDataPath + "/FileData.DATA", FileMode.Open);
                    fileData = (List<FileData>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    fileData = new List<FileData>();
                }
                break;
            case "tagData":
                if (File.Exists(persistentDataPath + "/TagData.DATA"))
                {
                    file = File.Open(persistentDataPath + "/TagData.DATA", FileMode.Open);
                    tagData = (TagData)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    tagData = new TagData();
                }
                break;
            case "imageData":
                if (File.Exists(persistentDataPath + "/ImageData.DATA"))
                {
                    file = File.Open(persistentDataPath + "/ImageData.DATA", FileMode.Open);
                    imageData = (List<ImageData>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    imageData = new List<ImageData>();
                }
                break;
            case "e621CharacterData":
                if (File.Exists(persistentDataPath + "/Character.DATA"))
                {
                    file = File.Open(persistentDataPath + "/Character.DATA", FileMode.Open);
                    e621CharacterData = (List<E621CharacterData>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    e621CharacterData = new List<E621CharacterData>();
                }
                break;
            case "e621ArtistData":
                if (File.Exists(persistentDataPath + "/Artist.DATA"))
                {
                    file = File.Open(persistentDataPath + "/Artist.DATA", FileMode.Open);
                    e621ArtistData = (List<E621CharacterData>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    e621ArtistData = new List<E621CharacterData>();
                }
                break;
            case "e621SpecificTags":
                if (File.Exists(persistentDataPath + "/SpecificTags.DATA"))
                {
                    file = File.Open(persistentDataPath + "/SpecificTags.DATA", FileMode.Open);
                    e621SpecificTags = (List<string>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    e621SpecificTags = new List<string>();
                }
                break;
            case "e621Blacklist":
                if (File.Exists(persistentDataPath + "/Blacklist.DATA"))
                {
                    file = File.Open(persistentDataPath + "/Blacklist.DATA", FileMode.Open);
                    e621Blacklist = (List<string>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    e621Blacklist = new List<string>();
                }
                break;
            case "e621CharacterFilter":
                if (File.Exists(persistentDataPath + "/CharacterFilter.DATA"))
                {
                    file = File.Open(persistentDataPath + "/CharacterFilter.DATA", FileMode.Open);
                    e621CharacterFilterers = (List<string>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    e621CharacterFilterers = new List<string>();
                }
                break;
        }
        loadThread = null;
    }

    public void SaveAllData()
    {
        SaveData("fileData");
        SaveData("tagData");

        SaveData("imageData");
        SaveData("e621CharacterData");
        SaveData("e621ArtistData");
        SaveData("e621SpecificTags");
        SaveData("e621Blacklist");
        SaveData("e621CharacterFilter");
    }

    /// <summary>
    /// Save Data
    /// </summary>
    /// <param name="type">fileData, tagData, imageData, e621CharacterData, e621ArtistData, e621SpecificTags, e621Blacklist, e621CharacterFilter</param>
    public void SaveData(string type)
    {
        if (!saveQueue.Contains(type)) saveQueue.Enqueue(type);
    }

    public void SaveDataThreaded()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = null;

        switch (loadName)
        {
            case "fileData":
                file = File.Create(persistentDataPath + "/FileData.DATA");
                bf.Serialize(file, fileData);
                file.Close();
                break;
            case "tagData":
                file = File.Create(persistentDataPath + "/TagData.DATA");
                bf.Serialize(file, tagData);
                file.Close();
                break;
            case "imageData":
                file = File.Create(persistentDataPath + "/ImageData.DATA");
                bf.Serialize(file, imageData);
                file.Close();
                break;
            case "e621CharacterData":
                file = File.Create(persistentDataPath + "/Character.DATA");
                bf.Serialize(file, e621CharacterData);
                file.Close();
                break;
            case "e621ArtistData":
                file = File.Create(persistentDataPath + "/Artist.DATA");
                bf.Serialize(file, e621ArtistData);
                file.Close();
                break;
            case "e621SpecificTags":
                file = File.Create(persistentDataPath + "/SpecificTags.DATA");
                bf.Serialize(file, e621SpecificTags);
                file.Close();
                break;
            case "e621Blacklist":
                file = File.Create(persistentDataPath + "/Blacklist.DATA");
                bf.Serialize(file, e621Blacklist);
                file.Close();
                break;
            case "e621CharacterFilter":
                file = File.Create(persistentDataPath + "/CharacterFilter.DATA");
                bf.Serialize(file, e621CharacterFilterers);
                file.Close();
                break;
        }
        loadThread = null;
    }
    #endregion

    #region Tag Selector
    //----------------------------------------------------
    

    public void TagsOpenGameObject()
    {
        TagsDefault();
        objTagHandler.SetActive(true);
    }

    public void TagsDefault()
    {
        List<string> newOptions = new List<string>();
        foreach (E621CharacterData dat in e621CharacterData)
        {
            newOptions.Add(dat.tag);
        }
        //newOptions.Sort();
        newOptions.Insert(0, "Select");
        dropTagsChar.ClearOptions();
        dropTagsChar.AddOptions(newOptions);

        newOptions.Clear();

        foreach (E621CharacterData dat in e621ArtistData)
        {
            newOptions.Add(dat.tag);
        }
        newOptions.Insert(0, "Select");
        dropTagsArtist.ClearOptions();
        dropTagsArtist.AddOptions(newOptions);

        //create tags options here

        TagsDefaultSpecific();
    }

    #region Tag Character

    public void DropAddTagCharacter(int value)
    {
        dropTagsArtist.value = 0;
        dropTagsArtist.RefreshShownValue();

        GlobalActions.act.LoadImageCancel();

        textTagsCharName.gameObject.SetActive(value != 0);
        textTagsCharStats.gameObject.SetActive(value != 0);
        imageTagsChars.gameObject.SetActive(value != 0);
        buttonTagsCharAdd.gameObject.SetActive(value != 0);

        if (dropTagsChar.value == 0) return;

        E621CharacterData chara = e621CharacterData[value - 1];

        GlobalActions.act.LoadImage(sprLoading, sprError, imageTagsChars, PlayerPrefs.GetString("E621_CharPortraits") + @"\" + chara.portraitFile + ".png");
        textTagsCharName.text = "Showing: " + chara.name;

        int appeared = imageData.Count(t => t.tags.Contains(chara.tag) && !t.filtered);
        int filtered = imageData.Count(t => t.tags.Contains(chara.tag) && t.filtered);
        float div = filtered == 0 ? 1 : filtered;

        textTagsCharStats.text = "Appearances: " + appeared + ". Filtered: " + filtered + ".\nGood Image Ratio: " + ((Mathf.Round(((float)appeared / div) * 1000f) / 1000f)) ;

    }

    public void TagsButtonAddCharacter()
    {
        tagSelectorFunc(dropTagsChar.options[dropTagsChar.value].text);
    }

    public void TagsButtonNavigationCharacter(string val)
    {
        if(val == "next")
        {
            if (dropTagsChar.options.Count == 0 || dropTagsChar.value == dropTagsChar.options.Count - 1) return;
            dropTagsChar.value += 1;
            dropTagsChar.RefreshShownValue();
        }
        else if(val == "prev")
        {
            if (dropTagsChar.value == 0) return;
            dropTagsChar.value -= 1;
            dropTagsChar.RefreshShownValue();
        }
    }

    #endregion

    #region Tags Artist

    public void DropAddTagArtist(int value)
    {
        dropTagsChar.value = 0;
        dropTagsChar.RefreshShownValue();

        GlobalActions.act.LoadImageCancel();

        textTagsArtistName.gameObject.SetActive(value != 0);
        textTagsArtistStats.gameObject.SetActive(value != 0);
        imageTagsArtist.gameObject.SetActive(value != 0);
        buttonTagsArtistAdd.gameObject.SetActive(value != 0);

        if (dropTagsArtist.value == 0) return;

        E621CharacterData chara = e621ArtistData[value - 1];

        GlobalActions.act.LoadImage(sprLoading, sprError, imageTagsArtist, PlayerPrefs.GetString("E621_ArtistPortraits") + @"\" + chara.portraitFile + ".png");
        textTagsArtistName.text = "Showing: " + chara.name;

        int appeared = imageData.Count(t => t.tags.Contains(chara.tag.Replace("é", @"\u00e9")) && !t.filtered);
        int filtered = imageData.Count(t => t.tags.Contains(chara.tag.Replace("é", @"\u00e9")) && t.filtered);
        float div = filtered == 0 ? 1 : filtered;

        textTagsArtistStats.text = "Art: " + appeared + ". Filtered: " + filtered + ".\nGood Image Ratio: " + ((Mathf.Round(((float)appeared / div) * 1000f) / 1000f));

    }

    public void TagsButtonAddArtist()
    {
        tagSelectorFunc(dropTagsArtist.options[dropTagsArtist.value].text);
    }

    public void TagsButtonNavigationArtist(string val)
    {
        if (val == "next")
        {
            if (dropTagsArtist.options.Count == 0 || dropTagsArtist.value == dropTagsArtist.options.Count - 1) return;
            dropTagsArtist.value += 1;
            dropTagsArtist.RefreshShownValue();
        }
        else if (val == "prev")
        {
            if (dropTagsArtist.value == 0) return;
            dropTagsArtist.value -= 1;
            dropTagsArtist.RefreshShownValue();
        }
    }

    #endregion

    #region Tags Specific
    public void DropAddTagSpecific(int value)
    {
        if (value == 0) return;
        if (!toggleTagsDelete.isOn)
        {
            dropTagsSpecific.value = 0;
            dropTagsSpecific.RefreshShownValue();
            tagSelectorFunc(dropTagsSpecific.options[value].text);
        }
        else
        {
            dropTagsSpecific.value = 0;
            dropTagsSpecific.RefreshShownValue();
            GlobalActions.act.CreateAdvice("Delete the selected tag?\n" + dropTagsSpecific.options[value].text, 0, () =>
            {
                e621SpecificTags.Remove(dropTagsSpecific.options[value].text);
                TagsDefaultSpecific();
            });
        }
    }

    void TagsDefaultSpecific()
    {
        List<string> newOptions = new List<string>();
        foreach (string s in Data.act.e621SpecificTags)
        {
            newOptions.Add(s);
        }
        newOptions.Sort();
        newOptions.Insert(0, "Select");
        dropTagsSpecific.ClearOptions();
        dropTagsSpecific.AddOptions(newOptions);
    }

    public void InputAddTagSpecific(string value)
    {
        inputTagsSpecific.text = "";
        if (value != "" && !value.Contains(" ") && !Data.act.e621SpecificTags.Contains(value))
        {
            e621SpecificTags.Add(value);
            TagsDefaultSpecific();
        }
    }

    #endregion

    public void ButtonExitTags()
    {
        dropTagsChar.value = 0;
        dropTagsChar.RefreshShownValue();

        dropTagsArtist.value = 0;
        dropTagsArtist.RefreshShownValue();

        SaveData("e621SpecificTags");
    }


    
    //----------------------------------------------------
    #endregion
}
