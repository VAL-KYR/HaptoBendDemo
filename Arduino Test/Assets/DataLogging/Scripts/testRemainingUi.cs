using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testRemainingUi : MonoBehaviour {
	Text textItem;
	List<GameObject> loggers;
	GameObject dataMasterRef;
	public GameObject activeLogger;
	int trialsTotal = 0;
	int trailsDone = 0;
	public int trialsLeft = 0;

	// Use this for initialization
	void Start () {
		dataMasterRef = GameObject.FindGameObjectWithTag("dataMaster");
		textItem = transform.GetChild(0).GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		if (dataMasterRef.GetComponent<dataMaster>().dataLoggers.Count > 0)
		{
			loggers = dataMasterRef.GetComponent<dataMaster>().dataLoggers;
			
			foreach (GameObject log in loggers)
			{
				if (log.activeSelf)
				{
					activeLogger = log;
				}
			}
			
			trialsLeft = ((activeLogger.GetComponent<dataRecordingController>().testCateg.InvisLtdLimit +
							activeLogger.GetComponent<dataRecordingController>().testCateg.InvisPresetLimit +
							activeLogger.GetComponent<dataRecordingController>().testCateg.VisLtdLimit +
							activeLogger.GetComponent<dataRecordingController>().testCateg.VisPresetLimit) - 
							(activeLogger.GetComponent<dataRecordingController>().testCateg.InvisLtdRandom.Count + 
							activeLogger.GetComponent<dataRecordingController>().testCateg.InvisPresets.Count + 
							activeLogger.GetComponent<dataRecordingController>().testCateg.VisLtdRandom.Count + 
							activeLogger.GetComponent<dataRecordingController>().testCateg.VisPresets.Count));

			
			textItem.text = "Trials Remaining: " + trialsLeft;

		}
		
	}
}
