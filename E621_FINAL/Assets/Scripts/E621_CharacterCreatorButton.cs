using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System.Threading;

public class E621_CharacterCreatorButton : MonoBehaviour
{
    public Image imagePortrait;
    public Text textNamePlaceholder, textStats;

    public RectTransform transformAttachments;

    public E621CharacterData data;
    Coroutine thisCoroutine = null, imageCountCo;
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
        E621_CharacterCreator.act.queueCharacterStats.Enqueue(this);
        thisCoroutine = StartCoroutine(LoadImage(E621_CharacterCreator.act.inputPortraits.text + @"\" + data.portraitFile + ".png", imagePortrait));
    }

    void Atacchments()
    {
        string s = data.special.ToLower() + " " + data.tagHighlights.ToLower();
        GameObject objAttach = new GameObject();
        Destroy(objAttach);
        Image imageAttach = objAttach.AddComponent<Image>();
        imageAttach.preserveAspect = true;

        if (s.Contains("favorite"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprFavorite;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("egyptian"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprEgyptian;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("dickgirl"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprDickgirl;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("bestiality"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprBeast;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("muscular"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprMuscular;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("size difference") || s.Contains("size_difference") || s.Contains("age difference") || s.Contains("age_difference"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprSizeDiff;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("mature"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprMature;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("demon"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprDemon;
            Instantiate(objAttach, transformAttachments);
        }

        if (s.Contains("fetish"))
        {
            imageAttach.sprite = E621_CharacterCreator.act.sprFetish;
            Instantiate(objAttach, transformAttachments);
        }
    }

    public void StartStatsThread()
    {
        Thread t = new Thread(new ThreadStart(ThreadedStats));
        t.Start();
    }

    volatile bool endThread = false;
    void ThreadedStats()
    {
        try
        {
            while (!endThread)
            {
                int appeared = Data.act.imageData.Count(t => t.tags.Contains(data.tag.Replace("é", @"\u00e9")) && !t.filtered);
                int filtered = Data.act.imageData.Count(t => t.tags.Contains(data.tag.Replace("é", @"\u00e9")) && t.filtered);

                float div = filtered == 0 ? 1 : filtered;
                bool end = false;
                UnityThread.executeInUpdate(() =>
                {
                    if(textStats != null)
                    {
                        textStats.text = "Art: " + appeared + "  Filt: " + filtered + "  Ratio: " + ((Mathf.Round(((float)appeared / div) * 1000f) / 1000f));
                        E621_CharacterCreator.act.activeThreads--;
                    }
                    end = true;
                });
                while (end != false) { }
                endThread = true;
            }
            print("Ended Thread");
        }
        catch
        {
            print("Error in Thread");
            E621_CharacterCreator.act.activeThreads--;
        }
    }
    public void ShowOnPreview()
    {
        if (thisCoroutine != null) return;
        Image target = E621_CharacterCreator.act.objPreview.transform.GetChild(E621_CharacterCreator.act.indexPreviewFull).GetComponent<Image>();
        Image targetIco = E621_CharacterCreator.act.objPreview.transform.GetChild(E621_CharacterCreator.act.indexPreviewIco).GetComponent<Image>();

        thisCoroutine = StartCoroutine(LoadImage(E621_CharacterCreator.act.inputSources.text + @"\" + data.sourceFile, target));
        targetIco.sprite = imagePortrait.sprite;

        E621_CharacterCreator.act.objPreview.SetActive(true);
    }

    IEnumerator LoadImage(string url, Image _target)
    {
        _target.sprite = E621_CharacterCreator.act.imgLoading;
        
        yield return new WaitForSeconds(delay);

        if(!System.IO.File.Exists(url))
        {
            _target.sprite = E621_CharacterCreator.act.imgMissing;
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
                _target.sprite = newSprite;
                textNamePlaceholder.transform.parent.gameObject.SetActive(false);
            }
        }

        thisCoroutine = null;
    }

    private void OnDestroy()
    {
        print("Destroy Button");
        endThread = true;
        StopThisCoroutine();
    }
}
