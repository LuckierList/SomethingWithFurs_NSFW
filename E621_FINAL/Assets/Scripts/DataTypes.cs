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
public class E621CharacterData
{
    public string tag;
    public string name;
    public string sourceFile;
    public string portraitFile;
    public string tagHighlights;
    public string special;

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