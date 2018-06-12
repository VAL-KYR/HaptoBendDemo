using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dataSummary : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //// WHAT THE SCRIPT NEEDS TO DO ////
    // get the list of tests
    // get the data from each script
    /// compile the test results into a list of test precision ratings
    /// compile the test results into a list of test time ratings
    // compile the test results into an overall efficiency rating from the average of all efficencies

    /// GENERIC FUNCTIONS ///
    //// MATH OPERATIONS ////
    // Get an average with no weight
    public float unweightedAverage(params float[] numbers)
    {
        float total = 0;
        foreach (float n in numbers)
        {
            total += n;
        }

        return total / numbers.Length;
    }
}
