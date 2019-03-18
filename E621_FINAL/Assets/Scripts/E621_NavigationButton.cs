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
    public Image imageExistance, imagePreview;

    Sprite newSprite;
    Texture2D newTexture;

    
	// Use this for initialization
    
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
