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


    //private Color activeColor = new Color(0f, 1f, 0f);
    //private Color inactiveColor = new Color(1f, 1f, 1f);

    //public Image[] imageObjects;
    //public Image[] bendObjects;
    //public Text[] bendTextObjects;
    //public Text gestureText;

   // public bool lerpMode = false;
    //private float[] previousBendValues = new float[2];
    public float[] currentVals = new float[10];





    //public float bendValue = 0f;

    //private float zAngle = 0f;
    //private float zAnglePrev = 0f;


    //private bool calibrateComplete = false;
    //private float bend1Av = 0f;
    //private float bend2Av = 0f;
    //private int currentCalibrationFrame = 0;
    //private const int calibrationFrames = 10;



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
        //print("serialValue " + serialValue);

        if (serialValues.Length > 1)
        {
            string[] bendValues = serialValues[1].Split(',');

           // for (int t = 0; t < 5; t++)
                //print("bendValues " + bendValues[t]);
            //float[] floatBendValues = new float[bendValues.Length];
            for (int j = 0; j < (bendValues.Length); j++)
            {
                currentVals[j] = float.Parse(bendValues[j]);
                //if (lerpMode)
                //{
                //    float currentVal = float.Parse(bendValues[j]);
                //    previousBendValues[j] = Mathf.Lerp(previousBendValues[j], currentVal, 0.5f);
                //}
                //else
                //{
                //    currentVals[j] = float.Parse(bendValues[j]);
                //    //previousBendValues[j] = float.Parse(bendValues[j]);
                //}
                //print("bendValue " + j); 
                //print(bendValues[j]);
                //print("CurrentValue" + j);
                //print(currentVals[j]);


            }
        }
        //print("Gyro: " + currentVals[7]);
        //print("Accel: " + currentVals[8]);
        //print("Magnom: " + currentVals[9]);
    }

    //void RotateObject(float Value)
    //{
    //    zAngle = (Value - 511.5f) * 0.352f;
    //    float zAngleChange = zAngle - zAnglePrev;

    //    print("zAngle= " + zAngle);
    //    print("zAngleChange= " + zAngleChange);

    //    Vector3 Rotate = new Vector3(zAngleChange, 0, 0);

    //    transform.Rotate(Rotate, Space.Self);

    //    zAnglePrev = zAngle;
    //}
}
