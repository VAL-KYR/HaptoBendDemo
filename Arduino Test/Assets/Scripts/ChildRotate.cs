using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildRotate : MonoBehaviour
{

    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    private float bendValue = 0f;
    private JohnArduinoManager Arduino;

    public GameObject GameObject;


    void Awake()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();
    }

    // Update is called once per frame
    void Update()
    {
        bendValue = Arduino.currentVals[1];
        RotateObject(bendValue);
    }


    void RotateObject(float Value)
    {
        zAngle = (Value - 511.5f) * 0.352f * 0.666f;
        float zAngleChange = zAngle - zAnglePrev;

        transform.Rotate(0, 0, -zAngleChange, Space.Self);

        zAnglePrev = zAngle;
    }
}