using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildRotateInverse : MonoBehaviour
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
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bendValue = Arduino.currentVals[0];
        RotateObject(bendValue);
    }


    void RotateObject(float Value)
    {
        zAngle = (Value - 511.5f) * 0.352f * 0.666f;
        float zAngleChange = zAngle - zAnglePrev;

        //print("zAngle= " + zAngle);
        //print("zAngleChange= " + zAngleChange);

        //Vector3 Rotate = new Vector3(zAngleChange, 0, 0);

        //transform.rotation = Quaternion.AngleAxis(-zAngle, Vector3.left);

        transform.Rotate(0, 0, zAngleChange, Space.Self);
        if (Input.GetKeyDown("z"))
        {
            Quaternion zero = Quaternion.Euler(0, 0, 0);

            transform.rotation = zero;
        }

        //Vector3 axis = transform.TransformDirection(Vector3.up);

        //Vector3 point = new Vector3(0, 0.5f, -0.5f);
        //point = transform.TransformPoint(point);
        //Vector3 axis = new Vector3(1, 0, 0);
        //transform.RotateAround(point, axis, -zAngleChange);
        zAnglePrev = zAngle;
    }
}