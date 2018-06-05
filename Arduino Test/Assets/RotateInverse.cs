using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInverse : MonoBehaviour {
    float roll = 0;
    float rollPrev = 0;

    private float qW = 0;
    private float qY = 0;
    private float qX = 0;
    private float qZ = 0;

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
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {

        bendValue = Arduino.currentVals[2];
        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qX = Arduino.currentVals[5];
        qZ = Arduino.currentVals[6];

        roll = Mathf.Atan2(2 * qY * qW + 2 * qX * qZ, 1 - 2 * qY * qY - 2 * qZ * qZ);
        roll = roll * (180 / Mathf.PI);
        RotateObject(bendValue / 2, roll);

    }



    void RotateObject(float Value, float roll)
    {
        //if(zeroed == true)
        //{
        //    Value = Value - sValue;
        //}

        //if (Input.GetKeyDown("z"))
        //{
        //    zero++;
        //    if (zero % 2 == 0)
        //    {
        //        sValue = Value;
        //        zeroed = true;
        //    }
        //}

        zAngle = (Value - 511.5f) * 0.352f * 0.666f;
        float rollChange = roll - rollPrev;
        float zAngleChange = zAngle - zAnglePrev - rollChange;

        //print("zAngle= " + zAngle);
        //print("zAngleChange= " + zAngleChange);

        //Vector3 Rotate = new Vector3(zAngleChange, 0, 0);

        transform.Rotate(0, 0, zAngleChange, Space.Self);
        if (Input.GetKeyDown("z"))
        {
            Quaternion zero = Quaternion.Euler(0, 180, 0);

            transform.rotation = zero;
        }
        //transform.rotation = Quaternion.Euler(55, 13, 0);


        //Vector3 point = new Vector3(0, 0.5f, 0.5f);
        //point = transform.TransformPoint(point);
        //Vector3 axis = new Vector3(1, 0, 0);
        //transform.RotateAround(point, axis, zAngleChange);
        zAnglePrev = zAngle;
        rollPrev = roll;
        print("roll: " + roll);
    }



}
