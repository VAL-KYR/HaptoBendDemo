using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class ArduinoFlexSensor : MonoBehaviour {
    public float speed;

    private float amountToMove;

    SerialPort sp = new SerialPort("COM4", 9600);

    // Use this for initialization
    void Start()
    {
        sp.Open();
        sp.ReadTimeout = 1;
    }

    // Update is called once per frame
    void Update()
    {

        amountToMove = speed * Time.deltaTime;

        if (sp.IsOpen)
        {
            try
            {
                MoveObject(sp.ReadByte());
                print(sp.ReadByte());
            }
            catch (System.Exception)
            {

            }
        }
    }

    void MoveObject(int Direction)
    {
        if (Direction > 700)
        {
            transform.Translate(Vector3.left * amountToMove, Space.World);
        }
       else if(Direction < 15)
        {
            transform.Translate(Vector3.right * amountToMove, Space.World);
        }
        else
        {
            return;
        }
    }
}
