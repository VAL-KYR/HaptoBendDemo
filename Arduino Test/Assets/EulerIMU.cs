using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EulerIMU : MonoBehaviour {

    private JohnArduinoManager Arduino;

    public GameObject GameObject;

    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    private float bendValue = 0f;

    private float eY = 0;
    private float eX = 0;
    private float eZ = 0;
    //private int zeroToggle = 0;
    void Awake()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();

    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bendValue = Arduino.currentVals[0];



        eY = Arduino.currentVals[3];
        eZ = Arduino.currentVals[4];
        eX = Arduino.currentVals[5];



        //Quaternion imu = new Quaternion(qW, qZ, qY, qX);

        transform.eulerAngles = new Vector3(eX, eY, eZ);
        //transform.rotation = imu;
        //RotateObject(bendValue / 2);



    }

    void RotateObject(float Value)
    {

        zAngle = (Value - 511.5f) * 0.352f;

        float zAngleChange = zAngle - zAnglePrev;

        transform.Rotate(-zAngleChange, 0, 0, Space.Self);
        zAnglePrev = zAngle;
    }
}
