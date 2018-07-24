using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class multiDisplay : MonoBehaviour {

	// Use this for initialization
	void Start () {
		foreach(Display d in Display.displays)
		{
			d.Activate();
		}
		
	}
}
