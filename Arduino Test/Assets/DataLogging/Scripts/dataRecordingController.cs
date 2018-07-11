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
    public List<GameObject> tests;
    public GameObject currTest;

    bool randomDock = false;
    public List<GameObject> dockPresets = new List<GameObject>();
    public GameObject selectedDockPreset;
    public GameObject lastDockPreset;
    
    public Vector2 deviceLimits;
    public float dockPartSeperation = 0.8f;
    public string currAction = "Loaded";

    


    public float inputTimer = 1f;
    float inputTime = 0f;


    Transform rightWing;
    Transform leftWing;

    // Delete all Test Objects and files before starting
    public void Start()
    {
        rightWing = new GameObject("virtualWingR").transform;
        leftWing = new GameObject("virtualWingL").transform;

        rightWing.position = new Vector3(10, 0, 0);
        leftWing.position = new Vector3(-10, 0, 0);

        ClearTests();
        ClearReports();

        // Creates a new test before starting
        NewTest();
        SetCurrTest();
        // set dock shape mode before first new dock shape
        lastDockPreset = selectedDockPreset;
        NewDockShape();
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
                    currAction = currTest.name + " Started";
                }
                else
                {
                    currTest.GetComponent<dataRecorder>().recordAngles = false;
                    currAction = currTest.name + " Ended";
                }
                this.GetComponent<testDataGUI>().FetchAction(currAction);
                inputTime = 0f;
            }
            if (Input.GetButton("CreateTest"))
            {
                NewTest();
                currAction = "Test_" + (tests.Count - 1) + " Created";
                this.GetComponent<testDataGUI>().FetchAction(currAction);
                inputTime = 0f;
            }
            if (Input.GetButton("RandomizeDock"))
            {
                NewDockShape();
                currAction = "Dock Randomized";
                inputTime = 0f;
                this.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("ZeroDock"))
            {
                ZeroDockShape();
                currAction = "Dock Zeroed";
                inputTime = 0f;
                this.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DockStyle"))
            {
                // Flip the randomDock
                randomDock = !randomDock;
                currAction = "Random Docks = " + randomDock;
                inputTime = 0f;
                this.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("SummarizeTests"))
            {
                FinalReport();
                currAction = "Tests Summarized";
                inputTime = 0f;
                this.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeleteTests"))
            {
                ClearTests();
                currAction = "All Tests Deleted";
                inputTime = 0f;
                this.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeleteReports"))
            {
                ClearReports();
                currAction = "All Reports Deleted";
                inputTime = 0f;
                this.GetComponent<testDataGUI>().FetchAction(currAction);
            }
        }

        SetCurrTest();

        inputTime += Time.smoothDeltaTime;

    }

    public void SetCurrTest()
    {
        // Set current test to the oldest test not completed
        foreach (GameObject test in tests)
        {
            if (!test.GetComponent<dataRecorder>().anglesRecorded)
            {
                currTest = test;
                // we break to make sure it's the first one in the list historically top to bottom
                break;
            }
        }
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
        //currTest = test;

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

        //////////// CREATE HUGE IF STATEMENT FOR PRESET DOCK SHAPES AND RANDOM ONES
        //// PRESET DOCK SHAPES
        if (!randomDock)
        {

            // This could be cut out to remove randomness
            // random dock reorientation
            currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(
                                                                                    NewAngle(180f, -180f),
                                                                                    NewAngle(180f, -180f),
                                                                                    NewAngle(180f, -180f)));

            while (selectedDockPreset == lastDockPreset)
            {
                // Random Preset to copy from preset shape to dock
                selectedDockPreset = dockPresets[(int)Random.Range(0, dockPresets.Count - 1)];
            }

            lastDockPreset = selectedDockPreset;

            //// NEW
            for (int i = 0; i < currTest.GetComponent<dataRecorder>().dockAngleObjects.Length; i++)
            {
                currTest.GetComponent<dataRecorder>().dockAngleObjects[i].transform.localRotation =
                    selectedDockPreset.GetComponent<dockPreset>().shapes[i].transform.localRotation;
            }
            
        }

        //// RANDOM DOCK SHAPES
        else
        {
            // random dock reorientation
            currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(
                                                                                    NewAngle(180f, -180f),
                                                                                    NewAngle(180f, -180f),
                                                                                    NewAngle(180f, -180f)));

            //// NEW
            foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
            {
                if (angle.tag.Contains("Inverse"))
                {
                    if (angle.tag.Contains("Child"))
                    {
                        angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                    angle.transform.localRotation.x,
                                                    angle.transform.localRotation.y,
                                                    NewAngle(deviceLimits[0], deviceLimits[1])));
                        leftWing = angle.transform.GetChild(0);
                    }

                }

                else
                {
                    angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                    angle.transform.localRotation.x,
                                                    angle.transform.localRotation.y,
                                                    -NewAngle(deviceLimits[0], deviceLimits[1])));
                    if (angle.tag.Contains("Child"))
                    {
                        rightWing = angle.transform.GetChild(0);
                    }
                }

            }

            // Dock shape collision fixer
            while (Vector3.Distance(leftWing.position, rightWing.position) < dockPartSeperation)
            {
                foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
                {

                    if (angle.tag.Contains("Inverse"))
                    {
                        if (angle.tag.Contains("Child"))
                        {
                            angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                        angle.transform.localRotation.x,
                                                        angle.transform.localRotation.y,
                                                        NewAngle(deviceLimits[0], deviceLimits[1])));
                            leftWing = angle.transform.GetChild(0);
                        }

                    }

                    else
                    {
                        angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                        angle.transform.localRotation.x,
                                                        angle.transform.localRotation.y,
                                                        -NewAngle(deviceLimits[0], deviceLimits[1])));
                        if (angle.tag.Contains("Child"))
                        {
                            rightWing = angle.transform.GetChild(0);
                        }
                    }

                }
            }
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
            // Corrupt them all!
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
