using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class E621_CharacterCreatorButton : MonoBehaviour
{
    public Image imagePortrait;
    public Text textNamePlaceholder;

    public RectTransform transformAttachments;
    public Sprite sprFavorite, sprFetish, sprMature, sprBeast, sprDickgirl, sprEgyptian, sprSizeDiff, sprDemon, sprMuscular;

    public E621CharacterData data;
    Coroutine thisCoroutine = null;
    [HideInInspector]
    public float delay;

    Texture2D newTexture;
    Sprite newSprite;

    public void ButtonEdit()
    {
        E621_CharacterCreator.act.CreatorExistent(data);
    }

    public void StopThisCoroutine()
    {
        if (thisCoroutine != null)
            StopCoroutine(thisCoroutine);
    }

    public void LoadImageFunc()
    {
        StopThisCoroutine();
        textNamePlaceholder.text = data.name;
        Atacchments();
        thisCoroutine = StartCoroutine(LoadImage(E621_CharacterCreator.act.inputPortraits.text + @"\" + data.portraitFile + ".png"));
    }

    void Atacchments()
    {
        string s = data.special.ToLower() + " " + data.tagHighlights.ToLower();
        GameObject objAttach = new GameObject();
        Image imageAttach = objAttach.AddComponent<Image>();
        imageAttach.preserveAspect = true;

        if (s.Contains("favorite"))
        {
            imageAttach.sprite = sprFavorite;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("egyptian"))
        {
            imageAttach.sprite = sprEgyptian;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("dickgirl"))
        {
            imageAttach.sprite = sprDickgirl;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("bestiality"))
        {
            imageAttach.sprite = sprBeast;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("muscular"))
        {
            imageAttach.sprite = sprMuscular;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("size difference") || s.Contains("size_difference") || s.Contains("age difference") || s.Contains("age_difference"))
        {
            imageAttach.sprite = sprSizeDiff;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("mature"))
        {
            imageAttach.sprite = sprMature;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("demon"))
        {
            imageAttach.sprite = sprDemon;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("fetish"))
        {
            imageAttach.sprite = sprFetish;
            Instantiate(objAttach, transformAttachments);
        }
    }

    IEnumerator LoadImage(string url)
    {
        imagePortrait.sprite = E621_CharacterCreator.act.imgLoading;
        
        yield return new WaitForSeconds(delay);

        if(!System.IO.File.Exists(url))
        {
            imagePortrait.sprite = E621_CharacterCreator.act.imgMissing;
            yield break;
        }

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
                imagePortrait.sprite = newSprite;
                textNamePlaceholder.transform.parent.gameObject.SetActive(false);
            }
        }

        thisCoroutine = null;
    }

    private void OnDestroy()
    {
        StopThisCoroutine();
    }
}
