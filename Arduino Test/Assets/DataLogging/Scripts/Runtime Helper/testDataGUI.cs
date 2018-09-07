using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class testDataGUI : MonoBehaviour {

    public Font font;
    public int fontSize = 12;
    public string name = "";

    [System.Serializable]
    public class Debugger : System.Object
    {
        public bool allTrials = false;
        public bool trialsSum = false;
        public string cmd = "";
    }
    public Debugger debug = new Debugger();


    public int VisLtdRandom = 0;
    public int InvisLtdRandom = 0;
    public int VisPresets = 0;
    public int InvisPresets = 0;
    public float dockFoldSpeed = 10.0f;

    public Vector2 scrollPosition;
    public Vector2 scrollPositionL;

    public List<string> controlsList;
    public List<string> actionList = new List<string>(6);
    public List<string> testData;


    GUIStyle guiStyleC = new GUIStyle();
    GUIStyle guiStyleA = new GUIStyle();
    GUIStyle guiStyleL = new GUIStyle();
    GUIStyle guiStyleI = new GUIStyle();
    GUIStyle guiStyleCmd = new GUIStyle();

    // Use this for initialization
    void Start () {
        AddElement(actionList, "Loaded");

        // Graphical Settings for Controls List Box
        guiStyleC.font = font;
        guiStyleC.alignment = TextAnchor.UpperLeft;

        // Graphical Settings for Action List Box
        guiStyleA.font = font;
        guiStyleA.alignment = TextAnchor.UpperLeft;

        // Graphical Settings for Logger List Box
        guiStyleL.font = font;
        guiStyleL.alignment = TextAnchor.UpperLeft;

        // Graphical Settings for Command Prompt Input
        guiStyleI.font = font;
        guiStyleI.alignment = TextAnchor.UpperLeft;
    }

    public void OnGUI()
    {
        UpdateTextScaling();

        GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            
                string allControls = "";
                foreach (string c in controlsList)
                {
                    allControls += c + "\n";
                }

                GUILayout.BeginVertical();

                    GUI.color = Color.blue;
                    GUI.skin.textArea.font = font;
                    GUI.skin.textArea.fontSize = Screen.width / 60;

                    // Controls List Box
                    GUILayout.TextArea(allControls);

                    // Action List
                    string allActions = "";
                    foreach (string a in actionList)
                    {
                        allActions += a + "\n";
                    }

                    GUI.color = Color.black;

                    // Action List Box
                    GUILayout.TextArea(allActions);

                GUILayout.EndVertical();
                
            GUILayout.EndVertical();


            GUILayout.Space(Screen.width * 0.25f);


            GUILayout.BeginVertical();

                GUILayout.BeginVertical();

                    GUILayout.Space(0);

                    GUILayout.Label("Test Data: ", guiStyleC);

                    // Test List
                    string allTests = "";
                    foreach (string t in testData)
                    {
                        allTests += t + "\n";
                    }

                    //scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                        //GUILayout.Label(allTests, guiStyleC);

                    //GUILayout.EndScrollView();

                    // Show active loggers
                    string loggerText = "";
                    foreach (GameObject logger in GetComponent<dataMaster>().dataLoggers)
                    {
                        loggerText += logger.name + "\n";

                        foreach (GameObject trial in logger.GetComponent<dataRecordingController>().tests)
                        {
                            loggerText += "\t" + trial.name + "\n";
                        }
                    }

                    scrollPositionL = GUILayout.BeginScrollView(scrollPositionL);

                        GUILayout.Label(loggerText, guiStyleC);

                    GUILayout.EndScrollView();

                    // Test Category Panel
                    GUILayout.BeginHorizontal();

                        GUILayout.BeginVertical();
                            
                            GUI.skin.toggle.font = font;
                            GUI.skin.label.font = font;
                            GUI.skin.textField.font = font;
                            GUI.color = Color.black;
                            GUI.skin.toggle.fontSize = Screen.width / 60;
                            GUI.skin.label.fontSize = Screen.width / 60;
                            GUI.skin.textField.fontSize = Screen.width / 60;
                            GUI.skin.toggle.alignment = TextAnchor.UpperLeft;
                            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

                            GUILayout.BeginVertical();

                                try
                                {
                                    GUILayout.BeginHorizontal();
                                        GUILayout.Label("# of VisLtdRandom: ");
                                        VisLtdRandom = int.Parse(GUILayout.TextField(VisLtdRandom.ToString(), GUILayout.Width(Screen.width * 0.1f)));
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                        GUILayout.Label("# of InvisLtdRandom: ");
                                        InvisLtdRandom = int.Parse(GUILayout.TextField(InvisLtdRandom.ToString(), GUILayout.Width(Screen.width * 0.1f)));
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                        GUILayout.Label("# of VisPresets: ");
                                        VisPresets = int.Parse(GUILayout.TextField(VisPresets.ToString(), GUILayout.Width(Screen.width * 0.1f)));
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                        GUILayout.Label("# of InvisPresets: ");
                                        InvisPresets = int.Parse(GUILayout.TextField(InvisPresets.ToString(), GUILayout.Width(Screen.width * 0.1f)));
                                    GUILayout.EndHorizontal();
                                    GUILayout.BeginHorizontal();
                                        GUILayout.Label("Dock Fold Speed: ");
                                        dockFoldSpeed = float.Parse(GUILayout.TextField(dockFoldSpeed.ToString(), GUILayout.Width(Screen.width * 0.1f)));
                                    GUILayout.EndHorizontal();
                                }
                                catch
                                {
                                }

                            GUILayout.EndVertical(); 


                            debug.allTrials = GUILayout.Toggle(debug.allTrials, " Monitor AllTrials", 
                                                                                GUILayout.Width(Screen.width * 0.2f),
                                                                                GUILayout.Height(Screen.height * 0.03f));
                            debug.trialsSum = GUILayout.Toggle(debug.trialsSum, " Monitor TrialCategorySum",
                                                                                GUILayout.Width(Screen.width * 0.2f),
                                                                                GUILayout.Height(Screen.height * 0.03f));    

                            GUI.SetNextControlName("console");

                            debug.cmd = GUILayout.TextField(debug.cmd, GUILayout.Width(Screen.width * 0.2f), 
                                                                        GUILayout.Height(Screen.height * 0.03f));
                            GUILayout.Label("[Hit Enter to Execute Commands]", guiStyleI, GUILayout.Width(200f));

                            if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && 
                                GUI.GetNameOfFocusedControl() == "console")
                            {
                                ExecuteCmd(debug.cmd);
                            }
                            
                        GUILayout.EndVertical();
                    
                    GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUILayout.Space(Screen.width * 0.15f);

            GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    public void UpdateTextScaling()
    {
        guiStyleC.fontSize = Screen.width / 60;
        guiStyleA.fontSize = Screen.width / 60;
        guiStyleL.fontSize = Screen.width / 60;
        guiStyleI.fontSize = Screen.width / 60;
    }

    public void ExecuteCmd(string command)
    {
        string[] commandBreakdown = command.Split('.');

        if (!commandBreakdown.Contains(""))
        {   
            if (commandBreakdown.Contains("add"))
            {
                // add.logger.[loggername]
                if (commandBreakdown.Contains("logger"))
                {
                    name = commandBreakdown[commandBreakdown.Length - 1];
                    GetComponent<dataMaster>().NewDataLogger(commandBreakdown[commandBreakdown.Length - 1]);
                }
            }
            else if (commandBreakdown.Contains("clear")) 
            {
                // clear.folder.[foldername]
                if (commandBreakdown.Contains("folder"))
                {
                    fileEditor.ClearDir("Assets/DataLogging/Reports/" + commandBreakdown[commandBreakdown.Length - 1]);
                }
                // clear.all
                else if (commandBreakdown.Contains("all"))
                {
                    fileEditor.ClearDir("Assets/DataLogging/Reports/");
                }
            }
            else if (commandBreakdown.Contains("delete")) 
            {
                // delete.logger.[loggername]
                if (commandBreakdown.Contains("folder"))
                {
                    fileEditor.Delete("Assets/DataLogging/Reports/" + commandBreakdown[commandBreakdown.Length - 1]);
                }
                // delete.logger.[loggername].[trialname]
                else if (commandBreakdown.Contains("logger"))
                {
                    if (commandBreakdown.Length > 3)
                    {
                        GetComponent<dataMaster>().DeleteTrial(commandBreakdown[commandBreakdown.Length - 2], commandBreakdown[commandBreakdown.Length - 1]);
                    }
                }
            }
        }
       
    }

    public void FetchAction(string action)
    {
        AddElement(actionList, action);
    }

    public void AddElement(List<string> list, string element)
    {
        // Always skip first element (0)
        // check to bottom for empty space
        int emptySpace = 0;
        bool roomForMore = false;


        /// find last empty space 
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i] == "")
            {
                // exit because we found an empty space
                emptySpace = i;
                roomForMore = true;
                break;
            }
        }

        /// Add Element (top down nearest null)
        if (roomForMore)
        {
            list[emptySpace] = element;
            roomForMore = false;
        }
        else
        {
            /// delete top element (1)
            list[1] = "";

            /// move all elements up by one
            for (int i = 1; i < list.Count - 1; i++)
            {
                list[i] = list[i + 1];
            }

            /// Add Element to last element (5)
            list[list.Count - 1] = element;
        }



    }
}