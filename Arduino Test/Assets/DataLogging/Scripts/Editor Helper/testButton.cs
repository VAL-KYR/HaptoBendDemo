using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

//// INSPECTOR GUI ////
[CustomEditor(typeof(dataRecordingController))]
public class testButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        dataRecordingController dataRecControl = (dataRecordingController)target;
        if (GUILayout.Button("Create Regular Test"))
        {
            dataRecControl.NewTest(false);
        }
        if (GUILayout.Button("Create Zero Test"))
        {
            dataRecControl.NewTest(true);
        }
        
        if (GUILayout.Button("New Dock Shape"))
        {
            dataRecControl.NewDockShape();
        }
        if (GUILayout.Button("Zero Dock Shape"))
        {
            dataRecControl.ZeroDockShape();
        }

        if (GUILayout.Button("Summarize Results (Final Report)"))
        {
            dataRecControl.FinalReport();
        }

        if (GUILayout.Button("Delete All Tests"))
        {
            dataRecControl.ClearTests();
        }
        if (GUILayout.Button("Delete All Reports"))
        {
            fileEditor.ClearDir(dataRecControl.filePath);
        }
    }
}

#endif