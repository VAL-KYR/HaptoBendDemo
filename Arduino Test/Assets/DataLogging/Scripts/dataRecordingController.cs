﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public GameObject device;
    public IMU imu;

    public List<string> dockStyles = new List<string>(3);
    public string activeDockStyle = "";

    public bool startWithNewDock = true;
    public bool virtualDeviceVisible = true;

    public List<GameObject> dockPresets = new List<GameObject>();
    public GameObject selectedDockPreset;
    public GameObject lastDockPreset;

    public float[] pureAnglesOrient = new float[3]; 
    public float[] pureAnglesShape = new float[3];
    
    public Vector2 deviceLimits;
    public float dockPartSeperation = 0.8f;

    string currAction = "Loaded";

    public bool errorGathered = false;

    public float inputTimer = 1f;
    float inputTime = 0f;

    public string testerName; //** change this with a text field

    // Get Layer names
    public string indexVDName = "VD";
    public string indexDefaultName = "Default";

    public bool deviceInit = false;

    public bool testsDone = false;

    public bool deviceReset = true;
    //

    // New dock solver
    public List<GameObject> checkObjects = new List<GameObject>();
    public List<GameObject> allClippingObjects = new List<GameObject>();
    public List<float> nextDockAngles = new List<float>();
    public float dockFoldSpeed = 3.0f;


    [System.Serializable]
    public class TestCategorization : System.Object
    {
        public bool automateTypes = true;
        
        public int VisLtdLimit = 10;
        public int InvisLtdLimit = 10;
        public int VisPresetLimit = 10;
        public int InvisPresetLimit = 10;


        public List<GameObject> VisLtdRandom;
        public List<GameObject> InvisLtdRandom;
        public List<GameObject> VisPresets;
        public List<GameObject> InvisPresets;
    }
    public TestCategorization testCateg = new TestCategorization();

    // Delete all Test Objects and files before starting
    public void Start()
    {
        device = GameObject.FindGameObjectWithTag("virtualDevice");
        imu = device.GetComponent<IMU>();
        dockPresets = GameObject.FindGameObjectsWithTag("dockPresets").ToList();
        checkObjects = GameObject.FindGameObjectsWithTag("clipCheck").ToList();

        ClearTests();
        fileEditor.ClearDir(filePath);

        // Creates a new test before starting
        NewTest(deviceReset);
    }

    public void Update()
    {
        // Initial Device Calibration
        if (errorGathered && !deviceInit)
        {
            // Calibrate the Device before first test
            if (imu.calibrate)
            {
                CalIMU();
            }
            else
            {
                ReCalIMU();
            }

            deviceInit = true;
        }

        // Check if tests are done
        if (testCateg.VisLtdRandom.Count >= testCateg.VisLtdLimit &&
            testCateg.InvisLtdRandom.Count >= testCateg.InvisLtdLimit && 
            testCateg.VisPresets.Count >= testCateg.VisPresetLimit &&
            testCateg.InvisPresets.Count >= testCateg.InvisPresetLimit)
        {
            testsDone = true;
        }

        // Inputs during test
        if (inputTime > inputTimer)
        {
            if ((Input.GetButton("ExecuteTest") || Input.GetButton("Pedal")) && !testsDone)
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
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
                inputTime = 0f;
            }
            if (Input.GetButton("CreateTest") && !testsDone)
            {
                NewTest(false);
                currAction = "Test_" + (tests.Count - 1) + " Created";
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
                inputTime = 0f;
            }
            if (Input.GetButton("RandomizeDock"))
            {
                NewDockShape();
                currAction = "Dock Randomized";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("ZeroDock"))
            {
                ZeroDockShape();
                currAction = "Dock Zeroed";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DockStyle"))
            {
                // Instead of flipping dock style try cycling dock style 
                activeDockStyle = NextDockStyle();
                currAction = "Docks are " + activeDockStyle;
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("SummarizeTests"))
            {
                FinalReport();
                currAction = "Tests Summarized";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeleteTests"))
            {
                ClearTests();
                currAction = "All Tests Deleted";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeleteReports"))
            {
                fileEditor.ClearDir(filePath);
                currAction = "All Reports Deleted";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeviceVisible"))
            {
                virtualDeviceVisible = !virtualDeviceVisible;
                FlipDeviceVisibilityInHMD(virtualDeviceVisible);
                currAction = "Device Visible " + virtualDeviceVisible;
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("ReCalDevice"))
            {
                ReCalIMU();

                currAction = "Device Recalibrated";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }  
            if (Input.GetButton("BadTest"))
            {
                activeDockStyle = "BAD DATA";

                currAction = currTest.name + " Trashed";
                inputTime = 0f;
                transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
            }      
        }
        // Automatically Creates and Starts new tests
        if (!testsDone)
        {
            if (currTest.GetComponent<dataRecorder>().anglesRecorded)
            {
                deviceReset = !deviceReset;
                NewTest(deviceReset);

                SetCurrTest();
                currTest.GetComponent<dataRecorder>().recordAngles = true;
                currAction = currTest.name + " Started";
            }
        }
        

        // Check what the current test is
        SetCurrTest();

        // Check for clipping and correct by choosing a new dock of the same dock style
        if (ClippingChecker())
        {
            NewDockShape();

            currAction = "Fixing Clipping";
            transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
        }

        // Always move the dock to a new position
        MoveDock();

        inputTime += Time.smoothDeltaTime;
    }

    //++ for lerping the dock to remove collisions
    public void MoveDock()
    {
        foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
        {
            if (angle.tag.Contains("Inverse"))
            {
                if (angle.tag.Contains("Child"))
                {
                    angle.transform.localRotation = Quaternion.Lerp(angle.transform.localRotation, 
                                                    Quaternion.Euler(new Vector3(
                                                        angle.transform.localRotation.x,
                                                        angle.transform.localRotation.y,
                                                        nextDockAngles[2])), 
                                                    dockFoldSpeed * Time.deltaTime);
                }

            }

            else
            {
                angle.transform.localRotation = Quaternion.Lerp(angle.transform.localRotation, 
                                                    Quaternion.Euler(new Vector3(
                                                        angle.transform.localRotation.x,
                                                        angle.transform.localRotation.y,
                                                        nextDockAngles[0])), 
                                                    dockFoldSpeed * Time.deltaTime);
                if (angle.tag.Contains("Child"))
                {
                    // This is a special distinction for a special case where the angle won't reach -150 with lerp but only -82 no idea why
                    if (activeDockStyle == "Presets" && selectedDockPreset.name == "dockPreset_1")
                    {
                        angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                            angle.transform.localRotation.x,
                                                            angle.transform.localRotation.y,
                                                            nextDockAngles[1]));
                    }
                    else 
                    {
                        angle.transform.localRotation = Quaternion.Lerp(angle.transform.localRotation, 
                                                    Quaternion.Euler(new Vector3(
                                                        angle.transform.localRotation.x,
                                                        angle.transform.localRotation.y,
                                                        nextDockAngles[1])), 
                                                    dockFoldSpeed * Time.deltaTime);
                    }
                }
            }

        }
    }

    //// Recalibrate With Wait
    public void ReCalIMU()
    {
        StartCoroutine(ReCalIMUSeq());
        new WaitForSeconds(0.1f);
        StartCoroutine(ReCalIMUSeq());
    }
    public IEnumerator ReCalIMUSeq()
    {
        imu.Calibrate();
        yield return new WaitForSeconds(0.1f);
        imu.BendReset();
        imu.Calibrate();
    }

    //// Calibrate
    public void CalIMU()
    {
        imu.BendReset();
        imu.Calibrate();
    }

    //// Find virtual device meshes [NEW]
    public void FlipDeviceVisibilityInHMD(bool deviceVisible)
    {
        List<MeshRenderer> meshes = device.GetComponentsInChildren<MeshRenderer>().ToList();
        
        // Set rather or not the objects have the layer that makes them invisible to the HMD
        // Give the meshes a transparent look for the test runner to understand visually that the object is not visible to the person in the HMD
        foreach (MeshRenderer mesh in meshes)
        {
            if (!deviceVisible)
            {
                mesh.gameObject.layer = LayerMask.NameToLayer(indexVDName);
                mesh.materials[0].color = new Color(mesh.materials[0].color.r, 
                                                    mesh.materials[0].color.g, 
                                                    mesh.materials[0].color.b, 
                                                    0.5f);
            }
            else
            {
                mesh.gameObject.layer = LayerMask.NameToLayer(indexDefaultName);
                mesh.materials[0].color = new Color(mesh.materials[0].color.r, 
                                                    mesh.materials[0].color.g, 
                                                    mesh.materials[0].color.b, 
                                                    1f);
            }
        }
    }

    //// Keeps track of what is the test that will have data sent
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
    public void NewTest(bool zeroDockCal)
    {
        TestCatCheck();

        //// If using an experiment procedure do this ////
        if (testCateg.automateTypes)
        {
            while (true)
            {
                // Generate a random style choice and exit with a break when done
                float index = Random.Range(0f, 1.0f);

                if ((index >= 0f && index < 0.25f) && testCateg.VisLtdRandom.Count < testCateg.VisLtdLimit)
                {
                    activeDockStyle = dockStyles[2];
                    virtualDeviceVisible = true;
                    //Debug.Log("VisLtdRandom");
                    break;
                }
                else if ((index >= 0.25f && index < 0.5f) && testCateg.InvisLtdRandom.Count < testCateg.InvisLtdLimit)
                {
                    activeDockStyle = dockStyles[2];
                    virtualDeviceVisible = false;
                    //Debug.Log("InvisLtdRandom");
                    break;
                }
                else if ((index >= 0.5f && index < 0.75f) && testCateg.VisPresets.Count < testCateg.VisPresetLimit)
                {
                    activeDockStyle = dockStyles[1];
                    virtualDeviceVisible = true;
                    //Debug.Log("VisPresets");
                    break;
                }
                else if ((index >= 0.75f && index <= 1.0f) && testCateg.InvisPresets.Count < testCateg.InvisPresetLimit)
                {
                    activeDockStyle = dockStyles[1];
                    virtualDeviceVisible = false;
                    //Debug.Log("InvisPresets");
                    break;
                }
                else if (testCateg.VisLtdRandom.Count >= testCateg.VisLtdLimit &&
                        testCateg.InvisLtdRandom.Count >= testCateg.InvisLtdLimit && 
                        testCateg.VisPresets.Count >= testCateg.VisPresetLimit &&
                        testCateg.InvisPresets.Count >= testCateg.InvisPresetLimit)
                {
                    currAction = "Tests Finished";
                    transform.parent.GetComponent<testDataGUI>().FetchAction(currAction);
                    break;
                }
            }

            FlipDeviceVisibilityInHMD(virtualDeviceVisible);
        }

        // Create test Object
        GameObject test = Instantiate<GameObject>(testObject, Vector3.zero, Quaternion.identity, this.transform);
        dataRecorder testParams = test.GetComponent<dataRecorder>();
        // Init file settings
        testParams.textLog.path = filePath;
        testParams.textLog.fileName = filePrefix + tests.Count;

        // Init overall shape orientation
        testParams.shape = GameObject.FindGameObjectWithTag("virtualDevice");
        // Init overall dock orientation
        testParams.dockShape = GameObject.FindGameObjectWithTag("dockDevice");
        // Init shape angles
        testParams.angleObjects[0] = GameObject.FindGameObjectWithTag("rotate");
        testParams.angleObjects[1] = GameObject.FindGameObjectWithTag("childRotate");
        testParams.angleObjects[2] = GameObject.FindGameObjectWithTag("rotateInverse");
        testParams.angleObjects[3] = GameObject.FindGameObjectWithTag("childRotateInverse");
        // Init dock angles
        testParams.dockAngleObjects[0] = GameObject.FindGameObjectWithTag("dockRotate");
        testParams.dockAngleObjects[1] = GameObject.FindGameObjectWithTag("dockChildRotate");
        testParams.dockAngleObjects[2] = GameObject.FindGameObjectWithTag("dockRotateInverse");
        testParams.dockAngleObjects[3] = GameObject.FindGameObjectWithTag("dockChildRotateInverse");



        // Name the TestObject
        test.name = "Test_" + tests.Count;
        // Tag that trial
        test.tag = "trial";
        // Give a trial ID
        testParams.finalResults.trialNumber = tests.Count;

        // Add to test list
        tests.Add(test);

        // Zero shape for calibration on end
        if (zeroDockCal)
        {
            test.name = "Test_dock_zero";
            test.tag = "zeroTrial";

            activeDockStyle = "ZEROING DATA";
            virtualDeviceVisible = true;
            FlipDeviceVisibilityInHMD(virtualDeviceVisible);
        }

        TestCatCheck();

        // Set the current test if it has not yet beens set
        if (!currTest)
            SetCurrTest();

        // Zero shape for calibration on end
        if (zeroDockCal)
        {
            ZeroDockShape();
        }
        else
        {
            // Once test is created give it a new dock shape
            NewDockShape();
        }
    }

    //// Find Completed test types and add to list ////
    public void TestCatCheck()
    {
        testCateg.VisLtdRandom = TestsOfType("True", "Ltd Random", tests);
        testCateg.InvisLtdRandom = TestsOfType("False", "Ltd Random", tests);
        testCateg.VisPresets = TestsOfType("True", "Presets", tests);
        testCateg.InvisPresets = TestsOfType("False", "Presets", tests);
    }

    // Create Final Report Summary
    public void FinalReport()
    {
        // Export an All Participants summary
        // Add all trials to this list
        if (transform.parent.GetComponent<testDataGUI>().name == "alldata")
        {
            tests = GameObject.FindGameObjectsWithTag("trial").ToList();

            //-- Remove the debug/dummy tests
            Destroy(tests[tests.Count - 1]);
            tests.RemoveAt(tests.Count - 1);

            TestCatCheck();
        }
        // Just do local trials
        else
        {
            //-- Remove the debug/dummy tests
            Destroy(tests[tests.Count - 1]);
            Destroy(tests[0]);
            tests.RemoveAt(tests.Count - 1);
            tests.RemoveAt(0);
        }

        // Export the test list summary
        GetComponent<dataSummary>().CalculateFinalResults(tests);
        GetComponent<dataSummary>().ExportTrials("Test", true, true);

        // Export the test category summaries
        GetComponent<dataSummary>().SetFileDestination(filePath, "Trial_Category_Summary", ".txt");
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.VisLtdRandom);
        GetComponent<dataSummary>().ExportTrialSummaries("VisLtdRandom", true, false);
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.InvisLtdRandom);
        GetComponent<dataSummary>().ExportTrialSummaries("InvisLtdRandom", true, false);
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.VisPresets);
        GetComponent<dataSummary>().ExportTrialSummaries("VisPresets", true, false);
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.InvisPresets);
        GetComponent<dataSummary>().ExportTrialSummaries("InvisPresets", true, false);
    }

    // Get a new shape to dock to
    public void NewDockShape()
    {        
        //// PURE PRESETS [Procedure Style]
        if (activeDockStyle == dockStyles[1])
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

            //++ Creating a new set of nextDockAngles
            nextDockAngles = new List<float>();

            //++ Creating a new set of nextDockAngles
            for (int i = 0; i < currTest.GetComponent<dataRecorder>().dockAngleObjects.Length; i++)
            {
                if (selectedDockPreset.GetComponent<dockPreset>().shapes[i].name != "RotateInverse")
                {
                    nextDockAngles.Add(selectedDockPreset.GetComponent<dockPreset>().shapes[i].transform.localEulerAngles.z);
                }
            }
            
        }

        //// LIMITED ANGLE GENERATIVE [Procedure Style]
        else if (activeDockStyle == dockStyles[2])
        {
            // random dock reorientation
            currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(
                                                                                    NewPresetAngle("shape"),
                                                                                    NewPresetAngle("shape"),
                                                                                    NewPresetAngle("shape")));
            
            // Creating a new set of nextDockAngles
            nextDockAngles = new List<float>
            {
                -NewPresetAngle("bend"),
                -NewPresetAngle("bend"), 
                NewPresetAngle("bend")
            };
        }

        //// RANDOM GENERATIVE DOCK SHAPES
        else if (activeDockStyle == dockStyles[0])
        {
            // random dock reorientation
            currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(
                                                                                    NewAngle(180f, -180f),
                                                                                    NewAngle(180f, -180f),
                                                                                    NewAngle(180f, -180f)));

            // Creating a new set of nextDockAngles
            nextDockAngles = new List<float>
            {
                -NewAngle(deviceLimits[0], deviceLimits[1]),
                -NewAngle(deviceLimits[0], deviceLimits[1]), 
                NewAngle(deviceLimits[0], deviceLimits[1])
            };
        }
    }

    // Test Dock Shape
    public void ZeroDockShape()
    {
        // dock reorientation
        currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));

        // Creating a new set of nextDockAngles
        nextDockAngles = new List<float>
        {
            0,
            0, 
            0
        };
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

    /// GENERIC FUNCTIONS ///
    float NewAngle(float negativeLimit, float positiveLimit)
    {
        return Random.Range(negativeLimit, positiveLimit);
    }

    float NewPresetAngle(string angleType)
    {
        float chance = Random.Range(0.0f, 1.0f);
        Debug.Log(chance);

        float angle = 0;

        if (angleType == "shape")
        {
            if (chance >= 0f && chance < 0.2f)
            {
                angle = pureAnglesOrient[0];
            }
            else if (chance >= 0.2f && chance < 0.6f)
            {
                angle = pureAnglesOrient[1];
            }
            else if (chance >= 0.6f && chance <= 1.0f)
            {
                angle = pureAnglesOrient[2];
            }
        }
        // adjust for higher angle balance
        else if (angleType == "bend")
        {
            if (chance >= 0f && chance < 0.3f)
            {
                angle = pureAnglesShape[0];
            }
            else if (chance >= 0.3f && chance < 0.7f)
            {
                angle = pureAnglesShape[1];
            }
            else if (chance >= 0.7f && chance <= 1.0f)
            {
                angle = pureAnglesShape[2];
            }
        }

        return angle;
    }

    // clipping fixing
    public bool ClippingChecker()
    {
        allClippingObjects = new List<GameObject>();

        // Check each of the checkObjects (physical objects) scipt for clippingObjects and add to a general list of clipping Objects
        foreach (GameObject checkObj in checkObjects)
        {
            foreach (GameObject clippingObj in checkObj.GetComponent<clippingManager>().clippingObjects)
            {
#if UNITY_EDITOR
                Debug.Log("clipping found");
#endif
                allClippingObjects.Add(clippingObj);
            }
        }

        // If there are any clipping objects return true else return false and continue program
        if (allClippingObjects.Count > 0)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    string NextDockStyle()
    {
        int index = 0;

        // Get currents index
        for (int i = 0; i < dockStyles.Count; i++) 
        {
            if (dockStyles[i] == activeDockStyle) 
            {
                index = i;
            }
        }

        // if at end of list set index to first
        if (index == dockStyles.Count - 1) 
        {
            index = 0;
        }
        else 
        {
            index++;
        }
        
        return dockStyles[index];
    }

    public List<GameObject> TestsOfType(string DesiredVisibility, string DesiredDockStyle, List<GameObject> listToSearch)
    {
        List<GameObject> testList = new List<GameObject>(); 

        foreach (GameObject test in listToSearch)
        {
            if ((test.GetComponent<dataRecorder>().finalResults.dockShapeStyle == DesiredDockStyle &&
            test.GetComponent<dataRecorder>().finalResults.deviceVisibility == DesiredVisibility))
                testList.Add(test);
        }

        return testList;
    } 
    
}