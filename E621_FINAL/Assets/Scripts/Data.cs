using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Data : MonoBehaviour
{
    public static Data act;
    [HideInInspector]
    public List<ImageData> imageData;
    [HideInInspector]
    public List<E621CharacterData> e621CharacterData;

    private void Awake()
    {
        if (act == null)
        {
            Screen.SetResolution(1920, 1080, true);
            act = this;
            ReloadAllData();
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void ReloadAllData()
    {
        LoadData("imageData");
        LoadData("e621CharacterData");
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
        }
    }

    public void SaveAllData()
    {
        SaveData("imageData");
        SaveData("e621CharacterData");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">imageData, e621CharacterData, </param>
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
        }
    }
}
