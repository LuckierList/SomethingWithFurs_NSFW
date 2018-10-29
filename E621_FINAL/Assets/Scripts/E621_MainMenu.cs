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
    public Sprite imgGallery, imgComic, imgVideo, imgGame, imgCharacter, imgArtist, imgReloadArrows, imgFilter, imgPage, imgDatabase, imgExit;

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
            case "filter":
                imageOpPortrait.sprite = imgFilter;
                textOp.text = "Opens the filterer, here you can check downloaded images/videos and choose which ones you want to keep.";
                break;
            case "page":
                imageOpPortrait.sprite = imgPage;
                textOp.text = "Go to the E621.net/posts page.";
                break;
            case "database":
                imageOpPortrait.sprite = imgDatabase;
                textOp.text = "Opens an editor to add Characters/Artists tags. Also add tags to images you'll add to the Gallery.";
                break;
            case "saveAll":
                textOp.text = "Overwrite all current save data with the changes done in this session. EVERYTHING will be overwriten.";
                break;
            case "reloadData":
                imageOpPortrait.sprite = imgReloadArrows;
                textOp.text = "Re-imports currently saved Data. Will override any changes that haven't been saved.";
                break;
            case "convertOldData":
                imageOpPortrait.sprite = imgReloadArrows;
                textOp.text = "Will convert the old save data format from old .dat files to the current save data system.";
                break;
            case "return":
                imageOpPortrait.sprite = imgExit;
                textOp.text = "Return to the Title Screen.";
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
                CreateAdvice("The comic gallery has not been implemented yet.");
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
            case "filter":
                //CreateAdvice("The filterer has not been implemented yet.");
                SceneManager.LoadSceneAsync("E621_Filterer");
                break;
            case "page":
                Application.OpenURL("https://e621.net/post");
                break;
            case "database":
                CreateAdvice("The database editor has not been implemented yet.");
                break;
            case "saveAll":
                CreateAdvice("Warning!", "Data loss is a risk if you are not sure if you need to save ALL the types of data. It's recommended to go to that section you want to save and do it from there. Still, continue?", 1, Data.act.SaveAllData);
                break;
            case "reloadData":
                CreateAdvice("Warning!", "Are you sure you want to reload all DATA?", 0 ,Data.act.ReloadAllData);
                break;
            case "convertOldData":
                CreateAdvice("Warning!", "Old saved data may be corrupted by this action. You should make a backup. Continue?", 0, () => { print("Here a conversion should happen..."); });
                break;
            case "return":
                Application.Quit();
                print("Quit");
                break;
            default:
                CreateAdvice("What the...?", "Something went horribly wrong here...");
                break;
        }
    }
}
