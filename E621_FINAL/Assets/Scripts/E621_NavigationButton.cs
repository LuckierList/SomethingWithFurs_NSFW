using System.Collections;
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
    public Button buttonBlacklisted, buttonKeep, buttonFilter;
    public Image imageExistance, imagePreview, imageAnim;

    Sprite newSprite;
    Texture2D newTexture;

    [HideInInspector]
    public string urlDownload, urlThumb, urlPreview, rating, status, md5, urlPage;
    [HideInInspector]
    public int id;
    [HideInInspector]
    public List<string> tags;

    Coroutine activeCo;

    bool hideBlacklist;
    bool canAppear = false;

    private void Update()
    {
        if (newSprite == null) return;
        if (canAppear)
        {
            imagePreview.sprite = newSprite;
            return;
        }
            
        hideBlacklist = E621_Navigation.act.toggleBlacklistHide.isOn;
        
        imagePreview.sprite = hideBlacklist ? imgBlacklist : newSprite;


    }

    public void Initialize()
    {
        activeCo = StartCoroutine(GetImage(urlThumb, imagePreview));
        foreach(string s in Data.act.e621Blacklist)
        {
            if(tags.Contains(s))
            {
                canAppear = true;
                break;
            }
        }
        imageAnim.gameObject.SetActive(tags.Contains("animated"));
        buttonBlacklisted.gameObject.SetActive(!canAppear);

        CheckExistance();
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
            t += s + "   ";
        }
        GlobalActions.act.CreateAdvice("Tags",t,2);
    }

    public void ButtonLoadPreview()
    {
        if (imagePreview.sprite == imgBlacklist || tags.Contains("animated")) return;
        E621_Navigation.act.objPreviewViewer.SetActive(true);
        if(E621_Navigation.act.previewLoadCo != null)
        {
            StopCoroutine(E621_Navigation.act.previewLoadCo);
            E621_Navigation.act.previewLoadCo = null;
        }
        E621_Navigation.act.previewLoadCo = StartCoroutine(GetImage(urlPreview, E621_Navigation.act.imagePreview));
    }

    IEnumerator GetImage(string url, Image target)
    {
        yield return null;
        target.sprite = imgLoading;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                target.sprite = imgError;
                Debug.Log(uwr.error);
            }
            else
            {
                newTexture = DownloadHandlerTexture.GetContent(uwr);
                newSprite = Sprite.Create(newTexture, new Rect(0f, 0f, newTexture.width, newTexture.height), new Vector2(.5f, .5f), 100f);
                target.sprite = newSprite;
            }
        }
        activeCo = null;
    }

    public void OnDestroy()
    {
        Debug.LogAssertion("Destroy button");
        if (activeCo != null) StopCoroutine(activeCo);
    }

    void CheckExistance()
    {
        Thread t = new Thread(new ThreadStart(CheckExistanceThread));
        t.Start();
        buttonKeep.interactable = false;
        buttonFilter.interactable = false;
    }

    void CheckExistanceThread()
    {
        ImageData load = Data.act.imageData.Where(temp => temp.filename.ToLower().Substring(0, temp.filename.IndexOf(".")) == md5).SingleOrDefault();
        print(md5);
        UnityThread.executeInUpdate(() =>
        {
            buttonKeep.interactable = load == null || load.filtered;
            buttonFilter.interactable = load == null || !load.filtered;
        });
    }
}
