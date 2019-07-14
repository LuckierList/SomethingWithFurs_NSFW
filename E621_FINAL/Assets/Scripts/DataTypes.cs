using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class ImageData
{
    public string sourceName;
    public int id;
    public List<string> tags;

    public string filename;
    public bool filtered;

    /// <summary>
    /// Create new data of type ImageData.
    /// </summary>
    /// <param name="_source">Whether is E621, R34, or other.</param>
    /// <param name="_id">The id of the image on its proper format.</param>
    /// <param name="_tags">The list of the tags that identify this image's content.</param>
    /// <param name="_filename">Name and extension of the file.</param>
    /// <param name="_filtered">Whether the image is filtered or not.</param>
    public ImageData(string _source, int _id, string _filename, bool _filtered)
    {
        sourceName = _source;
        id = _id;
        tags = new List<string>();
        filename = _filename;
        filtered = _filtered;
    }
}

[Serializable]
public class FileData
{

    [SerializeField] private int id;
    [SerializeField] private string md5;
    [SerializeField] private int[] tags;
    [SerializeField] private string filename;
    [SerializeField] private string format;
    [SerializeField] private bool filtered;
    public float gifVel = 1f;
    [SerializeField] private int stars = 0;

    private string urlFull, urlThumb, urlPreview, rating, status;

    private DateTime lastCheck;

    public FileData(int _id, string _md5, string _filename, string _format, bool _filtered, int[] _tags, string _full, string _thumb, string _preview, string _rating, string _status)
    {
        id = _id;
        md5 = _md5;
        filename = _filename;
        format = _format;
        filtered = _filtered;
        tags = _tags;

        urlFull = _full;
        urlThumb = _thumb;
        urlPreview = _preview;
        rating = _rating;
        status = _status;

        lastCheck = DateTime.Now;
    }
    #region Public Vars
    public int Id
    {
        get
        {
            return id;
        }

        set
        {
            id = value;
            lastCheck = DateTime.Now;
        }
    }

    public string Md5
    {
        get
        {
            return md5;
        }

        set
        {
            md5 = value;
            lastCheck = DateTime.Now;
        }
    }

    public int[] Tags
    {
        get
        {
            return tags;
        }

        set
        {
            tags = value;
            lastCheck = DateTime.Now;
        }
    }

    public string Filename
    {
        get
        {
            return filename;
        }

        set
        {
            filename = value;
            lastCheck = DateTime.Now;
        }
    }

    public string Format
    {
        get
        {
            return format;
        }

        set
        {
            format = value;
            lastCheck = DateTime.Now;
        }
    }

    public bool Filtered
    {
        get
        {
            return filtered;
        }

        set
        {
            filtered = value;
            lastCheck = DateTime.Now;
        }
    }

    public int Stars
    {
        get
        {
            return stars;
        }

        set
        {
            stars = value;
            lastCheck = DateTime.Now;
        }
    }

    public string UrlFull
    {
        get
        {
            return urlFull;
        }

        set
        {
            urlFull = value;
            lastCheck = DateTime.Now;
        }
    }

    public string UrlThumb
    {
        get
        {
            return urlThumb;
        }

        set
        {
            urlThumb = value;
            lastCheck = DateTime.Now;
        }
    }

    public string UrlPreview
    {
        get
        {
            return urlPreview;
        }

        set
        {
            urlPreview = value;
            lastCheck = DateTime.Now;
        }
    }

    public string Rating
    {
        get
        {
            return rating;
        }

        set
        {
            rating = value;
            lastCheck = DateTime.Now;
        }
    }

    public string Status
    {
        get
        {
            return status;
        }

        set
        {
            status = value;
            lastCheck = DateTime.Now;
        }
    }
    #endregion
}

[Serializable]
public class TagData
{
    public List<string> all;
    public List<int> character, artist, specific, blacklist, series;

    public TagData()
    {
        all = new List<string>();
        character = new List<int>();
        artist = new List<int>();
        series = new List<int>();
        specific = new List<int>();
        blacklist = new List<int>();
    }
}

[Serializable]
public class E621CharacterData
{
    public string tag;
    public string name;
    public string sourceFile;
    public string portraitFile;
    public string tagHighlights;
    public string special;

    public int timesSearched = 0;

    public float pMaxScale = 3f, pScale = 1f, pMaxOffX = 300f, pOffX = 0f, pMaxOffY = 300f, pOffY = 0f;

    public void SetData(string _tag, string _name, string _sFile, string _pFile, string _tagHighlights, string _special)
    {
        tag = _tag;
        name = _name;
        sourceFile = _sFile;
        portraitFile = _pFile;
        tagHighlights = _tagHighlights;
        special = _special;
    }

    public void SetPortraitData(float _maxScale, float _scale, float _maxOffX, float _offX, float _maxOffY, float _offY)
    {
        pMaxScale = _maxScale;
        pScale = _scale;
        pMaxOffX = _maxOffX;
        pOffX = _offX;
        pMaxOffY = _maxOffY;
        pOffY = _offY;
    }

    /* Old
    public string tagName;
    public string urlSmall;
    public string urlBig = "";
    public bool[] booleans = new bool[13];
    public int animated = 0, fetish = 0, quality = 0;
    public bool edited = false;

    public E621CharacterData(string _tagName, string _urlSource)
    {
        tagName = _tagName;
        urlSmall = _urlSource;
    }
    */
}