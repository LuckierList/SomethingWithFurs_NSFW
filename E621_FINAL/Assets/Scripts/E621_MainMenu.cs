using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Linq;

public class E621_MainMenu : GlobalActions
{
    public Image imageOpPortrait;
    public Text textOp;
    public Sprite imgBlank;
    public Sprite imgGallery, imgComic, imgVideo, imgGame, imgCharacter, imgArtist, imgReloadArrows, imgFilter, imgPage, imgDatabase, imgExit;
    public Dropdown dropResolutions;

    private void Start()
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
                textOp.text = "Add tags to images you'll add to the Gallery. Also pens an editor to add Characters/Artists tags.";
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

    public void DropResolution(int value)
    {
        switch(value)
        {
            case 0:
                Screen.SetResolution(1920, 1080, true);
                break;
            case 1:
                Screen.SetResolution(1280, 720, false);
                break;
        }
    }

    public void ButtonGalleryOption(string type)
    {
        switch (type)
        {
            case "gallery":
            case "comic":
            case "video":
            case "game":
            case "character":
            case "artist":
            case "filter":
            case "database":
                OpenSceneAsync(type);
                break;
            case "page":
                Application.OpenURL("https://e621.net/post");
                break;
            case "saveAll":
                CreateAdvice("Warning!", "Data loss is a risk if you are not sure if you need to save ALL the types of data. It's recommended to go to that section you want to save and do it from there. Still, continue?", 1, Data.act.SaveAllData);
                break;
            case "reloadData":
                CreateAdvice("Warning!", "Are you sure you want to reload all DATA?", 0 ,Data.act.ReloadAllData);
                break;
            case "convertOldData":
                CreateAdvice("Warning!", "Old saved data may be corrupted by this action. You should make a backup. Continue?", 0, ConvertOldData);
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

    public void ConvertOldData()
    {
        if (File.Exists(Application.persistentDataPath + "/OldSaves/Images.dat") && File.Exists(Application.persistentDataPath + "/OldSaves/ImagesFilter.dat"))
        {

            LoadingReset("Loading the files (Images.dat)...");
            StartLoadingWait();
            Data.act.imageData.Clear();
            Thread t = new Thread(new ThreadStart(ConvertOldDataThread));
            t.Start();
        }
        else CreateAdvice("Failed to load Images.dat and ImagesFilter.dat");
    }

    void ConvertOldDataThread()
    {
        bool done = false;
        float cont = 0f;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file1 = null;
        FileStream file2 = null;

        List<ImageGal> imageGal;
        List<ImageGalFilter> imageFilter;

        

        UnityThread.executeInUpdate(
            () =>
            {
                //open 1
                file1 = File.Open(Application.persistentDataPath + "/OldSaves/Images.dat", FileMode.Open);
                
                //open2
                file2 = File.Open(Application.persistentDataPath + "/OldSaves/ImagesFilter.dat", FileMode.Open);
                
                //loadingWait = false;
                //loadingComp.obj.SetActive(false);
                //print("Done");
                done = true;
            });
        while (!done) { }
        done = false;
        imageGal = (List<ImageGal>)bf.Deserialize(file1);
        file1.Close();
        UnityThread.executeInUpdate(
            () =>
            {
                LoadingReset("Loading the files (ImagesFilter.dat)...");
            });
        imageFilter = (List<ImageGalFilter>)bf.Deserialize(file2);
        file2.Close();

        //When loaded, check the filtered data, then check if there is data for this image in image data.
        foreach(ImageGalFilter img in imageFilter)
        {
            ImageData newData = null;
            UnityThread.executeInUpdate(() =>
            {
                loadingWait = false;
                UpdateLoadingValue(cont/imageFilter.Count,"Converting old save data. ( " + (cont + 1) + " / " + imageFilter.Count + " )");
                int newID = 0;
                if (img.filename.Contains("-"))
                {
                    string _newID = img.filename.Substring(0, img.filename.IndexOf("-"));
                    newID = int.Parse(_newID);
                }
                newData = new ImageData("E621", newID, img.filename, img.filtered);
                
                done = true;
            });
            while (!done) { }
            done = false;

            ImageGal imgData = imageGal.Where(temp => temp.filename == newData.filename).SingleOrDefault();
            if(imgData != null)
            {
                newData.tags = imgData.tags;
            }
            UnityThread.executeInUpdate(()=>
            {
                Data.act.imageData.Add(newData);
                cont++;
                done = true;
            });
            while (!done) { }
            done = false;
        }
        UnityThread.executeInUpdate(
            () =>
            {
                loadingWait = false;
                loadingComp.obj.SetActive(false);
                print("Done");
                done = true;
            });
        while (!done) { }
        done = false;
    }
}
