using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fileMetaData {
    public string name;
    public string extension;
    public string path;
    public string destination;

    public fileMetaData()
    {
        destination = path + "/" + name + "." + extension;
    }
}