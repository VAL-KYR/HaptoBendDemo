using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clippingManager : MonoBehaviour {

	public List<GameObject> clippingObjects = new List<GameObject>();

	public void OnTriggerStay(Collider other) 
	{
		if (!clippingObjects.Contains(other.gameObject))
		{
			clippingObjects.Add(other.gameObject);
		}
	}

	public void OnTriggerExit(Collider other) 
	{
		clippingObjects.Remove(other.gameObject);
	}

	
}
