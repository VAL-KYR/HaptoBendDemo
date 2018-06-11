using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class dataRecorder : MonoBehaviour {

    public bool noOverwrite = true;
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
        public string exportedText;
        public string[] correctAngles = new string[numberOfAngles];
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

    // Efficiency summary TIME && PRECISION
    [System.Serializable]
    public class Efficiency : System.Object
    {
        public float secondsTaken;
        public string completionTime;
        public List<float> rawCompletionTime;
        public float precision;
    }
    public Efficiency efficiency = new Efficiency();
    

    //+++ This will be the generic table method
    [System.Serializable]
    public class Table : System.Object
    {
        //+++ Generic Row Column stores for 1 table
        public int totalRows = 10;
        public int totalCols = 10;
        public List<List<string>> row = new List<List<string>>();
        public List<string> col = new List<string>();
    }
    //+++ Main Table
    public Table allRawData = new Table();
    //+++ Sub Tables
    public Table topHeader = new Table();
    public Table accuracyResults = new Table();
    public Table timeResults = new Table();
    public Table efficiencyResults = new Table();
    public Table botFooter = new Table();

    // Start
    void Start() {
        // Add filename to path
        textLog.path += textLog.fileName + ".txt";

        // Clear File before using
        Clear();

        //+++ Generic list creating code 
        //+++ Impliment with data write/read to consolidate code below in ExportData()
        //+++ Create variables that set the sizes of 5 and 5
        CompileAllDataTable();
        FormatAllDataTable();
        ExportAllData();

        // Read the file before starting any testing
        Read();
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
        angleSummary.shapeRot[0] = ProtratorAngle(shape.transform.rotation.eulerAngles.x);
        angleSummary.shapeRot[1] = ProtratorAngle(shape.transform.rotation.eulerAngles.y);
        angleSummary.shapeRot[2] = ProtratorAngle(shape.transform.rotation.eulerAngles.z);

        // Fetch data for currAngles from the virtual objects by their z value in unity
        for (int i = 0; i < numberOfAngles; i++)
        {
            if (i < numberOfAngles - angleSummary.shapeRot.Length)
            {
                angleSummary.currAngles[i] = ProtratorAngle(angleObjects[i].transform.rotation.eulerAngles.z);
            }
            else
            {
                angleSummary.currAngles[i] = angleSummary.shapeRot[i - (angleSummary.currAngles.Length - angleSummary.shapeRot.Length)];
            }
            
        }
    }




    //// COMPILE ANGLES INTO TABLE ////
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
                totalAnglesLine += "\t";
            }
        }

        angleSummary.anglesOverTime.Add(totalAnglesLine);
    }

    //// WRITING DATA TO FILE ////
    // Create Single Mass String
    void ExportData()
    {
        Clear();
        textLog.exportedText = "";

        /// FIRST LINE
        textLog.exportedText += "Frames" + "\t";
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
                textLog.exportedText += "\t";
            }
        }
        

        /// DATA LINES
        for (int i = 0; i < angleSummary.anglesOverTime.Count; i++)
        {
            textLog.exportedText += "\n" + i + "\t" + angleSummary.anglesOverTime[i];
        }


        /// FINAL LINES
        textLog.exportedText += "\n" + "Correct Angles" + "\t";
        for (int i = 0; i < textLog.correctAngles.Length; i++)
        {
            textLog.exportedText += textLog.correctAngles[i];

            // tab between each angle data to seperate into columns
            if (i < angleSummary.currAngles.Length - 1)
            {
                textLog.exportedText += "\t";
            }
        }

        // Send the data
        Append(textLog.exportedText);

        // Update the debug and inspector
        Read();
        UpdateEditor();
    }




    /// GENERIC FUNCTIONS /// 

    //// MATH OPERATIONS ////
    // Give nice protractor angles (180 relevant)
    float ProtratorAngle(float angle)
    {
        angle = (angle > 180) ? angle - 360 : angle;
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




    //// TABLE OPERATIONS ////
    //+++ CREATE MASS DATA TABLE
    public void CompileAllDataTable(List<Table> subTables)
    {
        /// data compiled into a generic table
        /// 

        for (int x = 0; x < allRawData.totalRows; x++)
        {
            for (int y = 0; y < allRawData.totalCols; y++)
            {
                if (allRawData.col.Count < allRawData.totalCols)
                {
                    allRawData.col.Add("test");
                }
                else
                {
                    allRawData.col[y] = "test";
                }
            }


            if (allRawData.row.Count < allRawData.totalRows)
            {
                allRawData.row.Add(allRawData.col);
            }
            else
            {
                allRawData.row[x] = allRawData.col;
            }

            //allRawData.row[x] = allRawData.col;
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
                textLog.exportedText += col + "\t";
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
