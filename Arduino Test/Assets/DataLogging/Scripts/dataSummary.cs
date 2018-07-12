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
        public List<float> allShapePrecision;
        public List<float> allOrientationPrecision;
        public List<float> allPrecision;
        public List<float> allEfficiency;
        public List<float> allTotalDifficulty;
        public List<float> allMV;
        public List<float> allTRE;

        public float timeTaken;
        public float shapePrecision;
        public float orientationPrecision;
        public float precision;
        public float efficiency;
        public float totalDifficulty;
        public float MV;
        public int TRE;
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
        /// FIRST SUMMARIES LINE
        textLog.exportedText += "All Report Averages" + textLog.cellSeperatorType + 
                                "Time Taken (avg)" + textLog.cellSeperatorType +
                                "Shape Precision % (avg)" + textLog.cellSeperatorType +
                                "Orientation Precision % (avg)" + textLog.cellSeperatorType +
                                "Overall Precision % (avg)" + textLog.cellSeperatorType + 
                                "Efficiency % (avg)" + textLog.cellSeperatorType + 
                                "Total Difficulty (avg)" + textLog.cellSeperatorType + 
                                "MV (avg)" + textLog.cellSeperatorType + 
                                "TRE (avg)" + textLog.cellSeperatorType;

        // skip the first test to prevent counting the blank next test
        for (int i = 0; i < summary.allTimeTaken.Count; i++)
        {
            textLog.exportedText += "\n";

            textLog.exportedText += textLog.cellSeperatorType;

            textLog.exportedText += summary.allTimeTaken[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allShapePrecision[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allOrientationPrecision[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allPrecision[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allEfficiency[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allTotalDifficulty[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allMV[i] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allTRE[i] + textLog.cellSeperatorType;
        }

        // seperate for new section
        textLog.exportedText += "\n" + "\n";

        /// FIRST SUMMARY SUM LINE
        textLog.exportedText += "Session Averages" + textLog.cellSeperatorType + 
                                "Time Taken (avg)" + textLog.cellSeperatorType +
                                "Shape Precision % (avg)" + textLog.cellSeperatorType +
                                "Orientation Precision % (avg)" + textLog.cellSeperatorType +
                                "Overall Precision % (avg)" + textLog.cellSeperatorType + 
                                "Efficiency % (avg)" + textLog.cellSeperatorType + 
                                "Total Difficulty (avg)" + textLog.cellSeperatorType + 
                                "MV (avg)" + textLog.cellSeperatorType + 
                                "TRE (avg)" + textLog.cellSeperatorType;

        string guiResults = "Session Averages" + textLog.cellSeperatorType + 
                            "Time Taken (avg)" + textLog.cellSeperatorType + 
                            "Shape Precision % (avg)" + textLog.cellSeperatorType +
                            "Orientation Precision % (avg)" + textLog.cellSeperatorType +
                            "Overall Precision % (avg)" + textLog.cellSeperatorType +
                            "Efficiency % (avg)" + textLog.cellSeperatorType + 
                            "Total Difficulty (avg)" + textLog.cellSeperatorType + 
                            "MV (avg)" + textLog.cellSeperatorType + 
                            "TRE (avg)" + textLog.cellSeperatorType;

        /// SUMMARY SUM LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType + 
                                summary.timeTaken + textLog.cellSeperatorType +
                                summary.shapePrecision + textLog.cellSeperatorType +
                                summary.orientationPrecision + textLog.cellSeperatorType +
                                summary.precision + textLog.cellSeperatorType + 
                                summary.efficiency + textLog.cellSeperatorType + 
                                summary.totalDifficulty + textLog.cellSeperatorType + 
                                summary.MV + textLog.cellSeperatorType + 
                                summary.TRE + textLog.cellSeperatorType;

        guiResults += "\n" + textLog.cellSeperatorType + 
                            summary.timeTaken + textLog.cellSeperatorType +
                            summary.shapePrecision + textLog.cellSeperatorType +
                            summary.orientationPrecision + textLog.cellSeperatorType +
                            summary.precision + textLog.cellSeperatorType + 
                            summary.efficiency + textLog.cellSeperatorType + 
                            summary.totalDifficulty + textLog.cellSeperatorType + 
                            summary.MV + textLog.cellSeperatorType + 
                            summary.TRE + textLog.cellSeperatorType;

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

        // skip the first test to prevent counting the blank next test
        for(int i = 0; i < summary.tests.Count - 1; i++)
        {
            summary.allTimeTaken.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.timeTaken);
            summary.allShapePrecision.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.shapePrecision);
            summary.allOrientationPrecision.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.orientationPrecision);
            summary.allPrecision.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.precision);
            summary.allEfficiency.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.efficiency);
            summary.allTotalDifficulty.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.totalDifficulty);
            summary.allMV.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.MV);
            summary.allTRE.Add(summary.tests[i].GetComponent<dataRecorder>().finalResults.TRE);
        }

        // Get averages of all data
        summary.timeTaken = unweightedAverage(summary.allTimeTaken);
        summary.shapePrecision = unweightedAverage(summary.allShapePrecision);
        summary.orientationPrecision = unweightedAverage(summary.allOrientationPrecision);
        summary.precision = unweightedAverage(summary.allPrecision);
        summary.efficiency = unweightedAverage(summary.allEfficiency);
        summary.totalDifficulty = unweightedAverage(summary.allTotalDifficulty);
        summary.MV = unweightedAverage(summary.allMV);
        summary.TRE = (int)unweightedAverage(summary.allTRE);
    }
    
}
