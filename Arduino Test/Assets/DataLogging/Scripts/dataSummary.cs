using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class dataSummary : MonoBehaviour {

    public bool createReport = false;
    bool reportDone = false;

    [System.Serializable]
    public class TextLog : System.Object
    {
        public string path = "Assets/DataLogging/Reports/";
        public string fileName = "Report_Final_Summary";
        public string cellSeperatorType = "\t";
        public string exportedText;
    }
    public TextLog textLog = new TextLog();

    [System.Serializable]
    public class Summary : System.Object
    {
        public List<GameObject> tests;
        public List<float> allTimeTaken;
        public List<float> allPrecision;
        public List<float> allEfficiency;

        public float timeTaken;
        public float precision;
        public float efficiency;
    }
    public Summary summary = new Summary();

    // Use this for initialization
    void Start () {
        // Add filename to path
        textLog.path += textLog.fileName + ".txt";

        // Clear File before using
        Clear();
        textLog.exportedText = "";

        // Read the file before starting any testing
        //Read();
        UpdateEditor();
    }
	
	// Update is called once per frame
	void Update () {
        if (createReport && !reportDone)
        {
            CalculateFinalResults();
            ExportSummaryFile();
            reportDone = true;
        }
	}

    //// WHAT THE SCRIPT NEEDS TO DO ////
    // get the list of tests
    // get the data from each script
    /// compile the test results into a list of test precision ratings
    /// compile the test results into a list of test time ratings
    // compile the test results into an overall efficiency rating from the average of all efficencies

    /// GENERIC FUNCTIONS ///
    //// MATH OPERATIONS ////
    // Get an average with no weight
    public float unweightedAverage(List<float> numbers)
    {
        float total = 0;
        foreach (float n in numbers)
        {
            total += n;
        }

        return total / numbers.Count;
    }

    //// REPORT CALCULATION AND CREATION ////
    public void ExportSummaryFile()
    {
        /// FIRST SUMMARY LINE
        textLog.exportedText += "All Reports Averages" + textLog.cellSeperatorType;
        textLog.exportedText += "Time Taken (avg)" + textLog.cellSeperatorType;
        textLog.exportedText += "Precision % (avg)" + textLog.cellSeperatorType;
        textLog.exportedText += "Efficiency % (avg)" + textLog.cellSeperatorType;

        /// SUMMARY LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        textLog.exportedText += summary.timeTaken + textLog.cellSeperatorType;
        textLog.exportedText += summary.precision + textLog.cellSeperatorType;
        textLog.exportedText += summary.efficiency + textLog.cellSeperatorType;

        // Send the data
        Append(textLog.exportedText);

        // Update the debug and inspector
        Read();
        UpdateEditor();
    }

    public void CalculateFinalResults()
    {
        // Get all test results into lists
        summary.tests = this.GetComponent<dataRecordingController>().tests;
        foreach (GameObject t in summary.tests)
        {
           summary.allTimeTaken.Add(t.GetComponent<dataRecorder>().finalResults.timeTaken);
           summary.allPrecision.Add(t.GetComponent<dataRecorder>().finalResults.precision);
           summary.allEfficiency.Add(t.GetComponent<dataRecorder>().finalResults.efficiency);
        }

        // Get averages of all data
        summary.timeTaken = unweightedAverage(summary.allTimeTaken);
        summary.precision = unweightedAverage(summary.allPrecision);
        summary.efficiency = unweightedAverage(summary.allEfficiency);
    }

    //// FILE OPERATIONS ////
    // Clear the file
    void Clear()
    {
        // Clear file
        File.WriteAllText(textLog.path, "");
    }

    // Write a line
    void Append(string text)
    {
        //Write some text to the Report.txt file
        StreamWriter writer = new StreamWriter(textLog.path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    // Overwrite text
    void Write(string text)
    {
        //Write some text to the Report.txt file
        StreamWriter writer = new StreamWriter(textLog.path, true);
        writer.Write(text);
        writer.Close();
    }

#if UNITY_EDITOR
    // Update the inspector
    void UpdateEditor()
    {
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(textLog.path);
        TextAsset asset = Resources.Load(textLog.fileName) as TextAsset;
    }
#endif

    // Read to console
    void Read()
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(textLog.path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
