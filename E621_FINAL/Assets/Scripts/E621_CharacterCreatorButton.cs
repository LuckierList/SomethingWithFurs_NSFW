using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class E621_CharacterCreatorButton : MonoBehaviour
{
    public Image imagePortrait;
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
        thisCoroutine = StartCoroutine(LoadImage(E621_CharacterCreator.act.inputPortraits.text + @"\" + data.portraitFile + ".png"));
    }

    IEnumerator LoadImage(string url)
    {
        imagePortrait.sprite = E621_CharacterCreator.act.imgLoading;
        yield return new WaitForSeconds(delay);
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
                    imagePortrait.sprite = newSprite;
                }
            }

            thisCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        StopThisCoroutine();
    }
}
