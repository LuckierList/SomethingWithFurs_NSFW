﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading;
using System.Linq;
using System.IO;

public class E621_NavigationButton : MonoBehaviour
{
    public Sprite imgBlank, imgError, imgLoading, imgBlacklist;
    public Color colorNull, colorOnDisk, colorNotOnDisk, colorOnFilter;
    public Button buttonBlacklisted, buttonKeep, buttonFilter, buttonCancelDownload, buttonReload, buttonSpTags;
    public Slider sliderProgress;
    public Image imageExistance, imagePreview, imageAnim, imageFilterStatus;
    public Text textRating, textType, textSpTags;

    Sprite newSprite;
    Texture2D newTexture;

    [HideInInspector]
    public string urlDownload, urlThumb, urlPreview, rating, status, md5, urlPage;
    [HideInInspector]
    public int id;
    [HideInInspector]
    public List<string> tags;

    Coroutine activeCo;
    Thread checkExistanceT;
    string fileLocation = "";
    ImageData data;
    //bool hideBlacklist;
    bool canAppear = false;
    [HideInInspector]
    public bool existanceChecked = false;

    [HideInInspector]
    public bool previewDwl = false;

    private void Update()
    {
        imageFilterStatus.color = data == null ? colorNull : !data.filtered ? colorOnDisk : colorNotOnDisk;

        if (newSprite == null) return;
        if (canAppear)
        {
            imagePreview.sprite = newSprite;
            return;
        }
        
        imagePreview.sprite = !canAppear ? imgBlacklist : newSprite;
    }

    public void Initialize()
    {
        imagePreview.sprite = imgBlank;
        textRating.text = rating.ToUpper();
        textRating.color = rating == "s" ? Color.green : rating == "q" ? Color.yellow : rating == "e" ? Color.red : Color.magenta;

        //text type
        textType.text = (tags.Contains("dickgirl") || tags.Contains("herm") || tags.Contains("intersex")) ? "Dickgirl" : tags.Contains("female") ? "Female" :
            tags.Contains("crossgender") && !tags.Contains("ambiguous_gender") ? "Crossgender" : !tags.Contains("crossgender") && tags.Contains("ambiguous_gender") ?
            "Ambiguous" : tags.Contains("crossgender") && tags.Contains("ambiguous_gender") ? "Cross/Ambiguous" : "Other";

        foreach (string s in Data.act.e621Blacklist)
        {
            if(tags.Contains(s))
            {
                canAppear = true;
                break;
            }
        }

        if (E621_Navigation.act.toggleBlacklistHide.isOn && !canAppear)
        {
            Destroy(gameObject);
            return;
        }

        //if (canAppear)E621_Navigation.act.queueExistance.Enqueue(this);
        E621_Navigation.act.queueExistance.Enqueue(this);

        activeCo = StartCoroutine(GetImage(urlThumb, imagePreview));
        imageAnim.gameObject.SetActive(tags.Contains("animated"));
        buttonBlacklisted.gameObject.SetActive(!canAppear);
        buttonSpTags.gameObject.SetActive(false);

        buttonKeep.interactable = false;
        buttonFilter.interactable = false;
        
        //print("Download:" + urlDownload); 
    }

    public void ButtonBlacklistShow()
    {
        canAppear = !canAppear;
    }

    public void ButtonTags()
    {
        string t = "";
        foreach(string s in tags)
        {
            t += "'" + s + "'  ";
        }
        GlobalActions.act.CreateAdvice("Tags",t,2);
    }

    public void ButtonLoadPreview()
    {
        if (imagePreview.sprite == imgBlacklist) return;
        if (tags.Contains("animated"))
        {
            Application.OpenURL(urlDownload);
            return;
        }

        E621_Navigation.act.imagePreview.sprite = newSprite;
        E621_Navigation.act.objPreviewViewer.SetActive(true);

        if (previewDwl) { return; }

        if(E621_Navigation.act.previewLoadCo != null)
        {
            StopCoroutine(E621_Navigation.act.previewLoadCo);
            E621_Navigation.act.previewLoadCo = null;
        }
        E621_Navigation.act.previewLoadCo = StartCoroutine(GetImage(urlPreview, E621_Navigation.act.imagePreview, true));
    }

    IEnumerator GetImage(string url, Image target, bool prev = false)
    {
        yield return null;
        GameObject objHourGlass = null;
        if (prev)
        {
            objHourGlass = target.transform.parent.GetChild(1).gameObject;
            objHourGlass.SetActive(true);
        }

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            //yield return uwr.SendWebRequest();
            yield return null;
            uwr.SendWebRequest();
            buttonReload.gameObject.SetActive(false);
            sliderProgress.gameObject.SetActive(true);
            while (!uwr.isDone)
            {
                sliderProgress.value = uwr.downloadProgress;
                yield return null;
            }
            sliderProgress.gameObject.SetActive(false);
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                buttonReload.gameObject.SetActive(true);
                target.sprite = imgError;
                Debug.Log(uwr.error);
            }
            else
            {
                yield return newTexture = DownloadHandlerTexture.GetContent(uwr);
                yield return newSprite = Sprite.Create(newTexture, new Rect(0f, 0f, newTexture.width, newTexture.height), new Vector2(.5f, .5f), 100f);
                target.sprite = newSprite;
            }
        }
        if (prev)
        {
            objHourGlass.SetActive(false);
            previewDwl = true;
        }
        activeCo = null;
    }

    public void OnDestroy()
    {
        Debug.LogAssertion("Destroy button");
        if (activeCo != null) StopCoroutine(activeCo);
        if (checkExistanceT != null && checkExistanceT.IsAlive)
        {
            checkExistanceT.Abort(checkExistanceT);
        }

    }

    public void ButtonCheckExistance()
    {
        print("lel");
        if(imageExistance.color == Color.magenta)
        {
            E621_Navigation.act.queueExistance.Enqueue(this);
            imageExistance.color = Color.black;
        }
        
    }

    public void CheckExistance()
    {
        checkExistanceT = new Thread(new ThreadStart(CheckExistanceThread));
        checkExistanceT.Start();
        buttonKeep.interactable = false;
        buttonFilter.interactable = false;
    }

    void CheckExistanceThread()
    {
        try
        {
            data = Data.act.imageData.Where(temp => temp.filename.ToLower().Substring(0, temp.filename.IndexOf(".")) == id + "-" + md5).SingleOrDefault();
            string sUrl = "null";
            UnityThread.executeInUpdate(() =>
            {
                sUrl = !(tags.Contains("intersex") || tags.Contains("dickgirl") || tags.Contains("herm")) ? E621_Navigation.act.inputStraightGal.text : E621_Navigation.act.inputDickgirlGal.text;
                if (tags.Contains("animated"))
                {
                    if (sUrl == E621_Navigation.act.inputStraightGal.text) sUrl = E621_Navigation.act.inputStraightVid.text; else sUrl = E621_Navigation.act.inputDickgirlVid.text;
                }
            });
            while (sUrl == "null") { }

            string md5_format = urlDownload.Substring(urlDownload.LastIndexOf("/") + 1, urlDownload.Length - (urlDownload.LastIndexOf("/") + 1));


            //bool exist = File.Exists(sUrl + "/" + id + "-" + md5_format) || File.Exists(sUrl + "/" + md5_format) || File.Exists(sUrl + "/" + md5_format.ToUpper());
            bool exist = false;
            string[] possibleUrls = new string[] { sUrl + "/" + id + "-" + md5_format, sUrl + "/" + md5_format, sUrl + "/" + md5_format.ToUpper() };

            for (int i = 0; i < possibleUrls.Length; i++)
            {
                if (File.Exists(possibleUrls[i]))
                {
                    fileLocation = possibleUrls[i];
                    exist = true;
                    break;
                }
            }

            if (data == null)
            {
                data = Data.act.imageData.Where(temp => temp.filename.ToLower().Substring(0, temp.filename.IndexOf(".")) == md5).SingleOrDefault();
            }
            //print(md5);

            //Get the tags in it

            string charTags = "";
            string artistTags = "";
            string specificTags = "";

            foreach(string s in tags)
            {
                E621CharacterData temp1 = Data.act.e621CharacterData.Where(temp => temp.tag.Replace("é", @"\u00e9") == s).FirstOrDefault();
                if(temp1 != null) charTags += charTags == "" ? s : " " + s;

                temp1 = Data.act.e621ArtistData.Where(temp => temp.tag == s).FirstOrDefault();
                if (temp1 != null) artistTags += artistTags == "" ? s : " " + s;

                string temp2 = Data.act.e621SpecificTags.Where(temp => temp == s).FirstOrDefault();
                if (temp2 != null) specificTags += specificTags == "" ? s : " " + s;
            }

            string finalTags = "";
            if (charTags != "") finalTags += "Characters:\n" + charTags;
            if (artistTags != "") finalTags += finalTags == "" ? "Artists:\n" + artistTags : "\nArists:\n" + artistTags;
            //if (specificTags != "") finalTags += finalTags == "" ? "Specific Tags:\n" + specificTags : "\nSpecific Tags:\n" + specificTags;

           

            UnityThread.executeInUpdate(() =>
            {
                try
                {
                    if (finalTags != "")
                    {
                        buttonSpTags.gameObject.SetActive(true);
                        textSpTags.text = finalTags;
                    }

                    buttonKeep.interactable = data == null || data.filtered;
                    buttonFilter.interactable = data == null || !data.filtered;
                    print(data);

                    imageExistance.color = exist ? colorOnDisk : colorNotOnDisk;

                    existanceChecked = true;
                }
                catch
                {
                    print("This error should only happen if the button stopped existing");
                }
            });
        }
        catch
        {
            print("Thread Error: Nav. Button");
            return;
        }
        
    }

    public void ButtonOpenInPage()
    {
        Application.OpenURL(urlPage);
    }

    public void ButtonFilter(string value)
    {
        ButtonFilter(value, false);
    }

    public void ButtonFilter(string value, bool skipBlackList)
    {
        if ((imagePreview.sprite == imgLoading || !canAppear) && !skipBlackList)
        {
            E621_Navigation.act.CreateAdvice("This image is BlackListed... \n\n Keep/Filter it ANYWAY?", 0, ()=>
            {
                ButtonFilter(value, true);
            });
            return;
        }

        if (!tags.Contains("animated"))
        {
            if ((tags.Contains("dickgirl") || tags.Contains("intersex") || tags.Contains("herm")) && E621_Navigation.act.inputDickgirlGal.text == "")
            {
                E621_Navigation.act.CreateAdvice("Dickgirl Gal doesn't exist", "Can´t Keep/Filter the iamge because it's 'Dickgirl' related and the gallery has not been assigned.");
                return;
            }

            if (!((tags.Contains("dickgirl") || tags.Contains("intersex") || tags.Contains("herm"))) && E621_Navigation.act.inputStraightGal.text == "")
            {
                E621_Navigation.act.CreateAdvice("Straight Gal doesn't exist", "Can´t Keep/Filter the iamge because it's NOT 'Dickgirl' related and the gallery has not been assigned.");
                return;
            }
        }
        else
        {
            if ((tags.Contains("dickgirl") || tags.Contains("intersex") || tags.Contains("herm")) && E621_Navigation.act.inputDickgirlVid.text == "")
            {
                E621_Navigation.act.CreateAdvice("Dickgirl Gal doesn't exist", "Can´t Keep/Filter the iamge because it's 'Dickgirl' related and the gallery has not been assigned.");
                return;
            }

            if (!((tags.Contains("dickgirl") || tags.Contains("intersex") || tags.Contains("herm"))) && E621_Navigation.act.inputStraightVid.text == "")
            {
                E621_Navigation.act.CreateAdvice("Straight Gal doesn't exist", "Can´t Keep/Filter the iamge because it's NOT 'Dickgirl' related and the gallery has not been assigned.");
                return;
            }

        }

        //--------------------------
        //check for old data

        if (data != null)
        {
            Data.act.imageData.Remove(data);
            print("Detected old data and removed it");
        }

        //--------------------------
        string filename = id + "-" + urlDownload.Substring(urlDownload.LastIndexOf("/") + 1, urlDownload.Length - (urlDownload.LastIndexOf("/") + 1));

        switch (value)
        {
            case "keep":
                
                data = new ImageData("E621", id, filename, false);
                data.tags = tags;
                if (imageExistance.color != colorOnDisk)
                E621_Navigation.act.listCoroutines.Add(new DownloadImage(urlDownload, this, imagePreview.sprite, data, Instantiate(E621_Navigation.act.prefabDownloading, E621_Navigation.act.downloaderContent)));
                break;
            case "filter":
                data = new ImageData("E621", id, filename, true);
                data.tags = tags;
                if (E621_Navigation.act.listCoroutines.Where(temp => temp.data.id == id).SingleOrDefault() != null)
                {
                    CancelDownload();
                }
                if(imageExistance.color == colorOnDisk)
                {
                    File.Delete(fileLocation);
                }
                break;
        }

        Data.act.imageData.Add(data);
        print("saved:" + data.filename);
        CheckExistance();

    }

    public void CancelDownload()
    {
        DownloadImage d = E621_Navigation.act.listCoroutines.Where(temp => temp.data.id == id).SingleOrDefault();
        
        if(d != null)
        {
            StopCoroutine(d.thisCoroutine);
            Destroy(d.objProgress);
            E621_Navigation.act.listCoroutines.Remove(E621_Navigation.act.listCoroutines.Where(temp => temp.data.id == id).Single());
            print("download cancelled");
        }
    }
}
