using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class dataSummary : MonoBehaviour {

    [System.Serializable]
    public class TextLog : System.Object
    {
        public string path = "Assets/DataLogging/Reports/";
        public string fileName = "Reports_Summary";
        public string cellSeperatorType = "\t";
        public string sendFile = "";
        public string exportedText;
    }
    public TextLog textLog = new TextLog();

    [System.Serializable]
    public class Summary : System.Object
    {
        public List<int> allTrialNumbers;
        public List<string> allTesterName;
        public List<string> allTestTimestamp;
        public List<string> allDockShapeStyle;
        public List<string> allDeviceVisibility;

        public List<float> allTimeTaken;
        public List<float> allShapePrecision;
        public List<List<float>> allCorrectAngles = new List<List<float>>();
        public List<float> allOrientationPrecision;
        public List<float> allPrecision;
        public List<float> allEfficiency;
        public List<float> allTotalDifficulty;
        public List<float> allMV;
        public List<List<float>> allMVAngles = new List<List<float>>();
        public List<float> allTRE;
        public List<List<float>> allTREAngles = new List<List<float>>();
        public string testerName;

        public float timeTaken;
        public float shapePrecision;
        public float orientationPrecision;
        public float precision;
        public float efficiency;
        public float totalDifficulty;
        public float MV;
        public List<float> MVJoints;
        public int TRE;
        public List<int> TREJoints;
    }
    public Summary summary = new Summary();

    // Use this for initialization
    void Start () {
        // Add filename to path
        textLog.path = GetComponent<dataRecordingController>().filePath;
        SetFileDestination(textLog.path, textLog.fileName, ".txt");

        // Clear File before using
        fileEditor.Clear(textLog.sendFile);
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

    //// ALL REPORT SUMMARY CREATION ////
    public void ExportTrials (string resultsName, bool createSumLines, bool textSeperation)
    {
        textLog.exportedText = "";

        /// FIRST SUMMARIES LINE
        if (createSumLines)
        {
            textLog.exportedText += "All " + resultsName + " Averages" + textLog.cellSeperatorType + 
                                "Trial Number" + textLog.cellSeperatorType + 
                                "Test Timestamp" + textLog.cellSeperatorType +
                                "Tester Name" + textLog.cellSeperatorType +  
                                "Dock Shape Style" + textLog.cellSeperatorType +
                                "Virtual Device Visible" + textLog.cellSeperatorType + 
                                "Time Taken" + textLog.cellSeperatorType +
                                "Shape Precision %" + textLog.cellSeperatorType +
                                "Orientation Precision %" + textLog.cellSeperatorType +
                                "Overall Precision %" + textLog.cellSeperatorType + 
                                "Dock Rotate Angle" + textLog.cellSeperatorType + 
                                "Dock ChildRotate Angle" + textLog.cellSeperatorType + 
                                "Dock RotateInverse Angle" + textLog.cellSeperatorType + 
                                "Dock ChildRotateInverse Angle" + textLog.cellSeperatorType + 
                                "Dock x Rotation" + textLog.cellSeperatorType + 
                                "Dock y Rotation" + textLog.cellSeperatorType + 
                                "Dock z Rotation" + textLog.cellSeperatorType + 
                                "Efficiency %" + textLog.cellSeperatorType + 
                                "Total Difficulty" + textLog.cellSeperatorType + 
                                "MV Avg" + textLog.cellSeperatorType + 
                                "MV Rotate Angle" + textLog.cellSeperatorType +
                                "MV ChildRotate Angle" + textLog.cellSeperatorType + 
                                "MV RotateInverse Angle" + textLog.cellSeperatorType + 
                                "MV ChildRotateInverse Angle" + textLog.cellSeperatorType + 
                                "MV x Rotation" + textLog.cellSeperatorType + 
                                "MV y Rotation" + textLog.cellSeperatorType +  
                                "MV z Rotation" + textLog.cellSeperatorType +
                                "TRE Avg" + textLog.cellSeperatorType + 
                                "TRE Rotate Angle" + textLog.cellSeperatorType +
                                "TRE ChildRotate Angle" + textLog.cellSeperatorType + 
                                "TRE RotateInverse Angle" + textLog.cellSeperatorType + 
                                "TRE ChildRotateInverse Angle" + textLog.cellSeperatorType + 
                                "TRE x Rotation" + textLog.cellSeperatorType + 
                                "TRE y Rotation" + textLog.cellSeperatorType +  
                                "TRE z Rotation" + textLog.cellSeperatorType;
        }
        

        // skip the first test to prevent counting the blank next test
        for (int x = 0; x < summary.allTimeTaken.Count; x++)
        {
            textLog.exportedText += "\n" + textLog.cellSeperatorType;

            textLog.exportedText += summary.allTrialNumbers[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allTestTimestamp[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allTesterName[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allDockShapeStyle[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allDeviceVisibility[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allTimeTaken[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allShapePrecision[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allOrientationPrecision[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allPrecision[x] + textLog.cellSeperatorType;

            // CORRECT JOINT BREAKDOWN [loop]
            for (int y = 0; y < summary.allCorrectAngles[x].Count; y++)
            {
                textLog.exportedText += summary.allCorrectAngles[x][y] + textLog.cellSeperatorType;
            }
            
            textLog.exportedText += summary.allEfficiency[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allTotalDifficulty[x] + textLog.cellSeperatorType;
            textLog.exportedText += summary.allMV[x] + textLog.cellSeperatorType;

            // MV PER JOINT BREAKDOWN [loop]
            for (int y = 0; y < summary.allMVAngles[x].Count; y++)
            {
                textLog.exportedText += summary.allMVAngles[x][y] + textLog.cellSeperatorType;
            }
            
            textLog.exportedText += summary.allTRE[x] + textLog.cellSeperatorType;

            // TRE PER JOINT BREAKDOWN [loop]
            for (int y = 0; y < summary.allTREAngles[x].Count; y++)
            {
                textLog.exportedText += summary.allTREAngles[x][y] + textLog.cellSeperatorType;
            }
        }

        // seperate for new section
        if (textSeperation)
            textLog.exportedText += "\n" + "\n";

        // SEND DATA TO THE FILE
        fileEditor.Append(textLog.sendFile, textLog.exportedText);

        // Update the debug and inspector
#if UNITY_EDITOR
        fileEditor.Read(textLog.sendFile);
        fileEditor.UpdateEditor(textLog.sendFile, textLog.fileName);
#endif
    }

    /// FINAL SUMMARIES CREATION ////
    public void ExportTrialSummaries (string resultsName, bool createSumLines, bool textSeperation)
    {
        textLog.exportedText = "";

        /// FIRST SUMMARY SUM LINE
        if (createSumLines)
        {
            textLog.exportedText += resultsName + " Averages" + textLog.cellSeperatorType + 
                                "Tester Names" + textLog.cellSeperatorType + 
                                "Time Taken (avg)" + textLog.cellSeperatorType +
                                "Shape Precision % (avg)" + textLog.cellSeperatorType +
                                "Orientation Precision % (avg)" + textLog.cellSeperatorType +
                                "Overall Precision % (avg)" + textLog.cellSeperatorType + 
                                "Efficiency % (avg)" + textLog.cellSeperatorType + 
                                "Total Difficulty (avg)" + textLog.cellSeperatorType + 
                                "MV (avg)" + textLog.cellSeperatorType + 
                                "MV Rotate Angle" + textLog.cellSeperatorType +
                                "MV ChildRotate Angle" + textLog.cellSeperatorType + 
                                "MV RotateInverse Angle" + textLog.cellSeperatorType + 
                                "MV ChildRotateInverse Angle" + textLog.cellSeperatorType + 
                                "MV x Rotation" + textLog.cellSeperatorType + 
                                "MV y Rotation" + textLog.cellSeperatorType +  
                                "MV z Rotation" + textLog.cellSeperatorType +
                                "TRE (avg)" + textLog.cellSeperatorType + 
                                "TRE Rotate Angle" + textLog.cellSeperatorType +
                                "TRE ChildRotate Angle" + textLog.cellSeperatorType + 
                                "TRE RotateInverse Angle" + textLog.cellSeperatorType + 
                                "TRE ChildRotateInverse Angle" + textLog.cellSeperatorType + 
                                "TRE x Rotation" + textLog.cellSeperatorType + 
                                "TRE y Rotation" + textLog.cellSeperatorType +  
                                "TRE z Rotation" + textLog.cellSeperatorType;
        }
        

        /// SUMMARY SUM LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType + 
                                summary.testerName + textLog.cellSeperatorType +
                                summary.timeTaken + textLog.cellSeperatorType +
                                summary.shapePrecision + textLog.cellSeperatorType +
                                summary.orientationPrecision + textLog.cellSeperatorType +
                                summary.precision + textLog.cellSeperatorType + 
                                summary.efficiency + textLog.cellSeperatorType + 
                                summary.totalDifficulty + textLog.cellSeperatorType;
        textLog.exportedText += summary.MV + textLog.cellSeperatorType; 
        
        foreach (float angle in summary.MVJoints)
            textLog.exportedText += angle + textLog.cellSeperatorType;

        textLog.exportedText += summary.TRE + textLog.cellSeperatorType;

        foreach (float angle in summary.TREJoints)
            textLog.exportedText += angle + textLog.cellSeperatorType;

        /*
        // SEND DATA TO THE GUI
        this.GetComponent<testDataGUI>().testData.Add(resultsName + " Summary:" + textLog.cellSeperatorType + guiResults + "\n" + "\n" + "\n");
        */

        // seperate for new section
        if (textSeperation)
            textLog.exportedText += "\n" + "\n";

        // SEND DATA TO THE FILE
        fileEditor.Append(textLog.sendFile, textLog.exportedText);

        // Update the debug and inspector
#if UNITY_EDITOR
        fileEditor.Read(textLog.sendFile);
        fileEditor.UpdateEditor(textLog.sendFile, textLog.fileName);
#endif
    }

    // When adding new summary data start by adding it here to these temporary export variables
    public void CalculateFinalResults(List<GameObject> testsToSummarize)
    {
        // skip the first test to prevent counting the blank next test
        for (int x = 0; x < testsToSummarize.Count; x++)
        {
            summary.allTrialNumbers.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.trialNumber);
            summary.allTesterName.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.testerName);
            summary.allTestTimestamp.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.testTimestamp);
            summary.allDockShapeStyle.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.dockShapeStyle);
            summary.allDeviceVisibility.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.deviceVisibility);
            
            summary.allTimeTaken.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.timeTaken);
            summary.allShapePrecision.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.shapePrecision);
            summary.allOrientationPrecision.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.orientationPrecision);

            // CORRECT JOINT BREAKDOWN
            summary.allCorrectAngles.Add(testsToSummarize[x].GetComponent<dataRecorder>().angleSummary.correctAngles.ToList());

            summary.allPrecision.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.precision);
            summary.allEfficiency.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.efficiency);
            summary.allTotalDifficulty.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.totalDifficulty);
            summary.allMV.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.MV);

            // MV PER JOINT BREAKDOWN
            summary.allMVAngles.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.MVAngles);

            summary.allTRE.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.TRE);

            // TRE PER JOINT BREAKDOWN
            summary.allTREAngles.Add(testsToSummarize[x].GetComponent<dataRecorder>().finalResults.TREAngles);
        }

        // Get averages of all data
        //summary.testerName = summary.allTesterName[0];
        List<string> priorNames = new List<string>();

        foreach (string name in summary.allTesterName)
        {
            if (!priorNames.Contains(name)) 
            {
                priorNames.Add(name);
                summary.testerName += name + " "; 
            }
        }


        summary.timeTaken = unweightedAverage(summary.allTimeTaken);
        summary.shapePrecision = unweightedAverage(summary.allShapePrecision);
        summary.orientationPrecision = unweightedAverage(summary.allOrientationPrecision);
        summary.precision = unweightedAverage(summary.allPrecision);
        summary.efficiency = unweightedAverage(summary.allEfficiency);
        summary.totalDifficulty = unweightedAverage(summary.allTotalDifficulty);
        summary.MV = unweightedAverage(summary.allMV); 
        summary.TRE = (int)unweightedAverage(summary.allTRE);

        // add joints MV averages
        for (int x = 0; x < summary.allMVAngles[0].Count; x++)
        {
            List<float> sum = new List<float>();

            for (int y = 0; y < summary.allMVAngles.Count; y++)
            {
                sum.Add(summary.allMVAngles[y][x]);
            }

            summary.MVJoints.Add(unweightedAverage(sum));
            sum.Clear();
        }
        // the average of all MVAngles by column

        // add joints TRE averages
        for (int x = 0; x < summary.allMVAngles[0].Count; x++)
        {
            List<float> sum = new List<float>();

            for (int y = 0; y < summary.allMVAngles.Count; y++)
            {
                sum.Add((float)summary.allTREAngles[y][x]);
            }

            summary.TREJoints.Add((int)unweightedAverage(sum));
            sum.Clear();
        }
        // the average of all TREAngles by column
    }

    /// Clear the data
    public void EraseResultsLists()
    {
        summary.testerName = "";

        summary.allTrialNumbers.Clear();
        summary.allTestTimestamp.Clear();
        summary.allTesterName.Clear();
        summary.allDockShapeStyle.Clear();
        summary.allDeviceVisibility.Clear();
        
        summary.allTimeTaken.Clear();
        summary.allShapePrecision.Clear();
        summary.allOrientationPrecision.Clear();
        summary.allPrecision.Clear();
        summary.allCorrectAngles.Clear();
        summary.allEfficiency.Clear();
        summary.allTotalDifficulty.Clear();
        summary.allMV.Clear();
        summary.allMVAngles.Clear();
        summary.MVJoints.Clear();
        summary.allTRE.Clear();
        summary.allTREAngles.Clear();
        summary.TREJoints.Clear();
    }

    /// Set filepath 
    public void SetFileDestination(string path, string fileName, string extension)
    {
        textLog.sendFile = path + fileName + extension;
        textLog.path = path;
        textLog.fileName = fileName;
    }
    
}
