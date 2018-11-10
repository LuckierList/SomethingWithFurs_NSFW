using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using UnityEngine.Networking;

public class E621_CharacterButton : MonoBehaviour
{
    //public string tagChar;
    public int id;
    public string url;
    public bool edited = false;
    public Image imageThumb, imageStatus;
    public Text textButton;
    public Color colorUnedited, colorEdited;
    [HideInInspector]
    public float delay;
    public Sprite imgLoading, imgError;
    Texture2D newTexture;
    Sprite newSprite;
    public E621CharacterData data;
    public Coroutine thisCoroutine;
	// Use this for initialization
	void Awake()
    {
        if (edited)
            imageStatus.color = colorEdited;
        else
            imageStatus.color = colorUnedited;
	}

    private void Start()
    {
        thisCoroutine = StartCoroutine(LoadImage(url));
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void SendData()
    {
        print("oof button");
        E621_Characters.act.OpenInEditor(data, id, imageThumb.sprite);
    }

    public void StopThisCoroutine()
    {
        if(thisCoroutine != null)
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
