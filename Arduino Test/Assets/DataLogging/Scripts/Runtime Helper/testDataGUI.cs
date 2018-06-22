using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testDataGUI : MonoBehaviour {

    public Font font;
    public int fontSize = 12;
    Vector2 totalLineSpace = new Vector2(0, 0);

    public Vector2 scrollPosition;

    public List<string> controlsList;
    public List<string> actionList = new List<string>(6);
    public List<string> testData;
    

    // Use this for initialization
    void Start () {
        AddElement(actionList, "Loaded");
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void OnGUI()
    {
        // Controls list
        string allControls = "";
        foreach (string c in controlsList)
        {
            allControls += c + "\n";
        }

        // Graphical Settings for Controls List Box
        GUI.color = Color.black;
        GUI.skin.textArea.font = font;
        GUI.skin.textArea.alignment = TextAnchor.UpperLeft;
        GUI.skin.textArea.fontSize = (int)(fontSize * 0.75f);
        GUI.TextArea(new Rect(0, 
                            0, 
                            20 * fontSize / 1.7f, 
                            controlsList.Count * fontSize / 2.3f), 
                            allControls);

        totalLineSpace.y += controlsList.Count * fontSize / 2.3f;

        // FetchAction();
        // Called from dataRecordingController every time a button is pressed

        // Action List
        string allActions = "";
        foreach (string a in actionList)
        {
            allActions += a + "\n";
        }

        // Graphical Settings for Action List Box
        GUI.color = Color.green;
        GUI.skin.textArea.font = font;
        GUI.skin.textArea.alignment = TextAnchor.UpperLeft;
        GUI.skin.textArea.fontSize = fontSize;
        GUI.TextArea(new Rect(0, 
                            (totalLineSpace.y), 
                            15 * fontSize / 1.7f,
                            actionList.Count * fontSize / 1.7f), 
                            allActions);

        totalLineSpace.y += (actionList.Count * fontSize / 1.7f) + 10f;

        // Test List
        string allTests = "";
        foreach (string t in testData)
        {
            allTests += t + "\n";
        }
 
        // Graphical Settings for Test List Box
        GUI.color = Color.red;
        GUI.skin.label.font = font;
        GUI.skin.label.fontSize = (int)(fontSize * 0.7f);
        Vector2 scrollSize = new Vector2(800, 300);


        GUILayout.BeginHorizontal();
        GUILayout.Space(Screen.width - scrollSize.x);

        GUILayout.BeginVertical();
        GUILayout.Space(0);

        GUILayout.Label("Test Data: ");

        GUI.color = Color.black;
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, 
                                                    GUILayout.Width(scrollSize.x), 
                                                    GUILayout.Height(scrollSize.y));

        GUILayout.Label(allTests);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        totalLineSpace.y = 0;
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
