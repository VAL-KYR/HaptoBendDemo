#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>
#include <EEPROM.h>

/* Set the delay between fresh samples */
#define BNO055_SAMPLERATE_DELAY_MS (100)

Adafruit_BNO055 bno = Adafruit_BNO055(55);


const int bend1 = 0;
const int bend2 = 1;
const int bend3 = 2;


/**************************************************************************/
/*
    Displays some basic information on this sensor from the unified
    sensor API sensor_t type (see Adafruit_Sensor for more information)
*/
/**************************************************************************/
void displaySensorDetails(void)
{
  sensor_t sensor;
  bno.getSensor(&sensor);
  Serial.println("------------------------------------");
  Serial.print  ("Sensor:       "); Serial.println(sensor.name);
  Serial.print  ("Driver Ver:   "); Serial.println(sensor.version);
  Serial.print  ("Unique ID:    "); Serial.println(sensor.sensor_id);
  Serial.print  ("Max Value:    "); Serial.print(sensor.max_value); Serial.println(" xxx");
  Serial.print  ("Min Value:    "); Serial.print(sensor.min_value); Serial.println(" xxx");
  Serial.print  ("Resolution:   "); Serial.print(sensor.resolution); Serial.println(" xxx");
  Serial.println("------------------------------------");
  Serial.println("");
  delay(500);
}

/**************************************************************************/
/*
    Display some basic info about the sensor status
*/
/**************************************************************************/
void displaySensorStatus(void)
{
  /* Get the system status values (mostly for debugging purposes) */
  uint8_t system_status, self_test_results, system_error;
  system_status = self_test_results = system_error = 0;
  bno.getSystemStatus(&system_status, &self_test_results, &system_error);

  /* Display the results in the Serial Monitor */
  Serial.println("");
  Serial.print("System Status: 0x");
  Serial.println(system_status, HEX);
  Serial.print("Self Test:     0x");
  Serial.println(self_test_results, HEX);
  Serial.print("System Error:  0x");
  Serial.println(system_error, HEX);
  Serial.println("");
  delay(500);
}

/**************************************************************************/
/*
    Display sensor calibration status
*/
/**************************************************************************/
void displayCalStatus(void)
{
  /* Get the four calibration values (0..3) */
  /* Any sensor data reporting 0 should be ignored, */
  /* 3 means 'fully calibrated" */
  uint8_t system, gyro, accel, mag;
  system = gyro = accel = mag = 0;
  bno.getCalibration(&system, &gyro, &accel, &mag);

  /* The data should be ignored until the system calibration is > 0 */
  //Serial.print("\t");
//  if (!system)
//  {
//    Serial.print("! ");
//  }
  if(gyro<3 && accel<3 && mag<3)
  {
  /* Display the individual values */
  Serial.print("Sys:");
  Serial.print(system, DEC);
  Serial.print(" G:");
  Serial.print(gyro, DEC);
  Serial.print(" A:");
  Serial.print(accel, DEC);
  Serial.print(" M:");
  Serial.print(mag, DEC);
  }
}

//void getQuat(void){
//  
//}

/**************************************************************************/
/*
    Arduino setup function (automatically called at startup)
*/
/**************************************************************************/
void setup(void)
{
  //bno.begin(bno.OPERATION_MODE_IMUPLUS);
 
  Serial.begin(115200);
  //Serial.println("Orientation Sensor Test"); Serial.println("");

  /* Initialise the sensor */
  if(!bno.begin())
  {
    /* There was a problem detecting the BNO055 ... check your connections */
    Serial.print("Ooops, no BNO055 detected ... Check your wiring or I2C ADDR!");
    while(1);
  }

//  delay(1000);
//
//  /* Display some basic information on this sensor */
//  displaySensorDetails();
//
//  /* Optional: Display current status */
//  displaySensorStatus();
//
  bno.setExtCrystalUse(true);
}


void loop() {
   bno.setMode(bno.OPERATION_MODE_IMUPLUS);
  // put your main code here, to run repeatedly:
  String input = "";
  
  imu::Quaternion quat = bno.getQuat();
  
  uint8_t system, gyro, accel, mag;
  system = gyro = accel = mag = 0;
  bno.getCalibration(&system, &gyro, &accel, &mag);

//  /* Display the individual values */
//  Serial.print("Sys:");
//  Serial.print(system, DEC);
//  Serial.print(" G:");
//  Serial.print(gyro, DEC);
//  Serial.print(" A:");
//  Serial.print(accel, DEC);
//  Serial.print(" M:");
//  Serial.print(mag, DEC);


  input += "&";
  input += String(analogRead(bend1));
  input += ",";
  input += String(analogRead(bend2));
  input += ",";
  input += String(analogRead(bend3));
  input += ",";


  input += String(quat.w(), 4);
  input += ",";
  input += String(quat.y(), 4);
  input += ",";
  input += String(quat.x(), 4);
  input += ",";
  input += String(quat.z(), 4);
  input += ",";
  
  /* Display the individual values */

  input += String(gyro, DEC);
  input += ",";  
  input += String(accel, DEC);
  input += ",";  
  input += String(mag, DEC);

  //displayCalStatus();
  
  Serial.println(input);
  Serial.flush();
  delay(20);

   
  /* Display the quat data */
//  Serial.print("qW: ");
//  Serial.print(quat.w(), 4);
//  Serial.print(" qX: ");
//  Serial.print(quat.y(), 4);
//  Serial.print(" qY: ");
//  Serial.print(quat.x(), 4);
//  Serial.print(" qZ: ");
//  Serial.print(quat.z(), 4);
//  Serial.println("");  


   /* Optional: Display calibration status */
  

  //quatToMatrix(quat);
  }


void quatToMatrix(imu::Quaternion quat){
    double sqw = quat.w() * quat.w();
    double sqx = quat.x() * quat.x();
    double sqy = quat.y() * quat.y();
    double sqz = quat.z() * quat.z();

    // invs (inverse square length) is only required if quaternion is not already normalised
    double invs = 1 / (sqx + sqy + sqz + sqw);
    double m00 = ( sqx - sqy - sqz + sqw) * invs ; // since sqw + sqx + sqy + sqz =1/invs*invs
    double m11 = (-sqx + sqy - sqz + sqw) * invs ;
    double m22 = (-sqx - sqy + sqz + sqw) * invs ;
    
    double tmp1 = quat.x() * quat.y();
    double tmp2 = quat.z() * quat.w();
    double m10 = 2.0 * (tmp1 + tmp2) * invs ;
    double m01 = 2.0 * (tmp1 - tmp2) * invs ;
    
    tmp1 = quat.x() * quat.z();
    tmp2 = quat.y() * quat.w();
    double m20 = 2.0 * (tmp1 - tmp2) * invs ;
    double m02 = 2.0 * (tmp1 + tmp2) * invs ;
    tmp1 = quat.y() * quat.z();
    tmp2 = quat.x() * quat.w();
    double m21 = 2.0 * (tmp1 + tmp2) * invs ;
    double m12 = 2.0 * (tmp1 - tmp2) * invs ;

    Serial.print(" m00: ");
    Serial.print(m00, 4);
    Serial.print(" m01: ");
    Serial.print(m01, 4);
    Serial.print(" m02: ");
    Serial.print(m02, 4);
    Serial.print(" m10: ");
    Serial.print(m10, 4);
    Serial.print(" m11: ");
    Serial.print(m11, 4);
    Serial.print(" m12: ");
    Serial.print(m12, 4);
    Serial.print(" m20: ");
    Serial.print(m20, 4);
    Serial.print(" m21: ");
    Serial.print(m21, 4);
    Serial.print(" m22: ");
    Serial.print(m22, 4);

    Serial.println("");  
}


