using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

public class dataMaster : MonoBehaviour {

	public GameObject dataLoggerPrefab;
	public List<GameObject> dataLoggers;

	[System.Serializable]
    public class TextLog : System.Object
    {
        public string path = "Assets/DataLogging/Reports/";
        public string cellSeperatorType = "\t";
		public string newLineType = "\n";
    }
    public TextLog textLog = new TextLog();

	// Use this for initialization
	void Start () {
		//ClearReports(textLog.path);
		fileEditor.ClearDir(textLog.path);
	}
	
	// Update is called once per frame
	void Update () {
		dataLoggers = GameObject.FindGameObjectsWithTag("dataLogger").ToList();
	}

	public void newDataLogger()
	{
		foreach (GameObject logger in dataLoggers)
			logger.SetActive(false);

		GameObject newDataLogger = Instantiate<GameObject>(dataLoggerPrefab, Vector3.zero, Quaternion.identity, this.transform);
		dataRecordingController dataLogPrefs = newDataLogger.GetComponent<dataRecordingController>();

		newDataLogger.name = "DataLogger_" + GetComponent<testDataGUI>().name;
		dataLogPrefs.filePath = textLog.path + GetComponent<testDataGUI>().name + "/";
	}
}
