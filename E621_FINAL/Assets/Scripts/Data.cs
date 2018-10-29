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

    private void Awake()
    {
        if (act == null)
        {
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

        print("Reloaded All Data");
    }

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
                    print("File 'ImageData.DATA' doesn't exis.");
                    imageData = new List<ImageData>();
                }
                break;
        }
    }

    public void ConvertOldData()
    {

    }

    public void SaveAllData()
    {
        SaveData("imageData");
    }

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
        }
    }
}
