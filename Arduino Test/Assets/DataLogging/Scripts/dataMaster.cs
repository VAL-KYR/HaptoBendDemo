using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class dataMaster : MonoBehaviour {

	public GameObject dataLoggerPrefab;
	public List<GameObject> dataLoggers;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		dataLoggers = GameObject.FindGameObjectsWithTag("dataLogger").ToList();
	}

	public void newDataLogger()
	{
		foreach (GameObject logger in dataLoggers)
		{
			logger.SetActive(false);
		}

		GameObject newDataLogger = Instantiate<GameObject>(dataLoggerPrefab, Vector3.zero, Quaternion.identity, this.transform);

		newDataLogger.name = "DataLogger_" + GetComponent<testDataGUI>().name;
	}
}
