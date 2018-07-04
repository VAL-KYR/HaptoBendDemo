using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System;
using UnityEngine;

public class ModelMap : MonoBehaviour {
    private Quaternion imucap;
    private float zAngle = 0f;
    private float zAnglePrev = 0f;
    private float bendValue = 0f;

    private JohnArduinoManager Arduino;
    public GameObject GameObject;

    public GameObject[] Device = new GameObject[4];
    private int[] random = new int[6] { 0, 1, 2, 3, 4, 5};

    private float[] difference = new float[4];

    private int counter = 1;

    private float qW = 0;
    private float qY = 0;
    private float qX = 0;
    private float qZ = 0;

    bool Mapped;

    int mapToggle = 1;
    int posToggle = 1;
    //bool reRandomize = true;


    void Awake()
    {
        Arduino = GameObject.GetComponent<JohnArduinoManager>();

    }


    // Use this for initialization
    void Start () {
        Mapped = false;
    }
	
	// Update is called once per frame
	void Update () {
        bendValue = Arduino.currentVals[0];
        qW = Arduino.currentVals[3];
        qY = Arduino.currentVals[4];
        qX = Arduino.currentVals[5];
        qZ = Arduino.currentVals[6];


        if (Input.GetKeyDown("v"))
        {
            posToggle++;
            posToggle = posToggle % 2;

            if (posToggle == 0)
            {

                if(counter == 1)
                {
                    System.Random r = new System.Random();

                    random = random.OrderBy(x => r.Next()).ToArray();
                }

                counter++;
                counter = counter % 6;
                print("counter " + counter);
                print("Model Number " + random[counter]);

                for (int i = 0; i < 4; i++)
                {
                    Color color = Device[i].GetComponent<Renderer>().material.color;
                    color.a = 0.5f;

                    Device[i].GetComponent<Renderer>().material.SetColor("_Color", color);
                }
                //Color color2 = Models[random[counter]].GetComponent<Renderer>().material.color;
                //color2.a = 0.5f;

                //Models[random[counter]].GetComponent<Renderer>().material.SetColor("_Color", color2);
            }

            if(posToggle == 1)
            {

                for (int i = 0; i < 4; i++)
                {
                    Color color = Device[i].GetComponent<Renderer>().material.color;
                    color.a = 1f;

                    Device[i].GetComponent<Renderer>().material.SetColor("_Color", color);
                }
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
                    Device[i].SetActive(false);
                }
                imucap = new Quaternion(qX, qZ, qY, qW);

                //Color color2 = Models[random[counter]].GetComponent<Renderer>().material.color;
                //color2.a = 1f;

                //Models[random[counter]].GetComponent<Renderer>().material.SetColor("_Color", color2);
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


        if (Mapped == false)
        {
            qW = Arduino.currentVals[3];
            qY = Arduino.currentVals[4];
            qZ = Arduino.currentVals[5];
            qX = Arduino.currentVals[6];


            //qW = qW - difference[0];
            //qY = qY - difference[1];
            //qZ = qZ - difference[2];
            //qX = qX - difference[3];

            //Quaternion diff = new Quaternion(-difference[0], -difference[1], -difference[2], -difference[3]);

            Quaternion imu = new Quaternion(qW, qY, qX, qZ);
            Quaternion newimu = imu*Quaternion.Inverse(imucap);
            //RotateObject(bendValue / 2);


        }
    }

    void RotateObject(float Value)
    {

        zAngle = (Value - 511.5f) * 0.352f;

        float zAngleChange = zAngle - zAnglePrev;

        zAnglePrev = zAngle;
    }
}
