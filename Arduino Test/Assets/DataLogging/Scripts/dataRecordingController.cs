using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class dataRecordingController : MonoBehaviour {

    public string filePrefix = "Report_Raw_";
    public string filePath = "Assets/DataLogging/Reports/";
    public GameObject testObject;
    public List<GameObject> tests = new List<GameObject>();
    public GameObject currTest;
    public Vector2 deviceLimits;


    public float inputTimer = 1f;
    float inputTime = 0f;

    // Delete all Test Objects and files before starting
    public void Start()
    {
        ClearTests();
        ClearReports();
    }

    public void Update()
    {
        // Inputs during test
        if (inputTime > inputTimer)
        {
            if (Input.GetButton("ExecuteTest"))
            {
                if (!currTest.GetComponent<dataRecorder>().recordAngles)
                {
                    currTest.GetComponent<dataRecorder>().recordAngles = true;
                }
                else
                {
                    currTest.GetComponent<dataRecorder>().recordAngles = false;
                }
                inputTime = 0f;
            }
            if (Input.GetButton("CreateTest"))
            {
                NewTest();
                inputTime = 0f;
            }
            if (Input.GetButton("RandomizeDock"))
            {
                NewDockShape();
                inputTime = 0f;
            }
            if (Input.GetButton("ZeroDock"))
            {
                ZeroDockShape();
                inputTime = 0f;
            }
            if (Input.GetButton("SummarizeTests"))
            {
                FinalReport();
                inputTime = 0f;
            }
            if (Input.GetButton("DeleteTests"))
            {
                ClearTests();
                inputTime = 0f;
            }
            if (Input.GetButton("DeleteReports"))
            {
                ClearReports();
                inputTime = 0f;
            }
        }
        
        

        inputTime += Time.smoothDeltaTime;

    }

    //// GENERIC TEST FUNCTIONS ////
    // Create a new test object
    public void NewTest()
    {
        // Create test Object
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
    public void FinalReport()
    {
        this.GetComponent<dataSummary>().CalculateFinalResults();
        this.GetComponent<dataSummary>().ExportSummaryFile();
    }

    // Get a new shape to dock to
    public void NewDockShape()
    {
        // dock reorientation
        currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(NewAngle(deviceLimits[0], deviceLimits[1]), NewAngle(deviceLimits[0], deviceLimits[1]), NewAngle(deviceLimits[0], deviceLimits[1])));

        // dock angles reorientation
        foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
        {
            angle.transform.localRotation = Quaternion.Euler(new Vector3(angle.transform.localRotation.x, angle.transform.localRotation.y, NewAngle(deviceLimits[0], deviceLimits[1])));
        }
    }

    // Test Dock Shape
    public void ZeroDockShape()
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
    public void ClearTests()
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
    public void ClearReports()
    {
        // If the path exists delete every file in it and refresh the unity editor if we're using it
        if (Directory.Exists(filePath))
        {
            Directory.Delete(filePath, true);
        }

        Directory.CreateDirectory(filePath);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }


    /// GENERIC FUNCTIONS ///
    float NewAngle(float negativeLimit, float positiveLimit)
    {
        return Random.Range(negativeLimit, positiveLimit);
    }
    
}
