using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class GlobalActions : MonoBehaviour
{
    //GameObject allObjectsPrefab;

    //Advice CTRL!!!
    AdviceComponents adviceComp;
    public delegate void ButtonConfirm();
    ButtonConfirm functionConfirm;
    public delegate void ButtonDeny();
    ButtonDeny functionDeny;

    //Loading CTRL!!!!
    [HideInInspector]
    public LoadingComponents loadingComp;
    [HideInInspector]
    public bool loadingWait;

    // Use this for initialization
    public virtual void Awake()
    {
        loadingWait = false;
        adviceComp = GameObject.FindGameObjectWithTag("Advice").GetComponent<AdviceComponents>();
        loadingComp = GameObject.FindGameObjectWithTag("Loading").GetComponent<LoadingComponents>();
        UnityThread.initUnityThread();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    //-----------------------------------------------------------
    //Advice Controls

    public void ButtonDoConfirm()
    {
        foreach (AdviceBox box in adviceComp.adviceBoxList)
        {
            box.obj.SetActive(false);
        }
        adviceComp.panel.gameObject.SetActive(false);

        functionConfirm();
    }

    public void ButtonDoDeny()
    {
        foreach (AdviceBox box in adviceComp.adviceBoxList)
        {
            box.obj.SetActive(false);
        }
        adviceComp.panel.gameObject.SetActive(false);

        functionDeny();
    }

    public void CreateAdvice(string text, int size = 0, ButtonConfirm actionConfirm = null, ButtonDeny actionDeny = null)
    {
        CreateAdvice("Advice", text, size, actionConfirm, actionDeny);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title">Advice Title</param>
    /// <param name="text">The message of the advice.</param>
    /// <param name="size">(1-3) Defaults to 0.</param>
    /// <param name="actionConfrim">Action when clicking Ok/Yes.</param>
    /// <param name="actionDeny">Action when clicking No. Will force a double button to appear instead of only the button Ok.</param>
    /// 
    public void CreateAdvice(string title, string text, int size = 0, ButtonConfirm actionConfirm = null, ButtonDeny actionDeny = null)
    {
        //Determinar el tamaño de la caja de mensaje
        adviceComp.panel.gameObject.SetActive(true);
        AdviceBox usedBox = adviceComp.adviceBoxList[size];
        usedBox.txtTitle.text = title;
        usedBox.txtAdvice.text = text;
        functionConfirm = actionConfirm;
        functionDeny = actionDeny;

        if(actionConfirm == null)
        {
            usedBox.buttonYes.gameObject.SetActive(false);
            usedBox.buttonNo.gameObject.SetActive(false);
            usedBox.buttonOk.gameObject.SetActive(true);
        }
        else
        {
            usedBox.buttonYes.gameObject.SetActive(true);
            usedBox.buttonNo.gameObject.SetActive(true);
            usedBox.buttonOk.gameObject.SetActive(false);
        }

        usedBox.buttonOk.onClick.RemoveAllListeners();
        usedBox.buttonYes.onClick.RemoveAllListeners();
        usedBox.buttonNo.onClick.RemoveAllListeners();

        if (actionConfirm == null)
            functionConfirm = () => { };
        if (actionDeny == null)
            functionDeny = () => { };

        usedBox.buttonOk.onClick.AddListener(ButtonDoConfirm);
        usedBox.buttonYes.onClick.AddListener(ButtonDoConfirm);
        usedBox.buttonNo.onClick.AddListener(ButtonDoDeny);

        usedBox.obj.SetActive(true);
    }
    //-----------------------------------------------------------

    //-----------------------------------------------------------
    //Loading Controls
    public void LoadingReset(string newTitle)
    {
        loadingComp.slider.value = 0f;
        loadingComp.message.text = newTitle;
        loadingComp.obj.SetActive(true);
    }
    
    public void UpdateLoadingValue(float value)
    {
        loadingComp.slider.value = value;
        //if (value == 1f && !loadingWait) loadingComp.obj.SetActive(false);
    }

    public void StartLoadingWait()
    {
        loadingWait = true;
        StartCoroutine(LoadingWait());
    }

    IEnumerator LoadingWait()
    {
        while (loadingWait)
        {
            loadingComp.slider.value += Time.deltaTime;
            if (loadingComp.slider.value >= 1f) loadingComp.slider.value = 0f;
            yield return null;
        }
        yield return null;
    }
    //-----------------------------------------------------------
}
