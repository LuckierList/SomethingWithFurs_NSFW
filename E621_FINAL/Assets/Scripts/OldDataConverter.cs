using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Linq;

public class OldDataConverter : GlobalActions
{
    public Text textDebug, textData;
    public int convertQ = 5;

    FileData newFiledata;
    ImageData oldData;

    Coroutine coroutine;

    bool endedGettingData = false;

    List<string> listErrors;
    
    private void Start()
    {
        listErrors = new List<string>();
    }

    private void Update()
    {
        textData.text = "I: " + Data.act.imageData.Count + " | F: " + Data.act.fileData.Count + "\nTags: " + Data.act.tagData.all.Count;
    }

    public void EraseDuplicate()
    {
        Data.act.fileData = Data.act.fileData.Distinct().ToList();
    }

    public void ButtonStart()
    {
        if(coroutine == null)
        coroutine = StartCoroutine(UpdateDataDownload());
    }

    public void Stop()
    {
        if(coroutine != null)
        {
            AddLog("Stoped by User\n\n");
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    public void Save()
    {
        Data.act.SaveData("fileData");
        Data.act.SaveData("tagData");
        Data.act.SaveData("imageData");
    }

    string html = "";
    IEnumerator UpdateDataDownload()
    {
        AddLog("Data Lenght = " + Data.act.imageData.Count);
        int usedLenght = convertQ;
        if (convertQ == -1) usedLenght = Data.act.imageData.Count;
        for (int i = 0; i < usedLenght; i++)
        {
            yield return null;
            endedGettingData = false;
            oldData = Data.act.imageData[i];
            AddLog("Started getting data for Image at index: " + i);

            string gotMd5 = oldData.filename.Substring(0, oldData.filename.IndexOf("."));
            if (gotMd5.Contains("-")) gotMd5 = gotMd5.Substring(gotMd5.IndexOf("-") + 1, gotMd5.Length - (gotMd5.IndexOf("-") + 1));
            string url = @"https://e621.net/post/index/1/status:any%20md5:" + gotMd5;


            //print(url);

            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                //yield return uwr.SendWebRequest();
                uwr.SendWebRequest();

                while (!uwr.isDone)
                {
                    //progress
                    yield return null;
                }
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    //CreateAdvice(uwr.error, 2);
                    AddLog("\nNetwork Error");
                    Debug.Log(uwr.error);
                    uwr.Dispose();
                    continue;
                }
                else
                {
                    html = uwr.downloadHandler.text;
                    uwr.Dispose();
                }
            }
            Resources.UnloadUnusedAssets();

            //Revisar si existen imagenes
            if (html.IndexOf("Post.register({") == -1)
            {
                AddLog("\nNo image exists with this MD5: " + gotMd5 + ", should delete?");
                continue;
            }

            //substring para obtener los datos de los posts
            html = html.Substring(html.IndexOf("Post.register({"), html.Length - html.IndexOf("Post.register({"));
            html = html.Substring(0, html.IndexOf("Post.blacklist_options ="));

            Thread t = new Thread(new ThreadStart(ThreadedGetImageData));
            t.Start();
            while (!endedGettingData) yield return null;
            Data.act.fileData.Add(newFiledata);
            Data.act.imageData.Remove(oldData);
            
            string text = "id: " + newFiledata.Id + "\n";
            text += "md5: " + newFiledata.Md5 + "\n";
            text += "filename: " + newFiledata.Filename + "\n";
            text += "format: " + newFiledata.Format + "\n";
            text += "filtered: " + newFiledata.Filtered + "\n";
            text += "full: " + newFiledata.UrlFull + "\n";
            text += "prev: " + newFiledata.UrlPreview + "\n";
            text += "thumb: " + newFiledata.UrlThumb + "\n";
            text += "r: " + newFiledata.Rating + "\n";
            text += "s: " + newFiledata.Status + "\n";

            //CreateAdvice(text,2);
            AddLog(text);
            AddLog("\nAdded data successfully!");
        }

        coroutine = null;
    }
    
    void ThreadedGetImageData()
    {
        try
        {
            html = html.Substring(html.IndexOf("Post.register({"), html.Length - html.IndexOf("Post.register({"));
            //eliminar el post register actual para no ciclarse infinitamente
            html = html.Substring(15, html.Length - 15);


            int id = -1;
            List<string> tags = new List<string>();
            string urlDownload = "";
            string urlThumb = "";
            string urlPreview = "";
            string rating = "";
            string status = "";
            string md5 = "";

            string temp;
            //obtener datos del post
            //ID
            temp = html.Substring(html.IndexOf("id") + 4, html.IndexOf(",") - 5);
            id = int.Parse(temp);
            html = html.Substring(html.IndexOf(",") + 9, html.Length - html.IndexOf(",") - 9);
            //print(html);

            //tags
            temp = html.Substring(0, html.IndexOf(",") - 1);
            temp += " ";
            while (temp.IndexOf(" ") != -1)
            {
                tags.Add(temp.Substring(0, temp.IndexOf(" ")));
                temp = temp.Substring(temp.IndexOf(" ") + 1, temp.Length - (temp.IndexOf(" ") + 1));
            }

            //MD5
            if (html.IndexOf("md5") != -1)
            {
                html = html.Substring(html.IndexOf("md5") + 6, html.Length - (html.IndexOf("md5") + 6));
                
                md5 = html.Substring(0, html.IndexOf(",") - 1);
                //print("MD5: " + md5);
               
            }

            html = html.Substring(html.IndexOf("file_url") + 11, html.Length - (html.IndexOf("file_url") + 11));
            //Image URL
            urlDownload = html.Substring(0, html.IndexOf(",") - 1);
            //print(urlDownload);
            html = html.Substring(html.IndexOf("preview_url") + 14, html.Length - (html.IndexOf("preview_url") + 14));

            //Thumb URL
            urlThumb = html.Substring(0, html.IndexOf(",") - 1);
            //print(urlThumb);
            html = html.Substring(html.IndexOf("sample_url") + 13, html.Length - (html.IndexOf("sample_url") + 13));

            //Preview URL
            urlPreview = html.Substring(0, html.IndexOf(",") - 1);
            //print(urlPreview);
            html = html.Substring(html.IndexOf("rating") + 9, html.Length - (html.IndexOf("rating") + 9));

            //Rating
            rating = html.Substring(0, html.IndexOf(",") - 1);
            //print(html);
            //print("Rating: " + rating);
            html = html.Substring(html.IndexOf("rating") + 9, html.Length - (html.IndexOf("rating") + 9));

            //Status
            html = html.Substring(html.IndexOf("status") + 6, html.Length - (html.IndexOf("status") + 6));
            status = html.Substring(0, html.IndexOf(",") - 1);
            if (md5 == "") md5 = status;
            //print(status);

            //Set Data
            string format = urlDownload.Substring(urlDownload.LastIndexOf(".") + 1, urlDownload.Length - (urlDownload.LastIndexOf(".") + 1));
            
            int[] newTags = new int[tags.Count];
            for (int i = 0; i < newTags.Length; i++)
            {
                newTags[i] = Data.act.TagData_GenerateID(tags[i]);
            }
            newFiledata = new FileData(id, md5, (id + "-" + md5), format, oldData.filtered, newTags, urlDownload, urlThumb, urlPreview, rating, status);
            endedGettingData = true;
        }
        catch
        {
            Debug.Log("ThreadError");
        }
    }

    void AddLog(string s)
    {
        string old = textDebug.text;
        if (old.Length >= 16100)
        {
            old = old.Substring(0, 6000);
            Debug.LogWarning("limit exceeded");
        }
        textDebug.text = s + "\n" + old;
    }
}
