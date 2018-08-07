using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class multiDisplay : MonoBehaviour {

	public List<GameObject> ovrDisplays;
	public bool multiDisplays = false;
	public bool duplicateDeviceView = false;

	// Use this for initialization
	void Start () {

		ovrDisplays = GameObject.FindGameObjectsWithTag("MainCamera").ToList();

		if (!duplicateDeviceView)
			UnityEngine.XR.XRSettings.showDeviceView = false;
		else
			UnityEngine.XR.XRSettings.showDeviceView = true;

		if (multiDisplays)
		{
			foreach (GameObject cam in ovrDisplays) 
			{
				cam.GetComponent<Camera>().targetDisplay = 1;
			}

			foreach (Display d in Display.displays)
			{
				d.Activate(d.systemWidth, d.systemHeight, 30);
			}
		}
		else
		{
			foreach (GameObject cam in ovrDisplays) 
			{
				cam.GetComponent<Camera>().targetDisplay = 0;
			}
		}
		
	}
}
