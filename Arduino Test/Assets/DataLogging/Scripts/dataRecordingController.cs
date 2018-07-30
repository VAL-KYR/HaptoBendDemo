using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//**
public class Test 
{
    GameObject testObject;
    string dockStyleType;
    string visibility;
}


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


    Transform rightWing;
    Transform leftWing;

    // Get Layer names
    public string indexVDName = "VD";
    public string indexDefaultName = "Default";

    public bool deviceInit = false;

    public bool testsDone = false;

    //
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
        rightWing = new GameObject("virtualWingR").transform;
        leftWing = new GameObject("virtualWingL").transform;

        rightWing.position = new Vector3(10, 0, 0);
        leftWing.position = new Vector3(-10, 0, 0);

        ClearTests();
        ClearReports();

        // Creates a new test before starting
        NewTest();
        SetCurrTest();

        // set dock shape preset before first new dock shape
        lastDockPreset = selectedDockPreset;
        // set dock shape mode before first new dock shape
        activeDockStyle = dockStyles[0];

        if (startWithNewDock)
            NewDockShape();
    }

    public void Update()
    {
        // Initial Device Calibration
        if (errorGathered && !deviceInit)
        {
            // Calibrate the Device before first test
            CalIMU();

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
            if (Input.GetButton("ExecuteTest") && !testsDone)
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
                GetComponent<testDataGUI>().FetchAction(currAction);
                inputTime = 0f;
            }
            if (Input.GetButton("CreateTest") && !testsDone)
            {
                NewTest();
                currAction = "Test_" + (tests.Count - 1) + " Created";
                GetComponent<testDataGUI>().FetchAction(currAction);
                inputTime = 0f;
            }
            if (Input.GetButton("RandomizeDock"))
            {
                NewDockShape();
                currAction = "Dock Randomized";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("ZeroDock"))
            {
                ZeroDockShape();
                currAction = "Dock Zeroed";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DockStyle"))
            {
                // Instead of flipping dock style try cycling dock style 
                activeDockStyle = NextDockStyle();
                currAction = "Docks are " + activeDockStyle;
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("SummarizeTests"))
            {
                FinalReport();
                currAction = "Tests Summarized";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeleteTests"))
            {
                ClearTests();
                currAction = "All Tests Deleted";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeleteReports"))
            {
                ClearReports();
                currAction = "All Reports Deleted";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("DeviceVisible"))
            {
                virtualDeviceVisible = !virtualDeviceVisible;
                FlipDeviceVisibilityInHMD(virtualDeviceVisible);
                currAction = "Device Visible " + virtualDeviceVisible;
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }
            if (Input.GetButton("ReCalDevice"))
            {
                StartCoroutine(ReCalIMU());
                new WaitForSeconds(0.1f);
                StartCoroutine(ReCalIMU());

                currAction = "Device Recalibrated";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }  
            if (Input.GetButton("BadTest"))
            {
                activeDockStyle = "BAD DATA";

                currAction = currTest.name + " Trashed";
                inputTime = 0f;
                GetComponent<testDataGUI>().FetchAction(currAction);
            }      
        }

        SetCurrTest();

        inputTime += Time.smoothDeltaTime;
    }

    //// Recalibrate With Wait
    public IEnumerator ReCalIMU()
    {
        imu.Calibrate();
        yield return new WaitForSeconds(0.1f);
        imu.BendReset();
        imu.Calibrate();
    }

    //// Calibrate With Wait
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
    public void NewTest()
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
                    GetComponent<testDataGUI>().FetchAction(currAction);
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

        // Add to test list
        tests.Add(test);

        TestCatCheck();

        // Set the current test if it has not yet beens set
        if (!currTest)
            SetCurrTest();

        // Once test is created give it a new dock shape
        NewDockShape();
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
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.VisLtdRandom);
        GetComponent<dataSummary>().ExportSummaryFile("VisLtdRandom");
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.InvisLtdRandom);
        GetComponent<dataSummary>().ExportSummaryFile("InvisLtdRandom");
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.VisPresets);
        GetComponent<dataSummary>().ExportSummaryFile("VisPresets");
        GetComponent<dataSummary>().EraseResultsLists();
        GetComponent<dataSummary>().CalculateFinalResults(testCateg.InvisPresets);
        GetComponent<dataSummary>().ExportSummaryFile("InvisPresets");
        GetComponent<dataSummary>().EraseResultsLists();
    }

    // Get a new shape to dock to
    public void NewDockShape()
    {

        //////////// CREATE HUGE IF STATEMENT FOR PRESET DOCK SHAPES AND RANDOM ONES
        //// PURE PRESETS
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

            //// NEW
            for (int i = 0; i < currTest.GetComponent<dataRecorder>().dockAngleObjects.Length; i++)
            {
                currTest.GetComponent<dataRecorder>().dockAngleObjects[i].transform.localRotation =
                    selectedDockPreset.GetComponent<dockPreset>().shapes[i].transform.localRotation;
            }
            
        }

        //// LIMITED ANGLE GENERATIVE [Rob Style]
        else if (activeDockStyle == dockStyles[2])
        {
            // random dock reorientation
            currTest.GetComponent<dataRecorder>().dockShape.transform.localRotation = Quaternion.Euler(new Vector3(
                                                                                    NewPresetAngle("shape"),
                                                                                    NewPresetAngle("shape"),
                                                                                    NewPresetAngle("shape")));
            
            // Give new preset shape angles
            foreach (GameObject angle in currTest.GetComponent<dataRecorder>().dockAngleObjects)
            {
                if (angle.tag.Contains("Inverse"))
                {
                    if (angle.tag.Contains("Child"))
                    {
                        angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                    angle.transform.localRotation.x,
                                                    angle.transform.localRotation.y,
                                                    NewPresetAngle("bend")));
                        leftWing = angle.transform.GetChild(0);
                    }

                }

                else
                {
                    angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                    angle.transform.localRotation.x,
                                                    angle.transform.localRotation.y,
                                                    -NewPresetAngle("bend")));
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
                                                        NewPresetAngle("bend")));
                            leftWing = angle.transform.GetChild(0);
                        }

                    }

                    else
                    {
                        angle.transform.localRotation = Quaternion.Euler(new Vector3(
                                                        angle.transform.localRotation.x,
                                                        angle.transform.localRotation.y,
                                                        -NewPresetAngle("bend")));
                        if (angle.tag.Contains("Child"))
                        {
                            rightWing = angle.transform.GetChild(0);
                        }
                    }

                }
            }
        }

        //// RANDOM GENERATIVE DOCK SHAPES
        else if (activeDockStyle == dockStyles[0])
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

    float NewPresetAngle(string angleType)
    {
        int index = (int)Random.Range(1,3);

        float angle = 0;

        if (angleType == "shape")
        {
            angle = pureAnglesOrient[index - 1];
        }
        else if (angleType == "bend")
        {
            angle = pureAnglesShape[index - 1];
        }

        return angle;
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

    //++
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
