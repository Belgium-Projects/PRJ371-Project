#include <Adafruit_LiquidCrystal.h>

char mystr[10] = "0";
Adafruit_LiquidCrystal lcd_1(0);

void setup() {
  Serial.begin(115400); 
  lcd_1.begin(16, 2);
  lcd_1.print("Getting ready");
  delay(1000);
  lcd_1.clear();
}

void loop() {
  lcd_1.setCursor(0, 0);
  lcd_1.print("Speed in m/s");
  lcd_1.setCursor(0, 1);
  lcd_1.print(mystr);
  lcd_1.print("m/s");
  if (Serial.available() >= 3) {
    Serial.readBytes(mystr, 3);
  }
}
