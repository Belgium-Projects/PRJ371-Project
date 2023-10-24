#include <IRremote.hpp>

//char mystr[10] = "0";
int speeed =0;
int distancecheck =0;
int distancecalc = 150;
int distance;
char status[10] ="0";
const int irReceiverPin = 8;
const byte sensorPin = 2;
unsigned long ultrasoundValue = 0;
unsigned long echo = 0;
unsigned long measureDistance();
//Adafruit_LiquidCrystal lcd_1(0);
char speed[10]="0";
int speedcalc =0;
double time = 5;
int time2 = 10;
IRrecv irrecv(irReceiverPin);
int light =0;
void setup() {
  pinMode(sensorPin, INPUT);
  //pinMode(sensorPin, INPUT);
  Serial.begin(115200); 
  pinMode(5,OUTPUT);
  pinMode(3,OUTPUT);
  pinMode(4,OUTPUT);
	//digitalWrite(sensorPin, HIGH);
   pinMode(7, OUTPUT);
  digitalWrite(7, LOW);
  digitalWrite(sensorPin, HIGH);
    irrecv.enableIRIn();
}

void loop() {
   Serial.readBytes(speed,4);
   Serial.println(speeed); 
  
  light =1;
  digitalWrite(5, LOW);
  digitalWrite(3, LOW);
  digitalWrite(4, HIGH);
  
  DelaySend();
	
  //time2 = 5;
  light =0;
  digitalWrite(5, HIGH);
  digitalWrite(3, LOW);
  digitalWrite(4, LOW);
  
  
   DelaySend();
   time2 = 15;
   light =2;
  digitalWrite(5, LOW);
  digitalWrite(3, HIGH);
  digitalWrite(4, LOW);

   DelaySend();
   //time2 = 5;
}
void DelaySend(){
	check();
	//1
	delay(500);
	 time -= 1;
     check(); 	 
	delay(500);
	 time -= 1;
	 check(); 
	 time2 = time2-1;
	 //2
	delay(500);
	 time -= 1;
	check(); 	 
	delay(500);
	 time -= 1;  
 	 check(); 
	 time2 = time2-1;
	 //3
	delay(500);
	 time = 5;  
	check(); 
	delay(500);
	 time -= 1;
	 check(); 
	 time2 = time2-1;
	 //4
	delay(500);
	 time -= 1;
	 check();   
	delay(500);
	 time -= 1;
	 check(); 
time2 = time2-1;
	//5	 
	delay(500);
	 time -= 1;
	 check();   
	delay(500);
	time = 5;  
	 time -= 1;
	 check(); 
time2 = time2-1;	 	 
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
void check(){
  distancecalc = distancecalc - (speeed*(time/10));
  
   if (irrecv.decode()) {
     auto value= irrecv.decodedIRData.decodedRawData;

    switch (value) {
      case 4261527296   :
      speedcalc = distancecalc/time2;
      
     	speeed = distancecalc/time2;
      if(speeed >63){
      speeed = distancecalc/(time2+15);
      }
        break;
      case 4211392256  : 
      // changeCharArray(speed, "200");
        break;
      case 4177968896  : 
       //changeCharArray(speed, "200"); 
        break;
      case 4127833856   : 
        //changeCharArray(speed, "200");
        break;
      case 4194680576   :
        //changeCharArray(speed, "0");
        break;
    }
    
    irrecv.resume(); 
  } 
  
  if(speeed != 0 || distancecheck ==1 ){
      speeed = distancecalc/time2 +1;
     if(speeed >63){
      speeed = 63;
      }
    if(distancecalc <63 && light ==0){
    speeed = 63;
    }
    
     
  }
  if(distancecalc <0){
  distancecalc = 150;
  }
  distance = measureDistance();
  if(distance <= 100){
    speeed = 0;
    distancecheck =1;
  }
  
   Serial.println(speeed); 
                    

}