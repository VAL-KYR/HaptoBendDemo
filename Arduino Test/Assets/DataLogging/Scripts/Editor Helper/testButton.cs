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
            dataRecControl.NewTest();
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
            dataRecControl.ClearReports();
        }
    }
}

#endif