using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AdviceBox
{
    public GameObject obj;
    public Text txtTitle, txtAdvice;
    public Button buttonOk, buttonYes, buttonNo;
}

public class AdviceComponents : MonoBehaviour
{
    public List<AdviceBox> adviceBoxList;
    public Image panel;

    private void Awake()
    {
        panel = transform.GetChild(0).GetComponent<Image>();
        for(int i = 1; i < transform.childCount; i++)
        {
            AdviceBox newAdv = new AdviceBox();
            newAdv.obj = transform.GetChild(i).gameObject;
            //newAdv.size = i;
            newAdv.txtTitle = newAdv.obj.transform.GetChild(1).gameObject.GetComponent<Text>();
            newAdv.txtAdvice = newAdv.obj.transform.GetChild(2).gameObject.GetComponent<Text>();
            newAdv.buttonOk = newAdv.obj.transform.GetChild(3).gameObject.GetComponent<Button>();
            newAdv.buttonYes = newAdv.obj.transform.GetChild(4).gameObject.GetComponent<Button>();
            newAdv.buttonNo = newAdv.obj.transform.GetChild(5).gameObject.GetComponent<Button>();
            adviceBoxList.Add(newAdv);
        }
    }
}
