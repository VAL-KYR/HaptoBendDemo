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
    public GameObject[] dockAngleObjects;
    public static int numberOfAngles = 7;
    public GameObject shape;
    public GameObject dockShape;
    public Vector3 shapeOrientation;

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
        public float[] shapeRot = new float[3];
        public List<string> anglesOverTime;
    }
    public AngleSummary angleSummary = new AngleSummary();

    // This will be the generic table method
    [System.Serializable]
    public class Table : System.Object
    {
        // 
        public List<List<string>> cell = new List<List<string>>();
        public List<List<string>> row = new List<List<string>>();
        public List<string> col = new List<string>();
    }
    public Table allData = new Table();


    // Use this for initialization
    void Start() {
        // Add filename to path
        textLog.path += textLog.fileName + ".txt";

        // Clear File before using
        Clear();

        // Generic list creating code 
        // Impliment with data write/read to consolidate code below in ExportData()
        // Create variables that set the sizes of 5 and 5

        // CREATE DATA TABLE
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (allData.col.Count < 5)
                {
                    allData.col.Add("test");
                }
                else
                {
                    allData.col[y] = "test";
                }
            }


            if (allData.row.Count < 5)
            {
                allData.row.Add(allData.col);
            }
            else
            {
                allData.row[x] = allData.col;
            }
            
            //allData.row[x] = allData.col;
        }

        // COLLECT TEXT FORMATTING
        foreach (List<string> row in allData.row)
        {
            foreach (string col in row)
            {
                textLog.exportedText += col + "\t";
            }

            textLog.exportedText += "\n";
        }

        // EXPORT
        Append(textLog.exportedText);

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

    //// Generic Math Functions ///////////////////////////////////////////////////
    float ProtratorAngle(float angle)
    {
        angle = (angle > 180) ? angle - 360 : angle;
        return angle;
    }

    //// Read From Device /////////////////////////////////////////////////////////
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
