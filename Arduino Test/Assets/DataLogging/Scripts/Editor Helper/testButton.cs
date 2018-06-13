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
        if (GUILayout.Button("Create Test"))
        {
            dataRecControl.newTest();
        }
        
        if (GUILayout.Button("New Dock Shape"))
        {
            dataRecControl.newDockShape();
        }
        if (GUILayout.Button("Zero Dock Shape"))
        {
            dataRecControl.zeroDockShape();
        }

        if (GUILayout.Button("Summarize Results (Final Report)"))
        {
            dataRecControl.finalReport();
        }

        if (GUILayout.Button("Delete All Tests"))
        {
            dataRecControl.clearTests();
        }
        if (GUILayout.Button("Delete All Reports"))
        {
            dataRecControl.clearReports();
        }
    }
}

#endif