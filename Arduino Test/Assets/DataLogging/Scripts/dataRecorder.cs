using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class dataRecorder : MonoBehaviour {

    public bool noOverwrite = true;
    public bool compareToDock = true;
    bool stopWriting = false; 
    public bool recordAngles = false;
    bool recodAnglesDone = false;
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
        public float[] correctAngles = new float[numberOfAngles];
    }
    public TextLog textLog = new TextLog();

    [System.Serializable]
    public class AngleSummary : System.Object
    {
        public float[] currAngles = new float[numberOfAngles];
        public float[] shapeRot = new float[3];
        public List<string> anglesOverTime;
    }
    public AngleSummary angleSummary = new AngleSummary();

    // Efficiency factors
    [System.Serializable]
    public class Efficiency : System.Object
    {
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
        public float precision;
        public float efficiency;
    }
    public FinalSummary finalResults = new FinalSummary();


    //+++ This will be the generic table method
    [System.Serializable]
    public class Table : System.Object
    {
        //+++ Generic Row Column stores for 1 table
        public int totalRows = 5;
        public int totalCols = 5;
        public List<List<string>> row = new List<List<string>>();
        public List<string> col = new List<string>();
    }
    //+++ Main Table
    public Table allRawData = new Table();
    //+++ Sub Tables
    public Table topHeader = new Table();
    public Table accuracyResults = new Table();
    public Table timeResults = new Table();
    public Table precisionResults = new Table();
    public Table efficiencyResults = new Table();
    public Table botFooter = new Table();

    // Start
    void Start() {
        // Add filename to path
        textLog.path += textLog.fileName + ".txt";

        // Clear File before using
        Clear();
        textLog.exportedText = "";

        //+++ Generic list creating code 
        //+++ Impliment with data write/read to consolidate code below in ExportData()
        //+++ Create variables that set the sizes of 5 and 5
        CompileAllDataTable(topHeader, accuracyResults, timeResults, precisionResults, efficiencyResults, botFooter);
        FormatAllDataTable();
        ExportAllData();

        // Read the file before starting any testing
        //Read();
        UpdateEditor();
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

            // Export Report
            ExportData();
            readyToExportData = false;

            // If noOverwrite is enabled you cannot write over the test data because of this bool that's tripped
            if (noOverwrite)
            {
                stopWriting = true;
            }
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
            if (i < numberOfAngles - angleSummary.shapeRot.Length)
            {
                angleSummary.currAngles[i] = ProtractorAngle(angleObjects[i].transform.localRotation.eulerAngles.z);
            }
            else
            {
                angleSummary.currAngles[i] = angleSummary.shapeRot[i - (angleSummary.currAngles.Length - angleSummary.shapeRot.Length)];
            }
            
        }
    }

    // Get the dock's current angles
    void GetDockAngles()
    {
        for (int i = 0; i < textLog.correctAngles.Length; i++)
        {
            // Make last three correct angles from dock orientation
            if (i == textLog.correctAngles.Length - 3)
            {
                textLog.correctAngles[i] = ProtractorAngle(dockShape.transform.localRotation.eulerAngles.x);
            }
            else if (i == textLog.correctAngles.Length - 2)
            {
                textLog.correctAngles[i] = ProtractorAngle(dockShape.transform.localRotation.eulerAngles.y);
            }
            else if (i == textLog.correctAngles.Length - 1)
            {
                textLog.correctAngles[i] = ProtractorAngle(dockShape.transform.localRotation.eulerAngles.z);
            }

            // Make first four correct angles from dockShapeAngles 
            else
            {
                textLog.correctAngles[i] = ProtractorAngle(dockAngleObjects[i].transform.localRotation.eulerAngles.z);
            }
        }
    }




    //// COMPARE ANGLES ////
    void CompareAngles()
    {
        for (int i = 0; i < efficiency.precision.Length; i++)
        {
            efficiency.precision[i] = ProtractorAngle(Mathf.Abs(textLog.correctAngles[i] - angleSummary.currAngles[i]));
        }
    }



    //// COMPILE CURRANGLES INTO TABLE ////
    // Take total angles recorded every second and compile into single line array of angles in time increments
    void AngleRecord()
    {
        string totalAnglesLine = "";

        for (int i = 0; i < angleSummary.currAngles.Length; i++)
        {
            totalAnglesLine += angleSummary.currAngles[i];

            // tab between each angle data to seperate into columns
            if (i < angleSummary.currAngles.Length - 1)
            {
                totalAnglesLine += textLog.cellSeperatorType;
            }
        }

        angleSummary.anglesOverTime.Add(totalAnglesLine);
    }




    //// WRITING DATA TO FILE ////
    // Create Single Mass String
    //+++ SPLIT THIS WHOLE THING INTO BASIC FLOATS AND STRINGS TO SEND TO THE SUB TABLES AND LATER COMPILE AS A EXPORT STRING
    void ExportData()
    {
        Clear();
        textLog.exportedText = "";

        /// FIRST ANGLES LINE
        textLog.exportedText += "Frames" + textLog.cellSeperatorType;
        for (int i = 0; i < angleSummary.currAngles.Length; i++)
        {
            if (i < angleSummary.currAngles.Length - angleSummary.shapeRot.Length)
            {
                textLog.exportedText += angleObjects[i].name;
            }
            else
            {
                if (i == angleSummary.currAngles.Length - 3)
                {
                    textLog.exportedText += "Docking x";
                }
                else if (i == angleSummary.currAngles.Length - 2)
                {
                    textLog.exportedText += "Docking y";
                }
                else if (i == angleSummary.currAngles.Length - 1)
                {
                    textLog.exportedText += "Docking z";
                }
            }


            // tab between each angle data to seperate into columns
            if (i < angleSummary.currAngles.Length - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }


        /// RECORDED ANGLE LINES
        for (int i = 0; i < angleSummary.anglesOverTime.Count; i++)
        {
            textLog.exportedText += "\n" + i + textLog.cellSeperatorType + angleSummary.anglesOverTime[i];
        }

        /// FIRST CORRECT ANGLE LINE
        textLog.exportedText += "\n";
        textLog.exportedText += "\n" + "Correct Angles" + textLog.cellSeperatorType;
        for (int i = 0; i < textLog.correctAngles.Length; i++)
        {

            if (i < angleObjects.Length)
            {
                textLog.exportedText += angleObjects[i].name;
            }
            else
            {
                if (i == textLog.correctAngles.Length - 3)
                {
                    textLog.exportedText += "Docking x";
                }
                else if (i == textLog.correctAngles.Length - 2)
                {
                    textLog.exportedText += "Docking y";
                }
                else if (i == textLog.correctAngles.Length - 1)
                {
                    textLog.exportedText += "Docking z";
                }
            }

            // tab between each angle data to seperate into columns
            if (i < textLog.correctAngles.Length - 1)
            {
                textLog.exportedText += textLog.cellSeperatorType;
            }
        }

        /// CORRECT ANGLE LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        for (int i = 0; i < textLog.correctAngles.Length; i++)
        {
            textLog.exportedText += textLog.correctAngles[i];

            // tab between each angle data to seperate into columns
            if (i < textLog.correctAngles.Length - 1)
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
        textLog.exportedText += "\n";
        textLog.exportedText += "\n" + "Final Results" + textLog.cellSeperatorType;
        textLog.exportedText += "Time Taken" + textLog.cellSeperatorType;
        textLog.exportedText += "Precision %" + textLog.cellSeperatorType;
        textLog.exportedText += "Efficiency %" + textLog.cellSeperatorType;

        /// SUMMARY LINES
        textLog.exportedText += "\n" + textLog.cellSeperatorType;
        textLog.exportedText += efficiency.completionTime + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.precision + textLog.cellSeperatorType;
        textLog.exportedText += finalResults.efficiency + textLog.cellSeperatorType;

        // Send the data
        Append(textLog.exportedText);

        // Update the debug and inspector
        Read();
        UpdateEditor();
    }

    ////+++ FINAL RESULTS MATH ////
    public void FinalResults()
    {
        // timetaken math
        finalResults.timeTaken = efficiency.secondsTaken;

        // precision math (out of 100) | the 180 is maximum error possible | 100f is to move up the decimals so add a % in the formatting
        // we want the amount of error angle absolutes on average thus we ABS() each precision value for this measurement before avg
        float[] angleErrors = efficiency.precision;
        for (int i = 0; i < angleErrors.Length; i++){ angleErrors[i] = Mathf.Abs(angleErrors[i]); }

        finalResults.precision = 100f - (((unweightedAverage(angleErrors))/180) * 100f);

        // efficiency math (reduce the effect of timeTaken's reduction of the efficiency rating for better numbers)
        finalResults.efficiency = finalResults.precision / (finalResults.timeTaken);
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
    public float unweightedAverage(params float[] numbers)
    {
        float total = 0;
        foreach (float n in numbers)
        {
            total += n;
        }

        return total / numbers.Length;
    }




    //// TABLE OPERATIONS ////
    //+++ CREATE MASS DATA TABLE
    public void CompileAllDataTable(params Table[] subTables)
    {
        /// data compiled into a generic table

        int finalRows = 0;
        int finalCols = 0;

        // find out how large the final table will be
        foreach (Table sub in subTables)
        {
            finalRows += sub.totalRows;
            finalCols += sub.totalCols;
        }

        // set the allRawData final table to have the appropriate size
        allRawData.totalRows = finalRows;
        allRawData.totalCols = finalCols;

        /// FILL TABLE ALL
        int currentRows = 0;
        int currentCols = 0;

        foreach (Table sub in subTables)
        {
            // rows
            for (int x = currentRows; x < allRawData.totalRows; x++)
            {
                // columns compile
                for (int y = currentCols; y < allRawData.totalCols; y++)
                {
                    if (allRawData.col.Count < allRawData.totalCols)
                    {
                        //allRawData.col.Add(sub.col[y]);
                        allRawData.col.Add("test");
                    }
                    else
                    {
                        //allRawData.col[y] = sub.col[y];
                        allRawData.col[y] = "test";
                    }
                }

                // rows compile
                if (allRawData.row.Count < allRawData.totalRows)
                {
                    allRawData.row.Add(allRawData.col);
                }
                else
                {
                    allRawData.row[x] = allRawData.col;
                }
            }

            // Running starting point for a table's appending
            currentRows += sub.totalRows;
            currentCols += sub.totalCols;

        }

    }

    //+++ MASS TEXT FORMATTING
    public void FormatAllDataTable()
    {
        /// data sent to the export string
        foreach (List<string> row in allRawData.row)
        {
            foreach (string col in row)
            {
                textLog.exportedText += col + textLog.cellSeperatorType;
            }

            textLog.exportedText += "\n";
        }
    }

    //+++ EXPORT
    public void ExportAllData()
    {
        Append(textLog.exportedText);
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
