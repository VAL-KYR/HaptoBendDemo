using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dockProperties : MonoBehaviour {

	public List<GameObject> shape;

	// Use this for initialization
	void Start () 
	{
		shape.Add(GameObject.FindGameObjectWithTag("dockRotate"));
		shape.Add(GameObject.FindGameObjectWithTag("dockChildRotate"));
		shape.Add(GameObject.FindGameObjectWithTag("dockRotateInverse"));
		shape.Add(GameObject.FindGameObjectWithTag("dockChildRotateInverse"));
	}	
}
