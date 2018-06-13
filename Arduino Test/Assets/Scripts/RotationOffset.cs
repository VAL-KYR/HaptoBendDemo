using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationOffset : MonoBehaviour {
    private JohnArduinoManager Arduino;
    Quaternion imucap;
    public GameObject GameObject;
    Quaternion offSet;
    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    private float bendValue = 0f;
    //public GameObject Rotate;
    //public GameObject InvRotate;
 

    private float qW = 0;
    private float qY = 0;
    private float qX = 0;
    private float qZ = 0;

    private int callibrate = 1;

    float zCap = 0;

    private bool align = false;
    //private int zeroToggle = 0;
    void Awake()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();

    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        bendValue = Arduino.currentVals[2];

        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qX = Arduino.currentVals[5];
        qZ = Arduino.currentVals[6];

        //qW = Arduino.currentVals[3];
        //qY = Arduino.currentVals[4];
        //qX = Arduino.currentVals[5];
        //qZ = Arduino.currentVals[6];

        // Quaternion imu = new Quaternion(qW, qY, qZ, qX);
        Quaternion imu = new Quaternion(qX, qZ, qY, qW);

        if (Input.GetKeyDown("x"))
        {
            callibrate++;
            callibrate = callibrate % 2;
            //imucap = new Quaternion(qW, qY, qZ, qX); ;
            imucap = new Quaternion(qX, qZ, qY, qW);

        }



        if (callibrate == 0)
        {
            align = true;
           
        }

        if (callibrate == 1)
        {
            align = false;
            
        }


        //Quaternion imu = new Quaternion(qX, qY, qZ, qW);
        Quaternion centre = Quaternion.identity;

        if (Input.GetKeyDown("z"))
        {
            zCap = bendValue;

            //Quaternion zero = Quaternion.Euler(0, 180, 0);

            //transform.rotation = zero;
        }



        //if (align == false)
        //{
        //    Rotate.transform.rotation = Quaternion.identity;
        //    InvRotate.transform.rotation = Quaternion.identity;
        //}


        if (align == true)
        {
            Quaternion newimu = imu * Quaternion.Inverse(imucap);

            //RotateObject(bendValue);
            //transform.Rotate(0, 180, 0, Space.World);

            zAngle = (bendValue - zCap) * 0.352f * 0.666f;

            float zAngleChange = zAngle - zAnglePrev;
            transform.Rotate(0, 0, -zAngleChange/2, Space.Self);
            //offSet = Quaternion.Euler(0, 0, -zAngle / 2);
            //transform.rotation = Quaternion.Inverse(offSet);
            Quaternion finalimu = newimu * offSet;
            transform.rotation = Quaternion.Inverse(offSet);
            //transform.rotation = Quaternion.Inverse(newimu);
            zAnglePrev = zAngle;

        }
    }
}
