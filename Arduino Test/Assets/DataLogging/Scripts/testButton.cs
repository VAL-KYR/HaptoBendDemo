﻿using System.Collections;
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
        if (GUILayout.Button("Delete All Tests"))
        {
            myScript.clearTests();
        }
    }
}

#endif