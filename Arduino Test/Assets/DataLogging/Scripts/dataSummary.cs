using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class dataSummary : MonoBehaviour {

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
        fileEditor.Clear(textLog.path);
        textLog.exportedText = "";
    }

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

        string guiResults = "All Reports Averages" + textLog.cellSeperatorType + 
                            "Time Taken (avg)" + textLog.cellSeperatorType + 
                            "Precision % (avg)" + textLog.cellSeperatorType + 
                            "Efficiency % (avg)" + textLog.cellSeperatorType;

        /// SUMMARY LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        textLog.exportedText += summary.timeTaken + textLog.cellSeperatorType;
        textLog.exportedText += summary.precision + textLog.cellSeperatorType;
        textLog.exportedText += summary.efficiency + textLog.cellSeperatorType;

        guiResults += "\n" + textLog.cellSeperatorType + 
                            summary.timeTaken + textLog.cellSeperatorType + 
                            summary.precision + textLog.cellSeperatorType + 
                            summary.efficiency + textLog.cellSeperatorType;

        // SEND DATA TO THE GUI
        this.GetComponent<testDataGUI>().testData.Add("Test Results Summary: " + "\n" + guiResults + "\n" + "\n" + "\n");

        // Send the data
        fileEditor.Append(textLog.path, textLog.exportedText);

        // Update the debug and inspector
        fileEditor.Read(textLog.path);
#if UNITY_EDITOR
        fileEditor.UpdateEditor(textLog.path, textLog.fileName);
#endif
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
    
}
