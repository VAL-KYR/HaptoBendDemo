using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

public class dataMaster : MonoBehaviour {

	public GameObject dataLoggerPrefab;
	public List<GameObject> dataLoggers;
	public GameObject currentLogger;

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

		// Set Box State
        UiBox.box.index = 2;
	}
	
	// Update is called once per frame
	void Update () {
		dataLoggers = GameObject.FindGameObjectsWithTag("dataLogger").ToList();

		foreach(GameObject logger in dataLoggers)
		{
			if (logger.GetComponent<dataRecordingController>().enabled)
			{
				currentLogger = logger;
			}
		}
		
	}

	public void NewDataLogger(string loggerName)
	{
		GetComponent<testDataGUI>().FetchAction("New Logger - " + loggerName);

		foreach (GameObject logger in dataLoggers)
			logger.GetComponent<dataRecordingController>().enabled = false;

		GameObject newDataLogger = Instantiate<GameObject>(dataLoggerPrefab, Vector3.zero, Quaternion.identity, transform);
		dataRecordingController dataLogPrefs = newDataLogger.GetComponent<dataRecordingController>();

		newDataLogger.name = "DataLogger_" + loggerName;
		newDataLogger.GetComponent<dataRecordingController>().testCateg.VisLtdLimit = GetComponent<testDataGUI>().VisLtdRandom;
		newDataLogger.GetComponent<dataRecordingController>().testCateg.InvisLtdLimit = GetComponent<testDataGUI>().InvisLtdRandom;
        newDataLogger.GetComponent<dataRecordingController>().testCateg.VisPresetLimit = GetComponent<testDataGUI>().VisPresets;
        newDataLogger.GetComponent<dataRecordingController>().testCateg.InvisPresetLimit = GetComponent<testDataGUI>().InvisPresets;
		dataLogPrefs.filePath = textLog.path + loggerName + "/";
	}

	public void DeleteLogger(string suffix)
	{
		GameObject loggerToDelete = new GameObject();

		//foreach (GameObject logger in dataLoggers)
			//logger.GetComponent<dataRecordingController>().enabled = true;

		foreach (GameObject logger in dataLoggers)
			if (logger.name == "DataLogger_" + suffix)
				Destroy(logger);
				
		//foreach (GameObject logger in dataLoggers)
			//logger.GetComponent<dataRecordingController>().enabled = false;
	}

	public void DeleteTrial(string loggerName, string trialName)
	{
		foreach (GameObject logger in dataLoggers)
			if (logger.name == "DataLogger_" + loggerName)
				foreach (GameObject trial in logger.GetComponent<dataRecordingController>().tests)
					if (trial.name == "Test_" + trialName)
					{
						trial.name += "_Trashed";
						trial.GetComponent<dataRecorder>().finalResults.dockShapeStyle = "BAD DATA";
					}
						
	}
}