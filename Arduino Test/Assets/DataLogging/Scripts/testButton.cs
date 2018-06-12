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

        dataRecordingController myScript = (dataRecordingController)target;
        if (GUILayout.Button("Create Test"))
        {
            myScript.newTest();
        }
        
        if (GUILayout.Button("New Dock Shape"))
        {
            myScript.newDockShape();
        }
        if (GUILayout.Button("Zero Dock Shape"))
        {
            myScript.zeroDockShape();
        }

        if (GUILayout.Button("Summarize Results (Final Report)"))
        {
            myScript.finalReport();
        }

        if (GUILayout.Button("Delete All Tests"))
        {
            myScript.clearTests();
        }
        if (GUILayout.Button("Delete All Reports"))
        {
            myScript.clearReports();
        }
    }
}

#endif