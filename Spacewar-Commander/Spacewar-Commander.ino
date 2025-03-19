#include <Servo.h>

// Analog sensor inputs
int knobPin = A0;                     // Potentiometer: resource distribution
int flashlightSensorPin = A1;         // Light sensor 1: flashlight control

// Button inputs
int shootButtonPin = 12;             // Shoot
int moveButtonPin = 4;              // Move 
int changeDirectionButtonPin = 5;   // Change direction (for switch)
int reloadPin = 6;
// Switch
int switchPin = 2;

int fwdLightPin = 9;
int backLightPin = 8;
int leftLightPin = 7;
int rightLightPin  = 10;

int servoControlPin = 11; //PWM
Servo reloadServo;

int buzzerPin = 3; //PWM

bool isReloading = false;
long long unsigned int reloadStartTime = 0;
long long unsigned int reloadDuration = 500;

void setup() {
  Serial.begin(9600);
  
  pinMode(knobPin, INPUT);
  pinMode(flashlightSensorPin, INPUT);
  
  // INPUT_PULLUP works better for buttons as it automatically sets 0V to 0
  pinMode(shootButtonPin, INPUT_PULLUP);
  pinMode(moveButtonPin, INPUT_PULLUP);
  pinMode(changeDirectionButtonPin, INPUT_PULLUP);
  pinMode(reloadPin, INPUT_PULLUP);

  pinMode(switchPin, INPUT_PULLUP);

  pinMode(buzzerPin, OUTPUT);

  reloadServo.attach(servoControlPin);
  reloadServo.write(50);  

}

void loop() {
  int knobValue = analogRead(knobPin);
  int flashlightValue = analogRead(flashlightSensorPin);
  
  int shootState = digitalRead(shootButtonPin);
  int moveState = digitalRead(moveButtonPin);
  int changeDirectionState = digitalRead(changeDirectionButtonPin);
  int reloadValue = digitalRead(reloadPin);

  int switchState = digitalRead(switchPin);

  if (reloadValue == 1 && !isReloading) {
    isReloading = true;
    reloadStartTime = millis();
    reloadServo.write(-100);
  }

  if (isReloading && (millis() - reloadStartTime) >= reloadDuration) {
    reloadServo.write(50);
    isReloading = false;
  }

  digitalWrite(fwdLightPin, HIGH);
  delay(100);
  digitalWrite(rightLightPin, HIGH);
  delay(100);
  digitalWrite(backLightPin, HIGH);
  delay(100);
  digitalWrite(leftLightPin, HIGH);
  delay(100);

  digitalWrite(fwdLightPin, LOW);
  delay(100);
  digitalWrite(rightLightPin, LOW);
  delay(100);
  digitalWrite(backLightPin, LOW);
  delay(100);
  digitalWrite(leftLightPin, LOW);
  tone(buzzerPin, 1000); // Send 1KHz sound signal...
  delay(1000);

  noTone(buzzerPin);

  Serial.print(knobValue);
  Serial.print(",");
  Serial.print(shootState);
  Serial.print(",");
  Serial.print(moveState);
  Serial.print(",");
  Serial.print(switchState);
  Serial.print(",");
  Serial.print(changeDirectionState);
  Serial.print(",");
  Serial.print(flashlightValue);
  Serial.print(",");
  Serial.println(reloadValue);
  
  
  delay(100);
}

