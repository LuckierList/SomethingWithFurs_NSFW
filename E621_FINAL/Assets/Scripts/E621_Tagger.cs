using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;
using System.Linq;


public class E621_Tagger : GlobalActions
{
    public InputField inputStraightNew, inputDickgirlNew, inputIntersexNew, inputHermNew, inputStartIndexTag;
    public Button buttonStartTag, buttonStopTag;
    public Dropdown dropSourceTags;
    public Text textLastGalIndex, textCurrentAction, textLog, textFileCount;
    public Toggle toggleUpdateTags;
    public Scrollbar scrollLog;
    public Button buttonReturn, buttonSave, buttonMoveFolder, buttonConfig, buttonGallery, buttonFilterer;
    public InputField inputStraightGal, inputDickgirlGal;
    bool onTagging;
    Coroutine currentCo;
    string[] filesOnSource;
    string sourceUrl;
    List<string> listLog = new List<string>();

    string moveFolderUrlSource = "";
    string moveFolderUrlDestiny = "";

    //for the textLastGalIndex
    static string lastGalIndexFormat = "Last Index:\n{0}\n{1}";
    int lastIndex;
    int lastSource;


    // Use this for initialization
    void Start()
    {
        SetSourcesToDefault();
        GetDefaultTaggerValues();
        onTagging = false;
    }

    // Update is called once per frame
    void Update()
    {
        ButtonStartStopBehaviour();
        UpdateCurrentActionText();
        UpdateLastGalIndex();
    }

    //-------------------------------------------------------------------
    //Last Gallery and Index
    void UpdateLastGalIndex()
    {
        textLastGalIndex.text = string.Format(lastGalIndexFormat, lastIndex, dropSourceTags.options[lastSource].text);
    }

    public void GetDefaultTaggerValues()
    {
        lastIndex = PlayerPrefs.GetInt("TaggerLastIndex", 0);
        lastSource = PlayerPrefs.GetInt("TaggerLastSource", 0);
        inputStartIndexTag.text = "" + lastIndex;
    }

    public void SetDefaultTaggerValues()
    {
        PlayerPrefs.SetInt("TaggerLastIndex", lastIndex);
        PlayerPrefs.SetInt("TaggerLastSource", lastSource);
    }
    //-------------------------------------------------------------------

    //-------------------------------------------------------------------
    //Source handler
    public void DropSource(int source)
    {
        
        switch (source)
        {
            case 0:
                sourceUrl = "";
                textFileCount.text = "Files on directory:\n0";
                return;
            case 1://straight new
                sourceUrl = inputStraightNew.text;
                break;
            case 2:
                sourceUrl = inputDickgirlNew.text;
                break;
            case 3:
                sourceUrl = inputIntersexNew.text;
                break;
            case 4:
                sourceUrl = inputHermNew.text;
                break;
        }

        //Si hay error, finalizar, sino ejecutar thread.
        if (!Directory.Exists(sourceUrl))
        {
            CreateAdvice("The URL of the source does not exist! Check it in configuration.");
            textFileCount.text = "Files on directory:\n0";
            dropSourceTags.value = 0;
            dropSourceTags.RefreshShownValue();
            return;
        }

        //thread
        LoadingReset("Loading the file list! This may take a while...");
        StartLoadingWait();

        Thread t = new Thread(new ThreadStart(ThreadedSourceFiles));
        t.Start();

    }

    void ThreadedSourceFiles()
    {
        string[] filesThread = Directory.GetFiles(sourceUrl);
        UnityThread.executeInUpdate(() =>
        {
            filesOnSource = filesThread;
            textFileCount.text = "Files on directory:\n" + filesOnSource.Length;
            loadingComp.obj.SetActive(false);
        });
    }

    public void SetSourcesToDefault()
    {
        inputStraightNew.text = PlayerPrefs.GetString("TaggerStraightNew");
        inputDickgirlNew.text = PlayerPrefs.GetString("TaggerDickgirlNew");
        inputIntersexNew.text = PlayerPrefs.GetString("TaggerIntersexNew");
        inputHermNew.text = PlayerPrefs.GetString("TaggerHermNew");

        inputStraightGal.text = PlayerPrefs.GetString("E621_StraightMainGal");
        inputDickgirlGal.text = PlayerPrefs.GetString("E621_DickgirlMainGal");
    }

    //-------------------------------------------------------------------

    void ButtonStartStopBehaviour()
    {
        if (onTagging)
        {
            buttonStartTag.interactable = false;
            buttonStopTag.interactable = true;
        }
        else
        {
            if (Directory.Exists(sourceUrl))
                buttonStartTag.interactable = true;
            else
                buttonStartTag.interactable = false;
            buttonStopTag.interactable = false;
            
        }

        toggleUpdateTags.interactable = !onTagging;
        buttonReturn.interactable = !onTagging;
        buttonSave.interactable = !onTagging;
        dropSourceTags.interactable = !onTagging;
        buttonConfig.interactable = !onTagging;
        buttonMoveFolder.interactable = !onTagging;
        buttonGallery.interactable = !onTagging;
        buttonFilterer.interactable = !onTagging;

    }

    void UpdateCurrentActionText()
    {
        if (!onTagging)
        {
            textCurrentAction.text = "Current: Nothing";
        }
        else if (onTagging)
        {
            textCurrentAction.text = "Current: Tagging";
        }
    }

    public void ButtonMoveToFolder()
    {
        if(dropSourceTags.value == 0)
        {
            CreateAdvice("Not available.");
            return;
        }
        Thread t = new Thread(new ThreadStart(MoveToFolderThread));
        if (dropSourceTags.value == 1)
        {
            moveFolderUrlSource = inputStraightNew.text;
            moveFolderUrlDestiny = inputStraightGal.text;
        }
        else
        {
            switch(dropSourceTags.value)
            {
                case 2:
                    moveFolderUrlSource = inputDickgirlNew.text;
                    break;
                case 3:
                    moveFolderUrlSource = inputStraightNew.text;
                    break;
                case 4:
                    moveFolderUrlSource = inputHermNew.text;
                    break;
            }
            moveFolderUrlDestiny = inputDickgirlGal.text;
        }
        //1--
        CreateAdvice("Currently moving: " + dropSourceTags.options[dropSourceTags.value].text + "\nTarget: " + Path.GetDirectoryName(moveFolderUrlDestiny) + "nContinue?", 0, () =>
        {
            if (Directory.Exists(moveFolderUrlDestiny) && Directory.Exists(moveFolderUrlSource))
            {
                t.Start();
            }
            else
            {
                CreateAdvice("A url didn't exist, please check it!");
            }
        });
        //1--

        
    }

    void MoveToFolderThread()
    {
        UnityThread.executeInUpdate(() =>
        {
            LoadingReset("Getting the list of the source folder...");
            StartLoadingWait();
        });
        string[] sourceUrls = Directory.GetFiles(moveFolderUrlSource);

        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Moving the images to the destiny folder!");
        });

        float cont = 0f;
        foreach(string s in sourceUrls)
        {
            try
            {
                Directory.Move(s, moveFolderUrlDestiny + @"\" + Path.GetFileName(s));
            }
            catch (System.Exception)
            {

            }
            cont++;
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(cont/sourceUrls.Length);
            });
        }

        UnityThread.executeInUpdate(() =>
        {
            LoadingReset("Getting the list of the filtered folder...");
            //print(moveFolderUrlSource.Replace("Good", "Filtered"));
            StartLoadingWait();
        });

        sourceUrls = Directory.GetFiles(moveFolderUrlSource.Replace("Good", "Filtered"));

        UnityThread.executeInUpdate(() =>
        {
            loadingWait = false;
            LoadingReset("Deleting the filtered folder images...");
        });

        foreach (string s in sourceUrls)
        {
            try
            {
               File.Delete(s);
            }
            catch (System.Exception)
            {

            }
            cont++;
            UnityThread.executeInUpdate(() =>
            {
                UpdateLoadingValue(cont / sourceUrls.Length);
            });
        }

        UnityThread.executeInUpdate(() =>
        {
            loadingComp.obj.SetActive(false);
            CreateAdvice("Done");
        });
    }

    public void ButtonActions(string value)
    {
        switch (value)
        {
            case "startTag":
                currentCo = StartCoroutine(TagSet());

                inputStartIndexTag.interactable = false;
                onTagging = true;

                break;
            case "stopTag":
                StopCoroutine(currentCo);
                currentCo = null;
                AddLog("Tagging ended by user!");
                TaggingEnd();
                break;
            case "configApply":
                string message = "This Urls are correct and ready to use:\n";

                if (Directory.Exists(inputStraightNew.text))
                {
                    PlayerPrefs.SetString("TaggerStraightNew", inputStraightNew.text);
                    message += "Straigth (New)\n";
                }

                if (Directory.Exists(inputDickgirlNew.text))
                {
                    PlayerPrefs.SetString("TaggerDickgirlNew", inputDickgirlNew.text);
                    message += "Dickgirl (New)\n";
                }

                if (Directory.Exists(inputIntersexNew.text))
                {
                    PlayerPrefs.SetString("TaggerIntersexNew", inputIntersexNew.text);
                    message += "Intersex (New)\n";
                }

                if (Directory.Exists(inputHermNew.text))
                {
                    PlayerPrefs.SetString("TaggerHermNew", inputHermNew.text);
                    message += "Herm (New)\n";
                }

                if (Directory.Exists(inputStraightGal.text))
                {
                    PlayerPrefs.SetString("E621_StraightMainGal", inputStraightGal.text);
                    message += "Straight Gal\n";

                }

                if (Directory.Exists(inputDickgirlGal.text))
                {
                    PlayerPrefs.SetString("E621_DickgirlMainGal", inputStraightGal.text);
                    message += "Dickgirl Gal\n";

                }

                message += "(If nothing shows, then nothing works....)";
                CreateAdvice(message, 1);
                break;
            case "save":
                CreateAdvice("Are you sure you want to Override ImageData.DATA?", 0,
                    () =>
                    {
                        Data.act.SaveData("imageData");
                        CreateAdvice("'ImageData.DATA' succesfully Overwritten!");
                    });
                break;
            case "return":
                OpenSceneAsync("mainMenu");
                break;
        }
    }

    public void InputStartIndexTag(string s)
    {
        if (s == "" || int.Parse(s) < 0) inputStartIndexTag.text = "0";
    }


    WWW www;

    IEnumerator TagSet()
    {
        yield return null;
        int startIndex = int.Parse(inputStartIndexTag.text);
        lastSource = dropSourceTags.value;
        SetDefaultTaggerValues();
        AddLog("Started Tagging from index no.: " + startIndex);
        for (int i = startIndex; i < filesOnSource.Length; i++)
        {
            inputStartIndexTag.text = "" + i;
            lastIndex = i;
            yield return null;
            string filename = Path.GetFileName(filesOnSource[i]);
            string filenameNoExtension = Path.GetFileNameWithoutExtension(filesOnSource[i]);

            if(filenameNoExtension.Contains("-"))
            {
                filenameNoExtension = filenameNoExtension.Substring(filenameNoExtension.IndexOf("-") + 1, filenameNoExtension.Length - (filenameNoExtension.IndexOf("-") + 1));
            }

            ImageData loaded = Data.act.imageData.Where(temp => temp.filename == filename).SingleOrDefault();
            int indexImage = -1;
            if(loaded != null)
            {
                indexImage = Data.act.imageData.IndexOf(loaded);
            }

            if(Data.act.imageData[indexImage].tags.Count != 0 && !toggleUpdateTags.isOn)
            {
                AddLog("Skipped Tagged Image: " + Data.act.imageData[indexImage].filename);
                continue;
            }
            //cargando pagina
            AddLog("Opening E621 page...");
            www = new WWW("https://e621.net/post/index/1/md5:" + filenameNoExtension);
            yield return www;
            string page = www.text;
            www.Dispose();
            page = page.Substring(page.IndexOf("Cookie.setup();"), page.Length - page.IndexOf("Cookie.setup();"));
            page = page.Substring(0, page.IndexOf("Post.init_blacklisted();"));

            if(page.IndexOf("tags") == -1)
            {
                //si la imagen no existe, determinar si fué borrada
                AddLog("Opening E621 page (DELETED)...");
                www = new WWW("https://e621.net/post/index/1/md5:" + filenameNoExtension + "%20status:deleted");
                yield return www;
                page = www.text;
                www.Dispose();
                page = page.Substring(page.IndexOf("Cookie.setup();"), page.Length - page.IndexOf("Cookie.setup();"));
                page = page.Substring(0, page.IndexOf("Post.init_blacklisted();"));
                if(page.IndexOf("tags") == -1)
                {
                    //Si la imagen no existe, igual agregala
                    List<string> noTags = new List<string>();
                    noTags.Add("unexistent");
                    ImageData newData = new ImageData("E621", -1, filename, false);
                    newData.tags = noTags;
                    Data.act.imageData.Add(newData);
                    AddLog("Added Unexistent Image: " + filename);
                    continue;
                }
            }

            //ObtenerID

            string imageIDs = page.Substring(page.IndexOf("id") + 4, page.Length - (page.IndexOf("id") + 4));
            //print(imageIDs);
            imageIDs = imageIDs.Substring(0, imageIDs.IndexOf(","));
            //print(imageIDs);
            int imageID = int.Parse(imageIDs);

            //Obtener tags
            List<string> tags = new List<string>();
            page = page.Substring(page.IndexOf("tags") + 7, page.Length - (page.IndexOf("tags") + 7));
            page = page.Substring(0, page.IndexOf(",") - 1);
            while(page.IndexOf(" ") != -1)
            {
                yield return null;
                tags.Add(page.Substring(0, page.IndexOf(" ")));
                page = page.Substring(page.IndexOf(" ") + 1, page.Length - (page.IndexOf(" ") + 1));
            }

            //Si la imagen ya existe, agraegarle los nuevos tags
            if(indexImage != -1)
            {
                AddLog("Added Tags for Existent Image: " + filename);
                Data.act.imageData[indexImage].tags = tags;
                continue;
            }

            AddLog("Added Tags for Unexistent Image: " + filename);
            ImageData newImage = new ImageData("E621", imageID, filename, false);
            newImage.tags = tags;
            Data.act.imageData.Add(newImage);

        }
        AddLog("Tagging ended!");
        //Finalizar busqueda
        currentCo = null;
        TaggingEnd();
    }

    void TaggingEnd()
    {
        SetDefaultTaggerValues();
        inputStartIndexTag.text = "0";
        inputStartIndexTag.interactable = true;
        onTagging = false;
    }

    void AddLog(string newLog)
    {
        if (listLog.Count > 224)
        {
            listLog.RemoveAt(0);
        }
        listLog.Add(newLog);
        
        textLog.text = "Start of the log:";
        foreach(string s in listLog)
        {
            textLog.text += "\n" + s + "\n";
        }
        scrollLog.value = 0;
    }
}
