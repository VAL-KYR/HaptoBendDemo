using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModelMap2 : MonoBehaviour {
    private Quaternion imucap;
    private Quaternion diff;
    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    private float bendValue = 0f;

    private JohnArduinoManager Arduino;
    public GameObject GameObject;

    public GameObject[] Device = new GameObject[4];

    public GameObject[] Models = new GameObject[6];
    private int[] random = new int[6] { 0, 1, 2, 3, 4, 5 };

    private float[] difference = new float[4];

    private int counter = 1;

    private float qW = 0;
    private float qY = 0;
    private float qX = 0;
    private float qZ = 0;


    //private int onOff;

    bool Mapped;

    int mapToggle = 1;
    int posToggle = 1;

    void Awake()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();

    }


    // Use this for initialization
    void Start()
    {
        //for (int i = 6; i < 6; i++)
        //{
        //    //int number = Random.Range(0, 5);

        //    random[i] = i;
        //}

        System.Random r = new System.Random();

        random = random.OrderBy(x => r.Next()).ToArray();

        for (int i = 0; i < 6; i++)
        {
            Models[i].SetActive(false);


            //Models[i].GetComponent<Renderer>().material.color.a = 1;
        }
        Mapped = false;
    }

    // Update is called once per frame
    void Update()
    {
        bendValue = Arduino.currentVals[0];
        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qZ = Arduino.currentVals[5];
        qX = Arduino.currentVals[6];


        if (Input.GetKeyDown("v"))
        {
            posToggle++;
            posToggle = posToggle % 2;

            if (posToggle == 0)
            {

                counter++;
                counter = counter % 6;

                Models[random[counter]].SetActive(true);
                Models[random[counter]].transform.position = new Vector3(0, 2, 0);
            }
            if (posToggle == 1)
            {
                Models[random[counter]].SetActive(false);
            }
        }



        if (Input.GetKeyDown("c"))
        {
            mapToggle++;
            mapToggle = mapToggle % 2;

            if (mapToggle == 0)
            {

                for (int i = 0; i < 4; i++)
                {
                    //Device[i].SetActive(false);
                }
                imucap = new Quaternion(qW, qY, qX, qZ);
                difference[0] = qW;
                difference[1] = qY;
                difference[2] = qZ;
                difference[3] = qX;
                diff = new Quaternion(difference[0], difference[1], difference[3], difference[2]);
                Mapped = true;
            }
            if (mapToggle == 1)
            {
                //Models[random[counter]].transform.rotation = Quaternion.identity;
                for (int i = 0; i < 4; i++)
                {
                    Device[i].SetActive(true);
                }

                Mapped = false;
            }
        }

        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qZ = Arduino.currentVals[5];
        qX = Arduino.currentVals[6];

        Quaternion imu = new Quaternion(qW, qY, qX, qZ);

        RotateObject(bendValue / 2);

        Models[random[counter]].transform.rotation = Quaternion.Inverse(imu);

        if (Mapped == false)
        {
            Models[random[counter]].transform.rotation = Quaternion.identity;
        }

        //if (Mapped == true)
        //{
        //    qW = Arduino.currentVals[3];
        //    qY = Arduino.currentVals[4];
        //    qZ = Arduino.currentVals[5];
        //    qX = Arduino.currentVals[6];


        //    //qW = qW - difference[0];
        //    //qY = qY - difference[1];
        //    //qZ = qZ - difference[2];
        //    //qX = qX - difference[3];

        //    //Quaternion diff = new Quaternion(-difference[0], -difference[1], -difference[2], -difference[3]);

        //    Quaternion imu = new Quaternion(qW, qY, qX, qZ);
        //    Quaternion newimu = imu*Quaternion.Inverse(diff);
        //        //Quaternion.Inverse(imucap);
        //    RotateObject(bendValue / 2);


        //    Models[random[counter]].transform.rotation = newimu;
        //    //Models[random[counter]].transform.rotation = Quaternion.Inverse(newimu);
        //    //Models[random[counter]].transform.Rotate(90, 0, 0, Space.World);
        //    //Models[random[counter]].transform.rotation = Quaternion.Inverse(newimu);

        //}
    }

    void RotateObject(float Value)
    {

        zAngle = (Value - 511.5f) * 0.352f;

        float zAngleChange = zAngle - zAnglePrev;

        Models[random[counter]].transform.Rotate(zAngleChange, 0, 0, Space.Self);
        zAnglePrev = zAngle;
    }
}
