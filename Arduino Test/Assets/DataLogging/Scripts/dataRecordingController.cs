using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dataRecordingController : MonoBehaviour {

    public List<GameObject> tests = new List<GameObject>();
    public string desiredTestObject = "Hammer"; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        foreach (GameObject g in tests)
        {
            if (g.name.Contains(desiredTestObject))
            {
                g.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }
        }
	}

}
