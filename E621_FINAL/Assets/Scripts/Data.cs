
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;

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

    [Header("Visual")]
    public GameObject objLoad;
    public Image imageLoadProgress;
    public Text textLoad;
    public Transform transLoad;

    private void Awake()
    {
        if (act == null)
        {
            Screen.SetResolution(1920, 1080, true);
            act = this;
            
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ReloadAllData();
    }

    public void ReloadAllData()
    {
        LoadData("imageData");
        LoadData("e621CharacterData");
        LoadData("e621SpecificTags");
        LoadData("e621Blacklist");
        print("Reloaded All Data");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">imageData, e621CharacterData, </param>
    public void LoadData(string type)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = null;
        switch (type)
        {
            case "imageData":
                if (File.Exists(Application.persistentDataPath + "/ImageData.DATA"))
                {
                    file = File.Open(Application.persistentDataPath + "/ImageData.DATA", FileMode.Open);
                    imageData = (List<ImageData>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    print("File 'ImageData.DATA' doesn't exist.");
                    imageData = new List<ImageData>();
                }
                break;
            case "e621CharacterData":
                if (File.Exists(Application.persistentDataPath + "/Character.DATA"))
                {
                    file = File.Open(Application.persistentDataPath + "/Character.DATA", FileMode.Open);
                    e621CharacterData = (List<E621CharacterData>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    print("File 'Character.DATA' doesn't exist.");
                    e621CharacterData = new List<E621CharacterData>();
                }
                break;
            case "e621SpecificTags":
                if (File.Exists(Application.persistentDataPath + "/SpecificTags.DATA"))
                {
                    file = File.Open(Application.persistentDataPath + "/SpecificTags.DATA", FileMode.Open);
                    e621SpecificTags = (List<string>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    print("File 'SpecificTags.DATA' doesn't exist.");
                    e621SpecificTags = new List<string>();
                }
                break;
            case "e621Blacklist":
                if (File.Exists(Application.persistentDataPath + "/Blacklist.DATA"))
                {
                    file = File.Open(Application.persistentDataPath + "/Blacklist.DATA", FileMode.Open);
                    e621Blacklist = (List<string>)bf.Deserialize(file);
                    file.Close();
                }
                else
                {
                    print("File 'Blacklist.DATA' doesn't exist.");
                    e621Blacklist = new List<string>();
                }
                break;
        }
    }

    public void SaveAllData()
    {
        SaveData("imageData");
        SaveData("e621CharacterData");
        SaveData("e621SpecificTags");
        SaveData("e621Blacklist");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">imageData, e621CharacterData, e621SpecificTags, </param>
    public void SaveData(string type)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = null;
        switch (type)
        {
            case "imageData":
                file = File.Create(Application.persistentDataPath + "/ImageData.DATA");
                bf.Serialize(file, imageData);
                file.Close();
                break;
            case "e621CharacterData":
                file = File.Create(Application.persistentDataPath + "/Character.DATA");
                bf.Serialize(file, e621CharacterData);
                file.Close();
                break;
            case "e621SpecificTags":
                file = File.Create(Application.persistentDataPath + "/SpecificTags.DATA");
                bf.Serialize(file, e621SpecificTags);
                file.Close();
                break;
            case "e621Blacklist":
                file = File.Create(Application.persistentDataPath + "/Blacklist.DATA");
                bf.Serialize(file, e621Blacklist);
                file.Close();
                break;
        }
    }
}
