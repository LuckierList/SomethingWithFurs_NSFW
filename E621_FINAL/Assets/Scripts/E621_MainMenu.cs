using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class E621_MainMenu : GlobalActions
{
    public Image imageOpPortrait;
    public Text textOp;
    public Sprite imgBlank;
    public Sprite imgGallery, imgComic, imgVideo, imgGame, imgCharacter, imgArtist, imgReloadArrows;
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void ButtonOnMouseHover(string type)
    {
        switch (type)
        {
            case "gallery":
                imageOpPortrait.sprite = imgGallery;
                textOp.text = "Opens a Gallery where you can choose to see what kind of content do you want to see on a slideshow and also filter by tags.";
                break;
            case "comic":
                imageOpPortrait.sprite = imgComic;
                textOp.text = "Opens a Gallery to choose a Comic and read it on a Comic Viewer.";
                break;
            case "video":
                imageOpPortrait.sprite = imgVideo;
                textOp.text = "Opens the video Gallery and you can choose what kind of content you want to see.";
                break;
            case "game":
                imageOpPortrait.sprite = imgGame;
                textOp.text = "''I don't even know if this can be implemented in unity.''";
                break;
            case "character":
                imageOpPortrait.sprite = imgCharacter;
                textOp.text = "Opens an editor and viewer of All Character data saved.";
                break;
            case "artist":
                imageOpPortrait.sprite = imgArtist;
                textOp.text = "Opens an editor and viewer of All Artist data saved.";
                break;
            case "reloadData":
                imageOpPortrait.sprite = imgReloadArrows;
                textOp.text = "Re-imports currently saved Data. Will override any changes that haven't been saved.";
                break;
            case "convertOldData":
                imageOpPortrait.sprite = imgReloadArrows;
                textOp.text = "Will convert the old save data format from old .dat files to the current save data system.";
                break;
            default:
                textOp.text = "Something went horribly wrong here...";
                break;
        }

    }

    public void ButtonOnMouseExit()
    {
        textOp.text = "";
        imageOpPortrait.sprite = imgBlank;
    }

    public void ButtonGalleryOption(string type)
    {
        switch (type)
        {
            case "gallery":
                CreateAdvice("The gallery has not been implemented yet.");
                break;
            case "comic":
                CreateAdvice("The comic gallery hasa not been implemented yet.");
                break;
            case "video":
                CreateAdvice("The video gallery has not been implemented yet.");
                break;
            case "game":
                CreateAdvice("The game has not been implemented yet.");
                break;
            case "character":
                CreateAdvice("The character has not been implemented yet.");
                break;
            case "artist":
                CreateAdvice("The artist has not been implemented yet.");
                break;
            case "reloadData":
                CreateAdvice("Warning!", "Are you sure you want to reload all DATA?",0,() => { print("Here a reload should happen..."); });
                break;
            case "convertOldData":
                CreateAdvice("Warning!", "Old saved data may be corrupted by this action. You should make a backup. Continue?", 0, () => { print("Here a conversion should happen..."); });
                break;
            default:

                break;
        }
    }
}
