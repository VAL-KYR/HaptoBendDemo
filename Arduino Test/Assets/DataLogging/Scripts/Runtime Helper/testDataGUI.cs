using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class testDataGUI : MonoBehaviour {

    public Font font;
    public int fontSize = 12;
    int totalLineCount = 0;
    public List<string> controlsList;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnGUI()
    {
        // Controls list
        GUI.skin.label.font = font;
        GUI.color = Color.black;
        GUI.skin.label.fontSize = fontSize;

        string allControls = "";
        foreach(string c in controlsList)
        {
            allControls += c + "\n";
        }

        GUI.Label(new Rect(0, 0, 
                            200 * fontSize / 1.7f, controlsList.Count * 100 * fontSize / 1.7f), 
                            allControls);

        // Status stuff
        GUI.skin.label.font = font;
        GUI.color = Color.green;
        GUI.skin.label.fontSize = fontSize;
        GUI.Label(new Rect(0, (controlsList.Count * fontSize/1.7f), 200 * fontSize / 1.7f, 
                            (controlsList.Count * 100) + 100), 
                            "What in the sweet heck?");
    }
}
