// C++ code
//

#include <IRremote.hpp>


//char mystr[100] = "0"; //String data
char speedval[100] = "0";
//[[int seconds = 0;
//int analogSPeed = 0;
int val = 1023;

void moveBackward();
void moveForward();
void turnRight();
void stopMoving();
unsigned long measureDistance();
const byte sensorPin = 2;
int distance;
unsigned long ultrasoundValue = 0;
unsigned long echo = 0;
int speed = 255;
int motorspeed = 0;
const int irReceiverPin = 8;
int temp = 1;
IRrecv irrecv(irReceiverPin);
//Adafruit_LiquidCrystal lcd_1(0);


void setup()
{
 
 Serial.begin(115220); 
  pinMode(sensorPin, INPUT);
  pinMode(13, OUTPUT);
  pinMode(11, OUTPUT);
  pinMode(9, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(3, OUTPUT);
  digitalWrite(13, HIGH);
  pinMode(7, OUTPUT);
  digitalWrite(7, LOW);
  digitalWrite(sensorPin, HIGH);
  
    irrecv.enableIRIn();
 
 

}


void loop()
{
    //distance = measureDistance();
  //Send data
   Serial.readBytes(speedval,4);
  //Serial.println(speed); 
  //changeCharArray(mystr, speed);
  
  if(atoi(speedval)>0){
    moveForward();
  }
    //Serial.write(jeff);
 //Serial.write("10",3);
  //delay(10);
  
     //stopMoving();

  //stopMoving();

   //val= digitalRead(9);
  //if(val == 1){
  //Serial.write(mystr , 4);
  //}
  
  //if(distance <= 100){
 //   stopMoving();
  //}
 // else{
   
  if (irrecv.decode()) {
    
     auto value= irrecv.decodedIRData.decodedRawData;
     //Serial.println(value);

    switch (value) {
      case 4261527296   :
     
        moveForward();
        //Serial.println(mystr);
        break;
      case 4211392256  : 
      
        turnLeft();
        break;
      case 4177968896  : 
      
        turnRight();
        break;
      case 4127833856   : 
        moveBackward();
        break;
      case 4194680576   :
        stopMoving();
        break;
     
    }
    irrecv.resume(); 
  } 
 // }
    
}



void moveForward()
{
  //Serial.write("1",3);
  //digitalWrite(11, HIGH);
  speed = map(atoi(speedval),0,63,0,255);
  
  digitalWrite(11, LOW);
  analogWrite(9, speed);
  //digitalWrite(5, HIGH);
  analogWrite(5, speed);
  digitalWrite(3, LOW);
 //hangeCharArray(mystr, jeff);
 	//temp = atoi(speed);
  delay(600);

}

void moveBackward()
{
  digitalWrite(11, HIGH);
  //digitalWrite(9, HIGH);
  analogWrite(9, LOW);
  digitalWrite(5, LOW);
  //digitalWrite(3, HIGH);
  analogWrite(3, speed);
  // delay(duration);
  //changeCharArray(mystr, "-20");
  //Serial.println(mystr);
}

void turnRight()
{
  //Serial.write("1",3);
  //digitalWrite(11, HIGH);
  //speed = map(atoi(speedval),0,63,0,255);
  
  analogWrite(5, speed);
  digitalWrite(3, LOW);
  //digitalWrite(5, HIGH);
  //digitalWrite(3, LOW);
  //analogWrite(8, speed);
   delay(100);
  digitalWrite(11, 0);
  digitalWrite(9, LOW);
  digitalWrite(5, LOW);
  digitalWrite(3, LOW);
  //digitalWrite(5, HIGH);
  //digitalWrite(5, HIGH);;
  //digitalWrite(3, LOW);
 //hangeCharArray(mystr, jeff);
 	//temp = atoi(speed);
  delay(200);
  
  
  //digitalWrite(3, HIGH);
   //analogWrite(5, speed);
  // delay(200);
 // digitalWrite(3, LOW);
}

void turnLeft()
{
  digitalWrite(11, LOW);
  analogWrite(9, 255);
  //digitalWrite(5, HIGH);
  //digitalWrite(3, LOW);
  //analogWrite(8, speed);
   delay(100);
  digitalWrite(11, 0);
  digitalWrite(9, LOW);
  digitalWrite(5, LOW);
  digitalWrite(3, LOW);
  //analogWrite(8, 0);
}

void stopMoving()
{
  digitalWrite(11, 0);
  digitalWrite(9, LOW);
  digitalWrite(5, LOW);
  digitalWrite(3, LOW);
  // delay(duration);
   //changeCharArray(mystr, "0");
 
 
  delay(100);
}

unsigned long measureDistance()
{
  digitalWrite(7, HIGH);
  delayMicroseconds(10);
  digitalWrite(7, LOW);
  pinMode(sensorPin, OUTPUT);
  digitalWrite(sensorPin, LOW);
  delayMicroseconds(2);
  digitalWrite(sensorPin, HIGH);
  delayMicroseconds(5);
  digitalWrite(sensorPin, LOW);
  pinMode(sensorPin, INPUT);
  digitalWrite(sensorPin, HIGH);
  echo = pulseIn(sensorPin, HIGH, 38000);
  ultrasoundValue = (echo / 58);
  return ultrasoundValue;
}
