using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Linq;

public class E621_GalleryButton : MonoBehaviour
{
    public Text textName;
    Coroutine thisCoroutine;
    public Image imageThumb;
    [HideInInspector]
    public float delay;
    [HideInInspector]
    public string url;
    public Sprite imgLoading, imgError;
    Texture2D newTexture;
    Sprite newSprite;
    [HideInInspector]
    public Image imageShower;

    private void Start()
    {
        thisCoroutine = StartCoroutine(LoadImage(url));
    }

    public void ButtonAction()
    {
        string allTags = "";

        ImageData data = Data.act.imageData.Where(tempo => tempo.filename == Path.GetFileName(url)).SingleOrDefault();
        if(data == null)
        {
            GlobalActions.act.CreateAdvice("Image data doesn't exist!");
            return;
        }
        foreach (string s in data.tags)
        {
            allTags += s + "  ";
        }
        GlobalActions.act.CreateAdvice("The tags of this image are: ", allTags, 2);
    }

    public void ButtonShowImage()
    {
        imageShower.sprite = newSprite;
        imageShower.gameObject.SetActive(true);
    }

    public void ButtonOpenInPage()
    {
        GlobalActions.act.OpenInPageE621(url);
    }

    public void StopThisCoroutine()
    {
        if (thisCoroutine != null)
            StopCoroutine(thisCoroutine);
    }

    IEnumerator LoadImage(string url)
    {
        imageThumb.sprite = imgLoading;
        yield return new WaitForSeconds(delay);
        if (!File.Exists(url))
        {
            GetComponent<Button>().interactable = false;
            imageThumb.sprite = imgError;
        }
        else
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + url))
            {
                yield return uwr.SendWebRequest();
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.Log(uwr.error);
                }
                else
                {
                    newTexture = DownloadHandlerTexture.GetContent(uwr);
                    newSprite = Sprite.Create(newTexture, new Rect(0f, 0f, newTexture.width, newTexture.height), new Vector2(.5f, .5f), 100f);
                    imageThumb.sprite = newSprite;
                }
            }
        }
        thisCoroutine = null;
    }


}
