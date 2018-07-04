using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

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
        public float timePenalty = 1.0f;
        public float efficiency;
    }
    public FinalSummary finalResults = new FinalSummary();

    //+++ Mass Table Compiler Method
    //+++ Main Table
    public DataTable allData = new DataTable();

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

        // Always reading virtual objects position every frame
        GetAngles();

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

            // Analyse Data
            AnalyseData();

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


        ////+++ ADD RECORDED ANGLE LINES [NEW]  
        /// FIRST ANGLES LINE
        allData.columnNames.Add("Frames");
        foreach (GameObject angle in angleObjects)
            allData.columnNames.Add(angle.name);
        allData.columnNames.Add("Docking x");
        allData.columnNames.Add("Docking y");
        allData.columnNames.Add("Docking z");

        /// ADD ANGLE DATA
        textLog.exportedText += textTableCompiler.FormatTable(allData, true, 
                                                            allData.columnNames, textLog.cellSeperatorType, "\n");

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
                                            "Time Taken" + textLog.cellSeperatorType +
                                            "Shape Precision %" + textLog.cellSeperatorType +
                                            "Orientation Precision %" + textLog.cellSeperatorType +
                                            "Overall Precision %" + textLog.cellSeperatorType + 
                                            "Efficiency %" + textLog.cellSeperatorType;

        // For GUI presentation
        string guiResults = "\n" + "\n" + "Final Results" + textLog.cellSeperatorType +
                                        "Time Taken" + textLog.cellSeperatorType +
                                        "Shape Precision %" + textLog.cellSeperatorType +
                                        "Orientation Precision %" + textLog.cellSeperatorType +
                                        "Overall Precision %" + textLog.cellSeperatorType +
                                        "Efficiency %" + textLog.cellSeperatorType;

        /// SUMMARY LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        textLog.exportedText += efficiency.completionTime + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.shapePrecision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.orientationPrecision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.precision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.efficiency + textLog.cellSeperatorType;

        // For GUI presentation
        guiResults += "\n" + textLog.cellSeperatorType;
        guiResults += efficiency.completionTime + textLog.cellSeperatorType;
        guiResults += finalResults.shapePrecision + textLog.cellSeperatorType;
        guiResults += finalResults.orientationPrecision + textLog.cellSeperatorType;
        guiResults += finalResults.precision + textLog.cellSeperatorType;
        guiResults += finalResults.efficiency + textLog.cellSeperatorType;

        // SEND DATA TO THE GUI
        this.transform.parent.GetComponent<testDataGUI>().testData.Add(this.name + " Results: " + "\n" + guiResults + "\n" + "\n" + "\n");

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

    }

    //// FINAL RESULTS MATH ////
    public void FinalResults()
    {
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

        // efficiency math (reduce the effect of timeTaken's reduction of the efficiency rating for better numbers)
        // finalResults.efficiency = finalResults.precision - (finalResults.timeTaken * (finalResults.timePenalty * finalResults.totalDifficulty));
        // new efficiency based on time penalty relative to angle distance needed to travel [scale time penalty for eff tweaking]
        finalResults.efficiency = 100f -
                                (finalResults.timeTaken * (finalResults.timePenalty * finalResults.totalDifficulty));


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
