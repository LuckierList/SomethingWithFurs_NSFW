
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

    Queue loadQueue = new Queue();
    Queue saveQueue = new Queue();
    Thread loadThread;
    string loadName = "";
    string persistentDataPath;

    [Header("Visual")]
    public GameObject objLoad;
    public Text textLoad;
    public Animation animLoad;
    

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
                animLoad.Play("LoadProgressHourGlass_Rotate");
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
                animLoad.Play("LoadProgressHourGlass_Rotate");
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
        print("Reloaded All Data");
    }

    public void InitialLoadData()
    {
        ReloadAllData();
    }

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
        }
        loadThread = null;
    }

    public void SaveAllData()
    {
        SaveData("imageData");
        SaveData("e621CharacterData");
        SaveData("e621SpecificTags");
        SaveData("e621Blacklist");
    }

    public void SaveData(string type)
    {
        if (!saveQueue.Contains(type)) saveQueue.Enqueue(type);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">imageData, e621CharacterData, e621SpecificTags, </param>
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

        }
        loadThread = null;
    }
}
