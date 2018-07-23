using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IMU : MonoBehaviour {
    private JohnArduinoManager Arduino;
    Quaternion imucap;
    public GameObject GameObject;

    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    public float bendValue = 0f;
    public GameObject Rotate;
    public GameObject InvRotate;
    public GameObject ChildRotate;
    public GameObject ChildInvRotate;
    public GameObject cylinder;

    private float qW = 0;
    private float qY = 0;
    private float qX = 0;
    private float qZ = 0;

    Quaternion imuinter;

    public bool calibrate = true;

    public float zCap = 0;
    float lowPassFactor = 0.2f; //Value should be between 0.01f and 0.99f. Smaller value is more damping.
    bool init = true;

    public void Awake()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();
    }
	
	// Update is called once per frame
	public void Update () 
    {
        bendValue = Arduino.currentVals[2];

        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qX = Arduino.currentVals[5];
        qZ = Arduino.currentVals[6];

        Quaternion imu = new Quaternion(qX, qZ, qY, qW);
        Quaternion centre = Quaternion.identity;


        
        if (Input.GetKeyDown("x"))
        {
            Calibrate();
        }

        if (Input.GetKeyDown("z"))
        {
            BendReset();
        }



        if (calibrate)
        {
            cylinder.SetActive(true);

            Rotate.transform.rotation = Quaternion.identity;
            InvRotate.transform.rotation = Quaternion.identity;
        }
        else
        {
            cylinder.SetActive(false);

            Quaternion newimu = imu * Quaternion.Inverse(imucap);

            transform.rotation = Quaternion.Inverse(lowPassFilterQuaternion(imuinter, newimu, lowPassFactor, init));
            
            init = false;

            zAngle = (bendValue - zCap) * 0.352f * 0.666f;

            float zAngleChange = zAngle - zAnglePrev;
            Quaternion offSet = Quaternion.Euler(0, 0, -zAngle/2);
            Quaternion finalimu = newimu* Quaternion.Inverse(offSet);

            zAnglePrev = zAngle;

        }
    }

    public void BendReset()
    {
        zCap = bendValue;

        ChildInvRotate.transform.rotation = Quaternion.Euler(0, 0, 0);
        ChildRotate.transform.rotation = Quaternion.Euler(0, 0, 0);
        Rotate.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void Calibrate()
    {
        calibrate = !calibrate;
        imucap = new Quaternion(qX, qZ, qY, qW);
    }

    Quaternion lowPassFilterQuaternion(Quaternion intermediateValue, Quaternion targetValue, float factor, bool init)
    {

        //intermediateValue needs to be initialized at the first usage.
        if (init)
        {
            intermediateValue = targetValue;
        }
        intermediateValue = Quaternion.Lerp(intermediateValue, targetValue, factor);

        return intermediateValue;
    }
}
