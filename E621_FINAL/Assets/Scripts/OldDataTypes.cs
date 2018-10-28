using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Character
{
    public string characterTag;
    public List<string> appearings;
    public List<string> notAppearings;

    public Character()
    {
        characterTag = "";
        appearings = new List<string>();
        notAppearings = new List<string>();
    }
}

[Serializable]
public class Artist
{
    public string artistTag;
    public List<string> appearings;
    public List<string> notAppearings;

    public Artist()
    {
        artistTag = "";
        appearings = new List<string>();
        notAppearings = new List<string>();
    }
}

[Serializable]
public class Series
{
    public string seriesTag;
    public List<string> appearings;
    public List<string> notAppearings;

    public Series()
    {
        seriesTag = "";
        appearings = new List<string>();
        notAppearings = new List<string>();
    }
}

[Serializable]
public class ImageGal
{
    public string filename;
    public int id;
    public List<string> tags;

    public bool artist = false;
    public bool character = false;
    public bool series = false;

    public ImageGal(string filenameGet, int idGet, List<string> tagsGet)
    {
        filename = filenameGet;
        id = idGet;
        tags = tagsGet;
    }
}

[Serializable]
public class ImageGalFilter
{
    public string filename;
    public string url;
    public bool filtered;
}

/*
[Serializable]
public class Data_Filter
{
    public string filename;
    public string url;
    public bool filtered;
}
*/
[Serializable]
public class CharacterData
{
    public Character character;
    public string portraitURL;
    public string portraitUneditedURL;
    public int animatedValue;
    public int qualityValue;
    public int favoriteValue;
    public int fetishValue;

    public bool[] sexes;
    public bool[] species;
    public bool[] attributes;
    public string lastCheck;

    public CharacterData(Character newChar, string sPortrait, string sPortraitUnedited)
    {
        character = newChar;
        portraitURL = sPortrait;
        portraitUneditedURL = sPortraitUnedited;
        animatedValue = 0;
        favoriteValue = 0;
        fetishValue = 0;
        qualityValue = 0;

        /*  sexes--------------------
         *  Female
         *  Dickgirl
         *  --------------------------*/
        sexes = new bool[2];
        /*  species--------------------
         *  Anthro
         *  Mid-Beast
         *  Angel
         *  Demon
         *  Marine
         *  Dragon
         *  Robot
         *  Human
         *  Egyptian
         *  Other
         *  --------------------------*/
        species = new bool[10];
        lastCheck = "Never";


    }
}

[Serializable]
public class ArtistData
{
    public Artist artist;
    public string portraitURL;
    public string portraitUneditedURL;

    public bool[] drawSexes;
    public bool[] attributes;
    public int[] qualifications;


    public ArtistData(Artist newArtist, string sPortrait, string sPortraitUnedited)
    {
        artist = newArtist;
        portraitURL = sPortrait;
        portraitUneditedURL = sPortraitUnedited;
        /* Draw Sexes
         * Female
         * Dickgirl
         * Male
        */
        drawSexes = new bool[3];
        /* Attributes
         * SexyPoses
         * Dirty
         * Weird
         * Exagerated Body
         * Muscular
         * Bestiality
         * Amazing Color Use
         * Known Series
         * Known Characters
         * Favorite
        */
        attributes = new bool[10];
        /* Qualifications
         * Animation
         * Constant Quality/Content
         * Fap to their content
         * Fetish Worth
        */
        qualifications = new int[4];
    }
}

//Galleries
[Serializable]
public class Comic
{
    public string urlComic;
    public string urlComicPortrait;
    public string title;
    public bool read;
    public int status;
    public int quality;
    public int fapable;
    public int fetish;
    public int category;
    public int lastPage;
    public int pages;

    public Comic(string url)
    {
        urlComic = url;
        string[] files = System.IO.Directory.GetFiles(urlComic);
        if (files.Contains(urlComic + @"\desktop.ini"))
        {
            List<string> files2 = files.ToList();
            files2.Remove(urlComic + @"\desktop.ini");
            files = files2.ToArray();
        }
        urlComicPortrait = files[0];
        title = System.IO.Path.GetDirectoryName(files[0]);
        pages = files.Length;
        read = false;
        status = 0;

        /* Status
         * Unknown
         * Incomplete
         * Running
         * Complete
        */
        quality = 0;
        fapable = 0;
        fetish = 0;
        category = 0;
        /* Category
         * Unknown
         * Male
         * Female
         * Dickgirl
         * Female / Male
         * Female / Dickgirl
         * Dickgirl / Male
         * Female / Female
         * Dickgirl / Dickgirl
         * Male / Male
         * Various (Enjoyable)
         * Various (Questionable)
        */
        lastPage = 0;
    }
}