using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class dataRecorder : MonoBehaviour {

    public bool recordAngles = false;
    bool recodAnglesDone = false;
    public bool readyToExportData = false;

    // These must match in length
    public GameObject[] angleObjects;
    public static int numberOfAngles = 4;

    [System.Serializable]
    public class TextLog : System.Object
    {
        public string path = "Assets/Resources/";
        public string fileName = "Report_Hammer";
        public string exportedText;
        public string[] correctAngles = new string[numberOfAngles];
    }
    public TextLog textLog = new TextLog();

    [System.Serializable]
    public class AngleSummary : System.Object
    {
        public float[] currAngles = new float[numberOfAngles];
        public List<string> anglesOverTime;
    }
    public AngleSummary angleSummary = new AngleSummary();


    // Use this for initialization
    void Start() {
        // Add filename to path
        textLog.path += textLog.fileName + ".txt";

        Clear();
        Read();
        UpdateEditor();
    }

    // Update is called once per frame
    void Update() {

        // Always reading virtual object position every frame
        GetAngles();

        // you can call AngleRecord seperately (for testing the script only)
        if (recordAngles)
        {
            AngleRecord();
            readyToExportData = true;
        }

        // Once Angle Recording stops the report is generated
        if (readyToExportData && !recordAngles)
        {
            ExportData();
            readyToExportData = false;
        }
    }



    //// Test Scripts ////////////////////////////////////////////////////////////

    // Test creating a table (for testing the script only)
    void TestTable()
    {
        Append("\t" + "Column 1" + "\t" + "Column 2" +
                "\n" + "Row 1" + "\t" + "11" + "\t" + "12" +
                "\n" + "Row 2" + "\t" + "21" + "\t" + "22");
        Read();
        UpdateEditor();
    }


    //// Read From Device /////////////////////////////////////////////////////////
    void GetAngles()
    {
        // Fetch data for currAngles from the virtual objects by their z value in unity
        for (int i = 0; i < numberOfAngles - 1; i++)
        {
            angleSummary.currAngles[i] = angleObjects[i].transform.rotation.z;
        }
    }


    //// Recording Data ///////////////////////////////////////////////////////////

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

    // Create Single Mass String
    void ExportData()
    {
        Clear();
        textLog.exportedText = "";

        /// FIRST LINE
        textLog.exportedText += "Frames" + "\t";
        for (int i = 0; i < angleSummary.currAngles.Length; i++)
        {
            textLog.exportedText += angleObjects[i].name;

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
        // other metadata?
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


    //// Reading and Writing File /////////////////////////////////////////////////

    // Clear the file
    void Clear()
    {
        // Clear file
        File.WriteAllText(textLog.path, "");
    }

    // Write a single line
    void Append(string text)
    {
        //Write some text to the Report.txt file
        StreamWriter writer = new StreamWriter(textLog.path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    // Overwrite with a new whole body of text
    void Write(string text)
    {
        //Write some text to the Report.txt file
        StreamWriter writer = new StreamWriter(textLog.path, true);
        writer.Write(text);
        writer.Close();
    }

    // Read and update the inspector
    void UpdateEditor()
    {
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(textLog.path);
        TextAsset asset = Resources.Load(textLog.fileName) as TextAsset;
    }

    // Read and send data to console
    void Read()
    {
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(textLog.path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
