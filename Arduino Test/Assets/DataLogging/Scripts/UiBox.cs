using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UiBox : MonoBehaviour {

	[System.Serializable]
    public class positioner : System.Object
    {
		public List<Vector3> position = new List<Vector3>
		{
			new Vector3(0,0,0),
			new Vector3(90,0,0),
			new Vector3(180,0,0),
			new Vector3(270,0,0),
		};
		public int index = 0;
		public float speed = 5.0f;
	}
	static public positioner box = new positioner();

	public bool customCurve = false;
	public AnimationCurve curve;

	// Update is called once per frame
	void Update () {
		transform.localRotation = Quaternion.Lerp(transform.localRotation, 
										Quaternion.Euler(box.position[box.index]), 
										box.speed * Time.deltaTime);
	}
}
