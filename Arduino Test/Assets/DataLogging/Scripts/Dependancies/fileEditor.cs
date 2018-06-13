using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class fileEditor {

    //// FILE OPERATIONS ////
    // Clear the file
    public static void Clear(string filePath)
    {
        // Clear file
        File.WriteAllText(filePath, "");
    }

    // Write a line
    public static void Append(string filePath, string text)
    {
        //Write some text to the Report.txt file
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.WriteLine(text);
        writer.Close();
    }

    // Overwrite text
    public static void Write(string filePath, string text)
    {
        //Write some text to the Report.txt file
        StreamWriter writer = new StreamWriter(filePath, true);
        writer.Write(text);
        writer.Close();
    }

#if UNITY_EDITOR
    // Update the inspector
    public static void UpdateEditor(string filePath, string fileName)
    {
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(filePath);
        TextAsset asset = Resources.Load(fileName) as TextAsset;
    }
#endif

    // Read to console
    public static void Read(string filePath)
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(filePath);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
