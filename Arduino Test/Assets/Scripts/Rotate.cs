using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    // Use this for initialization
    float roll = 0;
    float rollPrev = 0;

    private float qW = 0;
    private float qY = 0;
    private float qX = 0;
    private float qZ = 0;

    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    public float bendValue = 0f;
    private JohnArduinoManager Arduino;

    public GameObject GameObject;

    void Awake ()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();
    }
	
	// Update is called once per frame
	void Update () {
        bendValue = Arduino.currentVals[2];
        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qX = Arduino.currentVals[5];
        qZ = Arduino.currentVals[6];

        roll = Mathf.Atan2(2 * qY * qW + 2 * qX * qZ, 1 - 2 * qY * qY - 2 * qZ * qZ);
        roll = roll * (180 / Mathf.PI);
        RotateObject(bendValue, roll);
    }



    void RotateObject(float Value, float roll)
    {
        zAngle = (Value - 511.5f) * 0.352f * 0.666f;
        float rollChange = roll - rollPrev;
        float zAngleChange = zAngle - zAnglePrev;

        transform.Rotate(0, 0, -zAngleChange, Space.Self);
        
        zAnglePrev = zAngle;
        rollPrev = roll;
    }



}
