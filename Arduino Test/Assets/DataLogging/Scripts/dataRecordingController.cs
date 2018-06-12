﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class dataRecordingController : MonoBehaviour {

    public string filePrefix = "Report_Raw_";
    public string filePath = "Assets/DataLogging/Reports/";
    public GameObject testObject;
    public List<GameObject> tests = new List<GameObject>();
    public GameObject currTest;
    public Vector2 deviceLimits;

    // Delete all Test Objects and files before starting
    public void Start()
    {
        clearTests();
        clearReports();
    }

    public void Update()
    {

    }

    //// GENERIC TEST FUNCTIONS ////
    // Create a new test object
    public void newTest()
    {
        GameObject test = Instantiate<GameObject>(testObject, Vector3.zero, Quaternion.identity, this.transform);
        // Init file settings
        test.GetComponent<dataRecorder>().textLog.path = filePath;
        test.GetComponent<dataRecorder>().textLog.fileName = filePrefix + tests.Count;

        // Init overall shape orientation
        test.GetComponent<dataRecorder>().shape = GameObject.FindGameObjectWithTag("virtualDevice");
        // Init overall dock orientation
        test.GetComponent<dataRecorder>().dockShape = GameObject.FindGameObjectWithTag("dockDevice");

        // Init shape angles
        test.GetComponent<dataRecorder>().angleObjects[0] = GameObject.FindGameObjectWithTag("rotate");
        test.GetComponent<dataRecorder>().angleObjects[1] = GameObject.FindGameObjectWithTag("childRotate");
        test.GetComponent<dataRecorder>().angleObjects[2] = GameObject.FindGameObjectWithTag("rotateInverse");
        test.GetComponent<dataRecorder>().angleObjects[3] = GameObject.FindGameObjectWithTag("childRotateInverse");
        // Init dock angles
        test.GetComponent<dataRecorder>().dockAngleObjects[0] = GameObject.FindGameObjectWithTag("dockRotate");
        test.GetComponent<dataRecorder>().dockAngleObjects[1] = GameObject.FindGameObjectWithTag("dockChildRotate");
        test.GetComponent<dataRecorder>().dockAngleObjects[2] = GameObject.FindGameObjectWithTag("dockRotateInverse");
        test.GetComponent<dataRecorder>().dockAngleObjects[3] = GameObject.FindGameObjectWithTag("dockChildRotateInverse");

        // Name the TestObject
        test.name = "Test_" + tests.Count;

        // Set current test to whichever was last made
        currTest = test;
        // Add to test list
        tests.Add(test);
    }

    // Create Final Report Summary
    public void finalReport()
    {
        this.GetComponent<dataSummary>().createReport = true;
    }

    // Get a new shape to dock to
    public void newDockShape()
    {
        // dock reorientation
        currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(newAngle(deviceLimits[0], deviceLimits[1]), newAngle(deviceLimits[0], deviceLimits[1]), newAngle(deviceLimits[0], deviceLimits[1])));

        // dock angles reorientation
        foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
        {
            angle.transform.localRotation = Quaternion.Euler(new Vector3(angle.transform.localRotation.x, angle.transform.localRotation.y, newAngle(deviceLimits[0], deviceLimits[1])));
        }
    }

    // Test Dock Shape
    public void zeroDockShape()
    {
        // dock reorientation
        currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));

        // dock angles reorientation
        foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
        {
            angle.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
        }
    }

    // Delete all tests
    public void clearTests()
    {
        // Destroy the Test Objects
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }

        // Reset Test List
        tests.Clear();
    }

    // Clear all Reports
    public void clearReports()
    {
        // If the path exists delete every file in it and refresh the unity editor if we're using it
        if (Directory.Exists(filePath))
        {
            Directory.Delete(filePath, true);
        }

        Directory.CreateDirectory(filePath);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }


    /// GENERIC FUNCTIONS ///
    float newAngle(float negativeLimit, float positiveLimit)
    {
        return Random.Range(negativeLimit, positiveLimit);
    }
    
}
