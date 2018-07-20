using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;

public class dataRecorder : MonoBehaviour {

    public bool noOverwrite = true;
    public bool compareToDock = true;
    bool stopWriting = false; 
    public bool recordAngles = false;
    public bool anglesRecorded = false;
    public bool readyToExportData = false;

    // These must match in length
    public GameObject[] angleObjects;
    public GameObject[] dockAngleObjects;
    public static int numberOfAngles = 7;
    public GameObject shape;
    public GameObject dockShape;
    public Vector3 shapeOrientation;

    [System.Serializable]
    public class TextLog : System.Object
    {
        public string path = "Assets/Resources/";
        public string fileName = "Report_Raw";
        public string cellSeperatorType = "\t";
        public string exportedText;

        public string testerName;
        public string testTimestamp;
        public string dockShapeStyle;

        public bool deviceVisibility;
    }
    public TextLog textLog = new TextLog();

    [System.Serializable]
    public class AngleSummary : System.Object
    {
        public float[] correctAngles = new float[numberOfAngles];
        public float correctAngleSum;
        public float[] currAngles = new float[numberOfAngles];
        public float[] oversteer = new float[numberOfAngles];
        public float[] shapeRot = new float[3];
    }
    public AngleSummary angleSummary = new AngleSummary();

    // Efficiency factors
    [System.Serializable]
    public class Efficiency : System.Object
    {
        public float biggestOversteer = 0;
        public float secondsTaken;
        public string completionTime;
        public List<float> rawCompletionTime;
        public float[] precision = new float[numberOfAngles];

        public int deviceTwitchSampleSize = 10;
        public int deviceTwitchCounter = 0;
        public DataTable deviceTwitch = new DataTable();
    }
    public Efficiency efficiency = new Efficiency();

    // Efficiency results
    [System.Serializable]
    public class FinalSummary : System.Object
    {
        public float timeTaken;
        public float shapePrecision;
        public float orientationPrecision;
        public float precision;
        public float angleDifficulty;
        // Three angles with 150 total possible bending in each joint + the total possible 180 difference with correctangle orientations
        public float maxAngleDifficulty = (3f * 150f) + (3f * 180f);
        public float totalDifficulty;
        public float MVPenalty;
        public float TREPenalty;
        public float timePenalty = 1.0f;
        public float efficiency;

        public float deviceError;
        public float MV;
        public int TRE;

        public string testerName;
        public string testTimestamp;
        public string dockShapeStyle;

        public string deviceVisibility;
    }
    public FinalSummary finalResults = new FinalSummary();

    //+++ Mass Table Compiler Method
    //+++ Main Table
    public DataTable allData = new DataTable();
    public DataTable allDataMV = new DataTable();
    public DataTable allDataTRE = new DataTable();

    // Start
    void Start() {
        // Add filename to path
        textLog.path += textLog.fileName + ".txt";

        // Clear File before using
        fileEditor.Clear(textLog.path);
        textLog.exportedText = "";
    }

    // Update
    void Update() {
        // Get the dockstyle so we know what it is before we write to finalResults
        textLog.dockShapeStyle = this.GetComponentInParent<dataRecordingController>().activeDockStyle;

        // Get the dock visibility 
        textLog.deviceVisibility = this.GetComponentInParent<dataRecordingController>().virtualDeviceVisible;

        // Get the testername from the dataController
        textLog.testerName = this.GetComponentInParent<testDataGUI>().name;

        // Get the real world time and assign to this textlog
        DateTime now = DateTime.Now;
        textLog.testTimestamp = now.ToString("yyyy-MM-dd_HH:mm:ss");

        // Always reading virtual objects position every frame
        GetAngles();

        // Accumulate data from GetAngles to gather twitch data
        if (efficiency.deviceTwitchCounter < efficiency.deviceTwitchSampleSize 
        && !this.GetComponentInParent<dataRecordingController>().errorGathered)
        {
            efficiency.deviceTwitch.row.Add(angleSummary.currAngles.ToList());

            efficiency.deviceTwitchCounter++;

            if (efficiency.deviceTwitchCounter == efficiency.deviceTwitchSampleSize)
            {
                finalResults.deviceError = Mathf.Abs(DeviceTwichCalibration(efficiency.deviceTwitch));

                // This line assures the error is only gathered once per session
                this.GetComponentInParent<dataRecordingController>().errorGathered = true;
#if UNITY_EDITOR
                Debug.Log("device error = " + finalResults.deviceError);
#endif
            }
        }
        

        // you can call AngleRecord seperately (for testing the script only)
        if (recordAngles && !stopWriting)
        {
            AngleRecord();
            if (compareToDock)
                GetDockAngles();
            readyToExportData = true;

            // efficiency framerate measurements
            efficiency.secondsTaken += 1.0f * Time.smoothDeltaTime;

        }

        // Once Angle Recording stops the report is generated
        if (readyToExportData && !recordAngles)
        {
            // Calculate final efficiency results from data tables
            efficiency.completionTime = NiceTimeFromSeconds(efficiency.secondsTaken);
            efficiency.rawCompletionTime = RawTimeFromSeconds(efficiency.secondsTaken);

            // Export Report
            ExportData();
            readyToExportData = false;

            // If noOverwrite is enabled you cannot write over the test data because of this bool that's tripped
            if (noOverwrite)
            {
                stopWriting = true;
            }

            // Create a new test when a test finishes & creates a new dock shape for that new test
            this.GetComponentInParent<dataRecordingController>().NewTest();
            this.GetComponentInParent<dataRecordingController>().NewDockShape();

            anglesRecorded = true;
        }
    }

    ///// GET FILE METADATA ////
    public void MetaData()
    {
        ///////////////////////////////////////////////////// Take from Controller as Editable Fields
    }

    //// Re-Calibrate Device ////
    public void RecalibrateDevice()
    {   
        shape.GetComponent<IMU>().Callibrate();
        shape.GetComponent<IMU>().Callibrate();
        shape.GetComponent<IMU>().Callibrate();

        shape.GetComponent<IMU>().BendReset();
        shape.GetComponent<IMU>().Callibrate();
    }
    
    //// Device Twitch Calibration ////
    float DeviceTwichCalibration(DataTable deviceData)
    {
        List<float> angleSums = new List<float>(numberOfAngles);
        List<float> angleAvgs = new List<float>(numberOfAngles);

        for (int i = 0; i < numberOfAngles; i++)
        {
            angleSums.Add(0);
            angleAvgs.Add(0);
        }

        float angleAvg = 0;

        for (int x = 0; x < deviceData.row.Count; x++)
        {
            for (int y = 0; y < deviceData.row[x].Count; y++)
            {
                angleSums[y] += (deviceData.row[x][y]);
            }
        }

        for (int i = 0; i < angleSums.Count; i++)
        {
            angleAvgs.Add(angleSums[i] / deviceData.row.Count);
        }

        for (int i = 0; i < angleAvgs.Count; i++)
        {
            angleAvg += angleAvgs[i];
        }

        angleAvg = angleAvg / angleAvgs.Count;

        return angleAvg;
    }

    //// READ ANGLE DATA ////
    void GetAngles()
    {
        // Remove ProtratorAngle() to get positive/raw angles
        // Get overall object orientation
        angleSummary.shapeRot[0] = ProtractorAngle(shape.transform.localRotation.eulerAngles.x);
        angleSummary.shapeRot[1] = ProtractorAngle(shape.transform.localRotation.eulerAngles.y);
        angleSummary.shapeRot[2] = ProtractorAngle(shape.transform.localRotation.eulerAngles.z);

        // Fetch data for currAngles from the virtual objects by their z value in unity
        for (int i = 0; i < numberOfAngles; i++)
        {
            // Angles from shape
            if (i < numberOfAngles - angleSummary.shapeRot.Length)
            {
                angleSummary.currAngles[i] = ProtractorAngle(angleObjects[i].transform.localRotation.eulerAngles.z);
            }
            // Angles from orientation
            else
            {
                angleSummary.currAngles[i] = angleSummary.shapeRot[i - (angleSummary.currAngles.Length - angleSummary.shapeRot.Length)];
            }
        }


    }


    // Get the dock's current angles
    void GetDockAngles()
    {
        for (int i = 0; i < angleSummary.correctAngles.Length; i++)
        {
            // Make last three correct angles from dock orientation
            if (i == angleSummary.correctAngles.Length - 3)
            {
                angleSummary.correctAngles[i] = ProtractorAngle(dockShape.transform.localRotation.eulerAngles.x);
            }
            else if (i == angleSummary.correctAngles.Length - 2)
            {
                angleSummary.correctAngles[i] = ProtractorAngle(dockShape.transform.localRotation.eulerAngles.y);
            }
            else if (i == angleSummary.correctAngles.Length - 1)
            {
                angleSummary.correctAngles[i] = ProtractorAngle(dockShape.transform.localRotation.eulerAngles.z);
            }

            // Make first four correct angles from dockShapeAngles 
            else
            {
                angleSummary.correctAngles[i] = ProtractorAngle(dockAngleObjects[i].transform.localRotation.eulerAngles.z);
            }
        }
    }




    //// COMPARE ANGLES ////
    void CompareAngles()
    {
        for (int i = 0; i < efficiency.precision.Length; i++)
        {
            efficiency.precision[i] = ProtractorAngle(Mathf.Abs(angleSummary.correctAngles[i] - angleSummary.currAngles[i]));
        }
    }



    //// COMPILE CURRANGLES INTO TABLE ////
    // Take total angles recorded every second and compile into single line array of angles in time increments
    void AngleRecord()
    {
        ////+++ ADD RECORDED ANGLE LINES [NEW]      
        //+++ Add to official Data Table for Angles
        allData.row.Add(angleSummary.currAngles.ToList());       
    }






    //// WRITING DATA TO FILE ////
    // Create Single Mass String
    //+++ SPLIT THIS WHOLE THING INTO BASIC FLOATS AND STRINGS TO SEND TO THE SUB TABLES AND LATER COMPILE AS A EXPORT STRING
    void ExportData()
    {
        fileEditor.Clear(textLog.path);
        textLog.exportedText = "";

        // Get and fill the test metadata
        MetaData();

        ////+++ ADD RECORDED ANGLE LINES [NEW]  
        /// FIRST ANGLES LINE
        textLog.exportedText += "\n" + "Angles Over Time";
        allData.columnNames.Add("Frames");
        foreach (GameObject angle in angleObjects)
            allData.columnNames.Add(angle.name);
        allData.columnNames.Add("Docking x");
        allData.columnNames.Add("Docking y");
        allData.columnNames.Add("Docking z");

        /// ADD ANGLE DATA
        textLog.exportedText += textTableCompiler.FormatTable(allData, true, false, 
                                                            allData.columnNames, textLog.cellSeperatorType, "\n");

        
        // Analyse Data
        AnalyseData();

        /// FIRST MV ANGLES LINE
        //allDataMV.columnNames.Add("Angle MV instances");
        textLog.exportedText += "\n" + "\n" + "Angle MV instances";
        foreach (GameObject angle in angleObjects)
            allDataMV.columnNames.Add(angle.name);
        allDataMV.columnNames.Add("Docking x");
        allDataMV.columnNames.Add("Docking y");
        allDataMV.columnNames.Add("Docking z");

        /// ADD MV ANGLE DATA
        textLog.exportedText += textTableCompiler.FormatTable(allDataMV, false, true, 
                                                            allDataMV.columnNames, textLog.cellSeperatorType, "\n");

        textLog.exportedText += "Total MV Amount" + textLog.cellSeperatorType + finalResults.MV + "\n";

        /// FIRST TRE ANGLES LINE
        //allDataTRE.columnNames.Add("Angle TRE instances");
        textLog.exportedText += "\n" + "\n" + "Angle TRE instances";
        foreach (GameObject angle in angleObjects)
            allDataTRE.columnNames.Add(angle.name);
        allDataTRE.columnNames.Add("Docking x");
        allDataTRE.columnNames.Add("Docking y");
        allDataTRE.columnNames.Add("Docking z");
        allDataMV.columnNames.Add("Total TRE Count");

        /// ADD TRE ANGLE DATA
        textLog.exportedText += textTableCompiler.FormatTable(allDataTRE, false, true, 
                                                            allDataTRE.columnNames, textLog.cellSeperatorType, "\n");

        textLog.exportedText += "Total TRE Count" + textLog.cellSeperatorType + finalResults.TRE + "\n";

        /// FIRST CORRECT ANGLE LINE
        textLog.exportedText += "\n";
        textLog.exportedText += "\n" + "Correct Angles" + textLog.cellSeperatorType;
        for (int i = 0; i < angleSummary.correctAngles.Length; i++)
        {

            if (i < angleObjects.Length)
            {
                textLog.exportedText += angleObjects[i].name;
            }
            else
            {
                if (i == angleSummary.correctAngles.Length - 3)
                {
                    textLog.exportedText += "Docking x";
                }
                else if (i == angleSummary.correctAngles.Length - 2)
                {
                    textLog.exportedText += "Docking y";
                }
                else if (i == angleSummary.correctAngles.Length - 1)
                {
                    textLog.exportedText += "Docking z";
                }
            }

            // tab between each angle data to seperate into columns
            if (i < angleSummary.correctAngles.Length - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        /// CORRECT ANGLE LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        for (int i = 0; i < angleSummary.correctAngles.Length; i++)
        {
            textLog.exportedText += angleSummary.correctAngles[i];

            // tab between each angle data to seperate into columns
            if (i < angleSummary.correctAngles.Length - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        /// EXECUTE PRECISION CALCULATION
        CompareAngles();

        /// FIRST PRECISION LINE
        textLog.exportedText += "\n";
        textLog.exportedText += "\n" + "Angle Error" + textLog.cellSeperatorType;
        for (int i = 0; i < efficiency.precision.Length; i++)
        {

            if (i < angleObjects.Length)
            {
                textLog.exportedText += angleObjects[i].name;
            }
            else
            {
                if (i == efficiency.precision.Length - 3)
                {
                    textLog.exportedText += "Docking x";
                }
                else if (i == efficiency.precision.Length - 2)
                {
                    textLog.exportedText += "Docking y";
                }
                else if (i == efficiency.precision.Length - 1)
                {
                    textLog.exportedText += "Docking z";
                }
            }

            // tab between each angle data to seperate into columns
            if (i < efficiency.precision.Length - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        /// PRECISION LINES
        textLog.exportedText += "\n" + "" + textLog.cellSeperatorType;
        for (int i = 0; i < efficiency.precision.Length; i++)
        {
            textLog.exportedText += efficiency.precision[i];

            // tab between each angle data to seperate into columns
            if (i < efficiency.precision.Length - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        /// FIRST TIME LINE
        textLog.exportedText += "\n";
        textLog.exportedText += "\n" + "Time Taken" + textLog.cellSeperatorType;
        for (int i = 0; i < efficiency.rawCompletionTime.Count + 2; i++)
        {
            if (i == 0)
            {
                textLog.exportedText += "Hours";
            }

            else if (i == 1)
            {
                textLog.exportedText += "Minutes";
            }

            else if (i == 2)
            {
                textLog.exportedText += "Seconds";
            }

            else if (i == 3)
            {
                textLog.exportedText += "Milliseconds";
            }

            else if (i == 4)
            {
                textLog.exportedText += "Seconds float";
            }

            else if (i == 5)
            {
                textLog.exportedText += "Completion time";
            }

            // tab between each angle data to seperate into columns
            if (i < efficiency.rawCompletionTime.Count + 2 - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        /// TIME LINES
        textLog.exportedText += "\n" + "" + textLog.cellSeperatorType;
        for (int i = 0; i < efficiency.rawCompletionTime.Count + 2; i++)
        {
            // first name
            if (i < efficiency.rawCompletionTime.Count)
            {
                textLog.exportedText += efficiency.rawCompletionTime[i];
            }
            else
            {
                if (i == 4)
                {
                    textLog.exportedText += efficiency.secondsTaken;
                }
                else if (i == 5)
                {
                    textLog.exportedText += efficiency.completionTime;
                }
            }

            // tab between each angle data to seperate into columns
            if (i < efficiency.rawCompletionTime.Count + 2 - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        // Calculate final results and efficiency scores
        FinalResults();

        /// FIRST SUMMARY LINE
        textLog.exportedText += "\n" + "\n" + "Final Results" + textLog.cellSeperatorType +
                                            "Tester Name" + textLog.cellSeperatorType +
                                            "Time of Test" + textLog.cellSeperatorType +
                                            "Dock Shape Style" + textLog.cellSeperatorType +
                                            "Virtual Device Visible" + textLog.cellSeperatorType +
                                            "Time Taken" + textLog.cellSeperatorType +
                                            "Shape Precision %" + textLog.cellSeperatorType +
                                            "Orientation Precision %" + textLog.cellSeperatorType +
                                            "Overall Precision %" + textLog.cellSeperatorType + 
                                            "Efficiency %" + textLog.cellSeperatorType + 
                                            "Difficulty" + textLog.cellSeperatorType + 
                                            "Total MV" + textLog.cellSeperatorType + 
                                            "Total TRE" + textLog.cellSeperatorType;

         /// SUMMARY LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.testerName + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.testTimestamp + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.dockShapeStyle + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.deviceVisibility + textLog.cellSeperatorType;
        textLog.exportedText += efficiency.completionTime + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.shapePrecision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.orientationPrecision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.precision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.efficiency + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.totalDifficulty + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.MV + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.TRE + textLog.cellSeperatorType;

        // For GUI presentation
        string guiResults = "\n" + "\n" + this.name + " Results:" + textLog.cellSeperatorType +
                                        "Tester Name" + textLog.cellSeperatorType +
                                        "Time of Test" + textLog.cellSeperatorType +
                                        "Dock Shape Style" + textLog.cellSeperatorType +
                                        "Virtual Device Visible" + textLog.cellSeperatorType +
                                        "Time Taken" + textLog.cellSeperatorType +

                                        "\n" + textLog.cellSeperatorType +
                                        finalResults.testerName + textLog.cellSeperatorType +
                                        finalResults.testTimestamp + textLog.cellSeperatorType +
                                        finalResults.dockShapeStyle + textLog.cellSeperatorType +
                                        finalResults.deviceVisibility + textLog.cellSeperatorType +
                                        efficiency.completionTime + textLog.cellSeperatorType +

                                        "\n" + "\n" + textLog.cellSeperatorType + "Shape Precision %" + textLog.cellSeperatorType +
                                        "Orientation Precision %" + textLog.cellSeperatorType +
                                        "Overall Precision %" + textLog.cellSeperatorType +
                                        

                                        "\n" + textLog.cellSeperatorType + finalResults.shapePrecision + textLog.cellSeperatorType +
                                        finalResults.orientationPrecision + textLog.cellSeperatorType +
                                        finalResults.precision + textLog.cellSeperatorType +
                                        

                                        "\n" + "\n" + textLog.cellSeperatorType + "Efficiency %" + textLog.cellSeperatorType + 
                                        "Difficulty" + textLog.cellSeperatorType + 
                                        "Total MV" + textLog.cellSeperatorType + 
                                        "Total TRE" + textLog.cellSeperatorType +

                                        "\n" + textLog.cellSeperatorType + finalResults.efficiency + textLog.cellSeperatorType +
                                        finalResults.totalDifficulty + textLog.cellSeperatorType +
                                        finalResults.MV + textLog.cellSeperatorType +
                                        finalResults.TRE + textLog.cellSeperatorType;

        // SEND DATA TO THE GUI
        this.transform.parent.GetComponent<testDataGUI>().testData.Add("\n" + guiResults + "\n" + "\n" + "\n");

        // Send the data
        fileEditor.Append(textLog.path, textLog.exportedText);

        // Update the debug and inspector
        fileEditor.Read(textLog.path);
#if UNITY_EDITOR
        fileEditor.UpdateEditor(textLog.path, textLog.fileName);
#endif
    }

    //// Analyse Data ////
    public void AnalyseData()
    {

        for(int col = 0; col < allData.row[0].Count; col++)
        {
            // scale the MV error by the device's initial error (twitch)
            allDataMV.row.Add(AnalyseMV(allData, col, 2 + finalResults.deviceError));
            // scale the TRE zone by the device's initial error (twitch)
            allDataTRE.row.Add(AnalyseTRE(allData, col, 5 + finalResults.deviceError));
        }

        finalResults.MV = CountMV(allDataMV);
        finalResults.TRE = CountTRE(allDataTRE);
    }

    // Target Re-Entry Calculator
    public List<float> AnalyseTRE(DataTable data, int columnNumber, float zoneSize)
    {
        // is the angle close to what it's supposed to be within the zoneSize
        // first time it enters it triggers the distance away from the zoneSize it left when leaving the +/- zoneSize
        List<float> TRE = new List<float>();
        List<bool> inZones = new List<bool>();
        bool firstEntry = false;
        bool inZone = false;
        bool lastState = false;

        // for every row of frame search for the indicated column and analyse that data
        for(int x = 0; x < data.row.Count; x++)
        {
            if(data.row[x][columnNumber] >= angleSummary.correctAngles[columnNumber] - zoneSize 
            && data.row[x][columnNumber] <= angleSummary.correctAngles[columnNumber] + zoneSize)
            {
                if(firstEntry)
                    inZone = true;

                if(!firstEntry)
                    firstEntry = true;
            }
            else
            {
                inZone = false;
            }

            if(lastState != inZone && firstEntry)
            {
                TRE.Add(x);
            }

            lastState = inZone;

            inZones.Add(inZone);
        }

        return TRE;
    }

    // Total number of TREs
    public int CountTRE(DataTable data)
    {
        int numTRE = 0;

        // count only non-buggy TREs and actual numbers
        foreach (List<float> row in data.row)
            foreach (float cell in row)
                if (cell != null && cell != 1)
                    numTRE++;

        return numTRE;
    }

    // Movement Variability (Turnback) Calculator
    public List<float> AnalyseMV(DataTable data, int columnNumber, float turnBackAngle)
    {
        List<float> MV = new List<float>();
        float currData = 0;
        float lastData = 0;

        // for every row of frame search for the indicated column and analyse that data
        for(int x = 0; x < data.row.Count; x++)
        {
            currData = data.row[x][columnNumber];

            // if angle turnback (movement variability) has occurrred then record it
            if(Mathf.Abs(Mathf.Abs(currData) - Mathf.Abs(lastData)) > turnBackAngle)
            {
                MV.Add(Mathf.Abs(currData - lastData));
            }

            lastData = currData;
        }

        return MV;
    }

    // Total amount of MV
    public float CountMV(DataTable data)
    {
        float amountMV = 0;

        foreach (List<float> row in data.row)
            foreach (float cell in row)
                if (cell != null)
                    amountMV += cell;

        return amountMV;
    }

    //// FINAL RESULTS MATH ////
    public void FinalResults()
    {
        // Basic metadata
        finalResults.testerName = textLog.testerName;
        finalResults.testTimestamp = textLog.testTimestamp;
        finalResults.dockShapeStyle = textLog.dockShapeStyle;
        finalResults.deviceVisibility = textLog.deviceVisibility.ToString();

        // timetaken math
        finalResults.timeTaken = efficiency.secondsTaken;

        // precision math (out of 100) | the 180 is maximum error possible | 100f is to move up the decimals so add a % in the formatting
        // we want the amount of error angle absolutes on average thus we ABS() each precision value for this measurement before avg
        float[] angleErrors = efficiency.precision;
        for (int i = 0; i < angleErrors.Length; i++){ angleErrors[i] = Mathf.Abs(angleErrors[i]); }

        // New precision based on innaccuracy totals
        finalResults.shapePrecision = 100f - (((Sum(0, angleErrors.Length - 3, angleErrors)) / (180 * 4)) * 100f);
        finalResults.orientationPrecision = 100f - (((Sum(angleErrors.Length - 2, angleErrors.Length, angleErrors)) / (180 * 3)) * 100f);
        finalResults.precision = 100f - (((Sum(0, angleErrors.Length, angleErrors)) / (180 * numberOfAngles)) * 100f);

        
        // corrangle totals must be positive numbers to judge total positive deflection (deltas)
        float[] corrAngles = angleSummary.correctAngles;
        for (int a = 0; a < corrAngles.Length; a++)
        {
            corrAngles[a] = Mathf.Abs(corrAngles[a]);
        }

        // difficulty calculations based on how much rotation is needed from 0-1
        angleSummary.correctAngleSum = Sum(0, corrAngles.Length, corrAngles);
        finalResults.angleDifficulty = angleSummary.correctAngleSum;
        finalResults.totalDifficulty = finalResults.angleDifficulty / finalResults.maxAngleDifficulty;
        //10-20 -> 5-10 %
        finalResults.TREPenalty = finalResults.TRE / 2;
        //200-2000 -> 2-20 %
        finalResults.MVPenalty = finalResults.MV / 100;

        // efficiency math (reduce the effect of timeTaken's reduction of the efficiency rating for better numbers)
        // finalResults.efficiency = finalResults.precision - (finalResults.timeTaken * (finalResults.timePenalty * finalResults.totalDifficulty));
        // new efficiency based on time penalty relative to angle distance needed to travel [scale time penalty for eff tweaking]
        finalResults.efficiency = 100f -
                                (finalResults.timeTaken * (finalResults.timePenalty * finalResults.totalDifficulty));
        finalResults.efficiency -= ((finalResults.TREPenalty + finalResults.MVPenalty) * finalResults.totalDifficulty);
    }




    /// GENERIC FUNCTIONS /// 

    //// MATH OPERATIONS ////
    // Give nice protractor angles (180 relevant) // THIS COULD BE COMPLETELY WRONG
    float ProtractorAngle(float angle)
    {
        // get the remainder of an angle within a 0 to 360 degree sweep
        angle = angle % 360;
        // get the angle relative to which side of the 180 line it lies on
        angle = (angle > 180) ? angle - 360 : angle;
        // give a reasonable angle back
        return angle;
    }

    // Get a nice time from seconds as a fromatted single string
    string NiceTimeFromSeconds(float seconds)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
    }

    // Get a raw time from seconds as a list of floats
    List<float> RawTimeFromSeconds(float seconds)
    {
        System.TimeSpan t = System.TimeSpan.FromSeconds(seconds);
        List<float> rawTime = new List<float>(4);
        rawTime.Add(t.Hours);
        rawTime.Add(t.Minutes);
        rawTime.Add(t.Seconds);
        rawTime.Add(t.Milliseconds);
        return rawTime;
    }

    // Get an average with no weight
    public float UnweightedAverage(params float[] numbers)
    {
        float total = 0;
        foreach (float n in numbers)
        {
            total += n;
        }

        return total / numbers.Length;
    }

    // Get the sum of an array
    public float Sum(int start, int finish, params float[] numbers)
    {
        float total = 0;

        for (int n = start; n < finish; n++)
        {
            total += numbers[n];
        }

        return total;
    }
}
