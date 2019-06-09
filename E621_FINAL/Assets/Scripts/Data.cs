
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
    public static Data act;
    [HideInInspector]
    public List<ImageData> imageData;
    [HideInInspector]
    public List<E621CharacterData> e621CharacterData;
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

    [Header("Tag Handler")]
    public GameObject objTagHandler;
    public Dropdown dropTagsChar;
    public Dropdown dropTagsArtist;
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
        LoadData("imageData");
        LoadData("e621CharacterData");
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
    /// <param name="type">imageData, e621CharacterData. e621SpecificTags, e621Blacklist, e621CharacterFilter</param>
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
        SaveData("imageData");
        SaveData("e621CharacterData");
        SaveData("e621SpecificTags");
        SaveData("e621Blacklist");
        SaveData("e621CharacterFilter");
    }

    /// <summary>
    /// Save Data
    /// </summary>
    /// <param name="type">imageData, e621CharacterData. e621SpecificTags, e621Blacklist, e621CharacterFilter</param>
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
    #region Tags

    public void TagsOpenGameObject()
    {
        TagsDefault();
        objTagHandler.SetActive(true);
    }

    public void TagsDefault()
    {
        List<string> newOptions = new List<string>();
        foreach (E621CharacterData dat in Data.act.e621CharacterData)
        {
            newOptions.Add(dat.tag);
        }
        newOptions.Sort();
        newOptions.Insert(0, "Select");
        dropTagsChar.ClearOptions();
        dropTagsChar.AddOptions(newOptions);

        newOptions.Clear();

        //create tags options here

        TagsDefaultSpecific();
    }

    public void DropAddTagCharacter(int value)
    {
        if (value == 0) return;
        tagSelectorFunc(dropTagsChar.options[value].text);
        dropTagsChar.value = 0;
        dropTagsChar.RefreshShownValue();
        
    }

    public void DropAddTagArtist(int value)
    {
        if (value == 0) return;
        dropTagsArtist.value = 0;
        dropTagsArtist.RefreshShownValue();
        tagSelectorFunc(dropTagsArtist.options[value].text);
    }

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

    public void ButtonExitTags()
    {
        SaveData("e621SpecificTags");
    }


    #endregion
    //----------------------------------------------------
    #endregion
}
