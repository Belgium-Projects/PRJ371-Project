#include <Adafruit_LiquidCrystal.h>

char mystr[100];
int distance = 150;
Adafruit_LiquidCrystal lcd_1(0);

void setup() {
  Serial.begin(115230); 
  lcd_1.begin(16, 2);
  lcd_1.print("Getting ready");
  delay(1000);
  lcd_1.clear();
  lcd_1.print("Distance in M");
  lcd_1.setCursor(0, 1);
}

void loop() {
  lcd_1.setCursor(0, 1);
  lcd_1.print(distance);
  lcd_1.print("m");

  Serial.readBytes(mystr, 3);
  int distancetra = atoi(mystr) * 0.5;
  distance -= distancetra;

  if (distance < 0) {
    lcd_1.clear();
    lcd_1.print("Crossed intersection");
    lcd_1.setCursor(0, 1);
    lcd_1.print("Resetting");
    distance = 150;
    delay(500);
    lcd_1.clear();
    lcd_1.setCursor(0, 0);
    lcd_1.print("Distance in M");
    lcd_1.setCursor(0, 1);
  }
}
