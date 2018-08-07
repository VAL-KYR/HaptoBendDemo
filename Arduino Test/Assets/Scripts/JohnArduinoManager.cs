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
    public SerialPort mySPort = new SerialPort(serialName, 9600);
    public float[] currentVals = new float[10];

    // Use this for initialization
    void Start()
    {
        mySPort.Open();
    }

    // Update is called once per frame
    void Update()
    {
        string serialValue = mySPort.ReadLine();
        string[] serialValues = serialValue.Split('&');

        if (serialValues.Length > 1)
        {
            string[] bendValues = serialValues[1].Split(',');

            for (int j = 0; j < (bendValues.Length); j++)
            {
                currentVals[j] = float.Parse(bendValues[j]);
            }
        }
    }

}
