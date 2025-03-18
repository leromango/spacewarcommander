#include <Servo.h>

// Analog sensor inputs
int knobPin = A0;                     // Potentiometer: resource distribution
int flashlightSensorPin = A1;         // Light sensor 1: flashlight control
int reloadSensorPin = A2;             // Light sensor 2: reload mechanism

// Button inputs
int shootButtonPin = 3;             // Shoot
int moveButtonPin = 4;              // Move 
int changeDirectionButtonPin = 5;   // Change direction (for switch)

// Switch
int switchPin = 2;

Servo reloadServo;

void setup() {
  Serial.begin(9600);
  
  pinMode(knobPin, INPUT);
  pinMode(flashlightSensorPin, INPUT);
  pinMode(reloadSensorPin, INPUT);
  
  // INPUT_PULLUP works better for buttons as it automatically sets 0V to 0
  pinMode(shootButtonPin, INPUT_PULLUP);
  pinMode(moveButtonPin, INPUT_PULLUP);
  pinMode(changeDirectionButtonPin, INPUT_PULLUP);

  pinMode(switchPin, INPUT_PULLUP);
}

void loop() {
  int knobValue = analogRead(knobPin);
  int flashlightValue = analogRead(flashlightSensorPin);
  int reloadValue = analogRead(reloadSensorPin);
  
  int shootState = digitalRead(shootButtonPin);
  int moveState = digitalRead(moveButtonPin);
  int changeDirectionState = digitalRead(changeDirectionButtonPin);
  
  int switchState = digitalRead(switchPin);

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

