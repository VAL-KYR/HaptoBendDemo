using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lineTrack : MonoBehaviour {

	public GameObject trackObject;
	public Vector3 startOffset;

	private LineRenderer line;
	private UiBox uiBox;
	private dataMaster data;

	public int boxIndexActivate;
	public string dockTypeActivate;

	// Use this for initialization
	void Start () {
		try { line = GetComponent<LineRenderer>(); }
		catch { Debug.Log("No Line Renderer on " + name); }
		try { uiBox = FindObjectOfType<UiBox>(); }
		catch { Debug.Log("No UiBox in Scene"); }
		try { data = FindObjectOfType<dataMaster>(); }
		catch { Debug.Log("No DataMaster in Scene"); }
	}
	
	// Update is called once per frame
	void Update () {
		line.SetPosition(0, transform.position + startOffset);
		line.SetPosition(1, trackObject.transform.position);

		try
		{
			if (boxIndexActivate >= 0)
			{
				if (UiBox.box.index == boxIndexActivate)
					line.enabled = true;
				else
					line.enabled = false;
			}
		}
		catch { /*Debug.Log("UiBox was not assigned");*/ }

		try
		{
			if (dockTypeActivate != "null")
			{
				if (data.currentLogger.GetComponent<dataRecordingController>().activeDockStyle == dockTypeActivate)
					line.enabled = true;
				else
					line.enabled = false;
			}
		}
		catch { /*Debug.Log("DataMaster was not assigned");*/ }
		
	}
}
