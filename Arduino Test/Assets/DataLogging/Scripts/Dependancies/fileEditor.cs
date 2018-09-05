using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    // Fetch File Contents
    public static string Fetch(string filePath)
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(filePath);
        string allContents = reader.ReadToEnd();
        reader.Close();

        return allContents;
    }

    // Clear a directory
    public static void ClearDir(string path)
    {
        // If the path exists delete every file in it and refresh the unity editor if we're using it
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        Directory.CreateDirectory(path);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    // Delete
    public static void Delete(string path)
    {
        FileUtil.DeleteFileOrDirectory(path);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
