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
    /// <param name="_id">The id of the image on his proper format.</param>
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
}