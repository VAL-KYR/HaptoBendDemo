using UnityEngine;
using System.Collections;
using System.IO.Ports;
using UnityEngine.UI;
using System.Collections.Generic;
//using UnityEditor;
using System;
using System.Globalization;

public class JohnArduinoManager : MonoBehaviour
{
    /// Val | I've changed this value from COM7 to COM3 for testing on another computer
    public static string serialName = "COM3";
    public SerialPort sp = new SerialPort(serialName, 9600);
    public float[] currentVals = new float[10];

    string serialValue;
    string[] serialValues;
    string[] bendValues;

    // Use this for initialization
    void Start()
    {
        sp.Open();
    }

    // Update is called once per frame
    void Update()
    {
        if (sp.IsOpen)
        {
            try
            {
                serialValue = sp.ReadLine();
            }
            catch (System.Exception)
            {

            }

            serialValues = serialValue.Split('&');
        }
        
        if (serialValues.Length > 1)
        {
            bendValues = serialValues[1].Split(',');

            for (int j = 0; j < (bendValues.Length); j++)
            {
                currentVals[j] = float.Parse(bendValues[j]);
            }
        }
    }

}
