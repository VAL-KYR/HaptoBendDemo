using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class dataRecordingController : MonoBehaviour {

    public GameObject testObject;
    public List<GameObject> tests = new List<GameObject>();

	// Use this for initialization
	void Start () {
        newTest();
	}
	
	// Update is called once per frame
	void Update () {
        
	}


    //// GENERIC TEST FUNCTIONS ////
    // Create a new test object
    public void newTest()
    {
        GameObject test = Instantiate<GameObject>(testObject, Vector3.zero, Quaternion.identity, this.transform);
        // Init file settings
        test.GetComponent<dataRecorder>().textLog.path = "Assets/Resources/";
        test.GetComponent<dataRecorder>().textLog.fileName = "Report_Raw_" + tests.Count;
        // Init shape angles
        test.GetComponent<dataRecorder>().angleObjects[0] = GameObject.FindGameObjectWithTag("rotate");
        test.GetComponent<dataRecorder>().angleObjects[1] = GameObject.FindGameObjectWithTag("childRotate");
        test.GetComponent<dataRecorder>().angleObjects[2] = GameObject.FindGameObjectWithTag("rotateInverse");
        test.GetComponent<dataRecorder>().angleObjects[3] = GameObject.FindGameObjectWithTag("childRotateInverse");
        // Init overall shape orientation
        test.GetComponent<dataRecorder>().shape = GameObject.FindGameObjectWithTag("virtualDevice");
        // Name the TestObject
        test.name = "Test_" + tests.Count;


        tests.Add(test);
    }


    // Delete all tests
    public void clearTests()
    {
        foreach (GameObject t in tests)
        {
            Destroy(t);
        }

        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }

        tests.Clear();
        //tests = null;
    }
    
}
